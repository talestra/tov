using CSharpUtils.VirtualFileSystem;
using CSharpUtils.Streams;
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
using TalesOfVesperiaUtils.Imaging.Internal;

namespace TalesOfVesperiaTranslationEngine.CommonSvo.TextureDat
{
	public class TextureDat
	{
		public static void Handle(Patcher Patcher, FileSystem CommonSvo)
		{
			Patcher.Action("texture.dat", () =>
			{
				var PatcherDataFS = Patcher.PatcherDataFS;
				var TempFS = Patcher.TempFS;
				var TextureDatOri = "TEXTURE.DAT.ori";
				var TextureDatMod = "TEXTURE.DAT.mod";
				var TextureDatModCompressed = "TEXTURE.DAT.mod.c";

				var TextureDatName = "TEXTURE.DAT";
				//var TextureDatStream = CommonSvo.OpenFile(TextureDatName, FileMode.Open);
				byte[] TextureDatUncompressed = null;

				Patcher.Action("Uncompressing TEXTURE.DAT", () =>
				{
					if (!TempFS.Exists(TextureDatOri))
					{
						TempFS.WriteAllBytes(TextureDatOri, TalesCompression.DecompressStream(CommonSvo.OpenFile(TextureDatName, FileMode.Open)).ToArray());
					}
				});

				Patcher.Action("Copying ORI -> MOD", () =>
				{
					TempFS.Copy(TextureDatOri, TextureDatMod, true);
				});

				Patcher.Action("Reading TXM/TXV", () =>
				{
					var TxmTxvFPS4 = new FPS4(TempFS.OpenFile(TextureDatMod, FileMode.Open));
					TXM Txm = TXM.FromTxmTxv(TxmTxvFPS4[0].Open(), TxmTxvFPS4[1].Open());

					Patcher.Action("Updating Battle Font Image", () =>
					{
						var BattleFont = new Bitmap(Image.FromStream(PatcherDataFS.OpenFile("COMMON_SVO/TEXTURE_DAT/U_USUALBTLFONT00_EU.png", FileMode.Open)));
						Txm.Surface2DEntriesByName["U_USUALBTLFONT00"].UpdateBitmap(BattleFont);
						Txm.Surface2DEntriesByName["U_USUALBTLFONT00_EU"].UpdateBitmap(BattleFont);
						//Txm.Surface2DEntriesByName["U_USUALBTLFONT00_EU"].Open().FillStreamWithByte((byte)0x00);
					});

					Patcher.Action("Updating Battle Info Image", () =>
					{
						var BattleFont = new Bitmap(Image.FromStream(PatcherDataFS.OpenFile("COMMON_SVO/TEXTURE_DAT/U_USUALBTLFONT01.png", FileMode.Open)));
						Txm.Surface2DEntriesByName["U_USUALBTLFONT01"].UpdateBitmap(BattleFont);
					});

					Patcher.Action("Updating Loading Image", () =>
					{
						//Console.WriteLine(Txm.Surface2DEntriesByName["U_USUALLOAD00"]);
						var Load00Mod = new Bitmap(Image.FromStream(PatcherDataFS.OpenFile("COMMON_SVO/TEXTURE_DAT/U_USUALLOAD00.png", FileMode.Open)));
						var Load01Mod = new Bitmap(Image.FromStream(PatcherDataFS.OpenFile("COMMON_SVO/TEXTURE_DAT/U_USUALLOAD01.png", FileMode.Open)));
						Txm.Surface2DEntriesByName["U_USUALLOAD00"].UpdateBitmap(Load00Mod);
						Txm.Surface2DEntriesByName["U_USUALLOAD01"].UpdateBitmap(Load01Mod);
						//new DXT5().SaveSwizzled2D(Load00Mod, Txm.Surface2DEntriesByName["U_USUALLOAD00"].Open(), CompressDXT.CompressionMode.Normal);
						//new DXT5().SaveSwizzled2D(Load01Mod, Txm.Surface2DEntriesByName["U_USUALLOAD01"].Open(), CompressDXT.CompressionMode.Normal);
					});


					Patcher.Action("Updating Pause Image", () =>
					{
						var PauseMod = new Bitmap(Image.FromStream(PatcherDataFS.OpenFile("COMMON_SVO/TEXTURE_DAT/U_USUALFIEINFO02.png", FileMode.Open)));
						Txm.Surface2DEntriesByName["U_USUALFIEINFO02"].UpdateBitmap(PauseMod);
					});
				});

				Patcher.Action("Recompressing TEXTURE.DAT", () =>
				{
					TempFS.WriteAllBytes(TextureDatModCompressed, TalesCompression.CreateFromVersion(15).EncodeBytes(TempFS.ReadAllBytes(TextureDatMod)));
				});

				Patcher.Action("Reinserting TEXTUER.DAT in COMMON.SVO", () =>
				{
					if (TempFS.GetFileInfo("TEXTURE.DAT.mod.c").Size > TempFS.GetFileInfo("TEXTURE.DAT").Size)
					{
						throw(new Exception("Modified TEXTURE.DAT is biggen than original"));
					}

					CommonSvo.OpenFile(TextureDatName, FileMode.Open).WriteBytes(TempFS.ReadAllBytes(TextureDatModCompressed)).Flush();
				});

				Patcher.Action("Done", () =>
				{
				});

				//throw new NotImplementedException();
			});
		}
	}
}
