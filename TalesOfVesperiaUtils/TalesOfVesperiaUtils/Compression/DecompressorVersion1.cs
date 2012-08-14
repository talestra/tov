using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TalesOfVesperiaUtils.Compression
{
    public class DecompressorVersion1 : Decompressor1_3
    {
        public override void DecodeChunk(byte[] Input, byte[] Output, out uint InputReadedCount, out uint OutputWrittenCount)
        {
            throw new NotImplementedException();
        }
    }
}
