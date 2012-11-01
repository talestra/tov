using CSharpUtils.VirtualFileSystem;
using CSharpUtils.VirtualFileSystem.Local;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils;
using TalesOfVesperiaUtils.Imaging;

namespace TalesOfVesperiaTranslationEngine
{
	public class Patcher
	{
		public PatchInplace PatchInplace;
		public string PatcherPath;
		public FileSystem GameFileSystem;
		public FileSystem TempFS;
		public FileSystem PatcherDataFS;
		private Dictionary<string, List<TranslationEntry>> _EntriesByRoom;
		public Dictionary<string, List<TranslationEntry>> EntriesByRoom
		{
			get
			{
				if (_EntriesByRoom == null)
				{
					_EntriesByRoom = new Dictionary<string, List<TranslationEntry>>();

					var TovProto = "tov.proto";
					var TovJson = "Data/tov.json";

					if (!TempFS.Exists(TovProto))
					{
						JsonTranslations.JsonToProtocolBuffer(PatcherDataFS.OpenFileRead(TovJson), TempFS.OpenFile(TovProto, FileMode.Create));
					}

					foreach (var Entry in JsonTranslations.ReadProto(TempFS.OpenFileRead(TovProto)).Items)
					{
						if (!_EntriesByRoom.ContainsKey(Entry.text_path)) _EntriesByRoom[Entry.text_path] = new List<TranslationEntry>();
						_EntriesByRoom[Entry.text_path].Add(Entry);
					}
				}

				return _EntriesByRoom;
			}
		}

		[DebuggerHidden]
		public Patcher(FileSystem GameFileSystem)
		{
			this.GameFileSystem = GameFileSystem;
			this.PatcherPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			//PatcherDataFS = new LocalFileSystem(PatcherPath + "/../../Images", false);
			this.PatcherDataFS = new LocalFileSystem(PatcherPath + "/../../PatchData", false);
			this.TempFS = new LocalFileSystem(PatcherPath + "/Temp", true);
			this.PatchInplace = new PatchInplace(GameFileSystem);
			//JsonTranslations.JsonToProtocolBuffer();
		}

		[DebuggerHidden]
		public void GameAccessPath(string Path, Action ActionAccess)
		{
			this.Action(String.Format("Accessing {0}", Path), () =>
			{
				this.PatchInplace.Access(Path, ActionAccess);
			});
		}

		[DebuggerHidden]
		public void GameGetFile(string File1, Action<Stream> ActionRead)
		{
			this.Action(String.Format("File {0}", File1), () =>
			{
				this.PatchInplace.GetFile(File1, ActionRead);
			});
		}

		[DebuggerHidden]
		public void GameGetTXM(string FileTxm, string FileTxv, Action<TXM> ActionRead)
		{
			GameGetFile2(FileTxm, FileTxv, (StreamTxm, StreamTxv) =>
			{
				ActionRead(new TXM().Load(StreamTxm, StreamTxv));
			});
		}

		[DebuggerHidden]
		public void GameGetFile2(string File1, string File2, Action<Stream, Stream> ActionRead)
		{
			this.Action(String.Format("Files {0}, {1}", File1, File2), () =>
			{
				this.PatchInplace.GetFile2(File1, File2, ActionRead);
			});
		}

		[DebuggerHidden]
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

		[DebuggerHidden]
		public void UpdateTxm2DWithEmpty(TXM Txm, params string[] TxmNames)
		{
			foreach (var Name in TxmNames)
			{
				var Entry = Txm.Surface2DEntriesByName[Name];
				Entry.UpdateBitmap(new Bitmap(Entry.Width, Entry.Height));
			}
		}

		[DebuggerHidden]
		public void PatcherGetImage(string File1, Action<Bitmap> ActionRead)
		{
			PatcherGetFile(File1, (Stream) =>
			{
				ActionRead(new Bitmap(Image.FromStream(Stream)));
			});
		}

		[DebuggerHidden]
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

		[DebuggerHidden]
		public void Action(String Description, Action Action)
		{
			Console.WriteLine("{0}{1}...", new String(' ', ActionLevel * 2), Description);
			ActionLevel++;
			try
			{
				Action();
				//Console.WriteLine("Ok");
			}
			finally
			{
				ActionLevel--;
			}
		}
	}
}
