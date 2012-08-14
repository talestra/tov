using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils.Extensions;
using System.Text.RegularExpressions;

namespace TalesOfVesperiaUtils.Formats
{
	public class ACME1 : IDisposable
	{
		internal ZipFile ZipFile;

		public class File
		{
			public ACME1 ACME1;
			public ZipEntry ZipEntry;

			public class Entry
			{
				public int Index;
				public String Text;
				public Dictionary<String, String> Attributes = null;
			}

			protected List<Entry> _Entries;
			public List<Entry> Entries
			{
				get
				{
					if (_Entries == null)
					{
						_Entries = ACME1.ParseAcmeFile(Contents, Name);
					}
					return _Entries;
				}
			}

			public String _Name;
			public String Name
			{
				get
				{
					if (_Name == null)
					{
						_Name = ZipEntry.CleanName(ZipEntry.Name).Substr(4).Substr(0, -4);
					}
					return _Name;
				}
			}

			protected Stream _Stream;
			public Stream Stream
			{
				get
				{
					if (_Stream == null)
					{
						_Stream = new MemoryStream();
						var DecompressedStream = ACME1.ZipFile.GetInputStream(ZipEntry);
						//ACME1.ZipFile.err
						//Console.Error.WriteLine("STREAM!: " + DecompressedStream.Length);
						DecompressedStream.CopyToFast(_Stream);
						//DecompressedStream.CopyTo(_Stream);
						_Stream.Position = 0;
					}
					return SliceStream.CreateWithLength(_Stream);
				}
			}

			protected String _Contents;
			public String Contents
			{
				get
				{
					if (_Contents == null)
					{
						return Stream.ReadAllContentsAsString(ACME1.Encoding);
					}
					return _Contents;
				}
			}

			internal File(ACME1 Acme1, ZipEntry ZipEntry)
			{
				this.ACME1 = Acme1;
				this.ZipEntry = ZipEntry;
			}

			public override string ToString()
			{
				return "ACME1.File(Name='" + Name + "')";
			}
		}

		static public List<ACME1.File.Entry> ParseAcmeFile(Stream Contents, String Name, Encoding Encoding)
		{
			return ParseAcmeFile(Contents.ReadAllContentsAsString(Encoding), Name);
		}

		static public List<ACME1.File.Entry> ParseAcmeFile(String Contents, String Name)
		{
			if (Contents.Length == 0) throw(new InvalidDataException("Invalid ACME1 file '" + Name + "'"));
			var _Entries = new List<ACME1.File.Entry>();
			var Splits = Contents.Split(new string[] { "## POINTER " }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var Split in Splits)
			{
				var Parts = Split.Split(new string[] { "\n" }, 2, StringSplitOptions.None);
				String Header = Parts[0], Body = Parts[1].TrimEnd();
				int Index = Convert.ToInt32((new Regex(@"^(\d+)")).Match(Header).Captures[0].Value);
				Body = Body.Replace("\r\n", "\n").Replace("\r", "\n");
				_Entries.Add(new ACME1.File.Entry()
				{
					Index = Index,
					Text = Body,
				});
				//Console.WriteLine(Parts[0]);
				//Console.WriteLine(Parts[1]);
			}
			return _Entries;
		}

		public ACME1()
		{
		}

		public ACME1(Stream Stream, Encoding Encoding)
		{
			this.Load(Stream, Encoding);
		}

		public Dictionary<String, ACME1.File> Files = new Dictionary<String, ACME1.File>();
		public List<ACME1.File> FilesByIndex = new List<ACME1.File>();
		internal Encoding Encoding;

		public void Load(Stream Stream, Encoding Encoding)
		{
			this.ZipFile = new ZipFile(Stream);
			this.Encoding = Encoding;

			FilesByIndex.Clear();
			foreach (var ZipEntry in SourceAcmeTextFiles)
			{
				var Acme1File = new ACME1.File(this, ZipEntry);
				FilesByIndex.Add(Acme1File);
			}
			Files = FilesByIndex.CreateDictionary(File => File.Name);
		}

		protected IEnumerable<ZipEntry> SourceAcmeTextFiles
		{
			get
			{
				return ZipFile.Cast<ZipEntry>().Where(Entry => new Wildcard("SRC/*.txt").IsMatch(Entry.Name));
			}
		}

		public void Dispose()
		{
		}
	}
}
