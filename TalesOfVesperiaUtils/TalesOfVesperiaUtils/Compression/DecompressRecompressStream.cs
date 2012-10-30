using CSharpUtils.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaUtils.Compression
{
	public class DecompressRecompressStream : ProxyStream
	{
		Stream CompressedStream;

		public DecompressRecompressStream(Stream CompressedStream)
			: base(TalesCompression.DecompressStream(CompressedStream.Slice()))
		{
			this.CompressedStream = CompressedStream;
		}

		public override void Close()
		{
			var Compression = TalesCompression.CreateFromStart(CompressedStream.Slice().ReadBytes(0x10), CompressedStream.Length);
			var Data2 = Compression.EncodeFile(ParentStream).ToArray();
			if (Data2.Length > CompressedStream.Length) throw (new Exception(String.Format("Compressed file is bigger than original {0} > {1}", CompressedStream.Length, Data2.Length)));
			CompressedStream.Slice().WriteBytes(Data2);
			CompressedStream.WriteByteRepeated((byte)0x00, (int)(CompressedStream.Length - Data2.Length));
			base.Close();
		}
	}
}
