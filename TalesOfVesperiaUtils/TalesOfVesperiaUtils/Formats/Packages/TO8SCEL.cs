﻿using CSharpUtils;
using CSharpUtils.Endian;
using CSharpUtils.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TalesOfVesperiaUtils.Compression;

namespace TalesOfVesperiaUtils.Formats.Packages
{
	public class TO8SCEL : BasePackage, IDisposable, IEnumerable<TO8SCEL.Entry>
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		public struct HeaderStruct
		{
			/// <summary>
			/// Magic of the file its contents should be always "TO8SCEL\0" for a valid file.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
			public string Magic;

			/// <summary>
			/// 
			/// </summary>
			public uint_be TotalSize;

			/// <summary>
			/// 
			/// </summary>
			public uint_be ListStart;

			/// <summary>
			/// 
			/// </summary>
			public uint_be ListCount;

			/// <summary>
			/// 
			/// </summary>
			public uint_be DataStart;

			/// <summary>
			/// 
			/// </summary>
			public uint_le DataEnd;

			/// <summary>
			/// 
			/// </summary>
			public uint_be Padding;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		public struct EntryStruct
		{
			public uint_be Offset;

			public uint_be LengthCompressed;

			public uint_be LengthUncompressed;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
			private uint[] Padding;

			public override string ToString()
			{
				return String.Format(
					"EntryStruct(Offset={0}, LengthCompressed={1}, LengthUncompressed={2})",
					Offset.NativeValue.ToString("x8"),
					LengthCompressed.NativeValue.ToString("x8"),
					LengthUncompressed.NativeValue.ToString("x8")
				);
			}
		}

		public class Entry
		{
			public int Index;
			public TO8SCEL TO8SCEL;
			public EntryStruct EntryStruct;

			public bool Valid
			{
				get
				{
					return EntryStruct.Offset > 0;
				}
			}

			public bool IsCompressed
			{
				get
				{
					return EntryStruct.LengthCompressed < EntryStruct.LengthUncompressed;
				}
			}

			private Stream _CompressedStream;
			public Stream CompressedStream
			{
				get
				{
					if (_CompressedStream == null)
					{
						_CompressedStream = SliceStream.CreateWithLength(TO8SCEL.Stream, EntryStruct.Offset + TO8SCEL.Header.DataStart, EntryStruct.LengthCompressed);
					}
					return _CompressedStream;
				}
				set
				{
					_CompressedStream = value;
					EntryStruct.LengthCompressed = (value != null) ? (uint)value.Length : (uint)0;
				}
			}

			private Stream _UncompressedStream;
			public Stream UncompressedStream
			{
				get
				{
					if (_UncompressedStream == null)
					{
						if (IsCompressed)
						{
							_UncompressedStream = TalesCompression.DecompressStream(CompressedStream);
						}
						else
						{
							_UncompressedStream = CompressedStream;
						}
					}
					return _UncompressedStream;
				}
				set
				{
					_UncompressedStream = value;
					EntryStruct.LengthUncompressed = (value != null) ? (uint)value.Length : (uint)0;
				}
			}

			public Entry(TO8SCEL TO8SCEL, EntryStruct EntryStruct, int Index)
			{
				this.TO8SCEL = TO8SCEL;
				this.EntryStruct = EntryStruct;
				this.Index = Index;
			}
		}

		HeaderStruct Header;
		Stream Stream;
		public List<Entry> Entries = new List<Entry>();

		public TO8SCEL()
		{
		}

		public TO8SCEL(Stream Stream)
		{
			Load(Stream);
		}

		~TO8SCEL()
		{
		}

		override public void Load(Stream Stream)
		{
			this.Stream = Stream;

			this.Header = this.Stream.ReadStruct<HeaderStruct>();
			if (this.Header.Magic != "TO8SCEL") throw (new Exception("Invalid Magic"));

			if (this.Header.ListCount > 10000) throw (new Exception("List too big"));

			this.Stream.Position = Header.ListStart;

			for (int n = 0; n < Header.ListCount; n++)
			{
				var Entry = new Entry(this, this.Stream.ReadStruct<EntryStruct>(), n);
				//Console.WriteLine(Entry.EntryStruct);
				Entries.Add(Entry);
			}
		}

		public override void SaveTo(Stream Stream, bool DoAlign = true)
		{
			var ListStart = Marshal.SizeOf(typeof(HeaderStruct));
			var DataStart = ListStart + Marshal.SizeOf(typeof(EntryStruct)) * Entries.Count;
			Stream.Position = DataStart;
			Stream.WriteByteRepeated(0x00, 0x10);
			foreach (var Entry in Entries)
			{
				Entry.EntryStruct.Offset = (uint)(Stream.Position - DataStart);
				Stream.WriteStream(Entry.CompressedStream);
				Stream.WriteZeroToAlign(0x10);
			}

			Stream.Position = 0;

			this.Header.Magic = "TO8SCEL\0";
			this.Header.TotalSize = (uint)Stream.Length;
			this.Header.ListCount = (uint)Entries.Count;
			this.Header.ListStart = (uint)ListStart;
			this.Header.DataStart = (uint)DataStart;
			this.Header.DataEnd = (uint)Stream.Length;

			Stream.WriteStruct(this.Header);

			foreach (var Entry in Entries)
			{
				Stream.WriteStruct(Entry.EntryStruct);
			}
		}

		public void Dispose()
		{
			if (Stream != null)
			{
				Stream.Dispose();
				Stream = null;
			}
		}

		public IEnumerator<TO8SCEL.Entry> GetEnumerator()
		{
			return Entries.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		static public bool IsValid(byte[] Data)
		{
			try
			{
				var Header = StructUtils.BytesToStruct<HeaderStruct>(Data);
				if (Header.Magic != "TO8SCEL") return false;

				return true;
			}
			catch
			{
			}
			return false;
		}

		public Entry CreateEntry(int Id, Stream CompressedStream, Stream UncompressedStream)
		{
            lock (this)
            {
                var EntryStruct = default(EntryStruct);
                var Entry = new Entry(this, EntryStruct, Id);
                Entry.CompressedStream = CompressedStream;
                Entry.UncompressedStream = UncompressedStream;
                while (Entries.Count <= Id) Entries.Add(new Entry(this, default(EntryStruct), Entries.Count));
                return Entries[Id] = Entry;
            }
		}
	}
}
