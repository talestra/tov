using TalesOfVesperiaUtils.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;
using CSharpUtils;

namespace TalesOfVesperiaTests
{
	[TestClass]
	public class DecompressorLzxTest
	{
		[TestMethod]
		public void DecodeFileTest()
		{
			TalesCompression15_Lzx DecompressorLzx = new TalesCompression15_Lzx();
			var OutputStream = DecompressorLzx.DecodeFile(
				File.OpenRead(String.Format(@"{0}\VC980US.DAT", Utils.TestInputPath))
			);

			//OutputStream.CopyTo(File.OpenWrite(String.Format(@"{0}\VC980US.DAT.u", Utils.TestOutputPath)), 8 * 1024 * 1024);

			Assert.AreEqual<String>(
				"20cd60658d841fc72a4da4c8c3b6f3624b9fe8c4",
				SHA1.Create().ComputeHash(OutputStream).ToHexString()
			);
		}
	}
}
