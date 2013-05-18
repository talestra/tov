using CSharpUtils.Endian;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaUtils.Formats.Misc
{
	public class TO8MAPL
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 0x001C)]
		public struct Header
		{
			/// <summary>
			/// 
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			//[FieldOffset(0x0000)]
			public byte[] Magic;

			/// <summary>
			/// 
			/// </summary>
			public uint_be End;

			/// <summary>
			/// 
			/// </summary>
			public uint_be Start;

			/// <summary>
			/// 
			/// </summary>
			public uint_be Count;

			/// <summary>
			/// 
			/// </summary>
			public uint_be TextPointer;

			/// <summary>
			/// 
			/// </summary>
			public uint_le End_le;

			/// <summary>
			/// 
			/// </summary>
			public uint_be Padding;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 0x001C)]
		public struct EntryStruct
		{
			/// <summary>
			/// 
			/// </summary>
			public uint_be TextPointer1;

			/// <summary>
			/// 
			/// </summary>
			public uint_be TextPointer2;

			/// <summary>
			/// 
			/// </summary>
			public uint_be TextPointer3;

			/// <summary>
			/// 
			/// </summary>
			private uint_be _Zero1;

			/// <summary>
			/// 
			/// </summary>
			private uint_be _Zero2;

			/// <summary>
			/// 
			/// </summary>
			public uint_be Flags;

			/// <summary>
			/// 
			/// </summary>
			private uint_be _Zero3;

			/// <summary>
			/// 
			/// </summary>
			private uint_be _Zero4;
		}

		public class Entry
		{
			public int Index;
			public EntryStruct EntryStruct;
			public string Text1;
			public string Text2;
			public string Text3;
			public uint Flags { get { return EntryStruct.Flags; } }

			public string NoDummyText
			{
				get
				{
					if (Text1 != "dummy") return Text1;
					if (Text2 != "dummy") return Text2;
					if (Text3 != "dummy") return Text3;
					return "-";
				}
			}

			public static Entry FromEntryStruct(int Index, EntryStruct EntryStruct, Stream TextStream)
			{
				return new Entry()
				{
					Index = Index,
					Text1 = TextStream.ReadStringzAt(EntryStruct.TextPointer1),
					Text2 = TextStream.ReadStringzAt(EntryStruct.TextPointer2),
					Text3 = TextStream.ReadStringzAt(EntryStruct.TextPointer3),
					EntryStruct = EntryStruct,
				};
			}
		}

		public Entry[] Entries;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Stream"></param>
		/// <returns></returns>
		static public TO8MAPL Parse(Stream Stream)
		{
			Stream.Position = 0;
			var Header = Stream.ReadStruct<Header>();
			var Entries = Stream.ReadStructVectorAt<EntryStruct>(Header.Start, Header.Count);
			var TextStream = Stream.SliceWithBounds(Header.TextPointer, Header.End);
			int Index = 0;
			return new TO8MAPL()
			{
				Entries = Entries.Select(EntryStruct => Entry.FromEntryStruct(Index++, EntryStruct, TextStream)).ToArray(),
			};
		}
	}
}
