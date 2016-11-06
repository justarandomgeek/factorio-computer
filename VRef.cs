
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace compiler
{

	public interface VRef: VExpr // the assignable subset of vector expressions
	{
		
	}
	
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
		public string name;
		public override string ToString()
		{
			return string.Format("[VarVRef {0}]", name);
		}
		
		public VExpr CollapseConstants()
		{
			return this;			
		}

	}
	
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
		public string arrname;
		public SExpr offset;
		public override string ToString()
		{
			return string.Format("[ArrayVRef {0}+{1}]", arrname, offset);
		}

		public VExpr CollapseConstants()
		{
			return this;			
		}
	}
	
}