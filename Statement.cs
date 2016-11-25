
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
		Table Opcode();
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
			source = source.FlattenExpressions();
			if(source is Table)
			{
				//Convert literal tables to constants and address them by symbol name
				var constname = "__const"+source.GetHashCode();
				Program.CurrentProgram.Symbols.Add(new Symbol{
				                                   	type=SymbolType.Constant,
				                                   	name=constname,
				                                   	datatype=((Table)source).datatype,
				                                   	data=new List<Table>{(Table)source},
				                                   });
				source = new MemVRef{addr=new AddrSExpr{symbol=constname},datatype=((Table)source).datatype};
			}
			target = (VRef)target.FlattenExpressions(); //VRefs should always flatten to VRefs, specifically RegVRef or MemVRef
			b.Add(this);			
			return b;
		}
		
		public Table Opcode()
		{
			var op = new Table{datatype="opcode"};
			if(append) op.Add("acc",(IntSExpr)1);
			
			if(target is MemVRef)
			{
				if(source is RegVRef)
				{
					//mem write
					op.Add("op",(IntSExpr)81);
					var S1 = ((MemVRef)target).addr;
					if( S1 is FieldSRef)
					{
						op.Add("R1",	(IntSExpr)((RegVRef)((FieldSRef)S1).varref).reg);
						op.Add("S1",	new FieldIndexSExpr{field=((FieldSRef)S1).fieldname,type=((RegVRef)((FieldSRef)S1).varref).datatype});
					} else if( S1 is IntSExpr || S1 is AddrSExpr)
					{
						op.Add("R1",	(IntSExpr)13);
						op.Add("S1",	new FieldIndexSExpr{field="Imm1",type="opcode"});
						op.Add("Imm1",	S1);
					}
				}
			}else if(target is RegVRef)
			{
				if(source is RegVRef)
				{
					// reg copy
					op.Add("op",(IntSExpr)50); // V+s=>V
					op.Add("R1",(IntSExpr)((RegVRef)source).reg);
					op.Add("Rd",(IntSExpr)((RegVRef)target).reg);
				} else if (source is MemVRef) 
				{
					//mem read
					op.Add("op",(IntSExpr)82);
					var S1 = ((MemVRef)source).addr;
					if( S1 is FieldSRef)
					{
						op.Add("R1",	(IntSExpr)((RegVRef)((FieldSRef)S1).varref).reg);
						op.Add("S1",	new FieldIndexSExpr{field=((FieldSRef)S1).fieldname,type=((RegVRef)((FieldSRef)S1).varref).datatype});
					} else if( S1 is IntSExpr || S1 is AddrSExpr)
					{
						op.Add("R1",	(IntSExpr)13);
						op.Add("S1",	new FieldIndexSExpr{field="Imm1",type="opcode"});
						op.Add("Imm1",	S1);
					}
					op.Add("Rd",(IntSExpr)((RegVRef)target).reg);
				} else if(source is ArithVExpr)
				{
					var expr = (ArithVExpr)source;
					// must be reg v reg
					if(expr.V1 is RegVRef && expr.V2 is RegVRef)
					{
						// v arith v => v 
					}
				} else if(source is ArithVSExpr)
				{
					// must be reg v reg.sig
					var expr = (ArithVSExpr)source;
					
					if(expr.V1 is RegVRef)
					{
						// v.each arith s => v, 49-52 -+/*
						switch (expr.Op) {
							case ArithSpec.Subtract:
								op.Add("op",(IntSExpr)49);
								break;
								
							case ArithSpec.Add:
								op.Add("op",(IntSExpr)50);
								break;
								
							case ArithSpec.Divide:
								op.Add("op",(IntSExpr)51);
								break;
								
							case ArithSpec.Multiply:
								op.Add("op",(IntSExpr)52);
								break;
						}
						
						if( expr.S2 is FieldSRef)
						{
							op.Add("R2",	(IntSExpr)((RegVRef)((FieldSRef)expr.S2).varref).reg);
							op.Add("S2",	new FieldIndexSExpr{field=((FieldSRef)expr.S2).fieldname,type=((RegVRef)((FieldSRef)expr.S2).varref).datatype});
						} else if( expr.S2 is IntSExpr || expr.S2 is AddrSExpr)
						{
							op.Add("R2",	(IntSExpr)13);
							op.Add("S2",	new FieldIndexSExpr{field="Imm2",type="opcode"});
							op.Add("Imm2",	expr.S2);
						}
					}
					
				}
				
			}
			
			return op;
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
			source = source.FlattenExpressions();
			target = (SRef)target.FlattenExpressions(); //SRefs should always flatten to SRefs, specifically a FieldSRef over a RegVRef or MemVRef
			b.Add(this);			
			return b;
		}

		public Table Opcode()
		{
			var op = new Table{datatype="opcode"};
			if(append) op.Add("acc",(IntSExpr)1);
			
			if(target is FieldSRef)
			{
				var sd = (FieldSRef)target;
				var rd = (RegVRef)sd.varref;
				op.Add("Sd",new FieldIndexSExpr{type=rd.datatype,field=sd.fieldname});
				op.Add("Rd",(IntSExpr)rd.reg);
				
				if(source is FieldSRef || source is IntSExpr || source is AddrSExpr)
				{
					// field copy
					op.Add("op",(IntSExpr)57); // s+s=>s
					
					if( source is FieldSRef)
					{
						op.Add("R1",	(IntSExpr)((RegVRef)((FieldSRef)source).varref).reg);
						op.Add("S1",	new FieldIndexSExpr{field=((FieldSRef)source).fieldname,type=((RegVRef)((FieldSRef)source).varref).datatype});
					} else if( source is IntSExpr || source is AddrSExpr)
					{
						op.Add("R1",	(IntSExpr)13);
						op.Add("S1",	new FieldIndexSExpr{field="Imm1",type="opcode"});
						op.Add("Imm1",	source);
					}
					
				} else if(source is ArithSExpr)
				{
					var expr = (ArithSExpr)source;
					
					// v.each arith s => v, 49-52 -+/*
					switch (expr.Op) {
						case ArithSpec.Subtract:
							op.Add("op",(IntSExpr)49);
							break;
							
						case ArithSpec.Add:
							op.Add("op",(IntSExpr)50);
							break;
							
						case ArithSpec.Divide:
							op.Add("op",(IntSExpr)51);
							break;
							
						case ArithSpec.Multiply:
							op.Add("op",(IntSExpr)52);
							break;
					}
					
					if( expr.S1 is FieldSRef)
					{
						op.Add("R1",	(IntSExpr)((RegVRef)((FieldSRef)expr.S1).varref).reg);
						op.Add("S1",	new FieldIndexSExpr{field=((FieldSRef)expr.S1).fieldname,type=((RegVRef)((FieldSRef)expr.S1).varref).datatype});
					} else if( expr.S1 is IntSExpr || expr.S1 is AddrSExpr)
					{
						op.Add("R1",	(IntSExpr)13);
						op.Add("S1",	new FieldIndexSExpr{field="Imm1",type="opcode"});
						op.Add("Imm1",	expr.S1);
					}
					
					
					if( expr.S2 is FieldSRef)
					{
						op.Add("R2",	(IntSExpr)((RegVRef)((FieldSRef)expr.S2).varref).reg);
						op.Add("S2",	new FieldIndexSExpr{field=((FieldSRef)expr.S2).fieldname,type=((RegVRef)((FieldSRef)expr.S2).varref).datatype});
					} else if( expr.S2 is IntSExpr || expr.S2 is AddrSExpr)
					{
						op.Add("R2",	(IntSExpr)13);
						op.Add("S2",	new FieldIndexSExpr{field="Imm2",type="opcode"});
						op.Add("Imm2",	expr.S2);
					}
					
				}
				
			}
			
			return op;
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
		
		public Branch Flatten(int truejump, int falsejump)
		{
			return  new Branch{
				S1 = this.S1.FlattenExpressions(),
				S2 = this.S2.FlattenExpressions(),
				
				rjmpeq = this.Op.GetValueOrDefault().HasFlag(CompSpec.Equal)  ?truejump:falsejump,
				rjmpgt = this.Op.GetValueOrDefault().HasFlag(CompSpec.Greater)?truejump:falsejump,
				rjmplt = this.Op.GetValueOrDefault().HasFlag(CompSpec.Less)   ?truejump:falsejump,
			};
		}
		
		public Block Flatten()
		{
			Block b = new Block();
			S1 = S1.FlattenExpressions();
			S2 = S2.FlattenExpressions();
			b.Add(this);			
			return b;
		}
		
		public Table Opcode()
		{
			var op = new Table{datatype="opcode"};
			op.Add("op",	(IntSExpr)71		);
			
			if( S1 is FieldSRef)
			{
				op.Add("R1",	(IntSExpr)((RegVRef)((FieldSRef)S1).varref).reg);
				op.Add("S1",	new FieldIndexSExpr{field=((FieldSRef)S1).fieldname,type=((RegVRef)((FieldSRef)S1).varref).datatype});
			} else if( S1 is IntSExpr || S1 is AddrSExpr)
			{
				op.Add("R1",	(IntSExpr)13);
				op.Add("S1",	new FieldIndexSExpr{field="Imm1",type="opcode"});
				op.Add("Imm1",	S1);
			}
			
			if( S2 is FieldSRef)
			{
				op.Add("R2",	(IntSExpr)((RegVRef)((FieldSRef)S2).varref).reg);
				op.Add("S2",	new FieldIndexSExpr{field=((FieldSRef)S2).fieldname,type=((RegVRef)((FieldSRef)S2).varref).datatype});
			} else if( S2 is IntSExpr || S2 is AddrSExpr)
			{
				op.Add("R2",	(IntSExpr)13);
				op.Add("S2",	new FieldIndexSExpr{field="Imm2",type="opcode"});
				op.Add("Imm2",	S2);
			}
			
			
			op.Add("addr1",	(IntSExpr)this.rjmpeq);
			op.Add("addr2",	(IntSExpr)this.rjmplt);
			op.Add("addr3",	(IntSExpr)this.rjmpgt);
			
			return op;
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
			//TODO: flatten the branch and link to the two blocks
			return b;
			
		}
		
		public Table Opcode()
		{
			throw new InvalidOperationException();
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
				b.Add(branch.Flatten(1,flatbody.Count+2));
				b.AddRange(flatbody);
				b.Add(new Jump{target=(IntSExpr)(-(flatbody.Count+1)),relative=true});
				
			}
			
			return b;
			
		}
		
		public Table Opcode()
		{
			throw new InvalidOperationException();
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
			target = target.FlattenExpressions();
			callsite = (SRef)callsite.FlattenExpressions();
			b.Add(this);			
			return b;
		}

		public Table Opcode()
		{
			var op = new Table{datatype="opcode"};
			op.Add("op",	(IntSExpr)70);
			
			if(relative)
			{
				op.Add("signal-green",(IntSExpr)1);
			}
			
			if( target is FieldSRef)
			{
				op.Add("R1",	(IntSExpr)((RegVRef)((FieldSRef)target).varref).reg);
				op.Add("S1",	new FieldIndexSExpr{field=((FieldSRef)target).fieldname,type=((RegVRef)((FieldSRef)target).varref).datatype});
			} else if( target is IntSExpr || target is AddrSExpr)
			{
				op.Add("R1",	(IntSExpr)13);
				op.Add("S1",	new FieldIndexSExpr{field="Imm1",type="opcode"});
				op.Add("Imm1",	target);
			}
			
			if(callsite != null)
			{
				if (callsite is FieldSRef)
				{
					op.Add("Rd",	(IntSExpr)((RegVRef)((FieldSRef)callsite).varref).reg);
					op.Add("Sd",	new FieldIndexSExpr{field=((FieldSRef)callsite).fieldname,type=((RegVRef)((FieldSRef)callsite).varref).datatype});
					op.Add("acc",(IntSExpr)1);
				}
			}
			
			
			
			return op;
		}
	}
	
	public class Push:Statement
	{
		public int stack;
		public RegVRef reg;
		
		public override string ToString()
		{
			return string.Format("[Push {0} {1}]", stack, reg);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
				
		public Block Flatten()
		{
			Block b = new Block();
			b.Add(this);			
			return b;
		}
		
		public Table Opcode()
		{
			var op = new Table{datatype="opcode"};
			op.Add("op",	(IntSExpr)83		);
			op.Add("index",	(IntSExpr)stack		);
			op.Add("R2",	(IntSExpr)reg.reg	);
			return op;
		}
	}
	
	public class Pop:Statement
	{
		public int stack;
		public RegVRef reg;
		
		public override string ToString()
		{
			return string.Format("[Pop {0} {1}]", stack, reg);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
				
		public Block Flatten()
		{
			Block b = new Block();
			b.Add(this);			
			return b;
		}
		
		public Table Opcode()
		{
			var op = new Table{datatype="opcode"};
			op.Add("op",	(IntSExpr)82		);
			op.Add("index",	(IntSExpr)stack		);
			op.Add("Rd",	(IntSExpr)reg.reg	);
			return op;
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
			args.CollapseConstants();
			
			var r8 = new RegVRef{reg=8};
			
			//int args or null in r8
			if(args.ints.Count > 0)
			{
				for (int i = 0; i < args.ints.Count; i++) {
					
					b.Add(new SAssign{
					      	append= i!=0,
					      	source=args.ints[i],
					      	target=new FieldSRef{varref=r8, fieldname="signal-"+(i+1)}
					      });
				}
			} else {
				b.Add(new VAssign{					      	
			      	source=new RegVRef{reg=0},
			      	target=r8,
			      });
			}
			
			//table arg or null in r7
			b.Add(new VAssign{			
		      	source=args.var??new RegVRef{reg=0},
		      	target=new RegVRef{reg=7},
		      });
		
			//jump to function, with return in r8.0
			b.Add(new Jump{
			      	target = new AddrSExpr{symbol=name},
			      	callsite = new FieldSRef{varref=r8, fieldname= "signal-0"}
			      });
			
			//capture returned values
		
			if(returns!=null)
			{
				if(returns.ints.Count > 0)
				{
					for (int i = 1; i < returns.ints.Count; i++) {
						
						b.Add(new SAssign{
						      	append= i!=1,
						      	source=new FieldSRef{varref=r8, fieldname="signal-"+i},
						      	target=returns.ints[i],
						      });
					}	
				}
				
				if(returns.var != null)
				{
					b.Add(new VAssign{			
				      	source=args.var??new RegVRef{reg=0},
				      	target=new RegVRef{reg=7},
				      });
				}
			}
			
			return b;
		}

		public Table Opcode()
		{
			return new Table();
		}
	}
	
	public class Return:Statement
	{
		public ExprList returns;
		public override string ToString()
		{
			return string.Format("[Return {0}]", returns);
		}	
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		
		public Block Flatten()
		{
			Block b = new Block();
			var r8 = new RegVRef{reg=8};
			
			returns.CollapseConstants();
			
			if(returns.ints.Count > 0)
			{
				for (int i = 0; i < returns.ints.Count; i++) {
					
					b.Add(new SAssign{
					      	append= i!=0,
					      	source=returns.ints[i],
					      	target=new FieldSRef{varref=r8, fieldname="signal-"+i}
					      });
				}
			}
			
			if(returns.var != null)
			{
					b.Add(new VAssign{					      	
					      	source=returns.var,
					      	target=new RegVRef{reg=7},
					      });
				
			}
			
			b.Add(new Jump{
			      	target = new AddrSExpr{symbol="__return"},
			      	relative=true,
			      });
			
			return b;
		}
		
		public Table Opcode()
		{
			return new Table();
		}
	}
	
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
		public void CollapseConstants()
		{
			ints = ints.Select(se => se.FlattenExpressions()).ToList();
			if(var!=null) var = var.FlattenExpressions();
		}
	}
	
	public class RefList{
		public List<SRef> ints = new List<SRef>();
		public VRef var;
		public override string ToString()
		{
			return string.Format("[RefList Ints={0} || Vars={1}]", string.Join(",",ints), var);
		}
	}
	
}