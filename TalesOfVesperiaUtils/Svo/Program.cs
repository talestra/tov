using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpUtils.Getopt;
using TalesOfVesperiaUtils;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.Compression;
using System.Text.RegularExpressions;

namespace Svo
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
		[Command("-v", "--verbose")]
		[Description("Displays additional information")]
		protected bool Verbose = false;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="OutputFileName"></param>
		/// <param name="InputStreamGet"></param>
		private void _ExtractFile(string OutputFileName, Func<Stream> InputStreamGet, int Offset, int Length)
		{
			if (Verbose)
			{
				Console.Write("{0}({1:X8}[{2}])...", OutputFileName, Offset, Length);
			}
			else
			{
				Console.Write("{0}...", OutputFileName);
			}
			try
			{
				if (!File.Exists(OutputFileName) || Overwrite)
				{
					using (var InputStream = InputStreamGet())
					{
						InputStream.CopyToFile(OutputFileName);
					}
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
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DatPath"></param>
		/// <param name="DavPath"></param>
		/// <param name="OutputDirectory"></param>
		protected void ExtractSvo2(string DatPath, string DavPath, string OutputDirectory = null)
		{
			if (DavPath == null) DavPath = DatPath;

			if (OutputDirectory == null)
			{
				OutputDirectory = DatPath + ".d";
			}

			Console.WriteLine("Loading {0}...", DatPath);
			//try
			//{
			using (var _Stream1 = File.OpenRead(DatPath))
			using (var Stream2 = File.OpenRead(DavPath))
			{
				var Stream1 = (Stream)_Stream1;
				try { Directory.CreateDirectory(OutputDirectory); }
				catch { }

				int Compressed = TalesCompression.DetectVersion(Stream1.Slice().ReadBytes(16), Stream1.Length);
				if (Compressed >= 0)
				{
					Stream1 = TalesCompression.DecompressStream(Stream1);
				}

				if (Stream1.SliceWithLength().ReadString(7) == "TO8SCEL")
				{
					var TO8SCEL = new TO8SCEL(Stream1);

					foreach (var Entry in TO8SCEL)
					{
						_ExtractFile(OutputDirectory + "/" + Entry.Index, () => Entry.UncompressedStream, (int)(uint)Entry.EntryStruct.Offset, (int)(uint)Entry.EntryStruct.LengthCompressed);
					}
				}
				else
				{
					var FPS4 = new FPS4(Stream1, Stream2);

					Console.WriteLine("{0}", FPS4);

					foreach (var Entry in FPS4)
					{
						_ExtractFile(OutputDirectory + "/" + Entry.Name, () => Entry.Open(), (int)(uint)Entry.EntryStruct.Offset, (int)(uint)Entry.EntryStruct.LengthReal);
					}
				}
			}
			//}
			//catch (Exception Exception)
			//{
			//	Console.Error.WriteLine("{0}", Exception);
			//}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SvoPath"></param>
		/// <param name="OutputDirectory"></param>
		[Command("-l", "--list")]
		[Description("Lists a FPS4/TO8SCEL file")]
		[Example("-l UI.svo")]
		protected void ExtractSvo(string SvoPath)
		{
			//try
			//{
			using (var _SvoStream = File.OpenRead(SvoPath))
			{
				Stream SvoStream = _SvoStream;
				int Compressed = TalesCompression.DetectVersion(SvoStream.Slice().ReadBytes(16), SvoStream.Length);
				if (Compressed >= 0)
				{
					SvoStream = TalesCompression.DecompressStream(SvoStream);
				}

				if (SvoStream.SliceWithLength().ReadString(7) == "TO8SCEL")
				{
					var TO8SCEL = new TO8SCEL(SvoStream);

					foreach (var Entry in TO8SCEL)
					{
						Console.WriteLine("{0} ... Start: {1}, End: {2}, Length: {3}", Entry.Index, Entry.EntryStruct.Offset, Entry.EntryStruct.Offset + Entry.EntryStruct.LengthCompressed, Entry.EntryStruct.LengthCompressed);
					}
				}
				else
				{
					var FPS4 = new FPS4(SvoStream);

					Console.WriteLine("{0}", FPS4);

					foreach (var Entry in FPS4)
					{
						Console.WriteLine("{0} ... Start: {1}, End: {2}, Length: {3}", Entry.Name, Entry.EntryStruct.Offset, Entry.EntryStruct.Offset + Entry.EntryStruct.LengthReal, Entry.EntryStruct.LengthReal);
					}
				}
			}
		}

		[Command("--csvhandle")]
		[Description("Processes a ProcessMonitor csv file and identifies SVO readings.")]
		[Example("-csvhandle file.csv")]
		protected void CsvHandle(string CsvPath)
		{
			var ListLastPair = new HashSet<string>();
			var FPS4List = new Dictionary<string, FPS4>();
			var Parse = new Regex(@"ReadFile"",""([^""]+)"",""SUCCESS"",""Offset: ([\d\.]+), Length: ([\d\.]+)", RegexOptions.Compiled);
			foreach (var Line in File.ReadAllLines(CsvPath))
			{
				if (Line.Contains(@"ReadFile"))
				{
					var Match = Parse.Match(Line);
					if (Match.Groups[1].Value == "") {
						//Console.WriteLine(Line);
						Console.Error.WriteLine("Invalid Line! : {0}", Line);
					} else {
						var FilePath = Match.Groups[1].Value;
						var Offset = int.Parse(Match.Groups[2].Value.Replace(".", "").Trim());
						var Length = int.Parse(Match.Groups[3].Value.Replace(".", "").Trim());
						if (!FPS4List.ContainsKey(FilePath)) {
							try {
								FPS4List[FilePath] = new FPS4(File.OpenRead(FilePath));
							} catch {
							}
						}
						if (FPS4List.ContainsKey(FilePath)) {
							var FPS4 = FPS4List[FilePath];
							foreach (var Item in FPS4) {
								if (Offset >= Item.EntryStruct.Offset && Offset < Item.EntryStruct.Offset + Item.EntryStruct.LengthReal) {
									var CurrentPair = FilePath + Item.Name;
									if (!ListLastPair.Contains(CurrentPair))
									{
										ListLastPair.Add(CurrentPair);
										Console.WriteLine("{0}: {1}, {2} : {3}", FilePath, Offset, Length, Item.Name);
									}
								}
							}
						}
					}

					//var Parts = Line.Split(',');
					//Console.WriteLine(Parts[5]);
					//Console.WriteLine(Parts[7]);
					//Console.WriteLine(Parts[8]);
					//Console.WriteLine(String.Join("||", Parts));
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SvoPath"></param>
		/// <param name="OutputDirectory"></param>
		[Command("-e2", "--extract2")]
		[Description("Extracts a FPS4/TO8SCEL file with info and data in distinct files to a folder")]
		[Example("-e2 syspack.dat syspack.dav <folder>")]
		protected void ExtractSvo(string DatPath, string DavPath, string OutputDirectory = null)
		{
			//new FPS4(File.OpenRead(DatPath), File.OpenRead(DavPath));
			//ExtractSvo(DatPath, DavPath, OutputDirectory);
			ExtractSvo2(DatPath, DavPath, OutputDirectory);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SvoPath"></param>
		/// <param name="OutputDirectory"></param>
		[Command("-e", "--extract")]
		[Description("Extracts a FPS4/TO8SCEL file to a folder")]
		[Example("-e file.svo <folder>")]
		protected void ExtractSvo(string SvoPath, string OutputDirectory = null)
		{
			var BasePath = Path.GetDirectoryName(SvoPath);
			if (BasePath == "") BasePath = ".";

			if (SvoPath.Contains("*"))
			{
				var Pattern = Path.GetFileName(SvoPath);
				foreach (var File in Directory.EnumerateFiles(BasePath, Pattern))
				{
					ExtractSvo(File, null);
				}
				return;
			}

			ExtractSvo2(SvoPath, null, OutputDirectory);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SvoPath"></param>
		/// <param name="InputDirectory"></param>
		[Command("-c", "--create")]
		[Description("Create a SVO from a folder")]
		[Example("-c file.svo folder")]
		protected void CreateSvo(string SvoPath, string InputDirectory)
		{
			throw(new NotImplementedException());
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
