using TalesOfVesperiaUtils.Formats.Packages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CSharpUtils;

namespace TalesOfVesperiaTests.Formats.Packages
{
	[TestClass]
	public class TO8SCELTest
	{
		TO8SCEL to8scel;

		[TestInitialize]
		public void Initialize()
		{
			to8scel = new TO8SCEL();
		}

		[TestCleanup]
		public void Cleanup()
		{
			to8scel.Dispose();
		}

		[TestMethod]
		public void LoadValidTest()
		{
			to8scel.Load(new FileStream(Utils.TestInputPath + @"\scenario_es.dat", FileMode.Open));
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public void LoadInvalidTest()
		{
			to8scel.Load(new FileStream(Utils.TestInputPath + @"\textfile.txt", FileMode.Open));
		}

		[TestMethod]
		public void AccessSubfileTest()
		{
			to8scel.Load(new FileStream(Utils.TestInputPath + @"\scenario_es.dat", FileMode.Open));
			File.WriteAllBytes(Utils.TestOutputPath + @"\1.c", to8scel.Entries[0].CompressedStream.ReadAll());
			File.WriteAllBytes(Utils.TestOutputPath + @"\1.u", to8scel.Entries[0].UncompressedStream.ReadAll());
			//Console.WriteLine(to8scel.Entries[0].UncompressedStream.ReadBytes(0x20).ToHexString());
		}
	}
}
