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


//TODO: use conditional ALU ops



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
				foreach (var file in Options.Current.sourcefiles)
				{
					CurrentProgram = new Parser();
					CurrentProgram.Name = file.Split('\\', '/').Last().ToUpper();
					CurrentProgram.ReadMachineDesc();


					CurrentProgram.ParseFile(file);

					//TODO: command line options for various print styles/segments

					Console.WriteLine();
					Console.WriteLine("Functions:");
					foreach (var func in CurrentProgram.Functions.Values)
					{
						CurrentFunction = func;
						Console.WriteLine("{0}:{1}", func.name, func.returntype);
						func.AllocateLocals();

						foreach (var field in func.localints)
						{
							Console.WriteLine("  {0}:{1}", field.Key, field.Value);
						}
						foreach (var symbol in func.locals)
						{
							Console.WriteLine("  " + symbol);
						}
						func.body.Print("| ");
						var build = func.BuildFunction();
						build.Print("  ");

						if (func.localints.Count > 0) CurrentProgram.Types.Add("__li" + func.name, func.localints);
						//Console.WriteLine();

						CurrentProgram.Symbols.Add(new Symbol
						{
							name = func.name,
							type = SymbolType.Function,
							datatype = "opcode",
							data = build.Select(stat => stat.Opcode()).ToList()
						});

						Console.WriteLine();
					}
					CurrentFunction = null;

					CurrentProgram.AllocateTypes();

					foreach (var typedata in CurrentProgram.Types)
					{
						Console.WriteLine("Type: {0}", typedata.Key);
						if (typedata.Value.hasString) Console.WriteLine("  _string_");
						foreach (var field in typedata.Value)
						{
							Console.WriteLine("  {0}:{1}", field.Key, field.Value);
						}
					}


					CurrentProgram.AllocateSymbols();

					Console.WriteLine();
					Console.WriteLine("Symbols:");
					foreach (var symbol in CurrentProgram.Symbols)
					{
						Console.WriteLine(symbol);
					}


					Console.WriteLine("Rom Data:");
					foreach (var element in CurrentProgram.romdata)
					{
						Console.WriteLine(element.Evaluate());
					}

					CurrentProgram.MakeROM();

				}
					
				Console.ReadLine();
					
	            
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


		public void ParseFile(string file)
		{
			string prog = "";
			using (var reader = new StreamReader(file))
			{
				prog = reader.ReadToEnd();
			}
			Parse(prog);
		}

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
        
        
        public void BeginFunction(string name, string returntype, SymbolList paramlist)
        {
        	Functions.Add(name,new FunctionInfo{name=name, returntype = returntype, locals=paramlist});
        	
        	InFunction = name;
        }
        
        public void CompleteFunction(string name, Block body)
        {
        	Functions[name].body=body;
        	InFunction=null;
        }
        
		public void AllocateTypes()
		{
			foreach (var typename in Types.Keys)
			{
				Types[typename].Allocate();
			}
		}


        public void AllocateSymbols()
        {
        	// 0 in both frames is reserved. Const space has progsym, data space reserved for future use by memory allocator or other system services.
			int constaddr=1;
			romdata = new List<Table>();

			// leave space for symtable starting at 1
			int symtsize = Symbols.Count(sym =>
				sym.name.All(c=>"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(c))
			);
			constaddr += symtsize;


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

			var progsym = new Table(Name);
			progsym.Add("symtsize", (IntSExpr)symtsize);
			progsym.Add("romsize",(IntSExpr)(romdata.Count+1));
			progsym.Add("datasize", (IntSExpr)dataaddr);
			progsym.Add("mainloc", new AddrSExpr{frame=PointerIndex.ProgConst,symbol="MAIN"});
			
			progsym.datatype = "progsym";
										
			romdata.Insert(0,progsym);

			romdata.InsertRange(
				1,
				Symbols.Where(
					sym => sym.name.All(c => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(c))
					).Select(
					sym =>
					{
						var symtable = new Table(sym.name);
						symtable.datatype = "symbol";

						//DATA = 1 FUNC = 2 CONST = 3
						switch (sym.type)
						{
							case SymbolType.Data:
								symtable.Add("symtype", (IntSExpr)1);
								break;
							case SymbolType.Function:
								symtable.Add("symtype", (IntSExpr)2);
								break;
							case SymbolType.Constant:
								symtable.Add("symtype", (IntSExpr)3);
								break;
							default:
								throw new NotImplementedException();
						}
						
						symtable.Add("addr", (IntSExpr)sym.fixedAddr);
						symtable.Add("size", (IntSExpr)sym.size);
						
						return symtable;
					})
				);



		}

		public Dictionary<string, Func<FunctionCall, Block>> VBuiltins = new Dictionary<string, Func<FunctionCall, Block>>{
			{ "playerinfo", fcall=> { throw new NotImplementedException(); } },
			{ "memexchange", fcall => {
				// prevdata = memexchange(newdata,addr,frame)
				Block b = new Block();
				b.Add(new Exchange
				{
					addr = fcall.args.ints[0],
					source = fcall.args.var as RegVRef ?? RegVRef.rScratch2,
					dest = fcall.vreturn as RegVRef ?? RegVRef.rScratch2,
					frame = (PointerIndex)fcall.args.ints[1].Evaluate(),

				});
				return b;
			}},

			};

		public Dictionary<string, Func<FunctionCall, Block>> SBuiltins = new Dictionary<string, Func<FunctionCall, Block>>{
			//{ "sum", fcall=> {
			//	Block b = new Block();
			//	b.Add(new SAssign {target = fcall.sreturn, source = FieldSRef.VarField((VRef)fcall.args.var,"signal-each") });
			//	return b;

			//}},

			};

		public Tokens GetIdentType(string ident)
        {
			if (ExpectFieldType != null)
        	{
        		if ( ExpectFieldType != "var" && Types[ExpectFieldType].ContainsKey(ident) )
        		{
        			return Tokens.FIELD;	
        		}

				ExpectFieldType = null;
			}

			if (ident == "nil")
			{
				return Tokens.FIELD;
			}
			else if (SBuiltins.ContainsKey(ident))
			{
				return Tokens.SFUNCNAME;
			}
			else if (VBuiltins.ContainsKey(ident))
			{
				return Tokens.VFUNCNAME;
			}
			else if (Types.ContainsKey(ident))
        	{
        		return Tokens.TYPENAME;
        	}
        	else if (NativeFields.Contains(ident))
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
				return Functions[ident].returntype == "int" ? Tokens.SFUNCNAME : Tokens.VFUNCNAME;
        	}
        	else
			{
				return Tokens.UNDEF;
			}
        }
          
        
        public void ReadMachineDesc()
        {
			string maptext;

			var options = Options.Current;

			if(options.map != null)
			{
				using (var reader = new StreamReader(options.map))
				{
					maptext = reader.ReadToEnd();
				}
			}
			else
			{
				maptext = Program.GetResourceText("scalarmap");
			}
			
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
        		var rcon = new Rcon();
        		var rconhost = new System.Net.IPEndPoint(
	        		System.Net.Dns.GetHostEntry(options.rconhost).AddressList[0],
	        		options.rconport);
        		const string rconcommand = "/c remote.call('foreman','addBlueprint',game.players['{0}'],{1},{2})";

				Console.WriteLine("Sending {0} to Foreman by RCON...", printName);
				rcon.ConnectBlocking(rconhost,options.rconpass); 
	        	rcon.ServerCommandBlocking(
	        		string.Format(rconcommand,options.rconplayer,Compress(printData),printName));
	        	rcon.Disconnect();	
        	} else {
        		Console.WriteLine(printName);
				//Console.WriteLine(printData);
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

			var options = Options.Current;
			string romgen;
			if (options.romscript != null)
			{
				using (var reader = new StreamReader(options.romscript))
				{
					romgen = reader.ReadToEnd();
				}
			}
			else
			{
				romgen = Program.GetResourceText("CompileROM");
			}
			
			var compileROM = lua.LoadString(romgen, "CompileROM");
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


		public void Require(string file)
		{
			//TODO: be smarter about finding the file
			string prog = "";
			using (var reader = new StreamReader(file))
			{
				prog = reader.ReadToEnd();
			}

			Scanner.IncludeSource(prog);
			
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

		Stack<BufferContext> fileStack = new Stack<BufferContext>();
		public void IncludeSource(string text)
		{
			fileStack.Push(MkBuffCtx());
			SetSource(text, 0);
		}

		protected override bool yywrap()
		{
			if(fileStack.Count > 0 )
			{
				RestoreBuffCtx(fileStack.Pop());
				return false;
			} else {
				return true;
			}
			
		}

	}
}
