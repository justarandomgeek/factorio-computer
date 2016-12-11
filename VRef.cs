
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
		public int reg;
		public string datatype;

		//public RegVRef() { }
		public RegVRef(int i) { reg = i; }
		public RegVRef(int i, string t) { reg = i; datatype = t; }

		public static RegVRef rNull { get { return new RegVRef(0, "var"); } }
		public static RegVRef rGlobalInts { get { return new RegVRef(1, "__globalints"); } }
		public static RegVRef rLocalInts(string funcname) { return new RegVRef(2, "__li" + funcname); }
		public static RegVRef rIntArgs(string funcname) { return new RegVRef(8, "__li" + funcname); }
		public static RegVRef rScratch { get { return new RegVRef(8, "var"); } }
		public static RegVRef rScratch2 { get { return new RegVRef(7, "var"); } }
		public static RegVRef rVarArgs { get { return new RegVRef(7, "var"); } }

		public bool IsConstant()
		{
			return false;
		}
		public Table Evaluate()
		{
			throw new InvalidOperationException(); 
		}

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
			//if register, return a RegVRef, else MemVRef
			Symbol varsym =  new Symbol();
			if(Program.CurrentFunction != null) varsym = Program.CurrentFunction.locals.FirstOrDefault(sym=>sym.name == this.name);
			if(varsym.name == null) varsym = Program.CurrentProgram.Symbols.FirstOrDefault(sym=>sym.name == this.name);
			switch (varsym.type) {
				case SymbolType.Register:
					return new RegVRef(varsym.fixedAddr??-1,varsym.datatype);
				case SymbolType.Data:
					return new MemVRef{addr=new AddrSExpr{symbol=varsym.name},datatype=varsym.datatype};
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
			if(offset.IsConstant() & Program.CurrentProgram.Symbols.Exists(s=>s.name==arrname))
			{
				var sym = Program.CurrentProgram.Symbols.Find(s=>s.name==arrname);

                PointerIndex f = PointerIndex.None;
				if(!sym.fixedAddr.HasValue) f = sym.type==SymbolType.Data? PointerIndex.ProgData : PointerIndex.ProgConst;
				
				return new MemVRef{
					addr=new AddrSExpr{symbol=arrname,offset=offset.Evaluate(),frame=f},
					datatype = sym.datatype
				};
			}
			return this;			
		}
	}
	
}