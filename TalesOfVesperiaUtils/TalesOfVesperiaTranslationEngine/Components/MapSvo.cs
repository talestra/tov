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

            this.Patcher.ProgressHandler.AddProgressLevel("Translating Rooms", RoomCount, () =>
            {
                FileSystem.CopyFile(Patcher.GameFileSystem, "language/scenario_uk.dat", Patcher.TempFS, "scenario_uk.dat");
                var TO8SCEL = new TO8SCEL(Patcher.TempFS.OpenFileRW("scenario_uk.dat"));
                //TO8SCEL.Save(new MemoryStream());
                //Patcher.GameAccessPath("language/scenario_uk.dat", () =>

                Patcher.TempFS.CreateDirectory("SCENARIO_ES", 0777, false);

                Patcher.ParallelForeach("Translating Room", Iterators.IntRange(0, RoomCount - 1), (RoomId) => RoomId.ToString(), (RoomId) =>
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
                            });

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

        private void CompressRooms()
        {
            if (Patcher.TempFS.Exists("scenario_es.dat"))
            {
                return;
            }

            this.Patcher.ProgressHandler.AddProgressLevel("Compressing Rooms", RoomCount, () =>
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
                Patcher.ProgressHandler.AddProgressLevel("Updating scenario_uk.dat", 1, () =>
                {
                    Patcher.GameFileSystem.ReplaceFileWithStream("language/scenario_uk.dat", ScenarioEsStream.Slice(), (Current, Total) =>
                    {
                        Patcher.ProgressHandler.SetLevelProgressTo(Current, Total);
                    });
                });
                Patcher.ProgressHandler.AddProgressLevel("Updating scenario_us.dat", 1, () =>
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
