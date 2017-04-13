using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public interface Branch
	{
		List<Instruction> CodeGen(int truejump, int falsejump);
	}

	public class SBranch: Branch
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
				op1 = f1, imm1 = S1.IsConstant() ? new IntSExpr(S1.Evaluate()) : null,
				op2 = f2, imm2 = S2.IsConstant() ? new IntSExpr(S2.Evaluate()) : null,
				rjmpeq = this.Op.HasFlag(CompSpec.Equal) ? truejump : falsejump,
				rjmplt = this.Op.HasFlag(CompSpec.Less) ? truejump : falsejump,
				rjmpgt = this.Op.HasFlag(CompSpec.Greater) ? truejump : falsejump,
			});
			
			return b;	
		}
	}

	public class VBranch : Branch
	{
		public VExpr V1;
		public CompSpec Op;
		public VExpr V2;

		public override string ToString()
		{
			return string.Format("[Branch {0} {1} {2}]", V1, Op, V2);
		}

		public List<Instruction> CodeGen(int truejump, int falsejump)
		{
			if( Op == CompSpec.Equal)
			{
				var b = new List<Instruction>();

				b.AddRange(new ArithVExpr(V1,ArithSpec.Subtract,V2).FetchToReg(RegVRef.rFetch(1)));
								
				var flag = FieldSRef.ScratchInt();

				// ensure flag is clear, in case this branch is used in a loop and is coming around again to reuse it
				b.Add(new Clear(flag));
				flag.precleared = true;

				// set flag if every=0
				// TODO: replace this with a conditional expression once those have been done...
				b.Add(new Instruction
				{
					opcode = Opcode.EveryCMPS1,
					op1 = RegVRef.rFetch(1),
					dest = flag,
					acc = true,
					rjmpeq = 1,
				});
				
				b.Add(new Instruction
				{
					opcode = Opcode.Branch,
					op1 = flag,
					rjmpeq = falsejump,
					rjmplt = falsejump, // lt shouldn't happen at all, but...
					rjmpgt = truejump,
				});

				return b;
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}

}