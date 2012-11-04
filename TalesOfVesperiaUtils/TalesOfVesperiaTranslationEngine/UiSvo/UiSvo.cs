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

				foreach (var Suffix in new[] { "", "DE", "FR" })
				{
					Patcher.GameGetTXM("COMALL" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_COMALLINFO00, "U_COMALLINFO00");
					});

					Patcher.GameGetTXM("ICONBTN360" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_ICONBTN36000, "U_ICONBTN36000");
					});

					Patcher.GameGetTXM("ICONKIND" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_ICONKIND00, "U_ICONKIND00");
					});

					Patcher.GameGetTXM("ICONSORT" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_ICONSORT00, "U_ICONSORT00");
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_ICONSORT01, "U_ICONSORT01");
					});

					Patcher.GameGetTXM("ICONSYM" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_ICONSYM01, "U_ICONSYM01");
					});

					Patcher.GameGetTXM("MENU" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_MENUBTLINFO00, "U_MENUBTLINFO00");
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_MENUBTLINFO02, "U_MENUBTLINFO02");
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_MENUSTR01, "U_MENUSTR01");
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
