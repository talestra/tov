using CSharpUtils;
using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.Imaging;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaTranslationEngine.UiSvo
{
	public class UiSvo : PatcherComponent
	{
		public UiSvo(Patcher Patcher)
			: base(Patcher)
		{
		}

		public void Handle()
		{
			Patcher.GameAccessPath("ui.svo", () =>
			{
				HandleFont();
				HandleImages();
			});
		}

		public void HandleFont()
		{
			// Font
			Patcher.GameGetTXM("FONTTEX10.TXM", "FONTTEX10.TXV", (Txm) =>
			{
				Patcher.PatcherGetImage("Data/FONTTEX10.FONTTEX10.15.png", (Bitmap) =>
				{
					var Entry = Txm.Surface3DEntries[0];

					Entry.Bitmaps.Bitmaps[15] = Bitmap;
					Entry.UpdateBitmapList(Entry.Bitmaps);
				});
			});

		}

		public void HandleImages()
		{
			Patcher.Action("Patching Images...", () =>
			{
				var PatcherDataFS = Patcher.PatcherDataFS;

				foreach (var Files in GetImageParts(PatcherDataFS, "UI_SVO"))
				{
					var File = Files.Key;
					Patcher.Action("Patching " + File, () =>
					{
						foreach (var BaseName in new[] { File, File + "DE", File + "FR" })
						{
							Patcher.GameGetTXM(BaseName + ".TXM", BaseName + ".TXV", (TXM) =>
							{
								foreach (var Item in Files.Value)
								{
									var PngFile = String.Format("{0}.{1}.png", File, Item);
									TXM.Surface2DEntriesByName[Item].UpdateBitmap(new Bitmap(Image.FromStream(PatcherDataFS.OpenFile("UI_SVO/" + PngFile, FileMode.Open))));
								}
							});
						}
					});
				}
			});
		}

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
	}
}
