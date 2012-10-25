using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils.Getopt;
using TalesOfVesperiaUtils.Compression;
using TalesOfVesperiaUtils.Compression.C;

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
        /// <param name="CompressedFile"></param>
        /// <param name="UncompressedFile"></param>
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
            if (CompressedFile == UncompressedFile) throw(new Exception("Compressed and uncompressed files can't be the same"));

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

        [Command("-c", "--compress")]
        [Description("Compresses a file")]
        [Example("-c 3 file.bin.u <file.bin>")]
        protected void CompressFile(int Version, string UncompressedFile, string CompressedFile = null)
        {
            if (CompressedFile == null) CompressedFile = String.Format("{0}.c", UncompressedFile);
            var Compression = TalesCompression.CreateFromVersion(Version);

            if (CompressedFile == UncompressedFile) throw (new Exception("Compressed and uncompressed files can't be the same"));

            Console.Write("{0} -> {1}...", UncompressedFile, CompressedFile);

            var Uncompressed = File.ReadAllBytes(UncompressedFile);
            var Compressed = complib.Encode(3, Uncompressed);
            File.WriteAllBytes(CompressedFile, Compressed);

            /*
            using (var In = File.OpenRead(UncompressedFile))
            using (var Out = File.Open(CompressedFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Compression.EncodeFile(In, Out);
            }
            */
            Console.WriteLine("Ok");
            //Console.WriteLine(Version);
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
