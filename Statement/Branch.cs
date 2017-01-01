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
			
			//TODO: b.AddRange(S1.FetchToField());
			//TODO: b.AddRange(S2.FetchToField());

			b.Add(new Instruction
			{
				opcode = Opcode.Branch,
				op1 = S1 as FieldSRef ?? FieldSRef.Imm1(),
				imm1 = S1.IsConstant() ? new IntSExpr(S1.Evaluate()) : null,
				op2 = S2 as FieldSRef ?? FieldSRef.Imm2(),
				imm2 = S2.IsConstant() ? new IntSExpr(S2.Evaluate()) : null,
				rjmpeq = this.Op.HasFlag(CompSpec.Equal) ? truejump : falsejump,
				rjmpgt = this.Op.HasFlag(CompSpec.Greater) ? truejump : falsejump,
				rjmplt = this.Op.HasFlag(CompSpec.Less) ? truejump : falsejump,
			});
			
			return b;	
		}
	}

}