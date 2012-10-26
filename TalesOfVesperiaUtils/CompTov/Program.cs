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
				var Start = DateTime.UtcNow;
				using (var CompressedStream = File.OpenRead(CompressedFile))
				using (var UncompressedStream = File.Open(UncompressedFile, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					var Compression = TalesCompression.CreateFromStart(CompressedStream.SliceWithLength().ReadBytes(0x10));
					Compression.DecodeFile(CompressedStream, UncompressedStream);
				}
				var End = DateTime.UtcNow;
				Console.WriteLine("Ok({0})", (End - Start).TotalSeconds);
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

            //var Uncompressed = File.ReadAllBytes(UncompressedFile);
			//TalesCompression.CreateFromVersion(1).EncodeFile();
            //var Compressed = complib.Encode(3, Uncompressed);
            //File.WriteAllBytes(CompressedFile, Compressed);

			var Start = DateTime.UtcNow;
            using (var In = File.OpenRead(UncompressedFile))
            using (var Out = File.Open(CompressedFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Compression.EncodeFile(In, Out);
            }
			var End = DateTime.UtcNow;

			Console.WriteLine("Ok({0})", (End - Start).TotalSeconds);
            //Console.WriteLine(Version);
        }

		[Command("-t", "--test")]
		[Description("Test compression")]
		[Example("-t 1 file.bin")]
		protected void CompressFile(int Version, string UncompressedFile)
		{
			try
			{
				var Start = DateTime.UtcNow;
				var Compression = TalesCompression.CreateFromVersion(Version);
				var Uncompressed = File.ReadAllBytes(UncompressedFile);
				var Compressed = Compression.EncodeBytes(Uncompressed);
				var Uncompressed2 = Compression.DecodeBytes(Compressed);
				if (Uncompressed.Length != Uncompressed2.Length)
				{
					Console.Error.WriteLine("Length mismatch {0} != {1}", Uncompressed.Length, Uncompressed2.Length);
				}

				var Offsets = new List<int>();
				for (int n = 0; n < Uncompressed.Length; n++)
				{
					if (Uncompressed[n] != Uncompressed2[n]) Offsets.Add(n);
				}

				if (Offsets.Count > 0)
				{
					foreach (var Offset in Offsets.Take(100))
					{
						Console.Error.WriteLine(" {0:X8}: {1:X2} != {2:X2}", Offset, Uncompressed[Offset], Uncompressed2[Offset]);
					}
					if (Offsets.Count > 100) Console.WriteLine(" ...");
					throw (new Exception(String.Format("Mismatches: [{0}], {1}", String.Join(",", Offsets.Take(100)), Offsets.Count)));
				}

				var End = DateTime.UtcNow;
				Console.WriteLine("Ok({0})", (End - Start).TotalSeconds);
			}
			catch (Exception Exception)
			{
				Console.WriteLine("Error({0})", Exception);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			//args = new [] {"-c", "1", @"c:\temp\tsc.exe", @"c:\temp\tsc.exe.c1"};
			new Program().Run(args);
		}
	}
}
