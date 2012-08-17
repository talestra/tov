using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
			var CurrentAssembly = Assembly.GetEntryAssembly();
			var VersionInfo = FileVersionInfo.GetVersionInfo(CurrentAssembly.Location);

			Console.WriteLine(
				"{0} - {1} - {2} - {3} - soywiz - {4}",
				VersionInfo.FileDescription,
				String.Join(".", VersionInfo.FileVersion.Split('.').Take(2)),
				VersionInfo.Comments,
				VersionInfo.CompanyName,
				VersionInfo.LegalCopyright
			);
			Console.WriteLine("");
			Console.WriteLine("Switches:");
			Console.WriteLine("   -e - Extract SVO");
			Console.WriteLine("   -c - Create SVO with folder");
			Console.WriteLine("");
			Console.WriteLine("Examples:");
			Console.WriteLine("");
			Console.WriteLine("   {0}.exe -e file.svo <folder>", CurrentAssembly.GetName().Name);
			Console.WriteLine("   {0}.exe -c file.svo folder", CurrentAssembly.GetName().Name);
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

			if (Debugger.IsAttached)
			{
				Console.ReadKey();
			}
		}
	}
}
