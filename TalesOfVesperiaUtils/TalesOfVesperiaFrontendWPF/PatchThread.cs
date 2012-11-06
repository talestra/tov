using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Forms;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.IO;

namespace TalesOfVesperiaFrontendWPF
{
    public class PatchThread
    {
        static public void Run(string OriginalGamePath, string TranslatedGamePath, bool UseJTAGFolder = false)
        {
#if false
            //string OriginalPath = GamePath;
            //string TranslatedISOPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(OriginalPath), "Tales of Vesperia [PAL] [Español].iso");

            if (OriginalGamePath == TranslatedGamePath)
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
                    if (!File.Exists(TranslatedGamePath)) File.Copy(OriginalGamePath, TranslatedGamePath);
                    Console.WriteLine("Done");

                    var Patcher = new Patcher(TranslatedGamePath);
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
