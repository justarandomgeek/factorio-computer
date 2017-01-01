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
				opcode = compiler.Opcode.Jump,
				relative = j.relative,
				idx = j.frame ?? PointerIndex.None,
				op1 = j.target as FieldSRef ?? FieldSRef.Imm1(),
				imm1 = j.target.IsConstant() ? new IntSExpr(j.target.Evaluate()) : null,
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


		public Table Opcode()
		{
			var op = new Table{datatype="opcode"};
			op.Add("op",	70);
			
			if(relative)
			{
				op.Add("signal-green",IntSExpr.One);
			}
			
			if(frame.HasValue)
			{
				op.Add("index", (int)frame);
			}
			
			if( target is FieldSRef)
			{
				op.Add("R1",	((RegVRef)((FieldSRef)target).varref).reg);
				op.Add("S1",	new FieldIndexSExpr(((FieldSRef)target).fieldname,((RegVRef)((FieldSRef)target).varref).datatype));
			} else if( target is IntSExpr || target is AddrSExpr)
			{
				op.Add("R1",	13);
				op.Add("S1",	new FieldIndexSExpr("Imm1","opcode"));
				op.Add("Imm1",	target);
			}
			
			if(callsite != null)
			{
				if (callsite is FieldSRef)
				{
					op.Add("Rd",	((RegVRef)((FieldSRef)callsite).varref).reg);
					op.Add("Sd",	new FieldIndexSExpr(((FieldSRef)callsite).fieldname,((RegVRef)((FieldSRef)callsite).varref).datatype));
					op.Add("acc",   1);
				}
			}
			
			
			
			return op;
		}
	}

}