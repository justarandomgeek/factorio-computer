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

			var srcreg = source.AsReg();
			var tgtreg = target.AsReg();

			if (srcreg == null || (srcreg != null && tgtreg != null)) code.AddRange(source.FetchToReg(tgtreg ?? RegVRef.rScratchTab));
			if (tgtreg == null)	code.AddRange(target.PutFromReg(srcreg ?? RegVRef.rScratchTab));

			return code;
		}
	}

}