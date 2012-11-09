using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Blend4PatcherAnimation
{
	/// <summary>
	/// Interaction logic for RotatingRing.xaml
	/// </summary>
	public partial class RotatingRing : UserControl
	{
		public RotatingRing()
		{
			this.InitializeComponent();
			VisualStateManager.GoToElementState(this.LayoutRoot, "StartAnimation", true);
		}
	}
}