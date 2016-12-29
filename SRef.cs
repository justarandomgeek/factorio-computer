
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace compiler
{

	public interface SRef : SExpr // the assignable subset of scalar expressions
	{
		
	}
	
	public class IntVarSRef: SRef
	{
		public readonly string name;
		
		public IntVarSRef(string name)
		{
			this.name = name;
		}
		
		public bool IsConstant()
		{
			return false;
		}
		public int Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public override string ToString()
		{
			return string.Format("[IntVarSRef {0}]", name);
		}

		public SExpr FlattenExpressions()
		{
			if(Program.CurrentFunction.localints.ContainsKey(name))
			{
				return FieldSRef.LocalInt(Program.CurrentFunction.name,name);
			} else {
				return FieldSRef.GlobalInt(name);	
			}
			
		}
	}

	public class FieldSRef : SRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public int Evaluate()
		{
			throw new InvalidOperationException();
		}
		public readonly VRef varref;
		public readonly string fieldname;

		private FieldSRef() { }
		public FieldSRef(VRef varref, string fieldname) { this.varref = varref; this.fieldname = fieldname; }
		public static FieldSRef CallSite { get { return new FieldSRef(RegVRef.rScratch, "signal-0"); } }
		public static FieldSRef ScratchInt { get { return new FieldSRef(RegVRef.rScratch, "signal-0"); } }
		public static FieldSRef SReturn { get { return new FieldSRef(RegVRef.rScratch, "signal-1"); } }
		public static FieldSRef GlobalInt(string intname) { return new FieldSRef(RegVRef.rGlobalInts,intname); }
		public static FieldSRef LocalInt(string funcname, string intname) { return new FieldSRef(RegVRef.rLocalInts(funcname),intname); }
		public static FieldSRef IntArg(string funcname, string intname) { return new FieldSRef(RegVRef.rIntArgs(funcname), intname); }
		public static FieldSRef Imm1() { return new FieldSRef(RegVRef.rOpcode, "Imm1"); }
		public static FieldSRef Imm2() { return new FieldSRef(RegVRef.rOpcode, "Imm2"); }

		public static FieldSRef VarField(VRef varref, string fieldname) { return new FieldSRef(varref, fieldname); }

		public static FieldSRef Pointer(PointerIndex ptr)
		{
			string[] ptrnames = { "err", "callstack", "progbase", "progdata", "localdata" };
			return new FieldSRef(RegVRef.rIndex, ptrnames[(int)ptr]);
		}


		public FieldSRef(RegVRef reg) { this.varref = reg; }
		public static implicit operator FieldSRef(RegVRef reg) { return new FieldSRef(reg); }

		public override string ToString()
		{
			return string.Format("[FieldSRef {0}.{1}]", varref, fieldname);
		}
		public SExpr FlattenExpressions()
		{
			return new FieldSRef((VRef)varref.FlattenExpressions(),this.fieldname);			
		}

	}
	
}