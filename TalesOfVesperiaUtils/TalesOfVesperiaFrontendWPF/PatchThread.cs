using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace TalesOfVesperiaFrontendWPF
{
	public class PatchThread
	{
		static public void Run(string GamePath)
		{
#if false
			//Make a copy of the ISO
			string OriginalISOPath = GamePath;
			string TranslatedISOPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(OriginalISOPath), "Tales of Vesperia [PAL] [Español].iso");

			if (OriginalISOPath == TranslatedISOPath)
			{
				MessageBox.Show("No se puede actualizar el mismo archivo directamente.", "Hubo un error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			var RunTask = Task.Run(() =>
			{
				try
				{
					this.Dispatcher.Invoke(() =>
					{
						this.Parchear.IsEnabled = false;
					});

					//throw (new Exception("Test ERROR!"));

					UpdateProgress(0, 100, TaskbarItemProgressState.Normal);
					Console.WriteLine("Copying ISO...");
					if (!File.Exists(TranslatedISOPath)) File.Copy(OriginalISOPath, TranslatedISOPath);
					Console.WriteLine("Done");

					UpdateProgress(50, 100, TaskbarItemProgressState.Normal);

					var Patcher = new Patcher(TranslatedISOPath);
					Patcher.Progress += (Current, Total) =>
					{
						UpdateProgress(Current, Total, TaskbarItemProgressState.Normal);
					};
					new PatchAll(Patcher).Handle();
					//Load the ISO
					/*
					FileStream ToVISO = new FileStream(TranslatedISOPath, FileMode.Open, FileAccess.ReadWrite);
					var Dvd9Xbox360 = new Dvd9Xbox360().Load(ToVISO);
					var Vfs = new Dvd9Xbox360FileSystem(Dvd9Xbox360);

					//Trasteando cosas LOLOLOL
					ExtractFile(Vfs, "/snd/config.bin", Path.GetDirectoryName(OriginalISOPath));
            
					UpdateProgress(75, TaskbarItemProgressState.Normal);
					*/

					MessageBox.Show("¡Ya se ha traducido Tales of Vesperia al español!", "¡Felicidades!", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				catch (Exception Exception)
				{
					MessageBox.Show(Exception.ToString(), "Hubo un error. Se ha borrado System32 sin querer.", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				finally
				{
					this.Dispatcher.Invoke(() =>
					{
						this.Parchear.IsEnabled = true;
					});
				}
			});
#endif
		}
	}
}
