using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class FieldSRef : SRef
	{
		public readonly VRef varref;
		public readonly string fieldname;
		public bool precleared; //TODO: direct fetch where possible

		public override bool Equals(object obj)
		{
			if (obj is FieldSRef)
			{
				var fsr = obj as FieldSRef;
				return this.varref.Equals(fsr.varref) && this.fieldname == fsr.fieldname && this.precleared==fsr.precleared;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return varref.GetHashCode() ^ fieldname.GetHashCode();
		}

		public bool IsConstant()
		{
			return false;
		}
		public int Evaluate()
		{
			throw new InvalidOperationException();
		}
		
		private FieldSRef() { }
		public FieldSRef(VRef varref, string fieldname, bool precleared = false) { this.varref = varref; this.fieldname = fieldname; this.precleared = precleared; }
		public static FieldSRef GlobalInt(string intname) { return new FieldSRef(RegVRef.rGlobalInts,intname); }
		private readonly static int firstarg = Program.CurrentProgram?.NativeFields?.IndexOf("signal-0") ?? 0;
		public static FieldSRef LocalInt(int intnum) { return new FieldSRef(RegVRef.rLocalInts, Program.CurrentProgram.NativeFields[intnum + firstarg]); }
		public static FieldSRef IntArg(int intnum) { return new FieldSRef(RegVRef.rIntArgs, Program.CurrentProgram.NativeFields[intnum + firstarg]); }
		public static FieldSRef CallSite { get { return new FieldSRef(RegVRef.rIntArgs, "signal-0"); } }
		public static FieldSRef SReturn { get { return new FieldSRef(RegVRef.rIntArgs, "signal-1"); } }
		public static FieldSRef Imm1() { return new FieldSRef(RegVRef.rOpcode, "Imm1"); }
		public static FieldSRef Imm2() { return new FieldSRef(RegVRef.rOpcode, "Imm2"); }

		public static FieldSRef VarField(VRef varref, string fieldname) { return new FieldSRef(varref, fieldname); }

		public static FieldSRef Pointer(PointerIndex ptr)
		{
			string[] ptrnames = { "err", "callstack", "progbase", "progdata", "localdata" };
			return new FieldSRef(RegVRef.rIndex, ptrnames[(int)ptr]);
		}

		private static int nextScratch=Program.CurrentProgram?.NativeFields?.IndexOf("signal-0") ?? 0;
		public static FieldSRef ScratchInt() { return new FieldSRef(RegVRef.rScratchInts, Program.CurrentProgram.NativeFields[nextScratch++]); }
		public static void ResetScratchInts()
		{
			nextScratch = Program.CurrentProgram?.NativeFields?.IndexOf("signal-0") ?? 0;
		}

		// null-field conversion for use in Instruction to represent register only
		public static implicit operator FieldSRef(RegVRef reg) { return new FieldSRef(reg,null); }
		
		public bool IsLoaded
		{
			get
			{
				return varref.IsLoaded;
			}
		}

		public override string ToString()
		{
			return string.Format("[{0}.{1}]", varref, fieldname);
		}
		
		public List<Instruction> PutFromField(FieldSRef src)
		{
			var code = new List<Instruction>();
			if (varref.AsReg() == null)
			{
				code.AddRange(varref.FetchToReg(RegVRef.rFetch(1)));
				code.AddRange(this.InRegister(RegVRef.rFetch(1)).PutFromField(src));
				code.AddRange(varref.PutFromReg(RegVRef.rFetch(1)));
			}
			else
			{
				code.Add(new Instruction { opcode = Opcode.Sub, op1 = src, op2 = this, dest = this, acc = true });
			}

			return code;
		}

		public List<Instruction> PutFromInt(int value)
		{
			var code = new List<Instruction>();

			if(varref.IsLoaded)
			{
				code.Add(new Instruction { opcode = Opcode.Sub, imm1 = new IntSExpr(value), op1 = Imm1(), op2 = this, dest = this, acc = true });
			}
			else
			{
				code.AddRange(this.varref.FetchToReg(RegVRef.rFetch(1)));
				code.AddRange(this.InRegister(RegVRef.rFetch(1)).PutFromInt(value));
				code.AddRange(this.varref.PutFromReg(RegVRef.rFetch(1)));
			}

			return code;
			
		}

		public List<Instruction> FetchToField(FieldSRef dest)
		{
			var code = new List<Instruction>();
			if(varref.AsReg() == null)
			{
				code.AddRange(varref.FetchToReg(RegVRef.rFetch(1)));
				code.AddRange(this.InRegister(RegVRef.rFetch(1)).FetchToField(dest));
			}
			else
			{
				code.Add(new Instruction { opcode = Opcode.Sub, op1 = this, op2 = dest, dest = dest, acc = true });
			}

			return code;
		}

		public PointerIndex frame()
		{
			//TODO: the frame this field is in terms of, not the frame of varref!
			return PointerIndex.None;
		}

		public FieldSRef InRegister(RegVRef reg)
		{
			return new FieldSRef(reg.AsType(varref.datatype), fieldname);
		}

		public FieldSRef AsDirectField()
		{
			var reg = varref.AsReg();
			if(reg == null)
			{
				return null;
			}
			else
			{
				return this;
			}
		}

		public FieldSRef AsPreCleared()
		{
			var fsr = this;
			fsr.precleared = true;
			return fsr;
		}
	}

}