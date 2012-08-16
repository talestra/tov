using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Drawing;

namespace TalesOfVesperiaUtils.Imaging
{
	unsafe public class CompressDXT5
	{
		/// <summary>
		/// inset color bounding box
		/// </summary>
		const int INSET_COLOR_SHIFT = 4;
		
		/// <summary>
		/// inset alpha bounding box
		/// </summary>
		const int INSET_ALPHA_SHIFT = 5;

		/// <summary>
		/// 0xFF minus last three bits
		/// </summary>
		const int C565_5_MASK = 0xF8;

		/// <summary>
		/// 0xFF minus last two bits
		/// </summary>
		const int C565_6_MASK = 0xFC;

		byte* globalOutData;

		ushort ColorTo565(byte* color)
		{
			return (ushort)(((color[0] >> 3) << 11) | ((color[1] >> 2) << 5) | (color[2] >> 3));
		}

		void EmitByte(byte b)
		{
			globalOutData[0] = b;
			globalOutData += 1;
		}

		void EmitWord(ushort s)
		{
			globalOutData[0] = (byte)((s >> 0) & 255);
			globalOutData[1] = (byte)((s >> 8) & 255);
			globalOutData += 2;
		}

		void EmitDoubleWord(uint i)
		{
			globalOutData[0] = (byte)((i >> 0) & 255);
			globalOutData[1] = (byte)((i >> 8) & 255);
			globalOutData[2] = (byte)((i >> 16) & 255);
			globalOutData[3] = (byte)((i >> 24) & 255);
			globalOutData += 4;
		}

		void ExtractBlock(byte* inPtr, int width, byte* colorBlock)
		{
			int j;
			for (j = 0; j < 4; j++)
			{
				PointerUtils.Memcpy(&colorBlock[j * 4 * 4], inPtr, 4 * 4);
				inPtr += width * 4;
			}
		}

		void GetMinMaxYCoCg(byte* colorBlock, byte* minColor, byte* maxColor)
		{
			int i;

			minColor[0] = minColor[1] = minColor[2] = minColor[3] = 255;
			maxColor[0] = maxColor[1] = maxColor[2] = maxColor[3] = 0;

			for (i = 0; i < 16; i++)
			{
				if (colorBlock[i * 4 + 0] < minColor[0]) minColor[0] = colorBlock[i * 4 + 0];
				if (colorBlock[i * 4 + 1] < minColor[1]) minColor[1] = colorBlock[i * 4 + 1];
				if (colorBlock[i * 4 + 2] < minColor[2]) minColor[2] = colorBlock[i * 4 + 2];
				if (colorBlock[i * 4 + 3] < minColor[3]) minColor[3] = colorBlock[i * 4 + 3];

				if (colorBlock[i * 4 + 0] > maxColor[0]) maxColor[0] = colorBlock[i * 4 + 0];
				if (colorBlock[i * 4 + 1] > maxColor[1]) maxColor[1] = colorBlock[i * 4 + 1];
				if (colorBlock[i * 4 + 2] > maxColor[2]) maxColor[2] = colorBlock[i * 4 + 2];
				if (colorBlock[i * 4 + 3] > maxColor[3]) maxColor[3] = colorBlock[i * 4 + 3];
			}
		}

