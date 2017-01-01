using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class SymbolList:List<Symbol>
	{

		int paramcount=1;
		public void AddParam(FieldInfo pi)
		{
			this.Add(new Symbol{
			         	name=pi.name,
			         	type=SymbolType.Parameter,
			         	datatype=pi.basename,
			         	fixedAddr=paramcount++,
			         });
		}
	
		
	}

}