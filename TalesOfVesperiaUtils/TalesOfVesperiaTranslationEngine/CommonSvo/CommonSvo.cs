using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaTranslationEngine.CommonSvo
{
	public class CommonSvo : PatcherComponent
	{
		public CommonSvo(Patcher Patcher)
			: base(Patcher)
		{
		}

		public void Handle()
		{
			Patcher.GameAccessPath("common.svo", () =>
			{
				HandleTextureDat();
			});
		}

		public void HandleTextureDat()
		{
			Patcher.GameAccessPath("TEXTURE.DAT", () =>
			{
				Patcher.GameGetTXM("0", "1", (Txm) =>
				{
					Patcher.UpdateTxm2DWithPng(Txm, "COMMON_SVO/TEXTURE_DAT/U_USUALBTLFONT00_EU.png", "U_USUALBTLFONT00", "U_USUALBTLFONT00_EU");
					Patcher.UpdateTxm2DWithPng(Txm, "COMMON_SVO/TEXTURE_DAT/U_USUALBTLFONT01.png", "U_USUALBTLFONT01");
					Patcher.UpdateTxm2DWithPng(Txm, "COMMON_SVO/TEXTURE_DAT/U_USUALLOAD00.png", "U_USUALLOAD00");
					Patcher.UpdateTxm2DWithPng(Txm, "COMMON_SVO/TEXTURE_DAT/U_USUALLOAD01.png", "U_USUALLOAD01");
					Patcher.UpdateTxm2DWithPng(Txm, "COMMON_SVO/TEXTURE_DAT/U_USUALFIEINFO02.png", "U_USUALFIEINFO02");
				});
			});
		}
	}
}
