using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
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
			return new compiler.Instruction
			{
				opcode = compiler.Opcode.MemWrite,
				idx = p.frame,
				op1 = p.addr as FieldSRef ?? FieldSRef.Imm1(),
				op2 = p.source,
				dest = p.dest,
				imm1 = p.addr is FieldSRef ? null : p.addr, //TODO: this definitely needs work...
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
			var op = new Table { datatype = "opcode" };
			//mem write
			op.Add("op", 81);
			op.Add("index", (int)frame);
			op.Add("R2", source.reg);
			op.Add("Rd", dest.reg);

			var S1 = addr;
			if (S1 is FieldSRef)
			{
				op.Add("R1", ((RegVRef)((FieldSRef)S1).varref).reg);
				op.Add("S1", new FieldIndexSExpr(((FieldSRef)S1).fieldname, ((RegVRef)((FieldSRef)S1).varref).datatype));
				
			}
			else if (S1 is IntSExpr || S1 is AddrSExpr)
			{
				op.Add("R1", 13);
				op.Add("S1", new FieldIndexSExpr("Imm1","opcode"));
				op.Add("Imm1", S1);
			}
			
			
			return op;
		}
	}

}