using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Drawing;

namespace TalesOfVesperiaUtils.Imaging
{
	unsafe public class CompressDXT5
	{
		public struct _Y_CO_CG_A
		{
			public byte V0, V1, V2, V3;

			static public implicit operator _Y_CO_CG_A(Y_CO_CG_A In)
			{
				return new _Y_CO_CG_A()
				{
					V0 = In.CO,
					V1 = In.CG,
					V2 = In.A,
					V3 = In.Y,
				};
			}

			static public implicit operator Y_CO_CG_A(_Y_CO_CG_A In)
			{
				return new Y_CO_CG_A()
				{
					CO = In.V0,
					CG = In.V1,
					A = In.V2,
					Y = In.V3,
				};
			}
		}

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

		static private void GetMinMaxYCoCg(_Y_CO_CG_A[] ColorBlock, out _Y_CO_CG_A MinColor, out _Y_CO_CG_A MaxColor)
		{
			MinColor.V0 = MinColor.V1 = MinColor.V2 = MinColor.V3 = 0xFF;
			MaxColor.V0 = MaxColor.V1 = MaxColor.V2 = MaxColor.V3 = 0x00;

			for (int i = 0; i < 16; i++)
			{
				MinColor.V0 = Math.Min(MinColor.V0, ColorBlock[i].V0);
				MinColor.V1 = Math.Min(MinColor.V1, ColorBlock[i].V1);
				MinColor.V2 = Math.Min(MinColor.V2, ColorBlock[i].V2);
				MinColor.V3 = Math.Min(MinColor.V3, ColorBlock[i].V3);

				MaxColor.V0 = Math.Min(MaxColor.V0, ColorBlock[i].V0);
				MaxColor.V1 = Math.Min(MaxColor.V1, ColorBlock[i].V1);
				MaxColor.V2 = Math.Min(MaxColor.V2, ColorBlock[i].V2);
				MaxColor.V3 = Math.Min(MaxColor.V3, ColorBlock[i].V3);
			}
		}

		static private void ScaleYCoCg(_Y_CO_CG_A[] ColorBlock, ref _Y_CO_CG_A MinColor, ref _Y_CO_CG_A MaxColor)
		{
			int i;
			int m0 = Math.Abs(MinColor.V0 - 128);
			int m1 = Math.Abs(MinColor.V1 - 128);
			int m2 = Math.Abs(MaxColor.V0 - 128);
			int m3 = Math.Abs(MaxColor.V1 - 128);

			if (m1 > m0) m0 = m1;
			if (m3 > m2) m2 = m3;
			if (m2 > m0) m0 = m2;

			const int s0 = 128 / 2 - 1;
			const int s1 = 128 / 4 - 1;

			int mask0 = -((m0 <= s0) ? 1 : 0);
			int mask1 = -((m0 <= s1) ? 1 : 0);
			int scale = 1 + (1 & mask0) + (2 & mask1);

			MinColor.V0 = (byte)((MinColor.V0 - 128) * scale + 128);
			MinColor.V1 = (byte)((MinColor.V1 - 128) * scale + 128);
			MinColor.V2 = (byte)((scale - 1) << 3);

			MaxColor.V0 = (byte)((MaxColor.V0 - 128) * scale + 128);
			MaxColor.V1 = (byte)((MaxColor.V1 - 128) * scale + 128);
			MaxColor.V2 = (byte)((scale - 1) << 3);

			for (i = 0; i < 16; i++)
			{
				ColorBlock[i].V0 = (byte)((ColorBlock[i].V0 - 128) * scale + 128);
				ColorBlock[i].V1 = (byte)((ColorBlock[i].V1 - 128) * scale + 128);
			}
		}

