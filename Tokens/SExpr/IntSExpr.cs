using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class IntSExpr: SExpr
	{
		public readonly int value;

		public IntSExpr(int? i) : this(i ?? 0) { }
		public IntSExpr(int i) { value = i; }

		public static IntSExpr Zero = new IntSExpr(0);
		public static IntSExpr One  = new IntSExpr(1);

		public bool IsConstant() { return true; }
		public int Evaluate() { return value; }
		
		public override string ToString()
		{
			return string.Format("{0}", value);
		}
		
		public static bool operator ==(IntSExpr a1, IntSExpr a2) { return a1.Equals(a2); }
		public static bool operator !=(IntSExpr a1, IntSExpr a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return value.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as IntSExpr;
			if (other != null)
			{
				return other.value == this.value;
			}
			else
			{
				return false;
			}

		}

		public List<Instruction> FetchToField(FieldSRef dest)
		{
			var code = new List<Instruction>();
			code.Add(new compiler.Instruction {
				opcode = Opcode.Sub,
				op1 = FieldSRef.Imm1(),
				imm1 = this,
				op2 = dest,
				dest = dest,
				acc = true
			});
			return code;
		}

		public PointerIndex frame()
		{
			return PointerIndex.None;
		}

		public FieldSRef AsDirectField()
		{
			return null;
		}
	}

}