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

				b.AddRange(V1.FetchToReg(RegVRef.rScratchTab));

				var r2 = V2.AsReg();
				if (r2 == null)
				{
					// if V2 isn't already a register, borrow rScratchInts temporarily to fetch it
					r2 = RegVRef.rScratchInts;
					b.Add(new Push(RegVRef.rScratchInts));
					b.AddRange(V2.FetchToReg(r2));
				}

				b.Add(new Instruction
				{
					opcode = Opcode.EachMulV,
					acc = true,
					dest = RegVRef.rScratchTab,
					op1 = r2,
					op2 = FieldSRef.Imm2(),
					imm2 = new IntSExpr(-1),
				});

				// restore rScratchInts if needed
				if (r2 == RegVRef.rScratchInts) b.Add(new Pop(RegVRef.rScratchInts));
				var flag = FieldSRef.ScratchInt();

				// ensure flag is clear, in case this branch is used in a loop and is coming around again to reuse it
				b.Add(new Clear(flag));

				// set flag if every=0
				b.Add(new Instruction
				{
					opcode = Opcode.EveryEqS2S1,
					op1 = RegVRef.rScratchTab,
					dest = flag,
					acc = true,
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