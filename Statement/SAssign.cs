using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

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
				
		
		public List<Instruction> CodeGen()
		{
			var code = new List<Instruction>();
			if( source.IsConstant())
			{
				code.AddRange(target.PutFromInt(source.Evaluate()));
			}
			else
			{
				bool usedscratch=false;
				var tgt = target as FieldSRef;
				if (tgt == null)
				{
					if (target is IntVarSRef)
					{
						tgt = ((IntVarSRef)target).BaseField();
					}
					else
					{
						tgt = FieldSRef.ScratchInt();
						usedscratch = true;
					}
				}

				
				code.AddRange(source.FetchToField(tgt));

				if (usedscratch)
				{
					code.AddRange(target.PutFromField(tgt));
				}
			}			
			
			return code;
		}
	}

}