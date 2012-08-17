using System;
using System.Collections.Generic;
using System.Drawing;
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
	}
}
