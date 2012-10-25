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

		public enum Result : int
		{
			SUCCESS = 0,
			ERROR_FILE_IN = -1,
			ERROR_FILE_OUT = -2,
			ERROR_MALLOC = -3,
			ERROR_BAD_INPUT = -4,
			ERROR_UNKNOWN_VERSION = -5,
			ERROR_FILES_MISMATCH = -6,
		}

		/*
		[DllImport("TalesOfVesperiaCompression.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern Result Encode(int Version, byte* _in, int inl, byte* _out, ref int outl);

		[DllImport("TalesOfVesperiaCompression.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern Result Decode(int Version, byte* _in, int inl, byte* _out, ref int outl);
		*/

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

		public static byte[] Decode13(byte[] Input, int OutputMaxSize, int Version)
		{
			if ((Version != 1) && (Version != 3)) throw(new InvalidOperationException("Can't handle version '" + Version + "'"));

			var Output = new byte[OutputMaxSize + 0x1000];
			bool HasRle = (Version == 3);
			const int WindowSize = 0x1000;
			var MinLength = 2;
			int MaxLength = HasRle ? 0x11 : 0x12;


			var OffsetStart = WindowSize - MaxLength;
			WritePrependData(Output);
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
					if (Data == 0x001) Data = (uint)(*InCurrent++ | 0x100);
					bool Uncompressed = ((Data & 1) != 0); Data >>= 1;
					
					// UNCOMPRESSED
					if (Uncompressed)
					{
#if false
						Console.WriteLine("BYTE(0x{0:X2})", *InCurrent);
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
						if (HasRle && Length >= MaxLength)
						{
							int RleLength;
							byte RleByte;
							if ((WindowOffset >> 8) == 0)
							{
								RleLength = (WindowOffset & 0xFF) + MaxLength + 1 + 1;
								RleByte = (byte)(*InCurrent++);
							}
							else
							{
								RleLength = (WindowOffset >> 8) + MinLength + 1;
								RleByte = (byte)(WindowOffset & 0xFF);
							}

#if false
							Console.WriteLine("RLE(0x{0:X2}, {1})", RleByte, RleLength);
#endif

							PointerUtils.Memset(OutCurrent, RleByte, RleLength);
							OutCurrent += RleLength;
						}
						// LZ
						else
						{
							Length++;
							byte* SourcePointer;
							int CurrentWindowPos = (OffsetStart + (int)(OutCurrent - OutStart)) % WindowSize;
							int MinusDisp; 
							if (CurrentWindowPos < WindowOffset)
							{
								MinusDisp = (CurrentWindowPos + (WindowSize - WindowOffset));
							}
							else
							{
								MinusDisp = (CurrentWindowPos - WindowOffset);
							}

#if false
							Console.WriteLine(
								"LZ(0x{0:X3}, {1}) : CUR:0x{2:X3} : MIN:{3}",
								WindowOffset, Length, CurrentWindowPos , - MinusDisp
							);
#endif

							SourcePointer = OutCurrent - MinusDisp;
							//SourcePointer = (OutCurrent - WindowSize) + (WindowOffset + Offset) % WindowSize;
							PointerUtils.Memcpy(OutCurrent, SourcePointer, Length);
							OutCurrent += Length;
						}
					}
				}
				return PointerUtils.PointerToByteArray(OutStart, (int)(OutCurrent - OutStart));
			}
		}

		public static byte[] Encode2(byte[] Input, int Version = 1, int OutputMaxLength = -1)
		{
			byte[] Data;

			int MinLzLength;
			int MaxLzLength;
			int MaxLzDistance;
			int MinRleLength;
			int MaxRleLength;
			int WindowSize = 0x1000;
			int StartPosition = WindowSize;

			Data = GeneratePrependData().Concat(Input);

			switch (Version)
			{
				case 1:
					MinLzLength = 2;
					MaxLzLength = 0x12;
					MaxLzDistance = WindowSize;
					MinRleLength = 0;
					MaxRleLength = 0;
					break;
				/*
				case 3:
					MinLzLength = 2;
					MaxLzLength = 0x11;
				 * MaxLzDistance = WindowSize;
					break;
				*/
				default: throw (new Exception(String.Format("Can't handle version {0}", Version)));
			}

			var Out = new MemoryStream();
			Out.WriteStruct((byte)Version);
			Out.WriteStruct((int)0);
			Out.WriteStruct((int)0);

			int ControlByte = 0x01;
			var Set = new MemoryStream();
			Action<bool> Put = (bool Uncompressed) => {
				ControlByte |= Uncompressed ? 1 : 0;
				ControlByte <<= 1;
				if (ControlByte == 0x100)
				{
					Out.WriteByte((byte)ControlByte);
					Out.WriteBytes(Set.ToArray());
					Set.Position = 0;
					Set.SetLength(0);
					ControlByte = 0x01;
				}
			};

			Matcher.HandleLzRle(Data, StartPosition, MinLzLength, MaxLzLength, MaxLzDistance, MinRleLength, MaxRleLength, true,
				// Uncompressed
				(Position, Byte) =>
				{
					Set.WriteByte(Byte);
					Put(true);
				},
				// LZ
				(Position, Offset, Length) =>
				{
					Put(false);
				},
				// RLE
				(Position, Byte, Length) =>
				{
					Set.WriteByte(Byte);
					Put(true);
					
					Set.WriteByte(Byte);
					Put(false);
				}
			);

			throw(new NotImplementedException());
			//ERRROR_CONTINUE_HERE_TOMORROW!!

			return Out.ToArray();
		}

		public static byte[] Encode(byte[] Input, int Version = 1, int OutputMaxLength = -1)
		{
			if (OutputMaxLength == -1) OutputMaxLength = ((Input.Length * 9) / 8) + 1;

			byte[] RealOutput;
			byte[] Output = new byte[OutputMaxLength];
			int OutputLength = OutputMaxLength;
			fixed (byte* InputPtr = Input)
			fixed (byte* OutputPtr = Output)
			{
				var Result = (Result)complib.Encode(Version, InputPtr, Input.Length, OutputPtr, ref OutputLength);
				if (Result != Result.SUCCESS)
				{
					throw (new Exception(Result.ToString()));
				}
				RealOutput = new byte[OutputLength];
				Array.Copy(Output, RealOutput, OutputLength);
			}
			return RealOutput;
			//Encode(version, Marshal);
		}

		public static byte[] Decode(byte[] Input, int Version = 1, int OutputMaxLength = -1)
		{
			if (OutputMaxLength == -1) OutputMaxLength = Input.Length * 16;

			byte[] Output = new byte[OutputMaxLength + 0x1000];
			int OutputLength = OutputMaxLength;

			if (true)
			{
				return Decode13(Input, OutputMaxLength, Version);
			}
			else
			{
				fixed (byte* InputPtr = Input)
				fixed (byte* OutputPtr = Output)
				{
					var Result = (Result)complib.Decode(Version, InputPtr, Input.Length, OutputPtr, ref OutputLength);
					if (Result != Result.SUCCESS)
					{
						throw (new Exception(Result.ToString()));
					}
				}
			}
			return Output.Slice(0, OutputLength);
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
			byte[] CompressedBytes = Encode(UncompressedBytes, this.Version);

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
