using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Web.Script.Serialization;
using System.Reflection;
using NLua;
using SourceRconLib;
using CommandLine;
using nql.Blueprints;

namespace nql
{

	partial class Parser
    {
        new Scanner Scanner { get { return (Scanner)base.Scanner; } set { base.Scanner = value; } }

        public List<string> NativeFields = new List<string>();
		public Dictionary<string, string> NativeFieldTypes = new Dictionary<string, string>();
		public Dictionary<char, string> charmap = new Dictionary<char, string>();
		public Dictionary<string,TypeInfo> Types = new Dictionary<string, TypeInfo>();
        public Dictionary<string,FunctionInfo> Functions = new Dictionary<string, FunctionInfo>();
        public SymbolList Symbols = new SymbolList();
		public List<Extern> Externs = new List<Extern>();
        
		// Parser state used by lexer
        private string ExpectFieldType { get; set; }
        internal FunctionInfo InFunction { get; private set; }
		private Extern InExtern { get; set; }
        
        public List<Table> romdata;
        
        public string Name {get; set;}
                
		public void ParseFile(string file)
		{
			string prog = "";
			using (var reader = new StreamReader(file))
			{
				prog = reader.ReadToEnd();
			}
			Parse(prog);
		}

		public void RegisterType(string typename, TypeInfo typeinfo)
        {
        	(InExtern?.Types ?? Types).Add(typename,typeinfo);
        }
        
        public void CreateSym(Symbol sym)
        {
			(InExtern?.Symbols ?? InFunction?.locals ?? Symbols).Add(sym);			
        }
        
        public void CreateInt(string name)
        {
			if(InExtern != null)
			{
				throw new NotImplementedException(); //TODO: how to do extern ints? or just declare it not legal?
			} else if (InFunction != null) {
				InFunction.locals.Add(new Symbol { name = name, datatype="int", type = SymbolType.Register });
        		
        	} else {
        		if(!Types.ContainsKey("__globalints")){Types.Add("__globalints",new TypeInfo());}
        		Types["__globalints"].Add(name,null);
        	}
        }

		public void BeginExtern(string name)
		{
			InExtern = new Extern(name);
		}
		public void CompleteExtern()
		{
			Externs.Add(InExtern);
			InExtern = null;
		}

		public void ExternFunction(string name, string returntype, SymbolList paramlist)
		{
			InExtern.Functions.Add(new FunctionInfo { name = name, returntype = returntype, locals = paramlist });
		}


		public void BeginFunction(string name, string returntype, SymbolList paramlist)
        {
        	InFunction = new FunctionInfo { name = name, returntype = returntype, locals = paramlist };
        }
        
        public void CompleteFunction(string name, Block body)
        {
			InFunction.body = body;
			Functions.Add(name,InFunction);
			InFunction=null;
        }
        
		public void AllocateTypes()
		{
			foreach (var typename in Types.Keys)
			{
				Types[typename].Allocate();
			}
		}


