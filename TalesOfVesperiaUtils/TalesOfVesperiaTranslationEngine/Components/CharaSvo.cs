using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine.Components
{
	public class CharaSvo : PatcherComponent
	{
		public CharaSvo(Patcher Patcher)
			: base(Patcher)
		{
		}

        public void Handle()
        {
            Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("chara.svo", Handle1);
        }

		private void Handle1()
		{
            Patcher.ProgressHandler.AddProgressLevel("chara.svo", 3, () =>
            {
				Patcher.GameAccessPath("chara.svo", () =>
				{
					Patcher.GameAccessPath("BTL_COMMON.DAT", () =>
					{
						(new BtlSvo(Patcher)).HandleBattlePackImagesCommon();
					});

					Patcher.Action("Title Menu", () =>
					{
						Patcher.GameAccessPath("TITLE.DAT/2/E_A101_TITLE", () =>
						{
							Patcher.GameGetTXM("90", "91", (Txm) =>
							{
								//Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_P001, "E_A101_TITLE_P001", "E_A101_TITLE_P001_D", "E_A101_TITLE_P001_F");
								Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_P001, "E_A101_TITLE_P001");
								Patcher.UpdateTxm2DWithEmpty(Txm, "E_A101_TITLE_P001_D", "E_A101_TITLE_P001_F");
								Patcher.ProgressHandler.IncrementLevelProgress();
                            
								//Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_PUSH, "E_A101_TITLE_PUSH", "E_A101_TITLE_PUSH_D", "E_A101_TITLE_PUSH_F");
								Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_PUSH, "E_A101_TITLE_PUSH");
								Patcher.UpdateTxm2DWithEmpty(Txm, "E_A101_TITLE_PUSH_D", "E_A101_TITLE_PUSH_F");
								Patcher.ProgressHandler.IncrementLevelProgress();

								Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_CREDIT_E, "E_A101_TITLE_CREDIT_E");
								Patcher.ProgressHandler.IncrementLevelProgress();
							});
						});
					});

					Patcher.Action("Game Over", () =>
					{
						Patcher.GameAccessPath("GAMEOVER.DAT/2/E_A104_GAMEOVER", () =>
						{
							HandleGameOver();
						});
					});

                    Patcher.Action("Skill Tutorial", () =>
                    {
                        Patcher.GameAccessPath("EP_050_010.DAT", () =>
                        {
                            Patcher.GameAccessPath("2", () =>
                            {
                                Patcher.GameAccessPath("E_A031", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA01, "SKILL_NA01");
                                        Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_FR01", "SKILL_DE01");
                                    });
                                });

                                Patcher.GameAccessPath("E_A032", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_01, "SKILL_NA02_01");
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_02, "SKILL_NA02_02");
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_03, "SKILL_NA02_03");
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_04, "SKILL_NA02_04");

                                        Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_FR02_01", "SKILL_DE02_01");
                                        Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_FR02_02", "SKILL_DE02_02");
                                        Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_FR02_03", "SKILL_DE02_03");
                                        Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_FR02_04", "SKILL_DE02_04");
                                    });
                                });

                                Patcher.GameAccessPath("E_A033", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA03, "SKILL_NA03");
                                        Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_FR03", "SKILL_DE03");
                                    });
                                });
                            });
                        }, 15);
                    });

				});
			});
		}

		public void HandleGameOver()
		{
			Patcher.GameGetTXM("10", "11", (Txm) =>
			{
				Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_GAMEOVER_000_E, "U_GAMEOVER_000");
				Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_GAMEOVER_000_E, "U_GAMEOVER_000_D");
				Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_GAMEOVER_000_E, "U_GAMEOVER_000_E");
				Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_GAMEOVER_000_E, "U_GAMEOVER_000_F");
				Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_GAMEOVER_000_E, "U_GAMEOVER_001");
			});
		}
	}
}
