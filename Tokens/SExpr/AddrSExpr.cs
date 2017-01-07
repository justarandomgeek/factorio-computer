using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class AddrSExpr: SExpr
	{
		public bool IsConstant()
		{
			return true;
		}
		
		public int Evaluate()
		{
			var s = Program.CurrentProgram.Symbols.Find(sym=>sym.name == symbol);
			return (s.fixedAddr??0) + (offset??0);
		}
		public readonly string symbol;
		public readonly int? offset;

		public AddrSExpr(string sym, int? offset = null)
		{
			this.symbol = sym;
			this.offset = offset;
		}

		public override string ToString()
		{
			return string.Format("{2}{0}{1}",
				symbol,
				offset.HasValue ? "+" + offset : "",
				frame() != PointerIndex.None ? "[" + frame() + "]" : ""
				);
			
		}

		public static bool operator ==(AddrSExpr a1, AddrSExpr a2) { return a1.Equals(a2); }
		public static bool operator !=(AddrSExpr a1, AddrSExpr a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return symbol.GetHashCode() ^ offset.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as AddrSExpr;
			if (other != null)
			{
				return other.symbol == this.symbol && other.offset == this.offset;
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
			var s = Program.CurrentProgram.Symbols.Find(sym => sym.name == symbol);
			return s.frame;
		}

		public FieldSRef AsDirectField()
		{
			return null;
		}
	}

}