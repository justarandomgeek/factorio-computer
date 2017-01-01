using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public class MemVRef: VRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public Table Evaluate()
		{
			throw new InvalidOperationException(); 
		}

		public readonly SExpr addr;
		public string datatype { get; private set; }
		public bool IsLoaded { get { return false; } }

		public MemVRef(SExpr addr, string datatype = null)
		{
			this.addr = addr;
			this.datatype = datatype;
		}
		
		public override string ToString()
		{
			return string.Format("[MemVRef {0}:{1}]", addr, datatype);
		}
		
		public VExpr FlattenExpressions()
		{
			return this;			
		}
		
		#region Equality
		public static bool operator ==(MemVRef a1, MemVRef a2) { return a1.Equals(a2); }
		public static bool operator !=(MemVRef a1, MemVRef a2) { return !a1.Equals(a2); }
		public override int GetHashCode()
		{
			return addr.GetHashCode();// ^ datatype.GetHashCode(); //ignore datatype
		}
		public override bool Equals(object obj)
		{
			var other = obj as MemVRef;
			if (other != null)
			{
				return other.addr == this.addr;// && other.datatype == this.datatype;
			}
			else
			{
				return false;
			}

		}
		#endregion



		public List<Instruction> FetchToReg(RegVRef dest)
		{
			var code = new List<Instruction>();

			FieldSRef addr = this.addr as FieldSRef;
			
			if (addr == null)
			{
				if (this.addr.IsConstant())
				{
					addr = FieldSRef.Imm1();
				}
				else if(this.addr is IntVarSRef)
				{
					addr = ((IntVarSRef)this.addr).BaseField();
				}
				else
				{
					addr = FieldSRef.ScratchInt();
					code.AddRange(this.addr.FetchToField(addr));
				}
			}

			code.Add(new Instruction
			{
				opcode = Opcode.MemRead,
				op1 = addr,
				dest = dest,
				imm1 = this.addr.IsConstant() ? new IntSExpr(this.addr.Evaluate()) : null,
			});
			return code;
		}

		public List<Instruction> PutFromReg(RegVRef src)
		{
			var code = new List<Instruction>();

			FieldSRef addr = this.addr as FieldSRef;

			if (addr == null)
			{
				if (this.addr.IsConstant())
				{
					addr = FieldSRef.Imm1();
				}
				else
				{
					//TODO: allocate an intermediate here
					code.AddRange(this.addr.FetchToField(addr));
				}
			}

			code.Add(new Instruction
			{
				opcode = Opcode.MemWrite,
				op1 = addr,
				op2 = src,
				imm1 = this.addr.IsConstant() ? new IntSExpr(this.addr.Evaluate()) : null,
			});
			return code;
		}

	}

}