		void ScaleYCoCg(byte* colorBlock, byte* minColor, byte* maxColor)
		{
			int i;
			int m0 = Math.Abs(minColor[0] - 128);
			int m1 = Math.Abs(minColor[1] - 128);
			int m2 = Math.Abs(maxColor[0] - 128);
			int m3 = Math.Abs(maxColor[1] - 128);

			if (m1 > m0) m0 = m1;
			if (m3 > m2) m2 = m3;
			if (m2 > m0) m0 = m2;

			const int s0 = 128 / 2 - 1;
			const int s1 = 128 / 4 - 1;

			int mask0 = -((m0 <= s0) ? 1 : 0);
			int mask1 = -((m0 <= s1) ? 1 : 0);
			int scale = 1 + (1 & mask0) + (2 & mask1);

			minColor[0] = (byte)((minColor[0] - 128) * scale + 128);
			minColor[1] = (byte)((minColor[1] - 128) * scale + 128);
			minColor[2] = (byte)((scale - 1) << 3);

			maxColor[0] = (byte)((maxColor[0] - 128) * scale + 128);
			maxColor[1] = (byte)((maxColor[1] - 128) * scale + 128);
			maxColor[2] = (byte)((scale - 1) << 3);

			for (i = 0; i < 16; i++)
			{
				colorBlock[i * 4 + 0] = (byte)((colorBlock[i * 4 + 0] - 128) * scale + 128);
				colorBlock[i * 4 + 1] = (byte)((colorBlock[i * 4 + 1] - 128) * scale + 128);
			}
		}

		void InsetYCoCgBBox(byte* minColor, byte* maxColor)
		{
			var inset = stackalloc int[4];
			var mini = stackalloc int[4];
			var maxi = stackalloc int[4];

			inset[0] = (maxColor[0] - minColor[0]) - ((1 << (INSET_COLOR_SHIFT - 1)) - 1);
			inset[1] = (maxColor[1] - minColor[1]) - ((1 << (INSET_COLOR_SHIFT - 1)) - 1);
			inset[3] = (maxColor[3] - minColor[3]) - ((1 << (INSET_ALPHA_SHIFT - 1)) - 1);

			mini[0] = ((minColor[0] << INSET_COLOR_SHIFT) + inset[0]) >> INSET_COLOR_SHIFT;
			mini[1] = ((minColor[1] << INSET_COLOR_SHIFT) + inset[1]) >> INSET_COLOR_SHIFT;
			mini[3] = ((minColor[3] << INSET_ALPHA_SHIFT) + inset[3]) >> INSET_ALPHA_SHIFT;

			maxi[0] = ((maxColor[0] << INSET_COLOR_SHIFT) - inset[0]) >> INSET_COLOR_SHIFT;
			maxi[1] = ((maxColor[1] << INSET_COLOR_SHIFT) - inset[1]) >> INSET_COLOR_SHIFT;
			maxi[3] = ((maxColor[3] << INSET_ALPHA_SHIFT) - inset[3]) >> INSET_ALPHA_SHIFT;

			mini[0] = (mini[0] >= 0) ? mini[0] : 0;
			mini[1] = (mini[1] >= 0) ? mini[1] : 0;
			mini[3] = (mini[3] >= 0) ? mini[3] : 0;

			maxi[0] = (maxi[0] <= 255) ? maxi[0] : 255;
			maxi[1] = (maxi[1] <= 255) ? maxi[1] : 255;
			maxi[3] = (maxi[3] <= 255) ? maxi[3] : 255;

			minColor[0] = (byte)((mini[0] & C565_5_MASK) | (mini[0] >> 5));
			minColor[1] = (byte)((mini[1] & C565_6_MASK) | (mini[1] >> 6));
			minColor[3] = (byte)(mini[3]);

			maxColor[0] = (byte)((maxi[0] & C565_5_MASK) | (maxi[0] >> 5));
			maxColor[1] = (byte)((maxi[1] & C565_6_MASK) | (maxi[1] >> 6));
			maxColor[3] = (byte)(maxi[3]);
		}

		void SelectYCoCgDiagonal(byte* colorBlock, byte* minColor, byte* maxColor)
		{
			int i;
			byte c0, c1;
			byte mid0 = (byte)(((int)minColor[0] + maxColor[0] + 1) >> 1);
			byte mid1 = (byte)(((int)minColor[1] + maxColor[1] + 1) >> 1);

			byte side = 0;
			for (i = 0; i < 16; i++)
			{
				byte b0 = (byte)((colorBlock[i * 4 + 0] >= mid0) ? 1 : 0);
				byte b1 = (byte)((colorBlock[i * 4 + 1] >= mid1) ? 1 : 0);
				side += (byte)(b0 ^ b1);
			}

			byte mask = (byte)(-((side > 8) ? 1 : 0));

			//if (NVIDIA_7X_HARDWARE_BUG_FIX) mask &= -( minColor[0] != maxColor[0] );

			c0 = minColor[1];
			c1 = maxColor[1];

			c0 ^= c1 ^= mask &= c0 ^= c1;

			minColor[1] = c0;
			maxColor[1] = c1;
		}

