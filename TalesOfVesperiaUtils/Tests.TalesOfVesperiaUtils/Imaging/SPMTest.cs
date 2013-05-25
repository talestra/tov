using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaUtils.Imaging;
using System.Drawing;
using CSharpUtils;
using System.IO;
using CSharpUtils.Drawing;
using System.Runtime.InteropServices;
using TalesOfVesperiaUtils.Imaging.Internal;

namespace TalesOfVesperiaTests.Imaging
{
	[TestClass]
	unsafe public class SPMTest
	{
		[TestMethod]
		public void Test1()
		{
			var spm = new SPM();
			using (var SpmStream = File.OpenRead(Utils.TestInputPath + @"\MDL_FRI_I00_L.SPM"))
			{
				spm.Load(SpmStream);
			}
		}
	}
}
