using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Getopt;
using TalesOfVesperiaUtils.Imaging;

namespace Txm
{
	/// <summary>
	/// 
	/// </summary>
	class Program : GetoptCommandLineProgram
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="_TxmPath"></param>
		/// <param name="OutputDirectory"></param>
		[Command("-u", "--unpack")]
		[Description("Unpacks a TXM file")]
		[Example("-u file.txm <folder>")]
		protected void ExtractSvo(string TxmPath, string OutputDirectory = null)
		{
			var BasePath = Path.GetDirectoryName(TxmPath);

			if (TxmPath.Contains('*'))
			{
				foreach (var FileName in Directory.EnumerateFiles(BasePath, Path.GetFileName(TxmPath), SearchOption.TopDirectoryOnly))
				{
					ExtractSvo(FileName, OutputDirectory);
				}
				return;
			}
			//Wildcard TxmPath2;
			if (!File.Exists(TxmPath)) throw(new FileNotFoundException(String.Format("Can't find file '{0}'", TxmPath)));
			var BaseFileName = Path.GetFileNameWithoutExtension(TxmPath);
			
			TxmPath = BasePath + Path.DirectorySeparatorChar + BaseFileName + ".txm";
			var TxvPath = BasePath + Path.DirectorySeparatorChar + BaseFileName + ".txv";
			var Txm = (new TXM()).Load(File.OpenRead(TxmPath), File.OpenRead(TxvPath));

			if (OutputDirectory == null)
			{
				OutputDirectory = BasePath + Path.DirectorySeparatorChar + "extracted";
			}

			if (!Directory.Exists(OutputDirectory)) Directory.CreateDirectory(OutputDirectory);

#if true
			Console.WriteLine("{0}:", BaseFileName);
			foreach (var ImageEntry in Txm.Surface2DEntries)
			{
				var ImageEntryFileName = OutputDirectory + Path.DirectorySeparatorChar + BaseFileName + "." + ImageEntry.Name + ".png";
				Console.Write("  {0}.png : {1}...", ImageEntry.Name, ImageEntry.ImageEntry.ImageFileFormat.TextureFormat);
				//
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
				var ImageEntryFileNameTest = OutputDirectory + Path.DirectorySeparatorChar + BaseFileName + "." + ImageEntry.Name + "." + 0 + ".png";

				Console.Write("  {0}...", ImageEntry.Name);

				if (!File.Exists(ImageEntryFileNameTest))
				{
					int n = 0;
					foreach (var Bitmap in ImageEntry.Bitmaps.Bitmaps)
					{
						var ImageEntryFileName = OutputDirectory + Path.DirectorySeparatorChar + BaseFileName + "." + ImageEntry.Name + "." + n + ".png";
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
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			Console.WriteLine(Path.GetDirectoryName(@"c:\temp\test.bin"));
			new Program().Run(args);
		}
	}
}
