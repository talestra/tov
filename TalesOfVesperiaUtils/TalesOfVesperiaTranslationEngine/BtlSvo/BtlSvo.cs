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

		public void Handle(FileSystem GameRootFS)
		{
			Patcher.Action("Patching BTL.SVO...", () =>
			{
				using (var BtlSvo = new FPS4FileSystem(GameRootFS.OpenFileRW("btl.svo")))
				{
					HandleBattlePack(new FPS4FileSystem(BtlSvo.OpenFileRW("BTL_PACK_UK.DAT")));
					//HandleBattlePack(new FPS4FileSystem(BtlSvo.OpenFileRW("BTL_PACK_FR.DAT")));
					//HandleBattlePack(new FPS4FileSystem(BtlSvo.OpenFileRW("BTL_PACK_DE.DAT")));
				}
			});
		}

		public void HandleBattlePack(FileSystem BattlePackFS)
		{
			Patcher.Action("Patching PACK...", () =>
			{
				using (var _9Svo = new FPS4FileSystem(BattlePackFS.OpenFileRW("9")))
				{
					using (var BtlCommonSvo = new FPS4FileSystem(_9Svo.OpenFileRW("BTL_COMMON")))
					{
						using (var _9_Common_2 = new FPS4FileSystem(BtlCommonSvo.OpenFileRW("2")))
						{
							using (var E_A018 = new FPS4FileSystem(new DecompressRecompressStream(_9_Common_2.OpenFileRW("E_A018"))))
							{
								var Txm = TXM.FromTxmTxv(E_A018.OpenFileRW("8"), E_A018.OpenFileRW("9"));
								//Txm.Surface2DEntriesByName["E_A018_LVUP_00"].Bitmap.Save(@"c:/temp/level_up.png");
								Txm.Surface2DEntriesByName["E_A018_LVUP_00"].UpdateBitmap(
									new Bitmap(Image.FromStream(Patcher.PatcherDataFS.OpenFileRead("BTL_SVO/E_A018_LVUP_00.png")))
								);
								//"E_A018_LVUP_00.png"
							}
						}
					}
				}
			});
		}
	}
}
