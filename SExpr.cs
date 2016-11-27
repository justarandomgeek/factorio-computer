
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace compiler
{
	
	public interface SExpr
	{
		bool IsConstant();
		int Evaluate();
		SExpr FlattenExpressions();
		
	}
	
	public class ArithSExpr: SExpr
	{
		public bool IsConstant()
		{
			return S1.IsConstant() && S2.IsConstant();
		}
		
		public int Evaluate()
		{
			if(!IsConstant())
			{
				throw new InvalidOperationException();
			}
			switch (Op) {
				case ArithSpec.Add:
					return S1.Evaluate() + S2.Evaluate();
				case ArithSpec.Subtract:
					return S1.Evaluate() - S2.Evaluate();
				case ArithSpec.Multiply:
					return S1.Evaluate() * S2.Evaluate();
				case ArithSpec.Divide:
					return S1.Evaluate() / S2.Evaluate();
				default:
					throw new InvalidOperationException();
			} 
		}
		
		public SExpr S1;
		public ArithSpec Op;
		public SExpr S2;	
		public override string ToString()
		{
			return string.Format("[ArithSExpr {0} {1} {2}]", S1, Op, S2);
		}

		public SExpr FlattenExpressions()
		{
			if(this.IsConstant())
			{
				return (IntSExpr)this.Evaluate();
			} else {
				S1 = S1.FlattenExpressions();
				S2 = S2.FlattenExpressions();
				return this;
			}
		}
	}
	
	public class IntSExpr: SExpr
	{
		public bool IsConstant()
		{
			return true;
		}
		public int Evaluate()
		{
			return value;
		}
		public int value;
		public static implicit operator IntSExpr(int i)
		{
			return new IntSExpr{value=i};
		}
        public static implicit operator IntSExpr(PointerIndex i)
        {
            return new IntSExpr { value = (int)i };
        }
        public override string ToString()
		{
			return string.Format("{0}", value);
		}
		public SExpr FlattenExpressions()
		{
			return this;			
		}
	}
	
	public class FieldIndexSExpr : SExpr
	{
		public bool IsConstant()
		{
			return true;
		}
		public int Evaluate()
		{
			string signal = field;
			if((type??"var")!="var" && Program.CurrentProgram.Types[type].ContainsKey(field))  {
				signal = Program.CurrentProgram.Types[type][field];
			}
			return Program.CurrentProgram.NativeFields.IndexOf(signal)+1;
		}
		public string field;
		public string type;
		public override string ToString()
		{
			if(type == "var" || type == null) return field;
			return string.Format("{1}::{0}", field, type);
		}

		public SExpr FlattenExpressions()
		{
			return this;
		}
	}
	
	public class AddrSExpr: SExpr
	{
		public bool IsConstant()
		{
			return true;
		}
		public int Evaluate()
		{
			var s = Program.CurrentProgram.Symbols.Find(sym=>sym.name == symbol);
			return (s.fixedAddr??0) + (offset??0);
		}
		public string symbol;
		public int? offset;
        public PointerIndex frame;
		public override string ToString()
		{
			return string.Format("{2}{0}{1}", 
                symbol,
                offset.HasValue ? "+" + offset : "",
                frame>0 ? "[" + frame + "]" : "");
			
		}
		public SExpr FlattenExpressions()
		{
			return this;
		}
	}
	
}