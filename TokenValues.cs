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
using System.Linq;
using System.Text;
using NLua;


namespace compiler
{

	
	
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
	
	
	public class TypeInfo:Dictionary<string,string>
	{
		public void Add(FieldInfo fi)
		{
			this.Add(fi.name,fi.basename);
		}
	}
	public struct FieldInfo{
		public string name;
		public string basename;
	}
	
	public class SymbolList:List<Symbol>
	{
		public void AddParam(FieldInfo pi)
		{
			this.Add(new Symbol{
			         	name=pi.name,
			         	type=SymbolType.Parameter,
			         	datatype=pi.basename			         	
			         });
		}
	}
	
	public class FunctionInfo{
		public string name;
		public SymbolList locals = new SymbolList();
		public TypeInfo localints = new TypeInfo();
		public Block body;
		
		public void AllocateInts()
		{
			//TODO allocate any ->null localints sequentially by signal map
		}
		
	}
	
	public class FunctionCall:Statement
	{
		public string name;
		public ExprList args;
		public RefList returns;
		public override string ToString()
		{
			return string.Format("[FunctionCall {0}({1}) => {2}]", name, args, returns);
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		
		public void CollapseConstants()
		{
			args.CollapseConstants();
		}

	}
	
	public class Return:Statement
	{
		public ExprList returns;
		public override string ToString()
		{
			return string.Format("[Return Returns={0}]", returns);
		}	
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		public void CollapseConstants()
		{
			returns.CollapseConstants();
		}
	}
	
	public class ExprList{
		public List<SExpr> ints = new List<SExpr>();
		public List<VExpr> vars = new List<VExpr>();
		public override string ToString()
		{
			return string.Format("[ExprList Ints={0}; Vars={1}]", string.Join(",",ints), string.Join(",",vars));
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		public void CollapseConstants()
		{
			ints = ints.Select(se => se.CollapseConstants()).ToList();
			vars = vars.Select(ve => ve.CollapseConstants()).ToList();
		}
	}
	
	public class RefList{
		public List<SRef> ints = new List<SRef>();
		public List<VRef> vars = new List<VRef>();
		public override string ToString()
		{
			return string.Format("[RefList Ints={0}; Vars={1}]", string.Join(",",ints), string.Join(",",vars));
		}
	}
	
	public enum SymbolType{
		Function, 
		Data,
		Register,
		Program,
		Internal,
		Parameter,
		Array,
	}
	
	public struct Symbol
	{
		public string name;
		public SymbolType type;
		public string datatype;
		public int? fixedAddr;
		public int? size;
		
		public static Symbol Block = new Symbol{name="__block",type=SymbolType.Internal};
		public static Symbol TrueBlock = new Symbol{name="__trueblock",type=SymbolType.Internal};
		public static Symbol FalseBlock = new Symbol{name="__falseblock",type=SymbolType.Internal};
		public static Symbol Loop = new Symbol{name="__loop",type=SymbolType.Internal};
		public static Symbol End = new Symbol{name="__end",type=SymbolType.Internal};
		public static Symbol Return = new Symbol{name="__return",type=SymbolType.Internal};
		
		public override string ToString()
		{
			//return string.Format("{1}:{2} {0}", name, type.ToString()[0], datatype);
			return string.Format("{0}{1,5}:{2}\t{3}",type.ToString()[0],fixedAddr.GetValueOrDefault(),datatype,name);
		}
		
		public Tokens ToToken()
		{
			switch (type) {
				case SymbolType.Data:
				case SymbolType.Parameter:
				case SymbolType.Register:
					return datatype=="int"?Tokens.INTVAR:Tokens.VAR;
				case SymbolType.Function:
					return Tokens.FUNCNAME;
				case SymbolType.Array:
					return datatype=="int"?Tokens.INTARRAY:Tokens.ARRAY;
				default:
					return Tokens.error;
			}
		}

	}
	
	public struct SymbolRef
	{
		
		public int? value;
		public Symbol identifier;
		public int identifierOffset;
		public bool relative;
		
		public int resolve(int atAddr, Dictionary<Symbol,int> symbols)
		{
			if(value.HasValue){
				return this.value.Value - (relative?atAddr:0);
			} else {
				return symbols[this.identifier] + this.identifierOffset - (relative?atAddr:0);
			}			
		}
		
