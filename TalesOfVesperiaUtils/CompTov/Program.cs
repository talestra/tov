using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils.Getopt;
using TalesOfVesperiaUtils.Compression;

namespace CompTov
{
	/// <summary>
	/// 
	/// </summary>
	class Program : GetoptCommandLineProgram
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="SvoPath"></param>
		/// <param name="OutputDirectory"></param>
		[Command("-d", "--decompress")]
		[Description("Decompresses a file")]
		[Example("-d file.bin <file.bin.u>")]
		[Example("-d *.dat uncompressed_folder")]
		protected void DecompressFile(string CompressedFile, string UncompressedFile = null)
		{
			if (CompressedFile.Contains('*'))
			{
				var BaseDirectory = Path.GetDirectoryName(CompressedFile);
				if (BaseDirectory == "") BaseDirectory = ".";

				if (UncompressedFile != null)
				{
					if (!Directory.Exists(UncompressedFile)) Directory.CreateDirectory(UncompressedFile);
				}

				foreach (var CompressedFile2 in Directory.EnumerateFiles(BaseDirectory, Path.GetFileName(CompressedFile)))
				{
					var BaseName = Path.GetFileName(CompressedFile2);

					if (UncompressedFile == null)
					{
						DecompressFile(CompressedFile2, null);
					}
					else
					{
						var Out = UncompressedFile + Path.DirectorySeparatorChar + BaseName + ".u";
						DecompressFile(CompressedFile2, Out);
					}
				}

				return;
			}

			if (UncompressedFile == null) UncompressedFile = CompressedFile + ".u";

			Console.Write("{0} -> {1}...", CompressedFile, UncompressedFile);

			try
			{
				using (var CompressedStream = File.OpenRead(CompressedFile))
				using (var UncompressedStream = File.Open(UncompressedFile, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					var Compression = TalesCompression.CreateFromStart(CompressedStream.SliceWithLength().ReadBytes(0x10));
					Compression.DecodeFile(CompressedStream, UncompressedStream);
				}
				Console.WriteLine("Ok");
			}
			catch (Exception Exception)
			{
				Console.WriteLine("Error({0})", Exception.Message);
			}
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
