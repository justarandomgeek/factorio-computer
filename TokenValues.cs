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

	
	[Flags]
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
		int paramcount;
		public void AddParam(FieldInfo pi)
		{
			this.Add(new Symbol{
			         	name=pi.name,
			         	type=SymbolType.Parameter,
			         	datatype=pi.basename,
			         	fixedAddr=paramcount++,
			         });
		}
	
		
	}
	
	public class FunctionInfo{
		public string name;
		public SymbolList locals = new SymbolList();
		public TypeInfo localints = new TypeInfo();
		public Block body;
		
		public void AllocateLocals()
		{
			int nextReg = 6;
			foreach (var p in locals.Where(sym=>sym.type == SymbolType.Parameter && sym.datatype == "int")) {
				localints.Add(p.name,"signal-" + p.fixedAddr.GetValueOrDefault()); //TODO: cleaner assignments
			}
			foreach (var localint in localints) {
				if (localint.Value == null) {
					//TODO: allocate ints in __localtype. maybe skip a few for argument/return passing?
				}
			}
			var newlocals = new SymbolList();
			newlocals.AddRange(locals.Select((symbol) =>
				{
					if (symbol.type == SymbolType.Register && !symbol.fixedAddr.HasValue) {
						//TODO: fix this to actually confirm a register is available
						//TODO: fix this to switch to stack frame allocation when nextReg hits r2
						symbol.fixedAddr = nextReg--;
					}
				    return symbol;
				}));
			locals = newlocals;
		}
		
	}
		
	public enum SymbolType{
		Function=1, 
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
		public List<Table> data;
		
		public static Symbol Block = new Symbol{name="__block",type=SymbolType.Internal};
		public static Symbol TrueBlock = new Symbol{name="__trueblock",type=SymbolType.Internal};
		public static Symbol FalseBlock = new Symbol{name="__falseblock",type=SymbolType.Internal};
		public static Symbol Loop = new Symbol{name="__loop",type=SymbolType.Internal};
		public static Symbol End = new Symbol{name="__end",type=SymbolType.Internal};
		public static Symbol Return = new Symbol{name="__return",type=SymbolType.Internal};
		
		public override string ToString()
		{
			//return string.Format("{1}:{2} {0}", name, type.ToString()[0], datatype);
			return string.Format("{0}{1,5}:{2}\t{3}",type.ToString()[0],fixedAddr,datatype,name);
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
	
	public class Instruction //:Statement
	{
		public int op;
				
		public bool acc;
		public int index;
				
		public int r1;
		public string s1;
		
		public int r2;
		public string s2;
		
		public int rd;
		public string sd;
		
		public int? imm1;
		public string imm1sym;
		
		public int? imm2;
		public string imm2sym;
		
		public int? addr1;
		public string addr1sym;
		
		public int? addr2;
		public string addr2sym;
		
		public int? addr3;
		public string addr3sym;
	}
	
	public class Table:Dictionary<string,SExpr>, VExpr
	{
		public bool IsConstant()
		{
			return this.All((ti) => ti.Value.IsConstant());
		}
		public Table Evaluate()
		{
			//TODO: do type mapping for type->var conversion here?
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
			return string.Format("[{0}:{1}]", datatype, this.Count);
		}
		
		public VExpr FlattenExpressions()
		{
			return (Table)this.Select(kv=>new KeyValuePair<string,SExpr>(kv.Key,kv.Value.FlattenExpressions()));
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
		
		public Block()
		{
			
		}
		public Block(Statement s)
		{
			this.Add(s);
		}
		
		public Block Flatten()
		{
			Block flatblock = new Block();
			foreach (var element in this) {
				if(element != null) flatblock.AddRange(element.Flatten());
			}
			return flatblock;
		}
		
	}
	
	
	
	
}