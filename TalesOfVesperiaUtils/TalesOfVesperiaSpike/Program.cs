using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TalesOfVesperiaUtils.Formats.Packages;
using System.IO;
using CSharpUtils;
using TalesOfVesperiaTranslationEngine;
using TalesOfVesperiaUtils.Imaging;
using System.Drawing;
using ProtoBuf;
using Newtonsoft.Json;
using TalesOfVesperiaUtils.Compression;
using CSharpUtils.Streams;

#if false
namespace TalesOfVesperiaSpike
{
	class Program
	{
		[ProtoContract]
		class Person
		{
			[ProtoMember(1)]
			public string Name;
		}

#if false
		static private void JsonToProtocol()
		{

			var JsonFileName = @"C:\projects\projects\talestra\csharp\TalesOfVesperiaUtils\TestInput\tov.json";
			var ProtocolFileName = @"C:\projects\projects\talestra\csharp\TalesOfVesperiaUtils\TestInput\tov.protocol";

			/*
			Console.WriteLine("[1]");
			var Items = JsonTranslations.ReadAll(File.OpenRead()).ToArray();
			foreach (var Entry in Items)
			{
				//Console.WriteLine(Entry.texts["es"][0]);
			}
			*/

			Console.WriteLine("[1]");
			/*
			JsonTranslations.JsonToProtocolBuffer(JsonFileName, ProtocolFileName");
			*/
			var List = JsonTranslations.ReadProto(File.OpenRead(ProtocolFileName));
			foreach (var Entry in List.Items)
			{
				foreach (var Text in Entry.texts.es)
				{
					var Messages = PitfallDetector.Detect(Text);
					if (Messages.Count > 0)
					{
						Console.WriteLine(Text);
						Console.WriteLine(JsonConvert.SerializeObject(Messages));
					}
				}
			}
			//List.Items.

			Console.WriteLine("[2]");
			//JsonTranslations.WriteProto(File.OpenWrite("test2.bin"), List.Items);

			Console.WriteLine("[3]");
		}
#endif

		static MemoryStream DecompressLzx(Stream input)
		{
			var reader = new BinaryReader(input);

			long num = reader.ReadInt32();

            // ... let's decompress it!
            // get the decompressed size (num is our compressed size)
            int decompressedSize = reader.ReadInt32();

			Console.WriteLine("{0} / {1}", num, decompressedSize);

            // create a memory stream of that size
            MemoryStream output = new MemoryStream(decompressedSize);

            // save our initial position
            long pos = input.Position;
            // default window size for XNB encoded files is 64Kb (need 16 bits to represent it)
            LzxDecoder decoder = new LzxDecoder(16);
            // start our decode process
			while (pos < num)
			{
				// the compressed stream is seperated into blocks that will decompress
				// into 32Kb or some other size if specified.
				// normal, 32Kb output blocks will have a short indicating the size
				// of the block before the block starts
				// blocks that have a defined output will be preceded by a byte of value
				// 0xFF (255), then a short indicating the output size and another
				// for the block size
				// all shorts for these cases are encoded in big endian order
				int hi, lo, block_size, frame_size;
				// let's get the first byte
				hi = reader.ReadByte();
				// does this block define a frame size?
				if (hi == 0xFF)
				{
					// get our bytes
					hi = reader.ReadByte();
					lo = reader.ReadByte();
					// make a beautiful short baby together
					frame_size = (hi << 8) | lo;
					// let's read the block size
					hi = reader.ReadByte();
					lo = reader.ReadByte();
					block_size = (hi << 8) | lo;
					// add the read in bytes to the position
					pos += 5;
				}
				else
				{
					// just block size, so let's read the rest
					lo = reader.ReadByte();
					block_size = (hi << 8) | lo;
					// frame size is 32Kb by default
					frame_size = 32768;
					// add the read in bytes to the position
					pos += 2;
				}

				Console.WriteLine("{0} - {1}", block_size, frame_size);

				// either says there is nothing to decode
				if (block_size == 0 || frame_size == 0)
				//if (frame_size == 0)
				{
					break;
				}

				// let's decompress the sucker
				decoder.Decompress(input, block_size, output, frame_size);

				// let's increment the input position by the block size
				pos += block_size;
				// reset the position of the input just incase the bit buffer
				// read in some unused bytes
				input.Seek(pos, SeekOrigin.Begin);
			}

			return output;
		}

