using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler
{

	public enum Opcode
	{
		//Invalid = -1,
		Halt = 0,

		EachCMPS1 = 1,
		EachCMPSF,
		EachCMPSV,
		EveryCMPS1,
		EveryCMPSF,
		EveryCMPSV,
		AnyCMPS1,
		AnyCMPSF,
		AnyCMPSV,
		SCMPS1,
		SCMPSF,
		SCMPSV,

		/* GAP */

		EachSubV = 17,
		EachAddV,
		EachDivV,
		EachMulV,
		EachModV,
		EachAndV,
		EachOrV,
		EachXorV,
		EachPowV,
		EachLShiftV,
		EachRShiftV,

		/* GAP */
		
		Sub = 39,
		Add,
		Div,
		Mul,
		Mod,
		And,
		Or,
		Xor,
		Pow,
		LShift,
		RShift,

		/* GAP */

		VMul = 61,
		VDiv,

		SArrPick,
		SArrWrite,

		SShiftUp,
		SShiftDn,

		VReplace,

		/* GAP */

		Jump = 70,
		Branch,
		Exec,

		/* GAP */

		Wire = 80,
		MemWrite,
		MemRead,
		Push,
		Pop,
		Append,

		/* GAP */

		PlayerInfo = 100,
		ConMan,
		Scammer,
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
			var op = new Table();
			op.datatype = "opcode";
			op.Add("op", (int)inst.opcode);

			if (inst.relative) op.Add("signal-green", 1);
			if (inst.acc) op.Add("signal-A", 1);

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

			if (relative) sb.Append("R ");

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
