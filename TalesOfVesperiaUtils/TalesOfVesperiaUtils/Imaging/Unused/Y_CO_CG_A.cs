using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Drawing;

namespace TalesOfVesperiaUtils.Imaging
{
	public struct Y_CO_CG_A
	{
		/* 
			RGB <-> YCoCg
    
			Y  = [ 1/4  1/2  1/4] [R]
			Co = [ 1/2    0 -1/2] [G]
			CG = [-1/4  1/2 -1/4] [B]
    
			R  = [   1    1   -1] [Y]
			G  = [   1    0    1] [Co]
			B  = [   1   -1   -1] [Cg]
    
		*/

		/// <summary>
		/// Orange Chromancy
		/// </summary>
		public byte CO;

		/// <summary>
		/// Green Chromancy
		/// </summary>
		public byte CG;

		/// <summary>
		/// Alpha
		/// </summary>
		public byte A;

		/// <summary>
		/// Pseudo Luminance / Intensity
		/// </summary>
		public byte Y;

#if true
		static private byte CLAMP_BYTE(int x) { return (byte)((x) < 0 ? (0) : ((x) > 255 ? 255 : (x))); }

		static private int RGB_TO_YCOCG_Y(ARGB_Rev Color) { return (((Color.R + (Color.G << 1) + Color.B) + 2) >> 2); }
		static private int RGB_TO_YCOCG_CO(ARGB_Rev Color) { return ((((Color.R << 1) - (Color.B << 1)) + 2) >> 2); }
		static private int RGB_TO_YCOCG_CG(ARGB_Rev Color) { return (((-Color.R + (Color.G << 1) - Color.B) + 2) >> 2); }

		static public Y_CO_CG_A ConvertRGBToCoCg_Y(ARGB_Rev Input)
		{
			return new Y_CO_CG_A()
			{
				CO = CLAMP_BYTE(RGB_TO_YCOCG_CO(Input) + 128),
				CG = CLAMP_BYTE(RGB_TO_YCOCG_CG(Input) + 128),
				A = Input.A,
				Y = CLAMP_BYTE(RGB_TO_YCOCG_Y(Input)),
			};
		}

		static public ARGB_Rev ConvertCoCg_YToRGB(Y_CO_CG_A Input)
		{
			int co = Input.CO - 128;
			int cg = Input.CG - 128;
			int a = Input.A;
			int y = Input.Y;
			return new ARGB_Rev()
			{
				R = (byte)CLAMP_BYTE(y + (co - cg)),
				G = (byte)CLAMP_BYTE(y + (cg)),
				B = (byte)CLAMP_BYTE(y + (-co - cg)),
				A = (byte)a,
			};
		}

		static public implicit operator ARGB_Rev(Y_CO_CG_A Input)
		{
			return ConvertCoCg_YToRGB(Input);
		}

		static public implicit operator Y_CO_CG_A(ARGB_Rev Input)
		{
			return ConvertRGBToCoCg_Y(Input);
		}
#else

		static public implicit operator ARGB_Rev(Y_CO_CG_A Input)
		{
			var t = Input.Y - (Input.CG >> 1);
			var G = Input.CG + t;
			var B = t  - (Input.CO >> 1);
			var R = Input.CO + B;
			var A = Input.A;
			return new ARGB_Rev((byte)A, (byte)R, (byte)G, (byte)B);
		}

		static public implicit operator Y_CO_CG_A(ARGB_Rev Input)
		{
			var Co = Input.R - Input.B;
			var t = Input.B + (Co >> 1);
			var Cg = Input.G - t;
			var Y = t + (Cg >> 1);
			var A = Input.A;
			return new Y_CO_CG_A() { CO = (sbyte)Co, CG = (sbyte)Cg, Y = (byte)Y, A = (byte)A };
		}
#endif
	}
}
