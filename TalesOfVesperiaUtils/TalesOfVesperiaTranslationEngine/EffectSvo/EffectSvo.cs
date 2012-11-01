using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Imaging;

namespace TalesOfVesperiaTranslationEngine.EffectSvo
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
			Patcher.GameAccessPath("effect.svo", () =>
			{
				Patcher.Action("Level Up", () =>
				{
					Patcher.GameAccessPath("E_A018.DAT", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A018_LVUP_00.png", "E_A018_LVUP_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A018_LVUP_00.png", "E_A018_LVUP_00_F", "E_A018_LVUP_00_G"); });
					});
				});

				Patcher.Action("New Skill", () =>
				{
					Patcher.GameAccessPath("E_A019.DAT", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A019_NEW_00.png", "E_A019_NEW_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A019_NEW_00.png", "E_A019_NEW_00_F", "E_A019_NEW_00_G"); });
					});
				});

				Patcher.Action("Secret Mission", () =>
				{
					Patcher.GameAccessPath("E_A027.DAT", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A027_MISSION_00.png", "E_A027_MISSION_00"); });
						//Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A027_MISSION_00_F", "E_A027_MISSION_00_G"); });
					});
				});

				Patcher.Action("New Arte", () =>
				{
					Patcher.GameAccessPath("E_A024.DAT", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A024_LEARNING_00.png", "E_A024_LEARNING_00"); });
						//Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A024_LEARNING_00_F", "E_A024_LEARNING_00_G"); });
					});
				});

				Patcher.Action("New Title", () =>
				{
					Patcher.GameAccessPath("E_A062.DAT", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A062_TITLE_00.png", "E_A062_TITLE_00"); });
						//Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A062_TITLE_00_F", "E_A062_TITLE_00_G"); });
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
									Txm, "COMMON_SVO/TEXTURE_DAT/U_USUALBTLFONT01.png",
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
								Txm, "COMMON_SVO/TEXTURE_DAT/U_USUALBTLFONT01.png",
								"E_A034_BTLFONT01_E"
							);
						});
					});
				});

				Patcher.GameAccessPath("E_A101_TITLE.DAT", () =>
				{
					Patcher.GameGetTXM("90", "91", (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, "EFFECT_SVO/E_A101_TITLE_P001.png", "E_A101_TITLE_P001", "E_A101_TITLE_P001_D", "E_A101_TITLE_P001_F");
						Patcher.UpdateTxm2DWithPng(Txm, "EFFECT_SVO/E_A101_TITLE_PUSH.png", "E_A101_TITLE_PUSH", "E_A101_TITLE_PUSH_D", "E_A101_TITLE_PUSH_F");
						Patcher.UpdateTxm2DWithPng(Txm, "EFFECT_SVO/E_A101_TITLE_CREDIT_E.png", "E_A101_TITLE_CREDIT_E");
					});
				});
			});
		}
	}
}
