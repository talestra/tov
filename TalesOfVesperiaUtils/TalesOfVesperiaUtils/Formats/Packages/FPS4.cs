using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils;
using TalesOfVesperiaUtils.Compression;
using CSharpUtils.Endian;

namespace TalesOfVesperiaUtils.Formats.Packages
{
	public class FPS4 : BasePackage, IDisposable, IEnumerable<FPS4.Entry>
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 0x001C)]
		public struct HeaderStruct
		{
			/// <summary>
			/// Magic of the file its contents should be always "FPS4" for a valid file.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			//[FieldOffset(0x0000)]
			public byte[] Magic;

			/// <summary>
			/// Number of entries for this package.
			/// </summary>
			//[FieldOffset(0x0004)]
			public uint_be ListCount;

			/// <summary>
			/// Start of entry definitions.
			/// Note: Offset relative to the start of the file.
			/// </summary>
			//[FieldOffset(0x0008)]
			public uint_be ListStart;

			/// <summary>
			/// Ends of entry definitions.
			/// Note: Offset relative to the start of the file.
			/// </summary>
			//[FieldOffset(0x000C)]
			public uint_be ListEnd;

			/// <summary>
			/// Size of each entry.
			/// </summary>
			//[FieldOffset(0x0010)]
			public ushort_be EntrySizeof;

			/// <summary>
			/// Format of each entry??
			/// Probably a BitField? But not sure bit meanings.
			/// </summary>
			//[FieldOffset(0x0012)]
			public ushort_be EntryFormat;

			/// <summary>
			/// Format 2 of each entry.
			/// </summary>
			//[FieldOffset(0x0014)]
			public uint_be Unk;

			/// <summary>
			/// Offset to a string containing the original file path.
			/// Note: Offset relative to the start of the file.
			/// </summary>
			//[FieldOffset(0x0018)]
			public uint_be FilePos;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		public struct EntryStruct
		{
			/// <summary>
			/// 
			/// </summary>
			public uint_be Offset;

			/// <summary>
			/// 
			/// </summary>
			public uint_be LengthSectorAligned;

			/// <summary>
			/// 
			/// </summary>
			public uint_be LengthReal;

			//[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			//public string Name;
		}

		public class Entry : IDisposable
		{
			public FPS4 FPS4;
			public EntryStruct EntryStruct;
			public uint Index;
			public String Name;
			public uint MappedFileIndex;
			protected Entry _LinkedTo;
			protected Stream _Stream;

			internal Entry(FPS4 FPS4, EntryStruct EntryStruct, String Name)
			{
				this.FPS4 = FPS4;
				this.EntryStruct = EntryStruct;
				this.Name = Name;
				this._Stream = SliceStream.CreateWithLength(FPS4.Stream, EntryStruct.Offset, EntryStruct.LengthReal);
			}

			internal Entry(FPS4 FPS4, Stream Stream, String Name)
			{
				this.FPS4 = FPS4;
				this.Name = Name;
				this._Stream = Stream;
			}

			~Entry()
			{
				Dispose();
			}

			public void Dispose()
			{
				if (_Stream != null)
				{
					_Stream.Dispose();
					_Stream = null;
				}
			}

			/*
			public Stream Stream
			{
				get
				{
					return (this._Stream != null) ? Open() : null;
				}
				set
				{
					this._Stream = value;
					this._LinkedTo = null;
				}
			}
			*/

			public void SetStream(Stream Stream)
			{
				this._Stream = Stream;
				this._LinkedTo = null;
			}

			public long Length
			{
				get
				{
					if (this._Stream == null) return 0;
					return this._Stream.Length;
				}
			}

			public Stream Open()
			{
#if false
				if (MappedFileIndex > 0)
				{
					return SliceStream.CreateWithLength(new ZeroStream(this.EntryStruct.LengthReal), 0, this.EntryStruct.LengthReal);
				}
#endif
				return SliceStream.CreateWithLength(this._Stream, 0, this._Stream.Length);
			}

			public void ReplaceWithStream(Stream SourceStream)
			{
				using (var DestinationStream = Open())
				{
					SourceStream.CopyToFast(DestinationStream);
				}
			}

			public void ReplaceWithFile(string SourceFileName)
			{
				Console.Write("Replacing '{0}' with '{1}'...", this.Name, SourceFileName);
				using (var SourceStream = File.OpenRead(SourceFileName))
				{
					ReplaceWithStream(SourceStream);
				}
				Console.WriteLine("Ok");
			}

			public bool IsLinked
			{
				get
				{
					return _LinkedTo != null;
				}
			}

			public EntryStruct LinkedEntryStruct
			{
				get
				{
					return (LinkedTo != null) ? LinkedTo.EntryStruct : EntryStruct;
				}
			}

			public Entry LinkedTo
			{
				get
				{
					return this._LinkedTo;
				}
				set
				{
					if (value.FPS4 != this.FPS4) throw (new Exception("Entries from different FPS4"));
					this._LinkedTo = (value._LinkedTo != null) ? value.LinkedTo : value;
					this._Stream = null;
				}
			}

			public override string ToString()
			{
				if (IsLinked)
				{
					return String.Format(
						"FPS4.Entry(Name='{0}', Linked={1})",
						Name,
						LinkedTo
					);
				}
				else
				{
					return String.Format(
						"FPS4.Entry(Name='{0}', Offset=0x{1}, LengthCompressed={2}, LengthUncompressed={3})",
						Name,
						EntryStruct.Offset.NativeValue.ToString("X8"),
						EntryStruct.LengthSectorAligned,
						EntryStruct.LengthReal
					);
				}
			}
		}

		public Entry CreateEntry(String Name, Stream Stream)
		{
			return Entries[Name] = new Entry(this, Stream, Name);
		}

		public Entry CreateEntry(String Name, Entry LinkedTo)
		{
			var Entry = new Entry(this, null, Name);
			Entry.LinkedTo = LinkedTo;
			return Entries[Name] = Entry;
		}

		public String OriginalFilePath;
		public Stream Stream;
		public Dictionary<String, Entry> Entries = new Dictionary<String, Entry>();

		public List<Entry> EntryList
		{
			get
			{
				return new List<Entry>(Entries.Values);
			}
		}

		public FPS4()
		{
		}

		public FPS4(Stream Stream)
		{
			Load(Stream);
		}

		~FPS4()
		{
			Dispose();
		}

		HeaderStruct Header;

		override public void Load(Stream Stream)
		{
			this.Stream = Stream;

			Header = Stream.ReadStruct<HeaderStruct>();
			if (Encoding.ASCII.GetString(Header.Magic) != "FPS4") throw (new Exception("Invalid Magic"));

			if (Header.FilePos != 0)
			{
				Stream.Position = Header.FilePos;
				OriginalFilePath = Stream.ReadStringz(0x100, Encoding.GetEncoding("Shift-JIS"));
			}
			else
			{
				OriginalFilePath = "";
			}

			Stream.Position = Header.ListStart;
			//Console.WriteLine("{0:X8}", Stream.Position);

			if (Header.ListCount > 10000) throw (new Exception("List too big (" + Header.ListCount + ")"));

			for (uint n = 0; n < Header.ListCount; n++)
			{
				var EntryStream = Stream.ReadStream(Header.EntrySizeof);
				var EntryStruct = default(EntryStruct);
				var ExtraEntrySizeof = Header.EntrySizeof - 0x0C;
				var IndexName = String.Format("{0}", n);
				var Name = "";
				uint MappedFileIndex = 0;

				switch ((int)Header.EntryFormat)
				{
					case 0x8D:
						EntryStruct.Offset = EntryStream.ReadStruct<uint_be>();
						EntryStruct.LengthReal = EntryStruct.LengthSectorAligned = EntryStream.ReadStruct<uint_be>();
						Name = EntryStream.ReadStringz(0x20);
						MappedFileIndex = EntryStream.ReadStruct<uint_be>();
						break;
					default:
						EntryStruct = EntryStream.ReadStruct<EntryStruct>();

						switch (ExtraEntrySizeof)
						{
							case 0:
								{
								}
								break;
							case 4:
								{
									var Offset = EntryStream.ReadStruct<uint>();
									// Pointer to string name.
									if (Offset != 0)
									{
										throw (new NotImplementedException());
									}
								}
								break;
							default:
								{
									IndexName = Name = EntryStream.ReadStringz(ExtraEntrySizeof);
								}
								break;
						}
						break;
				}

				//Console.WriteLine("Name: {0}", Name);

				if (n == Header.ListCount - 1)
				{
					// Ignore last element with an empty name.
					if (Name.Length == 0)
					{
						continue;
					}
				}

				if (Name.Length == 0)
				{
					Name = IndexName;
				}

				var Entry = new Entry(this, EntryStruct, Name);
				Entry.MappedFileIndex = MappedFileIndex;
				Entry.Index = n;
				Entries.Add(IndexName, Entry);
			}
		}

		public override void Save(Stream Stream)
		{
			var EntryListWithLinks = EntryList.Union(new Entry[] { new Entry(FPS4: this, Stream: new MemoryStream(), Name:"") });
			var EntryListWithoutLinks = EntryListWithLinks.Where(Entry => !Entry.IsLinked);

			var SectorPadding = 0x800;
			var BinaryWriter = new BinaryWriter(Stream);
			Header.Magic = Encoding.ASCII.GetBytes("FPS4");
			Header.ListStart = (uint)Marshal.SizeOf(typeof(HeaderStruct));
			var ExtraEntrySizeof = Header.EntrySizeof - 0x0C;
			var OriginalFilePathBytes = Encoding.GetEncoding("Shift-JIS").GetBytes(OriginalFilePath);
			var DataStartOffset = MathUtils.Align(Header.ListStart + Header.EntrySizeof * EntryListWithLinks.Count() + OriginalFilePathBytes.Length, SectorPadding);

			Stream.WriteStruct(Header);
			long CurrentOffset = DataStartOffset;

			// PASS1. First we calculate all the pointers for non-linked entries.
			foreach (var Entry in EntryListWithoutLinks)
			{
				var LengthReal = Entry.Length;
				var LengthSectorAligned = (uint)MathUtils.Align(LengthReal, SectorPadding);

				Entry.EntryStruct = new EntryStruct()
				{
					Offset = (uint)CurrentOffset,
					LengthSectorAligned = (uint)LengthSectorAligned,
					LengthReal = (uint)LengthReal,
				};

				CurrentOffset += LengthSectorAligned;
			}

			// PASS2. We then write all the LinkedEntryStructs.
			foreach (var Entry in EntryListWithLinks)
			{
				Stream.WriteStruct(Entry.LinkedEntryStruct);

				switch (ExtraEntrySizeof)
				{
					case 0:
						break;
					case 4:
						// Pointer to string name.
						/*
						if (Offset != 0)
						{
							throw (new NotImplementedException());
						}
						//var Offset = BinaryReader.ReadUInt32();
						*/
						if (Entry.Name == "")
						{
							BinaryWriter.Write((uint)0);
						}
						else
						{
							throw (new NotImplementedException());
						}
						break;
					default:
						Stream.WriteStringz(Entry.Name, ExtraEntrySizeof);
						break;
				}
			}

			Stream.WriteBytes(OriginalFilePathBytes);
			Stream.WriteZeroToOffset(DataStartOffset);
			foreach (var Entry in EntryListWithoutLinks)
			{
				Entry.Open().CopyToFast(Stream);
				Stream.WriteZeroToAlign(SectorPadding);
			}

			Stream.Flush();

			//throw new NotImplementedException();
		}

		public void Dispose()
		{
			if (Stream != null)
			{
				Stream.Dispose();
				Stream = null;
			}
		}

		public IEnumerator<FPS4.Entry> GetEnumerator()
		{
			return Entries.Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public Entry this [String index]
		{
			get
			{
				return Entries[index];
			}
			set
			{
				Entries[index] = value;
			}
		}

		public Entry this[int index]
		{
			get
			{
				return Entries[index.ToString()];
			}
			set
			{
				Entries[index.ToString()] = value;
			}
		}

		public int Length
		{
			get
			{
				return Entries.Count;
			}
		}
	}
}
