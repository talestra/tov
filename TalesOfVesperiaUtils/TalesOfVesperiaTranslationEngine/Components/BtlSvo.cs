using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Compression;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.Formats.Script;
using TalesOfVesperiaUtils.Imaging;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaTranslationEngine.Components
{
	public class BtlSvo : PatcherComponent
	{
		public BtlSvo(Patcher Patcher) : base(Patcher)
		{
		}

		public void Handle()
		{
            Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("btl.svo",
                Handle1,
                Handle2,
                Handle3
            );
		}

        private void Handle1()
        {
            if (!Patcher.TempFS.Exists("BTL_PACK_UK.DAT"))
            {
                Patcher.GameAccessPath("btl.svo", () =>
                {
                    Patcher.GameGetFile("BTL_PACK_UK.DAT", (PackStream) =>
                    {
                        Patcher.TempFS.WriteAllBytes("BTL_PACK_UK.DAT", PackStream.ReadAll());
                    });
                });
            }
        }

        private void Handle2()
        {
            if (!Patcher.TempFS.Exists("BTL_PACK_ES.DAT"))
            {
                //Patcher.TempFS.Copy("BTL_PACK_UK.DAT", "BTL_PACK_ES.DAT", true);

                Patcher.TempFS.CreateDirectory("BTL_PACK", 0777, false);

                FileSystem.CopyTree(new FPS4FileSystem(Patcher.TempFS.OpenFileRead("BTL_PACK_UK.DAT")), "/", Patcher.TempFS, "BTL_PACK");
                Patcher.TempFS.CopyFile("BTL_PACK/3", "BTL_PACK/3.ori");

                Patcher.TempFS.OpenFileReadScope("BTL_PACK/3.ori", (OldStream) =>
                {
                    Patcher.TempFS.OpenFileCreateScope("BTL_PACK/3", (NewStream) =>
                    {
                        HandleBattlePackDialogs(OldStream, NewStream);
                    });
                });

                RepackBtlPack();
            }
        }

        public void HandleBattlePackDialogs(Stream OldStream, Stream NewStream)
        {
            FPS4 OldFps4;
            FPS4 NewFps4;

            OldFps4 = new FPS4(OldStream.Slice());
            NewFps4 = new FPS4(OldStream.Slice()); // Intended OldStream

            var TranslatedFiles = new ConcurrentDictionary<string, MemoryStream>();

            var Names = new[]
			{
				"BTL_EP_0070_010", "BTL_EP_030_040", "BTL_EP_030_080", "BTL_EP_0950_010",
				"BTL_EP_0960_020", "BTL_EP_1040_020", "BTL_EP_150_170", "BTL_EP_170_050",
				"BTL_EP_210_090", "BTL_EP_270_110", "BTL_EP_270_110_1", "BTL_EP_340_070",
				"BTL_EP_370_050", "BTL_EP_420_080", "BTL_EP_440_040", "BTL_EP_470_030",
				"BTL_EP_490_060_0", "BTL_EP_490_060_1", "BTL_EP_510_050", "BTL_EP_510_080",
				"BTL_EP_640_050", "BTL_EP_650_030", "BTL_EP_650_050", "BTL_LL_MONSTER",
				"MA_VAL_A_05",
			};

            Patcher.Action("Translating Battle Scripts", () =>
            {
                Patcher.ProgressHandler.AddProgressLevel("Translating Battle Scripts", Names.Length, () =>
                {
                    Patcher.ParallelForeach("Translating", Names, (Name) => Name, (Name) =>
                    {
                        using (var CompressedTssStream = OldFps4[Name].Open())
                        {
                            using (var TssStream = TalesCompression.DecompressStream(CompressedTssStream))
                            {
                                var TssName = Name;
                                var Tss = new TSS().Load(TssStream);
                                Tss.TranslateTexts((Entry) =>
                                {
                                    //if (Entry == null) return;
                                    var TranslationEntry = Patcher.EntriesByRoom["battle/" + TssName][String.Format("{0:X8}", Entry.Id2)];

                                    int TextCount = Entry.Original.Length;

                                    Entry.TranslateWithTranslationEntry(TranslationEntry);

                                    //Console.WriteLine("{0} : {1}", Entry.Translated[1], TranslationEntry.texts.es[1]);
                                });

                                var TranslatedCompressedStream = TalesCompression.CreateFromVersion(Patcher.CompressionVersion, Patcher.CompressionFallback).EncodeFile(Tss.Save());

                                TranslatedFiles[Name] = TranslatedCompressedStream;
                            }
                        }
                        Patcher.ProgressHandler.IncrementLevelProgress();
                    });
                });
            });

            Patcher.Action("Reconstructing Battle Scripts Package", () =>
            {
                NewFps4.ClearAllEntries();
                foreach (var Entry in OldFps4.Entries.Values)
                {
                    var EntryName = Entry.Name;
                    if (TranslatedFiles.ContainsKey(EntryName))
                    {
                        NewFps4.CreateEntry(EntryName, TranslatedFiles[EntryName]);
                    }
                    else
                    {
                        NewFps4.CreateEntry(EntryName, Entry.Open());
                    }
                }

                NewFps4.SaveTo(NewStream, DoAlign: false);
            });
        }

		public void RepackBtlPack()
		{
			//var OldFps4 = new FPS4(OldStream.Slice());
			//var NewFps4 = new FPS4(OldStream.Slice()); // Intended OldStream

			Patcher.Action("Packing BTL_PACK_ES.DAT", () =>
			{
				Patcher.TempFS.OpenFileReadScope("BTL_PACK_UK.DAT", (OldStream) =>
				{
					Patcher.TempFS.OpenFileCreateScope("BTL_PACK_ES.DAT", (NewStream) =>
					{
						var OldFps4 = new FPS4(OldStream.Slice());
						var NewFps4 = new FPS4(OldStream.Slice()); // Intended OldStream

						NewFps4.ClearAllEntries();

						for (int n = 0; n <= 19; n++)
						{
							NewFps4.CreateEntry(String.Format("{0}", n), Patcher.TempFS.OpenFileRead(String.Format("BTL_PACK/{0}", n)));
						}

						NewFps4.SaveTo(NewStream, DoAlign: false);
					});
				});
			});
		}

        private void Handle3()
        {
            Patcher.TempFS.OpenFileRWScope("BTL_PACK_ES.DAT", (BtlPackUkStream) =>
            {
                Patcher.GameSetFileSystem(new FPS4FileSystem(BtlPackUkStream), () =>
                {
                    HandleBattlePackImages();
                });
            });

            var OldBtlSvo = new FPS4(Patcher.GameFileSystem.OpenFileRead("btl.svo"));
            var NewBtlSvo = new FPS4(Patcher.GameFileSystem.OpenFileRead("btl.svo"));
            NewBtlSvo.ClearAllEntries();
            NewBtlSvo.CreateEntry("BTL_EFFECT.DAT", OldBtlSvo["BTL_EFFECT.DAT"].Open());
            var PackEsEntry = NewBtlSvo.CreateEntry("BTL_PACK_ES.DAT", Patcher.TempFS.OpenFileRead("BTL_PACK_ES.DAT"));
            NewBtlSvo.CreateEntry("BTL_PACK_DE.DAT", PackEsEntry);
            NewBtlSvo.CreateEntry("BTL_PACK_FR.DAT", PackEsEntry);
            NewBtlSvo.CreateEntry("BTL_PACK_UK.DAT", PackEsEntry);
            NewBtlSvo.CreateEntry("BTL_PACK_US.DAT", PackEsEntry);
            Patcher.TempFS.OpenFileCreateScope("btl.svo", (NewBtlSvoStream) =>
            {
                NewBtlSvo.SaveTo(NewBtlSvoStream);
            });

            Patcher.GameReplaceFile("btl.svo", Patcher.TempFS.OpenFileRead("btl.svo"));
        }

		public void HandleBattlePackImages()
		{
			Patcher.GameAccessPath("9/BTL_COMMON/2", () =>
			{
				Patcher.Action("Level Up", () =>
				{
					Patcher.GameAccessPath("E_A018", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A018_LVUP_00, "E_A018_LVUP_00"); });
					});
				});

				Patcher.Action("New Skill", () =>
				{
					Patcher.GameAccessPath("E_A019", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A019_NEW_00, "E_A019_NEW_00"); });
					});
				});

				Patcher.Action("New Arte", () =>
				{
					Patcher.GameAccessPath("E_A024", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A024_LEARNING_00, "E_A024_LEARNING_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A024_LEARNING_00_F", "E_A024_LEARNING_00_G"); });
					});
				});

				Patcher.Action("Secret Mission Completed!", () =>
				{
					Patcher.GameAccessPath("E_A027", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A027_MISSION_00, "E_A027_MISSION_00"); });
					});
				});

				Patcher.Action("New Title", () =>
				{
					Patcher.GameAccessPath("E_A062", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A062_TITLE_00, "E_A062_TITLE_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A062_TITLE_00_F", "E_A062_TITLE_00_G"); });
					});
				});
			});
		}
	}
}
