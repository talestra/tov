using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TalesOfVesperiaUtils.Imaging
{
	public class BitmapList
	{
		public Bitmap[] Bitmaps;

		public BitmapList(int Count)
		{
			this.Bitmaps = new Bitmap[Count];
		}
	}
}
