using CSharpUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
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

            const bool CheckMd5 = true;
            //const bool CheckMd5 = false;

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
                            Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("Preparándose para parchear", () =>
                            {
                                Patcher.ProgressHandler.AddProgressLevel("Comprobando MD5 de la ISO", 1, () =>
                                {
                                    if (CheckMd5)
                                    {
                                        var ExpectedMd5 = "546f74b4e48bb985fcd365496e03bd10";
                                        var ComputedMd5 = Hashing.GetMd5Hash(OriginalGamePath, (Current, Total) =>
                                        {
                                            Patcher.ProgressHandler.SetLevelProgressTo(Current, Total);
                                        });

                                        if (ComputedMd5 != ExpectedMd5)
                                        {
                                            throw (new Exception(
                                                "El md5 de la ISO a parchear no coincide.\n" +
                                                "Si estás intentando reparchear el juego, asegúrate de haber borrado la anterior ISO en español.\n" +
                                                "Se tiene que parchear usando la ISO original PAL, y no se puede usar la ISO en español ni ninguna otra ISO para el reparcheo.\n" +
                                                "Md5 calculado: " + ComputedMd5 + "\n" +
                                                "Md5 esperado: " + ExpectedMd5 + "\n" +
                                            ""));
                                        }
                                    }
                                });
                            }, () =>
                            {
                                if (CheckMd5)
                                {
                                    if (File.Exists(TranslatedGamePath))
                                    {
                                        try { File.Delete(TranslatedGamePath); }
                                        catch { }
                                    }
                                }
                                
                                var TranslatedGameDvdPath = System.IO.Path.ChangeExtension(TranslatedGamePath, ".dvd");

                                //LayerBreak=1913760
                                //s-tov.iso
                                
                                File.WriteAllLines(TranslatedGameDvdPath, new[] {
                                    "LayerBreak=1913760",
                                    System.IO.Path.GetFileName(TranslatedGamePath),
                                });

                                Patcher.ProgressHandler.AddProgressLevel("Copiando ISO", 1, () =>
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

                        if (!Patcher.ExternalPatchData) PatchDataDecompress();

                        Patcher.InitWithGamePath(TranslatedGamePath);
                        PatchAll.CheckFileSystemVesperiaExceptions(Patcher.GameFileSystem);
                        PatchAll.Handle();

                        if (!Patcher.ExternalPatchData) PatchDataDelete();

                        MessageBox.Show("¡Ya se ha traducido Tales of Vesperia al español!", "¡Felicidades!", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
                finally
                {
                    MainWindow.InProgress = false;
                }
            });
        }

        static private void PatchDataDecompress()
        {
            Console.Write("Decompressing \"PatchData.bin\" in temporary folder... ");

            byte[] data;
            byte[] XORBytes = { 0x54, 0x61, 0x4c, 0x65, 0x53, 0x74, 0x52, 0x61, 0x4d, 0x6f, 0x4c, 0x61, 0x4d, 0x61, 0x5a, 0x6f }; //TaLeStRaMoLaMaZo

            using (Stream zip = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("TalesOfVesperiaFrontendWPF.PatchData.bin"))
            {
                data = new byte[zip.Length];
                zip.Read(data, 0, data.Length);
            }

            for (int n = 0; n < data.Length; n++)
            {
                data[n] = (byte)((data[n] << 6) | ((data[n] & 0xc) << 2) | ((data[n] & 0x30) >> 2) | ((data[n] & 0xc0) >> 6));  //78563412 -> 12345678
                data[n] = (byte)(data[n] ^ XORBytes[n % 16]);
            }

            string ZipPath = Path.Combine(Path.GetTempPath(), "PatchData.bin");
            File.WriteAllBytes(ZipPath, data);

            string ExtractPath = Path.Combine(Path.GetTempPath(), "PatchData");
            if (Directory.Exists(ExtractPath)) Directory.Delete(ExtractPath, true);

            ZipFile.ExtractToDirectory(ZipPath, Path.GetTempPath());
            File.Delete(ZipPath);

            Console.WriteLine("OK");
        }

        static private void PatchDataDelete()
        {
            Console.Write("Deleting patch data in temporary folder... ");

            string DataPath = Path.Combine(Path.GetTempPath(), "PatchData");
            if (Directory.Exists(DataPath)) Directory.Delete(DataPath, true);

            Console.WriteLine("OK");
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