		public struct RGBA {
			public byte A, R, G, B;
		}

		static void DecodeImage(string basename)
		{
			var Lzx = new LzxDecoder(16);
			var Stream = new MemoryStream(File.ReadAllBytes(basename));
			var Reader = new BinaryReader(Stream);

			Stream.Position = 6;
			var OutStream = DecompressLzx(Stream);
			Console.WriteLine(OutStream.Length);
			File.WriteAllBytes(basename + ".u", OutStream.ToArray());

#if true
			//OutStream.Position = 0x30A;
			//var Bitmap = DXT5.LoadSwizzled(OutStream, 0x40, 0x80, Swizzled: true);
			//Bitmap.Save(basename + ".u.png");

			OutStream.Position = 0x37;
			var Reader2 = new BinaryReader(OutStream);
			var Format = Reader2.ReadInt32();
			var Width = Reader2.ReadInt32();
			var Height = Reader2.ReadInt32();
			var Levels = Reader2.ReadInt32();

			for (int Level = 0; Level < Levels; Level++)
			{

				OutStream.Position = 0x4B;
				//var Bitmap = new Bitmap(256, 512);
				//var Bitmap = new Bitmap(1024, 128);
				//var Bitmap = new Bitmap(512, 256);
				//var Bitmap = new Bitmap(1024, 1024);
				//var Bitmap = new Bitmap(128, 128);
				var Bitmap = new Bitmap(Width, Height);
				//var Bitmap = new Bitmap(64, 64);


				//var BitmapData = OutStream.ReadStructVector<RGBA>((uint)(Bitmap.Width * Bitmap.Height));
				//var BitmapData = OutStream.ReadBytes(Bitmap.Width * Bitmap.Height * 4);
				int Size = Bitmap.Width * Bitmap.Height;
				for (int n = 0; n < Size; n++)
				{
					int x, y;
					Swizzling.UnswizzledXY(n, Bitmap.Width, 4, out x, out y);
					var RGBA = OutStream.ReadStruct<RGBA>();
					Bitmap.SetPixel(x, y, Color.FromArgb(0xFF, RGBA.R, RGBA.G, RGBA.B));
				}
				//var Bitmap = DXT5.LoadSwizzled(OutStream, 0x40, 0x80, Swizzled: true);
				//Bitmap.SetChannelsDataLinear(BitmapData, BitmapChannel.Red, BitmapChannel.Green, BitmapChannel.Blue, BitmapChannel.Alpha);
				//Bitmap.SetChannelsDataLinear(BitmapData, BitmapChannel.Alpha, BitmapChannel.Green, BitmapChannel.Blue, BitmapChannel.Red);
				Bitmap.Save(basename + ".u." + Level + ".png");
			}
#endif

		}

