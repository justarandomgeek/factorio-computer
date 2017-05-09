/*
 * Created by SharpDevelop.
 * User: Thomas
 * Date: 2016-08-17
 * Time: 14:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using CommandLine;

namespace nql
{
	class Options {
		public static Options Current = new Options();
		
		[Option(HelpText="Use external CompileROM.lua")]
		public string romscript { get; set; }
		
		[Option(HelpText="Use custom mapfile, scalarmap.lua")]
		public string map { get; set; }
		
		[Option(DefaultValue="localhost", HelpText="rcon hostname to direct-insert blueprint")]
		public string rconhost { get; set; }
		
		[Option(DefaultValue=12345, HelpText="rcon port to direct-insert blueprint")]
		public int rconport { get; set; }
		
		[Option(HelpText="rcon password to direct-insert blueprint")]
		public string rconpass { get; set; }
		
		[Option(HelpText="rcon player name to direct-insert blueprint")]
		public string rconplayer { get; set; }

		[Option("symtable", HelpText = "print symbol table")]
		public bool symtable { get; set; }

		[Option("typeinfo", HelpText = "print type info")]
		public bool typeinfo { get; set; }

		bool _func;
		[Option(HelpText = "print functions")]
		public bool func { get { return _func || funcbody || funcbodyraw; } set { _func = value; } }

		[Option("funcbody", HelpText = "print parsed function bodies")]
		public bool funcbody { get; set; }

		[Option(HelpText = "print compiled function bodies")]
		public bool funcbodyraw { get; set; }

		[Option(HelpText = "print raw compiled ROM data")]
		public bool dumprom { get; set; }

		[ValueList(typeof(List<string>))]
		public List<string> sourcefiles {get;set;}
	}
}
