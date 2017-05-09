
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace nql
{
	public interface Statement
	{
		void Print(string prefix);
		
		List<Instruction> CodeGen();
	}
	
}