using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class While:Statement
	{
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
				FieldSRef.ResetScratchInts();
				b.AddRange(RegVRef.rScratchInts.PutFromReg(RegVRef.rNull));
				var flatbody = body.CodeGen();
				b.AddRange(branch.CodeGen(1, flatbody.Count + 2));
				b.AddRange(flatbody);
				b.Add(new Jump { target = new IntSExpr(-b.Count), relative = true });
				b.AddRange(RegVRef.rScratchInts.PutFromReg(RegVRef.rNull));
				FieldSRef.ResetScratchInts();

			}

			return b;
			
		}
	}

}