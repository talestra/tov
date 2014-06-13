using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaTranslationEngine.Components.Parts;

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
            Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("chara.svo",
                Handle1
            );
        }

		private void Handle1()
		{
            Patcher.ProgressHandler.AddProgressLevel("chara.svo", 3, () =>
            {
				Patcher.GameAccessPath("chara.svo", () =>
				{
					//Patcher.GameAccessPath("BTL_COMMON.DAT", () =>
					//{
					//	(new BtlSvo(Patcher)).HandleBattlePackImagesCommon();
					//});

					new Title(Patcher).HandleTitle();
					new GameOver(Patcher).HandleGameOver();
					new Tutorial(Patcher).HandleSkillTutorial();
				});
			});
		}
	}
}
