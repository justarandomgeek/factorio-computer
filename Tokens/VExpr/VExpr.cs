
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace compiler
{

	
	public interface VExpr
	{
		bool IsConstant();

		RegVRef AsReg();
		List<Instruction> FetchToReg(RegVRef dest);
	}
	
		
    

}