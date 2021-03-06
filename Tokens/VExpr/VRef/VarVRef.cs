using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
{

	public class VarVRef: VRef
	{
		public bool IsConstant()
		{
			return false;
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
				return VarSym().datatype;
			}
		}
		public bool IsLoaded
		{
			get
			{
				return VarSym().type == SymbolType.Register; //TODO: dynamic loading/unloading?
			}
		}

		public override string ToString()
		{
			return string.Format("[VarVRef {0}]", name);
		}
		
		public static bool operator ==(VarVRef a1, VarVRef a2) { return (a1?.Equals(a2)) ?? false; }
		public static bool operator ==(VarVRef a1, MemVRef a2) { return (a1?.Equals(a2)) ?? false; }
		public static bool operator ==(VarVRef a1, RegVRef a2) { return (a1?.Equals(a2)) ?? false; }
		public static bool operator !=(VarVRef a1, VarVRef a2) { return (!a1?.Equals(a2)) ?? false; }
		public static bool operator !=(VarVRef a1, MemVRef a2) { return (!a1?.Equals(a2)) ?? false; }
		public static bool operator !=(VarVRef a1, RegVRef a2) { return (!a1?.Equals(a2)) ?? false; }
		public override int GetHashCode()
		{
			return name.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if(obj == null)
			{
				return false;
			}
			else if (obj is VarVRef)
			{
				return ((VarVRef)obj).name == this.name;
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

		public Symbol VarSym()
		{
			Symbol varsym = new Symbol();
			var func = Program.CurrentFunction ?? Program.CurrentProgram.InFunction;
			if (func != null) varsym = func.locals.Find(sym => sym.name == this.name);
			if (varsym.name == null) varsym = Program.CurrentProgram.Symbols.Find(sym => sym.name == this.name);

			return varsym;
		}

		public VRef BaseVRef()
		{
			//if register, return a RegVRef, else MemVRef
			var varsym = VarSym();
			switch (varsym.type)
			{
				case SymbolType.Register:
					return new RegVRef(varsym.fixedAddr ?? -1, varsym.datatype);
				case SymbolType.Data:
					return new MemVRef(new AddrSExpr(varsym.name), varsym.datatype);
				case SymbolType.Parameter:
					return RegVRef.rArg(varsym.fixedAddr ?? -1);
				default:
					throw new InvalidOperationException();
			}
		}


		public static explicit operator RegVRef(VarVRef v)
		{
			return v == (VarVRef)null ? null : v.BaseVRef() as RegVRef;
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

		public RegVRef AsReg()
		{
			return BaseVRef() as RegVRef;
		}
	}

}