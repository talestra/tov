using System;
using CSharpUtils.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaUtils.Imaging;

namespace Tests.TalesOfVesperiaUtils.Imaging
{
	[TestClass]
	public class Y_CO_CG_ATest
	{
		[TestMethod]
		public void TestMethod1()
		{
			var Color = (ARGB_Rev)"#3466EEFF";
			var YCoCg = (Y_CO_CG_A)Color;
			var Color2 = (ARGB_Rev)YCoCg;
			Assert.AreEqual("#3467EEFF", Color2.ToString());
		}
	}
}
