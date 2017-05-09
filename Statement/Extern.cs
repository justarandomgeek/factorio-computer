using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nql
{
	public class Extern
	{
		public readonly string progname;
		public readonly Dictionary<string, TypeInfo> Types = new Dictionary<string, TypeInfo>();
		public readonly List<FunctionInfo> Functions = new List<FunctionInfo>();
		public readonly SymbolList Symbols = new SymbolList();

		public Extern(string name)
		{
			progname = name;
		}

		public IEnumerable<Table> ROMData()
		{
			var t = new Table(progname);
			t.Add("signal-grey", Functions.Count + Symbols.Count);
			yield return t;

			foreach (var f in Functions)
			{
				yield return new Table(f.name);
			}

			foreach (var s in Symbols)
			{
				yield return new Table(s.name);
			}
		}
	}
}
