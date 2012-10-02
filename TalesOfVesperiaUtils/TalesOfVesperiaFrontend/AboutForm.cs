using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace TalesOfVesperiaFrontend
{
	public partial class AboutForm : Form
	{
		public AboutForm()
		{
			InitializeComponent();
			webBrowser1.ScrollBarsEnabled = false;
			webBrowser1.Navigate("about:blank");
			webBrowser1.Document.Write(Assembly.GetExecutingAssembly().GetManifestResourceStream("TalesOfVesperiaFrontend.About.html").ReadAllContentsAsString());
		}

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
	}
}
