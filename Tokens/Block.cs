using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class Block:List<Statement>
	{
		public override string ToString()
		{
			return string.Format("[Block {0}]",string.Join(";",this));
		}
		
		public void Print(string prefix)
		{
			foreach (var statement in this) {
				if(statement != null) statement.Print(prefix);
			}
		}
		
		public Block()
		{
			
		}
		public Block(Statement s)
		{
			this.Add(s);
		}
		
		public List<Instruction> CodeGen()
		{
			var code = new List<Instruction>();
			foreach (var statement in this)
			{
				if (statement != null)
				{
					//TODO: maybe reset ints if needed?
					code.AddRange(statement.CodeGen());
				}
			}
			return code;
		}

		[Obsolete]
		public Block Flatten()
		{
			return new Block();
		}
		
		
		
	}

}