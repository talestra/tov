using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Compression;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.Imaging;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaTranslationEngine.BtlSvo
{
	public class BtlSvo : PatcherComponent
	{
		public BtlSvo(Patcher Patcher) : base(Patcher)
		{
		}

		public void Handle()
		{
			Patcher.GameAccessPath("btl.svo", () =>
			{
				Patcher.GameAccessPath("BTL_PACK_UK.DAT", HandleBattlePack);
				Patcher.GameAccessPath("BTL_PACK_US.DAT", HandleBattlePack);
				//Patcher.GameAccessPath("BTL_PACK_FR.DAT", HandleBattlePack);
				//Patcher.GameAccessPath("BTL_PACK_DE.DAT", HandleBattlePack);
			});
		}

		public void HandleBattlePack()
		{
			Patcher.GameAccessPath("9/BTL_COMMON/2", () =>
			{
				Patcher.Action("Level Up", () =>
				{
					Patcher.GameAccessPath("E_A018", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A018_LVUP_00.png", "E_A018_LVUP_00"); });
					});
				});

				Patcher.Action("New Skill", () =>
				{
					Patcher.GameAccessPath("E_A019", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A019_NEW_00.png", "E_A019_NEW_00"); });
					});
				});

				Patcher.Action("New Arte", () =>
				{
					Patcher.GameAccessPath("E_A024", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A024_LEARNING_00.png", "E_A024_LEARNING_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A024_LEARNING_00_F", "E_A024_LEARNING_00_G"); });
						//Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A024_LEARNING_00.png", "E_A024_LEARNING_00_F", "E_A024_LEARNING_00_G"); });
					});
				});

				Patcher.Action("Secret Mission Completed!", () =>
				{
					Patcher.GameAccessPath("E_A027", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A027_MISSION_00.png", "E_A027_MISSION_00"); });
					});
				});

				Patcher.Action("New Title", () =>
				{
					Patcher.GameAccessPath("E_A062", () =>
					{
						Patcher.GameGetTXM("8", "9", (Txm) => { Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A062_TITLE_00.png", "E_A062_TITLE_00"); });
						Patcher.GameGetTXM("10", "11", (Txm) => { Patcher.UpdateTxm2DWithEmpty(Txm, "E_A062_TITLE_00_F", "E_A062_TITLE_00_G"); });
					});
				});
			});
		}
	}
}
