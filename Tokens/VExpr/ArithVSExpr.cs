using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class ArithVSExpr: VExpr
	{
		public bool IsConstant()
		{
			return V1.IsConstant() && S2.IsConstant();
		}
		
		public Table Evaluate()
		{
			if(!IsConstant())
			{
				throw new InvalidOperationException();
			}
			switch (Op) {
				case ArithSpec.Add:
					return V1.Evaluate() + new IntSExpr(S2.Evaluate());
				case ArithSpec.Subtract:
					return V1.Evaluate() - new IntSExpr(S2.Evaluate());
				case ArithSpec.Multiply:
					return V1.Evaluate() * new IntSExpr(S2.Evaluate());
				case ArithSpec.Divide:
					return V1.Evaluate() / new IntSExpr(S2.Evaluate());
				default:
					throw new InvalidOperationException();
			} 
		}
		
		public readonly VExpr V1;
		public readonly ArithSpec Op;
		public readonly SExpr S2;

		public ArithVSExpr(VExpr V1, ArithSpec Op, SExpr S2)
		{
			this.V1 = V1;
			this.Op = Op;
			this.S2 = S2;
		}

		public override string ToString()
		{
			return string.Format("[ArithVSExpr {0} {1} {2}]", V1, Op, S2);
		}
		
		public static bool operator ==(ArithVSExpr a1, ArithVSExpr a2) { return a1.Equals(a2); }
		public static bool operator !=(ArithVSExpr a1, ArithVSExpr a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return V1.GetHashCode() ^ Op.GetHashCode() ^ S2.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as ArithVSExpr;
			if (other != null)
			{
				return other.V1 == this.V1 && other.Op == this.Op && other.S2 == this.S2;
			}
			else
			{
				return false;
			}

		}

		public List<Instruction> FetchToReg(RegVRef dest)
		{
			throw new NotImplementedException();
		}
	}

}