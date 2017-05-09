
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace nql
{

	public interface VRef: VExpr // the assignable subset of vector expressions
	{
		//string datatype { get; }
		List<Instruction> PutFromReg(RegVRef src);
		bool IsLoaded { get; }
	}
	
}