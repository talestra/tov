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
		/// <param name="OutputFileName"></param>
		/// <param name="InputStreamGet"></param>
		private void _ExtractFile(string OutputFileName, Func<Stream> InputStreamGet)
		{
			Console.Write("{0}...", OutputFileName);
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

			if (OutputDirectory == null)
			{
				OutputDirectory = SvoPath + ".d";
			}

			try { Directory.CreateDirectory(OutputDirectory); }
			catch { }

			Console.WriteLine("Loading {0}...", SvoPath);
			try
			{
				using (var Stream = File.OpenRead(SvoPath))
				{
					if (Stream.SliceWithLength().ReadString(7) == "TO8SCEL")
					{
						var TO8SCEL = new TO8SCEL(Stream);

						foreach (var Entry in TO8SCEL)
						{
							_ExtractFile(OutputDirectory + "/" + Entry.Index, () => Entry.UncompressedStream);
						}
					}
					else
					{
						var FPS4 = new FPS4(Stream);

						foreach (var Entry in FPS4)
						{
							_ExtractFile(OutputDirectory + "/" + Entry.Name, () => Entry.Open());
						}
					}
				}
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine("{0}", Exception);
			}
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
