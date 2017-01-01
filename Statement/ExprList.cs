using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class ExprList{
		public List<SExpr> ints = new List<SExpr>();
		public VExpr var;
		public override string ToString()
		{
			return string.Format("[ExprList Ints={0} || Var={1}]", string.Join(",",ints), var);
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
	}	

}