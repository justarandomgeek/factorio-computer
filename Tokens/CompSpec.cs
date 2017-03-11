using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	[Flags]
	public enum CompSpec
	{
		Undefined=0,
		Equal=1,
		Greater=2,
		Less=4,
		NotEqual = Greater | Less,
		GreaterEqual = Greater | Equal,
		LessEqual = Equal | Less,
		
	}

}