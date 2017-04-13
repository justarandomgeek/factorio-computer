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
	
	public class Program
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


					foreach (var ext in CurrentProgram.Externs)
					{
						Console.WriteLine(ext.progname);
						foreach (var func in ext.Functions)
						{
							Console.WriteLine("  {0}:{1}", func.name, func.returntype);
						}

						foreach (var symbol in ext.Symbols)
						{
							Console.WriteLine("  {0}", symbol);
						}
					}


					if (Options.Current.func) Console.WriteLine("Functions:");
					foreach (var func in CurrentProgram.Functions.Values)
					{
						CurrentFunction = func;
						if (Options.Current.func) Console.WriteLine("{0}:{1}", func.name, func.returntype);
						func.AllocateLocals();

						foreach (var symbol in func.locals)
						{
							if (Options.Current.func) Console.WriteLine("  " + symbol);
						}
						if (Options.Current.funcbody) func.body.Print("| ");
						var build = func.BuildFunction();
						if (Options.Current.funcbodyraw)
						{
							foreach (var item in build)
							{
								Console.WriteLine("  {0}", item);
							}
						}

						CurrentProgram.Symbols.Add(new Symbol
						{
							name = func.name,
							type = SymbolType.Function,
							frame = PointerIndex.ProgConst,
							datatype = "opcode",
							data = build.ConvertAll(inst => (Table)inst)
						});

						if (Options.Current.func) Console.WriteLine();
					}
					CurrentFunction = null;

					CurrentProgram.AllocateTypes();

					if (Options.Current.typeinfo)
					{
						foreach (var typedata in CurrentProgram.Types)
						{
							Console.WriteLine("Type: {0}", typedata.Key);
							if (typedata.Value.hasString) Console.WriteLine("  _string_");
							foreach (var field in typedata.Value)
							{
								Console.WriteLine("  {0}:{1}", field.Key, field.Value);
							}
						}
					}

					CurrentProgram.AllocateSymbols();

					if (Options.Current.symtable)
					{
						Console.WriteLine();
						Console.WriteLine("Symbols:");
						foreach (var symbol in CurrentProgram.Symbols)
						{
							Console.WriteLine(symbol);
						}
					}

					if (Options.Current.dumprom)
					{
						Console.WriteLine("Rom Data:");
						foreach (var element in CurrentProgram.romdata)
						{
							Console.WriteLine(element.Evaluate());
						}
					}

					CurrentProgram.MakeROM();
				}

				Console.ReadLine();   
			}
		}
	}
	
	
	


}
