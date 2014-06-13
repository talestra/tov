using TalesOfVesperiaUtils.Formats.Packages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using CSharpUtils;
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
			using (var Fps4Stream = File.OpenRead(Utils.TestInputPath + @"\cook.svo"))
			{
				fps4.Load(Fps4Stream);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public void LoadInvalidTest()
		{
			using (var Fps4Stream = File.OpenRead(Utils.TestInputPath + @"\textfile.txt"))
			{
				fps4.Load(Fps4Stream);
			}
		}

		[TestMethod]
		public void LoadCheckFilesTest()
		{
			using (var Fps4Stream = File.OpenRead(Utils.TestInputPath + @"\cook.svo"))
			{
				fps4.Load(Fps4Stream);

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
				fps4.SaveTo(OutputStream);
				Assert.AreEqual(
					"16bb49cdb511d95efbd357c7c7d7e102fae6b99b",
					SHA1.Create().ComputeHash(OutputStream.ReadAll()).ToHexString()
				);
			}
		}

		[TestMethod]
		public void TestMapFormat()
		{
			using (var InputStream = new FileStream(Utils.TestInputPath + @"\UNDT04.DAT", FileMode.Open))
			{
				fps4.Load(InputStream);
				var Expected = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,MDL_UND_T04.HRC,MDL_UND_T04.SPM,MDL_UND_T04.BLD,MDL_UND_T04.SHD,MDL_UND_T04.CLS,MDL_UND_T04.MTR,MDL_UND_T04.ANM,MDL_UND_T04.TXM,MDL_UND_T04.SPV,MDL_UND_T04.TXV";
				Assert.AreEqual(Expected, String.Join(",", fps4.Entries.Values.Select(Item => Item.Name)));
			}
		}

		[TestMethod]
		public void TestMapFormat2()
		{
			using (var InputStream = new FileStream(Utils.TestInputPath + @"\ZZZTEX.DAT", FileMode.Open))
			{
				fps4.Load(InputStream);
				var Expected = "0,1";
				Assert.AreEqual(Expected, String.Join(",", fps4.Entries.Values.Select(Item => Item.Name)));
			}
		}

		[TestMethod]
		public void TestMapFormatMdl()
		{
			using (var InputStream = new FileStream(Utils.TestInputPath + @"\E_A101_TITLE.DAT", FileMode.Open))
			{
				fps4.Load(InputStream);
				var Expected = "E_A101_TITLE/MDL_TITLE_CONTINUE.MDL,E_A101_TITLE/MDL_TITLE_CONTINUE.MDL.BLD,E_A101_TITLE/MDL_TITLE_CONTINUE.MDL.CLS,E_A101_TITLE/MDL_TITLE_CONTINUE.MDL.HRC,E_A101_TITLE/MDL_TITLE_CONTINUE.MDL.MTR,E_A101_TITLE/MDL_TITLE_CONTINUE.MDL.SHD,E_A101_TITLE/MDL_TITLE_CONTINUE.MDL.SPM,E_A101_TITLE/MDL_TITLE_CONTINUE.MDL.SPV,E_A101_TITLE/MDL_TITLE_CONTINUE.MDL.TXM,E_A101_TITLE/MDL_TITLE_CONTINUE.MDL.TXV,E_A101_TITLE/MDL_TITLE_CREDIT.MDL,E_A101_TITLE/MDL_TITLE_CREDIT.MDL.BLD,E_A101_TITLE/MDL_TITLE_CREDIT.MDL.CLS,E_A101_TITLE/MDL_TITLE_CREDIT.MDL.HRC,E_A101_TITLE/MDL_TITLE_CREDIT.MDL.MTR,E_A101_TITLE/MDL_TITLE_CREDIT.MDL.SHD,E_A101_TITLE/MDL_TITLE_CREDIT.MDL.SPM,E_A101_TITLE/MDL_TITLE_CREDIT.MDL.SPV,E_A101_TITLE/MDL_TITLE_CREDIT.MDL.TXM,E_A101_TITLE/MDL_TITLE_CREDIT.MDL.TXV,E_A101_TITLE/MDL_TITLE_EXIT.MDL,E_A101_TITLE/MDL_TITLE_EXIT.MDL.BLD,E_A101_TITLE/MDL_TITLE_EXIT.MDL.CLS,E_A101_TITLE/MDL_TITLE_EXIT.MDL.HRC,E_A101_TITLE/MDL_TITLE_EXIT.MDL.MTR,E_A101_TITLE/MDL_TITLE_EXIT.MDL.SHD,E_A101_TITLE/MDL_TITLE_EXIT.MDL.SPM,E_A101_TITLE/MDL_TITLE_EXIT.MDL.SPV,E_A101_TITLE/MDL_TITLE_EXIT.MDL.TXM,E_A101_TITLE/MDL_TITLE_EXIT.MDL.TXV,E_A101_TITLE/MDL_TITLE_EXNEWGAME.MDL,E_A101_TITLE/MDL_TITLE_EXNEWGAME.MDL.BLD,E_A101_TITLE/MDL_TITLE_EXNEWGAME.MDL.CLS,E_A101_TITLE/MDL_TITLE_EXNEWGAME.MDL.HRC,E_A101_TITLE/MDL_TITLE_EXNEWGAME.MDL.MTR,E_A101_TITLE/MDL_TITLE_EXNEWGAME.MDL.SHD,E_A101_TITLE/MDL_TITLE_EXNEWGAME.MDL.SPM,E_A101_TITLE/MDL_TITLE_EXNEWGAME.MDL.SPV,E_A101_TITLE/MDL_TITLE_EXNEWGAME.MDL.TXM,E_A101_TITLE/MDL_TITLE_EXNEWGAME.MDL.TXV,E_A101_TITLE/MDL_TITLE_NAME01.MDL,E_A101_TITLE/MDL_TITLE_NAME01.MDL.BLD,E_A101_TITLE/MDL_TITLE_NAME01.MDL.CLS,E_A101_TITLE/MDL_TITLE_NAME01.MDL.HRC,E_A101_TITLE/MDL_TITLE_NAME01.MDL.MTR,E_A101_TITLE/MDL_TITLE_NAME01.MDL.SHD,E_A101_TITLE/MDL_TITLE_NAME01.MDL.SPM,E_A101_TITLE/MDL_TITLE_NAME01.MDL.SPV,E_A101_TITLE/MDL_TITLE_NAME01.MDL.TXM,E_A101_TITLE/MDL_TITLE_NAME01.MDL.TXV,E_A101_TITLE/MDL_TITLE_NEWGAME.MDL,E_A101_TITLE/MDL_TITLE_NEWGAME.MDL.BLD,E_A101_TITLE/MDL_TITLE_NEWGAME.MDL.CLS,E_A101_TITLE/MDL_TITLE_NEWGAME.MDL.HRC,E_A101_TITLE/MDL_TITLE_NEWGAME.MDL.MTR,E_A101_TITLE/MDL_TITLE_NEWGAME.MDL.SHD,E_A101_TITLE/MDL_TITLE_NEWGAME.MDL.SPM,E_A101_TITLE/MDL_TITLE_NEWGAME.MDL.SPV,E_A101_TITLE/MDL_TITLE_NEWGAME.MDL.TXM,E_A101_TITLE/MDL_TITLE_NEWGAME.MDL.TXV,E_A101_TITLE/MDL_TITLE_PAR.MDL,E_A101_TITLE/MDL_TITLE_PAR.MDL.BLD,E_A101_TITLE/MDL_TITLE_PAR.MDL.CLS,E_A101_TITLE/MDL_TITLE_PAR.MDL.HRC,E_A101_TITLE/MDL_TITLE_PAR.MDL.MTR,E_A101_TITLE/MDL_TITLE_PAR.MDL.SHD,E_A101_TITLE/MDL_TITLE_PAR.MDL.SPM,E_A101_TITLE/MDL_TITLE_PAR.MDL.SPV,E_A101_TITLE/MDL_TITLE_PAR.MDL.TXM,E_A101_TITLE/MDL_TITLE_PAR.MDL.TXV,E_A101_TITLE/MDL_TITLE_PUSH.MDL,E_A101_TITLE/MDL_TITLE_PUSH.MDL.BLD,E_A101_TITLE/MDL_TITLE_PUSH.MDL.CLS,E_A101_TITLE/MDL_TITLE_PUSH.MDL.HRC,E_A101_TITLE/MDL_TITLE_PUSH.MDL.MTR,E_A101_TITLE/MDL_TITLE_PUSH.MDL.SHD,E_A101_TITLE/MDL_TITLE_PUSH.MDL.SPM,E_A101_TITLE/MDL_TITLE_PUSH.MDL.SPV,E_A101_TITLE/MDL_TITLE_PUSH.MDL.TXM,E_A101_TITLE/MDL_TITLE_PUSH.MDL.TXV,E_A101_TITLE/MDL_TITLE_TEAM.MDL,E_A101_TITLE/MDL_TITLE_TEAM.MDL.BLD,E_A101_TITLE/MDL_TITLE_TEAM.MDL.CLS,E_A101_TITLE/MDL_TITLE_TEAM.MDL.HRC,E_A101_TITLE/MDL_TITLE_TEAM.MDL.MTR,E_A101_TITLE/MDL_TITLE_TEAM.MDL.SHD,E_A101_TITLE/MDL_TITLE_TEAM.MDL.SPM,E_A101_TITLE/MDL_TITLE_TEAM.MDL.SPV,E_A101_TITLE/MDL_TITLE_TEAM.MDL.TXM,E_A101_TITLE/MDL_TITLE_TEAM.MDL.TXV,Base.TXM,Base.TXV,Base.SCFOMBIN";
				Console.WriteLine(String.Join("\n", fps4.Entries.Values.Select(Item => Item.Name)));
				//Assert.AreEqual(Expected, String.Join(",", fps4.Entries.Values.Select(Item => Item.Name)));
			}
		}
	}
}
