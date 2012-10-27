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
		public void Handle(Patcher Patcher, FileSystem UiSvo)
		{
			Patcher.Action("Patching COMALL", () =>
			{
				foreach (var BaseName in new[] { "COMALL", "COMALLDE", "COMALLFR" })
				{
					var TXM = new TXM().Load(UiSvo.OpenFile(BaseName + ".TXM", FileMode.Open), UiSvo.OpenFile(BaseName + ".TXV", FileMode.Open));
					TXM.Surface2DEntriesByName["U_COMALLINFO00"].UpdateBitmap(new Bitmap(Image.FromStream(Patcher.PatcherDataFS.OpenFile("U_COMALLINFO00.png", FileMode.Open))));
				}
			});

			Patcher.Action("Patching MENU", () =>
			{
				foreach (var BaseName in new[] { "MENU", "MENUDE", "MENUFR" })
				{
					var TXM = new TXM().Load(UiSvo.OpenFile(BaseName + ".TXM", FileMode.Open), UiSvo.OpenFile(BaseName + ".TXV", FileMode.Open));
					TXM.Surface2DEntriesByName["U_MENUBTLINFO00"].UpdateBitmap(new Bitmap(Image.FromStream(Patcher.PatcherDataFS.OpenFile("MENU.U_MENUBTLINFO00.png", FileMode.Open))));
					TXM.Surface2DEntriesByName["U_MENUBTLINFO02"].UpdateBitmap(new Bitmap(Image.FromStream(Patcher.PatcherDataFS.OpenFile("MENU.U_MENUBTLINFO02.png", FileMode.Open))));
					TXM.Surface2DEntriesByName["U_MENUSTR01"].UpdateBitmap(new Bitmap(Image.FromStream(Patcher.PatcherDataFS.OpenFile("MENU.U_MENUSTR01.png", FileMode.Open))));
				}
			});

			Patcher.Action("Patching ICONSORT", () =>
			{
				foreach (var BaseName in new[] { "ICONSORT", "ICONSORTDE", "ICONSORTFR" })
				{
					var TXM = new TXM().Load(UiSvo.OpenFile(BaseName + ".TXM", FileMode.Open), UiSvo.OpenFile(BaseName + ".TXV", FileMode.Open));
					TXM.Surface2DEntriesByName["U_ICONSORT00"].UpdateBitmap(new Bitmap(Image.FromStream(Patcher.PatcherDataFS.OpenFile("U_ICONSORT00.png", FileMode.Open))));
					TXM.Surface2DEntriesByName["U_ICONSORT01"].UpdateBitmap(new Bitmap(Image.FromStream(Patcher.PatcherDataFS.OpenFile("U_ICONSORT01.png", FileMode.Open))));
				}
			});

			Patcher.Action("Patching ICONBTN360", () =>
			{
				foreach (var BaseName in new[] { "ICONBTN360", "ICONBTN360DE", "ICONBTN360FR" })
				{
					var TXM = new TXM().Load(UiSvo.OpenFile(BaseName + ".TXM", FileMode.Open), UiSvo.OpenFile(BaseName + ".TXV", FileMode.Open));
					TXM.Surface2DEntriesByName["U_ICONBTN36000"].UpdateBitmap(new Bitmap(Image.FromStream(Patcher.PatcherDataFS.OpenFile("U_ICONBTN36000.png", FileMode.Open))));
				}
			});

			Patcher.Action("Patching ICONSYM", () =>
			{
				//ERROR_ADSADAS_TO_CONTINUË_WORKING_HERE!"··!"·!"$!""!!!
			});

			// ICONSYM
			// ICONKIND
			// EVENTMAP
		}
	}
}
