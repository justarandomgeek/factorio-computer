
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace compiler
{

	public interface Statement
	{
		void Print(string prefix);
		void CollapseConstants();
	}
	
	public class VAssign:Statement
	{
		public VRef target;
		public bool append;
		public VExpr source;
		public override string ToString()
		{
			return string.Format("[VAssign {0} {1} {2}]", target, append?"+=":"=", source);
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		public void CollapseConstants()
		{
			source = source.CollapseConstants();
		}

	}
	public class SAssign:Statement
	{
		public SRef target;
		public bool append;
		public SExpr source;
		public override string ToString()
		{
			return string.Format("[SAssign {0} {1} {2}]", target, append?"+=":"=", source);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		
		public void CollapseConstants()
		{
			source = source.CollapseConstants();
		}

	}
	
	public class Branch
	{
		public SExpr S1;
		public CompSpec Op;
		public SExpr S2;
		public override string ToString()
		{
			return string.Format("[Branch S1={0}, Op={1}, S2={2}]", S1, Op, S2);
		}
		
		public void CollapseConstants()
		{
			S1 = S1.CollapseConstants();
			S2 = S2.CollapseConstants();
		}
	}
	
	public class If:Statement
	{
		public Branch branch;
		public Block ifblock;
		public Block elseblock;
		public override string ToString()
		{
			return string.Format("[If Branch={0} [{1}] [{2}]]", branch, ifblock.Count, elseblock.Count);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
			ifblock.Print(prefix +"  ");
			if(elseblock!=null)
			{
				Console.WriteLine(prefix+"Else");
				elseblock.Print(prefix+"  ");					
			}
		}
		
		public void CollapseConstants()
		{
			branch.CollapseConstants();
			if(ifblock!=null)ifblock.CollapseConstants();
			if(elseblock!=null)elseblock.CollapseConstants();
		}

	}
	
	public class While:Statement
	{
		public Branch branch;
		public Block body;
		public override string ToString()
		{
			return string.Format("[While Branch={0} [{1}]]", branch, body.Count);
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
			body.Print(prefix +"  ");
		}

		public void CollapseConstants()
		{
			branch.CollapseConstants();
			if(body!=null)body.CollapseConstants();
		}
		
		
	}
	
	public class FunctionCall:Statement
	{
		public string name;
		public ExprList args;
		public RefList returns;
		public override string ToString()
		{
			return string.Format("[FunctionCall {0}({1}) => {2}]", name, args, returns);
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		
		public void CollapseConstants()
		{
			args.CollapseConstants();
		}

	}
	
	public class Return:Statement
	{
		public ExprList returns;
		public override string ToString()
		{
			return string.Format("[Return Returns={0}]", returns);
		}	
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		public void CollapseConstants()
		{
			returns.CollapseConstants();
		}
	}
	
	public class ExprList{
		public List<SExpr> ints = new List<SExpr>();
		public List<VExpr> vars = new List<VExpr>();
		public override string ToString()
		{
			return string.Format("[ExprList Ints={0} || Vars={1}]", string.Join(",",ints), string.Join(",",vars));
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		public void CollapseConstants()
		{
			ints = ints.Select(se => se.CollapseConstants()).ToList();
			vars = vars.Select(ve => ve.CollapseConstants()).ToList();
		}
	}
	
	public class RefList{
		public List<SRef> ints = new List<SRef>();
		public List<VRef> vars = new List<VRef>();
		public override string ToString()
		{
			return string.Format("[RefList Ints={0} || Vars={1}]", string.Join(",",ints), string.Join(",",vars));
		}
	}
	
}