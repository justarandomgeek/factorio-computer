using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class FunctionInfo{
		public string name;
		public string returntype;
		public SymbolList locals = new SymbolList();
		public TypeInfo localints = new TypeInfo();
		public Block body;
		public int framesize;
				
		public void AllocateLocals()
		{
			
			foreach (var p in locals.Where(sym=>sym.type == SymbolType.Parameter && sym.datatype == "int")) {
				localints.Add(p.name,"signal-" + p.fixedAddr);
			}
			locals.RemoveAll(sym => localints.Keys.Contains(sym.name));
			var newlocals = new SymbolList();

			//TODO: fix this to actually confirm a register is available
			if (locals.Count(sym => sym.frame == PointerIndex.CallStack) < 4)
			{
				int nextReg = 6;
				newlocals.AddRange(locals.Select((symbol) =>
				{
					if (symbol.frame == PointerIndex.CallStack)
					{
						symbol.type = SymbolType.Register;
						symbol.fixedAddr = nextReg--;
						symbol.frame = 0;

					}
					return symbol;
				}));
			}
			else
			{
				newlocals.AddRange(locals.Select((symbol) =>
				{
					if (symbol.frame == PointerIndex.CallStack)
					{
						symbol.fixedAddr = framesize++;
					}
					return symbol;
				}));

			}

			
			locals = newlocals;
		}

		public List<Instruction> BuildFunction()
        {
            var b = new List<Instruction>();

			// save call site (in r8.signal-0)
			b.Add(new Push(RegVRef.rScratch));

			if (localints.Count > 0)
			{
				// save parent localints
				b.Add(new Push(new RegVRef(2)));
			}

            // push regs as needed
            foreach (var sym in locals.Where(s => s.type == SymbolType.Register))
            {
                if (sym.fixedAddr.HasValue) b.Add(new Push(new RegVRef(sym.fixedAddr.Value)));
            }

			// wind stack down for locals if needed
			if (framesize > 0)
			{
				b.Add(new Instruction
				{
					opcode = Opcode.Add,
					op1 = FieldSRef.Imm1(),
					imm1 = new IntSExpr(-framesize),
					acc = true,
					dest = FieldSRef.Pointer(PointerIndex.CallStack)
				});
			}

            // copy params if named
            //int args or null in r8
            var intparas = locals.Where(sym => sym.type == SymbolType.Parameter && sym.datatype == "int").ToList();
            if (intparas.Count() > 0)
            {
                for (int i = 0; i < intparas.Count(); i++)
                {
					b.Add(new Instruction
					{
						opcode = Opcode.Add,
						op1 = FieldSRef.IntArg(name, intparas[i].name),
						dest = FieldSRef.LocalInt(name, intparas[i].name),
						acc = i != 0,
					});
                }
            }

            // body
            b.AddRange(body.CodeGen());

            // convert rjmp __return => rjmp <integer> to here.
            for (int i = 0; i < b.Count; i++) {
				var inst = b[i];
				if ((inst.imm1 as AddrSExpr)?.symbol  == "__return")
                {
					inst.imm1 = new IntSExpr(b.Count - i);
                }
				if ((inst.imm2 as AddrSExpr)?.symbol == "__return")
				{
					inst.imm2 = new IntSExpr(b.Count - i);
				}
			}

			// wind stack back up for locals if needed
			if (framesize > 0)
			{
				b.Add(new Instruction
				{
					opcode = Opcode.Add,
					op1 = FieldSRef.Imm1(),
					imm1 = new IntSExpr(framesize),
					acc = true,
					dest = FieldSRef.Pointer(PointerIndex.CallStack)
				});
				
			}

			// restore registers
			foreach (var sym in locals.Where(s => s.type == SymbolType.Register).Reverse())
            {
                if (sym.fixedAddr.HasValue) b.Add(new Pop(new RegVRef(sym.fixedAddr.Value)));
            }

			if (localints.Count > 0)
			{
				// restore parent localints
				b.Add(new Pop(RegVRef.rLocalInts(name)));
			}

			// get return site
			b.Add(new Exchange(RegVRef.rScratch2));
			b.Add(new Instruction
			{
				opcode = Opcode.Sub,
				op1 = FieldSRef.VarField(RegVRef.rScratch2, "signal-0"),
				op2 = FieldSRef.CallSite,
				dest = FieldSRef.CallSite,
				acc = true
			});

			b.Add(new Pop(RegVRef.rScratch2));

			// jump to return site
			b.Add(new Jump{target = FieldSRef.CallSite});
			
			return b;
		}
	}

}