﻿using System;
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
		virtual public byte[] EncodeBytes(byte[] Uncompressed)
		{
			return EncodeFile(new MemoryStream(Uncompressed)).ToArray();
		}
		virtual public byte[] DecodeBytes(byte[] Compressed)
		{
			return DecodeFile(new MemoryStream(Compressed)).ToArray();
		}

		public MemoryStream DecodeFile(Stream InputStream)
		{
			var OutputStream = new MemoryStream();
			DecodeFile(InputStream, OutputStream);
			OutputStream.Position = 0;
			return OutputStream;
		}
		public MemoryStream EncodeFile(Stream InputStream)
		{
			var OutputStream = new MemoryStream();
			EncodeFile(InputStream, OutputStream);
			OutputStream.Position = 0;
			return OutputStream;
		}
		static public MemoryStream DecompressStream(Stream InputStream)
		{
			var Decompressor = CreateFromStart(InputStream.Slice().ReadBytes(0x10), InputStream.Length);
			return Decompressor.DecodeFile(InputStream);
		}

		static public int DetectVersion(byte[] MagicData, long FileSize, bool ThrowException = false)
		{
			var Warnings = new List<String>();

			MagicData = MagicData.Take(0x10).ToArray();
			if (MagicData.Length < 0x10) throw (new Exception("Start to short"));

			// Version type (0, 1, 3, 4)
			{
				var HeaderStruct = StructUtils.BytesToStruct<TalesCompression1_3.HeaderStruct>(MagicData);
				//Console.WriteLine("[1]");

				bool Handled = true;

				switch (HeaderStruct.Version)
				{
#if false
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
#endif
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
						if (Math.Abs((long)HeaderStruct.CompressedLength - FileSize) >= 0x1000)
						{
							Warnings.Add("Invalid compressed size");
						}

						break;
					default:
						Handled = false;
						break;
				}

				//Console.WriteLine("[2]");

				if (Handled)
				{
					if (!Warnings.Any())
					{
						return HeaderStruct.Version;
					}
				}

				//Console.WriteLine("[3]");
			}

			//Console.WriteLine("[4]");

			// Check other types.
			{
				if (MagicData.SequenceEqual(TalesCompression15_Lzx.Signature))
				{
					//Console.WriteLine("[5]");
					return 15;
				}
			}

			if (!ThrowException)
			{
				return -1;
			}
			else
			{
				//Console.WriteLine("[6]");

				throw (new NotSupportedException("Version not detected : " + Warnings.Implode(",") + " : " + MagicData.ToHexString()));
				//throw (new Exception());
			}
		}


		static public TalesCompression CreateFromStart(byte[] MagicData, long FileSize)
		{
			return CreateFromVersion(DetectVersion(MagicData, FileSize, true));
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
