using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaTranslationEngine.CommonSvo
{
	public class CommonSvo
	{
		static public void Handle(Patcher Patcher, FileSystem RootFS)
		{
			Patcher.Action("common.svo", () =>
			{
				FileSystem CommonSvo = null;
				Patcher.Action("Reading common.svo", () =>
				{
					CommonSvo = new FPS4FileSystem(new FPS4(RootFS.OpenFile("common.svo", FileMode.Open)));
				});
				TextureDat.TextureDat.Handle(Patcher, CommonSvo);
			});
		}
	}
}
