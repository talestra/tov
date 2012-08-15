using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils;
using System.IO;
using TalesOfVesperiaUtils.Compression.C;
using CSharpUtils.Endian;

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
			if (OutputMaxLength == -1) OutputMaxLength = OutputMaxLength * 4;

			byte[] RealOutput;
			byte[] Output = new byte[OutputMaxLength];
			int OutputLength = OutputMaxLength;
			fixed (byte* InputPtr = Input)
			fixed (byte* OutputPtr = Output)
			{
				var Result = (Result)complib.Decode(Version, InputPtr, Input.Length, OutputPtr, ref OutputLength);
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
			var InputBuffer = InputStream.ReadBytes((int)(uint)Header.CompressedLength);
			var OutputBuffer = Decode(InputBuffer, Version, (int)(Header.UncompressedLength + 0x10000));
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
