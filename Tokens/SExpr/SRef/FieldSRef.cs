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

		public bool IsConstant()
		{
			return false;
		}
		public int Evaluate()
		{
			throw new InvalidOperationException();
		}
		
		private FieldSRef() { }
		public FieldSRef(VRef varref, string fieldname) { this.varref = varref; this.fieldname = fieldname; }
		public static FieldSRef CallSite { get { return new FieldSRef(RegVRef.rScratch, "signal-0"); } }
		public static FieldSRef SReturn { get { return new FieldSRef(RegVRef.rScratch, "signal-1"); } }
		public static FieldSRef GlobalInt(string intname) { return new FieldSRef(RegVRef.rGlobalInts,intname); }
		public static FieldSRef LocalInt(string funcname, string intname) { return new FieldSRef(RegVRef.rLocalInts(funcname),intname); }
		public static FieldSRef IntArg(string funcname, string intname) { return new FieldSRef(RegVRef.rIntArgs(funcname), intname); }
		public static FieldSRef Imm1() { return new FieldSRef(RegVRef.rOpcode, "Imm1"); }
		public static FieldSRef Imm2() { return new FieldSRef(RegVRef.rOpcode, "Imm2"); }

		public static FieldSRef VarField(VRef varref, string fieldname) { return new FieldSRef(varref, fieldname); }

		public static FieldSRef Pointer(PointerIndex ptr)
		{
			string[] ptrnames = { "err", "callstack", "progbase", "progdata", "localdata" };
			return new FieldSRef(RegVRef.rIndex, ptrnames[(int)ptr]);
		}

		private static int nextScratch=Program.CurrentProgram.NativeFields.IndexOf("signal-0");
		public static FieldSRef ScratchInt() { return new FieldSRef(RegVRef.rScratch, Program.CurrentProgram.NativeFields[nextScratch++]); }
		public static void ResetScratchInts()
		{
			nextScratch = Program.CurrentProgram.NativeFields.IndexOf("signal-0");
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
			return string.Format("[FieldSRef {0}.{1}]", varref, fieldname);
		}
		
		public List<Instruction> PutFromField(FieldSRef src)
		{
			throw new NotImplementedException();
		}

		public List<Instruction> PutFromInt(int value)
		{
			var code = new List<Instruction>();

			if(varref.IsLoaded)
			{
				code.Add(new Instruction
				{
					opcode = Opcode.Sub,
					op1 = FieldSRef.Imm1(),
					op2 = this,
					dest = this,
					acc = true,
				});
			}
			else
			{
				throw new NotImplementedException();
			}

			return code;
			
		}

		public List<Instruction> FetchToField(FieldSRef dest)
		{
			var code = new List<Instruction>();
			code.Add(new Instruction
			{
				opcode = Opcode.Sub,
				op1 = this,
				op2 = dest,
				dest = dest,
				acc = true,
			});

			return code;
		}
	}

}