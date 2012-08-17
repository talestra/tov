using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Drawing;

namespace TalesOfVesperiaUtils.Imaging
{
	public class CompressionSimpleDXT5
	{
		static private void FindBoundingBox(ARGB_Rev[] Colors, out ARGB_Rev Min, out ARGB_Rev Max)
		{
			Min = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
			Max = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
			
			foreach (var color in Colors)
			{
				Min.R = Math.Min(color.R, Min.R);
				Min.G = Math.Min(color.G, Min.G);
				Min.B = Math.Min(color.B, Min.B);
				//Min.A = Math.Min(color.A, Min.A);

				Max.R = Math.Max(color.R, Max.R);
				Max.G = Math.Max(color.G, Max.G);
				Max.B = Math.Max(color.B, Max.B);
				//Max.A = Math.Max(color.A, Max.A);
			}

			Min.A = 0xFF;
			Max.A = 0xFF;

			Console.WriteLine(Min);
			Console.WriteLine(Max);
		}

		static private void RefineMinMax(ARGB_Rev[] Colors, ref ARGB_Rev Min, ref ARGB_Rev Max)
		{
			int MinListCount = 0;
			int MaxListCount = 0;
			var MinAccum = default(RGBA32);
			var MaxAccum = default(RGBA32);

			foreach (var color in Colors)
			{
				int DistanceMin = ARGB_Rev.DistanceRGB(color, Min);
				int DistanceMax = ARGB_Rev.DistanceRGB(color, Max);

				if (DistanceMin <= DistanceMax)
				{
					MinAccum += color;
					MinListCount++;
				}

				if (DistanceMax <= DistanceMin)
				{
					MaxAccum += color;
					MaxListCount++;
				}
			}

			Min = MinAccum / MinListCount;
			Max = MaxAccum / MaxListCount;
			Min.A = 0xFF;
			Max.A = 0xFF;
		}

		static public ARGB_Rev[] GeneratePallete1(ARGB_Rev C0, ARGB_Rev C1)
		{
			return new ARGB_Rev[]
			{
				C0,
				C1,
				ColorUtils.Mix(C0, C1, 3, 2, 1),
				ColorUtils.Mix(C0, C1, 3, 1, 2),
			};
		}

		static public ARGB_Rev[] GeneratePallete2(ARGB_Rev C0, ARGB_Rev C1)
		{
			return new ARGB_Rev[]
			{
				C0,
				C1,
				new ARGB_Rev(0, 0, 0, 0),
				ColorUtils.Mix(C0, C1, 2, 1, 1),
			};
		}

		static public int ColorToIndexed(ARGB_Rev[] Colors, ARGB_Rev[] Palette, byte[] Indices)
		{
			int TotalDistance = 0;
			for (int n = 0; n < 16; n++)
			{
				var Color = Colors[n];
				int MinDistance = int.MaxValue;
				int BestIndex = 0;
				for (int m = 0; m < 4; m++)
				{
					var Distance = ARGB_Rev.DistanceRGB(Color, Palette[m]);
					if (Distance < MinDistance)
					{
						BestIndex = m;
						MinDistance = Distance;
					}
				}
				TotalDistance += MinDistance;
				Indices[n] = (byte)BestIndex;
			}
			return TotalDistance;
		}

		static public void FindColorPair(ARGB_Rev[] Colors, out ARGB_Rev Min, out ARGB_Rev Max)
		{
			FindBoundingBox(Colors, out Min, out Max);
			RefineMinMax(Colors, ref Min, ref Max);

			var Palette1 = GeneratePallete1(Min, Max);
			var Palette2 = GeneratePallete2(Min, Max);
			byte[] Indices1 = new byte[16];
			byte[] Indices2 = new byte[16];
			int Distance1 = ColorToIndexed(Colors, Palette1, Indices1);
			int Distance2 = ColorToIndexed(Colors, Palette2, Indices2);

			Console.WriteLine(Min);
			Console.WriteLine(Max);

			Console.WriteLine(Distance1);
			Console.WriteLine(Distance2);
		}
	}
}
