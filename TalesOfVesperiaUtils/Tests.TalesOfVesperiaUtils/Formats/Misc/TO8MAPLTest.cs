using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaUtils.Formats.Misc;
using System.IO;
using TalesOfVesperiaTests;

namespace Tests.TalesOfVesperiaUtils.Formats.Misc
{
	[TestClass]
	public class TO8MAPLTest
	{
		[TestMethod]
		public void TestMethod1()
		{
			var MapList = TO8MAPL.Parse(File.OpenRead(Utils.TestInputPath + @"\MAPLIST.DAT"));

			var BasePath = @"C:\vesperia\vesperia\language\scenario_uk.dat.d";
			var OutPath = @"C:\vesperia\vesperia\language\scenario_uk.dat.d\out";

			try { Directory.CreateDirectory(OutPath); } catch {}

			foreach (var Entry in MapList.Entries)
			{
				Console.WriteLine("{0}: {1}, {2}, {3}, {4} : {5:X8}", Entry.Index, Entry.NoDummyText, Entry.Text1, Entry.Text2, Entry.Text3, Entry.Flags);
				if (File.Exists(BasePath + @"\" + Entry.Index))
				{
					//File.Copy(BasePath + @"\" + Entry.Index, OutPath + @"\" + Entry.NoDummyText, true);
					File.Copy(BasePath + @"\" + Entry.Index, OutPath + @"\" + Entry.Text3, true);
				}
			}
		}
	}
}
