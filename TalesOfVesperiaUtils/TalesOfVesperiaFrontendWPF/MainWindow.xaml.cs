﻿using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shell;
using TalesOfVesperiaTranslationEngine;
using Ookii.Dialogs.Wpf;

namespace TalesOfVesperiaFrontendWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _InProgress;
        public bool InProgress
        {
            get
            {
                return _InProgress;
            }
            set
            {
                _InProgress = value;
                if (_InProgress == false)
                {
                    UpdateProgress(null);
                }
                Dispatcher.Invoke(() =>
                {
                    Parchear.IsEnabled = !_InProgress;
                });
            }
        }

        public MainWindow()
        {
            InitializeComponent();
			this.GlassBackground(0, 0, 0, 40);

			var BuildVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			Title = String.Format("Tales of Vesperia en español - v{0}", BuildVersion);
			BuildDate.Content = String.Format("Build {0}", RetrieveLinkerTimestamp());
        }

		private DateTime RetrieveLinkerTimestamp()
		{
			string filePath = Assembly.GetCallingAssembly().Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;
			byte[] b = new byte[2048];
			System.IO.Stream s = null;

			try
			{
				s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				s.Read(b, 0, 2048);
			}
			finally
			{
				if (s != null)
				{
					s.Close();
				}
			}

			int i = BitConverter.ToInt32(b, c_PeHeaderOffset);
			int secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
			DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
			dt = dt.AddSeconds(secondsSince1970);
			dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
			return dt;
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
        {
			Parchear.ContextMenu.PlacementTarget = Parchear;
			Parchear.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			Parchear.ContextMenu.IsOpen = true;
        }

        /*
        void ExtractFile(Dvd9Xbox360FileSystem fs, string InputFile, string OutputPath)
        {
            var Stream = fs.OpenFile(InputFile, FileMode.Open);

            byte[] data = new byte[Stream.Length];
            Stream.Read(data, 0, data.Length);

            InputFile = InputFile.TrimStart(new char[] { '/' }).Replace("/", Path.DirectorySeparatorChar.ToString());

            string OutputFile = Path.Combine(OutputPath, InputFile);

            if (!Directory.Exists(Path.GetDirectoryName(OutputFile))) Directory.CreateDirectory(Path.GetDirectoryName(OutputFile));
            File.WriteAllBytes(OutputFile, data);
        }*/

        DateTime LastUpdateProgressDateTime = DateTime.UtcNow;

        void UpdateProgress(ProgressHandler ProgressHandler)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (ProgressHandler == null)
                {
                    GlobalProgress.Value = 0;
                    GlobalProgress.Maximum = 0;
                    GlobalProgressText.Text = "";

                    LocalProgress.Value = 0;
                    LocalProgress.Maximum = 0;
                    LocalProgressText.Text = "";

                    this.TaskbarItemInfo = new TaskbarItemInfo()
                    {
                        ProgressValue = 0,
                        ProgressState = TaskbarItemProgressState.None,
                    };
                }
                else
                {
                    var GlobalValue = ProgressHandler.GetProcessedLevelProgress(0);
                    var GlobalText = ProgressHandler.GetLevelDescription(0);

                    if (GlobalProgress.Value != GlobalValue)
                    {
                        var CurrentDateTime = DateTime.UtcNow;
                        if ((CurrentDateTime - LastUpdateProgressDateTime).TotalSeconds >= 0.2)
                        {
                            LastUpdateProgressDateTime = CurrentDateTime;
                            //var LocalValue = ProgressHandler.GetProcessedLevelProgress(1);
                            //var LocalText = ProgressHandler.GetLevelDescriptionChain(1);
                            var LocalValue = ProgressHandler.GetProcessedLevelProgress(ProgressHandler.GetCurrentLevelIndex());
                            var LocalText = ProgressHandler.GetLevelDescriptionChain(1);

                            GlobalProgress.Value = GlobalValue;
                            GlobalProgress.Maximum = 1;
                            GlobalProgressText.Text = String.Format("{0} ({1:0}%)", GlobalText, GlobalValue * 100);

                            LocalProgress.Value = LocalValue;
                            LocalProgress.Maximum = 1;
                            LocalProgressText.Text = String.Format("{0} ({1:0}%)", LocalText, LocalValue * 100);

                            this.TaskbarItemInfo = new TaskbarItemInfo()
                            {
                                ProgressValue = GlobalValue,
                                ProgressState = TaskbarItemProgressState.Normal,
                            };
                        }
                    }
                }
            });
        }

		private void ParchearPopup_LostFocus(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("ParchearPopup_LostFocus");
		}

		private void PatchIso_Click_1(object sender, RoutedEventArgs e)
		{
			var Dialog = new OpenFileDialog();
			Dialog.Filter = "Archivos ISO (*.iso)|*.iso|Todos los archivos (*.*)|*.*";
			Dialog.Title = "Elige la ISO de la versión PAL del Tales of Vesperia";
            if (Dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var SDialog = new SaveFileDialog();
            SDialog.Filter = "Archivos ISO (*.iso)|*.iso";
            SDialog.Title = "Elige dónde quieres guardar la versión traducida del Tales of Vesperia";
            SDialog.FileName = "Tales of Vesperia [PAL] [Español].iso";
            if (SDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            PatchThread.Run(this, UpdateProgress, Dialog.FileName, SDialog.FileName, false);
		}

		private void PatchFolder_Click_1(object sender, RoutedEventArgs e)
		{
			//var Dialog = new FolderBrowserDialog();
            var Dialog = new VistaFolderBrowserDialog();
            Dialog.UseDescriptionForTitle = true;
			Dialog.Description = "Elige la carpeta con los archivos del Tales of Vesperia extraídos (para JTAG).";
            Dialog.ShowNewFolderButton = false;
            //if (Dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            if (!(bool)Dialog.ShowDialog()) return;

            PatchThread.Run(this, UpdateProgress, "", Dialog.SelectedPath, true);
		}

        private void AcercaDe_Click(object sender, RoutedEventArgs e)
        {
            new AboutForm().ShowDialog();
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            if (InProgress)
            {
                if (System.Windows.MessageBox.Show("¿Estás seguro de que deseas cancelar el proceso de parcheo?\n\nLa iso generada podría quedar inutilizada.", "Atención", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
