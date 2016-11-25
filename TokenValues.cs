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
		int paramcount=1;
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
				localints.Add(p.name,"signal-" + p.fixedAddr);
			}
			foreach (var localint in localints) {
				if (localint.Value == null) {
					//TODO: allocate ints in __liFUNC. maybe skip a few for argument/return passing?
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
		
		public Block BuildFunction()
		{
			var b = new Block();
			
			
			// save parent localints
			b.Add(new Push{reg = new RegVRef{reg=2},stack=1});
			
			// save call site (in r8.signal-0)
			b.Add(new Push{reg = new RegVRef{reg=8},stack=1});
			
			
			// push regs as needed
			foreach(var sym in locals.Where(s=>s.type==SymbolType.Register))
			{
				if(sym.fixedAddr.HasValue) b.Add(new Push{reg = new RegVRef{reg=sym.fixedAddr.Value},stack=1});
			}
			
			// copy params if named
			
			// body
			b.AddRange(body.Flatten());
			
			// convert rjmp __return => rjmp <integer> to here.
			for (int i = 0; i < b.Count; i++) {
				var j = b[i] as Jump;
				if (j!= null && j.relative && j.target is AddrSExpr && ((AddrSExpr)j.target).symbol=="__return")
				{
					j.target = new IntSExpr{value=b.Count-i};
				}
			}
			
			// restore registers
			foreach(var sym in locals.Where(s=>s.type==SymbolType.Register).Reverse())
			{
				if(sym.fixedAddr.HasValue) b.Add(new Pop{reg = new RegVRef{reg=sym.fixedAddr.Value},stack=1});
			}
			
			// get return site
			b.Add(new Pop{reg = new RegVRef{reg=2},stack=1}); // this was r8 originally
			b.Add(new SAssign{target = new FieldSRef{varref=new RegVRef{reg=8},fieldname="signal-0"},source=new FieldSRef{varref=new RegVRef{reg=2},fieldname="signal-0"},append=true});
			
			// restore parent localints
			b.Add(new Pop{reg = new RegVRef{reg=2},stack=1});
			
			// jump to return site
			b.Add(new Jump{target = new FieldSRef{varref=new RegVRef{reg=8},fieldname="signal-0"} });
			
			return b;
		}
	}
		
	public enum SymbolType{
		Function=1, 
		Data,
		Constant,
		Register,
		Program,
		Internal,
		Parameter,
	}
	
	public struct Symbol
	{
		public string name;
		public SymbolType type;
		public string datatype;
		public int? fixedAddr;
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
						return data!=null?(int?)data.Count:null;
				}
			}
			set
			{
				declsize=value;
			}
		}
		private int? declsize;
		public List<Table> data;
		
		public override string ToString()
		{
			//return string.Format("{1}:{2} {0}", name, type.ToString()[0], datatype);
			return string.Format("{0}{1,5}:{4,-3} {2,10} {3}",type.ToString()[0],fixedAddr,datatype,name,size);
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
					return Tokens.FUNCNAME;
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