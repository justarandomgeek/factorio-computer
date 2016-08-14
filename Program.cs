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
