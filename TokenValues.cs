/*
 * Created by SharpDevelop.
 * User: Thomas
 * Date: 2016-07-30
 * Time: 14:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Text;
using NLua;


namespace compiler
{

	
	
	public enum RegSpec
	{
		rNull = 0,
		r1,
		r2,
		r3,
		r4,
		r5,
		r6,
		r7,
		r8,
		rIndex,
		rRed,
		rGreen,
		rStat,
		rOp,
		rNixie,
		rFlanRX,
		rFlanTX,
		rKeyboard,
		
	}
	public enum CompSpec
	{
		Equal,
		Greater,
		Less
	}
	public enum InputMode{
		Any,
		Every,
		Each,
		Scalar
	}
	public enum ArithSpec
	{
		Add,
		Subtract,
		Multiply,
		Divide
	}
	
	public enum SymbolType{
		Unspecified=0,
		Function, 
		Data,
		Program,
		Internal,
	}
	
	public struct SymbolDef
	{
		public string name;
		public SymbolType type;
		
		
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			return (obj is SymbolDef) && Equals((SymbolDef)obj);
		}

		public bool Equals(SymbolDef other)
		{
			return (this.name == other.name) 
				&& (this.type == SymbolType.Unspecified || other.type == SymbolType.Unspecified || this.type == other.type);
		}

		public override int GetHashCode()
		{
			return name.GetHashCode();
		}

		public static bool operator ==(SymbolDef lhs, SymbolDef rhs) {
			return lhs.Equals(rhs);
		}

		public static bool operator !=(SymbolDef lhs, SymbolDef rhs) {
			return !(lhs == rhs);
		}

		#endregion
		
		public static SymbolDef Block = new SymbolDef{name="__block",type=SymbolType.Internal};
		public static SymbolDef TrueBlock = new SymbolDef{name="__trueblock",type=SymbolType.Internal};
		public static SymbolDef FalseBlock = new SymbolDef{name="__falseblock",type=SymbolType.Internal};
		public static SymbolDef Loop = new SymbolDef{name="__loop",type=SymbolType.Internal};
		public static SymbolDef End = new SymbolDef{name="__end",type=SymbolType.Internal};
		public static SymbolDef Return = new SymbolDef{name="__return",type=SymbolType.Internal};
		
		public override string ToString()
		{
			return string.Format("{1}:{0}", name, type.ToString()[0]);
		}

	}
	
	public struct AddrSpec
	{
		
		public int? addr;
		public SymbolDef identifier;
		public int identifierOffset;
		public bool relative;
		
		public int resolve(int atAddr, Dictionary<SymbolDef,int> symbols)
		{
			if(addr.HasValue){
				return this.addr.Value - (relative?atAddr:0);
			} else {
				return symbols[this.identifier] + this.identifierOffset - (relative?atAddr:0);
			}			
		}
		
		public static implicit operator AddrSpec(int i){ return new AddrSpec{addr=i}; }
		public static implicit operator AddrSpec(RegSpec r){ return new AddrSpec{addr=(int)r}; }
		public static implicit operator AddrSpec(string s){ return new AddrSpec{identifier=new SymbolDef{name=s}}; }
		public static implicit operator AddrSpec(SymbolDef sym){ return new AddrSpec{identifier=sym};}
		
		public static AddrSpec operator +(AddrSpec a1, AddrSpec a2)
		{
			if (a2.addr.HasValue) return a1 + a1.addr.Value;
			if (a1.addr.HasValue) return a2 + a1.addr.Value;
			//TODO: handle more cases?
			throw new ArgumentException(string.Format("Cannot add these AddrSpecs: {0},{1}",a1,a2));
		}
		
		public static AddrSpec operator +(AddrSpec a, int i)
		{
			if (a.addr.HasValue)
			{
				a.addr += i;
			} else {
				a.identifierOffset+=i;
			}
			return a;
		}
		
		public override string ToString()
		{
			if (addr.HasValue) {
				return string.Format("{0}",addr);
			} else if(identifierOffset==0) {
				return string.Format("{0}",identifier.name);				
			} else {
				return string.Format("{0}+{1}",identifier.name,identifierOffset);				
			}
		}
	}

	public struct DataItem
	{
		public SignalSpec signal;
		public AddrSpec addr;
		
		public DataItem(char c,AddrSpec addr):this(new SignalSpec(c),addr){}
		public DataItem(SignalSpec signal, AddrSpec addr)
		{
			this.signal =  signal;
			this.addr = addr;
		}
		
		public override string ToString()
		{
			return string.Format("[DataItem Signal={0}, Addr={1}]", signal, addr);
		}

	}
	
	
}