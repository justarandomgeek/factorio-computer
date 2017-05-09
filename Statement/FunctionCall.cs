using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
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

		//TODO: look up function and report it's actual datatype?
		public string datatype { get { return "var"; } }

		public List<Instruction> FetchToField(FieldSRef dest)
		{
			var code = CodeGen();
			code.AddRange(new SAssign { source = FieldSRef.SReturn, target = dest }.CodeGen());
			return code;
		}

		public List<Instruction> FetchToReg(RegVRef dest)
		{
			if (Program.CurrentProgram.VBuiltins.ContainsKey(this.name))
			{
				return Program.CurrentProgram.VBuiltins[this.name](this, dest);
			}
			else
			{
				var code = CodeGen();
				code.AddRange(new VAssign { source = RegVRef.rVReturn(), target = dest }.CodeGen());
				return code;
			}
		}

		public List<Instruction> CodeGen()
		{
			if (Program.CurrentProgram.VBuiltins.ContainsKey(this.name))
			{
				return Program.CurrentProgram.VBuiltins[this.name](this, RegVRef.rNull);
			}


			var b = new List<Instruction>();

			var locals = (Program.CurrentFunction ?? Program.CurrentProgram?.InFunction)?.locals;

			// push local regs
			foreach (var sym in locals.Where(s => s.type == SymbolType.Register && s.datatype != "int" && !(new RegVRef(s.fixedAddr ?? -1).CalleeSaved)).OrderBy(s => s.fixedAddr))
			{
				b.Add(new Push(new RegVRef(sym.fixedAddr.Value)));
			}

			// push local args
			foreach (var sym in locals.Where(s => s.type == SymbolType.Parameter && s.datatype != "int").OrderBy(s => s.fixedAddr))
			{
				b.Add(new Push(RegVRef.rArg(sym.fixedAddr.Value)));
			}

			// push local intargs
			// have to always push because contains callsite
			b.Add(new Push(RegVRef.rIntArgs));
			
			// prepare table args
			for (int i = 0; i < args.vars.Count; i++)
			{
				if (i < 4)
				{
					b.AddRange(args.vars[i].FetchToReg(RegVRef.rArg(i + 1)));
				}
				else
				{
					throw new NotImplementedException();
				}
			}

			// prepare int args
			// allways clear for callsite
			b.AddRange(new VAssign { source = RegVRef.rNull, target = RegVRef.rIntArgs }.CodeGen());
			if (args.ints.Count > 0)
			{
				for (int i = 0; i < args.ints.Count; i++)
				{
					b.AddRange(args.ints[i].FetchToField(FieldSRef.IntArg(i + 1).AsPreCleared()));
				}
			}
				
			//TODO: LongCall change pointers here
								
			// **JUMP**
			b.Add(new Jump
			{
				target = new AddrSExpr(name),
				callsite = FieldSRef.CallSite,
				frame = PointerIndex.ProgConst,
			});

			// pop local intargs
			b.Add(new Pop(RegVRef.rIntArgs));
			
			// pop local args
			foreach (var sym in locals.Where(s => s.type == SymbolType.Parameter && s.datatype != "int").OrderBy(s => s.fixedAddr))
			{
				b.Add(new Pop(RegVRef.rArg(sym.fixedAddr.Value)));
			}

			// pop local regs
			foreach (var sym in locals.Where(s => s.type == SymbolType.Register && s.datatype != "int" && !(new RegVRef(s.fixedAddr ?? -1).CalleeSaved)).OrderByDescending(s => s.fixedAddr))
			{
				b.Add(new Pop(new RegVRef(sym.fixedAddr.Value)));
			}
			
			// return values are in rFetch1/2 and rScratchInts
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