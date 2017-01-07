using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class ArithVExpr: VExpr
	{
		public bool IsConstant()
		{
			return V1.IsConstant() && V2.IsConstant();
		}
		
		public readonly VExpr V1;
		public readonly ArithSpec Op;
		public readonly VExpr V2;	

		public ArithVExpr(VExpr V1, ArithSpec Op, VExpr V2)
		{
			this.V1 = V1;
			this.Op = Op;
			this.V2 = V2;
		}

		public override string ToString()
		{
			return string.Format("[ArithVExpr {0} {1} {2}]", V1, Op, V2);
		}
		
		public static bool operator ==(ArithVExpr a1, ArithVExpr a2) { return a1.Equals(a2); }
		public static bool operator !=(ArithVExpr a1, ArithVExpr a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return V1.GetHashCode() ^ Op.GetHashCode() ^ V2.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as ArithVExpr;
			if (other != null)
			{
				return other.V1 == this.V1 && other.Op == this.Op && other.V2 == this.V2;
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

		public RegVRef AsReg()
		{
			throw new NotImplementedException();
		}
	}

}