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
		public static IntSExpr One  = new IntSExpr(0);

		public bool IsConstant() { return true; }
		public int Evaluate() { return value; }
		
		public override string ToString()
		{
			return string.Format("{0}", value);
		}
		public SExpr FlattenExpressions()
		{
			return this;			
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
			throw new NotImplementedException();
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