		public static implicit operator SymbolRef(int i){ return new SymbolRef{value=i}; }
		public static implicit operator SymbolRef(string s){ return new SymbolRef{identifier=new Symbol{name=s}}; }
		public static implicit operator SymbolRef(Symbol sym){ return new SymbolRef{identifier=sym};}
		
		public static SymbolRef operator +(SymbolRef a1, SymbolRef a2)
		{
			if (a2.value.HasValue) return a1 + a1.value.Value;
			if (a1.value.HasValue) return a2 + a1.value.Value;
			//TODO: handle more cases?
			throw new ArgumentException(string.Format("Cannot add these AddrSpecs: {0},{1}",a1,a2));
		}
		
		public static SymbolRef operator +(SymbolRef a, int i)
		{
			if (a.value.HasValue)
			{
				a.value += i;
			} else {
				a.identifierOffset+=i;
			}
			return a;
		}
		
		public override string ToString()
		{
			if (value.HasValue) {
				return string.Format("{0}",value);
			} else if(identifierOffset==0) {
				return string.Format("{0}",identifier.name);				
			} else {
				return string.Format("{0}+{1}",identifier.name,identifierOffset);				
			}
		}
	}

	
	public interface VExpr
	{
		bool IsConstant();
		Table Evaluate();
		VExpr CollapseConstants();
	}
	
	public interface SExpr
	{
		bool IsConstant();
		int Evaluate();
		SExpr CollapseConstants();
		
	}
	
	
	public class ArithSExpr: SExpr
	{
		public bool IsConstant()
		{
			return S1.IsConstant() && S2.IsConstant();
		}
		
		public int Evaluate()
		{
			if(!IsConstant())
			{
				throw new InvalidOperationException();
			}
			switch (Op) {
				case ArithSpec.Add:
					return S1.Evaluate() + S2.Evaluate();
				case ArithSpec.Subtract:
					return S1.Evaluate() - S2.Evaluate();
				case ArithSpec.Multiply:
					return S1.Evaluate() * S2.Evaluate();
				case ArithSpec.Divide:
					return S1.Evaluate() / S2.Evaluate();
				default:
					throw new InvalidOperationException();
			} 
		}
		
		public SExpr S1;
		public ArithSpec Op;
		public SExpr S2;	
		public override string ToString()
		{
			return string.Format("[ArithSExpr {0} {1} {2}]", S1, Op, S2);
		}

		public SExpr CollapseConstants()
		{
			if(this.IsConstant())
			{
				return (IntSExpr)this.Evaluate();
			} else {
				S1 = S1.CollapseConstants();
				S2 = S2.CollapseConstants();
				return this;
			}
		}
	}
	
	public class IntSExpr: SExpr
	{
		public bool IsConstant()
		{
			return true;
		}
		public int Evaluate()
		{
			return value;
		}
		public int value;
		public static implicit operator IntSExpr(int i)
		{
			return new IntSExpr{value=i};
		}
		public override string ToString()
		{
			return string.Format("[IntSExpr {0}]", value);
		}
		public SExpr CollapseConstants()
		{
			return this;			
		}
	}
	
	public class ArithVExpr: VExpr
	{
		public bool IsConstant()
		{
			return V1.IsConstant() && V2.IsConstant();
		}
		
		public Table Evaluate()
		{
			if(!IsConstant())
			{
				throw new InvalidOperationException();
			}
			switch (Op) {
				case ArithSpec.Add:
					return V1.Evaluate() + V2.Evaluate();
				case ArithSpec.Subtract:
					return V1.Evaluate() - V2.Evaluate();
				case ArithSpec.Multiply:
					return V1.Evaluate() * V2.Evaluate();
				case ArithSpec.Divide:
					return V1.Evaluate() / V2.Evaluate();
				default:
					throw new InvalidOperationException();
			} 
		}
		
		public VExpr V1;
		public ArithSpec Op;
		public VExpr V2;	
		public override string ToString()
		{
			return string.Format("[ArithVExpr {0} {1} {2}]", V1, Op, V2);
		}
		
