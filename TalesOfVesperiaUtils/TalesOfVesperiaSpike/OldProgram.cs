using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaSpike
{
#if false
	public class OldProgram
	{
		static string VesperiaFolder = @"C:\isos\360\vesperia";

#if true
		static void UncompressUiImage(string BaseFileName)
		{
			//Console.WriteLine();
			//break;

			//var BaseFileName = "COMALL";



			var Txm = (new TXM()).Load(
				File.OpenRead(VesperiaFolder + @"\UI.svo.d\" + BaseFileName + ".TXM"),
				File.OpenRead(VesperiaFolder + @"\UI.svo.d\" + BaseFileName + ".TXV")
			);
			//Console.WriteLine(Txm.ImageEntries.ToStringArray("\n"));
			Console.WriteLine("{0}:", BaseFileName);
			foreach (var ImageEntry in Txm.Surface2DEntries)
			{
				var ImageEntryFileName = VesperiaFolder + @"\UI.svo.d\images\" + BaseFileName + "." + ImageEntry.Name + ".png";
				Console.Write("  {0}.png : {1}...", ImageEntry.Name, ImageEntry.ImageEntry.ImageFileFormat.TextureFormat);
				Console.WriteLine(ImageEntry.ImageEntry);
				if (!File.Exists(ImageEntryFileName))
				{
					ImageEntry.Bitmap.Save(ImageEntryFileName);
					Console.WriteLine("Ok");
				}
				else
				{
					Console.WriteLine("Exists");
				}
			}

			foreach (var ImageEntry in Txm.Surface3DEntries)
			{
				var ImageEntryFileNameTest = VesperiaFolder + @"\UI.svo.d\images\" + BaseFileName + "." + ImageEntry.Name + "." + 0 + ".png";

				Console.Write("  {0}...", ImageEntry.Name);

				if (!File.Exists(ImageEntryFileNameTest))
				{
					int n = 0;
					foreach (var Bitmap in ImageEntry.Bitmaps.Bitmaps)
					{
						var ImageEntryFileName = VesperiaFolder + @"\UI.svo.d\images\" + BaseFileName + "." + ImageEntry.Name + "." + n + ".png";
						Console.Write("  {0}.png : {1}...", ImageEntry.Name, ImageEntry.ImageEntry.ImageFileFormat.TextureFormat);
						//Console.WriteLine(ImageEntry.ImageEntry);

						if (!File.Exists(ImageEntryFileName))
						{
							Bitmap.Save(ImageEntryFileName);
							Console.WriteLine("Ok");
						}
						else
						{
							Console.WriteLine("Exists");
						}
						n++;
					}
					Console.WriteLine("Ok");
				}
				else
				{
					Console.WriteLine("Exists");
				}
			}
		}

		static void Main(string[] Args)
		{
			//var Image = TXM.LoadAbgr(File.OpenRead(@"C:\isos\360\vesperia\UI.svo.d\COMALL.TXV"), 256, 1024);

			//var BaseFileName = "COOK_BEAFSTEW";
			//var BaseFileName = "WORLDMAP";

#if true
			foreach (var BaseFullFileName in Directory.GetFiles(VesperiaFolder + @"\UI.svo.d", "*.TXM", SearchOption.TopDirectoryOnly))
			{
				UncompressUiImage(new FileInfo(BaseFullFileName).Name.Substr(0, -4));
			}
#else
			//UncompressUiImage("FONTTEX00");
			UncompressUiImage("WORLDMAP");
#endif

			Console.ReadKey();
			//Image.Save(@"C:\isos\360\vesperia\UI.svo.d\COMALL.TXV.png");
		}
#else
		static void Main(string[] Args)
		{
			var Image = new Bitmap(@"C:\projects\talestra_tov\icons0.png");
			using (var Dxt5Stream = File.OpenWrite(@"C:\projects\talestra_tov\icons0.dxt5"))
			{
				DXT5.SaveSwizzled2D(Image, Dxt5Stream, CompressDXT5.CompressionMode.HighQuality);
			}

			var Image2 = DXT5.LoadSwizzled2D(File.OpenRead(@"C:\projects\talestra_tov\icons0.dxt5"), Image.Width, Image.Height);
			Image2.Save(@"C:\projects\talestra_tov\icons0.dxt5.png");

			Console.WriteLine("END");
			Console.ReadKey();
		}
#endif
	}
#endif
}