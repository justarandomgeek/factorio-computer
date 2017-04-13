using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{
	public enum RegNum
	{
		rNull = 0,
		rGlobalInts,
		rLocalInts,
		rScratchInts,
		rFetch1,
		rFetch2,
		rArg1,
		rArg2,
		rArg3,
		rArg4,
		rVReturn=rArg4,
		rArg5,
		rIntArgs=rArg5,
		rTemp1,
		rTemp2,
		rTemp3,
		rTemp4,
		rTemp5,
		rSTemp1,
		rSTemp2,
		rSTemp3,
		rSTemp4,
		rSTemp5,
		//gap
		rIndex = 101,
		rOpcode = 105,

	}

	public class RegVRef: VRef
	{
		public readonly int reg;
		public string datatype { get; private set; }
		public bool IsLoaded { get { return true; } }

		//public RegVRef() { }
		public RegVRef(int reg) { this.reg = reg; }
		public RegVRef(int reg, string datatype) { this.reg = reg; this.datatype = datatype; }

		public static RegVRef rNull { get { return new RegVRef(0, "var"); } }

		public static RegVRef rGlobalInts { get { return new RegVRef(1, "__globalints"); } }

		public static RegVRef rLocalInts { get { return new RegVRef(2, "var"); } }

		public static RegVRef rScratchInts { get { return new RegVRef(3, "var"); } }

		//TODO: these should probably throw on args too high
		public static RegVRef rFetch(int i) { return new RegVRef(4 + (i - 1), "var"); }

		public static RegVRef rArg(int i) { return new RegVRef(6 + (i - 1), "var"); }
		public static RegVRef rVReturn() { return rFetch(1); }
		public static RegVRef rIntArgs { get { return rArg(5); } }

		public static RegVRef rTemp(int i) { return new RegVRef(11 + (i - 1), "var"); }

		public static RegVRef rSTemp(int i) { return new RegVRef(16 + (i - 1), "var"); }

		// need a name for r21-25?
		// r26-100?
		
		
		
		// need mappings for other special regs? maybe rStat?
		public static RegVRef rIndex { get { return new RegVRef(101, "ireg"); } }
		public static RegVRef rOpcode { get { return new RegVRef(105, "opcode"); } }

		public RegVRef AsType(string datatype) { return new RegVRef(reg, datatype); }
		public bool IsConstant() { return false; }

		//
		public bool CalleeSaved { get { return reg == 1 || reg == 2 || (reg >= 16 && reg <= 25); } }

		public override string ToString() { return string.Format("[{0}:{1}]", (RegNum)reg, datatype); }

		public static bool operator ==(RegVRef a1, RegVRef a2) { return (a1?.Equals(a2)) ?? ((Object)a2) == null; }
		public static bool operator !=(RegVRef a1, RegVRef a2) { return !(a1?.Equals(a2) ?? ((Object)a2) == null); }
		public override int GetHashCode()
		{
			return reg.GetHashCode();// ^ datatype.GetHashCode(); //ignore datatype?
		}
		public override bool Equals(object obj)
		{
			if (obj is RegVRef)
			{
				var other = obj as RegVRef;
				return other.reg == this.reg;// && other.datatype == this.datatype;
			}
			else
			{
				return false;
			}
		}

		// this would only be used to copy a reg?
		public List<Instruction> FetchToReg(RegVRef dest)
		{
			if (dest == this) return new List<Instruction>();
			return new List<Instruction> {
				new Instruction { opcode = Opcode.EachAddV, op1 = this, dest = dest }
			};
		}
		public List<Instruction> PutFromReg(RegVRef src) { return src.FetchToReg(this); }
		public RegVRef AsReg() { return this; }
	}

}