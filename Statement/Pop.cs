using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
{

	public class Pop:Statement
	{
		public readonly PointerIndex stack;
		public readonly RegVRef reg;

		public Pop(RegVRef reg, PointerIndex stack = PointerIndex.CallStack)
		{
			this.reg = reg;
			this.stack = stack;
		}

		public override string ToString()
		{
			return string.Format("[Pop {0} {1}]", stack, reg);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}

		public static implicit operator Instruction(Pop p)
		{
			return new Instruction
			{
				opcode = Opcode.Pop,
				idx = p.stack,
				dest = p.reg
			};
		}
		public List<Instruction> CodeGen()
		{
			List<Instruction> b = new List<Instruction>();
			b.Add(this);
			return b;
		}

		public Block Flatten()
		{
			Block b = new Block();
			b.Add(this);			
			return b;
		}
	}
}