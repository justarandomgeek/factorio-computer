using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class FieldIndexSExpr : SExpr
	{
		public bool IsConstant()
		{
			return true;
		}
		public int Evaluate()
		{
			string signal = field;
			if((type??"var")!="var" && Program.CurrentProgram.Types[type].ContainsKey(field))  {
				signal = Program.CurrentProgram.Types[type][field];
			}
			return Program.CurrentProgram.NativeFields.IndexOf(signal)+1;
		}
		public readonly string field;
		public readonly string type;

		public FieldIndexSExpr(string field, string type)
		{
			this.field = field;
			this.type = type;
		}

		public override string ToString()
		{
			if(type == "var" || type == null) return field;
			return string.Format("{1}::{0}", field, type);
		}

		public SExpr FlattenExpressions()
		{
			return this;
		}

		public static bool operator ==(FieldIndexSExpr a1, FieldIndexSExpr a2) { return a1.Equals(a2); }
		public static bool operator !=(FieldIndexSExpr a1, FieldIndexSExpr a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return field.GetHashCode() ^ type.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as FieldIndexSExpr;
			if (other != null)
			{
				return other.field == this.field && other.type == this.type;
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
	}

}