        public void AllocateSymbols()
        {
			// 0 in both frames is reserved. 
			// 0Const has progsym
			// 0Data  is reserved for future use by memory allocator or other system services.
			int constaddr=1;
			romdata = new List<Table>();

			// leave space for symtable starting at 1
			int symtsize = Symbols.Count(sym =>sym.name.All(c=>charmap.Keys.Contains(c)));
			constaddr += symtsize;


			int dataaddr=1;
			var newsyms = new SymbolList();
			foreach (var symbol in Symbols.OrderByDescending(s=>s.type)) {
				switch (symbol.type) {
					case SymbolType.Constant:
					case SymbolType.Function:
						symbol.assign(constaddr);
						romdata.AddRange(symbol.data);
						constaddr+=symbol.size??1;
						break;
						
					case SymbolType.Data:
						if(!symbol.fixedAddr.HasValue)
						{
							symbol.assign(dataaddr);
							dataaddr+=symbol.size??1;
						}
						break;
				}
				newsyms.Add(symbol);
			}
			Symbols= newsyms;

			var progsym = new Table(Name);
			progsym.Add("symtsize", symtsize);
			progsym.Add("romsize", romdata.Count + 1);
			progsym.Add("datasize", dataaddr);
			progsym.Add("mainloc", new AddrSExpr("MAIN"));
			
			progsym.datatype = "progsym";
										
			romdata.Insert(0,progsym);

			romdata.InsertRange(
				1,
				Symbols.Where(
					sym => sym.name.All(c => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(c))
					).Select(
					sym =>
					{
						var symtable = new Table(sym.name);
						symtable.datatype = "symbol";

						//DATA = 1 FUNC = 2 CONST = 3
						switch (sym.type)
						{
							case SymbolType.Data:
								symtable.Add("symtype", 1);
								break;
							case SymbolType.Function:
								symtable.Add("symtype", 2);
								break;
							case SymbolType.Constant:
								symtable.Add("symtype", 3);
								break;
							default:
								throw new NotImplementedException();
						}

						symtable.Add("addr", sym.fixedAddr ?? 0);
						symtable.Add("size", sym.size ?? 0);
						
						return symtable;
					})
				);



		}

		
		public Dictionary<string, Func<FunctionCall, RegVRef, List<Instruction>>> VBuiltins = new Dictionary<string, Func<FunctionCall, RegVRef, List<Instruction>>>{
			{ "playerinfo", (fcall,dest) => {
				// summary = playerinfo()
				// player = playerinfo(int player_index)
				var b = new List<Instruction>();

				FieldSRef arg = fcall.args.ints[0] as FieldSRef;
				if(arg == null)
				{
					if ( fcall.args.ints[0] is IntVarSRef)
					{
						arg = ((IntVarSRef)fcall.args.ints[0]).BaseField();
					}
					else
					{
						arg = FieldSRef.ScratchInt();
						b.AddRange(fcall.args.ints[0].FetchToField(arg));
					}
				}

				b.Add(new Instruction {
					opcode = Opcode.PlayerInfo,
					
				});
				return b;

			} },
			{ "memexchange", (fcall,dest) => {
				// prevdata = memexchange(newdata,addr,frame)
				// 
				var b = new List<Instruction>();
				b.Add(new Exchange(
					fcall.args.vars[0] as RegVRef,
					dest,
					(PointerIndex)fcall.args.ints[1].Evaluate(),
					fcall.args.ints[0]
					));
				return b;
			}},
			{ "conman", (fcall,dest) => {
				// result = conman(command,data)
				var b = new List<Instruction>();
				b.AddRange(
					new ConManVExpr(
						fcall.args.vars[0],
						fcall.args.vars.Count>=2?fcall.args.vars[1]:RegVRef.rNull
						).FetchToReg(dest));
				return b;
			}},
			{ "scammer", (fcall,dest) => {
				// found = scammer(command)
				var b = new List<Instruction>();
				b.AddRange(
					new ScammerVExpr(
						fcall.args.vars[0],
						fcall.args.vars.Count>=2?fcall.args.vars[1]:RegVRef.rNull
						).FetchToReg(dest));
				return b;
			}},
			{ "halt", (fcall,dest) => {
				// scammer()
				return new List<Instruction> {
					new Instruction { opcode = Opcode.Halt }
				};
			}},
			};

		public Dictionary<string, Func<FunctionCall, Block>> SBuiltins = new Dictionary<string, Func<FunctionCall, Block>>{
			//{ "sum", fcall=> {
			//	Block b = new Block();
			//	b.Add(new SAssign {target = fcall.sreturn, source = FieldSRef.VarField((VRef)fcall.args.var,"signal-each") });
			//	return b;
			//}},

			};

		public Tokens GetIdentType(string ident)
        {
			if (ExpectFieldType != null)
        	{
        		if ( ExpectFieldType != "var" && Types[ExpectFieldType].ContainsKey(ident) )
        		{
					ExpectFieldType = null;
					return Tokens.FIELD;	
        		}

				ExpectFieldType = null;
			}

			if (ident == "nil")
			{
				return Tokens.FIELD;
			}
			else if (SBuiltins.ContainsKey(ident))
			{
				return Tokens.SFUNCNAME;
			}
			else if (VBuiltins.ContainsKey(ident))
			{
				return Tokens.VFUNCNAME;
			}
			else if (Types.ContainsKey(ident))
        	{
        		return Tokens.TYPENAME;
        	}
        	else if (NativeFields.Contains(ident))
			{
				return Tokens.FIELD;
			}
        	else if (InFunction != null && InFunction.locals.Exists((s) => s.name == ident))
        	{
        		return InFunction.locals.Find((s) => s.name == ident).ToToken();
        	}
        	else if(Symbols.Exists((s) => s.name == ident))
        	{
        		return Symbols.Find((s) => s.name == ident).ToToken();        		
        	}
        	else if (Types.ContainsKey("__globalints") && Types["__globalints"].ContainsKey(ident))
        	{
        		return Tokens.INTVAR;
        	}
        	else if (Functions.ContainsKey(ident))
        	{
				return Functions[ident].returntype == "int" ? Tokens.SFUNCNAME : Tokens.VFUNCNAME;
        	}
        	else
			{
				return Tokens.UNDEF;
			}
        }
          
        
        public void ReadMachineDesc()
        {
			Lua lua = new Lua();
			var serpent = lua.DoString(Program.GetResourceText("serpent"), "serpent");
			lua["serpent"] = serpent[0];

			string maptext;

			var options = Options.Current;

			if(options.map != null)
			{
				using (var reader = new StreamReader(options.map))
				{
					maptext = reader.ReadToEnd();
				}
			}
			else
			{
				maptext = Program.GetResourceText("scalarmap");
			}
			
			var signalmap = (LuaTable)lua.DoString(maptext,"scalarmap")[0];
			foreach (LuaTable sig in signalmap.Values) {
        		var name = sig["name"] as string;
        		var id = (int)(double)sig["id"];
        		var type = sig["type"] as string;
				var ch = sig["char"] as string;

        		NativeFieldTypes[name]=type;
        		NativeFields.Add(name);
				if (ch!=null && ch.Length==1) this.charmap[ch[0]] = name;
        	}
        }
                
