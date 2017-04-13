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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class Table : Dictionary<string, SExpr>, VExpr
	{
		public bool IsConstant()
		{
			return this.All(ti => ti.Value.IsConstant());
		}

		public Table ConstPart()
		{
			var t = new Table();
			t.datatype = this.datatype;
			
			foreach (var item in this.Where(ti => ti.Value.IsConstant()))
			{
				t.Add(item.Key, item.Value);
			}
			return t;
		}

		public Table ExprPart()
		{
			var t = new Table();
			t.datatype = this.datatype;
			foreach (var item in this.Where(ti => !ti.Value.IsConstant()))
			{
				t.Add(item.Key, item.Value);
			}
			return t;
		}

		public Table Evaluate() { return Evaluate(false); }
		public Table Evaluate(bool withdatatype)
		{
			var output = new Table();
			if (withdatatype) output.datatype=this.datatype;
			foreach (var element in this) {
				
				string field = element.Key;
				if(datatype != null && datatype != "var")
				{
					if(Program.CurrentProgram?.Types[datatype].ContainsKey(field)??false)
						field = Program.CurrentProgram.Types[datatype][field];
				}
				
				output.Add(
					field ?? element.Key,
					element.Value.IsConstant() ? new IntSExpr(element.Value.Evaluate()) : element.Value
				);
			}
			
			return output;
		}

		public string datatype;

		public void Add(TableItem ti)
		{
			this.Add(ti.fieldname, ti.value);
		}

		public void Add(string fieldname, int value)
		{
			this.Add(fieldname, new IntSExpr(value));
		}

		public Table() : base()
		{
		}

		public Table(string text)
		{
			var chars = new Dictionary<char, int>();
			int i = 0;
			foreach (var c in text) {
				if (!chars.ContainsKey(c))
					chars.Add(c, 0);
				chars[c] += 1 << i++;
			}
			foreach (var c in chars) {
				if (c.Key == ' ')
					continue;
				this.Add(new TableItem(c.Key, new IntSExpr(c.Value)));
			}
		}

		public static Table operator +(Table t1, Table t2) {
			var tres = new Table();
			foreach (var key in t1.Keys.Union(t2.Keys)) {
				SExpr eres;
				if (!t2.ContainsKey(key)) {
					eres = t1[key];
				}
				else if (!t1.ContainsKey(key)) {
					eres = t2[key];
				}
				else {
					eres = new ArithSExpr (
						t1[key],
						ArithSpec.Add,
						t2[key]
					);
				}
				tres.Add(key, eres);
			}
			return tres;
		}

		public static Table operator -(Table t1, Table t2) {
			var tres = new Table();
			foreach (var key in t1.Keys.Union(t2.Keys)) {
				SExpr eres;
				if (!t2.ContainsKey(key)) {
					eres = t1[key];
				}
				else if (!t1.ContainsKey(key)) {
					eres = t2[key];
				}
				else {
					eres = new ArithSExpr (
						t1[key],
						ArithSpec.Subtract,
						t2[key]
					);
				}
				tres.Add(key, eres);
			}
			return tres;
		}

		public static Table operator *(Table t1, Table t2) {
			var tres = new Table();
			foreach (var key in t1.Keys.Union(t2.Keys)) {
				SExpr eres;
				if (!t2.ContainsKey(key)) {
					eres = t1[key];
				}
				else if (!t1.ContainsKey(key)) {
					eres = t2[key];
				}
				else {
					eres = new ArithSExpr (
						t1[key],
						ArithSpec.Multiply,
						t2[key]
					);
				}
				tres.Add(key, eres);
			}
			return tres;
		}

		public static Table operator /(Table t1, Table t2) {
			var tres = new Table();
			foreach (var key in t1.Keys.Union(t2.Keys)) {
				SExpr eres;
				if (!t2.ContainsKey(key)) {
					eres = t1[key];
				}
				else if (!t1.ContainsKey(key)) {
					eres = t2[key];
				}
				else {
					eres = new ArithSExpr (
						t1[key],
						ArithSpec.Divide,
						t2[key]
					);
				}
				tres.Add(key, eres);
			}
			return tres;
		}

		public static Table operator +(Table t, SExpr s) {
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key, new ArithSExpr (
					ti.Value,
					ArithSpec.Add,
					s
				));
			}
			return tres;
		}

		public static Table operator -(Table t, SExpr s) {
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key, new ArithSExpr (
					ti.Value,
					ArithSpec.Subtract,
					s
				));
			}
			return tres;
		}

		public static Table operator *(Table t, SExpr s) {
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key, new ArithSExpr (
					ti.Value,
					ArithSpec.Multiply,
					s
				));
			}
			return tres;
		}

		public static Table operator /(Table t, SExpr s) {
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key, new ArithSExpr(
					ti.Value,
					ArithSpec.Divide,
					s
				));
			}
			return tres;
		}

		public override bool Equals(object obj)
		{
			if(obj is Table)
			{
				var t = obj as Table;

				if (this.datatype != t.datatype) return false;

				if (this.Count != t.Count) return false;

				return this.All(ti => t.ContainsKey(ti.Key) && t[ti.Key].Equals(ti.Value));
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return this.Evaluate(true).ToString().GetHashCode();
		}


		public override string ToString()
		{
			// Other formatters? Maybe a way to provide them externally?
			if(datatype == "opcode")
			{
				return string.Format("[OP{0} [{13}]{1}.{2} {3}.{4} => {7}{5}.{6} {8}:{9}:{10} {11}:{12}]",
				                     this.ContainsKey("op")?this["op"]:null,
				                     this.ContainsKey("R1")?this["R1"]:null,
				                     this.ContainsKey("S1")?this["S1"]:null,
				                     this.ContainsKey("R2")?this["R2"]:null,
				                     this.ContainsKey("S2")?this["S2"]:null,
				                     this.ContainsKey("Rd")?this["Rd"]:null,
				                     this.ContainsKey("Sd")?this["Sd"]:null,
				                     this.ContainsKey("acc")?"A":null,
				                     this.ContainsKey("addr1")?this["addr1"]:null,
				                     this.ContainsKey("addr2")?this["addr2"]:null,
				                     this.ContainsKey("addr3")?this["addr3"]:null,
				                     this.ContainsKey("Imm1")?this["Imm1"]:null,
				                     this.ContainsKey("Imm2")?this["Imm2"]:null,
				                     this.ContainsKey("index")?this["index"]:null
				                    );
			}
			return string.Format("[{0}:{1}  {2}]", datatype, this.Count, string.Join(", ",this.Select(item=>item.Key+":"+item.Value)));
		}

		public List<Instruction> FetchToReg(RegVRef dest)
		{
			//allocate as const if possible, and memread it
			if (this.IsConstant())
			{
				if (this.Count == 0)
				{
					return RegVRef.rNull.FetchToReg(dest);
				}
				else
				{
					var constname = "__const" + this.GetHashCode();
					var constsym = new Symbol
					{
						type = SymbolType.Constant,
						name = constname,
						frame = PointerIndex.ProgConst,
						datatype = this.datatype,
						data = new List<Table> { this },
					};

					if(Program.CurrentProgram!=null && !Program.CurrentProgram.Symbols.Contains(constsym))
						Program.CurrentProgram.Symbols.Add(constsym);

					return new MemVRef(new AddrSExpr(constname), this.datatype).FetchToReg(dest);
				}
			}
			else
			{
				// compose non-const table in a block of code

				
				// fetch const part
				var code = this.ConstPart().FetchToReg(dest);

				// sequentially fetch non-const parts? maybe cache sub-expressions
				code.AddRange(
					this.ExprPart().
					SelectMany(
						ti => ti.Value.FetchToField(
							new FieldSRef(dest.AsType(this.datatype), ti.Key, true)
							)
						)
					);
				
				return code;
			}


			
		}

		public RegVRef AsReg()
		{
			return null;
		}
	}
}