		static void Main(string[] args)
		{
			//var basename = @"C:\Juegos\fez\Content\Essentials.pak.d\fonts\zuish";
			//var basename = @"C:\Juegos\fez\Content\Essentials.pak.d\effects\basicposteffect";
			//var basename = @"C:\Juegos\fez\Content\Other.pak.d\background planes\qr_crypt";

			//DecodeImage(@"C:\Juegos\fez\Content\Other.pak.d\art objects\menu_cube");
			//Environment.Exit(0);

			//var Path = @"C:\Juegos\fez\Content\";
			var Path = @"C:\Juegos\fez\Content\Other.pak.d\art objects";
			foreach (var filename in Directory.EnumerateFiles(Path, "*", SearchOption.AllDirectories))
			{
				Console.WriteLine("{0}...", filename);
				try
				{
					DecodeImage(filename);
				}
				catch (Exception Exception)
				{
					//Console.WriteLine(Exception);
				}
			}
			
			
			//BE 5A 00 00
			//93 AA 04 00

			// 0x5ABE
			// 0xAA93

			/*
			var Out = new MemoryStream();
			Lzx.Decompress(Stream, 0x5ABE, Out, 0xAA93);
			Console.WriteLine(Out.Length);
			*/

			Console.ReadKey();

#if false
			//var FilePath = @"I:\isos\360\Tales of Vesperia [PAL] [Multi3] [English] [Xbox360].iso";
			var FilePath = @"C:\tov\tov_spa.iso";
			var Dvd9Xbox360 = new Dvd9Xbox360();

			using (var IsoStream = File.Open(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				Dvd9Xbox360.Load(IsoStream);

				using (var map_svo_stream = Dvd9Xbox360.RootEntry["/map.svo"].Open())
				{
					var map_svo = new FPS4(map_svo_stream);

					map_svo["SCENARIO.DAT"].ReplaceWithFile(@"c:\tov\language\scenario_uk.dat");
				}

				Dvd9Xbox360.RootEntry["/language/scenario_us.dat"].ReplaceWithFile(@"c:\tov\language\scenario_uk.dat");
				Dvd9Xbox360.RootEntry["/language/scenario_uk.dat"].ReplaceWithFile(@"c:\tov\language\scenario_uk.dat");
				Dvd9Xbox360.RootEntry["/language/string_dic_uk.so"].ReplaceWithFile(@"c:\tov\language\string_dic_uk.so");
				Dvd9Xbox360.RootEntry["/UI.svo"].ReplaceWithFile(@"c:\tov\ui.svo");
				Dvd9Xbox360.RootEntry["/chat.svo"].ReplaceWithFile(@"c:\tov\chat.svo");
			}

			Console.WriteLine("Done. Press any key to continue.");
			Console.ReadKey();

			return;

#if true

			var TranslateSkits = new TranslateSkits(@"c:\tov\chat.svo.bak", @"c:\tov\chat.svo", @"tov_skits.zip");
			//var TranslateSkits = new TranslateSkits();
			//TranslateSkits.Backup();
			TranslateSkits.Process();

			new UpdateFont().Process(@"c:\tov\UI.svo", "FONTTEX10.TXV.mod.png");

			/*
			Console.WriteLine("Decoding...");
			//var Fps4 = new FPS4(File.Open(@"F:\Isos\360\games\vesperia\UI.svo", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
			var Fps4 = new FPS4(File.Open(@"J:\games\vesperia\UI.svo", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
			using (var Out = Fps4["FONTTEX10.TXV"].Open())
			{
				DXT5.SaveSwizzled(new Bitmap(Image.FromFile("../../../TestInput/FONTTEX10.TXV.mod.png")), Out);
			}
			Console.WriteLine("Ok");
			*/

			/*
			//var Bitmap = DXT5.LoadSwizzled(File.OpenRead("../../../TestInput/FONTTEX05.TXV.mod.TXV"), 2048, 2048);
			using (var Out = File.OpenWrite("../../../Lol.txv"))
			{
				DXT5.SaveSwizzled(new Bitmap(Image.FromFile("../../../TestInput/FONTTEX10.TXV.mod.png")), Out);
			}
			*/
			//Bitmap.Save("../../../Lol.png");
#else

			var TranslateSkits = new TranslateSkits();
			TranslateSkits.Backup();
			TranslateSkits.Process();
			/*
			var FPS4 = new FPS4();
			FPS4.Load(File.OpenRead(@"J:\games\vesperia\chat.svo"));

			Console.WriteLine(FPS4.Entries["VC980US.DAT"].CompressedStream.Length);
			
			FPS4.Entries["VC980US.DAT"].CompressedStream.CopyTo(
				File.OpenWrite(@"C:\projects\svn.tales-tra.com\csharp\TalesOfVesperiaUtils\TestInput\VC980US.DAT"),
				8 * 1024 * 1024
			);
			*/
#endif
#endif
			Console.WriteLine("Done. Press any key to continue.");
			Console.ReadKey();
		}
	}
}
#endif

namespace TalesOfVesperiaSpike
{
	class Utils
	{
		static public string TestInputPath = "../../../TestInput";
	}

