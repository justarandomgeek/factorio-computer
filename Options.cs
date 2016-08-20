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

namespace compiler
{
	class Options {
		public static Options Current = new Options();
		
		[Option("romgen", HelpText="Use external CompileROM.lua")]
		public string romscript { get; set; }
		
		[Option('m', "map", HelpText="Use custom scalarmap.lua")]
		public string map { get; set; }
		
		[Option("rconhost", DefaultValue="localhost", HelpText="rcon hostname to direct-insert blueprint")]
		public string rconhost { get; set; }
		
		[Option("rconport", DefaultValue=12345, HelpText="rcon port to direct-insert blueprint")]
		public int rconport { get; set; }
		
		[Option("rconpass", HelpText="rcon password to direct-insert blueprint")]
		public string rconpass { get; set; }
		
		[Option("rconplayer", HelpText="rcon player name to direct-insert blueprint")]
		public string rconplayer { get; set; }
		
		[ValueList(typeof(List<string>))]
		public List<string> sourcefiles {get;set;}
	}
}
