// configuration options for DXT encoder. set them in the project/makefile or just define
// them at the top.

// STB_DXT_USE_ROUNDING_BIAS
//     use a rounding bias during color interpolation. this is closer to what "ideal"
//     interpolation would do but doesn't match the S3TC/DX10 spec. old versions (pre-1.03)
//     implicitly had this turned on. 
//
//     in case you're targeting a specific type of hardware (e.g. console programmers):
//     NVidia and Intel GPUs (as of 2010) as well as DX9 ref use DXT decoders that are closer
//     to STB_DXT_USE_ROUNDING_BIAS. AMD/ATI, S3 and DX10 ref are closer to rounding with no bias.
//     you also see "(a*5 + b*3) / 8" on some old GPU designs.
// #define STB_DXT_USE_ROUNDING_BIAS

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
		// stb_dxt.h - v1.04 - DXT1/DXT5 compressor - public domain
		// original by fabian "ryg" giesen - ported to C by stb
		// use '#define STB_DXT_IMPLEMENTATION' before including to create the implementation
		//
		// USAGE:
		//   call stb_compress_dxt_block() for every block (you must pad)
		//     source should be a 4x4 block of RGBA data in row-major order;
		//     A is ignored if you specify alpha=0; you can turn on dithering
		//     and "high quality" using mode.
		//
		// version history:
		//   v1.04  - (ryg) default to no rounding bias for lerped colors (as per S3TC/DX10 spec);
		//            single color match fix (allow for inexact color interpolation);
		//            optimal DXT5 index finder; "high quality" mode that runs multiple refinement steps.
		//   v1.03  - (stb) endianness support
		//   v1.02  - (stb) fix alpha encoding bug
		//   v1.01  - (stb) fix bug converting to RGB that messed up quality, thanks ryg & cbloom
		//   v1.00  - (stb) first release

		// compression mode (bitflags)

		public enum CompressionMode
		{
			/// <summary>
			/// 
			/// </summary>
			Normal = 0,

			/// <summary>
			/// use dithering. dubious win. never use for normal maps and the like!
			/// </summary>
			Dither = 1,

			/// <summary>
			/// high quality mode, does two refinement steps instead of 1. ~30-40% slower.
			/// </summary>
			HighQuality = 2,
		}

		static byte[] stb__Expand5 = new byte[32];
		static byte[] stb__Expand6 = new byte[64];
		static byte[,] stb__OMatch5 = new byte[256, 2];
		static byte[,] stb__OMatch6 = new byte[256, 2];
		static byte[] stb__QuantRBTab = new byte[256 + 16];
		static byte[] stb__QuantGTab = new byte[256 + 16];

		static int stb__Mul8Bit(int a, int b)
		{
			int t = a * b + 128;
			return (t + (t >> 8)) >> 8;
		}

		static void stb__From16Bit(out ARGB_Rev _out, ushort v)
		{
			int rv = (v & 0xf800) >> 11;
			int gv = (v & 0x07e0) >> 5;
			int bv = (v & 0x001f) >> 0;

			_out.R = stb__Expand5[rv];
			_out.G = stb__Expand6[gv];
			_out.B = stb__Expand5[bv];
			_out.A = 0;
		}

		static ushort stb__As16Bit(int r, int g, int b)
		{
			return (ushort)((stb__Mul8Bit(r, 31) << 11) + (stb__Mul8Bit(g, 63) << 5) + stb__Mul8Bit(b, 31));
		}

		// linear interpolation at 1/3 point between a and b, using desired rounding type
		static int stb__Lerp13(int a, int b)
		{
#if STB_DXT_USE_ROUNDING_BIAS
		   // with rounding bias
		   return a + stb__Mul8Bit(b-a, 0x55);
#else
			// without rounding bias
			// replace "/ 3" by "* 0xaaab) >> 17" if your compiler sucks or you really need every ounce of speed.
			return (2 * a + b) / 3;
#endif
		}

		// lerp RGB color
		static void stb__Lerp13RGB(ref ARGB_Rev _out, ARGB_Rev p1, ARGB_Rev p2)
		{
			_out.R = (byte)stb__Lerp13(p1.R, p2.R);
			_out.G = (byte)stb__Lerp13(p1.G, p2.G);
			_out.B = (byte)stb__Lerp13(p1.B, p2.B);
		}

		/****************************************************************************/

		// compute table to reproduce constant colors as accurately as possible
		static void stb__PrepareOptTable(byte* Table, byte* expand, int size)
		{
			int i, mn, mx;
			for (i = 0; i < 256; i++)
			{
				int bestErr = 256;
				for (mn = 0; mn < size; mn++)
				{
					for (mx = 0; mx < size; mx++)
					{
						int mine = expand[mn];
						int maxe = expand[mx];
						int err = Math.Abs(stb__Lerp13(maxe, mine) - i);

						// DX10 spec says that interpolation must be within 3% of "correct" result,
						// add this as error term. (normally we'd expect a random distribution of
						// +-1.5% error, but nowhere in the spec does it say that the error has to be
						// unbiased - better safe than sorry).
						err += Math.Abs(maxe - mine) * 3 / 100;

						if (err < bestErr)
						{
							Table[i * 2 + 0] = (byte)mx;
							Table[i * 2 + 1] = (byte)mn;
							bestErr = err;
						}
					}
				}
			}
		}

		static void stb__EvalColors(ARGB_Rev* color, ushort c0, ushort c1)
		{
			stb__From16Bit(out color[0], c0);
			stb__From16Bit(out color[1], c1);
			stb__Lerp13RGB(ref color[2], color[0], color[1]);
			stb__Lerp13RGB(ref color[3], color[1], color[0]);
		}

		// Block dithering function. Simply dithers a block to 565 RGB.
		// (Floyd-Steinberg)
		static void stb__DitherBlock(ARGB_Rev* dest, ARGB_Rev* block)
		{
			var err = stackalloc int[8];
			int* ep1 = err;
			int* ep2 = err + 4;
			int* et;
			int ch, y;

			fixed (byte* _stb__QuantGTab = stb__QuantGTab)
			fixed (byte* _stb__QuantRBTab = stb__QuantRBTab)
			{
				var bpList = new byte*[] { &block->R, &block->G, &block->B };
				var dpList = new byte*[] { &dest->R, &dest->G, &dest->B };

				// process channels seperately
				for (ch = 0; ch < 3; ++ch)
				{
					byte* bp = bpList[ch];
					byte* dp = dpList[ch];
					byte* quant = (ch == 1) ? _stb__QuantGTab + 8 : _stb__QuantRBTab + 8;
					PointerUtils.Memset((byte*)err, 0, sizeof(int) * 8);
					for (y = 0; y < 4; ++y)
					{
						dp[0] = quant[bp[0] + ((3 * ep2[1] + 5 * ep2[0]) >> 4)];
						ep1[0] = bp[0] - dp[0];
						dp[4] = quant[bp[4] + ((7 * ep1[0] + 3 * ep2[2] + 5 * ep2[1] + ep2[0]) >> 4)];
						ep1[1] = bp[4] - dp[4];
						dp[8] = quant[bp[8] + ((7 * ep1[1] + 3 * ep2[3] + 5 * ep2[2] + ep2[1]) >> 4)];
						ep1[2] = bp[8] - dp[8];
						dp[12] = quant[bp[12] + ((7 * ep1[2] + 5 * ep2[3] + ep2[2]) >> 4)];
						ep1[3] = bp[12] - dp[12];
						bp += 16;
						dp += 16;
						et = ep1; ep1 = ep2; ep2 = et; // swap
					}
				}
			}
		}

		// The color matching function
		static uint stb__MatchColorsBlock(ARGB_Rev* block, ARGB_Rev* color, bool dither)
		{
			uint mask = 0;
			int dirr = color[0].R - color[1].R;
			int dirg = color[0].G - color[1].G;
			int dirb = color[0].B - color[1].B;
			var dots = stackalloc int[16];
			var stops = stackalloc int[4];
			int i;
			int c0Point, halfPoint, c3Point;

			for (i = 0; i < 16; i++)
				dots[i] = block[i].R * dirr + block[i].G * dirg + block[i].B * dirb;

			for (i = 0; i < 4; i++)
				stops[i] = color[i].R * dirr + color[i].G * dirg + color[i].B * dirb;

			// think of the colors as arranged on a line; project point onto that line, then choose
			// next color out of available ones. we compute the crossover points for "best color in top
			// half"/"best in bottom half" and then the same inside that subinterval.
			//
			// relying on this 1d approximation isn't always optimal in terms of euclidean distance,
			// but it's very close and a lot faster.
			// http://cbloomrants.blogspot.com/2008/12/12-08-08-dxtc-summary.html

			c0Point = (stops[1] + stops[3]) >> 1;
			halfPoint = (stops[3] + stops[2]) >> 1;
			c3Point = (stops[2] + stops[0]) >> 1;

			if (!dither)
			{
				// the version without dithering is straightforward
				for (i = 15; i >= 0; i--)
				{
					int dot = dots[i];
					mask <<= 2;

					if (dot < halfPoint)
						mask |= (uint)((dot < c0Point) ? 1 : 3);
					else
						mask |= (uint)((dot < c3Point) ? 2 : 0);
				}
			}
			else
			{
				// with floyd-steinberg dithering
				var err = stackalloc int[8];
				int* ep1 = err;
				int* ep2 = err + 4;
				int* dp = dots;
				int y;

				c0Point <<= 4;
				halfPoint <<= 4;
				c3Point <<= 4;
				for (i = 0; i < 8; i++)
					err[i] = 0;

				for (y = 0; y < 4; y++)
				{
					int dot, lmask, step;

					dot = (dp[0] << 4) + (3 * ep2[1] + 5 * ep2[0]);
					if (dot < halfPoint)
						step = (dot < c0Point) ? 1 : 3;
					else
						step = (dot < c3Point) ? 2 : 0;
					ep1[0] = dp[0] - stops[step];
					lmask = step;

					dot = (dp[1] << 4) + (7 * ep1[0] + 3 * ep2[2] + 5 * ep2[1] + ep2[0]);
					if (dot < halfPoint)
						step = (dot < c0Point) ? 1 : 3;
					else
						step = (dot < c3Point) ? 2 : 0;
					ep1[1] = dp[1] - stops[step];
					lmask |= step << 2;

					dot = (dp[2] << 4) + (7 * ep1[1] + 3 * ep2[3] + 5 * ep2[2] + ep2[1]);
					if (dot < halfPoint)
						step = (dot < c0Point) ? 1 : 3;
					else
						step = (dot < c3Point) ? 2 : 0;
					ep1[2] = dp[2] - stops[step];
					lmask |= step << 4;

					dot = (dp[3] << 4) + (7 * ep1[2] + 5 * ep2[3] + ep2[2]);
					if (dot < halfPoint)
						step = (dot < c0Point) ? 1 : 3;
					else
						step = (dot < c3Point) ? 2 : 0;
					ep1[3] = dp[3] - stops[step];
					lmask |= step << 6;

					dp += 4;
					mask |= (uint)(lmask << (y * 8));
					{ int* et = ep1; ep1 = ep2; ep2 = et; } // swap
				}
			}

			return mask;
		}

		// The color optimization function. (Clever code, part 1)
		static void stb__OptimizeColorsBlock(ARGB_Rev* block, ushort* pmax16, ushort* pmin16)
		{
			int mind = 0x7fffffff, maxd = -0x7fffffff;
			var minp = default(ARGB_Rev);
			var maxp = default(ARGB_Rev);
			double magn;
			int v_r, v_g, v_b;
			const int nIterPower = 4;
			float* covf = stackalloc float[6];
			float vfr, vfg, vfb;

			// determine color distribution
			var cov = stackalloc int[6];
			var mu = stackalloc int[3];
			var min = stackalloc int[3];
			var max = stackalloc int[3];
			int ch, i, iter;

			for (ch = 0; ch < 3; ch++)
			{
				byte* bp = ((byte*)block) + ch;
				int muv, minv, maxv;

				muv = minv = maxv = bp[0];
				for (i = 4; i < 64; i += 4)
				{
					muv += bp[i];
					if (bp[i] < minv) minv = bp[i];
					else if (bp[i] > maxv) maxv = bp[i];
				}

				mu[ch] = (muv + 8) >> 4;
				min[ch] = minv;
				max[ch] = maxv;
			}

			// determine covariance matrix
			for (i = 0; i < 6; i++)
				cov[i] = 0;

			for (i = 0; i < 16; i++)
			{
				int r = block[i].R - mu[0];
				int g = block[i].G - mu[1];
				int b = block[i].B - mu[2];

				cov[0] += r * r;
				cov[1] += r * g;
				cov[2] += r * b;
				cov[3] += g * g;
				cov[4] += g * b;
				cov[5] += b * b;
			}

			// convert covariance matrix to float, find principal axis via power iter
			for (i = 0; i < 6; i++)
				covf[i] = cov[i] / 255.0f;

			vfr = (float)(max[0] - min[0]);
			vfg = (float)(max[1] - min[1]);
			vfb = (float)(max[2] - min[2]);

			for (iter = 0; iter < nIterPower; iter++)
			{
				float r = vfr * covf[0] + vfg * covf[1] + vfb * covf[2];
				float g = vfr * covf[1] + vfg * covf[3] + vfb * covf[4];
				float b = vfr * covf[2] + vfg * covf[4] + vfb * covf[5];

				vfr = r;
				vfg = g;
				vfb = b;
			}

			magn = Math.Abs(vfr);
			if (Math.Abs(vfg) > magn) magn = Math.Abs(vfg);
			if (Math.Abs(vfb) > magn) magn = Math.Abs(vfb);

			if (magn < 4.0f)
			{ // too small, default to luminance
				v_r = 299; // JPEG YCbCr luma coefs, scaled by 1000.
				v_g = 587;
				v_b = 114;
			}
			else
			{
				magn = 512.0 / magn;
				v_r = (int)(vfr * magn);
				v_g = (int)(vfg * magn);
				v_b = (int)(vfb * magn);
			}

			// Pick colors at extreme points
			for (i = 0; i < 16; i++)
			{
				int dot = block[i].R * v_r + block[i].G * v_g + block[i].B * v_b;

				if (dot < mind)
				{
					mind = dot;
					minp = block[i];
				}

				if (dot > maxd)
				{
					maxd = dot;
					maxp = block[i];
				}
			}

			*pmax16 = stb__As16Bit(maxp.R, maxp.G, maxp.B);
			*pmin16 = stb__As16Bit(minp.R, minp.G, minp.B);
		}

		static int stb__sclamp(float y, int p0, int p1)
		{
			int x = (int)y;
			if (x < p0) return p0;
			if (x > p1) return p1;
			return x;
		}

		// The refinement function. (Clever code, part 2)
		// Tries to optimize colors to suit block contents better.
		// (By solving a least squares system via normal equations+Cramer's rule)
		static bool stb__RefineBlock(ARGB_Rev* block, ushort* pmax16, ushort* pmin16, uint mask)
		{
			var w1Tab = new int[4] { 3, 0, 2, 1 };
			var prods = new int[4] { 0x090000, 0x000900, 0x040102, 0x010402 };
			// ^some magic to save a lot of multiplies in the accumulating loop...
			// (precomputed products of weights for least squares system, accumulated inside one 32-bit register)

			float frb, fg;
			ushort oldMin, oldMax, min16, max16;
			int i, akku = 0, xx, xy, yy;
			int At1_r, At1_g, At1_b;
			int At2_r, At2_g, At2_b;
			uint cm = mask;

			oldMin = *pmin16;
			oldMax = *pmax16;

			if ((mask ^ (mask << 2)) < 4) // all pixels have the same index?
			{
				// yes, linear system would be singular; solve using optimal
				// single-color match on average color
				int r = 8, g = 8, b = 8;
				for (i = 0; i < 16; ++i)
				{
					r += block[i].R;
					g += block[i].G;
					b += block[i].B;
				}

				r >>= 4; g >>= 4; b >>= 4;

				max16 = (ushort)((stb__OMatch5[r, 0] << 11) | (stb__OMatch6[g, 0] << 5) | stb__OMatch5[b, 0]);
				min16 = (ushort)((stb__OMatch5[r, 1] << 11) | (stb__OMatch6[g, 1] << 5) | stb__OMatch5[b, 1]);
			}
			else
			{
				At1_r = At1_g = At1_b = 0;
				At2_r = At2_g = At2_b = 0;
				for (i = 0; i < 16; ++i, cm >>= 2)
				{
					int step = (int)(cm & 3);
					int w1 = w1Tab[step];
					int r = block[i].R;
					int g = block[i].G;
					int b = block[i].B;

					akku += prods[step];
					At1_r += w1 * r;
					At1_g += w1 * g;
					At1_b += w1 * b;
					At2_r += r;
					At2_g += g;
					At2_b += b;
				}

				At2_r = 3 * At2_r - At1_r;
				At2_g = 3 * At2_g - At1_g;
				At2_b = 3 * At2_b - At1_b;

				// extract solutions and decide solvability
				xx = akku >> 16;
				yy = (akku >> 8) & 0xff;
				xy = (akku >> 0) & 0xff;

				frb = 3.0f * 31.0f / 255.0f / (xx * yy - xy * xy);
				fg = frb * 63.0f / 31.0f;

				// solve.
				max16 = (ushort)(stb__sclamp((At1_r * yy - At2_r * xy) * frb + 0.5f, 0, 31) << 11);
				max16 |= (ushort)(stb__sclamp((At1_g * yy - At2_g * xy) * fg + 0.5f, 0, 63) << 5);
				max16 |= (ushort)(stb__sclamp((At1_b * yy - At2_b * xy) * frb + 0.5f, 0, 31) << 0);

				min16 = (ushort)(stb__sclamp((At2_r * xx - At1_r * xy) * frb + 0.5f, 0, 31) << 11);
				min16 |= (ushort)(stb__sclamp((At2_g * xx - At1_g * xy) * fg + 0.5f, 0, 63) << 5);
				min16 |= (ushort)(stb__sclamp((At2_b * xx - At1_b * xy) * frb + 0.5f, 0, 31) << 0);
			}

			*pmin16 = min16;
			*pmax16 = max16;
			return (oldMin != min16) || (oldMax != max16);
		}

		// Color block compression
		static void stb__CompressColorBlock(byte* dest, ARGB_Rev* block, CompressionMode mode)
		{
			uint mask;
			int i;
			bool dither;
			int refinecount;
			ushort max16, min16;
			var dblock = stackalloc ARGB_Rev[16];
			var color = stackalloc ARGB_Rev[4];

			dither = (mode & CompressionMode.Dither) != 0;
			refinecount = ((mode & CompressionMode.HighQuality) != 0) ? 2 : 1;

			// check if block is constant
			for (i = 1; i < 16; i++)
				if (((uint*)block)[i] != ((uint*)block)[0])
					break;

			if (i == 16)
			{ // constant color
				int r = block[0].R, g = block[0].G, b = block[0].B;
				mask = 0xaaaaaaaa;
				max16 = (ushort)((stb__OMatch5[r, 0] << 11) | (stb__OMatch6[g, 0] << 5) | stb__OMatch5[b, 0]);
				min16 = (ushort)((stb__OMatch5[r, 1] << 11) | (stb__OMatch6[g, 1] << 5) | stb__OMatch5[b, 1]);
			}
			else
			{
				// first step: compute dithered version for PCA if desired
				if (dither)
					stb__DitherBlock(dblock, block);

				// second step: pca+map along principal axis
				stb__OptimizeColorsBlock(dither ? dblock : block, &max16, &min16);
				if (max16 != min16)
				{
					stb__EvalColors(color, max16, min16);
					mask = stb__MatchColorsBlock(block, color, dither);
				}
				else
					mask = 0;

				// third step: refine (multiple times if requested)
				for (i = 0; i < refinecount; i++)
				{
					uint lastmask = mask;

					if (stb__RefineBlock(dither ? dblock : block, &max16, &min16, mask))
					{
						if (max16 != min16)
						{
							stb__EvalColors(color, max16, min16);
							mask = stb__MatchColorsBlock(block, color, dither);
						}
						else
						{
							mask = 0;
							break;
						}
					}

					if (mask == lastmask)
						break;
				}
			}

			// write the color block
			if (max16 < min16)
			{
				ushort t = min16;
				min16 = max16;
				max16 = t;
				mask ^= 0x55555555;
			}

			dest[0] = (byte)(max16);
			dest[1] = (byte)(max16 >> 8);
			dest[2] = (byte)(min16);
			dest[3] = (byte)(min16 >> 8);
			dest[4] = (byte)(mask);
			dest[5] = (byte)(mask >> 8);
			dest[6] = (byte)(mask >> 16);
			dest[7] = (byte)(mask >> 24);
		}

		// Alpha block compression (this is easy for a change)
		static void stb__CompressAlphaBlock(byte* dest, ARGB_Rev* src, CompressionMode mode)
		{
			int i, dist, bias, dist4, dist2, bits, mask;

			// find min/max color
			int mn, mx;
			mn = mx = src[0].A;

			for (i = 1; i < 16; i++)
			{
				if (src[i].A < mn) mn = src[i].A;
				else if (src[i].A > mx) mx = src[i].A;
			}

			// encode them
			((byte*)dest)[0] = (byte)mx;
			((byte*)dest)[1] = (byte)mn;
			dest += 2;

			// determine bias and emit color indices
			// given the choice of mx/mn, these indices are optimal:
			// http://fgiesen.wordpress.com/2009/12/15/dxt5-alpha-block-index-determination/
			dist = mx - mn;
			dist4 = dist * 4;
			dist2 = dist * 2;
			bias = (dist < 8) ? (dist - 1) : (dist / 2 + 2);
			bias -= mn * 7;
			bits = 0;
			mask = 0;

			for (i = 0; i < 16; i++)
			{
				int a = src[i].A * 7 + bias;
				int ind, t;

				// select index. this is a "linear scale" lerp factor between 0 (val=min) and 7 (val=max).
				t = (a >= dist4) ? -1 : 0; ind = t & 4; a -= dist4 & t;
				t = (a >= dist2) ? -1 : 0; ind += t & 2; a -= dist2 & t;
				ind += (a >= dist) ? 1 : 0;

				// turn linear scale into DXT index (0/1 are extremal pts)
				ind = -ind & 7;
				ind ^= (2 > ind) ? 1 : 0;

				// write index
				mask |= ind << bits;
				if ((bits += 3) >= 8)
				{
					*dest++ = (byte)mask;
					mask >>= 8;
					bits -= 8;
				}
			}
		}

		static void stb__InitDXT()
		{
			int i;
			for (i = 0; i < 32; i++)
				stb__Expand5[i] = (byte)((i << 3) | (i >> 2));

			for (i = 0; i < 64; i++)
				stb__Expand6[i] = (byte)((i << 2) | (i >> 4));

			for (i = 0; i < 256 + 16; i++)
			{
				int v = i - 8 < 0 ? 0 : i - 8 > 255 ? 255 : i - 8;
				stb__QuantRBTab[i] = stb__Expand5[stb__Mul8Bit(v, 31)];
				stb__QuantGTab[i] = stb__Expand6[stb__Mul8Bit(v, 63)];
			}

			fixed (byte* _stb__OMatch5 = &stb__OMatch5[0, 0])
			fixed (byte* _stb__OMatch6 = &stb__OMatch6[0, 0])
			fixed (byte* _stb__Expand5 = stb__Expand5)
			fixed (byte* _stb__Expand6 = stb__Expand6)
			{
				stb__PrepareOptTable(_stb__OMatch5, _stb__Expand5, 32);
				stb__PrepareOptTable(_stb__OMatch6, _stb__Expand6, 64);
			}
		}

		static bool init = true;

		static public void CompressBlock(ARGB_Rev[] Colors, out DXT5.Block Block, CompressionMode mode = CompressionMode.HighQuality)
		{
			Block = default(DXT5.Block);

			fixed (ARGB_Rev* ColorsPtr = Colors)
			fixed (DXT5.Block* BlockPtr = &Block)
			{
				stb_compress_dxt_block((byte*)BlockPtr, ColorsPtr, true, mode);
			}
		}

