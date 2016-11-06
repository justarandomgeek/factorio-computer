
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

		public SExpr CollapseConstants()
		{
			return this;			
		}
	}
	
	public class FieldSRef: SRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public int Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public string varname;
		public string fieldname;
		public override string ToString()
		{
			return string.Format("[FieldSRef {0}.{1}]", varname, fieldname);
		}
		public SExpr CollapseConstants()
		{
			return this;			
		}

	}
	
}