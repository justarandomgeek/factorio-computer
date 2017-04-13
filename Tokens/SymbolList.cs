using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class SymbolList:List<Symbol>
	{

		int intparamcount = 1;
		int varparamcount = 1;
		public void AddParam(FieldInfo pi)
		{
			if (this.Exists(sym => sym.name == pi.name)) { throw new ArgumentException(); }

			this.Add(new Symbol
			{
				name = pi.name,
				type = SymbolType.Parameter,
				datatype = pi.basename,
				fixedAddr = pi.basename == "int" ? intparamcount++ : varparamcount++,
			});
		}
	
		
		public Symbol this[string name]
		{
			get { return this.Find(sym => sym.name == name);}
		}

		
		
	}

}