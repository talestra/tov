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
			Assert.AreEqual(
				"\x04(YUR)",
				TextProcessor.Instance.ProcessAndDetectPitfalls(
					"\x04(YUR)",
					"<STR>(YUR)",
					true
				)
			);
		}

		[TestMethod]
		public void TestPageWithLineBreaks()
		{
			Assert.AreEqual(
				"Hola\x0cMundo",
				TextProcessor.Instance.ProcessAndDetectPitfalls(
					"Hello\x0cWorld",
					"Hola<PAGE>\r\n\r\nMundo",
					true
				)
			);
		}

		[TestMethod]
		public void TestPageWithLineBreaks2()
		{
			Assert.AreEqual(
				"Hola\x0cMundo",
				TextProcessor.Instance.ProcessAndDetectPitfalls(
					"Hello\x0cWorld",
					"Hola\n\n<PAGE>\r\n\r\nMundo",
					true
				)
			);
		}

        [TestMethod]
        public void TestAddLineBreakToTheEnd()
        {
			Assert.AreEqual(
				"Refriega de cincuenta rivales (5.000 gald)\n",
				TextProcessor.Instance.ProcessAndDetectPitfalls(
					"Fifty Man Melee (5,000 Gald)\n",
					"Refriega de cincuenta rivales (5.000 gald)",
					true
				)
			);
        }

		[TestMethod]
		public void TestExtraTitlesInChatFix()
		{
			var Modified = TextProcessor.Instance.ProcessAndDetectPitfalls(
				"What makes you say that all of a sudden?",
				"<STR>(YUR)\n¿A qué viene esto ahora?",
				true
			);
			Assert.AreEqual("ËA qué viene esto ahora?", Modified);
		}
	}
}
