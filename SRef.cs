
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
				return new FieldSRef{varref=new RegVRef{reg=2,datatype="__li"+Program.CurrentFunction.name},fieldname=name};   	
			} else {
				return new FieldSRef{varref=new RegVRef{reg=1,datatype="__globalints"},fieldname=name};	
			}
			
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
		public VRef varref;
		public string fieldname;
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