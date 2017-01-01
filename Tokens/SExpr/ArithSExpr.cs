using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class ArithSExpr: SExpr
	{
		public bool IsConstant()
		{
			return S1.IsConstant() && S2.IsConstant();
		}
		
		public int Evaluate()
		{
			if(!IsConstant())
			{
				throw new InvalidOperationException();
			}
			switch (Op) {
				case ArithSpec.Add:
					return S1.Evaluate() + S2.Evaluate();
				case ArithSpec.Subtract:
					return S1.Evaluate() - S2.Evaluate();
				case ArithSpec.Multiply:
					return S1.Evaluate() * S2.Evaluate();
				case ArithSpec.Divide:
					return S1.Evaluate() / S2.Evaluate();
				default:
					throw new InvalidOperationException();
			} 
		}
		
		public readonly SExpr S1;
		public readonly ArithSpec Op;
		public readonly SExpr S2;

		public ArithSExpr(SExpr S1, ArithSpec Op, SExpr S2)
		{
			this.S1 = S1;
			this.Op = Op;
			this.S2 = S2;
		}

		public override string ToString()
		{
			return string.Format("[ArithSExpr {0} {1} {2}]", S1, Op, S2);
		}
		
		public List<Instruction> FetchToField(FieldSRef dest)
		{
			var code = new List<Instruction>();

			var scratch1 = S1 as FieldSRef;
			if (scratch1 == null)
			{
				if (S1.IsConstant())
				{
					scratch1 = FieldSRef.Imm1();
				}
				else if (S1 is IntVarSRef)
				{
					scratch1 = ((IntVarSRef)S1).BaseField();
				}
				else
				{
					scratch1 = FieldSRef.ScratchInt();
					code.AddRange(S1.FetchToField(scratch1));
				}
			}

			var scratch2 = S2 as FieldSRef;
			if (scratch2 == null)
			{
				if (S2.IsConstant())
				{
					scratch2 = FieldSRef.Imm2();
				}
				else if (S2 is IntVarSRef)
				{
					scratch2 = ((IntVarSRef)S2).BaseField();
				}
				else
				{
					scratch2 = FieldSRef.ScratchInt();
					code.AddRange(S2.FetchToField(scratch2));
				}
			}

			Opcode op;
			switch (Op)
			{
				case ArithSpec.Add:
					op = Opcode.Add;
					break;
				case ArithSpec.Subtract:
					op = Opcode.Sub;
					break;
				case ArithSpec.Multiply:
					op = Opcode.Mul;
					break;
				case ArithSpec.Divide:
					op = Opcode.Div;
					break;
				default:
					throw new InvalidOperationException();
			}

			code.Add(new Instruction
			{
				opcode = op,
				op1 = scratch1,
				imm1 = S1.IsConstant() ? S1 : null,
				op2 = scratch2,
				imm2 = S2.IsConstant() ? S2 : null,
				dest = dest,
				acc = true
			});

			return code;
		}
	}

}