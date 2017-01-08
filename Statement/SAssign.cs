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
		public SExpr source;
		public override string ToString()
		{
			return string.Format("[SAssign {0} {1} {2}]", target, "=", source);
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
				var src = source.AsDirectField();
				
				if(src == null)
				{
					var tgt = target.AsDirectField();
					if (tgt == null)
					{
						var scratch = FieldSRef.ScratchInt();
						code.AddRange(source.FetchToField(scratch));
						code.AddRange(target.PutFromField(scratch));
					}
					else
					{
						code.AddRange(source.FetchToField(tgt));
					}
					
				}
				else
				{
					code.AddRange(target.PutFromField(src));
				}

				
			}			
			
			return code;
		}
	}

}