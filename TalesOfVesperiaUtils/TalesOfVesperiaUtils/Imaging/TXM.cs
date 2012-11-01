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
using TalesOfVesperiaUtils.Imaging.Internal;

namespace TalesOfVesperiaUtils.Imaging
{
	unsafe public class TXM : IDisposable
	{
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
		public struct ImageVersionStruct
		{
			/// <summary>
			/// 0000 -
			/// </summary>
			[FieldOffset(0x0000)]
			public ushort_be Version;

			/// <summary>
			/// 0002 -
			/// </summary>
			[FieldOffset(0x0000)]
			public ushort_be VersionPadding;
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x50)]
		public struct ImageHeaderStructV2
		{
			/// <summary>
			/// 0000 -
			/// </summary>
			[FieldOffset(0x0000)]
			public ImageVersionStruct VersionInfo;

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
		public struct Surface2DInfoStructV2
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

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x0018)]
			public uint_be ContentOffset;

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
		public struct Surface3DInfoStructV2
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

			/// <summary>
			/// 
			/// </summary>
			[FieldOffset(0x001C)]
			public uint_be ContentOffset;

			public override string ToString()
			{
				return String.Format(
					"ImageEntryStruct(Size={0}x{1}, MipLevels={2}, ImageFileFormat={3}, StringOffset={4})",
					Width, Height, Depth, ImageFileFormat, StringOffset
				);
			}
		}

		abstract public class SurfaceEntryInfo<InfoStruct> : IDisposable
		{
			public readonly TXM TXM;
			public readonly InfoStruct ImageEntry;
			public readonly string Name;
			public readonly int Index;
			protected Stream SliceStream;

			abstract public int TotalBytes { get; }
			abstract public int ContentOffset { get; }

			public Stream Open()
			{
				return SliceStream.SliceWithLength();
			}

			public SurfaceEntryInfo(TXM TXM, int Index, InfoStruct ImageEntry, string Name)
			{
				this.TXM = TXM;
				this.Index = Index;
				this.ImageEntry = ImageEntry;
				this.Name = Name;
				
				//Console.WriteLine("{0}: {1}", Index, "%08X".Sprintf(TXM.TXVStream.Position));
				//this.SliceStream = TXM.TXVStream.ReadStream(TotalBytes);
				//Console.WriteLine(ContentOffset);
				this.SliceStream = TXM.TXVStream.SliceWithLength(ContentOffset, TotalBytes);
			}

			public void Dispose()
			{
			}
		}

		public class Surface3DEntryInfo : SurfaceEntryInfo<Surface3DInfoStructV2>
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
				//var BitmapList = new BitmapList(Depth);

				switch (ImageEntry.ImageFileFormat.TextureFormat)
				{
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5:
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT5A:
						return (new DXT5()).LoadSwizzled3D(this.SliceStream.Slice(), Width, Height, Depth, this.Tiled);

					default:
						throw (new NotImplementedException("[Surface3DEntryInfo] Not implemented format : " + ImageEntry.ImageFileFormat.TextureFormat));
				}

				//return BitmapList;
			}

			public Surface3DEntryInfo(TXM TXM, int Index, Surface3DInfoStructV2 ImageEntry, string Name)
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
			public bool Swizzled { get { return true; } }
			public int Stride { get { return BitsPerPixel * Width / 8; } }
			override public int TotalBytes { get
			{
				if (Swizzled)
				{
					switch (ImageEntry.ImageFileFormat.TextureFormat)
					{
						case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5:
						//case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT5A:
							return Swizzling.XGAddress3DTiledExtent(Width / 4, Height / 4, Depth, 16) * 16;
					}
				}
				return Stride * Height * Depth;
			} }

			public override int ContentOffset
			{
				get { return (int)(uint)ImageEntry.ContentOffset; }
			}

			public void UpdateBitmapList(BitmapList BitmapList)
			{
				var Bitmaps = BitmapList.Bitmaps;
				if (BitmapList.Bitmaps.Length == 0) throw(new Exception("Empty BitmapList"));
				if (BitmapList.Bitmaps.Length != Depth) throw (new Exception(String.Format("Invalid dimensions {0}x{1}x{2} != {2}x{3}x{4}", Bitmaps[0].Width, Bitmaps[0].Height, Bitmaps.Length, Width, Height, Depth)));
				if (Bitmaps[0].Width != Width || Bitmaps[0].Height != Height) throw (new Exception(String.Format("Invalid dimensions {0}x{1} != {2}x{3}", Bitmaps[0].Width, Bitmaps[0].Height, Width, Height)));

				switch (ImageEntry.ImageFileFormat.TextureFormat)
				{
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5:
						(new DXT5()).SaveSwizzled3D(BitmapList, this.SliceStream.Slice(), CompressDXT.CompressionMode.Normal);
						break;
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT1:
						(new DXT1()).SaveSwizzled3D(BitmapList, this.SliceStream.Slice(), CompressDXT.CompressionMode.Normal);
						break;
					default:
						throw (new NotImplementedException());
				}
			}
		}

		public class Surface2DEntryInfo : SurfaceEntryInfo<Surface2DInfoStructV2>
		{
			private Bitmap _Bitmap;

			public override int ContentOffset
			{
				get { return (int)(uint)ImageEntry.ContentOffset; }
			}

			public Surface2DEntryInfo(TXM TXM, int Index, Surface2DInfoStructV2 ImageEntry, string Name)
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
			public int BitsPerPixel { get { return GpuUtils.g_XGTextureFormatBitsPerPixel[(int)ImageEntry.ImageFileFormat.TextureFormat]; } }
			public bool Swizzled { get { return true; } }
			public int Stride { get { return BitsPerPixel * Width / 8; } }
			override public int TotalBytes { get {
				if (Swizzled)
				{
					switch (ImageEntry.ImageFileFormat.TextureFormat)
					{
						case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5:
						//case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT5A:
							return Swizzling.XGAddress2DTiledExtent(Width / 4, Height / 4, 16) * 16;
					}
				}
				return Stride * Height;
			} }

			private void _GenerateBitmapDecode(Bitmap Bitmap, Func<byte[], int, ARGB_Rev> Decode)
			{
				int BytesPerPixel = BitsPerPixel / 8;

				Bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
				{
					int WidthHeight = Width * Height;
					var Stream = SliceStream.Slice();
					var Bytes = Stream.ReadBytes(TotalBytes);
					int m = 0;
					bool Tiled = this.Tiled;

					var Base = (ARGB_Rev*)BitmapData.Scan0.ToPointer();

					for (int n = 0; n < WidthHeight; n++)
					{
						int X, Y;

						if (Tiled)
						{
							Swizzling.XGAddress2DTiledXY(n, Width, BytesPerPixel, out X, out Y);
						}
						else
						{
							Swizzling.UnswizzledXY(n, Width, BytesPerPixel, out X, out Y);
						}

						if (X >= Width || Y >= Height)
						{
							Console.Error.WriteLine("(Warning: Outside! ({0}, {1}) - ({2}x{3}))", X, Y, Width, Height);
							continue;
						}

						Base[Y * Width + X] = Decode(Bytes, m);
						m += BytesPerPixel;
					}
				});
			}

			private void _GenerateBitmapEncode(Bitmap Bitmap, Action<byte[], ARGB_Rev> Encode)
			{
				int TotalBytes = Bitmap.Width * Bitmap.Height * BitsPerPixel / 8;
				int BytesPerPixel = BitsPerPixel / 8;

				Bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
				{
					int WidthHeight = Width * Height;
					var Bytes = new byte[TotalBytes];
					bool Tiled = this.Tiled;
					var PixelBytes = new byte[BytesPerPixel];

					var Base = (ARGB_Rev*)BitmapData.Scan0.ToPointer();

					for (int y = 0; y < Height; y++)
					{
						for (int x = 0; x < Width; x++)
						{
							int n;

							if (Tiled)
							{
								n = Swizzling.XGAddress2DTiledOffset(x, y, Width, BytesPerPixel);
							}
							else
							{
								n = Swizzling.UnswizzledOffset(x, y, Width, BytesPerPixel);
							}

							Encode(PixelBytes, Base[y * Width + x]);
							Array.Copy(PixelBytes, 0, Bytes, n * BytesPerPixel, BytesPerPixel);
						}
					}

					SliceStream.Slice().WriteBytes(Bytes).Flush();
				});
			}

			public void Relink(Surface2DEntryInfo that)
			{
				if (this.TXM != that.TXM) throw(new InvalidOperationException("Relinked items must be in the same TXM"));
				if (this.ImageEntry.ContentOffset == that.ImageEntry.ContentOffset)
				{
					// Already linked
				}
				else
				{
					// Not linked yet
					throw (new NotImplementedException());
				}
			}

			public void UpdateBitmap(Bitmap Bitmap)
			{
				if (Bitmap.Width != Width || Bitmap.Height != Height) throw(new Exception(String.Format("Invalid dimensions {0}x{1} != {2}x{3}", Bitmap.Width, Bitmap.Height, Width, Height)));
				
				switch (ImageEntry.ImageFileFormat.TextureFormat)
				{
					//case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_4_4_4_4:
					//	break;
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_8_8_8_8:
						_GenerateBitmapEncode(Bitmap, (Out, Color) =>
						{
							Out[0] = Color.A;
							Out[1] = Color.R;
							Out[2] = Color.G;
							Out[3] = Color.B;
							//BitConverter.GetBytes();
						});
						break;
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5:
						(new DXT5()).SaveSwizzled2D(Bitmap, this.SliceStream.Slice(), CompressDXT.CompressionMode.Normal);
						break;
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT1:
						(new DXT1()).SaveSwizzled2D(Bitmap, this.SliceStream.Slice(), CompressDXT.CompressionMode.Normal);
						break;
					default:
						throw(new NotImplementedException());
				}
			}

			private Bitmap _GenerateBitmap()
			{
				var Bitmap = new Bitmap(Width, Height);

				switch (ImageEntry.ImageFileFormat.TextureFormat)
				{
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_4_4_4_4:
						_GenerateBitmapDecode(Bitmap, (byte[] Bytes, int m) =>
						{
							var Data = (uint)(((Bytes[m++] << 8) | Bytes[m++]));
							return new ARGB_Rev()
							{
								B = (byte)BitUtils.ExtractScaled(Data, 0, 4, 255),
								R = (byte)BitUtils.ExtractScaled(Data, 8, 4, 255),
								G = (byte)BitUtils.ExtractScaled(Data, 4, 4, 255),
								A = (byte)BitUtils.ExtractScaled(Data, 12, 4, 255),
							};
						});
						break;

					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_8_8_8_8:
						_GenerateBitmapDecode(Bitmap, (byte[] Bytes, int m) =>
						{
							var Data = (uint)(((Bytes[m++] << 24) | (Bytes[m++] << 16) | (Bytes[m++] << 8) | Bytes[m++]));
							return new ARGB_Rev()
							{
								B = (byte)BitUtils.ExtractScaled(Data, 0, 8, 255),
								G = (byte)BitUtils.ExtractScaled(Data, 8, 8, 255),
								R = (byte)BitUtils.ExtractScaled(Data, 16, 8, 255),
								A = (byte)BitUtils.ExtractScaled(Data, 24, 8, 255),
							};
						});

						break;

					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5:
					//case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT5A:
						return (new DXT5()).LoadSwizzled2D(this.SliceStream, Width, Height, this.Tiled);
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT1:
						return (new DXT1()).LoadSwizzled2D(this.SliceStream, Width, Height, this.Tiled);
#if false
					case GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_1:
						Console.Error.Write("Unimplemented GPUTEXTUREFORMAT_1");
						Graphics.FromImage(Bitmap).DrawString("Unimplemented GPUTEXTUREFORMAT_1", new Font("Arial", 20), new SolidBrush(Color.Red), new Point(16, 16));
						break;
#endif
					default:
						throw (new NotImplementedException("[Surface2DEntryInfo] Not implemented format : " + ImageEntry.ImageFileFormat.TextureFormat));
				}

				return Bitmap;
			}

			public override string ToString()
			{
				return String.Format("ImageEntryInfo(Name='{0}', ImageEntry={1})", Name, ImageEntry);
			}
		}

		public ImageVersionStruct ImageVersion;
		public ImageHeaderStructV2 ImageHeader;
		public Surface2DEntryInfo[] Surface2DEntries;
		public Surface3DEntryInfo[] Surface3DEntries;
		public Dictionary<string, Surface2DEntryInfo> Surface2DEntriesByName;
		public Dictionary<string, Surface3DEntryInfo> Surface3DEntriesByName;
		Stream TXVStream;

		static public TXM FromTxmTxv(Stream TXMStream, Stream TXVStream)
		{
			return new TXM().Load(TXMStream, TXVStream);
		}

		public TXM Load(Stream TXMStream, Stream TXVStream)
		{
			this.TXVStream = TXVStream;

			this.ImageVersion = TXMStream.Slice().ReadStruct<ImageVersionStruct>();

			if (ImageVersion.Version == 1)
			{
				this.Surface2DEntriesByName = new Dictionary<string, Surface2DEntryInfo>();
				this.Surface2DEntries = new Surface2DEntryInfo[0];
				this.Surface3DEntriesByName = new Dictionary<string, Surface3DEntryInfo>();
				this.Surface3DEntries = new Surface3DEntryInfo[0];
				Console.Error.WriteLine("Not Implemented TXM V1!!!!!!!");
			}
			else
			{
				this.ImageHeader = TXMStream.ReadStruct<ImageHeaderStructV2>();

				this.Surface2DEntriesByName = new Dictionary<string, Surface2DEntryInfo>();
				this.Surface2DEntries = new Surface2DEntryInfo[ImageHeader.Surface2DCount];
				for (int n = 0; n < ImageHeader.Surface2DCount; n++)
				{
					var ImageEntry = TXMStream.ReadStruct<Surface2DInfoStructV2>();
					var Name = TXMStream.SliceWithLength(TXMStream.Position + Marshal.OffsetOf(typeof(Surface2DInfoStructV2), "StringOffset").ToInt32() - sizeof(Surface2DInfoStructV2) + ImageEntry.StringOffset).ReadStringz();
					var Entry = new Surface2DEntryInfo(this, n, ImageEntry, Name);
					this.Surface2DEntries[n] = Entry;
					this.Surface2DEntriesByName[Name] = Entry;
				}

				this.Surface3DEntriesByName = new Dictionary<string, Surface3DEntryInfo>();
				this.Surface3DEntries = new Surface3DEntryInfo[ImageHeader.Surface3DCount];
				if (this.ImageHeader.VersionInfo.Version != 1)
				{
					for (int n = 0; n < ImageHeader.Surface3DCount; n++)
					{
						var ImageEntry = TXMStream.ReadStruct<Surface3DInfoStructV2>();
						var Name = TXMStream.SliceWithLength(TXMStream.Position + Marshal.OffsetOf(typeof(Surface3DInfoStructV2), "StringOffset").ToInt32() - sizeof(Surface3DInfoStructV2) + ImageEntry.StringOffset).ReadStringz();
						var Entry = new Surface3DEntryInfo(this, n, ImageEntry, Name);
						this.Surface3DEntries[n] = Entry;
						this.Surface3DEntriesByName[Entry.Name] = Entry;
					}
				}
			}

			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Stream"></param>
		/// <param name="Width"></param>
		/// <param name="Height"></param>
		/// <returns></returns>
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
			//Bitmap.GetChannelsDataInterleaved
			return Bitmap;
		}

		public void Dispose()
		{
			foreach (var Entry in Surface2DEntries) Entry.Dispose();
			foreach (var Entry in Surface3DEntries) Entry.Dispose();
		}

		public static bool IsValid(byte[] MagicData)
		{
			try
			{
				var ImageHeader = StructUtils.BytesToStruct<ImageVersionStruct>(MagicData);
				if (!(new int[] { 1, 2 }).Contains(ImageHeader.Version)) return false;
				return true;
			}
			catch
			{
				//Console.WriteLine(Exception);
			}
			return false;
		}
	}
}
