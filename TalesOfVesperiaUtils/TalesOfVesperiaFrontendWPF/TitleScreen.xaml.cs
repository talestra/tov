﻿using System;
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

namespace TalesOfVesperiaFrontendWPF
{
	/// <summary>
	/// Interaction logic for MainControl.xaml
	/// </summary>
	public partial class TitleScreen
	{
		public TitleScreen()
		{
			this.InitializeComponent();
			//this.RotatingRing.Children.Add(new RotatingRing());

            VisualStateManager.GoToElementState(this.LayoutRoot, "StartAnimation", true);
		}
	}
}