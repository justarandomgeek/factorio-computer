using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class FunctionCall:Statement, VExpr, SExpr
	{
        public string name;
		public ExprList args;
		public override string ToString()
		{
			return string.Format("[FunctionCall {0}({1})]", name, args);
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
				
		public bool IsConstant() { return false; }
		int SExpr.Evaluate() { throw new InvalidOperationException(); }

		public List<Instruction> FetchToField(FieldSRef dest)
		{
			var code = CodeGen();
			code.AddRange(new SAssign { source = FieldSRef.SReturn, target = dest }.CodeGen());
			return code;
		}

		public List<Instruction> FetchToReg(RegVRef dest)
		{
			var code = CodeGen();
			code.AddRange(new VAssign { source = RegVRef.rVarArgs, target = dest }.CodeGen());
			return code;
		}

		public List<Instruction> CodeGen()
		{
			var b = new List<Instruction>();
			
			//int args or null in r8
			if (args.ints.Count > 0)
			{
				for (int i = 0; i < args.ints.Count; i++)
				{
					b.AddRange(new SAssign
					{
						source = new ArithSExpr(args.ints[i], ArithSpec.Add, IntSExpr.Zero),
						target = FieldSRef.VarField(RegVRef.rScratchInts, "signal-" + (i + 1)),
					}.CodeGen());
				}
			}
			else
			{
				b.AddRange(new VAssign
				{
					source = RegVRef.rNull,
					target = RegVRef.rScratchInts,
				}.CodeGen());
			}

			//table arg or null in r7
			b.AddRange(new VAssign
			{
				source = args.var ?? RegVRef.rNull,
				target = RegVRef.rVarArgs,
			}.CodeGen());

			//jump to function, with return in r8.0
			b.Add(new Jump
			{
				target = new AddrSExpr(name),
				callsite = FieldSRef.CallSite,
				frame = PointerIndex.ProgConst,
			});

			// return values are in r7/r8 for use by following code

			//capture returned values
			//if (sreturn != null) b.AddRange(new SAssign { source = FieldSRef.SReturn, target = sreturn, }.CodeGen());
			//if (vreturn != null) b.AddRange(new VAssign { source = RegVRef.rVarArgs, target = (VRef)vreturn ?? RegVRef.rNull, }.CodeGen());

			return b;
		}

		public RegVRef AsReg()
		{
			return null;
		}

		public PointerIndex frame()
		{
			return PointerIndex.None;
		}

		public FieldSRef AsDirectField()
		{
			return null;
		}
	}

}