	public class Program
	{
		static void UncompressUiImage(string BaseFileName)
		{
			//Console.WriteLine();
			//break;

			//var BaseFileName = "COMALL";

			var Txm = (new TXM()).Load(
				File.OpenRead(@"C:\isos\360\vesperia\UI.svo.d\" + BaseFileName + ".TXM"),
				File.OpenRead(@"C:\isos\360\vesperia\UI.svo.d\" + BaseFileName + ".TXV")
			);
			//Console.WriteLine(Txm.ImageEntries.ToStringArray("\n"));
			Console.WriteLine("{0}:", BaseFileName);
			foreach (var ImageEntry in Txm.Surface2DEntries)
			{
				var ImageEntryFileName = @"C:\isos\360\vesperia\UI.svo.d\images\" + ImageEntry.Name + ".png";
				Console.Write("  {0}.png : {1}...", ImageEntry.Name, ImageEntry.ImageEntry.ImageFileFormat.TextureFormat);
				Console.WriteLine(ImageEntry.ImageEntry);
				if (!File.Exists(ImageEntryFileName))
				{
					ImageEntry.Bitmap.Save(ImageEntryFileName);
					Console.WriteLine("Ok");
				}
				else
				{
					Console.WriteLine("Exists");
				}
			}

			foreach (var ImageEntry in Txm.Surface3DEntries)
			{
				var ImageEntryFileNameTest = @"C:\isos\360\vesperia\UI.svo.d\images\" + ImageEntry.Name + "." + 0 + ".png";

				Console.Write("  {0}...", ImageEntry.Name);

				if (!File.Exists(ImageEntryFileNameTest))
				{
					int n = 0;
					foreach (var Bitmap in ImageEntry.Bitmaps.Bitmaps)
					{
						var ImageEntryFileName = @"C:\isos\360\vesperia\UI.svo.d\images\" + ImageEntry.Name + "." + n + ".png";
						Console.Write("  {0}.png : {1}...", ImageEntry.Name, ImageEntry.ImageEntry.ImageFileFormat.TextureFormat);
						//Console.WriteLine(ImageEntry.ImageEntry);

						if (!File.Exists(ImageEntryFileName))
						{
							Bitmap.Save(ImageEntryFileName);
							Console.WriteLine("Ok");
						}
						else
						{
							Console.WriteLine("Exists");
						}
						n++;
					}
					Console.WriteLine("Ok");
				}
				else
				{
					Console.WriteLine("Exists");
				}
			}
		}

		static void Main(string[] Args)
		{
			//var Image = TXM.LoadAbgr(File.OpenRead(@"C:\isos\360\vesperia\UI.svo.d\COMALL.TXV"), 256, 1024);

			//var BaseFileName = "COOK_BEAFSTEW";
			//var BaseFileName = "WORLDMAP";

#if true
			foreach (var BaseFullFileName in Directory.GetFiles(@"C:\isos\360\vesperia\UI.svo.d", "*.TXM", SearchOption.TopDirectoryOnly))
			{
				UncompressUiImage(new FileInfo(BaseFullFileName).Name.Substr(0, -4));
			}
#else
			UncompressUiImage("FONTTEX00");
#endif

			Console.ReadKey();
			//Image.Save(@"C:\isos\360\vesperia\UI.svo.d\COMALL.TXV.png");
		}
	}
}