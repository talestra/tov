using CSharpUtils.VirtualFileSystem;
using CSharpUtils.VirtualFileSystem.Local;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

		public void ParallelForeach<T>(string Verb, IEnumerable<T> List, Func<T, string> GetNameFunc, Action<T> EachAction)
		{
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
		public Patcher(string GamePath)
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

			Init(GameRootFS);
			CheckPatchPaths();
		}

		public Patcher(FileSystem GameFileSystem)
		{
			Init(GameFileSystem);
		}

		private void Init(FileSystem GameFileSystem)
		{
			this.GameFileSystem = GameFileSystem;
			this.PatcherPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			//PatcherDataFS = new LocalFileSystem(PatcherPath + "/../../Images", false);
			this.PatcherDataFS = new LocalFileSystem(PatcherPath + "/../../PatchData", false);
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

		public void GameAccessPath(string Path, Action ActionAccess)
		{
			this.Action(String.Format("Accessing {0}", Path), () =>
			{
				this.PatchInplace.Access(Path, ActionAccess);
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
	}
}
