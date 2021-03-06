using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
{

	public class StringVExpr: VExpr
	{
		public bool IsConstant()
		{
			return true;
		}
		public readonly string text;
		public StringVExpr(string s) { text = s; }
		public static implicit operator StringVExpr(string s) { return new StringVExpr(s); }
		public override string ToString()
		{
			return string.Format("\"{0}\"", text);
		}

		public static bool operator ==(StringVExpr a1, StringVExpr a2) { return a1.Equals(a2); }
		public static bool operator !=(StringVExpr a1, StringVExpr a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return text.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as StringVExpr;
			if (other != null)
			{
				return other.text == this.text;
			}
			else
			{
				return false;
			}

		}

		public string datatype { get { return "var"; } }

		public List<Instruction> FetchToReg(RegVRef dest)
		{
			return new Table(this.text).FetchToReg(dest);
		}

		public RegVRef AsReg()
		{
			return null;
		}
	}

}