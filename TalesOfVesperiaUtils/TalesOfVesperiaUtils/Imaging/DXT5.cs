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
	unsafe public class DXT5 : DXT<DXT5.Block>
	{
		protected override void EncodeBlock(ref DXT5.Block Block, ref ARGB_Rev[] Colors, CompressDXT5.CompressionMode CompressionMode)
		{
			CompressDXT5.CompressBlock(Colors, out Block, CompressionMode);
		}

		protected override void DecodeBlock(ref DXT5.Block Block, ref ARGB_Rev[] Colors)
		{
			Block.Decode(ref Colors);
		}

		unsafe public struct Block
		{
			public ushort_be alpha;
			public ushort_be alpha_data0, alpha_data1, alpha_data2;
			public ushort_be colors0, colors1;
			public ushort_be color_data0, color_data1;

			public void EncodeSimpleUnoptimizedWhiteAlpha(ARGB_Rev[] input)
			{
				alpha = 0x00FF;
				colors1 = colors0 = Color.White.Encode565();
				var lookup = new byte[] { 1, 7, 6, 5, 4, 3, 2, 0 };
				var alpha_data_transfer = new byte[16];
				int k = 0;
				foreach (var color in input)
				{
					int e = ((color.A * 7) / 255);
					alpha_data_transfer[k] = lookup[e];
					k++;
				}
				transferAlphaData(ref alpha_data_transfer, extract: false);
			}

			static void EXT_INS(ref ushort container, ref byte value, bool extract, int offset, int len, int offset_value = 0)
			{
				var mask = (ushort)((1 << len) - 1);
				if (extract)
				{
					value &= (byte)~((uint)mask << offset_value);
					value |= (byte)((((int)container >> (int)offset) & mask) << offset_value);
				}
				else
				{
					container = (ushort)((container & ~(mask << offset)) | (((value >> offset_value) & mask) << offset));
					//Console.WriteLine(container);
				}
			}

			private void exs_ins2(byte[] data_transfer, ref ushort_be first_data, bool extract, uint m, uint n, int offset, int len, int offset_value = 0)
			{
				fixed (ushort_be* data = &first_data)
				{
					ushort value = data[m];
					EXT_INS(ref value, ref data_transfer[n], extract, offset, len, offset_value);
					if (!extract) data[m] = value;
				}
			}

			private void transferAlphaData(ref byte[] alpha_data_transfer, bool extract)
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

			void transferColorData(ref byte[] color_data_transfer, bool extract)
			{
				exs_ins2(color_data_transfer, ref color_data0, extract, 0,  0,  0, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0,  1,  2, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0,  2,  4, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0,  3,  6, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0,  4,  8, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0,  5, 10, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0,  6, 12, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 0,  7, 14, 2, 0);

				exs_ins2(color_data_transfer, ref color_data0, extract, 1,  8, 0, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1,  9, 2, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 10, 4, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 11, 6, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 12, 8, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 13, 10, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 14, 12, 2, 0);
				exs_ins2(color_data_transfer, ref color_data0, extract, 1, 15, 14, 2, 0);
			}

			public void Decode(ref ARGB_Rev[] output)
			{
				var alpha_table = new byte[8];
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
	}
}
