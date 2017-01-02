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
		public Opcode opcode { get; set; }
		public bool acc { get; set; }
		public bool relative { get; set; }
		public PointerIndex idx { get; set; }

		public FieldSRef op1 { get; set; }
		public FieldSRef op2 { get; set; }
		public FieldSRef dest { get; set; }

		public SExpr imm1 { get; set; }
		public SExpr imm2 { get; set; }
		public int? rjmpeq { get; set; }
		public int? rjmplt { get; set; }
		public int? rjmpgt { get; set; }

		public static implicit operator Table(Instruction inst)
		{
			//TODO: generate a table of type opcode
			var op = new Table();
			op.datatype = "opcode";
			op.Add("op", (int)inst.opcode);

			if (inst.idx != PointerIndex.None) op.Add("index", (int)inst.idx);

			Action<Table,FieldSRef,string,string> sigfromfield = (Table t, FieldSRef f, string reg, string sig) =>
			{
				if (f != null)
				{
					t.Add(reg, f.varref.AsReg().reg);
					if (f.fieldname != null) t.Add(sig, new FieldIndexSExpr(f.fieldname, f.varref.datatype));
				}
			};


			sigfromfield(op, inst.op1, "R1", "S1");
			sigfromfield(op, inst.op2, "R2", "S2");
			sigfromfield(op, inst.dest, "Rd", "Sd");
			
			if (inst.imm1 != null) op.Add("Imm1", inst.imm1);
			if (inst.imm2 != null) op.Add("Imm2", inst.imm2);

			if (inst.rjmpeq.HasValue) op.Add("addr1", inst.rjmpeq.Value);
			if (inst.rjmplt.HasValue) op.Add("addr2", inst.rjmplt.Value);
			if (inst.rjmpgt.HasValue) op.Add("addr3", inst.rjmpgt.Value);

			return op;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[{0}] ", opcode);

			if (imm1 != null || imm2 != null) sb.AppendFormat("I({0}:{1}) ", imm1, imm2);

			if (idx != PointerIndex.None) sb.AppendFormat("[{0}] ", idx);

			if (op1 != null) sb.Append(op1.fieldname == null ? op1.varref.ToString() : op1.ToString());

			sb.Append(":");

			if (op2 != null) sb.Append(op2.fieldname == null ? op2.varref.ToString() : op2.ToString());

			sb.Append("=>");

			if (dest != null) sb.Append(dest.fieldname == null ? dest.varref.ToString() : dest.ToString());

			if (acc) sb.Append("+");

			if (rjmpeq != null || rjmpgt != null || rjmplt != null) sb.AppendFormat(" J({0}:{1}:{2})", rjmpeq, rjmplt, rjmpgt);
			
			return sb.ToString();

		}
	}

}
