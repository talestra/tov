using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using CSharpUtils.Endian;
using CSharpUtils;
using CSharpUtils.Drawing;
using TalesOfVesperiaUtils.Imaging.Internal;

namespace TalesOfVesperiaUtils.Imaging
{
	/// <summary>
	/// 
	/// </summary>
	unsafe public class DXT5 : DXT<DXT5.AlphaColorBlock>
	{
		protected override void EncodeBlock(ref DXT5.AlphaColorBlock Block, ref ARGB_Rev[] Colors, CompressDXT.CompressionMode CompressionMode)
		{
			CompressDXT.CompressBlockDXT5(Colors, out Block, CompressionMode);
		}

		protected override void DecodeBlock(ref DXT5.AlphaColorBlock Block, ref ARGB_Rev[] Colors)
		{
			Block.Decode(ref Colors);
		}

		unsafe public struct AlphaColorBlock
		{
			public DXT5A.AlphaBlock alphaBlock;
			public DXT1.ColorBlock colorBlock;

			public void EncodeSimpleUnoptimizedWhiteAlpha(ARGB_Rev[] input)
			{
				alphaBlock.alpha = 0x00FF;
				colorBlock.colors1 = colorBlock.colors0 = Color.White.Encode565();
				var lookup = new byte[] { 1, 7, 6, 5, 4, 3, 2, 0 };
				var alpha_data_transfer = new byte[16];
				int k = 0;
				foreach (var color in input)
				{
					int e = ((color.A * 7) / 255);
					alpha_data_transfer[k] = lookup[e];
					k++;
				}
				alphaBlock.transferAlphaData(ref alpha_data_transfer, extract: false);
			}

			public void Decode(ref ARGB_Rev[] output)
			{
				colorBlock.Decode(ref output);
				alphaBlock.Decode(ref output);
			}
		}
	}
}
