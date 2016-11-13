
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
	
	public class RegVRef: VRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public Table Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public int reg;
		public string datatype;
		public override string ToString()
		{
			return string.Format("[RegVRef {0}:{1}]", reg, datatype);
		}
		
		public VExpr FlattenExpressions()
		{
			return this;			
		}
	}
	
	public class MemVRef: VRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public Table Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public SExpr addr;
		public string datatype;
		public override string ToString()
		{
			return string.Format("[MemVRef {0}:{1}]", addr, datatype);
		}
		
		public VExpr FlattenExpressions()
		{
			return this;			
		}
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
		
		public VExpr FlattenExpressions()
		{
			//TODO: if register, return a RegVRef, else MemVRef
			Symbol varsym =  new Symbol();
			if(Program.CurrentFunction != null) varsym = Program.CurrentFunction.locals.FirstOrDefault(sym=>sym.name == this.name);
			if(varsym.name == null) varsym = Program.CurrentProgram.Symbols.FirstOrDefault(sym=>sym.name == this.name);
			switch (varsym.type) {
				case SymbolType.Register:
					return new RegVRef{reg=varsym.fixedAddr??-1,datatype=varsym.datatype};
				case SymbolType.Data:
					return new MemVRef{addr=(IntSExpr)varsym.fixedAddr??-1,datatype=varsym.datatype};
			}
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

		public VExpr FlattenExpressions()
		{
			return this;			
		}
	}
	
}