using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils.Getopt;
using TalesOfVesperiaUtils.Formats.Packages;

namespace Svo
{
	class Program
	{
		static void ExtractSvo(string FilePath, string OutputDirectory = null)
		{
			using (var Stream = File.OpenRead(FilePath))
			{
				var FPS4 = new FPS4(Stream);

				if (OutputDirectory == null)
				{
					OutputDirectory = FilePath + ".d";
				}

				try
				{
					Directory.CreateDirectory(OutputDirectory);
				}
				catch
				{
				}

				foreach (var Entry in FPS4)
				{
					var OutputFileName = OutputDirectory + "/" + Entry.Name;
					Console.Write("{0}...", OutputFileName);
					using (var OutputStream = Entry.Open())
					{
						OutputStream.CopyToFile(OutputFileName);
					}
					Console.WriteLine("Ok");
				}
			}
		}

		static void ShowHelp()
		{
			Console.WriteLine("SVO - Tales of Vesperia Utilities - 2012");
			Console.WriteLine("");
			Console.WriteLine("Switches:");
			Console.WriteLine("   -e - Extract SVO");
			Console.WriteLine("   -c - Create SVO with folder");
			Console.WriteLine("");
			Console.WriteLine("Examples:");
			Console.WriteLine("");
			Console.WriteLine("   SVO.exe -e file.svo <folder>");
			Console.WriteLine("   SVO.exe -c file.svo folder");
		}

		static void Main(string[] args)
		{
			var Getopt = new Getopt(args);
			
			Getopt.AddRule(new[] { "-e", "--extract" }, (string FileName) =>
			{
				string OutputDirectoryName = FileName + ".d";
				if (Getopt.HasMore)
				{
					OutputDirectoryName = Getopt.DequeueNext();
				}
				ExtractSvo(FileName, OutputDirectoryName);
			});

			Getopt.AddRule(new[] { "-?", "-h", "--help", "/?", "/h" }, () =>
			{
				ShowHelp();
			});

			Getopt.AddDefaultRule(() =>
			{
				ShowHelp();
			});

			try
			{
				Getopt.Process();
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception.Message);
				Environment.Exit(-1);
			}
		}
	}
}
