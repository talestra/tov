using CSharpUtils.Streams;
using System;
using System.IO;
using System.Windows.Forms;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.VirtualFileSystem;

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

            //Make a copy of the ISO
            string OriginalISOPath = ODlg.FileName;
            string TranslatedISOPath = Path.Combine(Path.GetDirectoryName(OriginalISOPath), "Tales of Vesperia [PAL] [Español].iso");
            if(!File.Exists(TranslatedISOPath)) File.Copy(OriginalISOPath, TranslatedISOPath);

            //Load the ISO
            FileStream ToVISO = new FileStream(TranslatedISOPath, FileMode.Open, FileAccess.ReadWrite);
            var Dvd9Xbox360 = new Dvd9Xbox360().Load(ToVISO);
            var Vfs = new Dvd9Xbox360FileSystem(Dvd9Xbox360);

            //Trasteando cosas LOLOLOL
            ExtractFile(Vfs, "/snd/config.bin", Path.GetDirectoryName(OriginalISOPath));
        }

        void ExtractFile(Dvd9Xbox360FileSystem fs, string InputFile, string OutputPath)
        {
            var Stream = fs.OpenFile(InputFile, FileMode.Open);

            byte[] data = new byte[Stream.Length];
            Stream.Read(data, 0, data.Length);

            InputFile = InputFile.TrimStart(new char[] {'/'}).Replace("/", Path.DirectorySeparatorChar.ToString());
            
            string OutputFile = Path.Combine(OutputPath, InputFile);

            if(!Directory.Exists(Path.GetDirectoryName(OutputFile))) Directory.CreateDirectory(Path.GetDirectoryName(OutputFile));
            File.WriteAllBytes(OutputFile, data);
        }
	}
}
