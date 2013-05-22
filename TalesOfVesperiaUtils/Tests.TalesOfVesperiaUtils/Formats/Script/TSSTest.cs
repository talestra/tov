using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaUtils.Formats.Script;
using TalesOfVesperiaTests;
using System.IO;
using System.Collections.Generic;

namespace Tests.TalesOfVesperiaUtils.Formats.Script
{
	[TestClass]
	public class TSSTest
	{
		[TestMethod]
		public void TestReadInstructions()
		{
			var Tss = new TSS().Load(File.OpenRead(Utils.TestInputPath + "/BTL_EP_210_090"));
			using (var OutputStream = File.Open(Utils.TestOutputPath + "/BTL_EP_210_090.script", FileMode.Create))
			using (var StreamWriter = new StreamWriter(OutputStream))
			{
				foreach (var Instruction in Tss.ReadInstructions())
				{
					StreamWriter.WriteLine("{0}", Instruction.ToString());
				}
			}
		}

		[TestMethod]
		public void TestReadTexts()
		{
			var Tss = new TSS().Load(File.OpenRead(Utils.TestInputPath + "/BTL_EP_210_090"));
			var Texts = Tss.ExtractTexts();
			Assert.AreEqual(4, Texts.Count);
			/*
			Assert.AreEqual(
				"TextEntry(TextType=0,Id=0x3FA74,Id2=0x42AC,Original=[\"バルボス\", \"\\t(VB36_0703)くっ、やりやがって……許さん！！\"],Translated=[\"Barbos\", \"\\t(VB36_0703)Agh... Y-you\\'ll never beat me!\"])",
				Texts[0].ToString()
			);
			*/
			Assert.AreEqual(
				"\t(VS21_176)You have lost! \nNow give up!".EscapeString(),
				Texts[3].Translated[1].Text.EscapeString()
			);
		}

		[TestMethod]
		public void TestSaveScriptAsOriginal()
		{
			var BytesOriginal = File.ReadAllBytes(Utils.TestInputPath + "/BTL_EP_210_090");
			var Tss = new TSS().Load(new MemoryStream(BytesOriginal));
			var BytesTranslated = Tss.Save().ToArray();
			CollectionAssert.AreEqual(BytesOriginal, BytesTranslated);
		}
		
		[TestMethod]
		public void TestTranslateScript()
		{
			string TssFileName = "BTL_EP_210_090";
			int ExpectedTextCount = 4;
			var ExpectedStrings = new Queue<String>();

			var Tss = new TSS().Load(File.OpenRead(Utils.TestInputPath + "/" + TssFileName));
			int mm = 0;
			Tss.TranslateTexts((Text) =>
			{
				for (int n = 0; n < Text.Translated.Length; n++)
				{
					Text.Original[n].Text = "Original" + mm++;
					Text.Translated[n].Text = "Translated" + mm++;
				}
			}, (String) => {
				try
				{
					//Console.WriteLine(String);
					if (String == "VB36_1402") {
						return String = "**VB36_1403**";
					}
					return null;
				}
				finally
				{
					ExpectedStrings.Enqueue(String);
				}
			});
			Assert.AreEqual(ExpectedTextCount * 2 * 2, mm);

			mm = 0;
			var Tss2 = new TSS().Load(Tss.Save());
			var TranslatedTexts = Tss2.ExtractTexts();
			Assert.AreEqual(ExpectedTextCount, TranslatedTexts.Count);
			foreach (var Text in TranslatedTexts)
			{
				for (int n = 0; n < Text.Translated.Length; n++)
				{
					Assert.AreEqual("Original" + mm++, Text.Original[n].Text);
					Assert.AreEqual("Translated" + mm++, Text.Translated[n].Text);
				}
			}
			var TranslatedStrings = Tss2.ExtractStrings();
			foreach (var StringInfo in TranslatedStrings)
			{
				//Assert.AreEqual("Original" + mm++, Text.Original[n].Text);
				Assert.AreEqual(ExpectedStrings.Dequeue(), StringInfo.Text);
			}
			Assert.AreEqual(ExpectedTextCount * 2 * 2, mm);
			//File.WriteAllBytes(Utils.TestOutputPath + "/BTL_EP_210_090.translated", Tss.Save().ToArray());
		}
	}
}
