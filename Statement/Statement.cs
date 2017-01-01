
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace compiler
{
	public interface Statement
	{
		void Print(string prefix);
		
		List<Instruction> CodeGen();
	}
	
}