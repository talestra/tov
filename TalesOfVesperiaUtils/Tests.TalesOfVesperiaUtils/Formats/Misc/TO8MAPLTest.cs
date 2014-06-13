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

			var BasePath = @"C:\vesperia\vesperia\language\scenario_us.dat.d";
			var OutPath = @"C:\vesperia\vesperia\language\scenario_us.dat.d\out";

			try { Directory.CreateDirectory(OutPath); } catch {}

			foreach (var Entry in MapList.Entries)
			{
				Console.WriteLine("{0}: [MapSvoName: {1}], [CharaSvoName: {2}], [Name: {3}] : {4:X8}", Entry.Index, Entry.MapSvoName, Entry.CharaSvoName, Entry.Name, Entry.Flags);
				if (File.Exists(BasePath + @"\" + Entry.Index))
				{
					//File.Copy(BasePath + @"\" + Entry.Index, OutPath + @"\" + Entry.NoDummyText, true);
					File.Copy(BasePath + @"\" + Entry.Index, OutPath + @"\" + Entry.Name, true);
				}
			}
		}
	}
}
