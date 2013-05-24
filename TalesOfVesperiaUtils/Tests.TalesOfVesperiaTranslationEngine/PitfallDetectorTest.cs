using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaTranslationEngine;
using Newtonsoft.Json;
using TalesOfVesperiaUtils.Text;

namespace TalesOfVesperiaTests
{
	[TestClass]
	public class PitfallDetectorTest
	{
		[TestMethod]
		public void TestMethod1()
		{
			var Messages = PitfallDetector.Detect("<VOICE>(test Hello!. Test <VOICE>(demo)");
			Assert.AreEqual(
				"[{'Severity':2,'Text':'Voice parameter doesn't have a parameter at 0'}]",
				JsonConvert.SerializeObject(Messages).Replace('"', '\'')
			);
		}
	}
}
