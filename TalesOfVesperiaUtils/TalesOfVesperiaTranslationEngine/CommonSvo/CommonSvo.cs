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
		public void Handle(Patcher Patcher, FileSystem GameRootFS)
		{
			Patcher.Action("common.svo", () =>
			{
				FileSystem CommonSvo = null;
				Patcher.Action("Reading common.svo", () =>
				{
					CommonSvo = new FPS4FileSystem(new FPS4(GameRootFS.OpenFile("common.svo", FileMode.Open)));
				});
				TextureDat.TextureDat.Handle(Patcher, CommonSvo);
			});
		}
	}
}
