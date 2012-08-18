using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaUtils.Compression
{
	/// <summary>
	/// Uncompressed.
	/// </summary>
	public class TalesCompression0 : TalesCompression
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="InputStream"></param>
		/// <param name="OutputStream"></param>
		public override void EncodeFile(Stream InputStream, Stream OutputStream)
		{
			var Header = new TalesCompression1_3.HeaderStruct()
			{
				Version = 0,
				CompressedLength = (uint)InputStream.Length,
				UncompressedLength = (uint)OutputStream.Length,
			};

			OutputStream.WriteStruct(Header);
			InputStream.CopyToFast(OutputStream);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InputStream"></param>
		/// <param name="OutputStream"></param>
		public override void DecodeFile(Stream InputStream, Stream OutputStream)
		{
			var Header = InputStream.ReadStruct<TalesCompression1_3.HeaderStruct>();
			InputStream.ReadStream(Header.CompressedLength).CopyToFast(OutputStream);
		}
	}
}
