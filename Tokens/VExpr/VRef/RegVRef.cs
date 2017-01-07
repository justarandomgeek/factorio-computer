using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class RegVRef: VRef
	{
		public readonly int reg;
		public string datatype { get; private set; }
		public bool IsLoaded { get { return true; } }

		//public RegVRef() { }
		public RegVRef(int i) { reg = i; }
		public RegVRef(int i, string t) { reg = i; datatype = t; }

		public static RegVRef rNull { get { return new RegVRef(0, "var"); } }
		public static RegVRef rGlobalInts { get { return new RegVRef(1, "__globalints"); } }
		public static RegVRef rLocalInts(string funcname) { return new RegVRef(2, "__li" + funcname); }
		public static RegVRef rIntArgs(string funcname) { return new RegVRef(8, "__li" + funcname); }
		public static RegVRef rScratchInts { get { return new RegVRef(8, "var"); } }
		public static RegVRef rScratchTab { get { return new RegVRef(7, "var"); } }
		public static RegVRef rVarArgs { get { return new RegVRef(7, "var"); } }

		public static RegVRef rIndex { get { return new RegVRef(9, "ireg"); } }
		public static RegVRef rOpcode { get { return new RegVRef(13, "opcode"); } }

		public RegVRef AsType(string datatype)
		{
			return new RegVRef(reg, datatype);
		}

		public bool IsConstant()
		{
			return false;
		}
		
		public override string ToString()
		{
			return string.Format("[R{0}:{1}]", reg, datatype);
		}
		
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
			return new List<Instruction>
			{
				new Instruction {
					opcode = Opcode.EachAddV,
					op1 = this,
					dest = dest
				}
			};
		}
		public List<Instruction> PutFromReg(RegVRef src)
		{
			if (src == this) return new List<Instruction>();
			return new List<Instruction>
			{
				new Instruction {
					opcode = Opcode.EachAddV,
					op1 = src,
					dest = this
				}
			};
		}

		public RegVRef AsReg()
		{
			return this;
		}
	}

}