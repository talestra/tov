using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TalesOfVesperiaUtils.Imaging
{
	sealed public class Swizzling
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="TexelPitch"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static private int XGLog2LE16(int TexelPitch)
		{
			return (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
		}

		/// <summary>
		/// Determine the amount of memory occupied by a tiled 2D surface to the 
		/// granularity of a matte (subtile).  The returned size refers to the
		/// largest tiled offset potentially referenced in the surface and is 
		/// measured in texels/blocks.
		/// </summary>
		/// <param name="Width">Width of the image in texels/blocks</param>
		/// <param name="Height">Height of the image in texels/blocks</param>
		/// <param name="TexelPitch">Size of an image texel/block in bytes</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public int XGAddress2DTiledExtent(int Width, int Height, int TexelPitch)
		{
			// @TODO: Unoptimized! VERY SLOW!
			int MaxOffset = 0;
			for (int y = 0; y < Height; y++)
			for (int x = 0; x < Width; x++)
			{
				MaxOffset = Math.Max(MaxOffset, XGAddress2DTiledOffset(x, y, Width, TexelPitch));
			}
			return MaxOffset + 1;
		}

		/// <summary>
		/// Determine the amount of memory occupied by a tiled 3D volume to the 
		/// granularity of a matte (subtile). The returned size refers to the
		/// largest tiled offset potentially referenced in the volume and is 
		/// measured in texels/blocks.
		/// </summary>
		/// <param name="Width">Width of a volume slice in texels/blocks</param>
		/// <param name="Height">Height of a volume slice in texels/blocks</param>
		/// <param name="Depth">Depth of a volume slice in texels/blocks</param>
		/// <param name="TexelPitch">Size of a volume texel/block in bytes</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public int XGAddress3DTiledExtent(int Width, int Height, int Depth, int TexelPitch)
		{
			// @TODO: Unoptimized! VERY SLOW!
			int MaxOffset = 0;
			for (int z = 0; z < Depth; z++)
			for (int y = 0; y < Height; y++)
			for (int x = 0; x < Width; x++)
			{
				MaxOffset = Math.Max(MaxOffset, XGAddress3DTiledOffset(x, y, z, Width, Height, TexelPitch));
			}
			return MaxOffset + 1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Offset"></param>
		/// <param name="Width"></param>
		/// <param name="TexelPitch"></param>
		/// <param name="OutX"></param>
		/// <param name="OutY"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public void UnswizzledXY(int Offset, int Width, int TexelPitch, out int OutX, out int OutY)
		{
			OutX = Offset % Width;
			OutY = Offset / Width;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="Width"></param>
		/// <param name="TexelPitch"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public int UnswizzledOffset(int x, int y, int Width, int TexelPitch)
		{
			return (x * Width + y);
		}

		/// <summary>
		/// Translate the address of a surface texel/block from 2D array coordinates into 
		/// a tiled memory offset measured in texels/blocks.
		/// </summary>
		/// <param name="x">x coordinate of the texel/block</param>
		/// <param name="y">y coordinate of the texel/block</param>
		/// <param name="Width">Width of the image in texels/blocks</param>
		/// <param name="TexelPitch">Size of an image texel/block in bytes</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public int XGAddress2DTiledOffset(int x, int y, int Width, int TexelPitch)
		{
			//XGASSERT(Width <= 8192); // Width in memory must be less than or equal to 8K texels
			//XGASSERT(x < Width);

			int AlignedWidth = (Width + 31) & ~31;
			int LogBpp = XGLog2LE16(TexelPitch);
			int Macro = ((x >> 5) + (y >> 5) * (AlignedWidth >> 5)) << (LogBpp + 7);
			int Micro = (((x & 7) + ((y & 6) << 2)) << LogBpp);
			int Offset = Macro + ((Micro & ~15) << 1) + (Micro & 15) + ((y & 8) << (3 + LogBpp)) + ((y & 1) << 4);

			return (((Offset & ~511) << 3) + ((Offset & 448) << 2) + (Offset & 63) + 
					((y & 16) << 7) + (((((y & 8) >> 2) + (x >> 3)) & 3) << 6)) >> LogBpp;
		}

		/// <summary>
		/// Translate the address of a volume texel/block from 3D array coordinates into 
		/// a tiled memory offset measured in texels/blocks.
		/// </summary>
		/// <param name="x">x coordinate of the texel/block</param>
		/// <param name="y">y coordinate of the texel/block</param>
		/// <param name="z">z coordinate of the texel/block</param>
		/// <param name="Width">Width of a volume slice in texels/blocks</param>
		/// <param name="Height">Height of a volume slice in texels/blocks</param>
		/// <param name="TexelPitch">Size of a volume texel/block in bytes</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public int XGAddress3DTiledOffset(int x, int y, int z, int Width, int Height, int TexelPitch)
		{
			//XGASSERT(Width <= 2048); // Width in memory must be less than or equal to 2K texels
			//XGASSERT(Height <= 2048); // Height in memory must be less than or equal to 2K texels
			//XGASSERT(x < Width);
			//XGASSERT(y < Height);

			int AlignedWidth = (Width + 31) & ~31;
			int AlignedHeight = (Height + 31) & ~31;
			int LogBpp = XGLog2LE16(TexelPitch);
			int Macro = ((z >> 2) * (AlignedHeight >> 4) + (y >> 4)) * (AlignedWidth >> 5) + (x >> 5);
			int Micro = (((y & 6) << 2) + (x & 7)) << LogBpp;
			long Offset1 = (long)(((long)Macro << (8 + LogBpp)) + ((long)(Micro & ~15) << 1) + (long)(Micro & 15) + ((long)(z & 3) << (6 + LogBpp)) + ((long)(y & 1) << 4));
			long Offset2 = (long)(((z >> 2) + (y >> 3)) & 1);

			return (int)((((Offset1 & ~511L) << 3) + ((Offset1 & 448L) << 2) + (Offset1 & 63L) + 
					(Offset2 << 11) + ((((Offset2 << 1) + (x >> 3)) & 3L) << 6)) >> LogBpp);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Offset"></param>
		/// <param name="Width"></param>
		/// <param name="TexelPitch"></param>
		/// <param name="OutX"></param>
		/// <param name="OutY"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public void XGAddress2DTiledXY(int Offset, int Width, int TexelPitch, out int OutX, out int OutY)
		{
			OutX = XGAddress2DTiledX(Offset, Width, TexelPitch);
			OutY = XGAddress2DTiledY(Offset, Width, TexelPitch);
		}

		/// <summary>
		/// Translate the address of a surface texel/block from a tiled memory offset 
		/// into a 2D array x coordinate measured in texels/blocks.
		/// </summary>
		/// <param name="Offset">Tiled memory offset in texels/blocks</param>
		/// <param name="Width">Width of the image in texels/blocks</param>
		/// <param name="TexelPitch">Size of an image texel/block in bytes</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public int XGAddress2DTiledX(int Offset, int Width, int TexelPitch)
		{
			int AlignedWidth = (Width + 31) & ~0x1F;

			int LogBpp = XGLog2LE16(TexelPitch);
			int OffsetB = Offset << LogBpp;
			int OffsetT = ((OffsetB & ~0xFFF) >> 3) + ((OffsetB & 0x700) >> 2) + (OffsetB & 0x3F);
			int OffsetM = OffsetT >> (7 + LogBpp);

			int MacroX = ((OffsetM % (AlignedWidth >> 5)) << 2);
			int Tile = ((((OffsetT >> (5 + LogBpp)) & 2) + (OffsetB >> 6)) & 3);
			int Macro = (MacroX + Tile) << 3;
			int Micro = ((((OffsetT >> 1) & ~0xF) + (OffsetT & 0xF)) & ((TexelPitch << 3) - 1)) >> LogBpp;

			return Macro + Micro;
		}

		/// <summary>
		/// Translate the address of a surface texel/block from a tiled memory offset 
		/// into a 2D array y coordinate measured in texels/blocks.
		/// </summary>
		/// <param name="Offset">Tiled memory offset in texels/blocks</param>
		/// <param name="Width">Width of the image in texels/blocks</param>
		/// <param name="TexelPitch">Size of an image texel/block in bytes</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public int XGAddress2DTiledY(int Offset, int Width, int TexelPitch)
		{
			int AlignedWidth = (Width + 31) & ~0x1F;

			int LogBpp = XGLog2LE16(TexelPitch);
			int OffsetB = Offset << LogBpp;
			int OffsetT = ((OffsetB & ~0xFFF) >> 3) + ((OffsetB & 0x700) >> 2) + (OffsetB & 0x3F);
			int OffsetM = OffsetT >> (7 + LogBpp);

			int MacroY = ((OffsetM / (AlignedWidth >> 5)) << 2);
			int Tile = ((OffsetT >> (6 + LogBpp)) & 1) + (((OffsetB & 2048) >> 10));
			int Macro = (MacroY + Tile) << 3;
			int Micro = ((((OffsetT & (((TexelPitch << 6) - 1) & ~31)) + ((OffsetT & 0xF) << 1)) >> (3 + LogBpp)) & ~1);

			return Macro + Micro + ((OffsetT & 0x10) >> 4);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Offset"></param>
		/// <param name="Width"></param>
		/// <param name="Height"></param>
		/// <param name="TexelPitch"></param>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		/// <param name="Z"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public void XGAddress3DTiledXYZ(int Offset, int Width, int Height, int TexelPitch, out int X, out int Y, out int Z)
		{
			X = XGAddress3DTiledX(Offset, Width, Height, TexelPitch);
			Y = XGAddress3DTiledY(Offset, Width, Height, TexelPitch);
			Z = XGAddress3DTiledZ(Offset, Width, Height, TexelPitch);
		}

		/// <summary>
		/// Translate the address of a volume texel/block from a tiled memory offset 
		/// into a 3D array x coordinate measured in texels/blocks.
		/// </summary>
		/// <param name="Offset">Tiled memory offset in texels/blocks</param>
		/// <param name="Width">Width of a volume slice in texels/blocks</param>
		/// <param name="Height">Height of a volume slice in texels/blocks</param>
		/// <param name="TexelPitch">Size of a volume texel/block in bytes</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public int XGAddress3DTiledX(int Offset, int Width, int Height, int TexelPitch)
		{
			//XGASSERT(Width <= 2048); // Width in memory must be less than or equal to 2K texels
			//XGASSERT(Height <= 2048); // Height in memory must be less than or equal to 2K texels

			int AlignedWidth = (Width + 31) & ~31;

			int LogBpp = XGLog2LE16(TexelPitch);
			int OffsetB = Offset << LogBpp;
			int OffsetM = ((Offset >> 11) & (~1 >> LogBpp)) + ((OffsetB & 1024) >> (LogBpp + 10));
			int OffsetT = ((((Offset << LogBpp) & ~4095) >> 3) + (((OffsetB & 1792) >> 2) + (OffsetB & 63))) & ((TexelPitch << 6) - 1);
			int Micro = (((OffsetT & ~31) >> 1) + (OffsetT & 15));

			int Macro = OffsetM % (AlignedWidth >> 5);
			int Tile = (((OffsetB & 2048) >> 10) + (OffsetB >> 6)) & 3;

			return (((Macro << 2) + Tile) << 3) + ((Micro >> LogBpp) & 7);
		}

		/// <summary>
		/// Translate the address of a volume texel/block from a tiled memory offset 
		/// into a 3D array y coordinate measured in texels/blocks.
		/// </summary>
		/// <param name="Offset">Tiled memory offset in texels/blocks</param>
		/// <param name="Width">Width of a volume slice in texels/blocks</param>
		/// <param name="Height">Height of a volume slice in texels/blocks</param>
		/// <param name="TexelPitch">Size of a volume texel/block in bytes</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public int XGAddress3DTiledY(int Offset, int Width, int Height, int TexelPitch)
		{
			//XGASSERT(Width <= 2048); // Width in memory must be less than or equal to 2K texels
			//XGASSERT(Height <= 2048); // Height in memory must be less than or equal to 2K texels

			int AlignedWidth = (Width + 31) & ~31;
			int AlignedHeight = (Height + 31) & ~31;

			int LogBpp = XGLog2LE16(TexelPitch);
			int OffsetB = Offset << LogBpp;
			int OffsetM = ((Offset >> 11) & (~1 >> LogBpp)) + ((OffsetB & 1024) >> (LogBpp + 10));
			int OffsetT = ((((Offset << LogBpp) & ~4095) >> 3) + (((OffsetB & 1792) >> 2) + (OffsetB & 63))) & ((TexelPitch << 6) - 1);
			int Micro = (((OffsetT & ~31) >> 1) + (OffsetT & 15));
			int TileZ = (OffsetM << 9) / (AlignedWidth * AlignedHeight);

			int Macro = (OffsetM / (AlignedWidth >> 5)) % (AlignedHeight >> 4);
			int Tile = (((OffsetB & 2048) >> 11) ^ TileZ) & 1;
			Micro = (((Micro & 15) << 1) + (OffsetT & ~31)) >> (LogBpp + 3);

			return (((Macro << 1) + Tile) << 3) + (Micro & ~1) + ((OffsetT & 16) >> 4);
		}

		/// <summary>
		/// Translate the address of a volume texel/block from a tiled memory offset 
		/// into a 3D array z coordinate measured in texels/blocks.
		/// </summary>
		/// <param name="Offset">Tiled memory offset in texels/blocks</param>
		/// <param name="Width">Width of a volume slice in texels/blocks</param>
		/// <param name="Height">Height of a volume slice in texels/blocks</param>
		/// <param name="TexelPitch">Size of a volume texel/block in bytes</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public int XGAddress3DTiledZ(int Offset, int Width, int Height, int TexelPitch)
		{
			//XGASSERT(Width <= 2048); // Width in memory must be less than or equal to 2K texels
			//XGASSERT(Height <= 2048); // Height in memory must be less than or equal to 2K texels

			int AlignedWidth = (Width + 31) & ~31;
			int AlignedHeight = (Height + 31) & ~31;

			int LogBpp = XGLog2LE16(TexelPitch);
			int OffsetB = Offset << LogBpp;
			int OffsetM = ((Offset >> 11) & (~1 >> LogBpp)) + ((OffsetB & 1024) >> (LogBpp + 10));
			int TileZ = (OffsetM << 9) / (AlignedWidth * AlignedHeight);

			return (((((Offset >> 9) & (~7 >> LogBpp))) + ((OffsetB & 1792) >> (LogBpp + 8))) & 3) + (TileZ << 2);
		}
	}
}
