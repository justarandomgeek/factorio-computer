using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
{

	public class Clear:Statement
	{
		public FieldSRef field;

		public Clear(FieldSRef field)
		{
			if (field.AsDirectField() == null) throw new ArgumentOutOfRangeException("field", "can only be a register field");
			this.field = field;
		}

		public override string ToString()
		{
			return string.Format("[Clear {0}]", field);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}

		public static implicit operator Instruction(Clear c)
		{
			return new Instruction
			{
				opcode = Opcode.Sub,
				op2 = c.field,
				dest = c.field,
				acc = true,
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