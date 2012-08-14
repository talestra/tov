using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TalesOfVesperiaUtils.Imaging
{
	sealed public class Swizzling
	{
		static public int XGLog2LE16(int TexelPitch)
		{
			return (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
		}

		static public void UnswizzledXY(int Offset, int Width, int TexelPitch, out int OutX, out int OutY)
		{
			OutX = Offset % Width;
			OutY = Offset / Width;
		}

		static public void XGAddress2DTiledXY(int Offset, int Width, int TexelPitch, out int OutX, out int OutY)
		{
			OutX = XGAddress2DTiledX(Offset, Width, TexelPitch);
			OutY = XGAddress2DTiledY(Offset, Width, TexelPitch);
		}

		static public int XGAddress2DTiledX(
			int Offset,        // Tiled memory offset in texels/blocks
			int Width,         // Width of the image in texels/blocks
			int TexelPitch     // Size of an image texel/block in bytes
		)
		{
			int AlignedWidth;
			int LogBpp;
			int OffsetB;
			int OffsetT;
			int OffsetM;
			int Tile;
			int Macro;
			int Micro;
			int MacroX;

			AlignedWidth = (Width + 31) & ~0x1F;

			LogBpp = XGLog2LE16(TexelPitch);
			OffsetB = Offset << LogBpp;
			OffsetT = ((OffsetB & ~0xFFF) >> 3) + ((OffsetB & 0x700) >> 2) + (OffsetB & 0x3F);
			OffsetM = OffsetT >> (7 + LogBpp);

			MacroX = ((OffsetM % (AlignedWidth >> 5)) << 2);
			Tile = ((((OffsetT >> (5 + LogBpp)) & 2) + (OffsetB >> 6)) & 3);
			Macro = (MacroX + Tile) << 3;
			Micro = ((((OffsetT >> 1) & ~0xF) + (OffsetT & 0xF)) & ((TexelPitch << 3) - 1)) >> LogBpp;

			return Macro + Micro;
		}

		static public int XGAddress2DTiledY(
			int Offset,        // Tiled memory offset in texels/blocks
			int Width,         // Width of the image in texels/blocks
			int TexelPitch     // Size of an image texel/block in bytes
			)
		{
			int AlignedWidth;
			int LogBpp;
			int OffsetB;
			int OffsetT;
			int OffsetM;
			int Tile;
			int Macro;
			int Micro;
			int MacroY;

			AlignedWidth = (Width + 31) & ~0x1F;

			LogBpp = XGLog2LE16(TexelPitch);
			OffsetB = Offset << LogBpp;
			OffsetT = ((OffsetB & ~0xFFF) >> 3) + ((OffsetB & 0x700) >> 2) + (OffsetB & 0x3F);
			OffsetM = OffsetT >> (7 + LogBpp);

			MacroY = ((OffsetM / (AlignedWidth >> 5)) << 2);
			Tile = ((OffsetT >> (6 + LogBpp)) & 1) + (((OffsetB & 2048) >> 10));
			Macro = (MacroY + Tile) << 3;
			Micro = ((((OffsetT & (((TexelPitch << 6) - 1) & ~31)) + ((OffsetT & 0xF) << 1)) >> (3 + LogBpp)) & ~1);

			return Macro + Micro + ((OffsetT & 0x10) >> 4);
		}
	}
}
