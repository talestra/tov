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
using System.Diagnostics;
using System.Security.Cryptography;
using CSharpUtils;

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

            bool CheckMd5 = true;
            //bool CheckMd5 = false;

            var RunTask = Task.Run(() =>
            {
                try
                {
                    MainWindow.InProgress = true;

                    TryCatchIfNotDebugger(() =>
                    {
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
                                //LayerBreak=1913760
                                //s-tov.iso

                                if (CheckMd5)
                                {
                                    if (File.Exists(TranslatedGamePath))
                                    {
                                        try { File.Delete(TranslatedGamePath); }
                                        catch { }
                                    }
                                }

                                var TranslatedGameDvdPath = System.IO.Path.ChangeExtension(TranslatedGamePath, ".dvd");

                                File.WriteAllLines(TranslatedGameDvdPath, new[] {
                                    "LayerBreak=1913760",
                                    System.IO.Path.GetFileName(TranslatedGamePath),
                                });

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
                            }, () =>
                            {
                                Patcher.ProgressHandler.AddProgressLevel("Checking ISO MD5...", 1, () =>
                                {
                                    if (CheckMd5)
                                    {
                                        var ExpectedMd5 = "546f74b4e48bb985fcd365496e03bd10";
                                        var ComputedMd5 = Hashing.GetMd5Hash(TranslatedGamePath, (Current, Total) =>
                                        {
                                            Patcher.ProgressHandler.SetLevelProgressTo(Current, Total);
                                        });

                                        if (ComputedMd5 != ExpectedMd5)
                                        {
                                            throw (new Exception(
                                                "El md5 de la iso a parchear no coincide.\n" +
                                                "Si estás intentando reparchear el juego, asegúrate de haber borrado la iso en español.\n" +
                                                "Se tiene que parchear usando la ISO original PAL, y no se puede usar la iso en español ni ninguna otra iso para el reparcheo\n" +
                                                "Md5 calculado: " + ComputedMd5 + "\n" +
                                                "Md5 esperado: " + ExpectedMd5 + "\n" +
                                            ""));
                                        }
                                    }
                                });
                            });
                        }

                        Patcher.InitWithGamePath(TranslatedGamePath);
                        PatchAll.CheckFileSystemVesperiaExceptions(Patcher.GameFileSystem);
                        PatchAll.Handle();

                        MessageBox.Show("¡Ya se ha traducido Tales of Vesperia al español!", "¡Felicidades!", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
                finally
                {
                    MainWindow.InProgress = false;
                }
            });
        }

        static private void TryCatchIfNotDebugger(Action Action)
        {
            if (Debugger.IsAttached)
            {
                Action();
            }
            else
            {
                try
                {
                    Action();
                }
                catch (Exception Exception)
                {
                    Console.Error.WriteLine(Exception);
                    MessageBox.Show(Exception.ToString(), "Hubo un error. Se ha borrado System32 sin querer.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
