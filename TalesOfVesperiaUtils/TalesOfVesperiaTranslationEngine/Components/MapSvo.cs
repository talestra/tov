using CSharpUtils;
using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Compression;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.Formats.Script;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaTranslationEngine.Components
{
    public class MapSvo : PatcherComponent
    {
        public MapSvo(Patcher Patcher)
            : base(Patcher)
        {
        }

        const string ScenarioTempFileNamePrep = "SCENARIO_ES/";
        const int RoomCount = 1445;

        public void Handle()
        {
            this.Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("map.svo",
                TranslateRooms,
                FixTexts,
                CompressRooms,
                ReinsertFiles
            );
        }

        private void TranslateRooms()
        {
            if (Patcher.TempFS.Exists("scenario_es.dat"))
            {
                return;
            }

            var PasswordString = Patcher.PatcherDataFS.ReadAllBytes("Text/password.txt").GetString(Encoding.UTF8).Trim();
            //Console.WriteLine(PasswordString);
            //Console.ReadKey();

            this.Patcher.ProgressHandler.AddProgressLevel("Traduciendo habitaciones", RoomCount, () =>
            {
                FileSystem.CopyFile(Patcher.GameFileSystem, "language/scenario_uk.dat", Patcher.TempFS, "scenario_uk.dat");
                var TO8SCEL = new TO8SCEL(Patcher.TempFS.OpenFileRW("scenario_uk.dat"));
                //TO8SCEL.Save(new MemoryStream());
                //Patcher.GameAccessPath("language/scenario_uk.dat", () =>

                Patcher.TempFS.CreateDirectory("SCENARIO_ES", 0777, false);

                Patcher.ParallelForeach
                    //Patcher.Foreach
                ("Translating Room", Iterators.IntRange(0, RoomCount - 1), (RoomId) => RoomId.ToString(), (RoomId) =>
                {
                    var ScenarioTempFileName = ScenarioTempFileNamePrep + RoomId;

                    if (!Patcher.TempFS.Exists(ScenarioTempFileName))
                    {
                        if (TO8SCEL.Entries[RoomId].CompressedStream.Length > 0)
                        {
                            var Stream = TO8SCEL.Entries[RoomId].UncompressedStream;

                            var RoomPath = String.Format("scenario/{0:D4}", RoomId);

                            var Tss = new TSS().Load(Stream);
                            Tss.TranslateTexts((Entry) =>
                            {
                                var TextId = String.Format("{0:X8}", Entry.Id);
                                if (Patcher.EntriesByRoom.ContainsKey(RoomPath))
                                {
                                    var TranslationRoom = Patcher.EntriesByRoom[RoomPath];
                                    if (TranslationRoom.ContainsKey(TextId))
                                    {
                                        var TranslationEntry = TranslationRoom[TextId];

                                        Entry.TranslateWithTranslationEntry(TranslationEntry);
                                    }
                                    else
                                    {
                                        Console.Error.WriteLine("Missing Translation {0} : {1:X8} : {2:X8} : {3:X8}", RoomPath, Entry.Id, Entry.Id2, Entry.Id3);
                                    }
                                }
                                else
                                {
                                    Console.Error.WriteLine("Missing Room");
                                }
                            }, (Text) =>
                            {
                                if (RoomId == 192)
                                {
                                    //Console.WriteLine("{0}", Text);
                                    if (Text == "sun") return PasswordString;
                                }
                                if (RoomId == 31 || RoomId == 35)
                                {
                                    //Console.WriteLine("{0}", Text);
                                    if (Text == "1st") return "1ª";
                                    if (Text == "2nd") return "2ª";
                                    if (Text == "Last") return "Última";
                                    if (Text == "%s Battle") return "%s batalla";
                                    if (Text == "%s?\n") return "¿%s?\n";
                                }
                                if (RoomId == 1256 || RoomId == 1257 || RoomId == 1258 || RoomId == 1259 || RoomId == 1260 || RoomId == 1271 || RoomId == 1272 || RoomId == 1273)
                                {
                                    if (Text == "SELECT") return "Selección";
                                }

                                //Esto no va. Apaño en la función FixTexts()
                                if (RoomId == 344 || RoomId == 1331)
                                {
                                    if (Text == "1st Battle") return "Ronda 1";
                                    if (Text == "2st Battle") return "Ronda 2"; //Sí, en inglés hay una errata.
                                    if (Text == "Last Battle") return "Última ronda";
                                }

                                return null;
                            });

                            //if (RoomId == 192) Console.ReadKey();

                            Patcher.TempFS.WriteAllBytes(ScenarioTempFileName, Tss.Save().ToArray());
                        }
                        else
                        {
                            //Console.WriteLine("CompressedStream.Length = 0");
                        }
                    }

                    this.Patcher.ProgressHandler.IncrementLevelProgress();
                });
                //FileSystem.CopyFile(Patcher.TempFS, "scenario_es.dat", Patcher.TempFS, "scenario_es.dat.finished");
            });
        }

        private void FixTexts() //Perdón por todo esto xd
        {
            string[] Rooms = { "344", "1331" };

            byte[] Ronda1txt = { 0x52, 0x6f, 0x6e, 0x64, 0x61, 0x20, 0x31, 0x00, 0x00, 0x00, 0x00 };
            byte[] Ronda2txt = { 0x52, 0x6f, 0x6e, 0x64, 0x61, 0x20, 0x32, 0x00, 0x00, 0x00, 0x00 };
            byte[] Ronda3txt = { 0x52, 0x6f, 0x6e, 0x64, 0x61, 0x20, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00 };

            uint[] BaseOffset = { 0x57602, 0x3ef22 };

            for (int i = 0; i < Rooms.Length; i++)
            {
                if (Patcher.TempFS.Exists(ScenarioTempFileNamePrep + Rooms[i]))
                {
                    var fs = Patcher.TempFS.OpenFileRW(ScenarioTempFileNamePrep + Rooms[i]);

                    fs.Seek(BaseOffset[i], SeekOrigin.Begin);

                    byte[] CheckData = new byte[10];
                    fs.Read(CheckData, 0, CheckData.Length);
                    if (CheckData[0] != 0x31 || CheckData[1] != 0x73 || CheckData[2] != 0x74 || CheckData[3] != 0x20 || CheckData[4] != 0x42)
                    {
                        fs.Close();
                        return;
                        //throw (new Exception(String.Format("Error fixing High/Low minigame texts in \"{0}\".", ScenarioTempFileNamePrep + Rooms[i])));
                    }

                    fs.Seek(BaseOffset[i], SeekOrigin.Begin);

                    for (int n = 0; n < 3; n++)
                    {
                        fs.Write(Ronda1txt, 0, Ronda1txt.Length);
                        fs.Write(Ronda2txt, 0, Ronda2txt.Length);
                        fs.Write(Ronda3txt, 0, Ronda3txt.Length);
                    }

                    fs.Seek(37, SeekOrigin.Current);
                    fs.Write(Ronda1txt, 0, Ronda1txt.Length);
                    fs.Write(Ronda2txt, 0, Ronda2txt.Length);
                    fs.Write(Ronda3txt, 0, Ronda3txt.Length);

                    fs.Seek(31, SeekOrigin.Current);
                    fs.Write(Ronda1txt, 0, Ronda1txt.Length);
                    fs.Write(Ronda2txt, 0, Ronda2txt.Length);
                    fs.Write(Ronda3txt, 0, Ronda3txt.Length);

                    fs.Close();
                }
            }
        }

        private void CompressRooms()
        {
            if (Patcher.TempFS.Exists("scenario_es.dat"))
            {
                return;
            }

            this.Patcher.ProgressHandler.AddProgressLevel("Comprimiendo habitaciones", RoomCount, () =>
            {
                var NewTO8SCEL = new TO8SCEL();

                Patcher.ParallelForeach("Compressing Room", Iterators.IntRange(0, RoomCount - 1), (RoomId) => RoomId.ToString(), (RoomId) =>
                {
                    this.Patcher.ProgressHandler.IncrementLevelProgress();

                    var ScenarioTempFileName = ScenarioTempFileNamePrep + RoomId;
                    var ScenarioTempFileCompressedName = ScenarioTempFileName + ".c";

                    if (Patcher.TempFS.Exists(ScenarioTempFileName))
                    {
                        if (!Patcher.TempFS.Exists(ScenarioTempFileCompressedName))
                        {
                            var UncompressedBytes = Patcher.TempFS.ReadAllBytes(ScenarioTempFileName);
                            var CompressedBytes = TalesCompression.CreateFromVersion(Patcher.CompressionVersion, Patcher.CompressionVersion).EncodeBytes(UncompressedBytes);
                            Patcher.TempFS.WriteAllBytes(ScenarioTempFileCompressedName, CompressedBytes);
                        }

                        NewTO8SCEL.CreateEntry(
                            RoomId,
                            Patcher.TempFS.OpenFileRead(ScenarioTempFileCompressedName),
                            Patcher.TempFS.OpenFileRead(ScenarioTempFileName)
                        );
                    }
                    else
                    {
                        NewTO8SCEL.CreateEntry(
                            RoomId,
                            new MemoryStream(),
                            new MemoryStream()
                        );
                    }
                });

                Patcher.TempFS.OpenFileCreateScope("scenario_es.dat", (Stream) =>
                {
                    NewTO8SCEL.SaveTo(Stream);
                    Stream.Position = 0;
                });
            });
        }

        private void ReinsertFiles()
        {
            Patcher.TempFS.OpenFileReadScope("scenario_es.dat", (ScenarioEsStream) =>
            {
                Patcher.GameAccessPath("map.svo", () =>
                {
                    Patcher.GameReplaceFile("SCENARIO.DAT", ScenarioEsStream.Slice());
                });

                //Patcher.GameFileSystem.ReplaceFileWithStream("language/scenario_de.dat", ScenarioEsStream.Slice());
                //Patcher.GameFileSystem.ReplaceFileWithStream("language/scenario_fr.dat", ScenarioEsStream.Slice());
                Patcher.ProgressHandler.AddProgressLevel("Actualizando scenario_uk.dat", 1, () =>
                {
                    Patcher.GameFileSystem.ReplaceFileWithStream("language/scenario_uk.dat", ScenarioEsStream.Slice(), (Current, Total) =>
                    {
                        Patcher.ProgressHandler.SetLevelProgressTo(Current, Total);
                    });
                });
                Patcher.ProgressHandler.AddProgressLevel("Actualizando scenario_us.dat", 1, () =>
                {
                    Patcher.GameFileSystem.ReplaceFileWithStream("language/scenario_us.dat", ScenarioEsStream.Slice(), (Current, Total) =>
                    {
                        Patcher.ProgressHandler.SetLevelProgressTo(Current, Total);
                    });
                });
            });
        }

    }
}
