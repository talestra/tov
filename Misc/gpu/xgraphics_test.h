#define _X86_
#define __forceinline __inline
#define WINAPI
#define XGASSERT(A)
#define UNREFERENCED_PARAMETER(A)
typedef unsigned int UINT;
typedef unsigned long long int UINT64;

//------------------------------------------------------------------------
// Calculate the log2 of a texel pitch which is less than or equal to 16 
// and a power of 2.

__forceinline UINT WINAPI XGLog2LE16(
    UINT TexelPitch)
{
    XGASSERT((TexelPitch > 0) && 
             (TexelPitch <= 16) && 
             (TexelPitch & (TexelPitch - 1)) == 0);

#if defined(_X86_) || defined(_X64_)
    return (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
#else
    return 31 - _CountLeadingZeros(TexelPitch);
#endif
}

//------------------------------------------------------------------------
// Translate the address of a surface texel/block from 2D array coordinates into 
// a tiled memory offset measured in texels/blocks.

__inline UINT WINAPI XGAddress2DTiledOffset(
    UINT x,             // x coordinate of the texel/block
    UINT y,             // y coordinate of the texel/block
    UINT Width,         // Width of the image in texels/blocks
    UINT TexelPitch     // Size of an image texel/block in bytes
    )
{
    UINT AlignedWidth;
    UINT LogBpp;
    UINT Macro;
    UINT Micro;
    UINT Offset;

    XGASSERT(Width <= 8192); // Width in memory must be less than or equal to 8K texels
    XGASSERT(x < Width);

    AlignedWidth = (Width + 31) & ~31;
    LogBpp       = XGLog2LE16(TexelPitch);
    Macro        = ((x >> 5) + (y >> 5) * (AlignedWidth >> 5)) << (LogBpp + 7);
    Micro        = (((x & 7) + ((y & 6) << 2)) << LogBpp);
    Offset       = Macro + ((Micro & ~15) << 1) + (Micro & 15) + ((y & 8) << (3 + LogBpp)) + ((y & 1) << 4);

    return (((Offset & ~511) << 3) + ((Offset & 448) << 2) + (Offset & 63) + 
            ((y & 16) << 7) + (((((y & 8) >> 2) + (x >> 3)) & 3) << 6)) >> LogBpp;
}

//------------------------------------------------------------------------
// Determine the amount of memory occupied by a tiled 2D surface to the 
// granularity of a matte (subtile).  The returned size refers to the
// largest tiled offset potentially referenced in the surface and is 
// measured in texels/blocks.

UINT WINAPI XGAddress2DTiledExtent(
    UINT Width,         // Width of the image in texels/blocks
    UINT Height,        // Height of the image in texels/blocks
    UINT TexelPitch     // Size of an image texel/block in bytes
    );


//------------------------------------------------------------------------
// Translate the address of a volume texel/block from 3D array coordinates into 
// a tiled memory offset measured in texels/blocks.

__inline UINT WINAPI XGAddress3DTiledOffset(
    UINT x,             // x coordinate of the texel/block
    UINT y,             // y coordinate of the texel/block
    UINT z,             // z coordinate of the texel/block
    UINT Width,         // Width of a volume slice in texels/blocks
    UINT Height,        // Height of a volume slice in texels/blocks
    UINT TexelPitch     // Size of a volume texel/block in bytes
    )
{
    UINT AlignedWidth;
    UINT AlignedHeight;
    UINT LogBpp;
    UINT Macro;
    UINT Micro;
    UINT64 Offset1;
    UINT64 Offset2;

    XGASSERT(Width <= 2048); // Width in memory must be less than or equal to 2K texels
    XGASSERT(Height <= 2048); // Height in memory must be less than or equal to 2K texels
    XGASSERT(x < Width);
    XGASSERT(y < Height);

    AlignedWidth  = (Width + 31) & ~31;
    AlignedHeight = (Height + 31) & ~31;
    LogBpp        = XGLog2LE16(TexelPitch);
    Macro         = ((z >> 2) * (AlignedHeight >> 4) + (y >> 4)) * (AlignedWidth >> 5) + (x >> 5);
    Micro         = (((y & 6) << 2) + (x & 7)) << LogBpp;
    Offset1       = (UINT64)(((UINT64)Macro << (8 + LogBpp)) + ((UINT64)(Micro & ~15) << 1) + (UINT64)(Micro & 15) + ((UINT64)(z & 3) << (6 + LogBpp)) + ((UINT64)(y & 1) << 4));
    Offset2       = (UINT64)(((z >> 2) + (y >> 3)) & 1);

    return (UINT)((((Offset1 & ~511ull) << 3ull) + ((Offset1 & 448ull) << 2ull) + (Offset1 & 63ull) + 
            (Offset2 << 11ull) + ((((Offset2 << 1ull) + (x >> 3)) & 3ull) << 6ull)) >> LogBpp);
}

//------------------------------------------------------------------------
// Determine the amount of memory occupied by a tiled 3D volume to the 
// granularity of a matte (subtile). The returned size refers to the
// largest tiled offset potentially referenced in the volume and is 
// measured in texels/blocks.

UINT WINAPI XGAddress3DTiledExtent(
    UINT Width,         // Width of a volume slice in texels/blocks
    UINT Height,        // Height of a volume slice in texels/blocks
    UINT Depth,         // Depth of a volume slice in texels/blocks
    UINT TexelPitch     // Size of a volume texel/block in bytes
    );

//------------------------------------------------------------------------
// Translate the address of a surface texel/block from a tiled memory offset 
// into a 2D array x coordinate measured in texels/blocks.

__inline UINT WINAPI XGAddress2DTiledX(
    UINT Offset,        // Tiled memory offset in texels/blocks
    UINT Width,         // Width of the image in texels/blocks
    UINT TexelPitch     // Size of an image texel/block in bytes
    )
{
    UINT AlignedWidth;
    UINT LogBpp;
    UINT OffsetB;
    UINT OffsetT;
    UINT OffsetM;
    UINT Tile;
    UINT Macro;
    UINT Micro;
    UINT MacroX;

    XGASSERT(Width <= 8192); // Width in memory must be less than or equal to 8K texels

    AlignedWidth = (Width + 31) & ~31;

    LogBpp       = XGLog2LE16(TexelPitch);
    OffsetB      = Offset << LogBpp;
    OffsetT      = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
    OffsetM      = OffsetT >> (7 + LogBpp);

    MacroX       = ((OffsetM % (AlignedWidth >> 5)) << 2);
    Tile         = ((((OffsetT >> (5 + LogBpp)) & 2) + (OffsetB >> 6)) & 3);
    Macro        = (MacroX + Tile) << 3;
    Micro        = ((((OffsetT >> 1) & ~15) + (OffsetT & 15)) & ((TexelPitch << 3) - 1)) >> LogBpp;

    return Macro + Micro;
}

//------------------------------------------------------------------------
// Translate the address of a surface texel/block from a tiled memory offset 
// into a 2D array y coordinate measured in texels/blocks.

__inline UINT WINAPI XGAddress2DTiledY(
    UINT Offset,        // Tiled memory offset in texels/blocks
    UINT Width,         // Width of the image in texels/blocks
    UINT TexelPitch     // Size of an image texel/block in bytes
    )
{
    UINT AlignedWidth;
    UINT LogBpp;
    UINT OffsetB;
    UINT OffsetT;
    UINT OffsetM;
    UINT Tile;
    UINT Macro;
    UINT Micro;
    UINT MacroY;

    XGASSERT(Width <= 8192); // Width in memory must be less than or equal to 8K texels

    AlignedWidth = (Width + 31) & ~31;

    LogBpp       = XGLog2LE16(TexelPitch);
    OffsetB      = Offset << LogBpp;
    OffsetT      = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
    OffsetM      = OffsetT >> (7 + LogBpp);

    MacroY       = ((OffsetM / (AlignedWidth >> 5)) << 2);
    Tile         = ((OffsetT >> (6 + LogBpp)) & 1) + (((OffsetB & 2048) >> 10));
    Macro        = (MacroY + Tile) << 3;
    Micro        = ((((OffsetT & (((TexelPitch << 6) - 1) & ~31)) + ((OffsetT & 15) << 1)) >> (3 + LogBpp)) & ~1);

    return Macro + Micro + ((OffsetT & 16) >> 4);
}

//------------------------------------------------------------------------
// Translate the address of a volume texel/block from a tiled memory offset 
// into a 3D array x coordinate measured in texels/blocks.

__inline UINT WINAPI XGAddress3DTiledX(
    UINT Offset,        // Tiled memory offset in texels/blocks
    UINT Width,         // Width of a volume slice in texels/blocks
    UINT Height,        // Height of a volume slice in texels/blocks
    UINT TexelPitch     // Size of a volume texel/block in bytes
    )
{
    UINT AlignedWidth;
    UINT LogBpp;
    UINT OffsetB;
    UINT OffsetT;
    UINT OffsetM;
    UINT Micro;
    UINT Macro;
    UINT Tile;

    XGASSERT(Width <= 2048); // Width in memory must be less than or equal to 2K texels
#if DBG
    XGASSERT(Height <= 2048); // Height in memory must be less than or equal to 2K texels
#else
    UNREFERENCED_PARAMETER(Height);
#endif

    AlignedWidth = (Width + 31) & ~31;

    LogBpp       = XGLog2LE16(TexelPitch);
    OffsetB      = Offset << LogBpp;
    OffsetM      = ((Offset >> 11) & (~1 >> LogBpp)) + ((OffsetB & 1024) >> (LogBpp + 10));
    OffsetT      = ((((Offset << LogBpp) & ~4095) >> 3) + (((OffsetB & 1792) >> 2) + (OffsetB & 63))) & ((TexelPitch << 6) - 1);
    Micro        = (((OffsetT & ~31) >> 1) + (OffsetT & 15));

    Macro        = OffsetM % (AlignedWidth >> 5);
    Tile         = (((OffsetB & 2048) >> 10) + (OffsetB >> 6)) & 3;

    return (((Macro << 2) + Tile) << 3) + ((Micro >> LogBpp) & 7);
}

//------------------------------------------------------------------------
// Translate the address of a volume texel/block from a tiled memory offset 
// into a 3D array y coordinate measured in texels/blocks.

__inline UINT WINAPI XGAddress3DTiledY(
    UINT Offset,        // Tiled memory offset in texels/blocks
    UINT Width,         // Width of a volume slice in texels/blocks
    UINT Height,        // Height of a volume slice in texels/blocks
    UINT TexelPitch     // Size of a volume texel/block in bytes
    )
{
    UINT AlignedWidth;
    UINT AlignedHeight;
    UINT LogBpp;
    UINT OffsetB;
    UINT OffsetT;
    UINT OffsetM;
    UINT Micro;
    UINT Macro;
    UINT Tile;
    UINT TileZ;

    XGASSERT(Width <= 2048); // Width in memory must be less than or equal to 2K texels
    XGASSERT(Height <= 2048); // Height in memory must be less than or equal to 2K texels

    AlignedWidth = (Width + 31) & ~31;
    AlignedHeight = (Height + 31) & ~31;

    LogBpp       = XGLog2LE16(TexelPitch);
    OffsetB      = Offset << LogBpp;
    OffsetM      = ((Offset >> 11) & (~1 >> LogBpp)) + ((OffsetB & 1024) >> (LogBpp + 10));
    OffsetT      = ((((Offset << LogBpp) & ~4095) >> 3) + (((OffsetB & 1792) >> 2) + (OffsetB & 63))) & ((TexelPitch << 6) - 1);
    Micro        = (((OffsetT & ~31) >> 1) + (OffsetT & 15));
    TileZ        = (OffsetM << 9) / (AlignedWidth * AlignedHeight);

    Macro        = (OffsetM / (AlignedWidth >> 5)) % (AlignedHeight >> 4);
    Tile         = (((OffsetB & 2048) >> 11) ^ TileZ) & 1;
    Micro        = (((Micro & 15) << 1) + (OffsetT & ~31)) >> (LogBpp + 3);

    return (((Macro << 1) + Tile) << 3) + (Micro & ~1) + ((OffsetT & 16) >> 4);
}

//------------------------------------------------------------------------
// Translate the address of a volume texel/block from a tiled memory offset 
// into a 3D array z coordinate measured in texels/blocks.

__inline UINT WINAPI XGAddress3DTiledZ(
    UINT Offset,        // Tiled memory offset in texels/blocks
    UINT Width,         // Width of a volume slice in texels/blocks
    UINT Height,        // Height of a volume slice in texels/blocks
    UINT TexelPitch     // Size of a volume texel/block in bytes
    )
{
    UINT AlignedWidth;
    UINT AlignedHeight;
    UINT LogBpp;
    UINT OffsetB;
    UINT OffsetM;
    UINT TileZ;

    XGASSERT(Width <= 2048); // Width in memory must be less than or equal to 2K texels
    XGASSERT(Height <= 2048); // Height in memory must be less than or equal to 2K texels

    AlignedWidth = (Width + 31) & ~31;
    AlignedHeight = (Height + 31) & ~31;

    LogBpp       = XGLog2LE16(TexelPitch);
    OffsetB      = Offset << LogBpp;
    OffsetM      = ((Offset >> 11) & (~1 >> LogBpp)) + ((OffsetB & 1024) >> (LogBpp + 10));
    TileZ        = (OffsetM << 9) / (AlignedWidth * AlignedHeight);

    return (((((Offset >> 9) & (~7 >> LogBpp))) + ((OffsetB & 1792) >> (LogBpp + 8))) & 3) + (TileZ << 2);
}
