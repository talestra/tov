using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine.Components.Parts
{
	public class Tutorial
	{
		Patcher Patcher;

		public Tutorial(Patcher Patcher)
		{
			this.Patcher = Patcher;
		}

		public void HandleSkillTutorial()
		{
			Patcher.Action("Skill Tutorial", () =>
			{
				Patcher.GameAccessPath("EP_050_010.DAT", () =>
				{
					Patcher.GameAccessPath("2", () =>
					{
						Patcher.GameAccessPath("E_A031", () => { Patcher.GameGetTXM("TEX_0", (Txm) => {
							Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA01, "SKILL_NA01");
							Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_01", "SKILL_FR01", "SKILL_DE01");
						}); });
						Patcher.GameAccessPath("E_A032", () =>
						{
							Patcher.GameGetTXM("TEX_0", (Txm) =>
							{
								Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_01, "SKILL_NA02_01");
								Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_02, "SKILL_NA02_02");
								Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_03, "SKILL_NA02_03");
								Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA02_04, "SKILL_NA02_04");

								Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_2_1", "SKILL_FR02_01", "SKILL_DE02_01");
								Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_2_2", "SKILL_FR02_02", "SKILL_DE02_02");
								Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_2_3", "SKILL_FR02_03", "SKILL_DE02_03");
								Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_2_4", "SKILL_FR02_04", "SKILL_DE02_04");
							});
						});
						Patcher.GameAccessPath("E_A033", () => { Patcher.GameGetTXM("TEX_0", (Txm) => {
							Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.SKILL_NA03, "SKILL_NA03");
							Patcher.UpdateTxm2DWithEmpty(Txm, "SKILL_03", "SKILL_FR03", "SKILL_DE03");
						}); });
					});
				}, 15);
			});
		}
	}
}
