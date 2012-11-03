using CSharpUtils.VirtualFileSystem;
using System;
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

namespace TalesOfVesperiaTranslationEngine.BtlSvo
{
	public class BtlSvo : PatcherComponent
	{
		public BtlSvo(Patcher Patcher) : base(Patcher)
		{
		}

		public void Handle()
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

			/*
			Patcher.GameSetFileSystem(new FPS4FileSystem(Patcher.TempFS.OpenFileRW("BTL_PACK_UK.DAT")), () =>
			{
				HandleBattlePackImages();
			});
			*/

			var OldBtlSvo = new FPS4(Patcher.GameFileSystem.OpenFileRead("btl.svo"));
			var NewBtlSvo = new FPS4(Patcher.GameFileSystem.OpenFileRead("btl.svo"));
			NewBtlSvo.ClearAllEntries();
			NewBtlSvo.CreateEntry("BTL_EFFECT.DAT", OldBtlSvo["BTL_EFFECT.DAT"].Open());
			var PackEsEntry = NewBtlSvo.CreateEntry("BTL_PACK_ES.DAT", Patcher.TempFS.OpenFileRead("BTL_PACK_UK.DAT"));
			NewBtlSvo.CreateEntry("BTL_PACK_DE.DAT", PackEsEntry);
			NewBtlSvo.CreateEntry("BTL_PACK_FR.DAT", PackEsEntry);
			NewBtlSvo.CreateEntry("BTL_PACK_UK.DAT", PackEsEntry);
			NewBtlSvo.CreateEntry("BTL_PACK_US.DAT", PackEsEntry);
			NewBtlSvo.Save(Patcher.TempFS.OpenFileCreate("btl.svo"));

			Patcher.GameReplaceFile("btl.svo", Patcher.TempFS.OpenFileRead("btl.svo"));
		}

		public void HandleBattlePack()
		{
			HandleBattlePackDialogs();
			HandleBattlePackImages();

			//Console.Error.WriteLine("/Break Battle"); return;
		}

		public void HandleBattlePackDialogs()
		{
			Patcher.GameAccessPath("3", () =>
			{
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

				foreach (var Name in Names)
				{
					Patcher.GameGetFile(Name, (TssStream) =>
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
						TssStream.Position = 0;
						Tss.SaveTo(TssStream);
					});
				}
			});
		}

		public void HandleBattlePackImages()
		{
			Patcher.GameAccessPath("9/BTL_COMMON/2", () =>
			{
				Patcher.Action("Level Up", () =>
				{
					Patcher.GameAccessPath("E_A018", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A018_LVUP_00.png", "E_A018_LVUP_00"); });
					});
				});

				Patcher.Action("New Skill", () =>
				{
					Patcher.GameAccessPath("E_A019", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A019_NEW_00.png", "E_A019_NEW_00"); });
					});
				});

				Patcher.Action("New Arte", () =>
				{
					Patcher.GameAccessPath("E_A024", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A024_LEARNING_00.png", "E_A024_LEARNING_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A024_LEARNING_00_F", "E_A024_LEARNING_00_G"); });
						//Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A024_LEARNING_00.png", "E_A024_LEARNING_00_F", "E_A024_LEARNING_00_G"); });
					});
				});

				Patcher.Action("Secret Mission Completed!", () =>
				{
					Patcher.GameAccessPath("E_A027", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A027_MISSION_00.png", "E_A027_MISSION_00"); });
					});
				});

				Patcher.Action("New Title", () =>
				{
					Patcher.GameAccessPath("E_A062", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A062_TITLE_00.png", "E_A062_TITLE_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A062_TITLE_00_F", "E_A062_TITLE_00_G"); });
					});
				});
			});
		}
	}
}
