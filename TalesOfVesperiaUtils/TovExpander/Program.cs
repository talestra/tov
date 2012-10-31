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
using TalesOfVesperiaUtils.Formats.Script;
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

		private void ShowException(Exception Exception)
		{
			Console.WriteLine("  ERROR: {0}", Verbose ? Exception.ToString() : Exception.Message.ToString());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="GameFolder"></param>
		[Command("-xa", "--expand-all")]
		[Description("Expands/decompresses all files in tov")]
		[Example("-xa <.>")]
		protected void ExpandAll(string GameFolder = null)
		{
			if (GameFolder == null) GameFolder = ".";
			Expand(GameFolder + "/*.svo");
			Expand(GameFolder + "/language/*.dat");
			Expand(GameFolder + "/language/*.so");
			Expand(GameFolder + "/snd/*.dat");
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
			if (FilePath.Contains('*') || FilePath.Contains('?'))
			{
				var BasePath = Path.GetDirectoryName(FilePath);
				var Recursive = false;
				if (BasePath == "") BasePath = ".";
				foreach (var FileName in Directory.EnumerateFiles(BasePath, Path.GetFileName(FilePath), Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
				{
					Expand(FileName);
				}
				return;
			}


			var ListToExpand = new List<string>();

			//Console.WriteLine("Expanding '{0}'...", FilePath);

			using (var _FileStream = File.OpenRead(FilePath))
			{
				if (_FileStream.Length == 0)
				{
					//Console.WriteLine("EMPTY: {0}", FilePath);
					return;
				}
				var FileStream = DecompressIfCompressedStream(_FileStream);
				var MagicData = FileStream.Slice().ReadBytesUpTo(0x100);

				if (false)
				{
				}
				else if (TO8SCEL.IsValid(MagicData))
				{
					try
					{
						var To8Scel = new TO8SCEL(FileStream);
						foreach (var Entry in To8Scel)
						{
							var EntryFilePath = FilePath + ".d/" + Entry.Index;
							if (Overwrite || !File.Exists(EntryFilePath))
							{
								Console.WriteLine("{0}", EntryFilePath);
								try
								{
									var EntryStream = DecompressIfCompressedStream(Entry.CompressedStream);
									if (EntryStream.Length > 0)
									{
										EntryStream.CopyToFile(EntryFilePath);
									}
								}
								catch (Exception Exception)
								{
									ShowException(Exception);
								}
							}
							if (File.Exists(EntryFilePath)) ListToExpand.Add(EntryFilePath);
						}
					}
					catch (Exception Exception)
					{
						ShowException(Exception);
					}
				}
				else if (FPS4.IsValid(MagicData))
				{
					//Console.WriteLine("FPS4");
					try
					{
						var Fps4 = new FPS4(FileStream);
						foreach (var Entry in Fps4)
						{
							var EntryFilePath = FilePath + ".d/" + Entry.Name;
							if (Overwrite || !File.Exists(EntryFilePath))
							{
								Console.WriteLine("{0}", EntryFilePath);
								try
								{
									var EntryStream = DecompressIfCompressedStream(Entry.Open());
									if (EntryStream.Length > 0)
									{
										EntryStream.CopyToFile(EntryFilePath);
									}
								}
								catch (Exception Exception)
								{
									ShowException(Exception);
								}
							}
							if (File.Exists(EntryFilePath)) ListToExpand.Add(EntryFilePath);
						}
					}
					catch (Exception Exception)
					{
						ShowException(Exception);
					}
				}
				else if (TSS.IsValid(MagicData))
				{
					int RoomId = 0;
					try { RoomId = int.Parse(Path.GetFileNameWithoutExtension(FilePath)); }
					catch { }
					var TxtFile = FilePath + ".txt";
					Console.WriteLine("{0}", TxtFile);
					if (Overwrite || !File.Exists(TxtFile))
					{
						using (var TxtStream = File.Open(TxtFile, FileMode.Create, FileAccess.Write))
						using (var TextWriter = new StreamWriter(TxtStream))
						{
							try
							{
								var Tss = new TSS().Load(FileStream);

								foreach (var Entry in Tss.ExtractTexts())
								{
									if (Entry == null)
									{
										TextWriter.WriteLine("---------------------------------------------");
									}
									else
									{
										TextWriter.WriteLine(
											"{0:X8}:[{1}]:[{2}]",
											Entry.Id,
											Entry.Original.Select(Item => "'" + Item.EscapeString() + "'").Implode(","),
											Entry.Translated.Select(Item => "'" + Item.EscapeString() + "'").Implode(",")
										);
									}
								}

								/*
								foreach (var Instruction in Tss.ReadInstructions())
								{
									Console.WriteLine(Instruction);
								}
								foreach (var Instruction in Tss.PushArrayInstructionNodes)
								{
									TextWriter.WriteLine("scenario/{0:D4}@{1:X8}@{2:X8}:", RoomId, Instruction.InstructionPosition, Instruction.ArrayPointer + Tss.Header.TextStart);
									try
									{
										foreach (var Element in Instruction.Elements)
										{
											TextWriter.WriteLine("'{0}'", StringExtensions.EscapeString(Element.ToString()));
										}
									}
									catch (Exception Exception)
									{
										ShowException(Exception);
									}
									TextWriter.WriteLine("");
									//Console.WriteLine("{0}", Instruction.Elements);
								}
								//Console.WriteLine("TSS");
								*/
							}
							catch (Exception Exception)
							{
								ShowException(Exception);
							}
						}
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
					/*
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
					*/

					foreach (var Entry in Txm.Surface2DEntries)
					{
						var ImagePath = BasePath + "." + Entry.Name + ".png";
						if (Overwrite || !File.Exists(ImagePath))
						{
							try
							{
								Entry.Bitmap.Save(ImagePath);
							}
							catch (Exception Exception)
							{
								ShowException(Exception);
							}
						}
					}

					foreach (var Entry in Txm.Surface3DEntries)
					{
						var ImagePath0 = BasePath + "." + Entry.Name + "." + 0 + ".png";
						if (Overwrite || !File.Exists(ImagePath0))
						{
							try
							{
								var n = 0;
								foreach (var Bitmap in Entry.Bitmaps.Bitmaps)
								{
									var ImagePath = BasePath + "." + Entry.Name + "." + n + ".png";
									Console.WriteLine("{0}", ImagePath);
									if (Overwrite || !File.Exists(ImagePath))
									{
										Bitmap.Save(ImagePath);
									}
									n++;
								}
							}
							catch (Exception Exception)
							{
								ShowException(Exception);
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
