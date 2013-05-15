﻿using CSharpUtils;
using CSharpUtils.Drawing;
using CSharpUtils.Drawing.Distance;
using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.Imaging;
using TalesOfVesperiaUtils.Imaging.Internal;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaTranslationEngine.Components
{
	unsafe public class UiSvo : PatcherComponent
	{
		public UiSvo(Patcher Patcher)
			: base(Patcher)
		{
		}

		public void Handle()
		{
            this.Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("ui.svo", new Action[] {
                this.Handle1
            });
		}

        private void Handle1()
        {
            Patcher.GameAccessPath("ui.svo", () =>
            {
                HandleFont();
                HandleImages();
            });
        }

		private void HandleFont()
		{
			// Font
			Patcher.GameGetTXM("FONTTEX10.TXM", "FONTTEX10.TXV", (Txm) =>
			{
				Patcher.PatcherGetImageColorAlpha(PatchPaths.FONTTEXT10_15_COLOR, PatchPaths.FONTTEXT10_15_ALPHA, (Bitmap) =>
				{
					//var MS = new MemoryStream();
					//(new DXT5()).SaveSwizzled3D(new BitmapList(Bitmap, Bitmap), MS, CompressDXT.CompressionMode.Normal);
					//var BitmapList2 = (new DXT5()).LoadSwizzled3D(MS.Slice(), Bitmap.Width, Bitmap.Height, 1);
					//Bitmap.Save(@"c:\projects\1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
					//BitmapList2.Bitmaps[0].Save(@"c:\projects\2.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

					var Entry = Txm.Surface3DEntries[0];

					Entry.Bitmaps.Bitmaps[15] = Bitmap;
					//Entry.Bitmaps.Bitmaps[15] = Entry.Bitmaps.Bitmaps[0].Duplicate();
					Entry.UpdateBitmapList(Entry.Bitmaps);
				});
			});

		}

        private void HandleImages()
		{
			Patcher.Action("Patching Images...", () =>
			{
				var PatcherDataFS = Patcher.PatcherDataFS;

				foreach (var Suffix in new[] { "", "DE", "FR" })
				{
					Patcher.GameGetTXM("COMALL" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_COMALLINFO00, "U_COMALLINFO00");
					});

					Patcher.GameGetTXM("ICONBTN360" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_ICONBTN36000, "U_ICONBTN36000");
					});

					Patcher.GameGetTXM("ICONKIND" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_ICONKIND00, "U_ICONKIND00");
					});

					Patcher.GameGetTXM("ICONSORT" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_ICONSORT00, "U_ICONSORT00");
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_ICONSORT01, "U_ICONSORT01");
					});

					Patcher.GameGetTXM("ICONSYM" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_ICONSYM01, "U_ICONSYM01");
					});

					Patcher.GameGetTXM("MENU" + Suffix, (Txm) =>
					{
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_MENUBTLINFO00, "U_MENUBTLINFO00");
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_MENUBTLINFO02, "U_MENUBTLINFO02");
						Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.U_MENUSTR01, "U_MENUSTR01");
					});
				}

				//TOWNMAPFOR.U_MAP_EFOR
				Patcher.GameGetTXM("TOWNMAPFOR", (Txm) =>
				{
					Patcher.PatcherGetFile("Font/Seagull.ttf", (FontStream) =>
					{
						var BaseImage = Txm.Surface2DEntriesByName["U_MAP_EFOR"].Bitmap;
						{
							var Graphics2 = Graphics.FromImage(BaseImage);
							Graphics2.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
							Graphics2.FillRectangle(new SolidBrush(Color.Transparent), new Rectangle(0, 38, 512, 190));
						}

						var PrivateFontCollection = new PrivateFontCollection();
						var FontBytes = FontStream.ReadAll();
						fixed (byte* FontPointer = FontBytes)
						{
							PrivateFontCollection.AddMemoryFont(new IntPtr(FontPointer), FontBytes.Length);
						}
						
						var Bitmap = new Bitmap(512, 512);
						var graphics = Graphics.FromImage(Bitmap);
						var Font1 = new Font(PrivateFontCollection.Families[0].Name, 26, FontStyle.Regular);
						var Font2 = new Font(PrivateFontCollection.Families[0].Name, 40, FontStyle.Regular);
						//graphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, 512, 512));
						graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
						graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
						graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
						graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
						graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

						graphics.DrawString("Fuerte de Deidon", Font2, new SolidBrush((ARGB_Rev)"#503c3c"), new PointF(-3, 97));
						//graphics.DrawString("The Imperial Capital", Font1, new SolidBrush((ARGB_Rev)"#503c3c"), new PointF(-2, 35.2f));

						var _DistanceMap = DistanceMap.GetDistanceMap(DistanceMap.GetMask(Bitmap));

						DistanceMap.DrawGlow(Bitmap, _DistanceMap, 6, "#fff0d3", 0.1f);
						graphics.DrawImage(BaseImage, Point.Empty);

						Patcher.UpdateTxm2DWithImage(Txm, Bitmap, "U_MAP_EFOR");
					});
				});

			});
		}

		static protected Dictionary<string, List<string>> GetImageParts(FileSystem PatcherDataFS, string Path)
		{
			var PARTS = new Dictionary<string, List<string>>();
			foreach (var Entry in PatcherDataFS.FindFiles(Path, new Wildcard("*.png")))
			{
				var NameParts = Entry.Name.Split('.');
				var File = NameParts[0].ToUpper();
				var Item = NameParts[1].ToUpper();

				if (!PARTS.ContainsKey(File)) PARTS[File] = new List<string>();
				PARTS[File].Add(Item);
			}
			return PARTS;
		}
	}
}
