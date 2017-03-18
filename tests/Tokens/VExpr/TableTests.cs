using Microsoft.VisualStudio.TestTools.UnitTesting;
using compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.Tests
{
	[TestClass()]
	public class TableTests
	{
		[TestMethod()]
		public void IsConstantTest()
		{
			var t = new Table();
			t.Add("signal-0", 1);
			Assert.IsTrue(t.IsConstant());
			t.Add("signal-1", FieldSRef.Imm1());
			Assert.IsFalse(t.IsConstant());
		}

		[TestMethod()]
		public void ConstPartTest()
		{
			var t = new Table();
			t.Add("signal-0", 1);
			t.Add("signal-1", FieldSRef.Imm1());

			t = t.ConstPart();
			Assert.IsTrue(t.IsConstant());
			Assert.AreEqual(t.Count, 1);
			Assert.AreEqual(t["signal-0"], 1);
		}

		[TestMethod()]
		public void ExprPartTest()
		{
			var t = new Table();
			t.Add("signal-0", 1);
			t.Add("signal-1", FieldSRef.Imm1());

			t = t.ExprPart();
			Assert.IsFalse(t.IsConstant());
			Assert.AreEqual(t.Count, 1);
			var f = t["signal-1"] as FieldSRef;
			Assert.AreEqual(f.fieldname, FieldSRef.Imm1().fieldname);
			Assert.AreEqual(f.varref, FieldSRef.Imm1().varref);
		}

		[TestMethod()]
		public void EvaluateTest()
		{
			var t = new Table();
			t.Add("signal-0", new ArithSExpr(new IntSExpr(4), ArithSpec.Add, new IntSExpr(6)));
			t = t.Evaluate();
			Assert.AreEqual(t.Count, 1);
			Assert.AreEqual(t["signal-0"], 10);
		}

		[TestMethod()]
		public void AddTest()
		{
			var t = new Table();

			t.Add("signal-0", 1);
			Assert.AreEqual(t.Count, 1);
			Assert.AreEqual(t["signal-0"], 1);

			t.Add(new TableItem("signal-1", new IntSExpr(2)));
			Assert.AreEqual(t.Count, 2);
			Assert.AreEqual(t["signal-1"], 2);
		}

		[TestMethod()]
		public void TableTest()
		{
			var t = new Table("TEST");
			Assert.AreEqual(t.Count, 3);
			Assert.AreEqual(t["T"], 9);
			Assert.AreEqual(t["E"], 2);
			Assert.AreEqual(t["S"], 4);
		}
		
		[TestMethod()]
		public void FetchToRegTest()
		{
			//TODO: test codegen more strictly?
			var t = new Table("TEST");
			var c = t.FetchToReg(RegVRef.rScratchTab);
			Assert.AreEqual(c.Count, 1);
			Assert.AreEqual(c[0].opcode,Opcode.MemRead);
			
			t.Add("signal-1", FieldSRef.Imm1());
			c = t.FetchToReg(RegVRef.rScratchTab);
			Assert.AreEqual(c.Count, 2);
			Assert.AreEqual(c[0].opcode, Opcode.MemRead);
			Assert.AreEqual(c[1].opcode, Opcode.Sub);
			Assert.IsTrue(c[1].acc);
			Assert.AreEqual(c[1].op2.varref, c[1].dest.varref);
			Assert.AreEqual(c[1].op2.fieldname, c[1].dest.fieldname);
		}

		[TestMethod()]
		public void AsRegTest()
		{
			Assert.IsNull(new Table().AsReg());
		}
	}
}