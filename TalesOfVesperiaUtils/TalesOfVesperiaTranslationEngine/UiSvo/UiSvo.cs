using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaTranslationEngine.UiSvo
{
	public class UiSvo
	{
		public void Handle(Patcher Patcher, FileSystem RootFS)
		{
			Patcher.Action("ui.svo", () =>
			{
				FileSystem UiSvo = null;
				Patcher.Action("Reading ui.svo", () =>
				{
					UiSvo = new FPS4FileSystem(new FPS4(RootFS.OpenFile("ui.svo", FileMode.Open)));
				});
				new ComAllTxm.ComAllTxm().Handle(Patcher, UiSvo);
			});
		}
	}
}
