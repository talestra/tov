using CSharpUtils;
using CSharpUtils.Endian;
using CSharpUtils.Streams;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TalesOfVesperiaUtils.Compression.CAB;
using TalesOfVesperiaUtils.Compression.LZX;

namespace TalesOfVesperiaUtils.Compression
{
	// .text:823C90A0 compression_15_perform_2:               # CODE XREF: compression_15_perform:loc_823C8B40p
	unsafe public class TalesCompression15_Lzx : TalesCompression
	{
		readonly static public byte[] Signature = new byte[] {
			0x0F, 0xF5, 0x12, 0xEE, 0x01, 0x02, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
		};

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		public struct HeaderStruct
		{
			/// <summary>
			/// This value should be the same as Signature.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
			public byte[] Magic;

			/// <summary>
			/// This value is always 0x20000.
			/// </summary>
			public uint_be Magic1;

			/// <summary>
			/// This value is always 0x80000.
			/// </summary>
			public uint_be Magic2;

			/// <summary>
			/// This value is always 0x00000.
			/// </summary>
			public uint_be Magic3;

			/// <summary>
			/// Uncompressed Size.
			/// </summary>
			public uint_be UncompressedSize;

			/// <summary>
			/// This value is always 0x00000.
			/// </summary>
			public uint_be Magic4;

			/// <summary>
			/// Size of the file minus 0x20.
			/// </summary>
			public uint_be CompressedSizePlus4;

			/// <summary>
			/// Uncompressed Size (Again).
			/// </summary>
			public uint_be UncompressedSize2;

			/// <summary>
			/// Size of the file minus 0x24.
			/// </summary>
			public uint_be CompressedSize;

			/// <summary>
			/// Size of the file minus 0x24.
			/// </summary>
			public uint_be CompressedSize2;
		}

		public override void DecodeFile(Stream InputStream, Stream OutputStream)
		{
			byte[] CompressedBytes = new byte[0x10000];
			byte[] UncompressedBytes = new byte[0x10000];

			var BinaryWriter = new BinaryWriter(OutputStream);

			using (var LZXState = new LZXState(17))
			{
				var BinaryReader = new BinaryReader(InputStream);

				HeaderStruct HeaderStruct = InputStream.ReadStruct<HeaderStruct>();
				if (!HeaderStruct.Magic.SequenceEqual(Signature)) throw (new Exception("File is not a TOV.LZX"));
				if (HeaderStruct.Magic1 != 0x20000) throw (new Exception("Invalid LZX"));
				if (HeaderStruct.Magic2 != 0x80000) throw (new Exception("Invalid LZX"));
				if (HeaderStruct.Magic3 != 0x00000) throw (new Exception("Invalid LZX"));
				if (HeaderStruct.Magic4 != 0x00000) throw (new Exception("Invalid LZX"));
				if (HeaderStruct.UncompressedSize != HeaderStruct.UncompressedSize2) throw (new Exception("Invalid LZX"));
				if (HeaderStruct.CompressedSize != HeaderStruct.CompressedSize2) throw (new Exception("Invalid LZX"));
				if (HeaderStruct.CompressedSizePlus4 != HeaderStruct.CompressedSize + 4) throw (new Exception("Invalid LZX"));
				if (InputStream.Position != 0x34);

				//Console.WriteLine(InputStream.Length);
				//Console.WriteLine(0x34 + HeaderStruct.CompressedSize);
				if (InputStream.Length < 0x34 + HeaderStruct.CompressedSize) throw(new Exception("Invalid LZX"));

				var DataInputStream = SliceStream.CreateWithLength(InputStream, 0x34, HeaderStruct.CompressedSize);
				var DataBinaryReader = new BinaryReader(DataInputStream);
				DataInputStream.Position = 0;

				while (DataInputStream.Position < DataInputStream.Length)
				{
					ushort UncompressedChunkLength, CompressedChunkLength;

					//Console.WriteLine(DataInputStream.Position + ":" + DataInputStream.Length);

					// Read Chunk's size.
					{
						byte Byte1, Byte2;

						Byte1 = DataBinaryReader.ReadByte();
						if (Byte1 != 0xFF)
						{
							Byte2 = DataBinaryReader.ReadByte();
							UncompressedChunkLength = 0x8000;
							CompressedChunkLength = (ushort)((Byte1 << 8) | Byte2);
						}
						else
						{
							UncompressedChunkLength = DataBinaryReader.ReadUint16Endian(Endianness.BigEndian);
							CompressedChunkLength = DataBinaryReader.ReadUint16Endian(Endianness.BigEndian);
						}
						
						if (CompressedChunkLength >= UncompressedChunkLength)
						{
							//Console.WriteLine("CompressedChunkLength >= UncompressedChunkLength");
							//Console.WriteLine("{0} -> {1}", CompressedChunkLength, UncompressedChunkLength);
						}
					}

					//Console.WriteLine("CHUNK: {0,8:X}, {1,8:X}, {2,8:X}", DataInputStream.Position, CompressedChunkLength, UncompressedChunkLength);

					if (CompressedChunkLength == 0)
					{
						DataInputStream.Seek(-2, SeekOrigin.Current);
						Debug.WriteLine("WARNING: Block with length 0: end!");
						break;
					}

					DataInputStream.Read(CompressedBytes, 0, CompressedChunkLength);

					//Debug.Assert(CompressedChunkLength < UncompressedChunkLength);

					{
						fixed (byte* CompressedBytesPtr = CompressedBytes)
						fixed (byte* UncompressedBytesPtr = UncompressedBytes)
						{
							//Console.WriteLine("{0} -> {1}", CompressedChunkLength, UncompressedChunkLength);
							LZXState.Decompress(CompressedBytesPtr, UncompressedBytesPtr, CompressedChunkLength, UncompressedChunkLength);
						}

						BinaryWriter.Write(UncompressedBytes, 0, UncompressedChunkLength);
					}
				}

				//Console.WriteLine("{0:X}", DataInputStream.Position);
				//Console.WriteLine("{0:X}", DataInputStream.Length);
				//Console.WriteLine("{0:X}", HeaderStruct.FileSizeMinusX30 + 0x30);

				if (DataInputStream.Position != DataInputStream.Length) throw(new Exception("Not readed all the contents"));
				if (!(SliceStream.CreateWithLength(InputStream, 0x34 + HeaderStruct.CompressedSize)).ReadAll().All(Byte => (Byte == 0))) throw(new Exception("?"));
			}
		}

