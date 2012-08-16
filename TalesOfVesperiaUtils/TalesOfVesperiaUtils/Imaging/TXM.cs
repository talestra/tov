using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;
using CSharpUtils.Drawing;
using CSharpUtils.Endian;

namespace TalesOfVesperiaUtils.Imaging
{
	unsafe public class TXM
	{
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x50)]
		public struct ImageHeaderStruct
		{
			/// <summary>
			/// 0000 -
			/// </summary>
			[FieldOffset(0x0000)]
			public uint_be Magic;

			/// <summary>
			/// 0004 - Total Size of the File (without the tailing padding)
			/// </summary>
			[FieldOffset(0x0004)]
			public uint_be FileSize;

			/// <summary>
			/// 0008 - Usually contains a 4?
			/// </summary>
			[FieldOffset(0x0008)]
			public uint_be Unknown0x08;

			/// <summary>
			/// 000C - Number of images
			/// </summary>
			[FieldOffset(0x000c)]
			public uint_be Surface2DCount;

			/// <summary>
			/// 0010 - Zero
			/// </summary>
			[FieldOffset(0x0010)]
			public uint_be Unknown0x10;

			/// <summary>
			/// 0014 - Contains something
			/// </summary>
			[FieldOffset(0x0014)]
			public uint_be Surface3DCount;

			/// <summary>
			/// 0018 - Zero
			/// </summary>
			[FieldOffset(0x0018)]
			public uint_be Unknown0x18;

			/// <summary>
			/// 001C - Zero
			/// </summary>
			[FieldOffset(0x001C)]
			public uint_be Unknown0x1C;
		}

		/// <summary>
		/// Starts at 0x0050
		/// Size 0x0058
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x58)]
		public struct Surface2DInfoStruct
		{
			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0000)]
			public uint_be Unknown0x00;

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0004)]
			public uint_be Width;

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0008)]
			public uint_be Height;

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x000C)]
			public uint_be Depth;

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0010)]
			public ImageFormat ImageFileFormat;   // D3D9:D3DXIMAGE_FILEFORMAT -or- D3DX10_IMAGE_FILE_FORMAT

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0014)]
			public uint_be StringOffset;

			public override string ToString()
			{
				return String.Format(
					"ImageEntryStruct(Size={0}x{1}, Depth={2}, ImageFileFormat={3}, StringOffset={4})",
					Width, Height, Depth, ImageFileFormat, StringOffset
				);
			}
		}

		/// <summary>
		/// Starts at 0x0050
		/// Size 0x0058
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x58)]
		public struct Surface3DInfoStruct
		{
			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0000)]
			public uint_be Unknown0x00;

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0004)]
			public uint_be Width;

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0008)]
			public uint_be Height;

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x000C)]
			public uint_be Depth;

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0010)]
			public uint_be Unknown0x10;

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0014)]
			public ImageFormat ImageFileFormat;   // D3D9:D3DXIMAGE_FILEFORMAT -or- D3DX10_IMAGE_FILE_FORMAT

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0018)]
			public uint_be StringOffset;

			public override string ToString()
			{
				return String.Format(
					"ImageEntryStruct(Size={0}x{1}, MipLevels={2}, ImageFileFormat={3}, StringOffset={4})",
					Width, Height, Depth, ImageFileFormat, StringOffset
				);
			}
		}

		abstract public class SurfaceEntryInfo<InfoStruct>
		{
			public readonly TXM TXM;
			public readonly InfoStruct ImageEntry;
			public readonly string Name;
			public readonly int Index;
			protected Stream SliceStream;

			virtual public int TotalBytes { get { return 0; } }

			public SurfaceEntryInfo(TXM TXM, int Index, InfoStruct ImageEntry, string Name)
			{
				this.TXM = TXM;
				this.Index = Index;
				this.ImageEntry = ImageEntry;
				this.Name = Name;
				
				//Console.WriteLine("{0}: {1}", Index, "%08X".Sprintf(TXM.TXVStream.Position));
				this.SliceStream = TXM.TXVStream.ReadStream(TotalBytes);
			}
		}

		public class Surface3DEntryInfo : SurfaceEntryInfo<Surface3DInfoStruct>
		{
			private BitmapList _Bitmaps;

			public BitmapList Bitmaps
			{
				get
				{
					if (_Bitmaps == null) _Bitmaps = _GenerateBitmapList();
					return _Bitmaps;
				}
			}

			private BitmapList _GenerateBitmapList()
			{
				var BitmapList = new BitmapList(Depth);

				switch (ImageEntry.ImageFileFormat.TextureFormat)
				{
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5:
						return DXT5.LoadSwizzled3D(this.SliceStream, Width, Height, Depth, this.Tiled);
						break;

					default:
						throw (new NotImplementedException("Not implemented format : " + ImageEntry.ImageFileFormat.TextureFormat));
				}

				return BitmapList;
			}

			public Surface3DEntryInfo(TXM TXM, int Index, Surface3DInfoStruct ImageEntry, string Name)
				: base(TXM, Index, ImageEntry, Name)
			{
			}

			public bool Tiled { get { return ImageEntry.ImageFileFormat.Tiled; } }
			public int Width { get { return (int)(uint)ImageEntry.Width; } }
			public int Height { get { return (int)(uint)ImageEntry.Height; } }
			public int Depth { get { return (int)(uint)ImageEntry.Depth; } }
			public int BitsPerPixel
			{
				get
				{
					return GpuUtils.g_XGTextureFormatBitsPerPixel[(int)ImageEntry.ImageFileFormat.TextureFormat];
				}
			}
			public int Stride { get { return BitsPerPixel * Width / 8; } }
			override public int TotalBytes { get { return Stride * Height * Depth; } }
		}

		public class Surface2DEntryInfo : SurfaceEntryInfo<Surface2DInfoStruct>
		{
			private Bitmap _Bitmap;

			public Surface2DEntryInfo(TXM TXM, int Index, Surface2DInfoStruct ImageEntry, string Name)
				: base(TXM, Index, ImageEntry, Name)
			{
			}

			public Bitmap Bitmap
			{
				get
				{
					if (_Bitmap == null) _Bitmap = _GenerateBitmap();
					return _Bitmap;
				}
			}

			public bool Tiled { get { return ImageEntry.ImageFileFormat.Tiled; } }
			public int Width { get { return (int)(uint)ImageEntry.Width; } }
			public int Height { get { return (int)(uint)ImageEntry.Height; } }
			public int BitsPerPixel
			{
				get
				{
					return GpuUtils.g_XGTextureFormatBitsPerPixel[(int)ImageEntry.ImageFileFormat.TextureFormat];
				}
			}
			public int Stride { get { return BitsPerPixel * Width / 8; } }
			override public int TotalBytes { get { return Stride * Height; } }

			private Bitmap _GenerateBitmap()
			{
				var Bitmap = new Bitmap(Width, Height);

				switch (ImageEntry.ImageFileFormat.TextureFormat)
				{
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_4_4_4_4:
						Bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
						{
							int WidthHeight = Width * Height;
							var Stream = SliceStream.SliceWithLength();
							var Bytes = Stream.ReadBytes(TotalBytes);
							int m = 0;
							bool Tiled = this.Tiled;

							var Base = (ARGB_Rev*)BitmapData.Scan0.ToPointer();

							for (int n = 0; n < WidthHeight; n++)
							{
								int X, Y;
								if (Tiled)
								{
									Swizzling.XGAddress2DTiledXY(n, Width, 2, out X, out Y);
								}
								else
								{
									Swizzling.UnswizzledXY(n, Width, 2, out X, out Y);
								}
								var Data = (uint)(((Bytes[m++] << 8) | Bytes[m++]));
								ARGB_Rev C;
								C.B = (byte)BitUtils.ExtractScaled(Data, 0, 4, 255);
								C.R = (byte)BitUtils.ExtractScaled(Data, 8, 4, 255);
								C.G = (byte)BitUtils.ExtractScaled(Data, 4, 4, 255);
								C.A = (byte)BitUtils.ExtractScaled(Data, 12, 4, 255);

								Base[Y * Width + X] = C;
								//BitmapData.
								//var COL = Color.FromArgb(A, R, G, B);
								//Bitmap.SetPixel(X, Y, Color.FromArgb(A, R, G, B));
							}
						});
						break;
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_8_8_8_8:
						Bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
						{
							int WidthHeight = Width * Height;
							var Stream = SliceStream.SliceWithLength();
							var Bytes = Stream.ReadBytes(TotalBytes);
							int m = 0;
							bool Tiled = this.Tiled;

							var Base = (ARGB_Rev*)BitmapData.Scan0.ToPointer();

							for (int n = 0; n < WidthHeight; n++)
							{
								int X, Y;
								if (Tiled)
								{
									Swizzling.XGAddress2DTiledXY(n, Width, 4, out X, out Y);
								}
								else
								{
									Swizzling.UnswizzledXY(n, Width, 4, out X, out Y);
								}
								var Data = (uint)(((Bytes[m++] << 24) | (Bytes[m++] << 16) | (Bytes[m++] << 8) | Bytes[m++]));
								ARGB_Rev C;
								C.B = (byte)BitUtils.ExtractScaled(Data, 0, 8, 255);
								C.G = (byte)BitUtils.ExtractScaled(Data, 8, 8, 255);
								C.R = (byte)BitUtils.ExtractScaled(Data, 16, 8, 255);
								C.A = (byte)BitUtils.ExtractScaled(Data, 24, 8, 255);

								//C.R = 0xFF;
								//C.G = 0x00;
								//C.B = 0x00;
								//C.A = 0xFF;

								Base[Y * Width + X] = C;
								//BitmapData.
								//var COL = Color.FromArgb(A, R, G, B);
								//Bitmap.SetPixel(X, Y, Color.FromArgb(A, R, G, B));
							}
						});
						break;
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5:
						//Graphics.FromImage(Bitmap).DrawImage(DXT5.LoadSwizzled(this.SliceStream, Width, Height, this.Tiled), new Point(0, 0));
						return DXT5.LoadSwizzled(this.SliceStream, Width, Height, this.Tiled);
						break;
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT1:
						Console.Error.Write("Unimplemented GPUTEXTUREFORMAT_DXT1");
						Graphics.FromImage(Bitmap).DrawString("Unimplemented GPUTEXTUREFORMAT_DXT1", new Font("Arial", 20), new SolidBrush(Color.Red), new Point(16, 16));
						break;
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_1:
						Console.Error.Write("Unimplemented GPUTEXTUREFORMAT_1");
						Graphics.FromImage(Bitmap).DrawString("Unimplemented GPUTEXTUREFORMAT_1", new Font("Arial", 20), new SolidBrush(Color.Red), new Point(16, 16));
						break;
					default:
						throw (new NotImplementedException("Not implemented format : " + ImageEntry.ImageFileFormat.TextureFormat));
				}

				return Bitmap;
			}

			public override string ToString()
			{
				return String.Format("ImageEntryInfo(Name='{0}', ImageEntry={1})", Name, ImageEntry);
			}
		}

		public ImageHeaderStruct ImageHeader;
		public Surface2DEntryInfo[] Surface2DEntries;
		public Surface3DEntryInfo[] Surface3DEntries;
		Stream TXVStream;

		public TXM Load(Stream TXMStream, Stream TXVStream)
		{
			this.TXVStream = TXVStream;

			this.ImageHeader = TXMStream.ReadStruct<ImageHeaderStruct>();

			this.Surface2DEntries = new Surface2DEntryInfo[ImageHeader.Surface2DCount];
			for (int n = 0; n < ImageHeader.Surface2DCount; n++)
			{
				var ImageEntry = TXMStream.ReadStruct<Surface2DInfoStruct>();
				var Name = TXMStream.SliceWithLength(TXMStream.Position + Marshal.OffsetOf(typeof(Surface2DInfoStruct), "StringOffset").ToInt32() - sizeof(Surface2DInfoStruct) + ImageEntry.StringOffset).ReadStringz();
				this.Surface2DEntries[n] = new Surface2DEntryInfo(this, n, ImageEntry, Name);
			}

			this.Surface3DEntries = new Surface3DEntryInfo[ImageHeader.Surface3DCount];
			for (int n = 0; n < ImageHeader.Surface3DCount; n++)
			{
				var ImageEntry = TXMStream.ReadStruct<Surface3DInfoStruct>();
				var Name = TXMStream.SliceWithLength(TXMStream.Position + Marshal.OffsetOf(typeof(Surface3DInfoStruct), "StringOffset").ToInt32() - sizeof(Surface3DInfoStruct) + ImageEntry.StringOffset).ReadStringz();
				this.Surface3DEntries[n] = new Surface3DEntryInfo(this, n, ImageEntry, Name);
			}

			return this;
		}

		static public Bitmap LoadAbgr(Stream Stream, int Width, int Height)
		{
			var Data = Stream.ReadBytes(Width * Height * 4);
			var Bitmap = new Bitmap(Width, Height);
			try
			{
				for (int n = 0; n < Data.Length; n += 4)
				{
					int X, Y;
					Swizzling.XGAddress2DTiledXY(n / 4, Width, 4, out X, out Y);
					byte A = Data[n + 0];
					byte R = Data[n + 1];
					byte G = Data[n + 2];
					byte B = Data[n + 3];
					Bitmap.SetPixel(X, Y, Color.FromArgb(A, R, G, B));
				}
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}
			/*
			fixed (byte* DataPtr = Data)
			{
				BitmapUtils.TransferChannelsDataInterleaved(
					Bitmap.GetFullRectangle(),
					Bitmap,
					DataPtr,
					BitmapUtils.Direction.FromDataToBitmap,
					BitmapChannel.Alpha, BitmapChannel.Red, BitmapChannel.Green, BitmapChannel.Blue
				);
			}
			*/
			//Bitmap.GetChannelsDataInterleaved
			return Bitmap;
		}
	}
}
