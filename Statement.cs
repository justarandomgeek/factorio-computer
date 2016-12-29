
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;


namespace compiler
{

	public enum Opcode
	{
		Halt = 0,

		EveryEqS2V,
		EveryLtS2V,
		EveryGtS2V,
		EveryEqS2V1,
		EveryGtS2V1,
		EveryLtS2V1,

		AnyEqS2V,
		AnyLtS2V,
		AnyGtS2V,
		AnyEqS2V1,
		AnyGtS2V1,
		AnyLtS2V1,

		S1EqS2V,
		S1LtS2V,
		S1GtS2V,
		S1EqS2V1,
		S1GtS2V1,
		S1LtS2V1,

		EachEqS2V,
		EachLtS2V,
		EachGtS2V,
		EachEqS2V1,
		EachGtS2V1,
		EachLtS2V1,

		EveryEqS2S,
		EveryLtS2S,
		EveryGtS2S,
		EveryEqS2S1,
		EveryGtS2S1,
		EveryLtS2S1,

		AnyEqS2S,
		AnyLtS2S,
		AnyGtS2S,
		AnyEqS2S1,
		AnyGtS2S1,
		AnyLtS2S1,

		S1EqS2S,
		S1LtS2S,
		S1GtS2S,
		S1EqS2S1,
		S1GtS2S1,
		S1LtS2S1,

		EachEqS2S,
		EachLtS2S,
		EachGtS2S,
		EachEqS2S1,
		EachGtS2S1,
		EachLtS2S1,

		EachSubV,
		EachAddV,
		EachDivV,
		EachMulV,

		EachSubS,
		EachAddS,
		EachDivS,
		EachMulS,

		Sub,
		Add,
		Div,
		Mul,

		VMul,
		VDiv,

		SArrPick,
		SArrWrite,

		SShiftUp,
		SShiftDn,

		VReplace,

		Jump = 70,
		Branch,
		Exec,

		Wire = 80,
		MemWrite,
		MemRead,
		Push,
		Pop,
		Append,

		PlayerInfo = 100,

	}

	public struct Instruction
	{
		public Opcode opcode;
		public bool acc;
		public bool relative;
		public PointerIndex idx;
		public FieldSRef op1;
		public FieldSRef op2;
		public FieldSRef dest;
		public SExpr imm1;
		public SExpr imm2;
		public int? rjmpeq;
		public int? rjmpgt;
		public int? rjmplt;

		
		public static implicit operator Table(Instruction inst)
		{
			//TODO: generate a table of type opcode
			return new Table();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[{0}] ", opcode);
			if(idx != PointerIndex.None)
				sb.AppendFormat("[{0}] ", idx);

			if (op1 != null) sb.Append(op1.fieldname != null ? op1.varref.ToString() : op1.ToString());

			sb.Append(":");

			if (op2 != null) sb.Append(op2.fieldname != null? op2.varref.ToString():op2.ToString());

			sb.Append("=>");

			if (dest != null) sb.Append(op2.fieldname != null ? dest.varref.ToString() : dest.ToString());

			if (acc) sb.Append("+");

			//TODO: imm1/2, rjmps

			return sb.ToString();

		}
	}


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
			target = (VRef)target.FlattenExpressions(); //VRefs should always flatten to VRefs, specifically RegVRef or MemVRef

			if (source is Table)
			{
				//Convert literal tables to constants and address them by symbol name
				var constname = "__const"+source.GetHashCode();
				Program.CurrentProgram.Symbols.Add(new Symbol{
				                                   	type=SymbolType.Constant,
				                                   	name=constname,
													frame=PointerIndex.ProgConst,
				                                   	datatype=((Table)source).datatype,
				                                   	data=new List<Table>{(Table)source},
				                                   });
				source = new MemVRef(new AddrSExpr{symbol=constname},((Table)source).datatype);
			} else if (source is FunctionCall)
			{
				var func = source as FunctionCall;
				func.vreturn = target ;
				return func.Flatten();
			}
			
