using TalesOfVesperiaUtils.Formats.Packages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CSharpUtils;
using CSharpUtils.Streams;
using System.Text;

namespace TalesOfVesperiaTests
{
	[TestClass]
	public class Dvd9Xbox360Test
	{
		[TestMethod]
		public void LoadTest()
		{
			
#if true
            //var FilePath = @"I:\isos\360\Tales of Vesperia [PAL] [Multi3] [English] [Xbox360].iso";
			var FilePath = @"E:\Isos\360\games\Tales of Vesperia [PAL] [Multi3] [English] [Xbox360].iso";
            var FileStream = File.OpenRead(FilePath);
            var FileStreamAnalyzed = new ProxyStreamReadWriteAnalyzer(FileStream);

			var Dvd9Xbox360 = new Dvd9Xbox360().Load(FileStreamAnalyzed);

			// Analyze.
			var MapStream = FileStreamAnalyzed.ConvertSpacesToMapStream(FileStreamAnalyzed.ReadUsage);

			// Dump the analyzed data.
            SerializerUtils.SerializeToMemoryStream(MapStream.Serialize).CopyToFile(Utils.TestInputPath + "/mini_iso.bin");
#else
            var FileStream = MapStream.Unserialize(new MemoryStream(File.OpenRead(Utils.TestInputPath + "/mini_iso.bin").ReadAll()));
            var Dvd9Xbox360 = new Dvd9Xbox360().Load(FileStream);
#endif
            
			Assert.AreEqual(
				"EntryStruct(FullName='/language/scenario_us.dat', Offset=689559, Size=64369024, Attributes=File)",
				Dvd9Xbox360.RootEntry["/language/scenario_us.dat"].ToString()
			);
			var Stream = Dvd9Xbox360.RootEntry["/language/scenario_us.dat"].Open();
			Console.WriteLine(Stream.ReadString(16, Encoding.UTF8));

			//Dvd9Xbox360.RootEntry["/language/scenario_us.dat.no"].ToString();

			/*
			foreach (var Item in Dvd9Xbox360.RootEntry.Descendant)
			{
				Console.WriteLine(Item);
			}
			*/
		}
	}
}
