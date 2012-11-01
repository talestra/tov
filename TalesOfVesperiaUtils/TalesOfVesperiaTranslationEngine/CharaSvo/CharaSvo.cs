using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine.CharaSvo
{
	public class CharaSvo : PatcherComponent
	{
		public CharaSvo(Patcher Patcher)
			: base(Patcher)
		{
		}

		public void Handle()
		{
			Patcher.GameAccessPath("chara.svo/TITLE.DAT/2/E_A101_TITLE", () =>
			{
				Patcher.Action("Title Menu", () =>
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
