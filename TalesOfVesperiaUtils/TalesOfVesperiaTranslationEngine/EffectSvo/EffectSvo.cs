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
