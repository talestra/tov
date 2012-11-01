using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Formats.Packages;

namespace TalesOfVesperiaUtils.VirtualFileSystem
{
	public class FPS4FileSystem : FileSystem, IDisposable
	{
		bool Disposed;
		FPS4 FPS4;

		public FPS4FileSystem(Stream Stream)
		{
			this.FPS4 = new FPS4(Stream);
		}

		public FPS4FileSystem(FPS4 FPS4)
		{
			this.FPS4 = FPS4;
		}

		~FPS4FileSystem()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (!Disposed)
			{
				Disposed = true;
				FPS4.Dispose();
			}
		}

		protected override FileSystemFileStream ImplOpenFile(string FileName, System.IO.FileMode FileMode)
		{
			return new FileSystemFileStreamStream(this, FPS4[FileName].Open());
		}

		protected override void ImplSetFileTime(string Path, FileSystemEntry.FileTime FileTime)
		{
			throw new NotImplementedException();
		}

		private FileSystemEntry ConvertEntry(FPS4.Entry Entry)
		{
			return new FileSystemEntry(this, Entry.Name)
			{
				Size = Entry.EntryStruct.LengthReal,
				Type = FileSystemEntry.EntryType.File,
			};
		}


		protected override FileSystemEntry ImplGetFileInfo(string Path)
		{
			return ConvertEntry(FPS4[Path]);
		}

		protected override void ImplDeleteFile(string Path)
		{
			throw new NotImplementedException();
		}

		protected override void ImplDeleteDirectory(string Path)
		{
			throw new NotImplementedException();
		}

		protected override void ImplCreateDirectory(string Path, int Mode = 0777)
		{
			throw new NotImplementedException();
		}

		protected override void ImplMoveFile(string ExistingFileName, string NewFileName, bool ReplaceExisiting)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<FileSystemEntry> ImplFindFiles(string Path)
		{
			foreach (var Entry in FPS4)
			{
				yield return ConvertEntry(Entry);
			}
		}

		protected override void ImplCreateSymLink(string Pointer, string Pointee)
		{
			throw new NotImplementedException();
		}
	}
}
