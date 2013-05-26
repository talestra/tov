using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine.Components.Parts
{
	public class Title
	{
		Patcher Patcher;

		public Title(Patcher Patcher)
		{
			this.Patcher = Patcher;
		}

		public void HandleTitle()
		{
			Patcher.Action("Title Menu", () =>
			{
				Patcher.GameAccessPath("TITLE.DAT/2/E_A101_TITLE", () =>
				{
					Patcher.GameGetTXM("TEX_0", (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_P001, "E_A101_TITLE_P001");
						Patcher.UpdateTxm2DWithEmpty(Txm, "E_A101_TITLE_P001_D", "E_A101_TITLE_P001_F");
						Patcher.ProgressHandler.IncrementLevelProgress();

						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_PUSH, "E_A101_TITLE_PUSH");
						Patcher.UpdateTxm2DWithEmpty(Txm, "E_A101_TITLE_PUSH_D", "E_A101_TITLE_PUSH_F");
						Patcher.ProgressHandler.IncrementLevelProgress();

						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_CREDIT_E, "E_A101_TITLE_CREDIT_E");
						Patcher.ProgressHandler.IncrementLevelProgress();
					});
				});
			});
		}
	}
}
