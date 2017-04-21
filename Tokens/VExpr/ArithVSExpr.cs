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
		
		public readonly VExpr V1;
		public readonly ArithSpec Op;
		public readonly SExpr S2;

		public ArithVSExpr(VExpr V1, ArithSpec Op, SExpr S2)
		{
			this.V1 = V1;
			this.Op = Op;
			this.S2 = S2;
		}

		public string datatype { get { return V1.datatype; } }

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

			var code = new List<Instruction>();
			RegVRef R1 = V1.AsReg();
			FieldSRef F2 = S2.AsDirectField();

			if (R1 == null)
			{
				R1 = RegVRef.rFetch(1);
				code.AddRange(V1.FetchToReg(R1));
			}

			if (F2 == null)
			{
				if (R1.Equals(RegVRef.rFetch(1))) { code.AddRange(new Push(R1).CodeGen()); }
				F2 = FieldSRef.ScratchInt();
				code.AddRange(S2.FetchToField(F2));
				if (R1.Equals(RegVRef.rFetch(1))) { code.AddRange(new Pop(R1).CodeGen()); }
			}

			switch (Op)
			{
				case ArithSpec.Add:
					code.Add(new Instruction { opcode = Opcode.EachAddV, acc = false, op1 = R1, op2 = F2, dest = dest });
					break;
				case ArithSpec.Subtract:
					code.Add(new Instruction { opcode = Opcode.EachSubV, acc = false, op1 = R1, op2 = F2, dest = dest });
					break;
				case ArithSpec.Multiply:
					code.Add(new Instruction { opcode = Opcode.EachMulV, acc = false, op1 = R1, op2 = F2, dest = dest });
					break;
				case ArithSpec.Divide:
					code.Add(new Instruction { opcode = Opcode.EachDivV, acc = false, op1 = R1, op2 = F2, dest = dest });
					break;
				default:
					throw new NotImplementedException();
			}

			return code;
		}

		public RegVRef AsReg()
		{
			return null;
		}
	}

}