using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Imaging;

namespace TalesOfVesperiaTranslationEngine.Components
{
	public class EffectSvo : PatcherComponent
	{
		// effects.svo/E_A101_TITLE.DAT/{90,91}
		// 8.E_A027_MISSION_00
		// 8.E_A024_LEARNING_00.png
		// 8.E_A019_NEW_00.png
		// 8.E_A018_LVUP_00.png
		public EffectSvo(Patcher Patcher)
			: base(Patcher)
		{
		}

        public void Handle()
        {
            Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("effect.svo",
                Handle1
            );
        }

        private void Handle1()
		{
			Patcher.GameAccessPath("effect.svo", () =>
			{
				Patcher.Action("Level Up", () =>
				{
					Patcher.GameAccessPath("E_A018.DAT", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A018_LVUP_00, "E_A018_LVUP_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A018_LVUP_00, "E_A018_LVUP_00_F", "E_A018_LVUP_00_G"); });
					});
				});

				Patcher.Action("New Skill", () =>
				{
					Patcher.GameAccessPath("E_A019.DAT", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A019_NEW_00, "E_A019_NEW_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A019_NEW_00, "E_A019_NEW_00_F", "E_A019_NEW_00_G"); });
					});
				});

				Patcher.Action("Secret Mission", () =>
				{
					Patcher.GameAccessPath("E_A027.DAT", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A027_MISSION_00, "E_A027_MISSION_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A027_MISSION_00_F", "E_A027_MISSION_00_G"); });
					});
				});

				Patcher.Action("New Arte", () =>
				{
					Patcher.GameAccessPath("E_A024.DAT", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A024_LEARNING_00, "E_A024_LEARNING_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A024_LEARNING_00_F", "E_A024_LEARNING_00_G"); });
					});
				});

				Patcher.Action("New Title", () =>
				{
					Patcher.GameAccessPath("E_A062.DAT", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A062_TITLE_00, "E_A062_TITLE_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A062_TITLE_00_F", "E_A062_TITLE_00_G"); });
					});
				});

				Patcher.Action("Game Over", () =>
				{
					Patcher.GameAccessPath("E_A104_GAMEOVER.DAT", () =>
					{
						new CharaSvo(Patcher).HandleGameOver();
					});
				});
				

				Patcher.Action("Encounter Types", () =>
				{
					foreach (var DatFile in new[] { "E_A023.DAT", "E_A028.DAT" })
					{
						Patcher.GameAccessPath(DatFile, () =>
						{
							Patcher.GameGetTXM("0", "1", (Txm) =>
							{
								Patcher.UpdateTxm2DWithPng(
									Txm, PatchPaths.U_USUALBTLFONT01,
									"E_A023_BTLFONT01", "E_A034_BTLFONT01_E", "E_A034_BTLFONT01_F", "E_A034_BTLFONT01_G"
								);
							});
						});
					}

					Patcher.GameAccessPath("E_A034.DAT", () =>
					{
						Patcher.GameGetTXM("0", "1", (Txm) =>
						{
							Patcher.UpdateTxm2DWithPng(
								Txm, PatchPaths.U_USUALBTLFONT01,
								"E_A034_BTLFONT01_E"
							);
						});
					});
				});

				Patcher.Action("Cooking Tutorial", () =>
				{
					Patcher.GameAccessPath("E_A057.DAT", () => { Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA01, "COOK_NA01", "COOK_FR01", "COOK_DE01"); }); }, 15);
					Patcher.GameAccessPath("E_A058.DAT", () => { Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA02, "COOK_NA02", "COOK_FR02", "COOK_DE02"); }); }, 15);
					Patcher.GameAccessPath("E_A059.DAT", () => { Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA03, "COOK_NA03", "COOK_FR03", "COOK_DE03"); }); }, 15);
					Patcher.GameAccessPath("E_A060.DAT", () => { Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA04, "COOK_NA04", "COOK_FR04", "COOK_DE04"); }); }, 15);
					Patcher.GameAccessPath("E_A061.DAT", () => { Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA05, "COOK_NA05", "COOK_FR05", "COOK_DE05"); }); }, 15);
				});
				
				Patcher.Action("Skill Tutorial", () =>
				{
					Patcher.GameAccessPath("E_A031.DAT", () => { Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA01, "SKILL_NA01", "SKILL_FR01", "SKILL_DE01"); }); }, 15);
					Patcher.GameAccessPath("E_A032.DAT", () => { Patcher.GameGetTXM("0", "1", (Txm) => {
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_01, "SKILL_NA02_01", "SKILL_FR02_01", "SKILL_DE02_01");
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_02, "SKILL_NA02_02", "SKILL_FR02_02", "SKILL_DE02_02");
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_03, "SKILL_NA02_03", "SKILL_FR02_03", "SKILL_DE02_03");
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_04, "SKILL_NA02_04", "SKILL_FR02_04", "SKILL_DE02_04");
					}); }, 15);
					Patcher.GameAccessPath("E_A033.DAT", () => { Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA03, "SKILL_NA03", "SKILL_FR03", "SKILL_DE03"); }); }, 15);
				});

				Patcher.Action("Title Menu", () =>
				{
					Patcher.GameAccessPath("E_A101_TITLE.DAT", () =>
					{
						Patcher.GameGetTXM("90", "91", (Txm) =>
						{
							Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_P001, "E_A101_TITLE_P001", "E_A101_TITLE_P001_D", "E_A101_TITLE_P001_F");
							Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_PUSH, "E_A101_TITLE_PUSH", "E_A101_TITLE_PUSH_D", "E_A101_TITLE_PUSH_F");
							Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_CREDIT_E, "E_A101_TITLE_CREDIT_E");
						});
					});
				});

				Patcher.Action("E_MG_RING_2D.DAT", () =>
				{
					Patcher.GameAccessPath("E_MG_RING_2D.DAT", () =>
					{
						Patcher.GameGetTXM("0", "1", (Txm) =>
						{
							Patcher.UpdateTxm2DWithPng(Txm, "Images/Minigames/U_MG_BR.png", "U_MG_BR");
						});
					}, 15);
				});
			});
		}
	}
}
