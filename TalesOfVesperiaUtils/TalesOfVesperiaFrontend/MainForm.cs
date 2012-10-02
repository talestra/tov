using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TalesOfVesperiaFrontend
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			new AboutForm().ShowDialog();
		}

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ODlg = new OpenFileDialog();
            ODlg.Filter = "Archivos ISO (*.iso)|*.iso|Todos los archivos (*.*)|*.*";
            ODlg.Title = "Elige la ISO de la versión PAL del Tales of Vesperia";
            if (ODlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


        }
	}
}
