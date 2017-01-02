using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler
{
	public class IntVarSRef : SRef
	{
		public readonly string name;

		public IntVarSRef(string name)
		{
			this.name = name;
		}

		public bool IsConstant()
		{
			return false;
		}
		public int Evaluate()
		{
			throw new InvalidOperationException();
		}
		public override string ToString()
		{
			return string.Format("[IntVarSRef {0}]", name);
		}

		public FieldSRef BaseField()
		{
			if (Program.CurrentFunction.localints.ContainsKey(name))
			{
				return FieldSRef.LocalInt(Program.CurrentFunction.name, name);
			}
			else
			{
				return FieldSRef.GlobalInt(name);
			}

		}
		
		public List<Instruction> PutFromField(FieldSRef src)
		{
			return this.BaseField().PutFromField(src);
		}

		public List<Instruction> PutFromInt(int value)
		{
			return this.BaseField().PutFromInt(value);
		}
		
		public List<Instruction> FetchToField(FieldSRef dest)
		{
			return this.BaseField().FetchToField(dest);
		}

		public PointerIndex frame()
		{
			return PointerIndex.None;
		}

		public FieldSRef AsDirectField()
		{
			return this.BaseField().AsDirectField();
		}
	}
}
