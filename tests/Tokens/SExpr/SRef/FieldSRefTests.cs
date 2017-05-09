using Microsoft.VisualStudio.TestTools.UnitTesting;
using nql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nql.Tests
{
	[TestClass()]
	public class FieldSRefTests
	{
		[TestMethod()]
		public void IsConstantTest()
		{
			Assert.IsFalse(FieldSRef.Imm1().IsConstant());
		}

		[TestMethod()]
		public void EvaluateTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void FieldSRefTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void ScratchIntTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void ResetScratchIntsTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void PutFromFieldTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void PutFromIntTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void FetchToFieldTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void frameTest()
		{
			Assert.AreEqual(FieldSRef.Imm1().frame(), PointerIndex.None);
		}

		[TestMethod()]
		public void InRegisterTest()
		{
			var r1 = FieldSRef.VarField(RegVRef.rOpcode, "signal-0");
			var r2 = r1.InRegister(RegVRef.rScratchInts);
			Assert.AreEqual(r2.varref, RegVRef.rScratchInts);
			Assert.AreEqual(r2.fieldname, "signal-0");


			var m1 = FieldSRef.VarField(new MemVRef(IntSExpr.Zero), "signal-0");
			var m2 = m1.InRegister(RegVRef.rScratchInts);
			Assert.AreEqual(m2.varref, RegVRef.rScratchInts);
			Assert.AreEqual(m2.fieldname, "signal-0");
		}

		[TestMethod()]
		public void AsDirectFieldTest()
		{
			var regfield = FieldSRef.VarField(RegVRef.rOpcode, "signal-0");
			Assert.AreEqual(regfield.AsDirectField(), regfield);

			var memfield = FieldSRef.VarField(new MemVRef(IntSExpr.Zero), "signal-0");
			Assert.IsNull(memfield.AsDirectField());
		}
	}
}