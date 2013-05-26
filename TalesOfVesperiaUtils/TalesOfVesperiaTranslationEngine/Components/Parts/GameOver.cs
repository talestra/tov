using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine.Components.Parts
{
	public class GameOver
	{
		Patcher Patcher;

		public GameOver(Patcher Patcher)
		{
			this.Patcher = Patcher;
		}

		public void HandleGameOver()
		{
			Patcher.Action("Game Over", () =>
			{
				Patcher.GameAccessPath("GAMEOVER.DAT", () =>
				{
					Patcher.GameAccessPath("2", () =>
					{
						Patcher.GameAccessPath("E_A104_GAMEOVER", () =>
						{
							HandleGameOverInternal();
						});
					});
				});
			});
		}

		public void HandleGameOverInternal()
		{
			Patcher.GameGetTXM("TEX_0", (Txm) =>
			{
				Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_GAMEOVER_000_E, "U_GAMEOVER_000_E");
				Patcher.UpdateTxm2DWithEmpty(Txm, "U_GAMEOVER_000", "U_GAMEOVER_000_D", "U_GAMEOVER_000_F", "U_GAMEOVER_001");
			});
		}
	}
}
