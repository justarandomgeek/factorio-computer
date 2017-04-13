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
			return ToString().GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as ArithVExpr;
			if (other != null)
			{
				return other.V1.Equals(this.V1) && other.Op == this.Op && other.V2.Equals(this.V2);
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
			RegVRef R2 = V2.AsReg();
			
			if (R1 == null)
			{
				R1 = RegVRef.rFetch(1);
				code.AddRange(V1.FetchToReg(R1));
			}

			if (R2 == null)
			{
				R2 = RegVRef.rFetch(2);
				code.AddRange(V2.FetchToReg(R2));
			}
									
			switch (Op)
			{
				case ArithSpec.Add:
					code.Add(new Instruction { opcode = Opcode.EachAddV, acc = false, op1 = R1, dest = dest });
					code.Add(new Instruction { opcode = Opcode.EachAddV, acc = true, op1 = R2, dest = dest });
					break;
				case ArithSpec.Subtract:
					code.Add(new Instruction { opcode = Opcode.EachAddV, acc = false, op1 = R1, dest = dest });
					code.Add(new Instruction { opcode = Opcode.EachMulV, acc = true, op1 = R2, op2 = FieldSRef.Imm2(), imm2 = new IntSExpr(-1), dest = dest });
					break;
				case ArithSpec.Multiply:
					code.Add(new Instruction { opcode = Opcode.VMul, op1 = R1, op2 = R2, dest = dest });
					break;
				case ArithSpec.Divide:
					code.Add(new Instruction { opcode = Opcode.VDiv, op1 = R1, op2 = R2, dest = dest });
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