using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
{

	public class IntSExpr: SExpr
	{
		public readonly int value;

		public IntSExpr(int? i){ value = i??0; }
		
		public static IntSExpr Zero = new IntSExpr(0);
		public static IntSExpr One  = new IntSExpr(1);

		public bool IsConstant() { return true; }
		public int Evaluate() { return value; }
		
		public override string ToString()
		{
			return string.Format("{0}", value);
		}

		public static bool operator ==(IntSExpr a1, int i2) { return (a1?.value ?? 0) == i2; }
		public static bool operator !=(IntSExpr a1, int i2) { return (a1?.value ?? 0) != i2; }
		public static bool operator ==(IntSExpr a1, IntSExpr a2) { return (a1?.value ?? 0) == (a2?.value ?? 0); }
		public static bool operator !=(IntSExpr a1, IntSExpr a2) { return (a1?.value ?? 0) != (a2?.value ?? 0); }
		public override int GetHashCode()
		{
			return value.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as IntSExpr;
			if (obj is IntSExpr)
			{
				return this.value == ((IntSExpr)obj).value;
			}
			else if (obj is int)
			{
				return this.value == (int)obj;
			}
			else
			{
				return false;
			}

		}

		public List<Instruction> FetchToField(FieldSRef dest)
		{
			var code = new List<Instruction>();
			code.Add(new Instruction {
				opcode = Opcode.Sub,
				op1 = FieldSRef.Imm1(),
				imm1 = this,
				op2 = dest.precleared ? null : dest,
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