using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaUtils.Imaging;
using System.Drawing;
using CSharpUtils;
using System.IO;
using CSharpUtils.Drawing;

namespace TalesOfVesperiaTests.Imaging
{
	[TestClass]
	unsafe public class DXT5Test
	{
		[TestMethod]
		public void TestMethod1()
		{
			var Block = default(DXT5.Block);
			Block.EncodeSimpleUnoptimizedWhiteAlpha(
				new ARGB_Rev[] {
					Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), // 0
					Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF), // 1
					Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF), // 2
					Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF), // 3
					Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), // 4
					Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF), // 5
					Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), // 6
					Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF), // 7
					Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF), // 8
					Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF), // 9
					Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF), // 10
					Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF), // 11
					Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), // 12
					Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF), // 13
					Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF), // 14
					Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF), // 15
				}
			);

			var Blocks = new[] { Block };
			byte[] Bytes;

			fixed (DXT5.Block* Pointer = &Blocks[0])
			{
				Bytes = PointerUtils.PointerToByteArray((byte*)Pointer, 16);
			}

			Assert.AreEqual(
				"00-FF-8B-E8-AD-23-B2-8B-FF-FF-FF-FF-00-00-00-00",
				BitConverter.ToString(Bytes)
			);

			var Bitmap = DXT5.LoadSwizzled(File.OpenRead("../../../TestInput/FONTTEX05.TXV.mod.TXV"), 2048, 2048);
			Bitmap.Save("../../../Lol.png");
		}
	}
}
