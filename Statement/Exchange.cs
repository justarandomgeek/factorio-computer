using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
{

	public class Exchange : Statement
	{
		public PointerIndex frame;
		public SExpr addr;
		public RegVRef source;
		public RegVRef dest;

		public Exchange(RegVRef reg, PointerIndex frame = PointerIndex.CallStack): this(reg, frame, IntSExpr.Zero){}
		public Exchange(RegVRef reg, PointerIndex frame, SExpr addr): this(reg,reg,frame,addr){}
		public Exchange(RegVRef source, RegVRef dest, PointerIndex frame, SExpr addr)
		{
			
			if (addr.AsDirectField() == null && !addr.IsConstant()) throw new ArgumentException("must be register field or constant", "addr");
			this.source = source;
			this.dest = dest;
			this.frame = frame;
			this.addr = addr;
		}

		public override string ToString()
		{
			return string.Format("[Exchange {0}+{1} {2} => {3}]", frame, addr, source, dest);
		}

		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}

		public static implicit operator Instruction(Exchange p)
		{

			return new nql.Instruction
			{
				opcode = Opcode.MemWrite,
				idx = p.frame,
				op1 = p.addr.AsDirectField() ?? FieldSRef.Imm1(),
				op2 = p.source,
				dest = p.dest,
				imm1 = p.addr.IsConstant() ? p.addr : null,
			};
		}
		public List<Instruction> CodeGen()
		{
			List<Instruction> b = new List<Instruction>();
			b.Add(this);
			return b;
		}
	}
}