using TalesOfVesperiaUtils.Formats.Packages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using TalesOfVesperiaUtils.Text;
using TalesOfVesperiaUtils.Formats;
using System.Text;

namespace TalesOfVesperiaTests
{
	[TestClass]
	public class TO8CHTXTest
	{
		[TestMethod]
		public void LoadTest()
		{
			using (var InputStream = File.OpenRead(Utils.TestInputPath + @"\VC001US.TO8CHTX"))
			{
				var TO8CHTX = new TO8CHTX(InputStream);
				Assert.AreEqual(12, TO8CHTX.Length);
				Assert.AreEqual<String>(TO8CHTX[0].TextTranslated, "Wow, the days just fly by...");
				Assert.AreEqual<String>(TO8CHTX[6].TextTranslated, "So all this bad luck made us fight, \nand so...we got stronger.");
			}
		}

		[TestMethod]
		public void TranslateWithAcmeFileTest()
		{
			using (var AcmeStream = File.OpenRead(Utils.TestInputPath + @"\tov_skits.zip"))
			using (var ACME1 = new ACME1(AcmeStream, Encoding.GetEncoding("ISO-8859-1")))
			{
				using (var InputStream = File.OpenRead(Utils.TestInputPath + @"\VC001US.TO8CHTX"))
				using (var OutputStream = File.Open(Utils.TestOutputPath + @"\VC001ES.TO8CHTX", FileMode.Create))
				{
					var TO8CHTX = new TO8CHTX(InputStream);
					TO8CHTX.TranslateWithAcmeFile(ACME1.Files["T3@VC001"]);
					TO8CHTX.Save(OutputStream);
				}

				using (var EnglishInputStream = File.OpenRead(Utils.TestInputPath + @"\VC001US.TO8CHTX"))
				using (var SpanishInputStream = File.OpenRead(Utils.TestOutputPath + @"\VC001ES.TO8CHTX"))
				{
					var EnglishTO8CHTX = new TO8CHTX(EnglishInputStream);
					var SpanishTO8CHTX = new TO8CHTX(SpanishInputStream);
					//Console.WriteLine("------------------------------");
					//ACME1.Files["T3@VC001"].Entries[0]
					//Console.WriteLine(SpanishTO8CHTX.Entries[0].TextTranslated);

					EnglishTO8CHTX.TranslateWithAcmeFile(ACME1.Files["T3@VC001"]);

					for (int n = 0; n < EnglishTO8CHTX.Length; n++)
					{
						var EnglishEntry = EnglishTO8CHTX.Entries[n];
						var SpanishEntry = SpanishTO8CHTX.Entries[n];
						Assert.AreEqual(EnglishEntry.Title, SpanishEntry.Title);
						Assert.AreEqual(EnglishEntry.Talker, SpanishEntry.Talker);
						Assert.AreEqual(EnglishEntry.TextTranslated, SpanishEntry.TextTranslated);
						Assert.AreEqual(EnglishEntry.TextOriginal, SpanishEntry.TextOriginal);
					}
				}
			}
		}

		[TestMethod]
		public void SaveTest()
		{
			using (var InputStream = File.OpenRead(Utils.TestInputPath + @"\VC001US.TO8CHTX"))
			using (var OutputStream = File.Open(Utils.TestOutputPath + @"\VC001US.TO8CHTX", FileMode.Create))
			{
				var TO8CHTX = new TO8CHTX(InputStream);
				foreach (var Entry in TO8CHTX.Entries)
				{
					Entry.TextOriginal = "";
					//Entry.TextTranslated = "";
				}
				TO8CHTX.Save(OutputStream);
			}
			using (var OutputStream = File.Open(Utils.TestOutputPath + @"\TEST.TO8CHTX", FileMode.Create))
			{
				var TO8CHTX = new TO8CHTX();
				TO8CHTX.Entries.Add(new TO8CHTX.Entry()
				{
					Title = CharacterMapping.Instance.Map("Title Test"),
					TextOriginal = CharacterMapping.Instance.Map("Text Original"),
					TextTranslated = CharacterMapping.Instance.Map("¡¿Esto es una prueba en ESPAÑOL o en Español?!"),
					Talker = TalesOfVesperiaUtils.Formats.Packages.TO8CHTX.Talker.Yuri,
				});
				TO8CHTX.Save(OutputStream);
			}
		}
	}
}
