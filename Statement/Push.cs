using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class Push:Statement
	{
		public readonly PointerIndex stack;
		public readonly RegVRef reg;
		public readonly bool clear;

		public Push(RegVRef reg, PointerIndex stack = PointerIndex.CallStack, bool clear = false)
		{
			this.reg = reg;
			this.stack = stack;
			this.clear = clear;
		}
		
		public override string ToString()
		{
			return string.Format("[Push {0} {1}{2}]", stack, reg, clear?" x":"");
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}

		public static implicit operator Instruction(Push p)
		{
			var i = new Instruction {
				opcode = Opcode.Push,
				idx = p.stack,
				op2 = p.reg
			};
			if (p.clear)
				i.dest = p.reg;
			return i;
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