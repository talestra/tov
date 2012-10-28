using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace TalesOfVesperiaUtils.Imaging
{
	/// <summary>
	/// List of Bitmaps for 3D Textures (that have a set of 2D textures)
	/// </summary>
	public class BitmapList
	{
		/// <summary>
		/// List of bitmaps.
		/// </summary>
		public readonly Bitmap[] Bitmaps;

		/// <summary>
		/// Create a list of bitmaps.
		/// </summary>
		/// <param name="Count"></param>
		public BitmapList(int Count)
		{
			this.Bitmaps = new Bitmap[Count];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="PixelFormat"></param>
		/// <param name="Action"></param>
		public void LockUnlockBits(PixelFormat PixelFormat, Action<BitmapData[]> Action)
		{
			var BitmapDatas = new BitmapData[Bitmaps.Length];

			for (int n = 0; n < Bitmaps.Length; n++)
			{
				BitmapDatas[n] = Bitmaps[n].LockBits(Bitmaps[n].GetFullRectangle(), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			}

			try
			{
				Action(BitmapDatas);
			}
			finally
			{
				for (int n = 0; n < Bitmaps.Length; n++)
				{
					Bitmaps[n].UnlockBits(BitmapDatas[n]);
				}
			}
		}
	}
}
