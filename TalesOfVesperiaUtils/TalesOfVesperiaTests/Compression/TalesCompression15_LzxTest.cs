using TalesOfVesperiaUtils.Compression.CAB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using TalesOfVesperiaUtils.Compression;

namespace TalesOfVesperiaTests
{
	[TestClass]
	public class TalesCompression15_LzxTest
	{
		[TestMethod]
		public void TalesCompression15_Lzx_Test()
		{
			//var FileNameBase = "item.svo";
			var FileNameBase = "VC572US.DAT.u";
			var FileNameInput = String.Format(@"{0}\{1}", Utils.TestInputPath, FileNameBase);
			var FileNameCompressed = String.Format(@"{0}\{1}.comp", Utils.TestOutputPath, FileNameBase);
			var FileNameUncompressed = String.Format(@"{0}\{1}.comp.u", Utils.TestOutputPath, FileNameBase);

			using (var OriginalFile = File.OpenRead(FileNameInput))
			using (var CompressedFile = File.OpenWrite(FileNameCompressed))
			{
				TalesCompression15_Lzx.CreateCompression15File(OriginalFile, CompressedFile);
			}

			using (var CompressedFile = File.OpenRead(FileNameCompressed))
			using (var NewUncompressedfile = File.OpenWrite(FileNameUncompressed))
			{
				(new TalesCompression15_Lzx()).DecodeFile(CompressedFile, NewUncompressedfile);
			}

			CollectionAssert.AreEqual(
				File.ReadAllBytes(FileNameInput),
				File.ReadAllBytes(FileNameUncompressed)
			);
		}

		[TestMethod]
		public void TalesCompression15_Lzx_2_Test()
		{
			var FileNameBase = "VC572US.DAT";
			var FileNameCompressed = String.Format(@"{0}\{1}", Utils.TestInputPath, FileNameBase);
			using (var CompressedFile = File.OpenRead(FileNameCompressed))
			{
				var TalesCompression15_Lzx = new TalesCompression15_Lzx();
				var UncompressedFile = TalesCompression15_Lzx.DecodeFile(CompressedFile);
				//TalesCompression15_Lzx.CreateCompression15File(OriginalFile, CompressedFile);
			}
		
		}
	}
}
