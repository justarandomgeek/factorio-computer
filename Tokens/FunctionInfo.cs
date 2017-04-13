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
		public Block body;
		public int framesize;
				
		public void AllocateLocals()
		{
			var newlocals = new SymbolList();
			
			newlocals.AddRange(locals.Where(s => s.type == SymbolType.Parameter && s.datatype == "int"));

			int i = 1;
			newlocals.AddRange(locals.Where(s => s.type == SymbolType.Register && s.datatype == "int").Select(s => { s.fixedAddr = i++; return s; }));
						
			newlocals.AddRange(locals.Where(s => s.type == SymbolType.Parameter && s.datatype != "int"));

			i = 1;
			newlocals.AddRange(locals
				.Where(s => s.type == SymbolType.Data||(s.type == SymbolType.Register && s.datatype != "int"))
				.Select(s => {
					if(i<=5)
					{ 
						return new Symbol { name = s.name, datatype = s.datatype, type = SymbolType.Register, fixedAddr = RegVRef.rTemp(i++).reg };
					}
					else if(i <= 10)
					{
						return new Symbol { name = s.name, datatype = s.datatype, type = SymbolType.Register, fixedAddr = RegVRef.rSTemp((i++)-5).reg };
					}
					else
					{
						return new Symbol { name = s.name, datatype = s.datatype, type = SymbolType.Data, frame = PointerIndex.LocalData, fixedAddr = (i++) - 10 };
					}

					throw new ArgumentException();

				})
				);
			
			locals = newlocals;
		}

		public List<Instruction> BuildFunction()
        {
            var b = new List<Instruction>();
			
			// save parent localints
			if (locals.Count(s => s.type == SymbolType.Register && s.datatype == "int") > 0) b.Add(new Push(RegVRef.rLocalInts));

			// push regs as needed
			foreach (var sym in locals.Where(s => s.type == SymbolType.Register && s.datatype != "int" && new RegVRef(s.fixedAddr ?? -1).CalleeSaved).OrderBy(s => s.fixedAddr))
			{
				b.Add(new Push(new RegVRef(sym.fixedAddr.Value)));
			}
			
			//TODO: allocate stackframe and set LocalData pointer

			FieldSRef.ResetScratchInts();
			b.AddRange(RegVRef.rScratchInts.PutFromReg(RegVRef.rNull));
			
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
				b[i] = inst;
			}

			// restore registers
			foreach (var sym in locals.Where(s => s.type == SymbolType.Register && s.datatype != "int" && new RegVRef(s.fixedAddr ?? -1).CalleeSaved).OrderByDescending(s => s.fixedAddr))
			{
				if (sym.fixedAddr.HasValue) b.Add(new Pop(new RegVRef(sym.fixedAddr.Value)));
			}

			// restore parent localints
			if (locals.Count(s => s.type == SymbolType.Register && s.datatype == "int") > 0) b.Add(new Pop(RegVRef.rLocalInts));
						
			// jump to return site
			b.Add(new Jump{target = FieldSRef.CallSite});
			
			return b;
		}
	}

}