		void EmitAlphaIndices(byte* colorBlock, byte minAlpha, byte maxAlpha)
		{

			int i;
			//assert( maxAlpha >= minAlpha );

			const int ALPHA_RANGE = 7;

			byte mid, ab1, ab2, ab3, ab4, ab5, ab6, ab7;
			var indexes = stackalloc byte[16];

			mid = (byte)((maxAlpha - minAlpha) / (2 * ALPHA_RANGE));

			ab1 = (byte)(minAlpha + mid);
			ab2 = (byte)((6 * maxAlpha + 1 * minAlpha) / ALPHA_RANGE + mid);
			ab3 = (byte)((5 * maxAlpha + 2 * minAlpha) / ALPHA_RANGE + mid);
			ab4 = (byte)((4 * maxAlpha + 3 * minAlpha) / ALPHA_RANGE + mid);
			ab5 = (byte)((3 * maxAlpha + 4 * minAlpha) / ALPHA_RANGE + mid);
			ab6 = (byte)((2 * maxAlpha + 5 * minAlpha) / ALPHA_RANGE + mid);
			ab7 = (byte)((1 * maxAlpha + 6 * minAlpha) / ALPHA_RANGE + mid);

			for (i = 0; i < 16; i++)
			{
				byte a = colorBlock[i * 4 + 3];
				int b1 = (a <= ab1) ? 1 : 0;
				int b2 = (a <= ab2) ? 1 : 0;
				int b3 = (a <= ab3) ? 1 : 0;
				int b4 = (a <= ab4) ? 1 : 0;
				int b5 = (a <= ab5) ? 1 : 0;
				int b6 = (a <= ab6) ? 1 : 0;
				int b7 = (a <= ab7) ? 1 : 0;
				int index = (b1 + b2 + b3 + b4 + b5 + b6 + b7 + 1) & 7;
				indexes[i] = (byte)(index ^ ((2 > index) ? 1 : 0));
			}

			EmitByte((byte)((indexes[0] >> 0) | (indexes[1] << 3) | (indexes[2] << 6)));
			EmitByte((byte)((indexes[2] >> 2) | (indexes[3] << 1) | (indexes[4] << 4) | (indexes[5] << 7)));
			EmitByte((byte)((indexes[5] >> 1) | (indexes[6] << 2) | (indexes[7] << 5)));

			EmitByte((byte)((indexes[8] >> 0) | (indexes[9] << 3) | (indexes[10] << 6)));
			EmitByte((byte)((indexes[10] >> 2) | (indexes[11] << 1) | (indexes[12] << 4) | (indexes[13] << 7)));
			EmitByte((byte)((indexes[13] >> 1) | (indexes[14] << 2) | (indexes[15] << 5)));
		}

