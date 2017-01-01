using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class ArrayVRef: VRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public Table Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public readonly string arrname;
		public readonly SExpr offset;

		public string datatype
		{
			get
			{
				Symbol varsym = new Symbol();
				if (Program.CurrentFunction != null) varsym = Program.CurrentFunction.locals.FirstOrDefault(sym => sym.name == this.arrname);
				if (varsym.name == null) varsym = Program.CurrentProgram.Symbols.FirstOrDefault(sym => sym.name == this.arrname);

				return varsym.datatype;
			}
		}
		public bool IsLoaded { get { return false; } } //TODO: Maybe sometimes loaded?

		public ArrayVRef(string arrname, SExpr offset)
		{
			this.arrname = arrname;
			this.offset = offset;
		}

		public static bool operator ==(ArrayVRef a1, ArrayVRef a2){ return a1.Equals(a2); }
		public static bool operator !=(ArrayVRef a1, ArrayVRef a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return arrname.GetHashCode() ^ offset.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as ArrayVRef;
			if (other != null)
			{
				return other.arrname == this.arrname && other.offset == this.offset;
			}
			else
			{
				return false;
			}
			
		}

		public override string ToString()
		{
			return string.Format("[ArrayVRef {0}+{1}]", arrname, offset);
		}

		public List<Instruction> FetchToReg(RegVRef dest)
		{
			Symbol varsym = new Symbol();
			if (Program.CurrentFunction != null) varsym = Program.CurrentFunction.locals.FirstOrDefault(sym => sym.name == this.arrname);
			if (varsym.name == null) varsym = Program.CurrentProgram.Symbols.FirstOrDefault(sym => sym.name == this.arrname);

			if (offset.IsConstant())
			{
				PointerIndex f = varsym.frame;

				return new MemVRef(new AddrSExpr(arrname, offset.Evaluate()), varsym.datatype).FetchToReg(dest);
			}
			else
			{
				return new MemVRef(new ArithSExpr(new AddrSExpr(arrname), ArithSpec.Add, offset), varsym.datatype).FetchToReg(dest);
			}
		}

		public List<Instruction> PutFromReg(RegVRef src)
		{
			Symbol varsym = new Symbol();
			if (Program.CurrentFunction != null) varsym = Program.CurrentFunction.locals.FirstOrDefault(sym => sym.name == this.arrname);
			if (varsym.name == null) varsym = Program.CurrentProgram.Symbols.FirstOrDefault(sym => sym.name == this.arrname);

			if (offset.IsConstant())
			{
				PointerIndex f = varsym.frame;

				return new MemVRef(new AddrSExpr(arrname, offset.Evaluate()), varsym.datatype).PutFromReg(src);
			}
			else
			{
				return new MemVRef(new ArithSExpr(new AddrSExpr(arrname), ArithSpec.Add, offset), varsym.datatype).PutFromReg(src);
			}
		}
	}

}