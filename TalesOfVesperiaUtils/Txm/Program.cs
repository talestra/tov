﻿using System;
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
		[Command("-o", "--overwrite")]
		[Description("Overwrites existent files when extracting")]
		protected bool Overwrite = false;

		/// <summary>
		/// 
		/// </summary>
		[Command("-r", "--recursive")]
		[Description("Iterates over all subdirectories too")]
		protected bool Recursive = false;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_TxmPath"></param>
		/// <param name="OutputDirectory"></param>
		[Command("-u2", "--unpack2")]
		[Description("Unpacks a TXM/TXV file pair")]
		[Example("-u2 file.txm file.txv <folder>")]
		protected void ExtractTxmTxv(string TxmPath, string TxvPath = null, string OutputDirectory = null)
		{
			var BasePath = Path.GetDirectoryName(TxmPath);
			if (BasePath == "") BasePath = ".";

			try
			{
				var BaseFileName = Path.GetFileNameWithoutExtension(TxmPath);
				if (TxmPath == null)
				{
					TxmPath = BasePath + Path.DirectorySeparatorChar + BaseFileName + ".txm";
				}
				if (TxvPath == null)
				{
					TxvPath = BasePath + Path.DirectorySeparatorChar + BaseFileName + ".txv";
				}

				if (!File.Exists(TxmPath)) throw (new FileNotFoundException(String.Format("Can't find file '{0}'", TxmPath)));
				if (!File.Exists(TxvPath)) throw (new FileNotFoundException(String.Format("Can't find file '{0}'", TxmPath)));

				if (!File.Exists(TxmPath) && File.Exists(BasePath + Path.DirectorySeparatorChar + "0"))
				{
					TxmPath = BasePath + Path.DirectorySeparatorChar + "0";
					TxvPath = BasePath + Path.DirectorySeparatorChar + "1";
				}

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
					// Console.WriteLine(ImageEntry.ImageEntry);
					try
					{
						if (Overwrite || !File.Exists(ImageEntryFileName))
						{
							//ImageEntry.Bitmap.Save(ImageEntryFileName);
							var BitmapMixed = ImageEntry.Bitmap;
							var AlphaChannel = BitmapMixed.GetChannelsDataLinear(BitmapChannel.Alpha);
							var SolidAlphaChannel = Enumerable.Repeat((byte)0xFF, AlphaChannel.Length).ToArray();
							var BitmapAlpha = BitmapMixed.Clone(BitmapMixed.GetFullRectangle(), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
							var BitmapColor = BitmapMixed.Clone(BitmapMixed.GetFullRectangle(), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
							
							BitmapAlpha.SetChannelsDataLinear(SolidAlphaChannel, BitmapChannel.Alpha);
							
							BitmapColor.SetChannelsDataLinear(AlphaChannel, BitmapChannel.Red);
							BitmapColor.SetChannelsDataLinear(AlphaChannel, BitmapChannel.Green);
							BitmapColor.SetChannelsDataLinear(AlphaChannel, BitmapChannel.Blue);
							
							BitmapMixed.Save(ImageEntryFileName);
							BitmapColor.Save(ImageEntryFileName + ".color.png");
							BitmapAlpha.Save(ImageEntryFileName + ".alpha.png");
							Console.WriteLine("Ok");
						}
						else
						{
							Console.WriteLine("Exists");
						}
					}
					catch (Exception Exception)
					{
						Console.WriteLine("Error({0})", Exception.Message);
						//Console.Error.WriteLine(Exception);
					}
				}

				foreach (var ImageEntry in Txm.Surface3DEntries)
				{
					var ImageEntryFileNameTest = OutputDirectory + Path.DirectorySeparatorChar + BaseFileName + "." + ImageEntry.Name + "." + 0 + ".png";

					Console.Write("  {0}...", ImageEntry.Name);

					if (Overwrite || !File.Exists(ImageEntryFileNameTest))
					{
						int n = 0;
						foreach (var Bitmap in ImageEntry.Bitmaps.Bitmaps)
						{
							var ImageEntryFileName = OutputDirectory + Path.DirectorySeparatorChar + BaseFileName + "." + ImageEntry.Name + "." + n + ".png";
							Console.Write("  {0}.png : {1}...", ImageEntry.Name, ImageEntry.ImageEntry.ImageFileFormat.TextureFormat);
							//Console.WriteLine(ImageEntry.ImageEntry);

							try
							{
								if (Overwrite || !File.Exists(ImageEntryFileName))
								{
									//Bitmap.Save(ImageEntryFileName);
									var BitmapMixed = Bitmap;
									var AlphaChannel = BitmapMixed.GetChannelsDataLinear(BitmapChannel.Alpha);
									var SolidAlphaChannel = Enumerable.Repeat((byte)0xFF, AlphaChannel.Length).ToArray();
									var BitmapAlpha = BitmapMixed.Clone(BitmapMixed.GetFullRectangle(), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
									var BitmapColor = BitmapMixed.Clone(BitmapMixed.GetFullRectangle(), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
									
									BitmapColor.SetChannelsDataLinear(SolidAlphaChannel, BitmapChannel.Alpha);
									
									BitmapAlpha.SetChannelsDataLinear(AlphaChannel, BitmapChannel.Red);
									BitmapAlpha.SetChannelsDataLinear(AlphaChannel, BitmapChannel.Green);
									BitmapAlpha.SetChannelsDataLinear(AlphaChannel, BitmapChannel.Blue);
									BitmapAlpha.SetChannelsDataLinear(SolidAlphaChannel, BitmapChannel.Alpha);
									
									
									BitmapMixed.Save(ImageEntryFileName);
									BitmapColor.Save(ImageEntryFileName + ".color.png");
									BitmapAlpha.Save(ImageEntryFileName + ".alpha.png");
									Console.WriteLine("Ok");
								}
								else
								{
									Console.WriteLine("Exists");
								}
							}
							catch (Exception Exception)
							{
								Console.WriteLine("Error({0})", Exception.Message);
								//Console.Error.WriteLine(Exception);
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
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_TxmPath"></param>
		/// <param name="OutputDirectory"></param>
		[Command("-u", "--unpack")]
		[Description("Unpacks a TXM file")]
		[Example("-u file.txm <folder>")]
		protected void ExtractTxm(string TxmPath, string OutputDirectory = null)
		{
			var BasePath = Path.GetDirectoryName(TxmPath);
			if (BasePath == "") BasePath = ".";

			if (TxmPath.Contains('*'))
			{
				foreach (var FileName in Directory.EnumerateFiles(BasePath, Path.GetFileName(TxmPath), Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
				{
					ExtractTxm(FileName, OutputDirectory);
				}
				return;
			}

			ExtractTxmTxv(TxmPath, null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			new Program().Run(args);
		}
	}
}