		void EmitColorIndices(byte* colorBlock, byte* minColor, byte* maxColor)
		{
			var colors = new ushort[4, 4];
			int i;
			uint result = 0;

			colors[0, 0] = (ushort)((maxColor[0] & C565_5_MASK) | (maxColor[0] >> 5));
			colors[0, 1] = (ushort)((maxColor[1] & C565_6_MASK) | (maxColor[1] >> 6));
			colors[0, 2] = (ushort)((maxColor[2] & C565_5_MASK) | (maxColor[2] >> 5));
			colors[0, 3] = (ushort)(0);
			colors[1, 0] = (ushort)((minColor[0] & C565_5_MASK) | (minColor[0] >> 5));
			colors[1, 1] = (ushort)((minColor[1] & C565_6_MASK) | (minColor[1] >> 6));
			colors[1, 2] = (ushort)((minColor[2] & C565_5_MASK) | (minColor[2] >> 5));
			colors[1, 3] = (ushort)(0);
			colors[2, 0] = (ushort)((2 * colors[0, 0] + 1 * colors[1, 0]) / 3);
			colors[2, 1] = (ushort)((2 * colors[0, 1] + 1 * colors[1, 1]) / 3);
			colors[2, 2] = (ushort)((2 * colors[0, 2] + 1 * colors[1, 2]) / 3);
			colors[2, 3] = (ushort)(0);
			colors[3, 0] = (ushort)((1 * colors[0, 0] + 2 * colors[1, 0]) / 3);
			colors[3, 1] = (ushort)((1 * colors[0, 1] + 2 * colors[1, 1]) / 3);
			colors[3, 2] = (ushort)((1 * colors[0, 2] + 2 * colors[1, 2]) / 3);
			colors[3, 3] = (ushort)(0);

			for (i = 15; i >= 0; i--)
			{
				int c0, c1, d0, d1, d2, d3;

				c0 = colorBlock[i * 4 + 0];
				c1 = colorBlock[i * 4 + 1];

				d0 = Math.Abs(colors[0, 0] - c0) + Math.Abs(colors[0, 1] - c1);
				d1 = Math.Abs(colors[1, 0] - c0) + Math.Abs(colors[1, 1] - c1);
				d2 = Math.Abs(colors[2, 0] - c0) + Math.Abs(colors[2, 1] - c1);
				d3 = Math.Abs(colors[3, 0] - c0) + Math.Abs(colors[3, 1] - c1);

				int b0 = (d0 > d3) ? 1 : 0;
				int b1 = (d1 > d2) ? 1 : 0;
				int b2 = (d0 > d2) ? 1 : 0;
				int b3 = (d1 > d3) ? 1 : 0;
				int b4 = (d2 > d3) ? 1 : 0;

				int x0 = b1 & b2;
				int x1 = b0 & b3;
				int x2 = b0 & b4;

				result |= (uint)((x2 | ((x0 | x1) << 1)) << (i << 1));
			}

			EmitDoubleWord(result);
		}

		static public void CompressBlock(ARGB_Rev[] Colors, ref DXT5.Block Block)
		{
			int outputBytes = 0;
			var CompressDXT5 = new CompressDXT5();
			fixed (ARGB_Rev* ColorsPtr = Colors)
			fixed (DXT5.Block* BlockPtr = &Block)
			{
				CompressDXT5.CompressYCoCgDXT5(
					(byte*)ColorsPtr,
					(byte*)BlockPtr,
					4, 4,
					out outputBytes
				);
			}
		}

		bool CompressYCoCgDXT5(byte* inBuf, byte* outBuf, int width, int height, out int outputBytes)
		{
			int i, j;
			var block = stackalloc byte[64];
			var minColor = stackalloc byte[4];
			var maxColor = stackalloc byte[4];

			globalOutData = outBuf;

			for (j = 0; j < height; j += 4, inBuf += width * 4 * 4)
			{
				for (i = 0; i < width; i += 4)
				{

					ExtractBlock(inBuf + i * 4, width, block);

					GetMinMaxYCoCg(block, minColor, maxColor);
					ScaleYCoCg(block, minColor, maxColor);
					InsetYCoCgBBox(minColor, maxColor);
					SelectYCoCgDiagonal(block, minColor, maxColor);

					EmitByte(maxColor[3]);
					EmitByte(minColor[3]);

					EmitAlphaIndices(block, minColor[3], maxColor[3]);

					EmitWord(ColorTo565(maxColor));
					EmitWord(ColorTo565(minColor));

					EmitColorIndices(block, minColor, maxColor);
				}
			}

			outputBytes = (int)(globalOutData - outBuf);

			return true;
		}

	}
}
