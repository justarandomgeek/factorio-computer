using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
{

	public class Return:Statement
	{
		public readonly SExpr sreturn;
		public readonly VExpr vreturn;

		public Return(){}
		public Return(SExpr sret)
		{
			sreturn = sret;
		}
		public Return(VExpr vret)
		{
			vreturn = vret;
		}

		public override string ToString()
		{
			return string.Format("[Return {0}|{1}]", sreturn, vreturn);
		}	
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		
		public List<Instruction> CodeGen()
		{

			var b = new List<Instruction>();

			if (sreturn != null)
			{
				b.AddRange(sreturn.FetchToField(FieldSRef.SReturn));
			}

			if (vreturn != null)
			{
				b.AddRange(vreturn.FetchToReg(RegVRef.rVReturn()));
			}

			b.Add(new Jump
			{
				target = new AddrSExpr("__return"),
				relative = true,
			});

			return b;
		}
	}

}