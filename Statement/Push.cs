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

		public Push(RegVRef reg, PointerIndex stack = PointerIndex.CallStack)
		{
			this.reg = reg;
			this.stack = stack;
		}
		
		public override string ToString()
		{
			return string.Format("[Push {0} {1}]", stack, reg);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}

		public static implicit operator Instruction(Push p)
		{
			return new compiler.Instruction
			{
				opcode = compiler.Opcode.Push,
				idx = p.stack,
				op2 = p.reg
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
		
		public Table Opcode()
		{
			var op = new Table{datatype="opcode"};
			op.Add("op",	83		);
			op.Add("index",	(int)stack		);
			op.Add("R2",	reg.reg	);
			return op;
		}
	}

}