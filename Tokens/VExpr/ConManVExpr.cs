using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class ConManVExpr: VExpr
	{
		public bool IsConstant()
		{
			return Command.IsConstant() && Data.IsConstant();
		}
		
		public readonly VExpr Command;
		public readonly VExpr Data;	

		public ConManVExpr(VExpr Command, VExpr Data)
		{
			this.Command = Command;
			this.Data = Data;
		}

		public override string ToString()
		{
			return string.Format("[ConManVExpr {0} {1}]", Command, Data);
		}
		
		public static bool operator ==(ConManVExpr a1, ConManVExpr a2) { return a1.Equals(a2); }
		public static bool operator !=(ConManVExpr a1, ConManVExpr a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return Command.GetHashCode() ^ Data.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as ConManVExpr;
			if (other != null)
			{
				return other.Command == this.Command && other.Data == this.Data;
			}
			else
			{
				return false;
			}

		}

		public List<Instruction> FetchToReg(RegVRef dest)
		{
			var code = new List<Instruction>();
			var cmd = Command.AsReg();
			if(cmd == null)
			{
				cmd = RegVRef.rFetch(1);
				code.AddRange(Command.FetchToReg(cmd));
			}

			var data = Data.AsReg();
			if (data == null)
			{
				data = RegVRef.rFetch(2);
				code.AddRange(Data.FetchToReg(data));
			}

			code.Add(new Instruction
			{
				opcode = Opcode.ConMan,
				op1 = cmd,
				op2 = data,
				dest = dest,
			});

			return code;
		}

		public RegVRef AsReg()
		{
			return null;
		}
	}

}