        public void MakeROM()
        {
			var b = new Blueprint();

			var addrsignal = new SignalID { name = "signal-black", type = "virtual" };
			var sigeverything = new SignalID { name = "signal-everything", type = "virtual" };
			var sigeach = new SignalID { name = "signal-each", type = "virtual" };

			b.icons[0] = new SignalID { name = "arithmetic-combinator", type = "item" };
			b.icons[1] = new SignalID { name = "decider-combinator", type = "item" };
			b.entities.Add(new PowerPole
			{
				name = "medium-electric-pole",
				position = { x = 0, y = 0 }
			});
			b.entities.Add(new ConstantCombinator
			{
				name = "constant-combinator",
				position = { x = 0, y = -1 },
				direction = 4,
				filters = { new Filter { count = -1000, signal = addrsignal } }
			});

			var memreqwire = new CircuitConnection { entity_id = 1, circuit_id = 1 };
			var memrepwire = new CircuitConnection { entity_id = 1, circuit_id = 1 };
			var baseaddrwire = new CircuitConnection { entity_id = 2, circuit_id = 1 };

			for (int i = 0; i < this.romdata.Count; i++)
			{
				var dc = new DeciderCombinator
				{
					name = "decider-combinator",
					position = { x = 1 + i, y = 1.5 },
					direction = 4,
					first_signal = addrsignal,
					comparator = "=",
					output_signal = sigeverything,
					copy_count_from_input = true,
					connections = {
						["1"] = new CircuitPort{ green = { memreqwire } },
						["2"] = new CircuitPort{ red = { memrepwire } },
					},
				};

				b.entities.Add(dc);
				memreqwire = new CircuitConnection { entity_id = dc.entity_number, circuit_id = 1 };
				memrepwire = new CircuitConnection { entity_id = dc.entity_number, circuit_id = 2 };

				//TODO: do CC-splitting for large data?
				List<Filter> data = romdata[i];
				data.Add(new Filter { count = -i, signal = addrsignal });
				var cc = new ConstantCombinator
				{
					name = "constant-combinator",
					position = { x = 1 + i, y = -2 },
					direction = 4,
					filters = data
				};
				b.entities.Add(cc);

				var ac = new ArithmeticCombinator
				{
					name = "arithmetic-combinator",
					position = { x = 1 + i, y = -0.5 },
					direction = 4,
					first_signal = sigeach,
					operation = "+",
					output_signal = sigeach,
					connections = {
						["1"] = new CircuitPort{
							green = { baseaddrwire },
							red = { new CircuitConnection { entity_id = cc.entity_number, circuit_id = 1 } }
						},
						["2"] = new CircuitPort{
							red = { new CircuitConnection { entity_id = dc.entity_number, circuit_id = 1 } }
						},
					},
				};
				b.entities.Add(ac);
				baseaddrwire = new CircuitConnection { entity_id = ac.entity_number, circuit_id = 1 };
			}
			
			string print = b.GetBlueprintString();
			Console.WriteLine(print);
		}
		
        public Parser():base(null){}
		
		public void Parse(string input)
        {
			var s = new Scanner();
            s.SetSource(input, 0);
            s.Parser=this;
	        this.Scanner = s;
            this.Parse();
        }


		public void Require(string file)
		{
			//TODO: be smarter about finding the file - relative to program, then maybe standard include list?
			string prog = "";
			using (var reader = new StreamReader(file))
			{
				prog = reader.ReadToEnd();
			}

			Scanner.IncludeSource(prog);
			
		}


    }

}