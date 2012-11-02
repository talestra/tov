using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaUtils.Formats.Script;
using TalesOfVesperiaTests;
using System.IO;

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
			Assert.AreEqual(6, Texts.Count);
			Assert.AreEqual(
				"TextEntry(TextType=0,Id=0x3FA74,Id2=0x42AC,Original=[\"バルボス\", \"\\t(VB36_0703)くっ、やりやがって……許さん！！\"],Translated=[\"Barbos\", \"\\t(VB36_0703)Agh... Y-you\\'ll never beat me!\"])",
				Texts[0].ToString()
			);
			Assert.AreEqual(
				"\t(VS21_176)You have lost! \nNow give up!".EscapeString(),
				Texts[4].Translated[1].Text.EscapeString()
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
			var Tss = new TSS().Load(File.OpenRead(Utils.TestInputPath + "/BTL_EP_210_090"));
			Tss.TranslateTexts((Text) =>
			{
				foreach (var Entry in Text.Original) Entry.Text = "";
			});
			File.WriteAllBytes(Utils.TestOutputPath + "/BTL_EP_210_090.translated", Tss.Save().ToArray());
		}
	}
}
