
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
		public bool IsConstant()
		{
			return false;
		}
		public int Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public string name;
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
		public VRef varref;
		public string fieldname;

		private FieldSRef() { }
		public static FieldSRef CallSite { get { return new FieldSRef { varref = RegVRef.rScratch, fieldname = "signal-0" }; } }
		public static FieldSRef ScratchInt { get { return new FieldSRef { varref = RegVRef.rScratch, fieldname = "signal-0" }; } }
		public static FieldSRef SReturn { get { return new FieldSRef { varref = RegVRef.rScratch, fieldname = "signal-1" }; } }
		public static FieldSRef GlobalInt(string intname) { return new FieldSRef { varref = RegVRef.rGlobalInts, fieldname = intname }; }
		public static FieldSRef LocalInt(string funcname, string intname) { return new FieldSRef { varref = RegVRef.rLocalInts(funcname), fieldname = intname }; }
		public static FieldSRef IntArg(string funcname, string intname) { return new FieldSRef { varref = RegVRef.rIntArgs(funcname), fieldname = intname }; }

		public static FieldSRef VarField(VRef varref, string fieldname) { return new FieldSRef { varref = varref, fieldname = fieldname }; }

		public override string ToString()
		{
			return string.Format("[FieldSRef {0}.{1}]", varref, fieldname);
		}
		public SExpr FlattenExpressions()
		{
			varref = (VRef)varref.FlattenExpressions();
			return this;			
		}

	}
	
}