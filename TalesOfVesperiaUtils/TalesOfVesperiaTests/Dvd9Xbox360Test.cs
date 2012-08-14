using TalesOfVesperiaUtils.Formats.Packages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CSharpUtils.Extensions;
using System.Text;

namespace TalesOfVesperiaTests
{
	[TestClass]
	public class Dvd9Xbox360Test
	{
		[TestMethod]
		public void LoadTest()
		{
			//var FilePath = @"I:\isos\360\Tales of Vesperia [PAL] [Multi3] [English] [Xbox360].iso";
			var FilePath = @"F:\Isos\360\games\Tales of Vesperia [PAL] [Multi3] [English] [Xbox360].iso";
			var Dvd9Xbox360 = new Dvd9Xbox360();
			Dvd9Xbox360.Load(File.OpenRead(FilePath));
			Assert.AreEqual(
				"EntryStruct(FullName='/language/scenario_us.dat', Offset=689559, Size=64369024, Attributes=File)",
				Dvd9Xbox360.RootEntry["/language/scenario_us.dat"].ToString()
			);
			var Stream = Dvd9Xbox360.RootEntry["/language/scenario_us.dat"].Open();
			Console.WriteLine(Stream.ReadString(16, Encoding.UTF8));

			Dvd9Xbox360.RootEntry["/language/scenario_us.dat.no"].ToString();

			/*
			foreach (var Item in Dvd9Xbox360.RootEntry.Descendant)
			{
				Console.WriteLine(Item);
			}
			*/
		}
	}
}
