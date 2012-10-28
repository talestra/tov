using CSharpUtils;
using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Imaging;

namespace TalesOfVesperiaTranslationEngine.UiSvo.ComAllTxm
{
	public class ComAllTxm
	{
		static protected Dictionary<string, List<string>> GetImageParts(FileSystem PatcherDataFS, string Path)
		{
			var PARTS = new Dictionary<string, List<string>>();
			foreach (var Entry in PatcherDataFS.FindFiles(Path, new Wildcard("*.png")))
			{
				var NameParts = Entry.Name.Split('.');
				var File = NameParts[0].ToUpper();
				var Item = NameParts[1].ToUpper();

				if (!PARTS.ContainsKey(File)) PARTS[File] = new List<string>();
				PARTS[File].Add(Item);
			}
			return PARTS;
		}

		public void Handle(Patcher Patcher, FileSystem UiSvo)
		{
			var PatcherDataFS = Patcher.PatcherDataFS;

			foreach (var Files in GetImageParts(PatcherDataFS, "UI_SVO"))
			{
				var File = Files.Key;
				Patcher.Action("Patching " + File, () =>
				{
					foreach (var BaseName in new[] { File, File + "DE", File + "FR" })
					{
						var TXM = new TXM().Load(UiSvo.OpenFile(BaseName + ".TXM", FileMode.Open), UiSvo.OpenFile(BaseName + ".TXV", FileMode.Open));
						foreach (var Item in Files.Value)
						{
							var PngFile = String.Format("{0}.{1}.png", File, Item);
							TXM.Surface2DEntriesByName[Item].UpdateBitmap(new Bitmap(Image.FromStream(PatcherDataFS.OpenFile("UI_SVO/" + PngFile, FileMode.Open))));
						}
					}
				});
			}
		}
	}
}
