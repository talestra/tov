using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TalesOfVesperiaUtils.Formats.Packages
{
	abstract public class BasePackage
	{
		abstract public void Load(Stream Stream);
		abstract public void Save(Stream Stream, bool DoAlign = true);
		public MemoryStream Save(bool DoAlign = true)
		{
			var Stream = new MemoryStream();
			Save(Stream);
			Stream.Position = 0;
			return Stream;
		}
	}
}
