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
		int RecompressVersion;
		bool RecompressJustWhenModified;

		public DecompressRecompressStream(Stream CompressedStream, int RecompressVersion = -1, bool RecompressJustWhenModified = true)
			: base(TalesCompression.DecompressStream(CompressedStream.Slice()))
		{
			this.CompressedStream = CompressedStream;
			this.RecompressVersion = RecompressVersion;
			this.RecompressJustWhenModified = RecompressJustWhenModified;
			UncompressedOriginalData = ((MemoryStream)this.ParentStream).ToArray();
		}

		[DebuggerHidden]
		public override void Close()
		{
			// Data has changed.
			if (!RecompressJustWhenModified || !UncompressedOriginalData.SequenceEqual(((MemoryStream)this.ParentStream).ToArray()))
			{
				int Version = RecompressVersion;
				if (Version == -1)
				{
					Version = TalesCompression.DetectVersion(this.CompressedStream.Slice());
				}
				var Compression = TalesCompression.CreateFromVersion(Version);
				var RecompressedData = Compression.EncodeFile(ParentStream).ToArray();
				if (RecompressedData.Length > this.CompressedStream.Length) throw (new Exception(String.Format("Compressed file is bigger than original Updated: {0} > Previous: {1}", RecompressedData.Length, CompressedStream.Length)));
				var CompressedStream2 = this.CompressedStream.Slice();
				CompressedStream2
					.WriteBytes(RecompressedData)
					.WriteByteRepeatedTo((byte)0x00, CompressedStream2.Length);
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
