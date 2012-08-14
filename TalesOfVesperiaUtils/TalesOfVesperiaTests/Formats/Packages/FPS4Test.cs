using TalesOfVesperiaUtils.Formats.Packages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CSharpUtils;
using CSharpUtils.Extensions;
using System.Text;
using System.Security.Cryptography;

namespace TalesOfVesperiaTests.Formats.Packages
{
	[TestClass]
	public class FPS4Test
	{
		FPS4 fps4;

		[TestInitialize()]
		public void Initialize()
		{
			fps4 = new FPS4();
		}

		[TestCleanup()]
		public void Cleanup()
		{
			fps4.Dispose();
		}

		[TestMethod]
		public void DoNotLoadTest()
		{
		}

		[TestMethod]
		public void LoadValidTest()
		{
			fps4.Load(new FileStream(Utils.TestInputPath + @"\cook.svo", FileMode.Open));
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public void LoadInvalidTest()
		{
			fps4.Load(new FileStream(Utils.TestInputPath + @"\textfile.txt", FileMode.Open));
		}

		[TestMethod]
		public void LoadCheckFilesTest()
		{
			fps4.Load(new FileStream(Utils.TestInputPath + @"\cook.svo", FileMode.Open));

			var FirstEntry = fps4.Entries["COOKDATA.BIN"];
			var Stream = FirstEntry.Open();

			Assert.AreSame(FirstEntry, fps4.EntryList[0]);

			Assert.AreEqual<string>(FirstEntry.Name, "COOKDATA.BIN");
			Assert.AreEqual<uint>(10240, FirstEntry.EntryStruct.LengthSectorAligned);
			Assert.AreEqual<uint>(9928, FirstEntry.EntryStruct.LengthReal);
			Assert.AreEqual<long>(9928, Stream.Length);
			Assert.AreEqual<string>("COOKDAT", Stream.ReadStringz(7));
			Assert.AreEqual<string>("../Release/共通/cook.svo", fps4.OriginalFilePath);
		}

		[TestMethod]
		public void SaveTest()
		{
			using (var InputStream = new FileStream(Utils.TestInputPath + @"\cook.svo", FileMode.Open))
			//using (var OutputStream = new FileStream(Utils.TestOutputPath + @"\cook.new.svo", FileMode.Create))
			using (var OutputStream = new MemoryStream())
			{
				fps4.Load(InputStream);
				fps4.Entries["COOKDATA.BIN"].SetStream(new MemoryStream(Encoding.ASCII.GetBytes("Hello World")));
				fps4.CreateEntry("COOKDATA2.BIN", fps4.Entries["COOKDATA.BIN"]);
				fps4.Save(OutputStream);
				Assert.AreEqual(
					"16bb49cdb511d95efbd357c7c7d7e102fae6b99b",
					SHA1.Create().ComputeHash(OutputStream.ReadAll()).ToHexString()
				);
			}
		}
	}
}