		static public MemoryStream CompressToLZXStreamRAW(String FileToCompress)
		{
			using (Stream StreamToCompress = File.OpenRead(FileToCompress))
			{
				var OutputStream = new MemoryStream();
				var OutputBinaryWriter = new BinaryWriter(OutputStream);

				while (!StreamToCompress.Eof())
				{
					byte[] CompressedBytes;
					long Position = StreamToCompress.Position;
					var UncompressedBytes = StreamToCompress.ReadBytesUpTo(0x8000);

					{
						var MS = new MemoryStream();

						var BW = new BitWriter2(MS, 2, false);

						if (Position == 0)
						{
							BW.WriteBits(1, 0); // Header
							//BW.WriteBits(1, 1); // Header
							//BW.WriteBits(16, 183);
							//BW.WriteBits(16, 6912);
						}

						BW.WriteBits(3, 3); // Bits
						BW.WriteBits(24, UncompressedBytes.Length);
						BW.Align();
						MS.WriteStruct((uint)0);
						MS.WriteStruct((uint)0);
						MS.WriteStruct((uint)0);
						MS.WriteBytes(UncompressedBytes);

						CompressedBytes = MS.ToArray();
					}

					if ((UncompressedBytes.Length != 0x8000))
					{
						OutputBinaryWriter.Write((byte)0xFF);
						OutputBinaryWriter.WriteEndian((ushort)UncompressedBytes.Length, Endianness.BigEndian);
						OutputBinaryWriter.WriteEndian((ushort)CompressedBytes.Length, Endianness.BigEndian);
					}
					else
					{
						OutputBinaryWriter.WriteEndian((ushort)CompressedBytes.Length, Endianness.BigEndian);
					}

					OutputStream.WriteBytes(CompressedBytes);
				}

				OutputStream.Position = 0;
				return OutputStream;
			}
		}

