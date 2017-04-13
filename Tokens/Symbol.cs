using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	public struct Symbol
	{
		public string name;
		public SymbolType type;
		public string datatype;
		public int? fixedAddr;
        public PointerIndex frame;
		public int? size
		{
			get
			{
				switch (type) {
					
					default:
						return null;
					case SymbolType.Data:
						return declsize;
					case SymbolType.Constant:
					case SymbolType.Function:
						return data?.Count;
				}
			}
			set
			{
				declsize=value;
			}
		}
		private int? declsize;
		public List<Table> data;


		public override bool Equals(object obj)
		{
			if(obj is Symbol)
			{
				var sym = (Symbol)obj;
				return this.name == sym.name &&
					this.type == sym.type &&
					this.datatype == sym.datatype &&
					this.fixedAddr == sym.fixedAddr &&
					this.frame == sym.frame &&
					this.declsize == sym.declsize &&

					(this.data != null ? this.data.SequenceEqual(sym.data) : sym.data == null);

			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return this.name.GetHashCode() ^ this.type.GetHashCode() ^ this.datatype.GetHashCode() ^
				this.fixedAddr.GetHashCode() ^ this.frame.GetHashCode() ^ this.declsize.GetHashCode() ^
				(this.data?.GetHashCode() ?? 0);
		}

		public void assign(int addr)
		{
			fixedAddr = addr;
		}
		
		public override string ToString()
		{
			char frametype;
			switch (frame)
			{
				case PointerIndex.None:
					frametype = ' ';
					break;
				case PointerIndex.CallStack:
					frametype = 'S';
					break;
				case PointerIndex.ProgConst:
					frametype = 'C';
					break;
				case PointerIndex.ProgData:
					frametype = 'D';
					break;
				case PointerIndex.LocalData:
					frametype = 'L';
					break;
				default:
					frametype = 'X';
					break;
			}

			return string.Format("{5}{0}{1,5}:{4,-3} {2,10} {3}",
				type.ToString()[0],
				fixedAddr,
				datatype,
				name,
				size,
				frametype
				);
		}
		
		public Tokens ToToken()
		{
			switch (type) {
				case SymbolType.Data:
					if(declsize>1) return datatype=="int"?Tokens.INTARRAY:Tokens.ARRAY;
					return datatype=="int"?Tokens.INTVAR:Tokens.VAR;
				case SymbolType.Parameter:
				case SymbolType.Register:
					return datatype=="int"?Tokens.INTVAR:Tokens.VAR;
				case SymbolType.Function:
					var func = Program.CurrentProgram.Functions[name];
					return func.returntype == "int" ? Tokens.SFUNCNAME : Tokens.VFUNCNAME;
				default:
					return Tokens.error;
			}
		}

	}

}