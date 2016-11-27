
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace compiler
{

	
	public interface VExpr
	{
		bool IsConstant();
		Table Evaluate();
		VExpr FlattenExpressions();
	}
	
		
	public class ArithVExpr: VExpr
	{
		public bool IsConstant()
		{
			return V1.IsConstant() && V2.IsConstant();
		}
		
		public Table Evaluate()
		{
			if(!IsConstant())
			{
				throw new InvalidOperationException();
			}
			switch (Op) {
				case ArithSpec.Add:
					return V1.Evaluate() + V2.Evaluate();
				case ArithSpec.Subtract:
					return V1.Evaluate() - V2.Evaluate();
				case ArithSpec.Multiply:
					return V1.Evaluate() * V2.Evaluate();
				case ArithSpec.Divide:
					return V1.Evaluate() / V2.Evaluate();
				default:
					throw new InvalidOperationException();
			} 
		}
		
		public VExpr V1;
		public ArithSpec Op;
		public VExpr V2;	
		public override string ToString()
		{
			return string.Format("[ArithVExpr {0} {1} {2}]", V1, Op, V2);
		}
		
		public VExpr FlattenExpressions()
		{
			if(this.IsConstant())
			{
				return this.Evaluate();
			} else {
				V1 = V1.FlattenExpressions();
				V2 = V2.FlattenExpressions();
				return this;
			}
		}

	}
		
	public class ArithVSExpr: VExpr
	{
		public bool IsConstant()
		{
			return V1.IsConstant() && S2.IsConstant();
		}
		
		public Table Evaluate()
		{
			if(!IsConstant())
			{
				throw new InvalidOperationException();
			}
			switch (Op) {
				case ArithSpec.Add:
					return V1.Evaluate() + S2.Evaluate();
				case ArithSpec.Subtract:
					return V1.Evaluate() - S2.Evaluate();
				case ArithSpec.Multiply:
					return V1.Evaluate() * S2.Evaluate();
				case ArithSpec.Divide:
					return V1.Evaluate() / S2.Evaluate();
				default:
					throw new InvalidOperationException();
			} 
		}
		
		public VExpr V1;
		public ArithSpec Op;
		public SExpr S2;	
		public override string ToString()
		{
			return string.Format("[ArithVSExpr {0} {1} {2}]", V1, Op, S2);
		}

		public VExpr FlattenExpressions()
		{
			if(this.IsConstant())
			{
				return this.Evaluate();
			} else {
				V1 = V1.FlattenExpressions();
				S2 = S2.FlattenExpressions();
				return this;
			}
		}
	}
	
	public class StringVExpr: VExpr
	{
		public bool IsConstant()
		{
			return true;
		}
		public Table Evaluate()
		{
			return new Table(text);
		}
		public string text;
		public static implicit operator StringVExpr(string s)
		{
			return new StringVExpr{text=s};
		}
		public override string ToString()
		{
			return string.Format("\"{0}\"", text);
		}
		public VExpr FlattenExpressions()
		{
			return this.Evaluate();
		}
	}
	
    

}