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
				//Patcher.GameAccessPath("BTL_PACK_FR.DAT", HandleBattlePack);
				//Patcher.GameAccessPath("BTL_PACK_DE.DAT", HandleBattlePack);
			});
		}

		public void HandleBattlePack()
		{
			Patcher.GameAccessPath("9/BTL_COMMON/2/E_A018", () =>
			{
				Patcher.GameGetTXM("8", "9", (Txm) =>
				{
					Patcher.UpdateTxm2DWithPng(Txm, "BTL_SVO/E_A018_LVUP_00.png", "E_A018_LVUP_00");
				});
			});
		}
	}
}
