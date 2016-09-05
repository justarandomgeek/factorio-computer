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
			var newsymbols = new Dictionary<SymbolDef,int>();
			foreach (var sym in symbols.Keys) {
				newsymbols[sym]= symbols[sym] +newOffset -Offset;
			}
			symbols=newsymbols;
			Offset = newOffset;
		}
		
		public Dictionary<SymbolDef,int> symbols = new Dictionary<SymbolDef, int>();
				
		public static StatementList Function(string name,StatementList body)
		{
			var funcsym = new SymbolDef{name = name, type = SymbolType.Function};
			var sl = new StatementList();
			sl.symbols.Add(funcsym,0);
			sl.Add(DataList.Push(1,RegSpec.r8));
			
			if (!sl.symbols.ContainsKey(SymbolDef.Block)) sl.symbols.Add(SymbolDef.Block,0);
			sl.Add(body);
			
			sl.symbols.Add(SymbolDef.Return,sl.Count);
			sl.Add(DataList.Pop(1,RegSpec.r8));
			sl.Add(DataList.Jump(new SignalSpec(RegSpec.r8,"signal-green"),false,false));
			
			sl.RewriteSymbol(SymbolDef.Block,funcsym);
			sl.RewriteSymbol(SymbolDef.Return,funcsym);
			sl.symbols.Remove(SymbolDef.Block);
			sl.symbols.Remove(SymbolDef.Return);
			
			return sl;
		}
		
		public static StatementList If(DataList ifcomp, StatementList trueblock, StatementList falseblock)
		{
			
			var sl = new StatementList();
			sl.symbols.Add(SymbolDef.Block,0);
			sl.Add(ifcomp);
			
			trueblock.symbols.Add(SymbolDef.TrueBlock,0);
			sl.Add(trueblock);
			
			
			if(falseblock != null)
			{
				sl.Add(DataList.Jump(SymbolDef.End,true,false));
				falseblock.symbols.Add(SymbolDef.FalseBlock,0);
				sl.Add(falseblock);
			} else  {
				sl.symbols.Add(SymbolDef.FalseBlock,sl.Count);
			}
			sl.symbols.Add(SymbolDef.End,sl.Count);
			sl.RewriteSymbol(SymbolDef.TrueBlock,SymbolDef.Block);
			sl.RewriteSymbol(SymbolDef.FalseBlock,SymbolDef.Block);
			sl.RewriteSymbol(SymbolDef.End,SymbolDef.Block);
			
			sl.symbols.Remove(SymbolDef.TrueBlock);
			sl.symbols.Remove(SymbolDef.FalseBlock);
			sl.symbols.Remove(SymbolDef.End);
			
			return sl;
		}
		
		public static StatementList While(DataList ifcomp, StatementList loopblock)
		{
			var sl = new StatementList();
			var s = ifcomp.ToString();
			sl.symbols.Add(SymbolDef.Loop,0);
			sl.symbols.Add(SymbolDef.Block,0);
			sl.Add(ifcomp);
			loopblock.symbols.Add(SymbolDef.TrueBlock,0);
			sl.Add(loopblock);
			sl.Add(DataList.Jump(SymbolDef.Loop,true,false));
			sl.symbols.Add(SymbolDef.FalseBlock,sl.Count);
			sl.symbols.Add(SymbolDef.End,sl.Count);
			
			sl.RewriteSymbol(SymbolDef.TrueBlock,SymbolDef.Block);
			sl.RewriteSymbol(SymbolDef.FalseBlock,SymbolDef.Block);
			sl.RewriteSymbol(SymbolDef.Loop,SymbolDef.Block);
			sl.RewriteSymbol(SymbolDef.End,SymbolDef.Block);
			
			sl.symbols.Remove(SymbolDef.TrueBlock);
			sl.symbols.Remove(SymbolDef.FalseBlock);
			sl.symbols.Remove(SymbolDef.Loop);
			sl.symbols.Remove(SymbolDef.End);
			
			return sl;
		}
		
		
		public void Add(StatementList otherlist)
		{
			int newbase = this.Count;
			
			foreach (var statement in otherlist) {
		     	var st = new DataList();
             	foreach (var item in statement) {
					if (item.Value.identifier==SymbolDef.Block)
					{
						st.Add(item.Key,item.Value+newbase);
					} else {
						st.Add(item.Key,item.Value);
					}
				}
		     	this.Add(st);
			}
			
				
			otherlist.symbols.Remove(SymbolDef.Block);
			foreach (var sym in otherlist.symbols.Keys) {
				this.symbols.Add(sym,otherlist.symbols[sym] + newbase);
			}		
		}
		
		
		public void RewriteSymbol(SymbolDef sym, SymbolDef refsym)
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
								new AddrSpec{
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