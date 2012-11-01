using CSharpUtils.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaUtils.Compression
{
	public class DecompressRecompressStream : ProxyStream
	{
		byte[] UncompressedOriginalData;
		Stream CompressedStream;

		public DecompressRecompressStream(Stream CompressedStream)
			: base(TalesCompression.DecompressStream(CompressedStream.Slice()))
		{
			this.CompressedStream = CompressedStream;
			UncompressedOriginalData = ((MemoryStream)this.ParentStream).ToArray();
		}

		public override void Close()
		{
			var UncompressedNewData = ((MemoryStream)this.ParentStream).ToArray();

			// Data has changed.
			if (!UncompressedOriginalData.SequenceEqual(UncompressedNewData))
			//if (true)
			{
				var Compression = TalesCompression.CreateFromStart(this.CompressedStream.Slice().ReadBytes(0x10), this.CompressedStream.Length);
				var RecompressedData = Compression.EncodeFile(ParentStream).ToArray();
				if (RecompressedData.Length > this.CompressedStream.Length) throw (new Exception(String.Format("Compressed file is bigger than original {0} > {1}", CompressedStream.Length, RecompressedData.Length)));
				var CompressedStream2 = this.CompressedStream.Slice();
				CompressedStream2
					.WriteBytes(RecompressedData)
					.WriteByteRepeated((byte)0x00, (int)(this.CompressedStream.Length - RecompressedData.Length))
				;
			}
			else
			{
				//Console.WriteLine("Unchanged!");
			}

			base.Close();
		}
	}
}
