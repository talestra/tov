using TalesOfVesperiaUtils.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CSharpUtils;
using System.Text;
using System.Collections.Generic;

namespace TalesOfVesperiaTests
{
	[TestClass]
	public class ACME1Test
	{
		[TestMethod]
		public void LoadTest()
		{
			using (var InputStream = File.OpenRead(Utils.TestInputPath + @"\tov_skits.zip"))
			{
				var ACME1 = new ACME1(InputStream, Encoding.GetEncoding("ISO-8859-1"));
				Assert.AreEqual(
					"(YUR)\nPrimero el asunto de ayer con los Caballeros.\nY ahora va y se rompe el aqua blastia.",
					ACME1.FilesByIndex[0].Entries[1].Text
				);
			}
		}

		[TestMethod]
		public void ParseAcmeFileTest()
		{
			using (var InputStream = File.OpenRead(Utils.TestInputPath + @"\T0@VC700.txt"))
			{
				var Entries = ACME1.ParseAcmeFile(InputStream, "T0@VC700.txt", Encoding.GetEncoding("ISO-8859-1"));
				Assert.AreEqual(
					"El día a día en los barrios bajos",
					Entries[0].Text
				);
			}
		}
	}
}
