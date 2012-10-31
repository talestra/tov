using CSharpUtils;
using CSharpUtils.Endian;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaUtils.Audio
{
	public class SE3
	{
		public struct HeaderStruct
		{
			public uint_be Magic;
			public uint NameSize;
			public uint_be Unk1;
			public uint_be DataStart;
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0xF0)]
		public struct XMAEntryHeaderStruct
		{
			/// <summary>
			/// 'xma '
			/// </summary>
			[FieldOffset(0x0000)]
			public uint_be Magic;

			[FieldOffset(0x0014)]
			public uint_be DataLength;

			[FieldOffset(0x0018)]
			public uint_be DataStart;

			[FieldOffset(0x00BC + 2 * 0)]
			public ushort_be v0;

			[FieldOffset(0x00BC + 2 * 1)]
			public ushort_be v1;

			[FieldOffset(0x00BC + 2 * 2)]
			public ushort_be v2;

			[FieldOffset(0x00BC + 2 * 3)]
			public ushort_be v3;

			[FieldOffset(0x00BC + 2 * 4)]
			public ushort_be v4;

			[FieldOffset(0x00BC + 2 * 5)]
			public ushort_be v5;

			[FieldOffset(0x00BC + 2 * 6 + 4 * 0)]
			public uint_be v6;

			[FieldOffset(0x00BC + 2 * 6 + 4 * 1)]
			public uint_be v7;

			[FieldOffset(0x00BC + 2 * 6 + 4 * 2)]
			public uint_be v8;

			[FieldOffset(0x00BC + 2 * 6 + 4 * 3)]
			public uint_be v9;

			[FieldOffset(0x00BC + 2 * 6 + 4 * 4 + 2 * 0)]
			public ushort_be v10;

			[FieldOffset(0x00BC + 2 * 6 + 4 * 4 + 2 * 1)]
			public ushort_be v11;
		}

		public class SE3Entry
		{
			public string Name;
			public XMAEntryHeaderStruct Header;
			public Stream DataStream;

			public void ToWav(Stream Output)
			{
				XMA2WAV.ConvertXmaToWav(ToXmaWav(), Output);
				//System.Resources.
			}

			public MemoryStream ToXmaWav()
			{
				var WavStream = new MemoryStream();
				WavStream.WriteString("RIFF");
				WavStream.WriteStruct((uint_le)0);
				WavStream.WriteString("WAVE");
				WavStream.WriteString("fmt ");
				WavStream.WriteStruct((uint_le)0x20);

				WavStream.WriteStruct((ushort_le)Header.v0);
				WavStream.WriteStruct((ushort_le)Header.v1);
				WavStream.WriteStruct((ushort_le)Header.v2);
				WavStream.WriteStruct((ushort_le)Header.v3);
				WavStream.WriteStruct((ushort_le)Header.v4);
				WavStream.WriteStruct((ushort_be)Header.v5);

				WavStream.WriteStruct((uint_le)Header.v6);
				WavStream.WriteStruct((uint_le)Header.v7);
				WavStream.WriteStruct((uint_le)Header.v8);
				WavStream.WriteStruct((uint_le)Header.v9);
				WavStream.WriteStruct((ushort_be)Header.v10);
				WavStream.WriteStruct((ushort_le)Header.v11);
				WavStream.WriteString("data");
				WavStream.WriteStruct((uint_le)DataStream.Length);
				WavStream.WriteStream(DataStream.Slice());

				WavStream.SliceWithLength(4).WriteStruct((uint_le)WavStream.Length);

				WavStream.Position = 0;

				return WavStream;
			}
		}

		public List<SE3Entry> Entries;
		public Dictionary<string, SE3Entry> EntriesWithName;

		public SE3 Load(Stream Stream)
		{
			var Header = Stream.ReadStruct<HeaderStruct>();
			if (Header.Magic != 0x53453320) throw(new Exception("Not a SE3 File"));
			var NameStream = Stream.SliceWithLength(0x10, Header.NameSize);
			var ELBP = NameStream.ReadStruct<uint_be>();
			var NameCount = (uint)NameStream.ReadStruct<uint_be>();
			Entries = new List<SE3Entry>();
			EntriesWithName = new Dictionary<string, SE3Entry>();
			for (int n = 0; n < NameCount; n++)
			{
				var Entry = new SE3Entry()
				{
					Name = NameStream.ReadStringz(0x30),
				};
				Entries.Add(Entry);
				EntriesWithName[Entry.Name] = Entry;
			}
			_LoadData(Stream.SliceWithLength(Header.DataStart));
			return this;
		}

		private void _LoadData(Stream Stream)
		{
			var Unk0 = Stream.ReadStruct<uint_be>();
			var Unk1 = Stream.ReadStruct<uint_be>();
			var Unk2 = Stream.ReadStruct<uint_be>();
			var Count = (uint)Stream.ReadStruct<uint_be>();
			var DataStart = Stream.ReadStruct<uint_be>();
			var Unk4 = Stream.ReadStruct<uint_be>();
			var Unk5 = Stream.ReadStruct<uint_be>();
			var DataStream = Stream.SliceWithLength(DataStart);
			for (int n = 0; n < Count; n++)
			{
				var Offset = Stream.ReadStruct<uint_be>();
				var Header = Stream.SliceWithLength(Offset).ReadStruct<XMAEntryHeaderStruct>();
				Entries[n].Header = Header;
				Entries[n].DataStream = DataStream.SliceWithLength(Header.DataStart, Header.DataLength);
			}
		}

		static public bool IsValid(byte[] Data)
		{
			try
			{
				var Header = StructUtils.BytesToStruct<HeaderStruct>(Data);
				if (Header.Magic != 0x53453320) return false;
				return true;
			}
			catch
			{
			}
			return false;
		}
	}
}
