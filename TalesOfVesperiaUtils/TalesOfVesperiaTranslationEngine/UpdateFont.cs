using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TalesOfVesperiaUtils.Formats.Packages;
using System.IO;
using System.Drawing;
using TalesOfVesperiaUtils.Imaging;

namespace TalesOfVesperiaTranslationEngine
{
	public class UpdateFont
	{
		public void Process()
		{
			Process(
				PatchFolders.ExtractedPath + @"\UI.svo",
				"../../../TestInput/FONTTEX10.TXV.mod.png"
			);
		}

		public void Process(string UI_SVO, string FontTex10ModifiedPng)
		{
			var UI_SVO_BAK = PatchUtils.BackupIfNotBackuped(UI_SVO);

			using (var UI_SVO_Stream = File.Open(UI_SVO, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			using (var FontTex10ModifiedPng_Stream = File.OpenRead(FontTex10ModifiedPng))
			{
				Process(UI_SVO_Stream, FontTex10ModifiedPng_Stream);
			}
		}

		public void Process(Stream UI_SVO, Stream FontTex10ModifiedPng)
		{
			Console.Write("Updating Font...");
			var Fps4 = new FPS4(UI_SVO);
			using (var Out = Fps4["FONTTEX10.TXV"].Open())
			{
				(new DXT5()).SaveSwizzled2D(new Bitmap(Image.FromStream(FontTex10ModifiedPng)), Out);
			}
			Console.WriteLine("Ok");
		}
	}
}