		public VExpr CollapseConstants()
		{
			if(this.IsConstant())
			{
				return this.Evaluate();
			} else {
				V1 = V1.CollapseConstants();
				V2 = V2.CollapseConstants();
				return this;
			}
		}

	}
		
	public class ArithVSExpr: VExpr
	{
		public bool IsConstant()
		{
			return V1.IsConstant() && S2.IsConstant();
		}
		
		public Table Evaluate()
		{
			if(!IsConstant())
			{
				throw new InvalidOperationException();
			}
			switch (Op) {
				case ArithSpec.Add:
					return V1.Evaluate() + S2.Evaluate();
				case ArithSpec.Subtract:
					return V1.Evaluate() - S2.Evaluate();
				case ArithSpec.Multiply:
					return V1.Evaluate() * S2.Evaluate();
				case ArithSpec.Divide:
					return V1.Evaluate() / S2.Evaluate();
				default:
					throw new InvalidOperationException();
			} 
		}
		
		public VExpr V1;
		public ArithSpec Op;
		public SExpr S2;	
		public override string ToString()
		{
			return string.Format("[ArithVSExpr {0} {1} {2}]", V1, Op, S2);
		}

		public VExpr CollapseConstants()
		{
			if(this.IsConstant())
			{
				return this.Evaluate();
			} else {
				V1 = V1.CollapseConstants();
				S2 = S2.CollapseConstants();
				return this;
			}
		}
	}
	
	public class StringVExpr: VExpr
	{
		public bool IsConstant()
		{
			return true;
		}
		public Table Evaluate()
		{
			return new Table(text);
		}
		public string text;
		public static implicit operator StringVExpr(string s)
		{
			return new StringVExpr{text=s};
		}
		public override string ToString()
		{
			return string.Format("[StringVExpr {0}]", text);
		}
		public VExpr CollapseConstants()
		{
			return this;			
		}
	}
	
	public interface SRef : SExpr // the assignable subset of scalar expressions
	{
		//void Assign(SExpr value);
	}
	
	public class IntVarSRef: SRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public int Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public string name;
		public override string ToString()
		{
			return string.Format("[IntVarSRef {0}]", name);
		}

		public SExpr CollapseConstants()
		{
			return this;			
		}
	}
	
	public class FieldSRef: SRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public int Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public string varname;
		public string fieldname;
		public override string ToString()
		{
			return string.Format("[FieldSRef {0}.{1}]", varname, fieldname);
		}
		public SExpr CollapseConstants()
		{
			return this;			
		}

	}
	
	public interface VRef: VExpr // the assignable subset of vector expressions
	{
		
	}
	
	public class VarVRef: VRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public Table Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public string name;
		public override string ToString()
		{
			return string.Format("[VarVRef {0}]", name);
		}
		
