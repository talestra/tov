using CSharpUtils.VirtualFileSystem;
using CSharpUtils.VirtualFileSystem.Local;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine
{
	public class Patcher
	{
		public string PatcherPath;
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

		public Patcher()
		{
			PatcherPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			//PatcherDataFS = new LocalFileSystem(PatcherPath + "/../../Images", false);
			PatcherDataFS = new LocalFileSystem(PatcherPath + "/../../PatchData", false);
			TempFS = new LocalFileSystem(PatcherPath + "/Temp", true);
			//JsonTranslations.JsonToProtocolBuffer();
		}

		int Level = 0;

		public void Action(String Description, Action Action)
		{
			Console.WriteLine("{0}{1}...", new String(' ', Level * 2), Description);
			Level++;
			try
			{
				Action();
				//Console.WriteLine("Ok");
			}
			finally
			{
				Level--;
			}
		}
	}
}
