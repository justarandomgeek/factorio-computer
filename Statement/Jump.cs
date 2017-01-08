using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class Jump:Statement
	{
		public SExpr target;
		public SRef callsite;
		public bool relative;
		public bool? setint;
		public PointerIndex? frame;

		public override string ToString()
		{
			return string.Format("[Jump {0} Callsite={1}, Relative={2}, Setint={3}, Frame={4}]", target, callsite, relative, setint, frame);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
				
		public static implicit operator Instruction(Jump j)
		{
			return new Instruction
			{
				opcode = Opcode.Jump,
				relative = j.relative,
				idx = j.frame ?? PointerIndex.None,
				op1 = j.target as FieldSRef ?? FieldSRef.Imm1(),
				imm1 = j.target.IsConstant() ? j.target : null,
				dest = j.callsite as FieldSRef,
				acc = j.callsite is FieldSRef
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