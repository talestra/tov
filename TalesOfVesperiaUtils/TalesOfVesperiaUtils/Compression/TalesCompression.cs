using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils;

namespace TalesOfVesperiaUtils.Compression
{
	abstract public class TalesCompression
	{
		abstract public void EncodeFile(Stream InputStream, Stream OutputStream);
		abstract public void DecodeFile(Stream InputStream, Stream OutputStream);

		public Stream DecodeFile(Stream InputStream)
		{
			Stream OutputStream = new MemoryStream();
			DecodeFile(InputStream, OutputStream);
			OutputStream.Position = 0;
			return OutputStream;
		}
		public Stream EncodeFile(Stream InputStream)
		{
			Stream OutputStream = new MemoryStream();
			EncodeFile(InputStream, OutputStream);
			OutputStream.Position = 0;
			return OutputStream;
		}
		static public Stream DecompressStream(Stream InputStream)
		{
			InputStream = SliceStream.CreateWithLength(InputStream, 0);
			byte[] Info = InputStream.ReadBytes(0x10);
			//Console.WriteLine(Info.ToHexString());
			var Decompressor = CreateFromStart(Info);
			InputStream.Position = 0;
			return Decompressor.DecodeFile(InputStream);
		}

		static public TalesCompression CreateFromStart(byte[] MagicData)
		{
			var Warnings = new List<String>();

			if (MagicData.Length < 0x10) throw (new Exception("Start to short"));
			// Version type (0, 1, 3, 4)
			{
				var HeaderStruct = StructUtils.BytesToStruct<TalesCompression1_3.HeaderStruct>(MagicData);

				bool Handled = true;

				switch (HeaderStruct.Version)
				{
					case 0:
						if (HeaderStruct.CompressedLength != HeaderStruct.UncompressedLength)
						{
							Warnings.Add("Compressed/Uncompressed Length must match");
						}
						if (HeaderStruct.CompressedLength >= 64 * 1024 * 1024)
						{
							Warnings.Add("Compressed/Uncompressed Length too big");
						}
						break;
					case 1:
					case 3:
					case 4:
						if (HeaderStruct.CompressedLength >= 64 * 1024 * 1024)
						{
							Warnings.Add("Compressed Length too big");
						}
						if (HeaderStruct.UncompressedLength >= 64 * 1024 * 1024)
						{
							Warnings.Add("Uncompressed Length too big");
						}
						if (HeaderStruct.CompressedLength > HeaderStruct.UncompressedLength)
						{
							Warnings.Add("Compressed size is bigger than the uncompressed size");
						}

						break;
					default:
						Handled = false;
						break;
				}

				if (Handled)
				{
					if (!Warnings.Any())
					{
						return CreateFromVersion(HeaderStruct.Version);
					}
				}
			}
			// Check other types.
			{
				if (MagicData.SequenceEqual(TalesCompression15_Lzx.Signature))
				{
					return CreateFromVersion(15);
				}
			}
			throw (new NotSupportedException("Version not detected : " + Warnings.Implode(",") + " : " + MagicData.ToHexString()));
			//throw (new Exception());
		}

		static public TalesCompression CreateFromVersion(int Version)
		{
			switch (Version)
			{
				case 0: return new TalesCompression0();
				case 1: case 3: return new TalesCompression1_3(Version);
				case 15: return new TalesCompression15_Lzx();
				case 4: throw (new NotImplementedException());
				case 7: throw (new NotImplementedException());
				default: throw (new Exception("Unknown Version '" + Version + "'"));
			}
		}
	}
}
