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
		Equal,
		Greater,
		Less,
		NotEqual = Greater | Less,
		GreaterEqual = Greater | Equal,
		LessEqual = Equal | Less,
		
	}

}