﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using nql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nql.Tests
{
	[TestClass()]
	public class IntSExprTests
	{
		[TestMethod()]
		public void IntSExprTest()
		{
			Assert.AreEqual(IntSExpr.Zero, 0);
			Assert.AreEqual(IntSExpr.One, 1);
			Assert.AreEqual(new IntSExpr(null), 0);
			Assert.AreEqual(new IntSExpr(null), IntSExpr.Zero);
			Assert.AreEqual(new IntSExpr(10), 10);
			Assert.AreEqual(new IntSExpr(10), new IntSExpr(10));
			Assert.AreEqual(new IntSExpr(-10), -10);
		}

		[TestMethod()]
		public void IsConstantTest()
		{
			Assert.IsTrue(IntSExpr.Zero.IsConstant());
			Assert.IsTrue(IntSExpr.One.IsConstant());
			Assert.IsTrue(new IntSExpr(null).IsConstant());
		}

		[TestMethod()]
		public void EvaluateTest()
		{
			Assert.AreEqual(IntSExpr.Zero.Evaluate(), 0);
			Assert.AreEqual(IntSExpr.One.Evaluate(), 1);
			Assert.AreEqual(new IntSExpr(null).Evaluate(), 0);
			Assert.AreEqual(new IntSExpr(10).Evaluate(), 10);
			Assert.AreEqual(new IntSExpr(-10).Evaluate(), -10);
		}

		[TestMethod()]
		public void FetchToFieldTest()
		{
			var c = IntSExpr.One.FetchToField(FieldSRef.VarField(RegVRef.rScratchInts, "signal-0"));
			Assert.AreEqual(c.Count, 1);
			Assert.AreEqual(c[0].opcode, Opcode.Sub);
			Assert.AreEqual(c[0].op2, c[0].dest);
			Assert.IsTrue(c[0].acc);
		}

		[TestMethod()]
		public void frameTest()
		{
			Assert.AreEqual(IntSExpr.One.frame(), PointerIndex.None);
		}

		[TestMethod()]
		public void AsDirectFieldTest()
		{
			Assert.IsNull(IntSExpr.One.AsDirectField());
		}
	}
}