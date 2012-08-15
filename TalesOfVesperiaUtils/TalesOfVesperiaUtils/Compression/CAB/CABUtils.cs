using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using CSharpUtils;

namespace TalesOfVesperiaUtils.Compression.CAB
{
	/// <summary>
	/// 
	/// </summary>
	/// <see cref="http://www.codeproject.com/KB/dotnet/Cdecl_CSharp_VB.aspx"/>
	unsafe public class CABUtils
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct HeaderStruct
		{
			/// <summary>
			/// cabinet file signature
			/// 
			/// Contains the characters 'M','S','C','F' (bytes 0x4D, 0x53, 0x43, 0x46).  This field is used to assure that the file is a cabinet file.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public byte[] Signature;

			/// <summary>
			/// reserved
			/// 
			/// Reserved field, set to zero.
			/// </summary>
			public uint Reserved1;

			/// <summary>
			/// size of this cabinet file in bytes
			/// 
			/// 
			/// </summary>
			public uint cbCabinet;
			public uint Reserved2;       // reserved
			public uint CoffFiles;       // offset of the first CFFILE entry
			public uint Reserved3;       // reserved
			public byte VersionMinor;    // cabinet file format version, minor
			public byte VersionMajor;    // cabinet file format version, major
			public ushort cFolders;      // number of CFFOLDER entries in this cabinet
			public ushort cFiles;        // number of CFFILE entries in this cabinet

			/// <summary>
			/// Bit-mapped values which indicate the presence of optional data:
			/// 
			/// #define cfhdrPREV_CABINET       0x0001
			/// #define cfhdrNEXT_CABINET       0x0002
			/// #define cfhdrRESERVE_PRESENT    0x0004
			/// 
			/// flags.cfhdrPREV_CABINET is set if this cabinet file is not the first in a set of cabinet files.  When this bit is set, the szCabinetPrev and szDiskPrev fields are present in this CFHEADER.
			/// 
			/// flags.cfhdrNEXT_CABINET is set if this cabinet file is not the last in a set of cabinet files.  When this bit is set, the szCabinetNext and szDiskNext fields are present in this CFHEADER.
			/// 
			/// flags.cfhdrRESERVE_PRESENT is set if this cabinet file contains any reserved fields.  When this bit is set, the cbCFHeader, cbCFFolder, and cbCFData fields are present in this CFHEADER.
			/// 
			/// Other bit positions in the flags field are reserved. 
			/// </summary>
			public ushort Flags;         // cabinet file option indicators
			public ushort SetID;         // must be the same for all cabinets in a set
			public ushort iCabinet;      // number of this cabinet file in a set
			/*
			public ushort cbCFHeader;    // (optional) size of per-cabinet reserved area
			public byte cbCFFolder;      // (optional) size of per-folder reserved area
			public byte cbCFData;        // (optional) size of per-datablock reserved area
			public byte abReserve;       // (optional) per-cabinet reserved area
			public byte[] szCabinetPrev; // (optional) name of previous cabinet file
			public byte[] szDiskPrev;    // (optional) name of previous disk
			public byte[] szCabinetNext; // (optional) name of next cabinet file
			public byte[] szDiskNext;    // (optional) name of next disk
			*/
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct FileEntryStruct
		{
			/// <summary>
			/// uncompressed size of this file in bytes
			/// </summary>
			public uint cbFile;

			/// <summary>
			/// uncompressed offset of this file in the folder
			/// </summary>
			public uint uoffFolderStart;

			/// <summary>
			/// index into the CFFOLDER area
			/// </summary>
			public ushort iFolder;

			/// <summary>
			/// date stamp for this file
			/// </summary>
			public ushort date;

			/// <summary>
			/// time stamp for this file
			/// </summary>
			public ushort time;

			/// <summary>
			/// attribute flags for this file
			/// </summary>
			public ushort attribs;

			/// <summary>
			/// name of this file
			/// </summary>
			//[MarshalAs(UnmanagedType.LPStr)]
			//[MarshalAs(UnmanagedType.str)]
			//public string szName;
		}

		public static int ExecuteCommand(string Command)
		{
			int ExitCode;
			ProcessStartInfo ProcessInfo;
			Process Process;

			ProcessInfo = new ProcessStartInfo("cmd.exe", "/C " + Command);
			ProcessInfo.CreateNoWindow = true;
			ProcessInfo.UseShellExecute = false;
			Process = Process.Start(ProcessInfo);
			//Process.WaitForExit(Timeout);
			Process.WaitForExit();
			ExitCode = Process.ExitCode;
			Process.Close();

			return ExitCode;
		}

		public static String QuoteArgument(String Argument)
		{
			return '"' + Argument.Replace("\\", "\\\\").Replace("\"", "\\\"") + '"';
		}

		static public String CreateCAB(String FileToAdd, String CabFile = null)
		{
			if (CabFile == null) CabFile = Path.GetTempFileName();

			if (!File.Exists(Environment.SystemDirectory + @"\MAKECAB.EXE")) throw(new FileNotFoundException("Can't find MAKECAB.EXE"));

			ExecuteCommand(
				String.Format(
					Environment.SystemDirectory + @"\MAKECAB.EXE /D CompressionMemory=17 /D CompressionType=LZX {0} {1}",
					//@"C:\projects\svn.tales-tra.com\csharp\TalesOfVesperiaUtils\ModifiedMakeCab\MAKECAB_TOV.EXE /D CompressionMemory=17 /D CompressionType=LZX {0} {1}",
					QuoteArgument(FileToAdd),
					QuoteArgument(CabFile)
				)
			);

			return CabFile;
		}

		static public Stream ExtractLZXStreamFromCAB(String CabFile)
		{
			var Stream = File.OpenRead(CabFile);
			var HeaderStruct = Stream.ReadStruct<HeaderStruct>();
			Debug.Assert(Encoding.ASCII.GetString(HeaderStruct.Signature) == "MSCF");
			Debug.Assert(HeaderStruct.cFiles == 1);

			Stream.Position = HeaderStruct.CoffFiles;
			var FileEntryStruct = Stream.ReadStruct<FileEntryStruct>();
			var szName = Stream.ReadStringz();

			var CompressedBytes = Stream.Length - Stream.Position;

			Stream.Position = Stream.Position + FileEntryStruct.uoffFolderStart;
			//var MemoryStream = new MemoryStream(Stream.ReadBytes((int)FileEntryStruct.cbFile));
			var MemoryStream = new MemoryStream(Stream.ReadBytes((int)CompressedBytes));

			Stream.Close();

			return MemoryStream;
		}
	}
}
