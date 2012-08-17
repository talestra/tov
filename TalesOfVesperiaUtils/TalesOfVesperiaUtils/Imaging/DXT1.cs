using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Drawing;
using CSharpUtils.Endian;
using TalesOfVesperiaUtils.Imaging.Internal;

namespace TalesOfVesperiaUtils.Imaging
{
	public class DXT1 : DXT<DXT1.ColorBlock>
	{
		public struct ColorBlock
		{
			public ushort_be colors0, colors1;
			public ushort_be color_data0, color_data1;

			internal void transferColorData(ref byte[] color_data_transfer, bool extract)
			{
				exs_ins2(color_data_transfer, ref color_data0, extract, 0, 0, 0, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0, 1, 2, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0, 2, 4, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0, 3, 6, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0, 4, 8, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0, 5, 10, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0, 6, 12, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0, 7, 14, 2, 0);

				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 8, 0, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 9, 2, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 10, 4, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 11, 6, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 12, 8, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 13, 10, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 14, 12, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 15, 14, 2, 0);
			}

			internal void Decode(ref ARGB_Rev[] output)
			{
				var color_table = new ARGB_Rev[4];

				color_table[0] = ColorUtils.Encode(ColorFormats.RGBA_5650, colors0);
				color_table[1] = ColorUtils.Encode(ColorFormats.RGBA_5650, colors1);
				MathUtils.Swap(ref color_table[0].R, ref color_table[0].B);
				MathUtils.Swap(ref color_table[1].R, ref color_table[1].B);

				// Color table.
				if (colors0 > colors1)
				{
					color_table[2] = ColorUtils.Mix(color_table[0], color_table[1], 3, 2, 1);
					color_table[3] = ColorUtils.Mix(color_table[0], color_table[1], 3, 1, 2);
				}
				else
				{
					color_table[2] = ColorUtils.Mix(color_table[0], color_table[1], 2, 1, 1);
					color_table[3] = Color.FromArgb(0, 0, 0, 0);
				}

				var color_data_transfer = new byte[16];
				transferColorData(ref color_data_transfer, extract: true);
				for (int n = 0; n < 16; n++) output[n] = color_table[color_data_transfer[n]];
			}
		}

		protected override void EncodeBlock(ref DXT1.ColorBlock Block, ref ARGB_Rev[] Colors, CompressDXT.CompressionMode CompressionMode)
		{
			CompressDXT.CompressBlockDXT1(Colors, out Block, CompressionMode);
		}

		protected override void DecodeBlock(ref DXT1.ColorBlock Block, ref ARGB_Rev[] Colors)
		{
			Block.Decode(ref Colors);
		}
	}
}
