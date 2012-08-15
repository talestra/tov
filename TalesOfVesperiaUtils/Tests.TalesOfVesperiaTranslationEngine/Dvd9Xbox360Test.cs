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
#if false
			bool GENERATE_MINI_ISO = false;
#endif
			Dvd9Xbox360 Dvd9Xbox360;
			MapStream mapStream;
			ProxyStreamReadWriteAnalyzer FileStreamAnalyzed;
			Stream FileStream;

#if false
			if (GENERATE_MINI_ISO)
			{
				//var FilePath = @"I:\isos\360\Tales of Vesperia [PAL] [Multi3] [English] [Xbox360].iso";
				var FilePath = @"E:\Isos\360\games\Tales of Vesperia [PAL] [Multi3] [English] [Xbox360].iso";
				FileStream = File.OpenRead(FilePath);
			}
			else
#endif
			{
				mapStream = MapStream.Unserialize(new MemoryStream(File.OpenRead(Utils.TestInputPath + "/mini_iso.bin").ReadAll()));
				FileStream = mapStream;
			}

			FileStreamAnalyzed = new ProxyStreamReadWriteAnalyzer(FileStream);
			Dvd9Xbox360 = new Dvd9Xbox360().Load(FileStreamAnalyzed);

			Assert.AreEqual(
				"EntryStruct(FullName='/snd/config.bin', Offset=366721, Size=3040, Attributes=File)",
				Dvd9Xbox360.RootEntry["/snd/config.bin"].ToString()
			);
			var Stream = Dvd9Xbox360.RootEntry["/snd/config.bin"].Open();
			var TextReaded = Stream.ReadString(4, Encoding.UTF8);

			Assert.AreEqual("nusc", TextReaded);

#if false
			if (GENERATE_MINI_ISO)
			{
				// Analyze.
				mapStream = FileStreamAnalyzed.ConvertSpacesToMapStream(FileStreamAnalyzed.ReadUsage.JoinWithThresold(Thresold: 32));
				SerializerUtils.SerializeToMemoryStream(mapStream.Serialize).CopyToFile(Utils.TestInputPath + "/mini_iso.bin");
			}
#endif
		}
	}
}
