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
		/// <param name="SvoPath"></param>
		/// <param name="OutputDirectory"></param>
		[Command("-e", "--extract")]
		[Description("Extracts a SVO file to a folder")]
		[Example("-e file.svo <folder>")]
		protected void ExtractSvo(string SvoPath, string OutputDirectory = null)
		{
			using (var Stream = File.OpenRead(SvoPath))
			{
				var FPS4 = new FPS4(Stream);

				if (OutputDirectory == null)
				{
					OutputDirectory = SvoPath + ".d";
				}

				try { Directory.CreateDirectory(OutputDirectory); } catch { }

				foreach (var Entry in FPS4)
				{
					var OutputFileName = OutputDirectory + "/" + Entry.Name;
					Console.Write("{0}...", OutputFileName);
					if (!File.Exists(OutputFileName) || Overwrite)
					{
						using (var OutputStream = Entry.Open())
						{
							OutputStream.CopyToFile(OutputFileName);
						}
						Console.WriteLine("Ok");
					}
					else
					{
						Console.WriteLine("Exists");
					}
				}
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
