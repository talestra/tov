//#define DEBUG_ISO_LOADING

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSharpUtils;
using CSharpUtils.Streams;
using System.Runtime.InteropServices;

namespace TalesOfVesperiaUtils.Formats.Packages
{
	public class Dvd9Xbox360
	{
		public static uint SECTOR_SIZE = 0x800;
		public static uint XDVDFS_SECTOR_XBOX1 = 0x30600;
		public static uint XDVDFS_SECTOR_XBOX360 = 0x1FB20;
		public static uint XDVDFS_SECTOR_XBOX360_LAYER1 = 0x1B3880;

		public Entry RootEntry;
		protected Stream IsoStream;

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		struct MediaHeaderStruct
		{
			/// <summary>
			/// 
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.AnsiBStr)]
			public byte[] Magic;

			/// <summary>
			/// 
			/// </summary>
			public uint RootSector;

			/// <summary>
			/// 
			/// </summary>
			public uint RootSize;

			/// <summary>
			/// 
			/// </summary>
			String MagicString
			{
				get
				{
					return Magic.GetString();
				}
			}

			/// <summary>
			/// 
			/// </summary>
			public void CheckValid()
			{
				if (MagicString != "MICROSOFT*XBOX*MEDIA")
				{
					throw (new Exception("Invalid Xbox360 ISO Image"));
				}
			}
		}

		public Dvd9Xbox360()
		{
		}

		static public Stream GetStreamByLBA(Stream Stream, long LBA, long Size = -1)
		{
			return SliceStream.CreateWithLength(Stream, LBA * SECTOR_SIZE, Size);
		}

        public Dvd9Xbox360 Load(Stream Stream)
		{
			IsoStream = GetStreamByLBA(Stream, XDVDFS_SECTOR_XBOX360);
			var StartStream = GetStreamByLBA(IsoStream, 0x20);
			var MediaHeader = StartStream.ReadStruct<MediaHeaderStruct>();
			MediaHeader.CheckValid();

			RootEntry = new Entry();

			//Console.WriteLine("{0:X}", (XDVDFS_SECTOR_XBOX360 + MediaHeader.RootSector) * SECTOR_SIZE);
			LoadProcessNode(
				IsoStream : IsoStream,
				RootStream: GetStreamByLBA(IsoStream, MediaHeader.RootSector, MediaHeader.RootSize),
				EntryParent: RootEntry
			);

            return this;
		}

		public enum EntryAttributes : byte
		{
			File = 0x80,
			Directory = 0x10,
		}

		public class Entry : IEnumerable<Entry>
		{
			public Dvd9Xbox360 Dvd9Xbox360;
			public Entry Parent = null;
			public List<Entry> Childs = new List<Entry>();
			public Dictionary<String, Entry> ChildsByName = new Dictionary<String, Entry>();

			public Entry Root
			{
				get
				{
					if (Parent == null) return this;
					return Parent.Root;
				}
			}

			public uint SectorOffset;
			public uint Size;
			public EntryAttributes Attributes;
			public byte NameSize;
			public String Name;

			public bool IsDirectory { get { return (Attributes & EntryAttributes.Directory) != 0; } }
			public bool IsFile { get { return (Attributes & EntryAttributes.File) != 0; } }

			public String FullName
			{
				get
				{
					if (Parent != null) return Parent.FullName + "/" + Name;
					return Name;
				}
			}

			public override string ToString()
			{
				return String.Format("EntryStruct(FullName='{0}', Offset={1}, Size={2}, Attributes={3})", FullName, SectorOffset, Size, Attributes);
			}

			public Stream Open()
			{
				return Dvd9Xbox360.GetStreamByLBA(Dvd9Xbox360.IsoStream, SectorOffset, Size);
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
				Console.Write("Replacing '{0}' with '{1}'...", this.FullName, SourceFileName);
				using (var SourceStream = File.OpenRead(SourceFileName))
				{
					ReplaceWithStream(SourceStream);
				}
				Console.WriteLine("Ok");
			}

			public Entry this[String Path]
			{
				get
				{
					var Parts = Path.Replace('\\', '/').Split(new char[] { '/' });
					Entry That = this;
					int PartIndex = 0;
					foreach (var Part in Parts)
					{
						switch (Part)
						{
							case "":
								if (PartIndex == 0) That = That.Root;
								break;
							case ".":
								break;
							case "..":
								if (That.Parent != null) That = That.Parent;
								break;
							default:
								if (That.ChildsByName.ContainsKey(Part))
								{
									That = That.ChildsByName[Part];
								}
								else
								{
									throw(new KeyNotFoundException("Can't find component '" + Part + "' in path '" + Path + "'"));
								}
								break;
						}
						PartIndex++;
					}
					return That;
					/*
					var Parts = Path.Replace('\\', '/').Split(new char[] { '/' }, 2);
					Entry That = this;
					if (Parts.Length >= 1)
					{
						switch (Parts[0])
						{
							case "": That = Root; break;
							case ".": That = this; break;
							case "..": That = Parent; break;
							default:
								That = ChildsByName[Parts[0]];
								break;
						}
						if (Parts.Length >= 2)
						{
							That = That[Parts[1]];
						}
					}
					return That;
					*/
				}
			}

			public IEnumerable<Entry> Descendant
			{
				get
				{
					//IEnumerable<Entry> Descendant = Childs;
					IEnumerable<Entry> Descendant = new List<Entry>();
					foreach (var Node in Childs)
					{
						var ListSingleNode = new List<Entry>();
						ListSingleNode.Add(Node);
						Descendant = Descendant.Concat(ListSingleNode).Concat(Node.Descendant);
					}
					return Descendant;
				}
			}

			public IEnumerator<Entry> GetEnumerator()
			{
				return Childs.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return Childs.GetEnumerator();
			}
		}

		protected void LoadProcessNode(Stream IsoStream, Stream RootStream, uint Offset = 0, Entry EntryParent = null, int Level = 0)
		{
			var NodeReader = new BinaryReader(RootStream);

			RootStream.Position = Offset;

			if (Level > 20)
			{
				throw (new Exception("Too deep tree"));
			}

			// LeftNode.Name < ThisNode.Name < RightNode.Name
			uint LeftNodeOffset = (uint)(NodeReader.ReadUInt16() * 4);
			uint RightNodeOffset = (uint)(NodeReader.ReadUInt16() * 4);

#if DEBUG_ISO_LOADING
			Console.WriteLine("LEFT: {0:X}, RIGHT: {1:X}", LeftNodeOffset, RightNodeOffset);
#endif

			var Entry = new Entry();
			Entry.Dvd9Xbox360 = this;
			Entry.Parent = EntryParent;

			{
				Entry.SectorOffset = NodeReader.ReadUInt32();
				Entry.Size = NodeReader.ReadUInt32();
				Entry.Attributes = (EntryAttributes)NodeReader.ReadByte();
				Entry.NameSize = NodeReader.ReadByte();

				Entry.Name = NodeReader.ReadBytes(Entry.NameSize).GetString(Encoding.ASCII);

				if (Entry.Attributes.HasFlag(EntryAttributes.Directory))
				{
					LoadProcessNode(
						IsoStream,
						GetStreamByLBA(IsoStream, Entry.SectorOffset, Entry.Size),
						0,
						Entry
					);
					// @TODO
				}
			}

			if (LeftNodeOffset > 0)
			{
				LoadProcessNode(IsoStream, RootStream, LeftNodeOffset, EntryParent, Level + 1);
			}

			{
				EntryParent.Childs.Add(Entry);
#if DEBUG_ISO_LOADING
				Console.Error.WriteLine("--- {0}", Entry.Name);
#endif
				EntryParent.ChildsByName.Add(Entry.Name, Entry);
#if DEBUG_ISO_LOADING
				Console.WriteLine(Entry);
#endif
			}

			if (RightNodeOffset > 0)
			{
				LoadProcessNode(IsoStream, RootStream, RightNodeOffset, EntryParent, Level + 1);
			}
		}
	}
}