		static public MemoryStream CompressToLZXStream(String FileToCompress, int FallbackCompression = 15)
		{
			using (Stream StreamToCompress = File.OpenRead(FileToCompress))
			{
				var CabFile = CABUtils.CreateCAB(FileToCompress);
				var InputStream = CABUtils.ExtractLZXStreamFromCAB(CabFile);
				File.Delete(CabFile);

				var OutputStream = new MemoryStream();
				var InputBinaryReader = new BinaryReader(InputStream);
				var OutputBinaryWriter = new BinaryWriter(OutputStream);

				StreamToCompress.Position = 0;
				while (InputStream.Position < InputStream.Length)
				{
					uint Hash = InputBinaryReader.ReadUInt32();
					ushort LengthCompressed = InputBinaryReader.ReadUInt16();
					ushort LengthUncompressed = InputBinaryReader.ReadUInt16();
					if (LengthUncompressed > 0x8000) throw (new Exception("Invalid chunk"));
					var CompressedData = InputStream.ReadBytes(LengthCompressed);

					if (LengthCompressed >= LengthUncompressed)
					{
						throw (new InvalidDataException());
					}

					if ((LengthUncompressed != 0x8000))
					{
						OutputBinaryWriter.Write((byte)0xFF);
						OutputBinaryWriter.WriteEndian((ushort)LengthUncompressed, Endianness.BigEndian);
						OutputBinaryWriter.WriteEndian((ushort)LengthCompressed, Endianness.BigEndian);
					}
					else
					{
						OutputBinaryWriter.WriteEndian((ushort)LengthCompressed, Endianness.BigEndian);
					}

					OutputBinaryWriter.Write(CompressedData);
					StreamToCompress.Position += LengthUncompressed;

					if (LengthUncompressed < 0x8000)
					{
						if (InputStream.Position != InputStream.Length) throw (new Exception("(LengthUncompressed < 0x8000)"));
						break;
					}
				}

				OutputStream.Position = 0;

				return OutputStream;
			}
		}

		static public Stream CreateCompression15File(Stream InputStream, int FallbackCompression = 15)
		{
			var OutputStream = new MemoryStream();
			CreateCompression15File(InputStream, OutputStream, FallbackCompression);
			OutputStream.Position = 0;
			return OutputStream;
		}

		static public void CreateCompression15File(Stream InputStream, Stream OutputStream, int FallbackCompression = 15)
		{
			var TempFile = Path.GetTempFileName();
			File.WriteAllBytes(TempFile, InputStream.ReadAll());
			CreateCompression15File(TempFile, OutputStream, FallbackCompression);
		}

		static public void CreateCompression15File(String FileName, Stream OutputStream, int FallbackCompression = 15)
		{
			/*
			if (pState->block_type == LZX_BLOCKTYPE_UNCOMPRESSED)
			{
				if (pState->block_length & 1) inpos++; // realign bitstream to word
				INIT_BITSTREAM;
			}
			*/

			//throw (new NotImplementedException("Can't compress using LZX. Please use compressión 03 instead."));
			Stream LZXStream;
			try
			{
				LZXStream = CompressToLZXStream(FileName, FallbackCompression);
			}
			catch (InvalidDataException)
			{
				if (FallbackCompression == 15)
				{
					LZXStream = CompressToLZXStreamRAW(FileName);
				}
				else
				{
					OutputStream.WriteBytes(TalesCompression.CreateFromVersion(FallbackCompression).EncodeBytes(File.ReadAllBytes(FileName)));
					return;
				}
			}

			uint UncompressedSize = (uint)(new FileInfo(FileName)).Length;
			uint CompressedSize = (uint)LZXStream.Length;
			var HeaderStruct = new TalesCompression15_Lzx.HeaderStruct()
			{
				Magic = TalesCompression15_Lzx.Signature,
				Magic1 = 0x20000,
				Magic2 = 0x80000,
				Magic3 = 0x00000,
				Magic4 = 0x00000,
				UncompressedSize = UncompressedSize,
				UncompressedSize2 = UncompressedSize,
				CompressedSize = CompressedSize,
				CompressedSize2 = CompressedSize,
				CompressedSizePlus4 = CompressedSize + 4,
			};

			OutputStream.WriteStruct(HeaderStruct);
			LZXStream.CopyTo(OutputStream);
			OutputStream.Flush();

			//Console.WriteLine(OutputStream.Position);
			//Console.WriteLine(0x34 + CompressedSize);

			if (OutputStream.Position != 0x34 + CompressedSize) throw (new Exception("(OutputStream.Position != 0x34 + CompressedSize)"));
		}

		public override void EncodeFile(Stream InputStream, Stream OutputStream)
		{
			//Console.WriteLine(this.FallbackVersion);
			CreateCompression15File(InputStream, OutputStream, this.FallbackVersion);
		}
	}
}
