﻿using CSharpUtils;
using CSharpUtils.VirtualFileSystem;
using CSharpUtils.VirtualFileSystem.Local;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using TalesOfVesperiaTranslationUtils;
using TalesOfVesperiaUtils;
using TalesOfVesperiaUtils.Compression;
using TalesOfVesperiaUtils.Imaging;
using TalesOfVesperiaUtils.VirtualFileSystem;

namespace TalesOfVesperiaTranslationEngine
{
    public class Patcher : IDisposable
    {
        //public int CompressionVersion = 15;
        public int CompressionVersion = 3;
        public int CompressionFallback = 3;

        public PatchInplace PatchInplace;
        public string PatcherPath;
        public FileSystem GameFileSystem;
        public FileSystem TempFS;
        public FileSystem PatcherDataFS;
        private Dictionary<string, Dictionary<string, TranslationEntry>> _EntriesByRoom;
        public ProgressHandler ProgressHandler = new ProgressHandler();

        //En caso de true, poner el Build Action de "PatchData.bin" a None. En caso de false, poner a Embedded Resource.
        public bool ExternalPatchData = false;

        public Dictionary<string, Dictionary<string, TranslationEntry>> EntriesByRoom
        {
            get
            {
                lock (this)
                {
                    if (_EntriesByRoom == null)
                    {
                        _EntriesByRoom = new Dictionary<string, Dictionary<string, TranslationEntry>>();

                        var TovProto = "tov.proto";
                        var TovJson = PatchPaths.TextsTovJson;

                        if (!TempFS.Exists(TovProto))
                        {
                            JsonTranslations.JsonToProtocolBuffer(PatcherDataFS.OpenFileRead(TovJson), TempFS.OpenFile(TovProto, FileMode.Create));
                        }

                        foreach (var Entry in JsonTranslations.ReadProto(TempFS.OpenFileRead(TovProto)).Items)
                        {
                            if (!_EntriesByRoom.ContainsKey(Entry.text_path)) _EntriesByRoom[Entry.text_path] = new Dictionary<string, TranslationEntry>();
                            _EntriesByRoom[Entry.text_path][Entry.text_id] = Entry;
                        }
                    }

                    return _EntriesByRoom;
                }
            }
        }

        public Patcher CheckPatchPaths()
        {
            foreach (var Field in typeof(PatchPaths).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var FieldFullName = Field.DeclaringType.Name + "." + Field.Name;
                var Path = (string)Field.GetValue(null);
                if ((Path != null) && (Path != "") && this.PatcherDataFS.Exists(Path))
                {
                    //Console.WriteLine("{0}: '{1}'... Ok", FieldFullName, Path);
                }
                else
                {
                    throw (new Exception(String.Format("{0}: Path doesn't exists '{1}'", FieldFullName, Path)));
                }
            }

            return this;
        }

        public void Foreach<T>(string Verb, IEnumerable<T> List, Func<T, string> GetNameFunc, Action<T> EachAction)
        {
            var OldActionLevel = this.ActionLevel;
            List.ForEach((Item) =>
            {
                this.Action(String.Format("{0} {1}", Verb, GetNameFunc(Item)), () =>
                {
                    EachAction(Item);
                }, OldActionLevel);
            });
        }

        public void ParallelForeach<T>(string Verb, IEnumerable<T> List, Func<T, string> GetNameFunc, Action<T> EachAction)
        {
            //Foreach(Verb, List.AsParallel(), GetNameFunc, EachAction);
            var OldActionLevel = this.ActionLevel;
            List.AsParallel().ForEach((Item) =>
            {
                this.Action(String.Format("{0} {1}", Verb, GetNameFunc(Item)), () =>
                {
                    EachAction(Item);
                }, OldActionLevel);
            });
        }

        Stream GameIsoStream = null;
        public Patcher(string GamePath = null)
        {
            InitCommon();
            if (GamePath != null) InitWithGamePath(GamePath);
        }

