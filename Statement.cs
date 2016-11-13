
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
		Block Flatten();
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
		
		public Block Flatten()
		{
			Block b = new Block();
			//TODO: flatten properly - expressions and internal blocks
			source = source.FlattenExpressions();
			target = (VRef)target.FlattenExpressions(); //VRefs should always flatten to VRefs, specifically RegVRef or MemVRef
			b.Add(this);			
			return b;
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
				
		public Block Flatten()
		{
			Block b = new Block();
			//TODO: flatten properly - expressions and internal blocks
			source = source.FlattenExpressions();
			target = (SRef)target.FlattenExpressions(); //SRefs should always flatten to SRefs, specifically a FieldSRef over a RegVRef or MemVRef
			b.Add(this);			
			return b;
		}

	}
	
	public class Branch: Statement
	{
		public SExpr S1;
		public CompSpec? Op;
		public SExpr S2;
		
		public int? rjmpeq;
		public int? rjmpgt;
		public int? rjmplt;
		public override string ToString()
		{
			if(Op.HasValue)
			{
				return string.Format("[Branch {0} {1} {2}]", S1, Op, S2);
			} else {
				return string.Format("[Branch {0} ? {1} => {2}:{3}:{4}]", S1, S2, rjmpeq, rjmplt, rjmpgt);
			}
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		
		public void FlattenExpressions()
		{
			S1 = S1.FlattenExpressions();
			S2 = S2.FlattenExpressions();
		}
		
		public Branch Flatten(int truejump, int falsejump)
		{
			return  new Branch{
				S1 = this.S1,
				S2 = this.S2,
				
				rjmpeq = this.Op.GetValueOrDefault().HasFlag(CompSpec.Equal)  ?truejump:falsejump,
				rjmpgt = this.Op.GetValueOrDefault().HasFlag(CompSpec.Greater)?truejump:falsejump,
				rjmplt = this.Op.GetValueOrDefault().HasFlag(CompSpec.Less)   ?truejump:falsejump,
			};
		}
		
		public Block Flatten()
		{
			Block b = new Block();
			//TODO: flatten properly - expressions and internal blocks
			b.Add(this);			
			return b;
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
		
		public Block Flatten()
		{
			Block b = new Block();
			Block flatif = ifblock.Flatten();
			Block flatelse = elseblock.Flatten();
			
			return b;
			
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

		public Block Flatten()
		{
			Block b = new Block();
			if(body.Count==0)
			{
				// Empty loop, just wait on self until fails...
				b.Add(branch.Flatten(0,1));
				
			} else {
				Block flatbody = body.Flatten();
				branch.FlattenExpressions();
				b.Add(branch.Flatten(1,flatbody.Count+2));
				b.AddRange(flatbody);
				b.Add(new Jump{target=(IntSExpr)(-(flatbody.Count+1)),relative=true});
				//TODO: add a jump back to loop
			}
			
			return b;
			
		}
		
	}
	
	public class Jump:Statement
	{
		public SExpr target;
		public SRef callsite;
		public bool relative;
		public bool? setint;

		public override string ToString()
		{
			return string.Format("[Jump {0} Callsite={1}, Relative={2}, Setint={3}]", target, callsite, relative, setint);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
				
		public Block Flatten()
		{
			Block b = new Block();
			//TODO: flatten properly - expressions and internal blocks
			target = target.FlattenExpressions();
			b.Add(this);			
			return b;
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
				
		public Block Flatten()
		{
			Block b = new Block();
			//TODO: flatten properly - expressions and internal blocks
			args.CollapseConstants();
			b.Add(this);			
			return b;
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
		
		public Block Flatten()
		{
			Block b = new Block();
			//TODO: flatten properly - expressions and internal blocks
			returns.CollapseConstants();
			b.Add(this);			
			return b;
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
			ints = ints.Select(se => se.FlattenExpressions()).ToList();
			vars = vars.Select(ve => ve.FlattenExpressions()).ToList();
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