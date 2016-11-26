/*
 * Created by SharpDevelop.
 * User: Thomas
 * Date: 2016-07-29
 * Time: 22:46
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using NLua;
using SourceRconLib;
using CommandLine;


namespace compiler
{
	class Program
	{
		//TODO: get rid of these somehow?
		public static FunctionInfo CurrentFunction;
		public static Parser CurrentProgram;
		
		public static string GetResourceText(string name)
		{
			using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(name)))
			{
				return reader.ReadToEnd();
			}
		}
		
		public static void Main(string[] args)
		{
			if (CommandLine.Parser.Default.ParseArguments(args, Options.Current)) {
				CurrentProgram = new Parser();
				CurrentProgram.Name="DEMO";
	            CurrentProgram.ReadMachineDesc();
	            
	            foreach (var file in Options.Current.sourcefiles) {
	            	string prog = "";
	            	using(var reader = new StreamReader(file))
					{
	            		prog = reader.ReadToEnd();
	            	}
					CurrentProgram.Parse(prog);
					
					
					
					Console.WriteLine();
					Console.WriteLine("Functions:");
					foreach (var func in CurrentProgram.Functions.Values) {
						CurrentFunction = func;
						Console.WriteLine(func.name);
						func.AllocateLocals();
						
						foreach (var field in func.localints) {
							Console.WriteLine("  {0}:{1}",field.Key,field.Value);
						}
						foreach (var symbol in func.locals) {
							Console.WriteLine("  "+symbol);
						}
						//func.body.Print("| ");
						var build = func.BuildFunction();
						build.Print("  ");
						
						if(func.localints.Count > 0) CurrentProgram.Types.Add("__li"+func.name,func.localints);
						//Console.WriteLine();
						
						CurrentProgram.Symbols.Add(new Symbol{
						                           	name=func.name,
						                           	type=SymbolType.Function,
						                           	datatype="opcode",
						                           	data=build.Select(stat => stat.Opcode()).ToList()
						                           });
						
						Console.WriteLine();
					}
					CurrentFunction = null;
					
					foreach (var typedata in CurrentProgram.Types) {
						Console.WriteLine("Type: {0}",typedata.Key);
						if(typedata.Value.hasString) Console.WriteLine("  _string_");
						foreach (var field in typedata.Value) {
							Console.WriteLine("  {0}:{1}",field.Key,field.Value);
						}						
					}
					
					
					CurrentProgram.AllocateSymbols();
					
					Console.WriteLine();
					Console.WriteLine("Symbols:");
					foreach (var symbol in CurrentProgram.Symbols) {
						Console.WriteLine(symbol);
					}
					
					
					
					
					//CurrentProgram.romdata = CurrentProgram.romdata.Select(t=>t.Evaluate()).ToList();
					
					Console.WriteLine("Rom Data:");
					
					foreach (var element in CurrentProgram.romdata) {
						Console.WriteLine(element.Evaluate());
					}
					
					CurrentProgram.MakeROM();
					
					Console.ReadLine();
					
	            }
			}
		}
	}
	
	
	

	partial class Parser
    {
        new Scanner Scanner { get { return (Scanner)base.Scanner; } set { base.Scanner = value; } }

        public List<string> NativeFields = new List<string>();
        public Dictionary<string,TypeInfo> Types = new Dictionary<string, TypeInfo>();
        public Dictionary<string,FunctionInfo> Functions = new Dictionary<string, FunctionInfo>();
        public SymbolList Symbols = new SymbolList();
        
        public string ExpectFieldType { get; private set; }
        public string InFunction { get; private set; }
        
        public List<Table> romdata;
        
        public string Name {get; set;}
        
        Lua lua = new Lua();
        
        public void RegisterType(string typename, TypeInfo typeinfo)
        {
        	Types.Add(typename,typeinfo);
        }
        
        public void CreateSym(Symbol sym)
        {
        	if (InFunction != null) {
        		Functions[InFunction].locals.Add(sym);
        	} else {
        		Symbols.Add(sym);
        	}
        }
        
        public void CreateInt(string name)
        {
        	if (InFunction != null) {
        		Functions[InFunction].localints.Add(name,null);
        		
        	} else {
        		if(!Types.ContainsKey("__globalints")){Types.Add("__globalints",new TypeInfo());}
        		Types["__globalints"].Add(name,null);
        	}
        }
        
        
        public string GetSymbolDataType(string ident)
        {
        	if (InFunction != null && Functions[InFunction].locals.Exists((s)=>s.name==ident)) {
        		return Functions[InFunction].locals.Find((s)=>s.name==ident).datatype;
        	} else {
        		return Symbols.Find((s)=>s.name==ident).datatype;
        	}
        }
        
        
        public void BeginFunction(string name, SymbolList paramlist)
        {
        	Functions.Add(name,new FunctionInfo{name=name,locals=paramlist});
        	
        	InFunction = name;
        }
        
        public void CompleteFunction(string name, Block body)
        {
        	Functions[name].body=body;
        	InFunction=null;
        }
        
        public void AllocateSymbols()
        {
        	// 0 in both frames is reserved. Const space has progsym, data space reserved for future use by memory allocator or other system services.
			int constaddr=1;
			romdata = new List<Table>();
			int dataaddr=1;
			var newsyms = new SymbolList();
			foreach (var symbol in Symbols.OrderByDescending(s=>s.type)) {
				switch (symbol.type) {
					case SymbolType.Constant:
					case SymbolType.Function:
						symbol.assign(constaddr);
						romdata.AddRange(symbol.data);
						constaddr+=symbol.size??1;
						break;
						
					case SymbolType.Data:
						if(!symbol.fixedAddr.HasValue)
						{
							symbol.assign(dataaddr);
							dataaddr+=symbol.size??1;
						}
						break;
				}
				newsyms.Add(symbol);
			}
			Symbols= newsyms;
			
			var progsym = new Table();
					progsym.Add("constsize",(IntSExpr)(romdata.Count+1));
					progsym.Add("datasize", (IntSExpr)dataaddr);
					progsym.Add("mainloc", new AddrSExpr{frame=2,symbol="MAIN"});
					
					progsym += new Table(Name);
					
					progsym.datatype = "progsym";
										
					romdata.Insert(0,progsym);
        }
        
        
        public Tokens GetIdentType(string ident)
        {
        	if (ExpectFieldType != null)
        	{
        		if (ident=="nil"||(ExpectFieldType =="var" && NativeFields.Contains(ident))||(Types[ExpectFieldType].ContainsKey(ident)))
        		{
        			ExpectFieldType = null;
        			return Tokens.FIELD;	
        		} 
        	}
        	
        	if(Types.ContainsKey(ident))
        	{
        		return Tokens.TYPENAME;
        	}
        	else if(NativeFields.Contains(ident))
			{
				return Tokens.FIELD;
			}
        	else if (InFunction != null && Functions[InFunction].locals.Exists((s) => s.name == ident))
        	{
        		return Functions[InFunction].locals.Find((s) => s.name == ident).ToToken();
        	}
        	else if (InFunction != null && Functions[InFunction].localints.ContainsKey(ident))
        	{
        		return Tokens.INTVAR;
        	}
        	else if(Symbols.Exists((s) => s.name == ident))
        	{
        		return Symbols.Find((s) => s.name == ident).ToToken();        		
        	}
        	else if (Types.ContainsKey("__globalints") && Types["__globalints"].ContainsKey(ident))
        	{
        		return Tokens.INTVAR;
        	}
        	else if (Functions.ContainsKey(ident))
        	{
        		return Tokens.FUNCNAME;
        	}
        	else
			{
				return Tokens.UNDEF;
			}
        }
        
        
        
        public void ReadMachineDesc()
        {
        	string maptext = Program.GetResourceText("scalarmap");
        	
        	//TODO: get filename from options.map
        	//using(reader = new StreamReader(mapfile))
        	//{
        	//maptext = reader.ReadToEnd();
        	//}
			
			//TODO: use serpent to load this?
			var signalmap = (LuaTable)lua.DoString(maptext,"scalarmap")[0];
			lua.NewTable("signaltypes");
			LuaTable sigtypes = (LuaTable)lua["signaltypes"];
			foreach (LuaTable sig in signalmap.Values) {
        		var name = sig["name"] as string;
        		var id = (int)(double)sig["id"];
        		var type = sig["type"] as string;
        		
        		sigtypes[name]=type;
        		this.NativeFields.Add(name);
        	}
        }

        public static string Compress(string s)
        {
        	using(var ms = new MemoryStream())
        	{
        		using(var sw = new GZipStream(ms, CompressionMode.Compress))
        		{
					var byteprint = Encoding.UTF8.GetBytes(s);
		        	sw.Write(byteprint, 0, byteprint.Length);
        		}
        		
        		return Convert.ToBase64String(ms.ToArray());			
        	}
        }
                
        public void returnBlueprint(string printName, string printData)
        {
        	var options = Options.Current;
        	if(options.rconplayer != null)
        	{
        		//TODO: rework this for updated Foreman API.
        		var rcon = new Rcon();
        		var rconhost = new System.Net.IPEndPoint(
	        		System.Net.Dns.GetHostEntry(options.rconhost).AddressList[0],
	        		options.rconport);
        		// this command is definitely wrong
				const string rconcommand = "/c remote.call('foreman','addBlueprintToTable',game.players['{0}'],{1},{2});remote.call('foreman','refreshPrintFrame',game.players['{0}']))";
	        	rcon.ConnectBlocking(rconhost,options.rconpass); 
	        	rcon.ServerCommandBlocking(
	        		string.Format(rconcommand,options.rconplayer,Compress(printData),printName));
	        	rcon.Disconnect();	
        	} else {
        		Console.WriteLine(printName);
        		Console.WriteLine(Compress(printData));
        		Console.WriteLine("");
        	}
        }
        
        public void MakeROM()
        {
        	
			Console.WriteLine("Compiling ROM:");
			lua["parser"]=this;
			
			lua.NewTable("romdata");
			
			LuaTable rd = (LuaTable)lua["romdata"];
			for (int i = 0; i < romdata.Count; i++) {
				var table = romdata[i].Evaluate();
				var lt = CreateLuaTable();
            	foreach (var element in table) {
            		lt[element.Key]=element.Value.Evaluate();
            	}			                                    	
				rd[i]= lt;
			}
			
			var compileROM = lua.LoadFile("compileROM.lua");
			var foo = compileROM.Call();
        }

        public LuaTable CreateLuaTable()
		{
		    return (LuaTable)lua.DoString("return {}")[0];
		}

        public Parser():base(null)
        {
        	var serpent = lua.DoString(Program.GetResourceText("serpent"),"serpent");
        	lua["serpent"]=serpent[0];
        	
        }
        
        public void Parse(string input)
        {
            var s = new Scanner();
            s.SetSource(input, 0);
            s.Parser=this;
            this.Scanner = s;
            this.Parse();
        }


    }

    partial class Scanner
    {
    	public Parser Parser;
        public override void yyerror(string format, params object[] args)
        {
            //base.yyerror(format, args);
            Console.Error.WriteLine("{0} At line:{1} char:{2}",format, yyline, yycol);
        }
    }
}
