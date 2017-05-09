
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace nql
{
	
	public interface SExpr
	{
		bool IsConstant();

		int Evaluate();

		PointerIndex frame();
		FieldSRef AsDirectField();
		List<Instruction> FetchToField(FieldSRef dest);

	}
}