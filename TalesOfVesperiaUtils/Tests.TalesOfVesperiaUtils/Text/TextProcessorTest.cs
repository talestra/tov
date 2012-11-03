using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaUtils.Text;

namespace Tests.TalesOfVesperiaUtils.Text
{
	[TestClass]
	public class TextProcessorTest
	{
		[TestMethod]
		public void TestSimpleCharaOk()
		{
			Assert.AreEqual("\x04(YUR)", TextProcessor.Instance.ProcessAndDetectPitfalls("\x04(YUR)", "<STR>(YUR)", true));
		}

		[TestMethod]
		public void TestPageWithLineBreaks()
		{
			Assert.AreEqual("Hola\x0cMundo", TextProcessor.Instance.ProcessAndDetectPitfalls("Hello\x0cWorld", "Hola<PAGE>\r\n\r\nMundo", true));
		}

		[TestMethod]
		public void TestPageWithLineBreaks2()
		{
			Assert.AreEqual("Hola\x0cMundo", TextProcessor.Instance.ProcessAndDetectPitfalls("Hello\x0cWorld", "Hola\n\n<PAGE>\r\n\r\nMundo", true));
		}
	}
}
