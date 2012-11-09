using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaTranslationEngine.Components
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
					Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_USUALBTLFONT00, "U_USUALBTLFONT00", "U_USUALBTLFONT00_EU");
					Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_USUALBTLFONT01, "U_USUALBTLFONT01");
					Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_USUALLOAD00, "U_USUALLOAD00");
					Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_USUALLOAD01, "U_USUALLOAD01");
					Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_USUALFIEINFO02, "U_USUALFIEINFO02");
				});
			});
		}
	}
}
