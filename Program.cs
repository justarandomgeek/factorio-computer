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
				var parser = new Parser();
	            parser.ReadMachineDesc();
	            
	            foreach (var file in Options.Current.sourcefiles) {
	            	using(var reader = new StreamReader(file))
					{
						parser.Parse(reader.ReadToEnd());
						Console.WriteLine("Program Name: {0}", parser.Name);
			            parser.PrintAddrMap();
			            parser.MakeROM();
					}
		
	            }
	            
			}

			
		}
		
	}

	partial class Parser
    {
        new Scanner Scanner { get { return (Scanner)base.Scanner; } set { base.Scanner = value; } }

        public Dictionary<int,DataList> programData = new Dictionary<int, DataList>();
        
        Lua lua = new Lua();
        
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
			foreach (LuaTable sig in signalmap.Values) {
        		var name = sig["name"] as string;
        		var id = (int)(double)sig["id"];
        		var type = sig["type"] as string;
        		SignalSpec.signalMap.Add(name,id);
        		SignalSpec.typeMap.Add(name,type);
        	}
        }

        public void PrintAddrMap(){
        	Console.WriteLine("Labels:");
        	foreach (var label in AddrSpec.map) {
        		Console.WriteLine("{0}: {1}@",label.Key,label.Value);
        	}
        	Console.WriteLine("");

        }

      
        public SignalSpec mapChar(string s)
        {
        	if(string.IsNullOrEmpty(s)) return mapChar(' ');
        	return mapChar(s[0]);
        }
        public SignalSpec mapChar(char c)
        {
        	SignalSpec s = new SignalSpec(c);
        	if(string.IsNullOrEmpty(s.signal))
        	{
        		s.signal="signal-blue";
        	}
        	return s;
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
			lua["map"]=AddrSpec.map;
			lua["parser"]=this;
			
			var compileROM = lua.LoadFile("compileROM.lua");
			compileROM.Call();
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
            this.Scanner = s;
            this.Parse();
        }

        public string Name {get; private set;}
		int nextAddr=1;
		List<string> nextLabels = new List<string>();


		void Add(string s)
		{
			Add(s,new DataList());
		}

		void Add(string s, DataList dl)
		{
			foreach (char c in s) {
				DataList cdat = new DataList(c,1);
				foreach (var element in dl) {
				  cdat.Add(element.Key,element.Value);
				}
				Add(cdat);
			}
		}



		void Add(DataList d)
		{
			programData.Add(nextAddr,d);
			AddExtern();
		}

		void AddExtern()
		{
			foreach (var label in nextLabels) {
				AddrSpec.map.Add(label,nextAddr);
			}
			nextLabels.Clear();
			nextAddr++;
		}

		void AddLabel(string s)
		{
			nextLabels.Add(s);
		}

		void SetNext(int i)
		{
			nextAddr = i;
		}

    }

    partial class Scanner
    {
        public override void yyerror(string format, params object[] args)
        {
            //base.yyerror(format, args);
            Console.Error.WriteLine("{0} At line:{1} char:{2}",format, yyline, yycol);
        }
    }
}
