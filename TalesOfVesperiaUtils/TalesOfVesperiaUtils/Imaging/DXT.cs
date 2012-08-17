using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils.Drawing;
using TalesOfVesperiaUtils.Imaging.Internal;

namespace TalesOfVesperiaUtils.Imaging
{
	unsafe abstract public class DXT<TBlock> where TBlock : struct
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Bitmap"></param>
		/// <param name="File"></param>
		/// <param name="mode"></param>
		public void SaveSwizzled2D(Bitmap Bitmap, Stream File, CompressDXT5.CompressionMode mode = CompressDXT5.CompressionMode.Normal)
		{
			int Width = Bitmap.Width, Height = Bitmap.Height;
			if ((Width % 4) != 0 || (Height % 4) != 0) throw (new InvalidDataException());

			Bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
			{
				var Base = (ARGB_Rev*)BitmapData.Scan0.ToPointer();

				int BlockWidth = Width / 4;
				int BlockHeight = Height / 4;
				var BlockCount = BlockWidth * BlockHeight;
				var CurrentDecodedColors = new ARGB_Rev[16];
				var Blocks = new TBlock[(uint)BlockCount];

				for (int dxt5_n = 0; dxt5_n < BlockCount; dxt5_n++)
				{
					int TileX, TileY;
					Swizzling.XGAddress2DTiledXY(dxt5_n, BlockWidth, 16, out TileX, out TileY);

					int PositionX = TileX * 4;
					int PositionY = TileY * 4;
					int n = 0;

					if ((PositionX + 3 >= Width) || (PositionY + 3 >= Height))
					{
						Console.Error.WriteLine("Warning SaveSwizzled2D ({0}, {1})!", PositionX, PositionY);
						continue;
					}

					for (int y = 0; y < 4; y++)
					{
						for (int x = 0; x < 4; x++)
						{
							CurrentDecodedColors[n] = Base[(PositionY + y) * Width + (PositionX + x)];
							n++;
						}
					}

					EncodeBlock(ref Blocks[dxt5_n], ref CurrentDecodedColors, mode);
				}

				File.WriteStructVector(Blocks);
				File.Flush();
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Seems to have problems with non-power of two Width/Height s</remarks>
		/// <param name="File"></param>
		/// <param name="Width"></param>
		/// <param name="Height"></param>
		/// <param name="Swizzled"></param>
		/// <returns></returns>
		public Bitmap LoadSwizzled2D(Stream File, int Width, int Height, bool Swizzled = true)
		{
			if ((Width % 4) != 0 || (Height % 4) != 0) throw (new InvalidDataException());
			var Bitmap = new Bitmap(Width, Height);
			Bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
			{
				var Base = (ARGB_Rev*)BitmapData.Scan0.ToPointer();

				int BlockWidth = Width / 4, BlockHeight = Height / 4;
				var BlockCount = BlockWidth * BlockHeight;
				var CurrentDecodedColors = new ARGB_Rev[16];
				var Blocks = File.ReadStructVector<TBlock>((uint)BlockCount);

				for (int dxt5_n = 0; dxt5_n < BlockCount; dxt5_n++)
				{
					int TileX, TileY;
					if (Swizzled)
					{
						Swizzling.XGAddress2DTiledXY(dxt5_n, BlockWidth, 16, out TileX, out TileY);
					}
					else
					{
						TileX = dxt5_n % BlockWidth;
						TileY = dxt5_n / BlockWidth;
					}

					DecodeBlock(ref Blocks[dxt5_n], ref CurrentDecodedColors);

					int PositionX = TileX * 4;
					int PositionY = TileY * 4;
					int n = 0;

					if ((PositionX + 3 >= Bitmap.Width) || (PositionY + 3 >= Bitmap.Height))
					{
						Console.Error.Write("Warning!");
						continue;
					}

					for (int y = 0; y < 4; y++)
					{
						for (int x = 0; x < 4; x++)
						{
							Base[(PositionY + y) * Width + (PositionX + x)] = CurrentDecodedColors[n];
							n++;
						}
					}
				}
			});

			return Bitmap;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="File"></param>
		/// <param name="Width"></param>
		/// <param name="Height"></param>
		/// <param name="Depth"></param>
		/// <param name="Swizzled"></param>
		/// <returns></returns>
		public BitmapList LoadSwizzled3D(Stream File, int Width, int Height, int Depth, bool Swizzled = true)
		{
			if ((Width % 4) != 0 || (Height % 4) != 0) throw (new InvalidDataException());

			//Width *= 4;
			//Height *= 4;

			var BitmapList = new BitmapList(Depth);
			var BitmapListData = new BitmapData[Depth];
			var BitmapListPointers = new ARGB_Rev*[Depth];
			for (int n = 0; n < Depth; n++)
			{
				BitmapList.Bitmaps[n] = new Bitmap(Width, Height);
			}

			for (int n = 0; n < Depth; n++)
			{
				var Bitmap = BitmapList.Bitmaps[n];
				BitmapListData[n] = Bitmap.LockBits(Bitmap.GetFullRectangle(), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				BitmapListPointers[n] = (ARGB_Rev*)BitmapListData[n].Scan0.ToPointer();
			}

			int BlockWidth = Width / 4, BlockHeight = Height / 4;
			var BlockCount = BlockWidth * BlockHeight * Depth;
			//var BlockCount = BlockWidth * BlockHeight;
			var CurrentDecodedColors = new ARGB_Rev[16];
			//Console.WriteLine(1);
			var Blocks = File.ReadStructVector<TBlock>((uint)BlockCount);
			//Console.WriteLine(2);

			for (int dxt5_n = 0; dxt5_n < BlockCount; dxt5_n++)
			{
				int TileX, TileY;
				int Z = 0;

				if (Swizzled)
				{
					//Swizzling.XGAddress3DTiledXYZ(dxt5_n, BlockWidth, BlockHeight, GpuUtils.GetBitsPerPixelForEnum(GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5), out TileX, out TileY, out Z);
					Swizzling.XGAddress3DTiledXYZ(dxt5_n, BlockWidth, BlockHeight, 16, out TileX, out TileY, out Z);

					//Swizzling.XGAddress2DTiledXY(dxt5_n, BlockWidth / 4, GpuUtils.GetBitsPerPixelForEnum(GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5), out TileX, out TileY);
					//Z = 0;
				}
				else
				{
					TileX = dxt5_n % BlockWidth;
					TileY = dxt5_n / BlockWidth;
					Z = 0;
					Console.Error.Write("Not implemented");
				}

				//Z = 0;

				DecodeBlock(ref Blocks[dxt5_n], ref CurrentDecodedColors);

				int PositionX = TileX * 4;
				int PositionY = TileY * 4;

				if ((PositionX + 3 >= Width) || (PositionY + 3 >= Height))
				{
					Console.Error.Write("Warning!");
					continue;
				}

				int n = 0;
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						BitmapListPointers[Z][(PositionY + y) * Width + (PositionX + x)] = CurrentDecodedColors[n];
						n++;
					}
				}
			}

			for (int n = 0; n < Depth; n++)
			{
				BitmapList.Bitmaps[n].UnlockBits(BitmapListData[n]);
			}


			return BitmapList;
		}

		abstract protected void EncodeBlock(ref TBlock Block, ref ARGB_Rev[] Colors, CompressDXT5.CompressionMode CompressionMode);
		abstract protected void DecodeBlock(ref TBlock Block, ref ARGB_Rev[] Colors);
	}
}
