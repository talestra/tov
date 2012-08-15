using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils;
using System.IO;
using TalesOfVesperiaUtils.Compression;
using CSharpUtils.Endian;

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
			public uint_be DataEnd;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		public struct EntryStruct
		{
			public uint_be Offset;

			public uint_be LengthCompressed;

			public uint_be LengthUncompressed;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
			public uint[] Padding;

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

		public class Entry {
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

			Stream _CompressedStream;

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
			}

			Stream _UncompressedStream;

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
			}

			public Entry(TO8SCEL TO8SCEL, EntryStruct EntryStruct)
			{
				this.TO8SCEL = TO8SCEL;
				this.EntryStruct = EntryStruct;
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

			Header = Stream.ReadStruct<HeaderStruct>();
			if (Header.Magic != "TO8SCEL") throw(new Exception("Invalid Magic"));

			if (Header.ListCount > 10000) throw(new Exception("List too big"));

			Stream.Position = Header.ListStart;

			for (int n = 0; n < Header.ListCount; n++)
			{
				var Entry = new Entry(this, Stream.ReadStruct<EntryStruct>());
				//Console.WriteLine(Entry.EntryStruct);
				Entries.Add(Entry);
			}
		}

		public override void Save(Stream Stream)
		{
			throw new NotImplementedException();
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
	}
}
