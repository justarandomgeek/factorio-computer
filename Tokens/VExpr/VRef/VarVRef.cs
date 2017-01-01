using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class VarVRef: VRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public Table Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public readonly string name;

		public VarVRef(string name)
		{
			this.name = name;
		}

		public string datatype
		{
			get
			{
				Symbol varsym = new Symbol();
				if (Program.CurrentFunction != null) varsym = Program.CurrentFunction.locals.FirstOrDefault(sym => sym.name == this.name);
				if (varsym.name == null) varsym = Program.CurrentProgram.Symbols.FirstOrDefault(sym => sym.name == this.name);

				return varsym.datatype;
			}
		}
		public bool IsLoaded
		{
			get
			{
				Symbol varsym = new Symbol();
				if (Program.CurrentFunction != null) varsym = Program.CurrentFunction.locals.FirstOrDefault(sym => sym.name == this.name);
				if (varsym.name == null) varsym = Program.CurrentProgram.Symbols.FirstOrDefault(sym => sym.name == this.name);

				return varsym.type == SymbolType.Register; //TODO: dynamic loading/unloading?

			}
		}

		public override string ToString()
		{
			return string.Format("[VarVRef {0}]", name);
		}
		
		public VExpr FlattenExpressions()
		{
			//if register, return a RegVRef, else MemVRef
			Symbol varsym =  new Symbol();
			if(Program.CurrentFunction != null) varsym = Program.CurrentFunction.locals.FirstOrDefault(sym=>sym.name == this.name);
			if(varsym.name == null) varsym = Program.CurrentProgram.Symbols.FirstOrDefault(sym=>sym.name == this.name);
			switch (varsym.type) {
				case SymbolType.Register:
					return new RegVRef(varsym.fixedAddr??-1,varsym.datatype);
				case SymbolType.Data:
					return new MemVRef(new AddrSExpr(varsym.name),varsym.datatype);
			}
			return this;
		}

		public static bool operator ==(VarVRef a1, VarVRef a2) { return a1.Equals(a2); }
		public static bool operator ==(VarVRef a1, MemVRef a2) { return a1.Equals(a2); }
		public static bool operator ==(VarVRef a1, RegVRef a2) { return a1.Equals(a2); }
		public static bool operator !=(VarVRef a1, VarVRef a2) { return !a1.Equals(a2); }
		public static bool operator !=(VarVRef a1, MemVRef a2) { return !a1.Equals(a2); }
		public static bool operator !=(VarVRef a1, RegVRef a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return name.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as VarVRef;
			if (other != (VarVRef)null)
			{
				return other.name == this.name;
			}
			else if(obj is MemVRef || obj is RegVRef)
			{
				return this.BaseVRef().Equals(obj);
			}
			else
			{
				return false;
			}

		}

		public VRef BaseVRef()
		{
			//if register, return a RegVRef, else MemVRef
			Symbol varsym = new Symbol();
			if (Program.CurrentFunction != null) varsym = Program.CurrentFunction.locals.FirstOrDefault(sym => sym.name == this.name);
			if (varsym.name == null) varsym = Program.CurrentProgram.Symbols.FirstOrDefault(sym => sym.name == this.name);
			switch (varsym.type)
			{
				case SymbolType.Register:
					return new RegVRef(varsym.fixedAddr ?? -1, varsym.datatype);
				case SymbolType.Data:
					return new MemVRef(new AddrSExpr(varsym.name), varsym.datatype);
				default:
					throw new InvalidOperationException();
			}
		}


		public static explicit operator RegVRef(VarVRef v)
		{
			return v.BaseVRef() as RegVRef;
		}

		public static explicit operator MemVRef(VarVRef v)
		{
			return v.BaseVRef() as MemVRef;
		}

		public List<Instruction> FetchToReg(RegVRef dest)
		{
			return BaseVRef().FetchToReg(dest);			
		}

		public List<Instruction> PutFromReg(RegVRef src)
		{
			return BaseVRef().PutFromReg(src);
		}
	}

}