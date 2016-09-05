/*
 * Created by SharpDevelop.
 * User: Thomas
 * Date: 2016-09-05
 * Time: 10:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace compiler
{
	[DebuggerDisplay("{dbgstring}")]
	public class DataList: Dictionary<SignalSpec,AddrSpec>
	{
		public DataList():base(){}
		
		private string dbgstring{get{return this.ToString();}}
		public override string ToString()
		{
			if (this.Op.HasValue) {
				int op = this.Op.Value.addr.GetValueOrDefault();
				string r1s1 = string.Format("{0}.{1}",this.R1.GetValueOrDefault(),this.S1.GetValueOrDefault());
				string r2s2 = string.Format("{0}.{1}",this.R2.GetValueOrDefault(),this.S2.GetValueOrDefault());
				string rdsd = string.Format("{0}.{1}",this.RD.GetValueOrDefault(),this.SD.GetValueOrDefault());
				var i1 = this.Imm1.GetValueOrDefault();
				var i2 = this.Imm2.GetValueOrDefault();
				
				
				if (op>0 && op<=60)
				{
					return string.Format("ALU{3} {0} {1} => {2} | {4} {5}",r1s1,r2s2,rdsd,op,i1,i2);
				}
				
				switch (op) {
					case 70:
						return string.Format("{0}{1} {2} | {3}", 
						                     i2.addr==1?"R":"",
						                     rdsd=="."?"JUMP":"CALL",
						                     r1s1,
						                     i1
						                    );
					case 71:
						return string.Format("{0} {1}?={2} | {3} {4} {5}",
						                     rdsd=="."?"BRANCH":"BRCALL",
						                     r1s1,r2s2,
						                     this["signal-1"],
											 this["signal-2"],
											 this["signal-3"]											 
											);
					case 72:
						return string.Format("EXEC {0}", 
						                     this.R1.GetValueOrDefault()
						                    );
					case 81:
						return string.Format("{4} => [{0}] | {5}",r1s1,r2s2,rdsd,op,
						                     this.R2.GetValueOrDefault(), i1
						                    );
					case 82:
						return string.Format("[{0}] => {4} | {5}",r1s1,r2s2,rdsd,op,
						                     this.RD.GetValueOrDefault(), i1
						                    );
					case 83:
						return string.Format("PUSH {5} {4}",r1s1,r2s2,rdsd,op,
						                     this.R2.GetValueOrDefault(),
						                     this.S1.GetValueOrDefault()
						                    );
					case 84:
						return string.Format("POP {5} {4}",r1s1,r2s2,rdsd,op,
						                     this.RD.GetValueOrDefault(),
						                     this.S1.GetValueOrDefault()
						                    );
				}
			} 
			
			var sb = new StringBuilder();
			sb.Append("DATA:{");
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
		
		public DataList(DataItem data){this.Add(data);}
		public static implicit operator DataList(DataItem data){return new DataList(data);}
		public DataList(char c,AddrSpec addr):this(new DataItem(c,addr)){}
		
		public void Add(DataItem data)
		{
			this.Add(data.signal,data.addr);
		}
		public void Add(DataList data)
		{
			foreach (var element in data) {
				if (this.ContainsKey(element.Key)){
					this[element.Key] += element.Value;
				} else {
					this.Add(element.Key,element.Value);
				}
			}
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
		public static DataList ArithOp(AddrSpec Imm1, ArithSpec Op, AddrSpec Imm2)
		{
			var dl = ArithOp(
				new SignalSpec(RegSpec.rOp,InternalSignals.imm1),
				Op,
				new SignalSpec(RegSpec.rOp,InternalSignals.imm2));
			dl.Imm1 = Imm1;
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
		
		public static DataList Push(int stack, RegSpec R2)
		{
			var dl = new DataList();
			dl.Op=83;
			dl.R1 = RegSpec.rIndex;
			dl.R2 = R2;
			dl.S1=stack;
			return dl;
		}
		public static DataList Pop(int stack, RegSpec RD)
		{
			var dl = new DataList();
			dl.Op=84;
			dl.R1 = RegSpec.rIndex;
			dl.RD = RD;
			dl.S1=stack;
			return dl;
		}
		
		public static DataList Jump(SignalSpec S1, bool relative,bool call)
		{
			var dl = new DataList();
			dl.Op = 70;
			dl.R1=S1.reg;
			dl.S1=S1.sigval;
			dl.Imm2=relative?1:0;
			if(call)
			{
				dl.RD = RegSpec.r8;
				dl.SD = new SignalSpec("signal-green").sigval;
			}
			return dl;
		}		
		public static DataList Jump(AddrSpec Imm1, bool relative,bool call)
		{
			var dl = Jump(new SignalSpec(RegSpec.rOp,InternalSignals.imm1),relative,call);
			Imm1.relative=relative;
			dl.Imm1 = Imm1;
			return dl;
		}
		
		public static DataList BranchCond(SignalSpec S1, SignalSpec S2)
		{
			var dl = new DataList();
			dl.R1=S1.reg;
			dl.S1=S1.sigval;
			dl.R2=S2.reg;
			dl.S2=S2.sigval;
			return dl;
		}
		public static DataList BranchCond(SignalSpec S1, AddrSpec Imm2)
		{
			var dl = BranchCond(S1,SignalSpec.Imm2);
			dl.Imm2=Imm2;
			return dl;
		}
		public static DataList Branch(DataList br, AddrSpec eq, AddrSpec lt, AddrSpec gt, bool call)
		{
			br.Op = 71;
			if(call)
			{
				br.RD = RegSpec.r8;
				br.SD = new SignalSpec("signal-green").sigval;
			}
			br["signal-1"]=eq;
			br["signal-2"]=lt;
			br["signal-3"]=gt;
			return br;
		}
		
		
		public static DataList IfComp(SignalSpec S1, CompSpec comp, SignalSpec S2)
		{
			return IfComp(BranchCond(S1,S2),comp);
		}
		public static DataList IfComp(SignalSpec S1, CompSpec comp, AddrSpec Imm2)
		{
			return IfComp(BranchCond(S1,Imm2),comp);
		}
		
		public static DataList IfComp(DataList br, CompSpec comp)
		{
			br.Op=71;
			br[new SignalSpec("signal-1")]=new AddrSpec{identifier=SymbolDef.FalseBlock,relative=true};
			br[new SignalSpec("signal-2")]=new AddrSpec{identifier=SymbolDef.FalseBlock,relative=true};
			br[new SignalSpec("signal-3")]=new AddrSpec{identifier=SymbolDef.FalseBlock,relative=true};
			
			switch (comp) {
				case CompSpec.Equal:
					br[new SignalSpec("signal-1")]=new AddrSpec{identifier=SymbolDef.TrueBlock,relative=true};
					break;
				case CompSpec.Less:
					br[new SignalSpec("signal-2")]=new AddrSpec{identifier=SymbolDef.TrueBlock,relative=true};
					break;
				case CompSpec.Greater:
					br[new SignalSpec("signal-3")]=new AddrSpec{identifier=SymbolDef.TrueBlock,relative=true};
					break;
			}
			return br;
		}
		
		public static DataList Exec(RegSpec R1)
		{
			var dl = new DataList();
			dl.Op = 72;
			dl.R1=R1;
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