		static private void InsetYCoCgBBox(ref _Y_CO_CG_A minColor, ref _Y_CO_CG_A maxColor)
		{
			var inset = stackalloc int[4];
			var mini = stackalloc int[4];
			var maxi = stackalloc int[4];

			inset[0] = (maxColor.V0 - minColor.V0) - ((1 << (INSET_COLOR_SHIFT - 1)) - 1);
			inset[1] = (maxColor.V1 - minColor.V1) - ((1 << (INSET_COLOR_SHIFT - 1)) - 1);
			inset[3] = (maxColor.V3 - minColor.V3) - ((1 << (INSET_ALPHA_SHIFT - 1)) - 1);

			mini[0] = ((minColor.V0 << INSET_COLOR_SHIFT) + inset[0]) >> INSET_COLOR_SHIFT;
			mini[1] = ((minColor.V1 << INSET_COLOR_SHIFT) + inset[1]) >> INSET_COLOR_SHIFT;
			mini[3] = ((minColor.V3 << INSET_ALPHA_SHIFT) + inset[3]) >> INSET_ALPHA_SHIFT;

			maxi[0] = ((maxColor.V0 << INSET_COLOR_SHIFT) - inset[0]) >> INSET_COLOR_SHIFT;
			maxi[1] = ((maxColor.V1 << INSET_COLOR_SHIFT) - inset[1]) >> INSET_COLOR_SHIFT;
			maxi[3] = ((maxColor.V3 << INSET_ALPHA_SHIFT) - inset[3]) >> INSET_ALPHA_SHIFT;

			mini[0] = (mini[0] >= 0) ? mini[0] : 0;
			mini[1] = (mini[1] >= 0) ? mini[1] : 0;
			mini[3] = (mini[3] >= 0) ? mini[3] : 0;

			maxi[0] = (maxi[0] <= 255) ? maxi[0] : 255;
			maxi[1] = (maxi[1] <= 255) ? maxi[1] : 255;
			maxi[3] = (maxi[3] <= 255) ? maxi[3] : 255;

			minColor.V0 = (byte)((mini[0] & C565_5_MASK) | (mini[0] >> 5));
			minColor.V1 = (byte)((mini[1] & C565_6_MASK) | (mini[1] >> 6));
			minColor.V3 = (byte)(mini[3]);

			maxColor.V0 = (byte)((maxi[0] & C565_5_MASK) | (maxi[0] >> 5));
			maxColor.V1 = (byte)((maxi[1] & C565_6_MASK) | (maxi[1] >> 6));
			maxColor.V3 = (byte)(maxi[3]);
		}

		static private void SelectYCoCgDiagonal(_Y_CO_CG_A[] colorBlock, ref _Y_CO_CG_A MinColor, ref _Y_CO_CG_A MaxColor)
		{
			int i;
			byte c0, c1;
			byte mid0 = (byte)(((int)MinColor.V0 + MaxColor.V0 + 1) >> 1);
			byte mid1 = (byte)(((int)MinColor.V1 + MaxColor.V1 + 1) >> 1);

			byte side = 0;
			for (i = 0; i < 16; i++)
			{
				byte b0 = (byte)((colorBlock[i].V0 >= mid0) ? 1 : 0);
				byte b1 = (byte)((colorBlock[i].V1 >= mid1) ? 1 : 0);
				side += (byte)(b0 ^ b1);
			}

			byte mask = (byte)(-((side > 8) ? 1 : 0));

			//if (NVIDIA_7X_HARDWARE_BUG_FIX) mask &= -( minColor[0] != maxColor[0] );

			c0 = MinColor.V1;
			c1 = MaxColor.V1;

			c0 ^= c1 ^= mask &= c0 ^= c1;

			MinColor.V1 = c0;
			MaxColor.V1 = c1;
		}

		/*
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
		*/

		static public void CompressBlock(ARGB_Rev[] Colors, ref DXT5.Block Block)
		{
			var ColorsCoGg = new _Y_CO_CG_A[16];
			for (int n = 0; n < 16; n++) ColorsCoGg[n] = (_Y_CO_CG_A)(Y_CO_CG_A)Colors[n];
			CompressBlock(ColorsCoGg, ref Block);
		}

		static public void CompressBlock(_Y_CO_CG_A[] Colors, ref DXT5.Block Block)
		{
			var MinColor = default(_Y_CO_CG_A);
			var MaxColor = default(_Y_CO_CG_A);

			GetMinMaxYCoCg(Colors, out MinColor, out MaxColor);
			ScaleYCoCg(Colors, ref MinColor, ref MaxColor);
			InsetYCoCgBBox(ref MinColor, ref MaxColor);
			SelectYCoCgDiagonal(Colors, ref MinColor, ref MaxColor);

			Console.WriteLine((ARGB_Rev)(Y_CO_CG_A)MinColor);
			Console.WriteLine((ARGB_Rev)(Y_CO_CG_A)MaxColor);
		}
	}
}
