using TalesOfVesperiaUtils.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using CSharpUtils;

namespace TalesOfVesperiaTests
{
	/// <summary>
	///Se trata de una clase de prueba para TalesOfVesperiaCompressionWrapperTest y se pretende que
	///contenga todas las pruebas unitarias TalesOfVesperiaCompressionWrapperTest.
	///</summary>
	[TestClass()]
	public class TalesOfVesperiaCompressionWrapperTest
	{
		[TestMethod]
		public void EncodeTest()
		{
			byte[] Compressed = TalesCompression1_3.Encode(
				Version: 1,
				Input: Encoding.ASCII.GetBytes("Hello World. Hello World. Hello world again.")
			);

			Console.WriteLine(Encoding.ASCII.GetString(Compressed));
		}
	}
}
