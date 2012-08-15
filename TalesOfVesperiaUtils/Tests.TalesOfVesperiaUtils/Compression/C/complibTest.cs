using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaUtils.Compression.C;

namespace TalesOfVesperiaTests.Compression.C
{
	[TestClass]
	unsafe public class complibTest
	{
		[TestMethod]
		public void EncodeDecode()
		{
			var InputString = String.Join("", "Hello World. Hello World! WORLD! World! World! Hello! Hello World! HeheheheHehEheheheheheheWorld!".Repeat(16));
			var UncompressedByteArray = Encoding.UTF8.GetBytes(InputString);
			var CompressedByteArray = complib.Encode(3, UncompressedByteArray);
			var DeCompressedByteArray = complib.Decode(3, CompressedByteArray);
			var OutputString = Encoding.UTF8.GetString(DeCompressedByteArray);
			Assert.AreEqual(InputString, OutputString);
		}
	}
}
