using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

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
		
		public Table Opcode()
		{
			throw new InvalidOperationException();
		}

		public List<Instruction> CodeGen()
		{
			var b = new List<Instruction>();
			var flatif = ifblock.CodeGen();
			var flatelse = elseblock.CodeGen();

			flatif.Add(new Jump { relative = true, target = new IntSExpr(flatelse.Count) });
			b.AddRange(branch.CodeGen(1, flatif.Count + 1));
			b.AddRange(flatif);
			b.AddRange(flatelse);
			return b;
		}
	}

}