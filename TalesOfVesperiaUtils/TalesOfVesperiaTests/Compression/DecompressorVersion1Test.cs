using TalesOfVesperiaUtils.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CSharpUtils;
using CSharpUtils.Extensions;
using System.Text;

namespace TalesOfVesperiaTests
{
	[TestClass]
	public class DecompressorVersion1Test
	{
		TalesCompression1_3 DecompressorVersion1;

		[TestInitialize()]
		public void Initialize()
		{
			DecompressorVersion1 = new TalesCompression1_3(1);
		}

		[TestCleanup()]
		public void Cleanup()
		{
			//DecompressorVersion1.Dispose();
		}

		[TestMethod]
		public void DecompressStream()
		{
			using (var CompressedStream = File.OpenRead(Utils.TestInputPath + @"\textfile.txt.c1"))
			using (var UncompressedStream = TalesCompression.DecompressStream(CompressedStream))
			using (var OriginalStream = File.OpenRead(Utils.TestInputPath + @"\textfile.txt"))
			{
				CollectionAssert.AreEqual(
					OriginalStream.ReadAll(),
					UncompressedStream.ReadAll()
				);
			}
		}
	}
}
