using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class ScammerVExpr : VExpr
	{
		public bool IsConstant()
		{
			return Command.IsConstant() && Data.IsConstant();
		}
		
		public readonly VExpr Command;
		public readonly VExpr Data;	

		public ScammerVExpr(VExpr Command, VExpr Data)
		{
			this.Command = Command;
			this.Data = Data;
		}

		public override string ToString()
		{
			return string.Format("[ScammerVExpr {0} {1}]", Command, Data);
		}
		
		public static bool operator ==(ScammerVExpr a1, ScammerVExpr a2) { return a1.Equals(a2); }
		public static bool operator !=(ScammerVExpr a1, ScammerVExpr a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return Command.GetHashCode() ^ Data.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as ScammerVExpr;
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
				cmd = RegVRef.rScratchTab;
				code.AddRange(Command.FetchToReg(cmd));
			}

			var data = Data.AsReg();
			if (data == null)
			{
				data = cmd == RegVRef.rScratchTab ? RegVRef.rScratchInts : RegVRef.rScratchTab;
				if (data == RegVRef.rScratchInts) FieldSRef.ResetScratchInts();
				code.AddRange(Data.FetchToReg(data));
			}

			code.Add(new Instruction
			{
				opcode = Opcode.Scammer,
				op1 = cmd,
				op2 = data,
				dest = dest,
			});

			if (data == RegVRef.rScratchInts) code.AddRange(data.PutFromReg(RegVRef.rNull));

			return code;
		}

		public RegVRef AsReg()
		{
			return null;
		}
	}

}