			b.Add(this);			
			return b;
		}
		
		public Table Opcode()
		{
			var op = new Table{datatype="opcode"};
			if(append) op.Add("acc",IntSExpr.One);
			
			if(target is MemVRef)
			{
				if(source is RegVRef)
				{
					//mem write
					op.Add("op", 81);
                    op.Add("R2", ((RegVRef)source).reg);
                    var S1 = ((MemVRef)target).addr;
					if( S1 is FieldSRef)
					{
						op.Add("R1",	((RegVRef)((FieldSRef)S1).varref).reg);
						op.Add("S1",	new FieldIndexSExpr{field=((FieldSRef)S1).fieldname,type=((RegVRef)((FieldSRef)S1).varref).datatype});
					} else if( S1 is IntSExpr || S1 is AddrSExpr)
					{
						op.Add("R1",	new IntSExpr(13));
						op.Add("S1",	new FieldIndexSExpr{field="Imm1",type="opcode"});
						op.Add("Imm1",	S1);
					}
				}
			}else if(target is RegVRef)
			{
                op.Add("Rd", ((RegVRef)target).reg);

				if (source is RegVRef)
				{
					// reg copy
					op.Add("op", 50); // V+s=>V
					op.Add("R1", ((RegVRef)source).reg);
				}
				else if (source is MemVRef)
				{
					//mem read
					op.Add("op", 82);
					var S1 = ((MemVRef)source).addr;
					if (S1 is FieldSRef)
					{
						op.Add("R1", ((RegVRef)((FieldSRef)S1).varref).reg);
						op.Add("S1", new FieldIndexSExpr { field = ((FieldSRef)S1).fieldname, type = ((RegVRef)((FieldSRef)S1).varref).datatype });
					}
					else if (S1 is IntSExpr || S1 is AddrSExpr)
					{
						op.Add("R1", 13);
						op.Add("S1", new FieldIndexSExpr { field = "Imm1", type = "opcode" });
						op.Add("Imm1", S1);
						if (S1 is AddrSExpr) op.Add("index", (int)((AddrSExpr)S1).frame);
					}

				}
				else if (source is ArithVExpr)
				{
					var expr = (ArithVExpr)source;
					// must be reg v reg
					if (expr.V1 is RegVRef && expr.V2 is RegVRef)
					{
						// v arith v => v
						switch (expr.Op)
						{
							case ArithSpec.Subtract:
								// remap as vd  = v1 + 0
								// remap as vd += v2.Each * -1

								break;

							case ArithSpec.Add:
								// remap as vd  = v1 + 0
								// remap as vd += v2 + 0

								break;

							case ArithSpec.Multiply:
								//VMUL instruction 61
								op.Add("op", 61);
								op.Add("R1", ((RegVRef)expr.V1).reg);
								op.Add("R2", ((RegVRef)expr.V2).reg);
								break;

							case ArithSpec.Divide:
								//VDIV instruction 62
								op.Add("op", 62);
								op.Add("R1", ((RegVRef)expr.V1).reg);
								op.Add("R2", ((RegVRef)expr.V2).reg);
								break;
						}


					}
				}
				else if (source is ArithVSExpr)
				{
					// must be reg v reg.sig
					var expr = (ArithVSExpr)source;

					if (expr.V1 is RegVRef)
					{
						// v.each arith s => v, 49-52 -+/*
						switch (expr.Op)
						{
							case ArithSpec.Subtract:
								op.Add("op", 49);
								break;

							case ArithSpec.Add:
								op.Add("op", 50);
								break;

							case ArithSpec.Divide:
								op.Add("op", 51);
								break;

							case ArithSpec.Multiply:
								op.Add("op", 52);
								break;
						}

						if (expr.S2 is FieldSRef)
						{
							op.Add("R2", ((RegVRef)((FieldSRef)expr.S2).varref).reg);
							op.Add("S2", new FieldIndexSExpr { field = ((FieldSRef)expr.S2).fieldname, type = ((RegVRef)((FieldSRef)expr.S2).varref).datatype });
						}
						else if (expr.S2 is IntSExpr || expr.S2 is AddrSExpr)
						{
							op.Add("R2", 13);
							op.Add("S2", new FieldIndexSExpr { field = "Imm2", type = "opcode" });
							op.Add("Imm2", expr.S2);
						}
					}

				}
				else
				{
					throw new NotImplementedException("Assignment from unimplemented vector expression");
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
			target = (FieldSRef)target.FlattenExpressions(); //SRefs should always flatten to a FieldSRef over a RegVRef or MemVRef
            
            // fetch memref to scratch and replace with reg ref

            if (source is IntSExpr || source is AddrSExpr || source is FieldIndexSExpr || source is FieldSRef )
            {
                // convert to tgt+=src-tgt
                var arop = new ArithSExpr { S1 = source, Op = ArithSpec.Subtract, S2 = append ? (SExpr)IntSExpr.Zero : target };
                source = arop;
                append = true;
                b.Add(this);
            }
            else if (source is ArithSExpr)
            {
                int usedIntermediates = 0;
                // if args aren't single-instruction args, need to generate intermediates...


                if (!append)
                {
                    // scratch=op result
                    b.Add(new SAssign { append = usedIntermediates++ > 0, source = source, target = FieldSRef.ScratchInt });
                    // tgt+=scratch-tgt
                    var arop = new ArithSExpr
                    {
                        S1 = FieldSRef.ScratchInt,
                        Op = ArithSpec.Subtract,
                        S2 = append ? (SExpr)IntSExpr.Zero : target
                    };
                    source = arop;
                    append = true;
                }
                b.Add(this);
            }
            else
            {
                throw new NotImplementedException();
            }
            
            // restore reg to mem                        
			
			return b;
		}

		public Table Opcode()
		{
			var op = new Table{datatype="opcode"};
			if(append) op.Add("acc",IntSExpr.One);
			
			if(target is FieldSRef)
			{
				var sd = (FieldSRef)target;
				var rd = (RegVRef)sd.varref;
				op.Add("Sd",new FieldIndexSExpr{type=rd.datatype,field=sd.fieldname});
				op.Add("Rd",rd.reg);
				
				if(source is FieldSRef || source is IntSExpr || source is AddrSExpr)
				{
                    // These should all have flattened to arith's
                    throw new NotImplementedException();
					
				} else if(source is ArithSExpr)
				{
					var expr = (ArithSExpr)source;
					
					// s arith s => s, 57-60 -+/*
					switch (expr.Op) {
						case ArithSpec.Subtract:
							op.Add("op",57);
							break;
							
						case ArithSpec.Add:
							op.Add("op",58);
							break;
							
						case ArithSpec.Divide:
							op.Add("op",59);
							break;
							
						case ArithSpec.Multiply:
							op.Add("op",60);
							break;
					}
					
					if( expr.S1 is FieldSRef)
					{
						op.Add("R1",	((RegVRef)((FieldSRef)expr.S1).varref).reg);
						op.Add("S1",	new FieldIndexSExpr{field=((FieldSRef)expr.S1).fieldname,type=((RegVRef)((FieldSRef)expr.S1).varref).datatype});
					} else if( expr.S1 is IntSExpr || expr.S1 is AddrSExpr)
					{
						op.Add("R1",	13);
						op.Add("S1",	new FieldIndexSExpr{field="Imm1",type="opcode"});
						op.Add("Imm1",	expr.S1);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }


                    if ( expr.S2 is FieldSRef)
					{
						op.Add("R2",	((RegVRef)((FieldSRef)expr.S2).varref).reg);
						op.Add("S2",	new FieldIndexSExpr{field=((FieldSRef)expr.S2).fieldname,type=((RegVRef)((FieldSRef)expr.S2).varref).datatype});
					} else if( expr.S2 is IntSExpr || expr.S2 is AddrSExpr)
					{
						op.Add("R2",	13);
						op.Add("S2",	new FieldIndexSExpr{field="Imm2",type="opcode"});
						op.Add("Imm2",	expr.S2);
                    }
                    else
                    {
                        throw new NotImplementedException();
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
			op.Add("op", 71);
			
			if( S1 is FieldSRef)
			{
				op.Add("R1",	((RegVRef)((FieldSRef)S1).varref).reg);
				op.Add("S1",	new FieldIndexSExpr{field=((FieldSRef)S1).fieldname,type=((RegVRef)((FieldSRef)S1).varref).datatype});
			} else if( S1 is IntSExpr || S1 is AddrSExpr)
			{
				op.Add("R1",	13);
				op.Add("S1",	new FieldIndexSExpr{field="Imm1",type="opcode"});
				op.Add("Imm1",	S1);
			}
			
			if( S2 is FieldSRef)
			{
				op.Add("R2",	((RegVRef)((FieldSRef)S2).varref).reg);
				op.Add("S2",	new FieldIndexSExpr{field=((FieldSRef)S2).fieldname,type=((RegVRef)((FieldSRef)S2).varref).datatype});
			} else if( S2 is IntSExpr || S2 is AddrSExpr)
			{
				op.Add("R2",	13);
				op.Add("S2",	new FieldIndexSExpr{field="Imm2",type="opcode"});
				op.Add("Imm2",	S2);
			}


			op.Add("addr1", this.rjmpeq ?? 0);
			op.Add("addr2",	this.rjmplt ?? 0);
			op.Add("addr3",	this.rjmpgt ?? 0);
			
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

			flatif.Add(new Jump { relative = true, target = new IntSExpr(flatelse.Count) });
			b.Add(branch.Flatten(1, flatif.Count + 1));
			b.AddRange(flatif);
			b.AddRange(flatelse);
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
				b.Add(new Jump{target=new IntSExpr(-(flatbody.Count+1)),relative=true});
				
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
		public PointerIndex? frame;

		public override string ToString()
		{
			return string.Format("[Jump {0} Callsite={1}, Relative={2}, Setint={3}, Frame={4}]", target, callsite, relative, setint, frame);
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
			op.Add("op",	70);
			
			if(relative)
			{
				op.Add("signal-green",IntSExpr.One);
			}
			
			if(frame.HasValue)
			{
				op.Add("index", (int)frame);
			}
			
			if( target is FieldSRef)
			{
				op.Add("R1",	((RegVRef)((FieldSRef)target).varref).reg);
				op.Add("S1",	new FieldIndexSExpr{field=((FieldSRef)target).fieldname,type=((RegVRef)((FieldSRef)target).varref).datatype});
			} else if( target is IntSExpr || target is AddrSExpr)
			{
				op.Add("R1",	13);
				op.Add("S1",	new FieldIndexSExpr{field="Imm1",type="opcode"});
				op.Add("Imm1",	target);
			}
			
			if(callsite != null)
			{
				if (callsite is FieldSRef)
				{
					op.Add("Rd",	((RegVRef)((FieldSRef)callsite).varref).reg);
					op.Add("Sd",	new FieldIndexSExpr{field=((FieldSRef)callsite).fieldname,type=((RegVRef)((FieldSRef)callsite).varref).datatype});
					op.Add("acc",   1);
				}
			}
			
			
			
			return op;
		}
	}
	
    public enum PointerIndex
    {
        None=0,
        CallStack=1,
        ProgConst=2,
        ProgData=3,
        LocalData=4,
    }

	public class Exchange : Statement
	{
		public PointerIndex frame;
		public SExpr addr;
		public RegVRef source;
		public RegVRef dest;


		public Exchange(RegVRef reg, PointerIndex frame = PointerIndex.CallStack): this(reg, frame, IntSExpr.Zero)
		{
			
		}

		public Exchange(RegVRef reg, PointerIndex frame, SExpr addr): this(reg,reg,frame,addr)
		{
			
		}

		public Exchange(RegVRef source, RegVRef dest, PointerIndex frame, SExpr addr)
		{
			this.source = source;
			this.dest = dest;
			this.frame = frame;
			this.addr = addr;
		}

		public override string ToString()
		{
			return string.Format("[Exchange {0}+{1} {2} => {3}]", frame, addr, source, dest);
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
			var op = new Table { datatype = "opcode" };
			//mem write
			op.Add("op", 81);
			op.Add("index", (int)frame);
			op.Add("R2", source.reg);
			op.Add("Rd", dest.reg);

			var S1 = addr;
			if (S1 is FieldSRef)
			{
				op.Add("R1", ((RegVRef)((FieldSRef)S1).varref).reg);
				op.Add("S1", new FieldIndexSExpr { field = ((FieldSRef)S1).fieldname, type = ((RegVRef)((FieldSRef)S1).varref).datatype });
				
			}
			else if (S1 is IntSExpr || S1 is AddrSExpr)
			{
				op.Add("R1", 13);
				op.Add("S1", new FieldIndexSExpr { field = "Imm1", type = "opcode" });
				op.Add("Imm1", S1);
			}
			
			
			return op;
		}
	}
	public class Push:Statement
	{
		public readonly PointerIndex stack;
		public readonly RegVRef reg;

		public Push(RegVRef reg, PointerIndex stack = PointerIndex.CallStack)
		{
			this.reg = reg;
			this.stack = stack;
		}
		
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
			op.Add("op",	83		);
			op.Add("index",	(int)stack		);
			op.Add("R2",	reg.reg	);
			return op;
		}
	}
	public class Pop:Statement
	{
		public readonly PointerIndex stack;
		public readonly RegVRef reg;

		public Pop(RegVRef reg, PointerIndex stack = PointerIndex.CallStack)
		{
			this.reg = reg;
			this.stack = stack;
		}

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
			op.Add("op",	84);
			op.Add("index", (int)stack);
			op.Add("Rd",	reg.reg	);
			return op;
		}
	}

	public class FunctionCall:Statement, VExpr, SExpr
	{
        public string name;
		public ExprList args;
		public SRef sreturn;
		public VRef vreturn;
		public override string ToString()
		{
			return string.Format("[FunctionCall {0}({1}) => {2}|{3}]", name, args, sreturn, vreturn);
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
				
		public Block Flatten()
		{
			if (Program.CurrentProgram.SBuiltins.ContainsKey(name))
			{
				return Program.CurrentProgram.SBuiltins[name](this);
			}
			else if (Program.CurrentProgram.VBuiltins.ContainsKey(name))
			{
				return Program.CurrentProgram.VBuiltins[name](this);
			}

			Block b = new Block();
			args.CollapseConstants();
			
			//int args or null in r8
			if(args.ints.Count > 0)
			{
				for (int i = 0; i < args.ints.Count; i++) {
					
					b.Add(new SAssign{
					      	append= i!=0,
					      	source= new ArithSExpr{
                                  S1 = args.ints[i],
                                  Op = ArithSpec.Add,
                                  S2 = IntSExpr.Zero
                              },
					      	target=FieldSRef.VarField(RegVRef.rScratch,"signal-" + (i + 1)),
					      });
				}
			} else {
				b.Add(new VAssign{					      	
			      	source= RegVRef.rNull,
			      	target= RegVRef.rScratch,
			      });
			}

			//table arg or null in r7
			b.Add(new VAssign
			{
				source = args.var?.FlattenExpressions() ?? RegVRef.rNull,
				target = RegVRef.rVarArgs,
			});
		
			//jump to function, with return in r8.0
			b.Add(new Jump{
			      	target = new AddrSExpr{symbol=name},
			      	callsite = FieldSRef.CallSite,
			      	frame=PointerIndex.ProgConst,
			      });

			//capture returned values
			if(sreturn != null)
			{
						
				b.AddRange(new SAssign{
						source=FieldSRef.SReturn,
						target=sreturn,
						}.Flatten());
			}
				
			if(vreturn != null)
			{
				b.Add(new VAssign{
                    source = RegVRef.rVarArgs,
                    target = (VRef)vreturn.FlattenExpressions()?? RegVRef.rNull,
				    });
			}
			
			return b;
		}

		public bool IsConstant() { return false; }
		VExpr VExpr.FlattenExpressions() { return this; }
		SExpr SExpr.FlattenExpressions() { return this; }
		Table VExpr.Evaluate() { throw new InvalidOperationException(); }
		int SExpr.Evaluate() { throw new InvalidOperationException(); }

		public Table Opcode()
		{
			throw new InvalidOperationException();
		}
	}
	
	public class Return:Statement
	{
		public readonly SExpr sreturn;
		public readonly VExpr vreturn;

		public Return(){}
		public Return(SExpr sret)
		{
			sreturn = sret;
		}
		public Return(VExpr vret)
		{
			vreturn = vret;
		}

		public override string ToString()
		{
			return string.Format("[Return {0}|{1}]", sreturn, vreturn);
		}	
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		
		public Block Flatten()
		{
			Block b = new Block();
			
			if(sreturn != null)
			{
				b.Add(new SAssign{
					append=false,
					source=sreturn.FlattenExpressions(),
					target=FieldSRef.SReturn,
					});
			}
			
			if(vreturn != null)
			{
					b.Add(new VAssign{					      	
					      	source=vreturn.FlattenExpressions(),
					      	target= RegVRef.rScratch2,
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
}