using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public enum SymbolType{
		Function=1, 
		Data,
		Constant,
		Register,
		Program,
		Internal,
		Parameter,
	}

}