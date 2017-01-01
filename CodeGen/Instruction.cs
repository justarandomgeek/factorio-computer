using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler
{

	public enum Opcode
	{
		Halt = 0,

		EveryEqS2V,
		EveryLtS2V,
		EveryGtS2V,
		EveryEqS2V1,
		EveryGtS2V1,
		EveryLtS2V1,

		AnyEqS2V,
		AnyLtS2V,
		AnyGtS2V,
		AnyEqS2V1,
		AnyGtS2V1,
		AnyLtS2V1,

		S1EqS2V,
		S1LtS2V,
		S1GtS2V,
		S1EqS2V1,
		S1GtS2V1,
		S1LtS2V1,

		EachEqS2V,
		EachLtS2V,
		EachGtS2V,
		EachEqS2V1,
		EachGtS2V1,
		EachLtS2V1,

		EveryEqS2S,
		EveryLtS2S,
		EveryGtS2S,
		EveryEqS2S1,
		EveryGtS2S1,
		EveryLtS2S1,

		AnyEqS2S,
		AnyLtS2S,
		AnyGtS2S,
		AnyEqS2S1,
		AnyGtS2S1,
		AnyLtS2S1,

		S1EqS2S,
		S1LtS2S,
		S1GtS2S,
		S1EqS2S1,
		S1GtS2S1,
		S1LtS2S1,

		EachEqS2S,
		EachLtS2S,
		EachGtS2S,
		EachEqS2S1,
		EachGtS2S1,
		EachLtS2S1,

		EachSubV,
		EachAddV,
		EachDivV,
		EachMulV,

		EachSubS,
		EachAddS,
		EachDivS,
		EachMulS,

		Sub,
		Add,
		Div,
		Mul,

		VMul,
		VDiv,

		SArrPick,
		SArrWrite,

		SShiftUp,
		SShiftDn,

		VReplace,

		Jump = 70,
		Branch,
		Exec,

		Wire = 80,
		MemWrite,
		MemRead,
		Push,
		Pop,
		Append,

		PlayerInfo = 100,

	}

	public struct Instruction
	{
		public Opcode opcode;
		public bool acc;
		public bool relative;
		public PointerIndex idx;
		public FieldSRef op1;
		public FieldSRef op2;
		public FieldSRef dest;
		public SExpr imm1;
		public SExpr imm2;
		public int? rjmpeq;
		public int? rjmpgt;
		public int? rjmplt;

		//TODO: methods for most instruction types, maybe make new private?
		public static Instruction MemRead(Symbol sym, int offset, RegVRef dest)
		{
			return new Instruction
			{

			};
		}

		public static implicit operator Table(Instruction inst)
		{
			//TODO: generate a table of type opcode
			return new Table();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[{0}] ", opcode);
			if (idx != PointerIndex.None)
				sb.AppendFormat("[{0}] ", idx);

			if (op1 != null) sb.Append(op1.fieldname == null ? op1.varref.ToString() : op1.ToString());

			sb.Append(":");

			if (op2 != null) sb.Append(op2.fieldname == null ? op2.varref.ToString() : op2.ToString());

			sb.Append("=>");

			if (dest != null) sb.Append(dest.fieldname == null ? dest.varref.ToString() : dest.ToString());

			if (acc) sb.Append("+");

			//TODO: imm1/2, rjmps

			return sb.ToString();

		}
	}

}