        public Patcher(FileSystem GameFileSystem)
        {
            InitCommon();
            InitWithFileSystem(GameFileSystem);
        }

        private void InitCommon()
        {
            this.ProgressHandler.OnProgressUpdated += () =>
            {
                if (Progress != null) Progress(this.ProgressHandler);
            };
        }

        public void InitWithGamePath(string GamePath)
        {
            FileSystem GameRootFS;
            if (Directory.Exists(GamePath))
            {
                GameRootFS = new LocalFileSystem(GamePath);
            }
            else
            {
                this.GameIsoStream = File.Open(GamePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                GameRootFS = new Dvd9Xbox360FileSystem(this.GameIsoStream);
            }

            InitWithFileSystem(GameRootFS);
            CheckPatchPaths();
        }

        private void InitWithFileSystem(FileSystem GameFileSystem)
        {
            this.GameFileSystem = GameFileSystem;
            this.PatcherPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //PatcherDataFS = new LocalFileSystem(PatcherPath + "/../../Images", false);

            if (ExternalPatchData)
            {
                foreach (var PatchDataPath in new[] { PatcherPath + "/PatchData", PatcherPath + "/../PatchData", PatcherPath + "/../../PatchData"})
                {
                    if (Directory.Exists(PatchDataPath))
                    {
                        this.PatcherDataFS = new LocalFileSystem(PatchDataPath, false);
                        break;
                    }
                }
            }
            else
            {
                string PatchDataPath = Path.Combine(Path.GetTempPath(), "PatchData");
                if (Directory.Exists(PatchDataPath))
                {
                    this.PatcherDataFS = new LocalFileSystem(PatchDataPath, false);
                }
            }

            if (this.PatcherDataFS == null)
            {
                throw (new Exception("Can't find PatchData"));
            }
            this.TempFS = new LocalFileSystem(PatcherPath + "/Temp", true);
            this.PatchInplace = new PatchInplace(GameFileSystem);
            //JsonTranslations.JsonToProtocolBuffer();
        }

        public void GameSetFileSystem(FileSystem FileSystem, Action ActionAccess)
        {
            this.Action(String.Format("SettingFileSystem {0}", FileSystem), () =>
            {
                this.PatchInplace.SetFileSystem(FileSystem, ActionAccess);
            });
        }

        public void GameAccessPath(string Path, Action ActionAccess, int RecompressVersion = -1)
        {
            this.Action(String.Format("Accessing {0}", Path), () =>
            {
                this.PatchInplace.Access(Path, ActionAccess, RecompressVersion);
            });
        }

        public void GameGetFile(string File1, Action<Stream> ActionRead)
        {
            this.Action(String.Format("File {0}", File1), () =>
            {
                this.PatchInplace.GetFile(File1, ActionRead);
            });
        }

        public void GameReplaceFile(string File1, Stream NewStream)
        {
            this.Action(String.Format("Update {0}", File1), () =>
            {
                this.GameGetFile(File1, (OldStream) =>
                {
                    OldStream.WriteStream(NewStream.Slice());
                    //Console.WriteLine("ZeroBytes: {0}", OldStream.Length - OldStream.Position);
                    OldStream.WriteByteRepeatedTo((byte)0x00, OldStream.Length);
                    OldStream.Flush();
                });
            });
        }

        public void GameGetTXM(string BaseTxmTxv, Action<TXM> ActionRead)
        {
            GameGetTXM(BaseTxmTxv + ".TXM", BaseTxmTxv + ".TXV", ActionRead);
        }

        public void GameGetTXM(string FileTxm, string FileTxv, Action<TXM> ActionRead)
        {
            GameGetFile2(FileTxm, FileTxv, (StreamTxm, StreamTxv) =>
            {
                ActionRead(new TXM().Load(StreamTxm, StreamTxv));
            });
        }

        public void GameGetFile2(string File1, string File2, Action<Stream, Stream> ActionRead)
        {
            this.Action(String.Format("Files {0}, {1}", File1, File2), () =>
            {
                this.PatchInplace.GetFile2(File1, File2, ActionRead);
            });
        }

        public void UpdateTxm2DWithImage(TXM Txm, Bitmap Bitmap, params string[] TxmNames)
        {
            foreach (var Name in TxmNames)
            {
                Txm.Surface2DEntriesByName[Name].UpdateBitmap(Bitmap);
            }
        }

        static private int ColorDistance(Color a, Color b)
        {
            return (
                Math.Abs((int)a.R - (int)b.R) +
                Math.Abs((int)a.G - (int)b.G) +
                Math.Abs((int)a.B - (int)b.B)
            ) / 3;
        }

        class RleItem
        {
            public Color Color;
            public int Count;

            public RleItem(Color Color, int Count)
            {
                this.Color = Color;
                this.Count = Count;
            }
        }

        private void ReduceRle(List<RleItem> Input)
        {
            for (int n = 1; n < Input.Count - 1; n++)
            {
                if (
                    (Input[n].Count < (Input[n - 1].Count + Input[n + 1].Count) * 0.1)
                )
                {
                    if (
                        (ColorDistance(Input[n].Color, Input[n - 1].Color) < 64) &&
                        (ColorDistance(Input[n].Color, Input[n + 1].Color) < 64)
                    )
                    {
                        Input[n - 1].Count += Input[n].Count;
                        Input.RemoveAt(n);
                        n--;
                        continue;
                    }
                }
            }
        }

        //private void ReduceRowColors(Bitmap Bitmap)
        //{
        //	for (int y = 0; y < Bitmap.Height; y++)
        //	{
        //		var Rle = new List<RleItem>();
        //		for (int x = 0; x < Bitmap.Width - 1; x++)
        //		{
        //			int R = Bitmap.GetPixel(x + 0, y).R;
        //			int G = Bitmap.GetPixel(x + 0, y).G;
        //			int B = Bitmap.GetPixel(x + 0, y).B;
        //			int Count = 1;
        //			while ((x < Bitmap.Width - 1) && (ColorDistance(Bitmap.GetPixel(x, y), Bitmap.GetPixel(x + 1, y)) < 6))
        //			{
        //				R += Bitmap.GetPixel(x + 1, y).R;
        //				G += Bitmap.GetPixel(x + 1, y).G;
        //				B += Bitmap.GetPixel(x + 1, y).B;
        //				Count++;
        //				x++;
        //			}
        //			var Color2 = Color.FromArgb(R / Count, G / Count, B / Count);
        //			Rle.Add(new RleItem(Color2, Count));
        //			//Console.WriteLine("{0}: {1}", Color2, Count);
        //		}
        //		int x2 = 0;
        //		ReduceRle(Rle);
        //		foreach (var Item in Rle)
        //		{
        //			var Color = Item.Color;
        //			var Count = Item.Count;
        //			while (Count-- > 0) Bitmap.SetPixel(x2++, y, Color);
        //		}
        //	}
        //}
        //
        //public void UpdateTxm2DWithPngDxt5Loosy(TXM Txm, string PatchPngPath, params string[] TxmNames)
        //{
        //	PatcherGetImage(PatchPngPath, (Bitmap) =>
        //	{
        //		//Bitmap = new DXT5().CompressColors(Bitmap);
        //		ReduceRowColors(Bitmap);
        //
        //		Bitmap.Save(@"c:\temp\temp." + TxmNames[0] + ".png");
        //		foreach (var Name in TxmNames)
        //		{
        //			Txm.Surface2DEntriesByName[Name].UpdateBitmap(Bitmap);
        //		}
        //	});
        //}

        public void UpdateTxm2DWithPng(TXM Txm, string PatchPngPath, params string[] TxmNames)
        {
            PatcherGetImage(PatchPngPath, (Bitmap) =>
            {
                foreach (var Name in TxmNames)
                {
                    Txm.Surface2DEntriesByName[Name].UpdateBitmap(Bitmap);
                }
            });
        }

        public void UpdateTxm2DWithEmpty(TXM Txm, params string[] TxmNames)
        {
            foreach (var Name in TxmNames)
            {
                var Entry = Txm.Surface2DEntriesByName[Name];
                Entry.UpdateBitmap(new Bitmap(Entry.Width, Entry.Height));
            }
        }

        public void PatcherGetImageColorAlpha(string FileColor, string FileAlpha, Action<Bitmap> ActionRead)
        {
            PatcherGetFile(FileColor, (StreamColor) =>
            {
                PatcherGetFile(FileAlpha, (StreamAlpha) =>
                {
                    var BitmapColor = new Bitmap(Image.FromStream(StreamColor));
                    var BitmapAlpha = new Bitmap(Image.FromStream(StreamAlpha));
                    if (BitmapColor.Size != BitmapAlpha.Size) throw (new InvalidDataException("Sizes from alpha and color must match"));
                    var ComposedBitmap = new Bitmap(BitmapColor.Width, BitmapColor.Height, PixelFormat.Format32bppArgb);
                    ComposedBitmap.SetChannelsDataLinear(new[] {
						new BitmapChannelTransfer() { Bitmap = BitmapColor, From = BitmapChannel.Red, To = BitmapChannel.Red },
						new BitmapChannelTransfer() { Bitmap = BitmapColor, From = BitmapChannel.Green, To = BitmapChannel.Green },
						new BitmapChannelTransfer() { Bitmap = BitmapColor, From = BitmapChannel.Blue, To = BitmapChannel.Blue },
						new BitmapChannelTransfer() { Bitmap = BitmapAlpha, From = BitmapChannel.Red, To = BitmapChannel.Alpha },
					});
                    //ComposedBitmap.Save(@"c:\projects\1.png");
                    ActionRead(ComposedBitmap);
                });
            });
        }

        public void PatcherGetImage(string File1, Action<Bitmap> ActionRead)
        {
            PatcherGetFile(File1, (Stream) =>
            {
                ActionRead(new Bitmap(Image.FromStream(Stream)));
            });
        }

        public void PatcherGetFile(string File1, Action<Stream> ActionRead)
        {
            this.Action(String.Format("Patcher File {0}", File1), () =>
            {
                using (var Stream = this.PatcherDataFS.OpenFileRead(File1))
                {
                    ActionRead(Stream);
                }
            });
        }

        int ActionLevel = 0;
        public event Action<ProgressHandler> Progress;

        public void Action(String Description, Action Action, int OverrideActionLevel = -1)
        {
            if (OverrideActionLevel >= 0) ActionLevel = OverrideActionLevel;

            Console.WriteLine("{0}{1}...", new String(' ', ActionLevel * 2), Description);

            int OldActionLevel = ActionLevel;
            ActionLevel++;
            try
            {
                Action();
                //Console.WriteLine("Ok");
            }
            finally
            {
                ActionLevel = OldActionLevel;
            }
        }

        public void Dispose()
        {
            if (GameIsoStream != null)
            {
                GameIsoStream.Dispose();
            }
        }

        public void DecompressRecompressStreamIfRequired(Stream CompressedStream, Action<Stream> Action, int RecompressVersion = -1, bool RecompressJustWhenModified = true)
        {
            if (TalesCompression.DetectVersion(CompressedStream.Slice()) == -1)
            {
                Action(CompressedStream);
            }
            else
            {
                using (var UncompressedStream = new DecompressRecompressStream(CompressedStream, RecompressVersion, RecompressJustWhenModified))
                {
                    Action(UncompressedStream);
                }
            }
        }

        public void PatchAll()
        {

        }
    }
}
