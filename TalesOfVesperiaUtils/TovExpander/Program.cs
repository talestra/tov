using CSharpUtils.Getopt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Audio;
using TalesOfVesperiaUtils.Compression;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.Imaging;

namespace TovExpander
{
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
		[Command("-v", "--verbose")]
		[Description("Displays additional information")]
		protected bool Verbose = false;

		private Stream DecompressIfCompressedStream(Stream FileStream)
		{
			var MagicData = FileStream.Slice().ReadBytesUpTo(0x100);

			// Decompress if compressed
			try
			{
				if (TalesCompression.DetectVersion(MagicData, FileStream.Length) != -1)
				{
					return TalesCompression.DecompressStream(FileStream);
				}
			}
			catch
			{
			}

			return FileStream;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FilePath"></param>
		[Command("-x", "--expand")]
		[Description("Expands/decompresses a FPS4 file recursively and extracts image files/text files")]
		[Example("-x btl.svo")]
		protected void Expand(string FilePath)
		{
			var ListToExpand = new List<string>();

			Console.WriteLine("Expanding '{0}'...", FilePath);

			using (var _FileStream = File.OpenRead(FilePath))
			{
				if (_FileStream.Length == 0)
				{
					//Console.WriteLine("EMPTY: {0}", FilePath);
					return;
				}
				var FileStream = DecompressIfCompressedStream(_FileStream);
				var MagicData = FileStream.Slice().ReadBytesUpTo(0x100);

				if (FPS4.IsValid(MagicData))
				{
					//Console.WriteLine("FPS4");
					try
					{
						var Fps4 = new FPS4(FileStream);
						foreach (var Entry in Fps4)
						{
							var EntryFilePath = FilePath + ".d/" + Entry.Name;
							Console.WriteLine("{0}", EntryFilePath);
							if (Overwrite || !File.Exists(EntryFilePath))
							{
								try
								{
									var EntryStream = DecompressIfCompressedStream(Entry.Open());
									EntryStream.CopyToFile(EntryFilePath);
								}
								catch (Exception Exception)
								{
									Console.WriteLine("  ERROR: {0}", Verbose ? Exception.ToString() : Exception.Message.ToString());
								}
							}
							ListToExpand.Add(EntryFilePath);
						}
					}
					catch (Exception Exception)
					{
						Console.WriteLine("  ERROR: {0}", Verbose ? Exception.ToString() : Exception.Message.ToString());
					}
				}
				else if (TO8CHTX.IsValid(MagicData))
				{
					var Chtx = new TO8CHTX(FileStream);
					var TxtFile = FilePath + ".txt";
					Console.WriteLine("{0}", TxtFile);
					if (Overwrite || !File.Exists(TxtFile))
					{
						using (var TxtStream = File.Open(TxtFile, FileMode.Create, FileAccess.Write))
						using (var TextWriter = new StreamWriter(TxtStream))
						{
							foreach (var Entry in Chtx.Entries)
							{
								TextWriter.WriteLine("{0}", Entry.Title);
								TextWriter.WriteLine("{0}", Entry.TextOriginal);
								TextWriter.WriteLine("{0}", Entry.TextTranslated);
								TextWriter.WriteLine("");
							}
						}
						//Chtx.Entries[0].Title
						//Console.WriteLine("CHAT!");
					}
				}
				else if (SE3.IsValid(MagicData))
				{
					var Se3 = new SE3().Load(FileStream);
					foreach (var Entry in Se3.Entries)
					{
						var EntryFullNameXma = FilePath + "." + Entry.Name + ".xma";
						var EntryFullNameWav = FilePath + "." + Entry.Name + ".wav";
						Console.WriteLine("{0}", EntryFullNameXma);
						if (Overwrite || !File.Exists(EntryFullNameXma))
						{
							Entry.ToXmaWav().CopyToFile(EntryFullNameXma);
						}
						if (Overwrite || !File.Exists(EntryFullNameWav))
						{
							using (var WavOut = File.Open(EntryFullNameWav, FileMode.Create, FileAccess.Write))
							{
								Entry.ToWav(WavOut);
							}
						}
					}

				}
				else if (TXM.IsValid(MagicData))
				{
					string BasePath;
					string TxmPath;
					string TxvPath;

					if (Path.GetExtension(FilePath).ToLower() == ".txm")
					{
						BasePath = Path.GetDirectoryName(FilePath) + "/" + Path.GetFileNameWithoutExtension(FilePath);
						TxmPath = BasePath + ".txm";
						TxvPath = BasePath + ".txv";
					}
					else
					{
						var DirectoryPath = Path.GetDirectoryName(FilePath);
						TxmPath = DirectoryPath + "/" + Path.GetFileName(FilePath);
						TxvPath = DirectoryPath + "/" + (int.Parse(Path.GetFileName(TxmPath)) + 1);
						BasePath = TxmPath;
					}

					var Txm = TXM.FromTxmTxv(File.OpenRead(TxmPath), File.OpenRead(TxvPath));
					if (Txm.Surface2DEntries.Length > 0 && Txm.Surface3DEntries.Length > 0)
					{
						// 3D and 2D surfaces
						//Console.WriteLine("ERROR 3D and 2D SURFACES! (2D: {0}, 3D: {1})", Txm.Surface2DEntries.Length, Txm.Surface3DEntries.Length);
					}
					else if (Txm.Surface2DEntries.Length > 0)
					{
						// 2D Surfaces
						//Console.WriteLine("2D SURFACES! {0}", Txm.Surface2DEntries.Length);
					}
					else if (Txm.Surface3DEntries.Length > 0)
					{
						// 3D Surfaces
						//Console.WriteLine("3D SURFACES! {0}", Txm.Surface3DEntries.Length);
					}

					foreach (var Entry in Txm.Surface2DEntries)
					{
						var ImagePath = BasePath + "." + Entry.Name + ".png";
						if (Overwrite || !File.Exists(ImagePath))
						{
							Entry.Bitmap.Save(ImagePath);
						}
					}

					foreach (var Entry in Txm.Surface3DEntries)
					{
						var ImagePath0 = BasePath + "." + Entry.Name + "." + 0 + ".png";
						if (Overwrite || !File.Exists(ImagePath0))
						{
							var n = 0;
							foreach (var Bitmap in Entry.Bitmaps.Bitmaps)
							{
								var ImagePath = BasePath + "." + Entry.Name + "." + n + ".png";
								if (Overwrite || !File.Exists(ImagePath))
								{
									Bitmap.Save(ImagePath);
								}
								n++;
							}
						}
					}
				}
				else
				{
				}
			}

			// Expand all the queued stuff
			foreach (var Item in ListToExpand)
			{
				try
				{
					Expand(Item);
				}
				catch (Exception Exception)
				{
					Console.WriteLine("  ERROR: {0}", Verbose ? Exception.ToString() : Exception.Message.ToString());
				}
			}
		}

		static void Main(string[] args)
		{
			new Program().Run(args);
		}
	}
}
