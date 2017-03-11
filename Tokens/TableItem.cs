using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class TableItem
	{
		public readonly string fieldname;
		public readonly SExpr value;
		public override string ToString()
		{
			return string.Format("[{0}={1}]", fieldname, value);
		}
		public TableItem(string fieldname, SExpr value)
		{
			this.fieldname = fieldname;
			this.value = value;
		}
		
		public TableItem(char c, SExpr value)
		{			
			this.fieldname = Program.CurrentProgram.charmap[c];
			this.value = value;
		}

	}

}