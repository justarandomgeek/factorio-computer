using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class VAssign:Statement
	{
		public VRef target;
		[Obsolete]
		public bool append;
		public VExpr source;
		
		public override string ToString()
		{
			return string.Format("[VAssign {0} {1} {2}]", target, "=", source);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		
		public List<Instruction> CodeGen()
		{
			var code = new List<Instruction>();
			var scratch = target as RegVRef;
			if(scratch != null)
			{
				if(target is VarVRef)
				{
					scratch = (RegVRef)target;
				}
				else
				{
					scratch = RegVRef.rScratch2;
				}

			}	
			
			code.AddRange(source.FetchToReg(scratch));
			if(target != scratch) code.AddRange(target.PutFromReg(scratch));
			return code;
		}
	}

}