		public VExpr CollapseConstants()
		{
			return this;			
		}

	}
	
	public class ArrayVRef: VRef
	{
		public bool IsConstant()
		{
			return false;
		}
		public Table Evaluate()
		{
			throw new InvalidOperationException(); 
		}
		public string arrname;
		public SExpr offset;
		public override string ToString()
		{
			return string.Format("[ArrayVRef {0}+{1}]", arrname, offset);
		}

		public VExpr CollapseConstants()
		{
			return this;			
		}
	}
	
	public class Table:Dictionary<string,SExpr>, VExpr
	{
		public bool IsConstant()
		{
			return this.All((ti) => ti.Value.IsConstant());
		}
		public Table Evaluate()
		{
			return this; 
		}
		public string datatype;
		public void Add(TableItem ti)
		{
			this.Add(ti.fieldname,ti.value);
		}
		
		public Table():base(){}
		public Table(string text)
		{
			//TODO: build a string table here
			var chars = new Dictionary<char,int>();
			int i = 0;
			foreach (var c in text) {
				if(!chars.ContainsKey(c))chars.Add(c,0);
				chars[c]+=1<<i++;
			}
			
			foreach (var c in chars) {
				if(c.Key == ' ') continue;
				this.Add(new TableItem(c.Key,(IntSExpr)c.Value));
			}
		}
		
		public static Table Asm(int op, int r1, int s1, int r2, int s2, int rd, int sd)
		{
			return new Table{
				{"signal-0",(IntSExpr)op},
				{"signal-R",(IntSExpr)r1},
				{"signal-S",(IntSExpr)s1},
				{"signal-T",(IntSExpr)r2},
				{"signal-U",(IntSExpr)s2},
				{"signal-V",(IntSExpr)rd},
				{"signal-W",(IntSExpr)sd},
			};
		}
		
		public static Table operator +(Table t1, Table t2)
		{
			var tres = new Table();
			foreach (var key in t1.Keys.Union(t2.Keys)) {
				SExpr eres;
				if(!t2.ContainsKey(key)){
					eres=t1[key];
				} else if(!t1.ContainsKey(key)){
					eres=t2[key];
				} else {
					eres = new ArithSExpr{S1=t1[key],Op=ArithSpec.Add,S2=t2[key]};
				}				
				tres.Add(key,eres);
			}
			return tres;
		}
		public static Table operator -(Table t1, Table t2)
		{
			var tres = new Table();
			foreach (var key in t1.Keys.Union(t2.Keys)) {
				SExpr eres;
				if(!t2.ContainsKey(key)){
					eres=t1[key];
				} else if(!t1.ContainsKey(key)){
					eres=t2[key];
				} else {
					eres = new ArithSExpr{S1=t1[key],Op=ArithSpec.Subtract,S2=t2[key]};
				}				
				tres.Add(key,eres);
			}
			return tres;
		}
		public static Table operator *(Table t1, Table t2)
		{
			var tres = new Table();
			foreach (var key in t1.Keys.Union(t2.Keys)) {
				SExpr eres;
				if(!t2.ContainsKey(key)){
					eres=t1[key];
				} else if(!t1.ContainsKey(key)){
					eres=t2[key];
				} else {
					eres = new ArithSExpr{S1=t1[key],Op=ArithSpec.Multiply,S2=t2[key]};
				}				
				tres.Add(key,eres);
			}
			return tres;
		}
		public static Table operator /(Table t1, Table t2)
		{
			var tres = new Table();
			foreach (var key in t1.Keys.Union(t2.Keys)) {
				SExpr eres;
				if(!t2.ContainsKey(key)){
					eres=t1[key];
				} else if(!t1.ContainsKey(key)){
					eres=t2[key];
				} else {
					eres = new ArithSExpr{S1=t1[key],Op=ArithSpec.Divide,S2=t2[key]};
				}				
				tres.Add(key,eres);
			}
			return tres;
		}
		
		public static Table operator +(Table t, int i){return t+(IntSExpr)i;}
		public static Table operator -(Table t, int i){return t-(IntSExpr)i;}
		public static Table operator *(Table t, int i){return t*(IntSExpr)i;}
		public static Table operator /(Table t, int i){return t/(IntSExpr)i;}
		
		public static Table operator +(Table t, SExpr s)
		{
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key,new ArithSExpr{S1=ti.Value,Op=ArithSpec.Add,S2=s});
			}
			return tres;
		}
		public static Table operator -(Table t, SExpr s)
		{
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key,new ArithSExpr{S1=ti.Value,Op=ArithSpec.Subtract,S2=s});
			}
			return tres;
		}
		public static Table operator *(Table t, SExpr s)
		{
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key,new ArithSExpr{S1=ti.Value,Op=ArithSpec.Divide,S2=s});
			}
			return tres;
		}
		public static Table operator /(Table t, SExpr s)
		{
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key,new ArithSExpr{S1=ti.Value,Op=ArithSpec.Multiply,S2=s});
			}
			return tres;
		}
		
		public override string ToString()
		{
			return string.Format("[Table {0}:{1}]", datatype, this.Count);
		}
		public VExpr CollapseConstants()
		{
			
			return (Table)this.Select(kv=>new KeyValuePair<string,SExpr>(kv.Key,kv.Value.CollapseConstants()));
		}
		
	}
	
	public class TableItem
	{
		public string fieldname;
		public SExpr value;
		public override string ToString()
		{
			return string.Format("[{0}={1}]", fieldname, value);
		}
		public TableItem(string fieldname, SExpr value)
		{
			this.fieldname = fieldname;
			this.value = value;
		}
		
		public TableItem(char c, SExpr value)
		{
			Dictionary<char,string> charmap = new Dictionary<char, string>{
				{'1',"signal-1"},{'2',"signal-2"},{'3',"signal-3"},{'4',"signal-4"},{'5',"signal-5"},
	        	{'6',"signal-6"},{'7',"signal-7"},{'8',"signal-8"},{'9',"signal-9"},{'0',"signal-0"},
	        	{'A',"signal-A"},{'B',"signal-B"},{'C',"signal-C"},{'D',"signal-D"},{'E',"signal-E"},
				{'F',"signal-F"},{'G',"signal-G"},{'H',"signal-H"},{'I',"signal-I"},{'J',"signal-J"},
				{'K',"signal-K"},{'L',"signal-L"},{'M',"signal-M"},{'N',"signal-N"},{'O',"signal-O"},
				{'P',"signal-P"},{'Q',"signal-Q"},{'R',"signal-R"},{'S',"signal-S"},{'T',"signal-T"},
				{'U',"signal-U"},{'V',"signal-V"},{'W',"signal-W"},{'X',"signal-X"},{'Y',"signal-Y"},
				{'Z',"signal-Z"},{'-',"fast-splitter"},{'.',"train-stop"},
			};
			
			this.fieldname = charmap[c];
			this.value = value;
		}

	}
	
	public interface Statement
	{
		void Print(string prefix);
		void CollapseConstants();
	}
	
	public class VAssign:Statement
	{
		public VRef target;
		public bool append;
		public VExpr source;
		public override string ToString()
		{
			return string.Format("[VAssign {0} {1} {2}]", target, append?"+=":"=", source);
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		public void CollapseConstants()
		{
			source = source.CollapseConstants();
		}

	}
	public class SAssign:Statement
	{
		public SRef target;
		public bool append;
		public SExpr source;
		public override string ToString()
		{
			return string.Format("[SAssign {0} {1} {2}]", target, append?"+=":"=", source);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
		}
		
		public void CollapseConstants()
		{
			source = source.CollapseConstants();
		}

	}
	
	public class Branch
	{
		public SExpr S1;
		public CompSpec Op;
		public SExpr S2;
		public override string ToString()
		{
			return string.Format("[Branch S1={0}, Op={1}, S2={2}]", S1, Op, S2);
		}
		
		public void CollapseConstants()
		{
			S1 = S1.CollapseConstants();
			S2 = S2.CollapseConstants();
		}
	}
	
	public class If:Statement
	{
		public Branch branch;
		public Block ifblock;
		public Block elseblock;
		public override string ToString()
		{
			return string.Format("[If Branch={0}]", branch, ifblock, elseblock);
		}
		
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
			ifblock.Print(prefix +"  ");
			if(elseblock!=null)
			{
				Console.WriteLine(prefix+"Else");
				elseblock.Print(prefix+"  ");					
			}
		}
		
		public void CollapseConstants()
		{
			branch.CollapseConstants();
			if(ifblock!=null)ifblock.CollapseConstants();
			if(elseblock!=null)elseblock.CollapseConstants();
		}

	}
	
	public class While:Statement
	{
		public Branch branch;
		public Block body;
		public override string ToString()
		{
			return string.Format("[While Branch={0}]", branch);
		}
		public void Print(string prefix)
		{
			Console.WriteLine("{1}{0}", this, prefix);
			body.Print(prefix +"  ");
		}

		public void CollapseConstants()
		{
			branch.CollapseConstants();
			if(body!=null)body.CollapseConstants();
		}
	}
	
	public class Block:List<Statement>
	{
		public override string ToString()
		{
			return string.Format("[Block {0}]",string.Join(";",this));
		}
		
		public void Print(string prefix)
		{
			foreach (var statement in this) {
				if(statement != null) statement.Print(prefix);
			}
		}
		
		public void CollapseConstants()
		{
			foreach (var statement in this) {
				if(statement != null) statement.CollapseConstants();
			}
		}
		
	}
	
	public struct DataItem
	{
		public SignalSpec signal;
		public SymbolRef addr;
		
		public DataItem(char c,SymbolRef addr):this(new SignalSpec(c),addr){}
		public DataItem(SignalSpec signal, SymbolRef addr)
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