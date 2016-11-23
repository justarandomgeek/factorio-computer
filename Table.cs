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
	public class Table : Dictionary<string, SExpr>, VExpr
	{
		public bool IsConstant()
		{
			return this.All(ti => ti.Value.IsConstant());
		}

		public Table Evaluate()
		{
			//TODO: do type mapping for type->var conversion here?
			return this;
		}

		public string datatype;

		public void Add(TableItem ti)
		{
			this.Add(ti.fieldname, ti.value);
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
				this.Add(new TableItem(c.Key, (IntSExpr)c.Value));
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
					eres = new ArithSExpr {
						S1 = t1[key],
						Op = ArithSpec.Add,
						S2 = t2[key]
					};
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
					eres = new ArithSExpr {
						S1 = t1[key],
						Op = ArithSpec.Subtract,
						S2 = t2[key]
					};
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
					eres = new ArithSExpr {
						S1 = t1[key],
						Op = ArithSpec.Multiply,
						S2 = t2[key]
					};
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
					eres = new ArithSExpr {
						S1 = t1[key],
						Op = ArithSpec.Divide,
						S2 = t2[key]
					};
				}
				tres.Add(key, eres);
			}
			return tres;
		}

		public static Table operator +(Table t, int i) {
			return t + (IntSExpr)i;
		}

		public static Table operator -(Table t, int i) {
			return t - (IntSExpr)i;
		}

		public static Table operator *(Table t, int i) {
			return t * (IntSExpr)i;
		}

		public static Table operator /(Table t, int i) {
			return t / (IntSExpr)i;
		}

		public static Table operator +(Table t, SExpr s) {
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key, new ArithSExpr {
					S1 = ti.Value,
					Op = ArithSpec.Add,
					S2 = s
				});
			}
			return tres;
		}

		public static Table operator -(Table t, SExpr s) {
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key, new ArithSExpr {
					S1 = ti.Value,
					Op = ArithSpec.Subtract,
					S2 = s
				});
			}
			return tres;
		}

		public static Table operator *(Table t, SExpr s) {
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key, new ArithSExpr {
					S1 = ti.Value,
					Op = ArithSpec.Divide,
					S2 = s
				});
			}
			return tres;
		}

		public static Table operator /(Table t, SExpr s) {
			var tres = new Table();
			foreach (var ti in t) {
				tres.Add(ti.Key, new ArithSExpr {
					S1 = ti.Value,
					Op = ArithSpec.Multiply,
					S2 = s
				});
			}
			return tres;
		}

		public override string ToString()
		{
			return string.Format("[{0}:{1}]", datatype, this.Count);
		}

		public VExpr FlattenExpressions()
		{
			return (Table)this.Select(kv => new KeyValuePair<string, SExpr>(kv.Key, kv.Value.FlattenExpressions()));
		}
	}
}

