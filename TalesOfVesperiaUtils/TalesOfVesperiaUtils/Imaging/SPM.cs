using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;
using CSharpUtils.Drawing;
using CSharpUtils.Endian;
using TalesOfVesperiaUtils.Imaging.Internal;

namespace TalesOfVesperiaUtils.Imaging
{
	unsafe public class SPM
	{
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
		struct MagicStruct
		{
			/// <summary>
			/// 0000 -
			/// </summary>
			[FieldOffset(0x0000)]
			public uint_be Magic;

			public bool IsValid
			{
				get { return Magic == 0x00010000; }
			}
		}

		struct HeaderStruct
		{
			public uint Magic;
			public uint_le FileSize;
			public uint_le EntryCount;
			public uint_le _Unknown;
			public uint_le _Padding;
			public uint_le SizeFromHere; // Probably a Stream
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0xA0)]
		struct EntryStruct
		{
			[FieldOffset(0x007C)]
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x24)]
			public string Name;
		}

		public SPM()
		{
		}

		public void Load(Stream Stream)
		{
			var Magic = Stream.ReadStruct<MagicStruct>();
			if (!Magic.IsValid) throw(new Exception("Invalid SPM stream"));
			//var Left = Stream.ReadStream(Stream.ReadStruct<uint_be>());
			Stream.Position = 0xE4;
			HandlePacket(Stream, 0);
		}

		public void HandlePacket(Stream Stream, int Level)
		{
			while (!Stream.Eof())
			{
				var PacketType = Stream.ReadStruct<uint_be>();
				var PacketLength = Stream.ReadStruct<uint_be>();
				var Stream2 = Stream.ReadStream(PacketLength - 8);
				Console.WriteLine("{0}{1}: {2}", new String(' ', Level * 4), PacketType, PacketLength);
				switch (PacketType)
				{
					case 0x00000200:
						var Entry = Stream2.ReadStruct<EntryStruct>();
						Console.WriteLine(Entry.Name);
					break;
				}
			}
		}

		public static bool IsValid(byte[] MagicData)
		{
			try
			{
				return StructUtils.BytesToStruct<MagicStruct>(MagicData).IsValid;
			}
			catch
			{
			}
			return false;
		}
	}
}
