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
using System.Runtime.InteropServices;

namespace TalesOfVesperiaTests.Imaging
{
	[TestClass]
	unsafe public class DXT5Test
	{
		[TestMethod]
		public void TestCompressDXT5()
		{
			var Colors1 = new ARGB_Rev[16]
			{
				"#E0D6A973",
				"#E0D6A900",
				"#E0D6A900",
				"#E0D6A900",
				"#E0D6A9BC",
				"#E0D6A95B",
				"#E0D6A95B",
				"#E0D6A95B",
				"#E3DAAED5",
				"#E0D6A9D5",
				"#E0D6A9D5",
				"#E0D6A9D5",
				"#E3DAAE8B",
				"#E6DEB4FF",
				"#E6DEB4FF",
				"#E6DEB4FF",
			};
			
			var Colors2 = new ARGB_Rev[16];
			var Block = default(DXT5.Block);
			//var Color1 = default(ARGB_Rev);
			//var Color2 = default(ARGB_Rev);

			CompressDXT5.CompressBlock(Colors1, out Block, CompressDXT5.CompressionMode.Normal);

			Console.WriteLine(StructUtils.StructToBytes(Block).ToHexString().ToUpper());

			Block.Decode(ref Colors2);

			Assert.AreEqual(
				"#E0D6A973,#E0D6A900,#E0D6A900,#E0D6A900,#E0D6A9BC,#E0D6A95B,#E0D6A95B,#E0D6A95B,#E3DAAED5,#E0D6A9D5,#E0D6A9D5,#E0D6A9D5,#E3DAAE8B,#E6DEB4FF,#E6DEB4FF,#E6DEB4FF",
				Colors1.ToStringArray(",")
			);
			Assert.AreEqual(
				"#DED6AC6D,#DED6AC00,#DED6AC00,#DED6AC00,#DED6ACB6,#DED6AC6D,#DED6AC6D,#DED6AC6D,#E0D8AEDA,#DED6ACDA,#DED6ACDA,#DED6ACDA,#E0D8AE91,#E6DEB4FF,#E6DEB4FF,#E6DEB4FF",
				Colors2.ToStringArray(",")
			);

			//CompressionSimpleDXT5.FindColorPair(Colors1, out Color1, out Color2);
			
			//CompressYCoCgDXT5.CompressBlock(Colors1, ref Block);
		}

		[TestMethod]
		public void TestDecodeBlock()
		{
			var Block1Data = new byte[] { 0xD5, 0x5B, 0x5D, 0xB2, 0x49, 0x00, 0xFF, 0xB2, 0xE6, 0xF6, 0xDE, 0x94, 0xFF, 0xFF, 0x02, 0xFE };

			var Block = new MemoryStream(Block1Data).ReadStruct<DXT5.Block>();
			var Colors = new ARGB_Rev[16];
			Block.Decode(ref Colors);

			Assert.AreEqual(
				"#E0D6A973,#E0D6A900,#E0D6A900,#E0D6A900,#E0D6A9BC,#E0D6A95B,#E0D6A95B,#E0D6A95B," +
				"#E3DAAED5,#E0D6A9D5,#E0D6A9D5,#E0D6A9D5,#E3DAAE8B,#E6DEB4FF,#E6DEB4FF,#E6DEB4FF"
				,
				Colors.ToStringArray(",")
			);
		}

		[TestMethod]
		public void TestEncodeUnoptimizedWhiteAlpha()
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

			var Bitmap = DXT5.LoadSwizzled2D(File.OpenRead("../../../TestInput/FONTTEX05.TXV.mod.TXV"), 2048, 2048);
			Bitmap.Save("../../../Lol.png");
		}
	}
}
