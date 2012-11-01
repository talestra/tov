using CSharpUtils.Getopt;
using CSharpUtils.VirtualFileSystem;
using CSharpUtils.VirtualFileSystem.Local;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace Iso360Extract
{
	class Program : GetoptCommandLineProgram
	{
		[Command("-x", "--extract")]
		[Description("Extracts ISO into a Folder")]
		[Example("-x 360.iso folder/to/extracted/files")]
		public void ExtractIsoToFolder(string SourceIsoPath, string DestExtractPath)
		{
			if (!Directory.Exists(DestExtractPath)) Directory.CreateDirectory(DestExtractPath);
			var Iso = new Dvd9Xbox360FileSystem(File.OpenRead(SourceIsoPath));
			var Out = new LocalFileSystem(DestExtractPath);
			FileSystem.CopyTree(Iso, "/", Out, "/", null, (Entry, Current, End) =>
			{
				if (Current == 0) Console.WriteLine();
				Console.Write("{0} {1:0.00}% ({2})\r", Entry.FullName, ((double)Current / (double)End) * 100, End);
			});
		}

		static void Main(string[] args)
		{
			new Program().Run(args);
		}
	}
}
