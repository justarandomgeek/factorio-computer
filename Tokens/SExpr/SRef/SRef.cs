
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace nql
{

	public interface SRef : SExpr // the assignable subset of scalar expressions
	{
		List<Instruction> PutFromField(FieldSRef src);
		List<Instruction> PutFromInt(int value);
	}
	
}