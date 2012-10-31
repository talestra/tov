using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils;
using System.Diagnostics;
using CSharpUtils.SpaceAssigner;
using TalesOfVesperiaUtils.Text;
using CSharpUtils.Endian;

namespace TalesOfVesperiaUtils.Formats.Packages
{
	public class TO8CHTX : BasePackage, IEnumerable<TO8CHTX.Entry>, IDisposable
	{
		Stream Stream, StreamEntries, StreamTexts;

		public enum Talker : uint
		{
			Yuri = 1,
			Estellise = 2,
			Karol = 3,
			Rita = 4,
			Raven = 5,
			Judith = 6,
			Repede = 7,
			Flynn = 8,
			//Patty = 9, // ???
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 0x20)]
		public struct HeaderStruct
		{
			/// <summary>
			/// 
			/// </summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
			public string Magic;

			/// <summary>
			/// 
			/// </summary>
			public uint_be FileSize;

			/// <summary>
			/// 
			/// </summary>
			public uint_be TextCount;

			/// <summary>
			/// 
			/// </summary>
			public uint_be OffsetEntries;

			/// <summary>
			/// 
			/// </summary>
			public uint_be OffsetTexts;

			/// <summary>
			/// 
			/// </summary>
			public ulong_be Padding;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 0x10)]
		public struct EntryStruct
		{
			/// <summary>
			/// 
			/// </summary>
			public uint_be OffsetTitle;

			/// <summary>
			/// 
			/// </summary>
			public uint_be OffsetTextOriginal;

			/// <summary>
			/// 
			/// </summary>
			public uint_be OffsetTextTranslated;

			/// <summary>
			/// 
			/// </summary>
			public uint_be Talker;
		}

		public class Entry
		{
			public String Title, TextOriginal, TextTranslated;
			public Talker Talker = 0;

			public Entry()
			{
			}

			public void Load(TO8CHTX TO8CHTX, EntryStruct EntryStruct)
			{
				this.Title = TO8CHTX.StreamTexts.SetPosition(EntryStruct.OffsetTitle).ReadStringz(-1, Encoding.UTF8);
				this.TextOriginal = TO8CHTX.StreamTexts.SetPosition(EntryStruct.OffsetTextOriginal).ReadStringz(-1, Encoding.UTF8);
				this.TextTranslated = TO8CHTX.StreamTexts.SetPosition(EntryStruct.OffsetTextTranslated).ReadStringz(-1, Encoding.UTF8);
				this.Talker = (Talker)(uint)EntryStruct.Talker;
			}

			public override string ToString()
			{
				return String.Format(
					"TO8CHTX.Entry(Title='{0}', TextOriginal='{1}', TextTranslated='{2}', Talker={3})",
					this.Title,
					this.TextOriginal,
					this.TextTranslated,
					this.Talker
				);
			}
		}

		public List<Entry> Entries = new List<Entry>();

		public TO8CHTX()
		{
		}

		public TO8CHTX(Stream Stream)
		{
			Load(Stream);
		}

		~TO8CHTX()
		{
			Dispose();
		}

		override public void Load(Stream Stream)
		{
			this.Stream = Stream;
			var HeaderStruct = Stream.ReadStruct<HeaderStruct>();
			Debug.Assert(HeaderStruct.Magic == "TO8CHTX");
			Debug.Assert(HeaderStruct.FileSize <= Stream.Length);
			StreamEntries = SliceStream.CreateWithLength(Stream, HeaderStruct.OffsetEntries);
			StreamTexts = SliceStream.CreateWithLength(Stream, HeaderStruct.OffsetTexts);
			for (int n = 0; n < HeaderStruct.TextCount; n++)
			{
				var EntryStruct = StreamEntries.ReadStruct<EntryStruct>();
				var Entry = new Entry();
				Entry.Load(this, EntryStruct);
				Entries.Add(Entry);
			}
		}

		override public void Save(Stream Stream)
		{
			var SpaceAssigner1D = new SpaceAssigner1D().AddAllPositiveAvailable();
			var SpaceAssigner1DUniqueAllocator = new SpaceAssigner1DUniqueAllocatorStream(SpaceAssigner1D, new MemoryStream());
			SpaceAssigner1DUniqueAllocator.Encoding = Encoding.UTF8;

			SpaceAssigner1DUniqueAllocator.AllocateUnique(SpaceAssigner1DUniqueAllocator.Encoding.GetBytes("dummy"));
			SpaceAssigner1DUniqueAllocator.AllocateUnique(new byte[] { 0 });

			var EntryStructs = new List<EntryStruct>();

			foreach (var Entry in Entries)
			{
				EntryStructs.Add(new EntryStruct()
				{
					OffsetTitle = (uint)SpaceAssigner1DUniqueAllocator.AllocateUnique(Entry.Title).Min,
					OffsetTextOriginal = (uint)SpaceAssigner1DUniqueAllocator.AllocateUnique(Entry.TextOriginal).Min,
					OffsetTextTranslated = (uint)SpaceAssigner1DUniqueAllocator.AllocateUnique(Entry.TextTranslated).Min,
					Talker = (uint)Entry.Talker,
				});
			}

			var OffsetEntries = Marshal.SizeOf(typeof(HeaderStruct));
			var OffsetTexts = OffsetEntries + Entries.Count * Marshal.SizeOf(typeof(EntryStruct));
			var OffsetEnd = OffsetTexts + SpaceAssigner1DUniqueAllocator.Stream.Length;

			Stream.WriteStruct(new HeaderStruct()
			{
				Magic = "TO8CHTX",
				OffsetEntries = (uint)OffsetEntries,
				OffsetTexts = (uint)OffsetTexts,
				TextCount = (uint)Entries.Count,
				FileSize = (uint)OffsetEnd,
				Padding = 0,
			});
			

			foreach (var EntryStruct in EntryStructs)
			{
				Stream.WriteStruct(EntryStruct);
			}

			SpaceAssigner1DUniqueAllocator.Stream.CopyToFast(Stream);
		}

		public IEnumerator<TO8CHTX.Entry> GetEnumerator()
		{
			return Entries.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public void TranslateWithAcmeFile(ACME1.File ACME1File)
		{
			/*
			Console.Error.WriteLine("LENGTH: {0}, {1}", Length, ACME1File.Entries.Count);
			foreach (var Entry in ACME1File.Entries) Console.Error.WriteLine(Entry);
			*/

			if (ACME1File.Entries.Count < Length + 1) throw(new InvalidDataException("Invalid ACME file"));

			for (int n = 0; n < Length; n++)
			{
				var ChtxEntry = this.Entries[n];
				var AcmeEntry = ACME1File.Entries[n + 1];
				var AcmeEntrySets = AcmeEntry.Text.Split(new char[] { '\n' }, 2);

				ChtxEntry.TextOriginal = "";
				ChtxEntry.Title = TextProcessor.Instance.ProcessAndDetectPitfalls(ChtxEntry.Title, AcmeEntrySets[0]);
				ChtxEntry.TextTranslated = TextProcessor.Instance.ProcessAndDetectPitfalls(ChtxEntry.TextTranslated, AcmeEntrySets[1]);
			}
		}

		public Entry this[int index]
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

		public int Length
		{
			get
			{
				return Entries.Count;
			}
		}

		public void Dispose()
		{
			//throw new NotImplementedException();
		}

		static public bool IsValid(byte[] Data)
		{
			try
			{
				var Header = StructUtils.BytesToStruct<HeaderStruct>(Data);
				if (Header.Magic != "TO8CHTX") return false;
				return true;
			}
			catch
			{
			}
			return false;
		}
	}
}
