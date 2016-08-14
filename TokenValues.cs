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
	public static class InternalSignals
	{
		public const string op = "signal-0";
		public const string opA= "signal-A";
		
		public const string r1 = "signal-R";
		public const string s1 = "signal-S";
		
		public const string r2 = "signal-T";
		public const string s2 = "signal-U";
		
		public const string rd = "signal-V";
		public const string sd = "signal-W";
		
		public const string imm1 = "signal-grey";
		public const string imm2 = "signal-white";
		
		public const string pc = "signal-blue";
		public const string ret = "signal-green";
	}
	
	public static class Util
    {   
		public static int CondOpCode(InputMode sIn, bool sOut, bool flags, CompSpec comp){
			int i = 0;
        	switch (comp) {
        		case CompSpec.Equal:
					i=1; break;
        		case CompSpec.Less:
        			i=2; break;
        		case CompSpec.Greater:
					i=3; break;
        	}
			
			if(flags) i+=3;
			
			switch (sIn) {
				case InputMode.Every:
					i+=0; break;
				case InputMode.Any:
					i+=6; break;
				case InputMode.Scalar:
					i+=12; break;
				case InputMode.Each:
					i+=18; break;
			}
			
			if(sOut) i+=24;
        	
        	return i;
		}
        
        public static int ArithOpCode(InputMode sIn, bool sOut, ArithSpec arith){
			int i = sOut?52:48;
			
			switch (arith) {
				case ArithSpec.Subtract:
					i+=1; break;
				case ArithSpec.Add:
					i+=2; break;
				case ArithSpec.Divide:
					i+=3; break;
				case ArithSpec.Multiply:
					i+=4; break;
			}
			
			switch (sIn) {
				case InputMode.Every:
				case InputMode.Any:
					throw new Exception();
				case InputMode.Scalar:
					i+=4; break;
				case InputMode.Each:
					i+=0; break;
			}
			
			
			
        	return i;
		}
    }
	
	
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
		r9,
		rRed,
		rGreen,
		rStat,
		rOp,
		rNixie,
		
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
	
	public class SignalSpec
	{
		public static Dictionary<string,int> signalMap = new Dictionary<string, int>();
		static readonly Dictionary<char,SignalSpec> charMap = new Dictionary<char,SignalSpec>{
        	{'1',"signal-1"},{'2',"signal-2"},{'3',"signal-3"},{'4',"signal-4"},{'5',"signal-5"},
        	{'6',"signal-6"},{'7',"signal-7"},{'8',"signal-8"},{'9',"signal-9"},{'0',"signal-0"},
        	{'A',"signal-A"},{'B',"signal-B"},{'C',"signal-C"},{'D',"signal-D"},{'E',"signal-E"},
			{'F',"signal-F"},{'G',"signal-G"},{'H',"signal-H"},{'I',"signal-I"},{'J',"signal-J"},
			{'K',"signal-K"},{'L',"signal-L"},{'M',"signal-M"},{'N',"signal-N"},{'O',"signal-O"},
			{'P',"signal-P"},{'Q',"signal-Q"},{'R',"signal-R"},{'S',"signal-S"},{'T',"signal-T"},
			{'U',"signal-U"},{'V',"signal-V"},{'W',"signal-W"},{'X',"signal-X"},{'Y',"signal-Y"},
			{'Z',"signal-Z"},{'-',"fast-splitter"},{'.',"train-stop"}};
		
		public SignalSpec(char c)
        {
			if(charMap.ContainsKey(c))
			{
				this.signal = charMap[c].signal;
			}	
        }
		public AddrSpec sigval{
			get { return signalMap.ContainsKey(this.signal)?signalMap[this.signal]:0; }
		}
		
		public static Dictionary<string,string> typeMap = new Dictionary<string, string>();
		public string type {
			get 
			{
				if(string.IsNullOrEmpty(this.signal)) return "virtual";
				if(!typeMap.ContainsKey(this.signal)) return "virtual";
				return typeMap[this.signal];
			}
		}
		
		public RegSpec reg;
		public string signal;
		
		public static SignalSpec Imm1{get{return new SignalSpec(RegSpec.rOp,InternalSignals.imm1);}}
		public static SignalSpec Imm2{get{return new SignalSpec(RegSpec.rOp,InternalSignals.imm2);}}

		public SignalSpec(){}
		
		public static implicit operator SignalSpec(string s){return new SignalSpec(s);}
		public SignalSpec(string s){this.signal = s;}
		public SignalSpec(RegSpec r, string s){this.reg = r;this.signal = s;}
		
		public override string ToString()
		{
			return string.Format("{0}.{1}", reg==RegSpec.rNull?"":reg.ToString(), signal);
		}

	}
	public struct AddrSpec
	{
		public static Dictionary<string,int> map = new Dictionary<string, int>();
		
		public int? addr;
		public string identifier;
		public int identifierOffset;
		public bool relative;
		
		public int resolve(int atAddr)
		{
			if(addr.HasValue){
				return this.addr.Value - (relative?atAddr:0);
			} else {
				return map[this.identifier] + this.identifierOffset - (relative?atAddr:0);
			}			
		}
		
		public static implicit operator AddrSpec(int i){ return new AddrSpec{addr=i}; }
		public static implicit operator AddrSpec(RegSpec r){ return new AddrSpec{addr=(int)r}; }

		public override string ToString()
		{
			if (addr.HasValue) {
				return string.Format("{0}",addr);
			} else if(identifierOffset==0) {
				return string.Format("{0}",identifier);				
			} else {
				return string.Format("{0}+{1}",identifier,identifierOffset);				
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
	
	public class DataList: Dictionary<SignalSpec,AddrSpec>
	{
		public DataList():base(){}
		
		public DataList(DataItem data){this.Add(data);}
		public static implicit operator DataList(DataItem data){return new DataList(data);}
		public DataList(char c,AddrSpec addr):this(new DataItem(c,addr)){}
		
		public void Add(DataItem data)
		{
			this.Add(data.signal,data.addr);
		}
		
		
		public static DataList CondOp(bool sOut, bool flags, SignalSpec S1, CompSpec comp, SignalSpec S2)
		{
			var dl = new DataList();
			
			InputMode im;
			switch (S1.signal) {
				case "signal-each":
					im=InputMode.Each; break;
				case "signal-anything":
					im=InputMode.Any; break;
				case "signal-everything":
					im=InputMode.Every; break;
				default:
					im=InputMode.Scalar; break;
			}
			
			dl.Op = Util.CondOpCode(im,sOut,flags,comp);
			
			dl.R1 = S1.reg;
			dl.S1 = S1.sigval;
			dl.R2 = S2.reg;
			dl.S2 = S2.sigval;

			return dl;
		}
		public static DataList CondOp(bool sOut, bool flags, SignalSpec S1, CompSpec comp, AddrSpec Imm2)
		{
			var dl = CondOp(sOut,flags,S1,comp,SignalSpec.Imm2);
			dl.Imm2=Imm2;
			return dl;
		}
		
		public static DataList ArithOp(RegSpec R1, ArithSpec Op, SignalSpec S2){
			var dl = new DataList();
			dl.Op = Util.ArithOpCode(InputMode.Each,false,Op);
			dl.R1 = R1;
			dl.R2 = S2.reg;
			dl.S2 = S2.sigval;
			return dl;
			
		}
		public static DataList ArithOp(RegSpec R1, ArithSpec Op, AddrSpec Imm2)
		{
			var dl = ArithOp(R1,Op,new SignalSpec(RegSpec.rOp,InternalSignals.imm2));
			dl.Imm2 = Imm2;
			return dl;
		}
		public static DataList ArithOp(SignalSpec S1, ArithSpec Op, SignalSpec S2){
			var dl = new DataList();
			dl.Op = Util.ArithOpCode(InputMode.Scalar,true,Op);
			dl.R1 = S1.reg;
			dl.S1 = S1.sigval;
			dl.R2 = S2.reg;
			dl.S2 = S2.sigval;
			return dl;
		}
		public static DataList ArithOp(SignalSpec S1, ArithSpec Op, AddrSpec Imm2)
		{
			var dl = ArithOp(S1,Op,new SignalSpec(RegSpec.rOp,InternalSignals.imm2));
			dl.Imm2 = Imm2;
			return dl;
		}
		
		public DataList AssignOp(RegSpec RD, bool Append)
		{
			var dl = this;
			if(Append) dl.OpA = 1;
			dl.RD = RD;
			return dl;
		}
		public DataList AssignOp(SignalSpec SD, bool Append)
		{
			var dl = this;
			if(Append) dl.OpA = 1;
			dl.RD = SD.reg;
			dl.SD = SD.sigval;
			return dl;
		}
		
		
		public static DataList Wire(RegSpec R1, RegSpec R2)
		{
			var dl = new DataList();
			dl.Op = 80;
			dl.R1 = R1;
			dl.R2 = R2;
			return dl;
		}
		
		public static DataList WriteMemory(SignalSpec S1, RegSpec R2)
		{
			var dl = new DataList();
			dl.Op = 81;
			dl.R1 = S1.reg;
			dl.S1 = S1.sigval;
			dl.R2 = R2;
			return dl;
		}
		public static DataList WriteMemory(AddrSpec Imm1, RegSpec R2)
		{
			var dl = WriteMemory(new SignalSpec(RegSpec.rOp,InternalSignals.imm1),R2);
			dl.Imm1 = Imm1;
			return dl;
		}
		
		public static DataList ReadMemory(SignalSpec S1)
		{
			var dl = new DataList();
			dl.Op = 82;
			dl.R1 = S1.reg;
			dl.S1 = S1.sigval;
			return dl;			
		}
		public static DataList ReadMemory(AddrSpec Imm1)
		{
			var dl = ReadMemory(new SignalSpec(RegSpec.rOp,InternalSignals.imm1));
			dl.Imm1 = Imm1;
			return dl;
		}
		
		public static DataList Jump(SignalSpec S1, bool relative,bool call)
		{
			var dl = new DataList();
			dl.Op = !relative?(!call?70:71):(!call?72:73);
			dl.R1=S1.reg;
			dl.S1=S1.sigval;
			return dl;
		}		
		public static DataList Jump(AddrSpec Imm1, bool relative,bool call)
		{
			var dl = Jump(new SignalSpec(RegSpec.rOp,InternalSignals.imm1),relative,call);
			Imm1.relative=relative;
			dl.Imm1 = Imm1;
			return dl;
		}
		
		public static DataList Branch(SignalSpec S1, SignalSpec S2, bool call)
		{
			var dl = new DataList();
			dl.Op = !call?74:75;
			dl.R1=S1.reg;
			dl.S1=S1.sigval;
			dl.R2=S2.reg;
			dl.S2=S2.sigval;
			return dl;
		}		
		public static DataList Branch(SignalSpec S1, AddrSpec Imm2, bool call)
		{
			var dl = Branch(S1,new SignalSpec(RegSpec.rOp,InternalSignals.imm2),call);
			dl.Imm2 = Imm2;
			return dl;
		}
		
		public static DataList BinaryString(string s)
		{
			var chars = new Dictionary<char,int>();
			int i = 0;
			foreach (var c in s) {
				if(!chars.ContainsKey(c))chars.Add(c,0);
				chars[c]+=1<<i++;
			}
			var dl = new DataList();
			foreach (var c in chars) {
				dl.Add(new SignalSpec(c.Key),c.Value);
			}
			return dl;
		}
		
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("{");
			foreach (var signal in this) {
				sb.AppendFormat("{0}={1}, ",signal.Key,signal.Value);
			}
			if(sb.Length>1)
			{
				sb.Remove(sb.Length-2,2);
			}
			sb.Append("}");
			return sb.ToString();
		}
		
		public AddrSpec? Op {
			get {
				if (this.ContainsKey(InternalSignals.op)) return this[InternalSignals.op];
				return null;
			}
			set {
				if (value.HasValue){
				 	this[InternalSignals.op] = value.Value;
				}else{
					if(this.ContainsKey(InternalSignals.op)) this.Remove(InternalSignals.op);
				}
			}
		}
		public AddrSpec? Imm1 {
			get {
				if (this.ContainsKey(InternalSignals.imm1)) return this[InternalSignals.imm1];
				return null;
			}
			set {
				if (value.HasValue){
				 	this[InternalSignals.imm1] = value.Value;
				}else{
					if(this.ContainsKey(InternalSignals.imm1)) this.Remove(InternalSignals.imm1);
				}
			}
		}
		public AddrSpec? Imm2 {
			get {
				if (this.ContainsKey(InternalSignals.imm2)) return this[InternalSignals.imm2];
				return null;
			}
			set {
				if (value.HasValue){
				 	this[InternalSignals.imm2] = value.Value;
				}else{
					if(this.ContainsKey(InternalSignals.imm2)) this.Remove(InternalSignals.imm2);
				}
			}
		}
		public AddrSpec? OpA {
			get {
				if (this.ContainsKey(InternalSignals.opA)) return this[InternalSignals.opA];
				return null;
			}
			set {
				if (value.HasValue){
				 	this[InternalSignals.opA] = value.Value;
				}else{
					if(this.ContainsKey(InternalSignals.opA)) this.Remove(InternalSignals.opA);
				}
			}
		}
		public AddrSpec? R1 {
			get {
				if (this.ContainsKey(InternalSignals.r1)) return this[InternalSignals.r1];
				return null;
			}
			set {
				if (value.HasValue){
				 	this[InternalSignals.r1] = value.Value;
				}else{
					if(this.ContainsKey(InternalSignals.r1)) this.Remove(InternalSignals.r1);
				}
			}
		}
		public AddrSpec? R2 {
			get {
				if (this.ContainsKey(InternalSignals.r2)) return this[InternalSignals.r2];
				return null;
			}
			set {
				if (value.HasValue){
				 	this[InternalSignals.r2] = value.Value;
				}else{
					if(this.ContainsKey(InternalSignals.r2)) this.Remove(InternalSignals.r2);
				}
			}
		}
		public AddrSpec? RD {
			get {
				if (this.ContainsKey(InternalSignals.rd)) return this[InternalSignals.rd];
				return null;
			}
			set {
				if (value.HasValue){
				 	this[InternalSignals.rd] = value.Value;
				}else{
					if(this.ContainsKey(InternalSignals.rd)) this.Remove(InternalSignals.rd);
				}
			}
		}
		public AddrSpec? S1 {
			get {
				if (this.ContainsKey(InternalSignals.s1)) return this[InternalSignals.s1];
				return null;
			}
			set {
				if (value.HasValue){
				 	this[InternalSignals.s1] = value.Value;
				}else{
					if(this.ContainsKey(InternalSignals.s1)) this.Remove(InternalSignals.s1);
				}
			}
		}
		public AddrSpec? S2 {
			get {
				if (this.ContainsKey(InternalSignals.s2)) return this[InternalSignals.s2];
				return null;
			}
			set {
				if (value.HasValue){
				 	this[InternalSignals.s2] = value.Value;
				}else{
					if(this.ContainsKey(InternalSignals.s2)) this.Remove(InternalSignals.s2);
				}
			}
		}
		public AddrSpec? SD {
			get {
				if (this.ContainsKey(InternalSignals.sd)) return this[InternalSignals.sd];
				return null;
			}
			set {
				if (value.HasValue){
				 	this[InternalSignals.sd] = value.Value;
				}else{
					if(this.ContainsKey(InternalSignals.sd)) this.Remove(InternalSignals.sd);
				}
			}
		}
		
	}
	
}