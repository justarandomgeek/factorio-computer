using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
{

	public class While:Statement
	{
		//TODO: do/while loops, body-first?


		public Branch branch;
		public Block body;
		public override string ToString()
		{
			return string.Format("[While Branch={0} [{1}]]", branch, body.Count);
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
			body.Print(prefix +"  ");
		}
		
		public List<Instruction> CodeGen()
		{
			var b = new List<Instruction>();
			if (body.Count == 0)
			{
				// Empty loop, just wait on self until fails...
				
				b.AddRange(branch.CodeGen(0, 1));

			}
			else
			{
				b.AddRange(FieldSRef.ResetScratchInts());
				
				var flatbody = body.CodeGen();
				
				b.AddRange(branch.CodeGen(1, 1 + flatbody.Count + 2));

				b.AddRange(flatbody);

				b.AddRange(FieldSRef.ResetScratchInts(true));

				b.Add(new Jump { target = new IntSExpr(-b.Count), relative = true });		
				

			}

			return b;
			
		}

		public static Block VFor(VRef loopvar, VExpr start, VExpr end, VExpr increment, Block body)
		{
			throw new NotImplementedException();
		}

		public static Block SFor(SRef loopvar, SExpr start, SExpr end, SExpr increment, Block body)
		{
			var b = new Block();
			b.Add(new SAssign { target=loopvar, source = start });
			//TODO: precompute end/increment if non-const?
			body.Add(new SAssign { target = loopvar, source = new ArithSExpr(loopvar, ArithSpec.Add, increment) });
			b.Add(new While {
				
				branch = new SBranch {
					S1 = loopvar,
					//TODO: downcounts?
					Op = CompSpec.LessEqual,
					S2 = end },
				body = body
			});
			return b;
		}
	}

}