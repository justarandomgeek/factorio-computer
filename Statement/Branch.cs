using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class Branch
	{
		public SExpr S1;
		public CompSpec Op;
		public SExpr S2;
		
		public override string ToString()
		{
			return string.Format("[Branch {0} {1} {2}]", S1, Op, S2);
		}
		
		public List<Instruction> CodeGen(int truejump, int falsejump)
		{
			var b = new List<Instruction>();

			var f1 = S1.AsDirectField() ?? (S1.IsConstant() ? FieldSRef.Imm1() : null);
			if(f1 == null)
			{
				f1 = FieldSRef.ScratchInt();
				b.AddRange(S1.FetchToField(f1));
			}

			var f2 = S2.AsDirectField() ?? (S2.IsConstant() ? FieldSRef.Imm2() : null);
			if (f2 == null)
			{
				f2 = FieldSRef.ScratchInt();
				b.AddRange(S2.FetchToField(f2));
			}
			
			b.Add(new Instruction
			{
				opcode = Opcode.Branch,
				op1 = f1,
				imm1 = S1.IsConstant() ? new IntSExpr(S1.Evaluate()) : null,
				op2 = f2,
				imm2 = S2.IsConstant() ? new IntSExpr(S2.Evaluate()) : null,
				rjmpeq = this.Op.HasFlag(CompSpec.Equal) ? truejump : falsejump,
				rjmplt = this.Op.HasFlag(CompSpec.Less) ? truejump : falsejump,
				rjmpgt = this.Op.HasFlag(CompSpec.Greater) ? truejump : falsejump,
			});
			
			return b;	
		}
	}

}