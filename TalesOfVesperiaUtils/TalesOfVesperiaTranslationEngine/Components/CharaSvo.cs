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

                                Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_TEAM, "E_A101_TITLE_TEAM", "E_A101_TITLE_TEAM_D", "E_A101_TITLE_TEAM_F");
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

                                /*
                                Patcher.GameAccessPath("E_A063", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA01, "SKILL_NA01");
                                    });
                                });

                                Patcher.GameAccessPath("E_A064", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_01, "SKILL_NA02_01");
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_02, "SKILL_NA02_02");
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_03, "SKILL_NA02_03");
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_04, "SKILL_NA02_04");
                                    });
                                });

                                Patcher.GameAccessPath("E_A065", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA03, "SKILL_NA03");
                                    });
                                });
                                */
                            });
                        }, 15);
                    });

                    //TODO: Asegurarse de que metiendo compresión 15 a cada DAT, funcione.
                    //TODO: Nada, que no hay manera de meter esto porque siempre ocupa más que el original
                    /*Patcher.Action("Cooking Tutorial", () =>
                    {
                        Patcher.GameAccessPath("EP_060_040.DAT", () =>
                        {
                            Patcher.GameAccessPath("2", () =>
                            {
                                Patcher.GameAccessPath("E_A057", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA01, "COOK_NA01", "COOK_FR01", "COOK_DE01");
                                        //Patcher.UpdateTxm2DWithEmpty(Txm, "COOK01");
                                    });
                                }, 15);

                                //No hay E_A058, que es donde estaría el COOK_NA02

                                Patcher.GameAccessPath("E_A059", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA03, "COOK_NA03", "COOK_FR03", "COOK_DE03");
                                        //Patcher.UpdateTxm2DWithEmpty(Txm, "COOK03");
                                    });
                                }, 15);

                                Patcher.GameAccessPath("E_A060", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA04, "COOK_NA04", "COOK_FR04", "COOK_DE04");
                                        //Patcher.UpdateTxm2DWithEmpty(Txm, "COOK04");
                                    });
                                }, 15);

                                Patcher.GameAccessPath("E_A061", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA05, "COOK_NA05", "COOK_FR05", "COOK_DE05");
                                        //Patcher.UpdateTxm2DWithEmpty(Txm, "COOK05");
                                    });
                                }, 15);

                                
                                Patcher.GameAccessPath("E_A066", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA01, "COOK_NA01"); });
                                });
                                Patcher.GameAccessPath("E_A068", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA03, "COOK_NA03"); });
                                });
                                Patcher.GameAccessPath("E_A069", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA04, "COOK_NA04"); });
                                });
                                Patcher.GameAccessPath("E_A070", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.COOK_NA05, "COOK_NA05"); });
                                });
                                
                            });
                        }, 15);
                    });*/

                    //El dichoso "Great!" aparece en cada una de las batallas que tienen misión secreta
                    Patcher.Action("Battle texts", () =>
                    {
                        foreach (var DatFileNumber in new[] { "030_080", "110_040", "140_050", "150_170", "170_050", "195_030", "210_090", "270_110", "280_030", "300_070", "340_070", "370_050", "420_080", "440_040", "470_030_1", "490_060_1", "510_050", "510_080", "580_030", "590_030", "640_050", "650_030", "650_050" })
                        {
                            Patcher.GameAccessPath("BTL_EP_" + DatFileNumber + ".DAT/2/E_A028", () =>
                            {
                                Patcher.GameGetTXM("0", "1", (Txm) =>
                                {
                                    Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_USUALBTLFONT01, "E_A034_BTLFONT01_E", "E_A034_BTLFONT01_F", "E_A034_BTLFONT01_G");
                                    Patcher.UpdateTxm2DWithEmpty(Txm, "E_A023_BTLFONT01");
                                });
                            });
                        }
                    });

                    Patcher.Action("High/Low, Even/Odd Minigame", () =>
                    {
                        foreach (var DatFile in new[] { "EP_0670_010.DAT", "POR_C03.DAT" })
                        {
                            Patcher.GameAccessPath(DatFile, () =>
                            {
                                Patcher.GameAccessPath("2", () =>
                                {
                                    Patcher.GameAccessPath("E_MG_STONERESULT01", () =>
                                    {
                                        Patcher.GameGetTXM("0", "1", (Txm) =>
                                        {
                                            Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_MINIGAMEISHI01E, "U_MINIGAMEISHI01E", "U_MINIGAMEISHI01F", "U_MINIGAMEISHI01G");
                                            Patcher.UpdateTxm2DWithEmpty(Txm, "U_MINIGAMEISHI01J");
                                        });
                                    });

                                    Patcher.GameAccessPath("E_MG_STONERESULT02", () =>
                                    {
                                        Patcher.GameGetTXM("0", "1", (Txm) =>
                                        {
                                            Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_MINIGAMEISHI02E, "U_MINIGAMEISHI02E", "U_MINIGAMEISHI02F", "U_MINIGAMEISHI02G");
                                            Patcher.UpdateTxm2DWithEmpty(Txm, "U_MINIGAMEISHI02J");
                                        });
                                    });
                                });
                            }, 15);
                        }
                    });

                    //Se decidió dejar este minijuego en inglés.
                    /*
                    Patcher.Action("Tales of Draspi Minigame", () =>
                    {
                        Patcher.GameAccessPath("EP_0680_010.DAT", () =>
                        {
                            Patcher.GameAccessPath("2", () =>
                            {
                                Patcher.GameAccessPath("E_MG_STG_FONT", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.MG_BS_FONT_128, "MG_BS_FONT");
                                    });
                                });
                            });
                        }, 15);

                        Patcher.GameAccessPath("NAM_C.DAT", () =>
                        {
                            Patcher.GameAccessPath("2", () =>
                            {
                                Patcher.GameAccessPath("E_MG_STG_FONT", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.MG_BS_FONT, "MG_BS_FONT", "MG_BS_FONT_DE", "MG_BS_FONT_FR");
                                        //Patcher.UpdateTxm2DWithEmpty(Txm, "MG_BS_FONT_DE", "MG_BS_FONT_FR");
                                    });
                                }, 15);
                            });
                        }, 15);
                    });*/

                    Patcher.Action("Ba'ul Rings Minigame", () =>
                    {
                        Patcher.GameAccessPath("FIELD.DAT", () =>
                        {
                            Patcher.GameAccessPath("2", () =>
                            {
                                Patcher.GameAccessPath("E_MG_RING_2D", () =>
                                {
                                    Patcher.GameGetTXM("0", "1", (Txm) =>
                                    {
                                        Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_MG_BR, "U_MG_BR", "U_MG_BR_DE", "U_MG_BR_FR");
                                    });
                                }, 15);
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
