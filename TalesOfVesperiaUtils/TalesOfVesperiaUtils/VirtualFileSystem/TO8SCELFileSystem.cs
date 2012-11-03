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
	public class TO8SCELFileSystem : FileSystem
	{
		TO8SCEL TO8SCEL;

		public TO8SCELFileSystem(Stream Stream)
		{
			this.TO8SCEL = new TO8SCEL(Stream);
		}

		public TO8SCELFileSystem(TO8SCEL TO8SCEL)
		{
			this.TO8SCEL = TO8SCEL;
		}

		private TO8SCEL.Entry ScelEntryFromName(string FileName)
		{
			try
			{
				return this.TO8SCEL.Entries[int.Parse(FileName)];
			}
			catch
			{
				throw(new FileNotFoundException(String.Format("Can't find '{0}'", FileName)));
			}
		}

		private FileSystemEntry ScelEntryToFileEntry(TO8SCEL.Entry Entry)
		{
			return new FileSystemEntry(this, String.Format("{0}", Entry.Index))
			{
				Type = FileSystemEntry.EntryType.File,
				Size = Entry.EntryStruct.LengthCompressed,
			};
		}

		protected override FileSystemFileStream ImplOpenFile(string FileName, System.IO.FileMode FileMode)
		{
			return new FileSystemFileStreamStream(this, ScelEntryFromName(FileName).CompressedStream);
		}

		protected override void ImplSetFileTime(string Path, FileSystemEntry.FileTime FileTime)
		{
			throw new NotImplementedException();
		}

		protected override FileSystemEntry ImplGetFileInfo(string Path)
		{
			return ScelEntryToFileEntry(ScelEntryFromName(Path));
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
			foreach (var Entry in TO8SCEL.Entries)
			{
				yield return ScelEntryToFileEntry(Entry);
			}
		}

		protected override void ImplCreateSymLink(string Pointer, string Pointee)
		{
			throw new NotImplementedException();
		}
	}
}
