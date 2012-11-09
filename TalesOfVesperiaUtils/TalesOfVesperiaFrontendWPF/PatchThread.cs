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
using TalesOfVesperiaTranslationEngine;
using TalesOfVesperiaTranslationEngine.Components;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaFrontendWPF
{
    public class PatchThread
    {
        static public void Run(MainWindow MainWindow, Action<ProgressHandler> OnProgress, string OriginalGamePath, string TranslatedGamePath, bool UseJTAGFolder)
        {
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
                    MainWindow.InProgress = true;
                  
                    //throw (new Exception("Test ERROR!"));

                    var Patcher = new Patcher((string)null);
                    var PatchAll = new PatchAll(Patcher);

                    if (!UseJTAGFolder)
                    {
                        var Iso = new Dvd9Xbox360FileSystem(File.OpenRead(OriginalGamePath));
                        PatchAll.CheckFileSystemVesperiaExceptions(Iso);
                    }

                    Patcher.Progress += (ProgressHandler) =>
                    {
                        OnProgress(ProgressHandler);
                    };

                    if (!UseJTAGFolder)
                    {
                        Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("Preparing for patching", () =>
                        {
                            Patcher.ProgressHandler.AddProgressLevel("Copying ISO...", 1, () =>
                            {
                                Console.WriteLine("Copying ISO...");
                                if (!File.Exists(TranslatedGamePath) || (new FileInfo(TranslatedGamePath).Length != new FileInfo(OriginalGamePath).Length))
                                {
                                    using (var In = File.OpenRead(OriginalGamePath))
                                    using (var Out = File.Open(TranslatedGamePath, FileMode.Create, FileAccess.Write, FileShare.None))
                                    {
                                        Out.WriteStream(In, (Current, Total) =>
                                        {
                                            //Console.WriteLine("{0}%", ((double)Current / (double)Total) * 100);
                                            Patcher.ProgressHandler.SetLevelProgressTo(Current, Total);
                                        });
                                    }
                                }
                                Console.WriteLine("Done");
                            });
                        });
                    }

                    Patcher.InitWithGamePath(TranslatedGamePath);
                    PatchAll.CheckFileSystemVesperiaExceptions(Patcher.GameFileSystem);
                    PatchAll.Handle();

                    MessageBox.Show("¡Ya se ha traducido Tales of Vesperia al español!", "¡Felicidades!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception Exception)
                {
                    Console.Error.WriteLine(Exception);
                    MessageBox.Show(Exception.ToString(), "Hubo un error. Se ha borrado System32 sin querer.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    MainWindow.InProgress = false;
                }
            });
        }
    }
}
