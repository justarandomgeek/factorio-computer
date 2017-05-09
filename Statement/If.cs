using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
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
			if(elseblock?.Count > 0)
			{
				Console.WriteLine(prefix+"Else");
				elseblock.Print(prefix+"  ");					
			}
		}
		
		public List<Instruction> CodeGen()
		{
			var b = new List<Instruction>();
			var flatif = ifblock.CodeGen();
			var flatelse = elseblock.CodeGen();

			if ( flatelse.Count > 0 ) flatif.Add(new Jump { relative = true, target = new IntSExpr(flatelse.Count) });

			b.AddRange(branch.CodeGen(1, flatif.Count + 1));
			b.AddRange(flatif);
			b.AddRange(flatelse);
			return b;
		}
	}

}