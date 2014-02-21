using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TalesOfVesperiaFrontendWPF
{
	public static class GlassingExtension
	{
		/// <summary>
		/// Sets glass background to whole window.
		/// </summary>
		/// <remarks>Remember to set your WPF Window Background to "Transparent"!</remarks>
		/// <param name="win"></param>
		public static void GlassBackground(this Window win)
		{
			// Glass extend WINAPI thingie http://msdn.microsoft.com/en-us/library/aa969512%28VS.85%29.aspx form more details
			// If any of the margins is "-1" the whole window is glass!
			win.GlassBackground(-1, 0, 0, 0);
		}
		/// <summary>
		/// Sets glass background to custom margins in the window.
		/// </summary>
		/// <param name="win"></param>
		public static void GlassBackground(this Window win, int left, int right, int top, int bottom)
		{
			// Why would you read the inner workings? Why? If you need to know why...
			// DwmExtendFrameIntoClientArea http://msdn.microsoft.com/en-us/library/aa969512%28VS.85%29.aspx is the magical WINAPI call
			// rest is just crap to get its parameters populated.
			win.Loaded += delegate(object sender, RoutedEventArgs e)
			{
				try
				{
					// Obtain the window handle for WPF application
					IntPtr mainWindowPtr = new WindowInteropHelper(win).Handle;
					HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);

					// Transparent shall be glassed!
					mainWindowSrc.CompositionTarget.BackgroundColor = System.Windows.Media.Colors.Transparent;

					// Margin for the DwmExtendFrameIntoClientArea WINAPI call.
					NonClientRegionAPI.MARGINS margins = new NonClientRegionAPI.MARGINS();
					margins.cxLeftWidth = left;
					margins.cxRightWidth = right;
					margins.cyBottomHeight = bottom;
					margins.cyTopHeight = top;

					// Glass extend WINAPI thingie http://msdn.microsoft.com/en-us/library/aa969512%28VS.85%29.aspx form more details
					int hr = NonClientRegionAPI.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
					if (hr < 0)
					{
						//DwmExtendFrameIntoClientArea Failed
					}
					else
					{
						win.Background = System.Windows.Media.Brushes.Transparent;
					}
				}
				// If not glassing capabilities (Windows XP...), paint background white.
				catch (DllNotFoundException)
				{
					System.Windows.Application.Current.MainWindow.Background = System.Windows.Media.Brushes.White;
				}
			};
		}

		private class NonClientRegionAPI
		{
			[StructLayout(LayoutKind.Sequential)]
			public struct MARGINS
			{
				public int cxLeftWidth;      // width of left border that retains its size
				public int cxRightWidth;     // width of right border that retains its size
				public int cyTopHeight;      // height of top border that retains its size
				public int cyBottomHeight;   // height of bottom border that retains its size
			};


			[DllImport("DwmApi.dll")]
			public static extern int DwmExtendFrameIntoClientArea(
				IntPtr hwnd,
				ref MARGINS pMarInset);

		}
	}

}
