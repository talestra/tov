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
		[Flags]
		public enum EntryFlags : ushort
		{
			HasOffset = (1 << 0),
			HasLengthSectorAligned = (1 << 1),
			HasRealLength = (1 << 2),
			HasInlineName = (1 << 3),
			_Unknown1 = (1 << 4),
			HasStringExtension = (1 << 5),
			HasStringOffset = (1 << 6),
			HasAdditional_Uint = (1 << 7),
		}

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
			private ushort_be _EntryFormat;
			public EntryFlags EntryFormat { get { return (EntryFlags)(ushort)this._EntryFormat; } }

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
			public int Index;
			public String Name;
			public String StringAttribute;
			public String StringExtension;
			public int MappedFileIndex;
			protected Entry _LinkedTo;
			protected Stream _Stream;

			internal Entry(FPS4 FPS4, EntryStruct EntryStruct, String Name, int MappedFileIndex)
			{
				this.FPS4 = FPS4;
				this.EntryStruct = EntryStruct;
				this.Name = Name;
				this.MappedFileIndex = MappedFileIndex;
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

			public Stream Stream
			{
				get
				{
					if (this._Stream == null) Open();
					return this._Stream;
				}
				set
				{
					this._Stream = value;
					this._LinkedTo = null;
				}
			}

			public void SetStream(Stream Stream)
			{
				this._Stream = Stream;
				this._LinkedTo = null;
			}

			public long Length
			{
				get
				{
					if (this.Stream == null) return 0;
					return this.Stream.Length;
				}
			}

			public Stream Open()
			{
				if (this._Stream == null)
				{
					this._Stream = SliceStream.CreateWithLength(FPS4.MapStreams[MappedFileIndex], EntryStruct.Offset, EntryStruct.LengthReal);
				}
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
						"FPS4.Entry(Name='{0}', Offset=0x{1}, LengthCompressed={2}, LengthUncompressed={3}, MappedFileIndex={4}, Attribute='{5}', Extension='{6}')",
						Name,
						EntryStruct.Offset.NativeValue.ToString("X8"),
						EntryStruct.LengthSectorAligned,
						EntryStruct.LengthReal,
						MappedFileIndex,
						StringAttribute,
						StringExtension
					);
				}
			}
		}

		public FPS4 ClearAllEntries()
		{
			Entries = new Dictionary<string, Entry>();
			return this;
		}

		public Entry CreateEntry(String Name, Stream Stream)
		{
			var Entry = new Entry(this, Stream, Name);
			Entry.Index = Entries.Count;
			return Entries[Name] = Entry;
		}

		public Entry CreateEntry(String Name, Entry LinkedTo)
		{
			var Entry = new Entry(this, null, Name);
			Entry.Index = Entries.Count;
			Entry.LinkedTo = LinkedTo;
			return Entries[Name] = Entry;
		}

		public String OriginalFilePath = "";
		public List<Entry> EntriesByIndex = new List<Entry>();
		public Dictionary<String, Entry> Entries = new Dictionary<String, Entry>();
		HeaderStruct Header;
		Stream[] MapStreams;

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

		public FPS4(Stream Fps4Stream, params Stream[] ExtraStreams)
		{
			Load(Fps4Stream, ExtraStreams);
		}

		public override string ToString()
		{
			return Header.ToStringDefault();
		}

		~FPS4()
		{
			Dispose();
		}

		override public void Load(Stream Fps4Stream)
		{
			Load(Fps4Stream);
		}

		public void Load(Stream Fps4Stream, params Stream[] ExtraStreams)
		{
			this.MapStreams = new Stream[] { }.Concat(new[] { Fps4Stream }).Concat(ExtraStreams);
			//if (DavStream == null) DavStream = DatStream;

			Header = Fps4Stream.ReadStruct<HeaderStruct>();
			var Magic = Encoding.ASCII.GetString(Header.Magic);
			if (Magic != "FPS4") throw (new Exception(String.Format("Invalid Magic '{0}'", Magic)));

			if (Header.FilePos != 0)
			{
				Fps4Stream.Position = Header.FilePos;
				OriginalFilePath = Fps4Stream.ReadStringz(0x100, Encoding.GetEncoding("Shift-JIS"));
			}
			else
			{
				OriginalFilePath = "";
			}

			//Console.WriteLine("{0:X8}", Stream.Position);

			if (Header.ListCount > 10000) throw (new Exception("List too big (" + Header.ListCount + ")"));

			EntriesByIndex.Clear();
			Entries.Clear();

			//Console.WriteLine("EntryFormat: {0}", Header.EntryFormat);

			Queue<string> Extensions = new Queue<string>();

			int MaxMappedFileIndex = 0;
			Fps4Stream.Position = Header.ListStart;
			for (int n = 0; n < Header.ListCount; n++)
			{
				var EntryOffset = Fps4Stream.Position;
				var EntryStream = Fps4Stream.ReadStream(Header.EntrySizeof);
				var EntryStruct = default(EntryStruct);
				var Name = "";
				var StringAttribute = "";
				var StringExtension = "";
				int MappedFileIndex = 0;
				uint StringOffset = 0;

				if ((Header.EntryFormat & EntryFlags.HasOffset) != 0) EntryStruct.Offset = EntryStream.ReadStruct<uint_be>();
				if ((Header.EntryFormat & EntryFlags.HasLengthSectorAligned) != 0) EntryStruct.LengthSectorAligned = EntryStruct.LengthReal = EntryStream.ReadStruct<uint_be>();
				if ((Header.EntryFormat & EntryFlags.HasRealLength) != 0) EntryStruct.LengthReal = EntryStream.ReadStruct<uint_be>();
				if ((Header.EntryFormat & EntryFlags.HasInlineName) != 0) Name = EntryStream.ReadStringz(0x20);
				if ((Header.EntryFormat & EntryFlags._Unknown1) != 0) throw (new Exception("Unknown FPS4 Format Flags : _Unknown1"));
				if ((Header.EntryFormat & EntryFlags.HasStringExtension) != 0) StringExtension = EntryStream.ReadStringz(4);
				if ((Header.EntryFormat & EntryFlags.HasStringOffset) != 0) StringOffset = EntryStream.ReadStruct<uint_be>();
				if ((Header.EntryFormat & EntryFlags.HasAdditional_Uint) != 0) MappedFileIndex = (int)(uint)EntryStream.ReadStruct<uint_be>();

				if (StringOffset != 0)
				{
					StringAttribute = Fps4Stream.SliceWithLength(StringOffset, 0x1000).ReadStringz();
				}

				//Console.WriteLine("{0}: {1}: {2}", Name, StringExtension, StringAttribute);
				//Console.WriteLine("'{0}':{1}", Name, EntryStruct.ToStringDefault());

				MaxMappedFileIndex = Math.Max(MaxMappedFileIndex, (int)MappedFileIndex);

				EntriesByIndex.Add(new Entry(this, EntryStruct, Name, MappedFileIndex)
				{
					StringExtension = StringExtension,
					StringAttribute = StringAttribute,
					Index = n,
				});
			}

			// Must calculate lengths
			if (
				((Header.EntryFormat & EntryFlags.HasLengthSectorAligned) == 0) &&
				((Header.EntryFormat & EntryFlags.HasRealLength) == 0)
			)
			{
				if (MaxMappedFileIndex > 1) throw(new Exception(String.Format("Supporting just one mapped file but found {0}", MaxMappedFileIndex)));

				for (int MappedFileIndex = 0; MappedFileIndex <= MaxMappedFileIndex; MappedFileIndex++)
				{
					var EntriesForThisMap = EntriesByIndex.Where(Entry => Entry.MappedFileIndex == MappedFileIndex).ToList();
					EntriesForThisMap.Add(new Entry(this, new EntryStruct() { Offset = (uint)MapStreams[MappedFileIndex].Length }, "", MappedFileIndex));
					for (int n = 0; n < EntriesForThisMap.Count - 1; n++)
					{
						EntriesForThisMap[n].EntryStruct.LengthReal = (
							EntriesForThisMap[n + 1].EntryStruct.Offset -
							EntriesForThisMap[n + 0].EntryStruct.Offset
						);
					}
				}
			}

			if (EntriesByIndex.Count > 0)
			{
				var LastEntry = EntriesByIndex[EntriesByIndex.Count - 1];
				if (LastEntry.Name.Length == 0 && LastEntry.EntryStruct.LengthReal == 0)
				{
					EntriesByIndex.Remove(LastEntry);
				}
			}

			var LastBaseIndexPerExtension = new Dictionary<string, int>();
			string LastBaseName = "";
			string LastExtension = "";
			var ExtensionQueue = new Queue<string>();
			var MDL_Extensions = new[] { "ANM", "BLD", "CLS", "HRC", "MTR", "SHD", "SPM", "SPV", "TXM", "TXV", "SCFOMBIN" };
			var TEX_Extensions = new[] { "TXM", "TXV" };
			var SCR_Extensions = new[] { "SCFOMBIN" };
			var STA_Extensions = new[] { "AMS" };
			foreach (var Entry in EntriesByIndex)
			{
				if (Entry.Name == "")
				{
					if (Entry.StringExtension != "")
					{
						if (Entry.StringAttribute != "")
						{
							LastBaseName = Entry.StringAttribute;
						}
						else
						{
							if (!LastBaseIndexPerExtension.ContainsKey(Entry.StringExtension)) LastBaseIndexPerExtension[Entry.StringExtension] = 0;
							LastBaseName = String.Format("{0}_{1}", Entry.StringExtension, LastBaseIndexPerExtension[Entry.StringExtension]++);
						}
						LastExtension = Entry.StringExtension;
						switch (Entry.StringExtension)
						{
							case "STA": ExtensionQueue = new Queue<string>(STA_Extensions); break;
							case "MDL": ExtensionQueue = new Queue<string>(MDL_Extensions); break;
							case "TEX": ExtensionQueue = new Queue<string>(TEX_Extensions); break;
							case "SCR": ExtensionQueue = new Queue<string>(SCR_Extensions); break;
							default: throw (new Exception(String.Format("Unknown StringExtension: '{0}','{1}','{2}'", Entry.StringExtension, Entry.StringAttribute, Entry.Name)));
						}
					}

					if (ExtensionQueue.Count > 0)
					{
						Entry.Name = LastBaseName + "." + ExtensionQueue.Dequeue();
					}
				}
			}

			foreach (var Entry in EntriesByIndex)
			{
				if (Entry.Name.Length == 0 && Entry.StringExtension.Length == 0 && Entry.StringAttribute.Length > 0)
				{
					Entry.Name = Entry.StringAttribute;
					//Console.WriteLine("'{0}'", StringAttribute);
				}


				if (Entry.Name.Length == 0) Entry.Name = String.Format("{0}", Entry.Index);
				if (Entries.ContainsKey(Entry.Name)) Entry.Name = Entry.Index + "." + Entry.Name;

				//Console.WriteLine("{0}", Entry);

				Entries[Entry.Name] = Entry;
			}
		}

		public override void SaveTo(Stream Stream, bool DoAlign = true)
		{
			var EntryListWithLinks = EntryList.Union(new Entry[] { new Entry(FPS4: this, Stream: new MemoryStream(), Name:"") }).ToArray();
			var EntryListWithoutLinks = EntryListWithLinks.Where(Entry => !Entry.IsLinked).ToArray();

			var SectorPadding = DoAlign ? 0x800 : 0x10;
			var BinaryWriter = new BinaryWriter(Stream);
			var OriginalFilePathBytes = Encoding.GetEncoding("Shift-JIS").GetBytes(OriginalFilePath);
			long NamesStartOffset = Header.ListStart + Header.EntrySizeof * EntryListWithLinks.Length + OriginalFilePathBytes.Length;
			long DataStartOffset = NamesStartOffset;

			// Strings at the end of the entry list.
			if ((int)Header.EntryFormat == 0x47)
			{
				foreach (var Entry in EntryListWithLinks) DataStartOffset += Encoding.UTF8.GetByteCount(Entry.Name) + 1;
			}

			DataStartOffset = MathUtils.Align(DataStartOffset, SectorPadding);

			Header.Magic = Encoding.ASCII.GetBytes("FPS4");
			Header.ListCount = (uint)EntryListWithLinks.Length;
			Header.ListStart = (uint)Marshal.SizeOf(typeof(HeaderStruct));
			Header.ListEnd = (uint)(DataStartOffset);
			var ExtraEntrySizeof = Header.EntrySizeof - 0x0C;

			Stream.WriteStruct(Header);
			long CurrentOffset = DataStartOffset;

			// PASS1. First we calculate all the pointers for non-linked entries.
			foreach (var Entry in EntryListWithoutLinks)
			{
				var LengthReal = Entry.Length;
				var LengthSectorAligned = (uint)MathUtils.NextAligned((long)LengthReal, (long)SectorPadding);

				Entry.EntryStruct = new EntryStruct()
				{
					Offset = (uint)CurrentOffset,
					LengthSectorAligned = (uint)LengthSectorAligned,
					LengthReal = (uint)LengthReal,
				};

				CurrentOffset += LengthSectorAligned;
			}

			var EndStringNames = new MemoryStream();

			// PASS2. We then write all the EntryListWithLinks.
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
						int NameIndex = -1;
						if (!int.TryParse(Entry.Name, out NameIndex))
						{
							NameIndex = -1;
						}

						if ((Entry.Name == "") || (Entry.Index == NameIndex))
						{
							Stream.WriteStruct((uint_be)(uint)(0));
							//Console.Error.WriteLine("Zero '{0}' : {1} : {2}", Entry.Name, Entry.Index, NameIndex);
						}
						else
						{
							Stream.WriteStruct((uint_be)(uint)(NamesStartOffset + EndStringNames.Length));
							EndStringNames.WriteStringz(Entry.Name);
							//Console.Error.WriteLine("Problem?");
							//throw (new NotImplementedException());
						}
						break;
					default:
						Console.WriteLine("{0}", Entry.Name);
						Stream.WriteStringz(Entry.Name, ExtraEntrySizeof);
						break;
				}
			}

			Stream.WriteBytes(OriginalFilePathBytes);
			Stream.WriteZeroToOffset(DataStartOffset);

			// PASS3: Write Names
			if ((int)Header.EntryFormat == 0x47)
			{
				//Console.WriteLine("PASS3: Write Names");
				Stream.SliceWithBounds(NamesStartOffset, DataStartOffset).WriteStream(EndStringNames.Slice());
			}

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

		public static bool IsValid(byte[] MagicData)
		{
			try
			{
				var Header = StructUtils.BytesToStruct<HeaderStruct>(MagicData);
				var Magic = Encoding.ASCII.GetString(Header.Magic);
				if (Magic != "FPS4") return false;
				if (Header.ListCount > 10000) return false;
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
