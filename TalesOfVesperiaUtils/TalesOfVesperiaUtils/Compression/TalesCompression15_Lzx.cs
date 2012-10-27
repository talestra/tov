using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TalesOfVesperiaUtils.Compression.LZX;
using TalesOfVesperiaUtils.Compression.CAB;
using CSharpUtils.Endian;

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
				Debug.Assert(HeaderStruct.Magic.SequenceEqual(Signature), "File is not a TOV.LZX");
				Debug.Assert(HeaderStruct.Magic1 == 0x20000);
				Debug.Assert(HeaderStruct.Magic2 == 0x80000);
				Debug.Assert(HeaderStruct.Magic3 == 0x00000);
				Debug.Assert(HeaderStruct.Magic4 == 0x00000);
				Debug.Assert(HeaderStruct.UncompressedSize == HeaderStruct.UncompressedSize2);
				Debug.Assert(HeaderStruct.CompressedSize == HeaderStruct.CompressedSize2);
				Debug.Assert(HeaderStruct.CompressedSizePlus4 == HeaderStruct.CompressedSize + 4);
				Debug.Assert(InputStream.Position == 0x34);

				//Console.WriteLine(InputStream.Length);
				//Console.WriteLine(0x34 + HeaderStruct.CompressedSize);
				Debug.Assert(InputStream.Length >= 0x34 + HeaderStruct.CompressedSize);

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

				Debug.Assert(DataInputStream.Position == DataInputStream.Length);
				Debug.Assert((SliceStream.CreateWithLength(InputStream, 0x34 + HeaderStruct.CompressedSize)).ReadAll().All(Byte => (Byte == 0)));
			}
		}

		static public Stream CompressToLZXStream(String FileToCompress)
		{
			var CabFile = CABUtils.CreateCAB(FileToCompress);
			var InputStream = CABUtils.ExtractLZXStreamFromCAB(CabFile);
			File.Delete(CabFile);

			var OutputStream = new MemoryStream();
			var InputBinaryReader = new BinaryReader(InputStream);
			var OutputBinaryWriter = new BinaryWriter(OutputStream);

			while (InputStream.Position < InputStream.Length)
			{
				uint Hash = InputBinaryReader.ReadUInt32();
				ushort LengthCompressed = InputBinaryReader.ReadUInt16();
				ushort LengthUncompressed = InputBinaryReader.ReadUInt16();
				Debug.Assert(LengthUncompressed <= 0x8000);

				if (LengthUncompressed == 0x8000)
				{
					OutputBinaryWriter.WriteEndian((ushort)LengthCompressed, Endianness.BigEndian);
				}
				else
				{
					OutputBinaryWriter.Write((byte)0xFF);
					OutputBinaryWriter.WriteEndian((ushort)LengthUncompressed, Endianness.BigEndian);
					OutputBinaryWriter.WriteEndian((ushort)LengthCompressed, Endianness.BigEndian);
				}

				OutputBinaryWriter.Write(InputStream.ReadBytes(LengthCompressed));

				if (LengthUncompressed < 0x8000)
				{
					Debug.Assert(InputStream.Position == InputStream.Length);
					break;
				}
			}

			OutputStream.Position = 0;

			return OutputStream;
		}

		static public Stream CreateCompression15File(Stream InputStream)
		{
			var OutputStream = new MemoryStream();
			CreateCompression15File(InputStream, OutputStream);
			OutputStream.Position = 0;
			return OutputStream;
		}

		static public void CreateCompression15File(Stream InputStream, Stream OutputStream)
		{
			var TempFile = Path.GetTempFileName();
			File.WriteAllBytes(TempFile, InputStream.ReadAll());
			CreateCompression15File(TempFile, OutputStream);
		}

		static public void CreateCompression15File(String FileName, Stream OutputStream)
		{
			/*
			if (pState->block_type == LZX_BLOCKTYPE_UNCOMPRESSED)
			{
				if (pState->block_length & 1) inpos++; // realign bitstream to word
				INIT_BITSTREAM;
			}
			*/

			//throw (new NotImplementedException("Can't compress using LZX. Please use compressión 03 instead."));

			Stream LZXStream = CompressToLZXStream(FileName);
			var OutputBinaryWriter = new BinaryWriter(OutputStream);

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
			Debug.Assert(OutputStream.Position == 0x34 + CompressedSize);
		}

		public override void EncodeFile(Stream InputStream, Stream OutputStream)
		{
			CreateCompression15File(InputStream, OutputStream);
		}
	}
}
