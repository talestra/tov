﻿using CSharpUtils;
using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Compression;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaUtils
{
	public class PatchInplace : IDisposable
	{
		FileSystem FileSystem;

		public PatchInplace(FileSystem FileSystem)
		{
			this.FileSystem = FileSystem;
		}

		public void DecompressAndRecompressIfRequired(Stream Stream, Action<Stream> Action, int RecompressVersion = -1)
		{
			if (TalesCompression.IsValid(Stream))
			{
				using (var UncompressedStream = new DecompressRecompressStream(Stream, RecompressVersion))
				{
					Action(UncompressedStream);
				}
			}
			else
			{
				Action(Stream);
			}
		}

		public void SetFileSystem(FileSystem FileSystem, Action Action)
		{
			LanguageUtils.LocalSet(ref this.FileSystem, FileSystem, () =>
			{
				Action();
			});
		}

		public void Access(string Path, Action Action, int RecompressVersion = -1)
		{
			if (Path.Contains('/'))
			{
				var Parts = Path.Split(new[] { '/' }, 2);
				Access(Parts[0], () =>
				{
					Access(Parts[1], Action);
				});
				return;
			}

			GetFile(Path, (CompressedStream) =>
			{
				DecompressAndRecompressIfRequired(CompressedStream, (UncompressedStream) =>
				{
					var Magic = UncompressedStream.Slice().ReadBytesUpTo(0x100);
					if (FPS4.IsValid(Magic))
					{
						LanguageUtils.LocalSet(ref this.FileSystem, new FPS4FileSystem(UncompressedStream), Action);
					}
					else if (TO8SCEL.IsValid(Magic))
					{
						LanguageUtils.LocalSet(ref this.FileSystem, new TO8SCELFileSystem(UncompressedStream), Action);
					}
					else
					{
						throw (new InvalidOperationException(String.Format("Can't access '{0}'", Path)));
					}
				}, RecompressVersion);
			}, RecompressVersion);
		}

		public void GetFile(string File, Action<Stream> Action, int RecompressVersion = -1)
		{
			using (var CompressedStream = FileSystem.OpenFileRW(File))
			{
				//Console.WriteLine("{0}", CompressedStream.Length);
				DecompressAndRecompressIfRequired(CompressedStream, (UncompressedStream) =>
				{
					Action(UncompressedStream);
					//if (UncompressedStream.Length <= 4 * 1024 * 1024)
					//{
					//	System.IO.File.WriteAllBytes(@"J:\isos\360\vesperia_ori\_temp\TEMP.bin", UncompressedStream.ReadAll());
					//}
				}, RecompressVersion);
			}
		}

		public void GetFile2(string File1, string File2, Action<Stream, Stream> Action)
		{
			using (var Stream1 = FileSystem.OpenFileRW(File1))
			using (var Stream2 = FileSystem.OpenFileRW(File2))
			{
				Action(Stream1, Stream2);
			}
		}

		public void Dispose()
		{
			FileSystem.Dispose();
		}
	}
}
