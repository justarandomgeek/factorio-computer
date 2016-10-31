/*
 * Created by SharpDevelop.
 * User: Thomas
 * Date: 2016-09-05
 * Time: 10:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace compiler
{
	public class StatementList: List<DataList>
	{	
		public int Offset {get; private set;}
		public void Relocate(int newOffset)
		{
			var newsymbols = new Dictionary<Symbol,int>();
			foreach (var sym in symbols.Keys) {
				newsymbols[sym]= symbols[sym] +newOffset -Offset;
			}
			symbols=newsymbols;
			Offset = newOffset;
		}
		
		public Dictionary<Symbol,int> symbols = new Dictionary<Symbol, int>();
				
		public static StatementList Function(string name,StatementList body)
		{
			var funcsym = new Symbol{name = name, type = SymbolType.Function};
			var sl = new StatementList();
			sl.symbols.Add(funcsym,0);
			sl.Add(DataList.Push(1,8));
			
			if (!sl.symbols.ContainsKey(Symbol.Block)) sl.symbols.Add(Symbol.Block,0);
			sl.Add(body);
			
			sl.symbols.Add(Symbol.Return,sl.Count);
			sl.Add(DataList.Pop(1,8));
			sl.Add(DataList.Jump(new SignalSpec(8,"signal-green"),false,false));
			
			sl.RewriteSymbol(Symbol.Block,funcsym);
			sl.RewriteSymbol(Symbol.Return,funcsym);
			sl.symbols.Remove(Symbol.Block);
			sl.symbols.Remove(Symbol.Return);
			
			return sl;
		}
		
		public static StatementList If(DataList ifcomp, StatementList trueblock, StatementList falseblock)
		{
			
			var sl = new StatementList();
			sl.symbols.Add(Symbol.Block,0);
			sl.Add(ifcomp);
			
			trueblock.symbols.Add(Symbol.TrueBlock,0);
			sl.Add(trueblock);
			
			
			if(falseblock != null)
			{
				sl.Add(DataList.Jump(Symbol.End,true,false));
				falseblock.symbols.Add(Symbol.FalseBlock,0);
				sl.Add(falseblock);
			} else  {
				sl.symbols.Add(Symbol.FalseBlock,sl.Count);
			}
			sl.symbols.Add(Symbol.End,sl.Count);
			sl.RewriteSymbol(Symbol.TrueBlock,Symbol.Block);
			sl.RewriteSymbol(Symbol.FalseBlock,Symbol.Block);
			sl.RewriteSymbol(Symbol.End,Symbol.Block);
			
			sl.symbols.Remove(Symbol.TrueBlock);
			sl.symbols.Remove(Symbol.FalseBlock);
			sl.symbols.Remove(Symbol.End);
			
			return sl;
		}
		
		public static StatementList While(DataList ifcomp, StatementList loopblock)
		{
			var sl = new StatementList();
			var s = ifcomp.ToString();
			sl.symbols.Add(Symbol.Loop,0);
			sl.symbols.Add(Symbol.Block,0);
			sl.Add(ifcomp);
			loopblock.symbols.Add(Symbol.TrueBlock,0);
			sl.Add(loopblock);
			sl.Add(DataList.Jump(Symbol.Loop,true,false));
			sl.symbols.Add(Symbol.FalseBlock,sl.Count);
			sl.symbols.Add(Symbol.End,sl.Count);
			
			sl.RewriteSymbol(Symbol.TrueBlock,Symbol.Block);
			sl.RewriteSymbol(Symbol.FalseBlock,Symbol.Block);
			sl.RewriteSymbol(Symbol.Loop,Symbol.Block);
			sl.RewriteSymbol(Symbol.End,Symbol.Block);
			
			sl.symbols.Remove(Symbol.TrueBlock);
			sl.symbols.Remove(Symbol.FalseBlock);
			sl.symbols.Remove(Symbol.Loop);
			sl.symbols.Remove(Symbol.End);
			
			return sl;
		}
		
		
		public void Add(StatementList otherlist)
		{
			int newbase = this.Count;
			
			foreach (var statement in otherlist) {
		     	var st = new DataList();
             	foreach (var item in statement) {
					if (item.Value.identifier==Symbol.Block)
					{
						st.Add(item.Key,item.Value+newbase);
					} else {
						st.Add(item.Key,item.Value);
					}
				}
		     	this.Add(st);
			}
			
				
			otherlist.symbols.Remove(Symbol.Block);
			foreach (var sym in otherlist.symbols.Keys) {
				this.symbols.Add(sym,otherlist.symbols[sym] + newbase);
			}		
		}
		
		
		public void RewriteSymbol(Symbol sym, Symbol refsym)
		{
			for (int i = 0; i < this.Count; i++) {
				var statement = this[i];
				
				var keys = new SignalSpec[statement.Keys.Count];
				statement.Keys.CopyTo(keys,0);
				foreach (var key in keys) {
					if(statement[key].identifier == sym)
					{
						if(statement[key].relative){
							statement[key]=statement[key].resolve(this.IndexOf(statement), this.symbols);	
						} else {
							statement[key]=
								new SymbolRef{
								identifier = refsym,
								identifierOffset = statement[key].resolve(this.IndexOf(statement), this.symbols) - this.symbols[refsym]
							};
						}
						
					}
				}
			}
		}
		
	}

}