using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler
{
	public class IntVarSRef : SRef
	{
		public readonly string name;

		public override bool Equals(object obj)
		{
			if(obj is IntVarSRef)
			{
				var ivs = obj as IntVarSRef;
				return this.name == ivs.name;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return name.GetHashCode();
		}

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
			var varsym = Program.CurrentFunction?.locals.Find(s => s.name == name && s.datatype == "int");
			if (varsym.HasValue)
			{
				if(varsym.Value.type == SymbolType.Register)
				{
					return FieldSRef.LocalInt(varsym.Value.fixedAddr??-1);
				}
				else if(varsym.Value.type == SymbolType.Parameter)
				{
					return FieldSRef.IntArg(varsym.Value.fixedAddr ?? -1);
				}
				else
				{
					throw new InvalidOperationException();
				}
				
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