#if false
		static public void CompressBlock(RGBA[] Colors, out DXT5.Block Block, CompressionMode mode = CompressionMode.HighQuality)
		{
			CompressBlock(Colors.Select(Item => (ARGB_Rev)Item).ToArray(), out Block, mode);
		}
#endif

		/*
		unsafe public struct Block
		{
			public ushort_be alpha;
			public ushort_be alpha_data0, alpha_data1, alpha_data2;
			public ushort_be colors0, colors1;
			public ushort_be color_data0, color_data1;
		}
		*/

		static private void scramble(byte* dest)
		{
			MathUtils.Swap(ref dest[0], ref dest[1]);
			MathUtils.Swap(ref dest[2], ref dest[3]);
			MathUtils.Swap(ref dest[4], ref dest[5]);
			MathUtils.Swap(ref dest[6], ref dest[7]);
		}

		static private void stb_compress_dxt_block(byte* dest, ARGB_Rev* src, bool alpha, CompressionMode mode)
		{
			if (init)
			{
				stb__InitDXT();
				init = false;
			}

			if (alpha)
			{
				stb__CompressAlphaBlock(dest, src, mode);
				scramble(dest);
				dest += 8;
			}

			stb__CompressColorBlock(dest, src, mode);
			scramble(dest);
		}
	}
}
