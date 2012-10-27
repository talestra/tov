//#define DEBUG_COMPRESSION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils;
using System.IO;
using TalesOfVesperiaUtils.Compression.C;
using CSharpUtils.Endian;
using CSharpUtils.Ext.Compression.Lz;

namespace TalesOfVesperiaUtils.Compression
{
	// Tales of Vesperia - PAL @ default.xex
	// .text:820D5448 decompress_generic:                     # CODE XREF: sub_821C70C8+94p
	// .text:820D5098 compression_01:                         # CODE XREF: sub_820D5448+154p
	// .text:820D5220 compression_03:                         # CODE XREF: sub_820D5448+130p
	unsafe public class TalesCompression1_3 : TalesCompression
	{
		protected int Version;

		private static byte[] GeneratePrependData(int Length = 0x1000)
		{
			return WritePrependData(new byte[Length], Length);
		}

		private static byte[] WritePrependData(byte[] Data, int Length = 0x1000)
		{
			int p = 0;
			for (int n = 0; n != 0x100; n++, p += 8)
			{
				Data[p + 6] = Data[p + 4] = Data[p + 2] = Data[p + 0] = (byte)n;
				Data[p + 7] = Data[p + 5] = Data[p + 3] = Data[p + 1] = 0;
			}
			for (int n = 0; n != 0x100; n++, p += 7)
			{
				Data[p + 6] = Data[p + 4] = Data[p + 2] = Data[p + 0] = (byte)n;
				Data[p + 5] = Data[p + 3] = Data[p + 1] = 0xff;
			}
			while (p < Length) Data[p++] = 0;
			return Data;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="OutputMaxSize"></param>
		/// <param name="Version"></param>
		/// <returns></returns>
		public static byte[] Decode13(byte[] Input, int OutputMaxSize, int Version)
		{
			if ((Version != 1) && (Version != 3)) throw(new InvalidOperationException("Can't handle version '" + Version + "'"));

			var Output = new byte[OutputMaxSize + 0x1000];
			bool HasRle = (Version == 3);
			const int WindowSize = 0x1000;
			var MinLength = 3;
			int MaxLength = HasRle ? 0x11 : 0x12;
			var OffsetStart = WindowSize - MaxLength;
			WritePrependData(Output, OffsetStart);

#if DEBUG_COMPRESSION
			Console.WriteLine("Decoding at 0x{0:X8}", OffsetStart);
#endif

			//File.WriteAllBytes("c:/temp/lol.bin", Input);

#if false
			Console.WriteLine("");
			Console.WriteLine("START: 0x{0:X3}", OffsetStart);
#endif

			fixed (byte* InStart = &Input[0])
			fixed (byte* OutStart = &Output[OffsetStart])
			{
				byte* InCurrent = InStart, InEnd = InStart + Input.Length;
				byte* OutCurrent = OutStart, OutEnd = OutStart + OutputMaxSize;
				
				uint Data = 0x001;
				while (InCurrent < InEnd)
				{
					if (Data == 0x001)
					{
						Data = (uint)(*InCurrent++ | 0x100);
#if DEBUG_COMPRESSION
						Console.WriteLine("ControlByte: 0x{0:X2}", (byte)Data);
#endif
					}
					bool Uncompressed = ((Data & 1) != 0); Data >>= 1;
					
					// UNCOMPRESSED
					if (Uncompressed)
					{
#if DEBUG_COMPRESSION
						Console.WriteLine("{0:X8}: BYTE(0x{1:X2})", (OutCurrent - OutStart) + OffsetStart, *InCurrent);
#endif
						*OutCurrent++ = *InCurrent++;
					}
					// COMPRESSED
					else
					{
						int Byte1 = *InCurrent++;
						int Byte2 = *InCurrent++;
						var WindowOffset = Byte1 | ((Byte2 & 0xF0) << 4);
						var Length = (Byte2 & 0x0F) + MinLength;

						// RLE
						if (HasRle && Length > MaxLength)
						{
							int RleLength;
							byte RleByte;
							/*
							if ((WindowOffset >> 8) == 0)
							{
								RleByte = (byte)(*InCurrent++);
								RleLength = (WindowOffset & 0xFF) + MaxLength + 1 + 1;
							}
							else
							{
								RleByte = (byte)(WindowOffset & 0xFF);
								RleLength = (WindowOffset >> 8) + MinLength;
							}
							*/

							int Type;
							if (WindowOffset < 0x100)
							{
								Type = 1;
								RleByte = *InCurrent++;
								RleLength = WindowOffset + MaxLength + 1 + 1;
							}
							else
							{
								Type = 0;
								RleByte = (byte)WindowOffset;
								RleLength = (WindowOffset >> 8) + MinLength;
							}

#if DEBUG_COMPRESSION
							Console.WriteLine("{0:X8}: RLE(Byte: 0x{1:X2}, Length: {2}, Type: {3})", (OutCurrent - OutStart) + OffsetStart, RleByte, RleLength, Type);
#endif

							PointerUtils.Memset(OutCurrent, RleByte, RleLength);
							OutCurrent += RleLength;
						}
						// LZ
						else
						{
							//int CurrentWindowPos = (OffsetStart + (int)(OutCurrent - OutStart)) % WindowSize;
							//int MinusDisp = (CurrentWindowPos - WindowOffset + WindowSize) % WindowSize;

							int MinusDisp = ((OffsetStart + (int)(OutCurrent - OutStart)) - WindowOffset + WindowSize) % WindowSize;
							if (MinusDisp == 0) MinusDisp = WindowSize;

#if false
							Console.WriteLine(
								"LZ(0x{0:X3}, {1}) : CUR:0x{2:X3} : MIN:{3}",
								WindowOffset, Length, CurrentWindowPos , - MinusDisp
							);
#endif

							//SourcePointer = (OutCurrent - WindowSize) + (WindowOffset + Offset) % WindowSize;
							PointerUtils.Memcpy(OutCurrent, (OutCurrent - MinusDisp), Length);
		
#if DEBUG_COMPRESSION
							Console.WriteLine("{0:X8}: LZ(Offset: {1}, Length: {2})", (OutCurrent - OutStart) + OffsetStart, -MinusDisp, Length);
							for (int n = 0; n < Length; n++)
							{
								Console.WriteLine("  {0:X8}: 0x{1:X2}", (OutCurrent - OutStart) + OffsetStart - MinusDisp + n, OutCurrent[-MinusDisp + n]);
							}
#endif

							OutCurrent += Length;
						}
					}
				}
				return PointerUtils.PointerToByteArray(OutStart, (int)(OutCurrent - OutStart));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="Version"></param>
		/// <param name="OutputMaxLength"></param>
		/// <returns></returns>
		public static byte[] Encode2(byte[] Input, int Version = 1, int OutputMaxLength = -1)
		{
			byte[] Data;

			int MinLzLength;
			int MaxLzLength;
			int MaxLzDistance;
			int MinRleLength;
			int MaxRleLength;
			int WindowSize = 0x1000;

			MinLzLength = 3;
			MaxLzDistance = WindowSize;

			switch (Version)
			{
				case 1:
					MaxLzLength = 0x12;
					MinRleLength = 0;
					MaxRleLength = 0;
					break;
				case 3:
					MaxLzLength = 0x11;
					MinRleLength = MinLzLength + 1;
					//MinRleLength = MaxLzLength + 1 + 1;
					MaxRleLength = 0xFF + MaxLzLength + 1 + 1;
					break;
				default: throw (new Exception(String.Format("Can't handle version {0}", Version)));
			}


			int StartPosition = WindowSize - MaxLzLength;

			Data = GeneratePrependData(StartPosition).Concat(Input);

			var Out = new MemoryStream();
			//Out.WriteStruct((byte)Version);
			//Out.WriteStruct((int)0);
			//Out.WriteStruct((int)0);

			var Buffer = new List<String>();

			int ControlByte = 0x00;
			int ControlCount = 0;
			var Set = new MemoryStream();

			Action<bool> Put = (bool Uncompressed) => {
				ControlByte >>= 1;
				ControlByte |= Uncompressed ? 0x80 : 0x00;
				ControlCount++;
				if (ControlCount == 8)
				{
#if DEBUG_COMPRESSION
					Console.WriteLine("ControlByte: 0x{0:X2}", (byte)ControlByte);
					foreach (var Item in Buffer) Console.WriteLine(Item);
#endif
					Out.WriteByte((byte)ControlByte);
					Out.WriteBytes(Set.ToArray());
					Set = new MemoryStream();

					Buffer.Clear();
					//Set.Position = 0;
					//Set.SetLength(0);
					ControlByte = 0x00;
					ControlCount = 0;
				}
			};

#if DEBUG_COMPRESSION
			Console.WriteLine("Encoding at 0x{0:X8}", StartPosition);
#endif

			//File.WriteAllBytes(@"c:\temp\TEST_ENC", Data);

			Matcher.HandleLzRle(Data, StartPosition, MinLzLength, MaxLzLength, MaxLzDistance, MinRleLength, MaxRleLength, true,
				// Uncompressed
				(Position, Byte) =>
				{
#if DEBUG_COMPRESSION
					Buffer.Add(String.Format("{0:X8}: BYTE(0x{1:X2})", Position, Byte));
#endif
					Set.WriteByte(Byte);
					Put(true);
				},
				// LZ
				(Position, Offset, Length) =>
				{
#if DEBUG_COMPRESSION
					Buffer.Add(String.Format("{0:X8}: LZ(Offset: {1}, Length: {2})", Position, Offset, Length));
					for (int n = 0; n < Length; n++)
					{
						Buffer.Add(String.Format("  {0:X8}: 0x{1:X2}", Position + Offset + n, Data[Position + Offset + n]));
					}
#endif
					int WritePos = 0;
					int WriteLen = Length - MinLzLength;
					
					//Console.WriteLine(WriteLen);
					int CurrentWindowPos = (0xFEE + Position) % WindowSize;
					WritePos = (CurrentWindowPos + Offset + WindowSize + 0x12) % WindowSize;
					//PointerUtils.Memcpy(OutCurrent, (OutCurrent - MinusDisp), Length);

					/*
					int Byte1 = *InCurrent++;
					int Byte2 = *InCurrent++;
					var WindowOffset = Byte1 | ((Byte2 & 0xF0) << 4);
					var Length = (Byte2 & 0x0F) + MinLength;
					*/

					Set.WriteByte((byte)(WritePos & 0x0FF));
					Set.WriteByte((byte)((((WritePos & 0xF00) >> 8) << 4) | (WriteLen & 0xF)));
					Put(false);
				},
				// RLE
				(Position, Byte, Length) =>
				{
					/*
					if ((WindowOffset >> 8) == 0)
					{
						RleLength = (WindowOffset & 0xFF) + MaxLength + 1 + 1;
						RleByte = (byte)(*InCurrent++);
					}
					else
					{
						RleLength = (WindowOffset >> 8) + MinLength;
						RleByte = (byte)(WindowOffset & 0xFF);
					}
					*/

					if (Length <= MinLzLength) throw(new Exception("Invalid RLE Length"));

					int Type = 0;

					if (Length < MaxLzLength + 1 + 1)
					{
						//throw(new NotImplementedException());
						int WriteLen = 0xF;
						int WritePos = (int)(((int)Byte & 0xFF) | (int)(((Length - MinLzLength) & 0xF) << 8));

						Set.WriteByte((byte)(WritePos & 0x0FF));
						Set.WriteByte((byte)((((WritePos & 0xF00) >> 8) << 4) | (WriteLen & 0xF)));
						Put(false);
						Type = 0;
					}
					else
					{
						int WriteLen = 0xF;
						int WritePos = (Length - (MaxLzLength + 1 + 1)) & 0xFF;

						//Buffer.Add(String.Format("Length: {0}", Length));
						//Buffer.Add(String.Format("MaxLzLength: {0}", MaxLzLength));
						//Buffer.Add(String.Format("WritePos: {0}", WritePos));

						Set.WriteByte((byte)(WritePos & 0x0FF));
						Set.WriteByte((byte)((((WritePos & 0xF00) >> 8) << 4) | (WriteLen & 0xF)));
						Set.WriteByte(Byte);
						Put(false);
						Type = 1;
					}

#if DEBUG_COMPRESSION
					Buffer.Add(String.Format("{0:X8}: RLE(Byte: 0x{1:X2}, Length: {2}, Type: {3})", Position, Byte, Length, Type));
#endif
				}
			);

			while (ControlCount > 0) Put(false);

			//throw(new NotImplementedException());
			//ERRROR_CONTINUE_HERE_TOMORROW!!

			return Out.ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="Version"></param>
		/// <param name="OutputMaxLength"></param>
		/// <returns></returns>
		public static byte[] Encode(byte[] Input, int Version = 1, int OutputMaxLength = -1)
		{
			if (OutputMaxLength == -1) OutputMaxLength = ((Input.Length * 9) / 8) + 1;

			return Encode2(Input, Version, OutputMaxLength);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="Version"></param>
		/// <param name="OutputMaxLength"></param>
		/// <returns></returns>
		public static byte[] Decode(byte[] Input, int Version = 1, int OutputMaxLength = -1)
		{
			if (OutputMaxLength == -1) OutputMaxLength = Input.Length * 16;

			return Decode13(Input, OutputMaxLength, Version);
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		public struct HeaderStruct
		{
			/// <summary>
			/// 
			/// </summary>
			public byte Version;

			/// <summary>
			/// 
			/// </summary>
			public uint CompressedLength;

			/// <summary>
			/// 
			/// </summary>
			public uint UncompressedLength;
		}

		public TalesCompression1_3(int Version)
		{
			this.Version = Version;
		}

		public override void DecodeFile(Stream InputStream, Stream OutputStream)
		{
			var Header = InputStream.ReadStruct<HeaderStruct>();
			if (Header.Version != this.Version) throw(new Exception(String.Format("Version mismatch {0} != {1}", Header.Version, this.Version)));
			var InputBuffer = InputStream.ReadBytes((int)Header.CompressedLength);
			var OutputBuffer = Decode(InputBuffer, Version, (int)Header.UncompressedLength);
			//Decode(InputBuffer, OutputBuffer, out InputReadedCount, out OutputWrittenCount);
			if (OutputBuffer.Length != OutputBuffer.Length) throw (new Exception("Not written to all the output buffer"));
			OutputStream.WriteBytes(OutputBuffer);
		}

		public override void EncodeFile(Stream InputStream, Stream OutputStream)
		{
			byte[] UncompressedBytes = InputStream.ReadAll();
			byte[] CompressedBytes = Encode2(UncompressedBytes, this.Version);

			OutputStream.WriteStruct(new HeaderStruct() {
				CompressedLength = (uint)CompressedBytes.Length,
				UncompressedLength = (uint)UncompressedBytes.Length,
				Version = (byte)this.Version
			});
			OutputStream.WriteBytes(CompressedBytes);
			OutputStream.Flush();
			OutputStream.Position = 0;
		}
	}
}
