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
using NLua;


namespace compiler
{
	class Program
	{
		public static void Main(string[] args)
		{

			string sourcetext = new StreamReader(args[0]).ReadToEnd();

            var parser = new Parser();
            parser.ReadMachineDesc("scalarmap.lua");
            parser.Parse(sourcetext);

            Console.WriteLine("Program Name: {0}", parser.Name);
            parser.PrintAddrMap();
            //parser.PrintDataMap();
            parser.MakeROM();
		}
	}

	partial class Parser
    {
        new Scanner Scanner { get { return (Scanner)base.Scanner; } set { base.Scanner = value; } }

        readonly Dictionary<char,SignalSpec> charMap = new Dictionary<char,SignalSpec>{
        	{'1',"signal-1"},{'2',"signal-2"},{'3',"signal-3"},{'4',"signal-4"},{'5',"signal-5"},
        	{'6',"signal-6"},{'7',"signal-7"},{'8',"signal-8"},{'9',"signal-9"},{'0',"signal-0"},
        	{'A',"signal-A"},{'B',"signal-B"},{'C',"signal-C"},{'D',"signal-D"},{'E',"signal-E"},
			{'F',"signal-F"},{'G',"signal-G"},{'H',"signal-H"},{'I',"signal-I"},{'J',"signal-J"},
			{'K',"signal-K"},{'L',"signal-L"},{'M',"signal-M"},{'N',"signal-N"},{'O',"signal-O"},
			{'P',"signal-P"},{'Q',"signal-Q"},{'R',"signal-R"},{'S',"signal-S"},{'T',"signal-T"},
			{'U',"signal-U"},{'V',"signal-V"},{'W',"signal-W"},{'X',"signal-X"},{'Y',"signal-Y"},
			{'Z',"signal-Z"},{'-',"fast-splitter"},{'.',"train-stop"}};
        Dictionary<int,DataList> programData = new Dictionary<int, DataList>();
        
        Lua lua = new Lua();
        
        public void ReadMachineDesc(string filename)
        {
        	var signalmap = (LuaTable)lua.DoFile(filename)[0];
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

        public void PrintDataMap(){
        	Console.WriteLine("Program Data:");
        	foreach (var addr in programData) {
        		Console.WriteLine("{0}@ {1}",addr.Key,addr.Value);
        	}
        	Console.WriteLine("");

        }

        
        public SignalSpec mapChar(string s){
        	char c = s!=""?s[0]:' ';
        	return mapChar(c);
        }
        public SignalSpec mapChar(char c)
        {
        	return charMap.ContainsKey(c)?charMap[c]:new SignalSpec("signal-blue");
        }
        
        public void MakeROM()
        {
			Console.WriteLine("ROM:");
			lua["name"]=Name;
			lua["map"]=AddrSpec.map;
			lua["programdata"]=programData;
			lua["parser"]=this;
			
			
			
			
			var compileROM = lua.LoadFile("compileROM.lua");
			var luaprint = (string)compileROM.Call()[0];
			
			
			var ms = new MemoryStream();
        	var sw = new GZipStream(ms, CompressionMode.Compress);
			
        	var byteprint = Encoding.UTF8.GetBytes(luaprint);
        	sw.Write(byteprint, 0, byteprint.Length);
        	sw.Close();
        	
        	Console.WriteLine(Convert.ToBase64String(ms.ToArray()));			
        }


        public Parser():base(null)
        {
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
				DataList cdat;
				if(charMap.ContainsKey(c))
				{
					cdat = new DataList(c,charMap);
					foreach (var element in dl) {
						cdat.Add(element.Key,element.Value);
					}
				} else {
					cdat = dl;
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
