using CSharpUtils;
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

		[DebuggerHidden]
		public void DecompressAndRecompressIfRequired(Stream Stream, Action<Stream> Action)
		{
			if (TalesCompression.IsValid(Stream))
			{
				using (var UncompressedStream = new DecompressRecompressStream(Stream))
				{
					Action(UncompressedStream);
				}
			}
			else
			{
				Action(Stream);
			}
		}

		[DebuggerHidden]
		public void SetFileSystem(FileSystem FileSystem, Action Action)
		{
			LanguageUtils.LocalSet(ref this.FileSystem, FileSystem, () =>
			{
				Action();
			});
		}

		[DebuggerHidden]
		public void Access(string Path, Action Action)
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
					else
					{
						throw(new InvalidOperationException(String.Format("Can't access '{0}'", Path)));
					}
				});
			});
		}

		[DebuggerHidden]
		public void GetFile(string File, Action<Stream> Action)
		{
			using (var CompressedStream = FileSystem.OpenFileRW(File))
			{
				DecompressAndRecompressIfRequired(CompressedStream, (UncompressedStream) =>
				{
					Action(UncompressedStream);
				});
			}
		}

		[DebuggerHidden]
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
