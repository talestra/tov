using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using TalesOfVesperiaUtils.Compression.C;

namespace TalesOfVesperiaUtils.Compression.LZX
{
	/// <see cref="http://www.cabextract.org.uk/libmspack/"/>
	/// <see cref="http://msdn.microsoft.com/en-us/library/bb267310.aspx"/>
	unsafe public class LZXState : IDisposable
	{
		public enum Result : int
		{
			DECR_OK = 0,  // OK
			DECR_DATAFORMAT = 1,  // BAD FORMAT
			DECR_ILLEGALDATA = 2,  // ILLEGAL DATA
			DECR_NOMEMORY = 3,  // NO MEMORY
		}

		/*
		[DllImport("TalesOfVesperiaCompression.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr LZXinit(int window);

		[DllImport("TalesOfVesperiaCompression.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void LZXteardown(IntPtr pState);

		[DllImport("TalesOfVesperiaCompression.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int LZXreset(IntPtr pState);

		[DllImport("TalesOfVesperiaCompression.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern Result LZXdecompress(IntPtr pState, byte* inpos, byte* outpos, int inlen, int outlen);
		*/

		lzx_vesp.LZXstate pState;

		public LZXState(int window)
		{
			pState = lzx_vesp.LZXinit(window);
		}

		public int Reset()
		{
			return lzx_vesp.LZXreset(pState);
		}

		public void Decompress(byte* inpos, byte* outpos, int inlen, int outlen)
		{
			var LzxResult = (Result)lzx_vesp.LZXdecompress(pState, inpos, outpos, inlen, outlen);
			if (LzxResult != Result.DECR_OK) throw (new Exception(String.Format("LZXdecompress: {0}, inlen:{1}, outlen:{2}", LzxResult, inlen, outlen)));
		}

		~LZXState()
		{
			Dispose();
		}

		public void Dispose()
		{
			lzx_vesp.LZXteardown(pState);
			/*
			if (pState != IntPtr.Zero)
			{
				lzx_vesp.LZXteardown(pState);
				pState = IntPtr.Zero;
			}
			*/
		}
	}
}
