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

		unsafe public struct AlphaBlock
		{
			public ushort_be alpha;
			public ushort_be alpha_data0, alpha_data1, alpha_data2;

			internal void transferAlphaData(ref byte[] alpha_data_transfer, bool extract)
			{
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 0, 0, 0, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 0, 1, 3, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 0, 2, 6, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 0, 3, 9, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 0, 4, 12, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 0, 5, 15, 1, 0); exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 1, 5, 0, 2, 1);

				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 1, 6, 2, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 1, 7, 5, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 1, 8, 8, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 1, 9, 11, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 1, 10, 14, 2, 0); exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 2, 10, 0, 1, 2);

				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 2, 11, 1, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 2, 12, 4, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 2, 13, 7, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 2, 14, 10, 3, 0);
				exs_ins2(alpha_data_transfer, ref alpha_data0, extract, 2, 15, 13, 3, 0);
			}

			internal void Decode(ref ARGB_Rev[] output)
			{
				var alpha_table = new byte[8];

				// Alpha table.
				byte alpha_0 = alpha.Low;
				byte alpha_1 = alpha.High;

				if (alpha_0 > alpha_1)
				{
					alpha_table[0] = alpha_0;
					alpha_table[1] = alpha_1;
					for (int n = 0; n < 6; n++) alpha_table[n + 2] = (byte)(((6 - n) * alpha_0 + (n + 1) * alpha_1) / 7);
				}
				else
				{
					alpha_table[0] = alpha_0;
					alpha_table[1] = alpha_1;
					for (int n = 0; n < 4; n++) alpha_table[n + 2] = (byte)(((4 - n) * alpha_0 + (n + 1) * alpha_1) / 5);
					alpha_table[6] = 0;
					alpha_table[7] = 255;
				}

				var alpha_data_transfer = new byte[16];
				transferAlphaData(ref alpha_data_transfer, true);
				for (int n = 0; n < 16; n++)
				{
					output[n] = Color.FromArgb(
						alpha_table[alpha_data_transfer[n]],
						output[n].R,
						output[n].G,
						output[n].B
					);
				}
			}
		}

		unsafe public struct AlphaColorBlock
		{
			public DXT5.AlphaBlock alphaBlock;
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
