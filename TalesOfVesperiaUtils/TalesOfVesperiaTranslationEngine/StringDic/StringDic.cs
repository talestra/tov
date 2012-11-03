using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Formats.Script;

namespace TalesOfVesperiaTranslationEngine.StringDic
{
	public class StringDic : PatcherComponent
	{
		public StringDic(Patcher Patcher)
			: base(Patcher)
		{
		}

		public void Handle()
		{
			if (!Patcher.TempFS.Exists("string_dic_uk.so"))
			{
				FileSystem.CopyFile(Patcher.GameFileSystem, "language/string_dic_uk.so", Patcher.TempFS, "string_dic_uk.so");
			}

			FileSystem.CopyFile(Patcher.TempFS, "string_dic_uk.so", Patcher.TempFS, "string_dic_es.so.temp");

			if (!Patcher.TempFS.Exists("string_dic_es.so"))
			{
				Patcher.TempFS.OpenFileRWScope("string_dic_es.so.temp", (TssStream) =>
				{
					var Tss = new TSS().Load(TssStream);
					Tss.TranslateTexts((TextEntry) =>
					{
						var RoomPath = String.Format("misc/{0}", (TextEntry.Id / 1000) * 1000);
						var TextId = String.Format("{0:D8}", TextEntry.Id);

						var TranslationRoom = Patcher.EntriesByRoom[RoomPath];
						var TranslationEntry = TranslationRoom[TextId];


						TextEntry.TranslateWithTranslationEntry(TranslationEntry);
					});

					var TssTranslatedStream = Tss.Save();
					Console.WriteLine("Old: {0}", TssStream.Length);
					Console.WriteLine("New: {0}", TssTranslatedStream.Length);

					TssStream.Position = 0;
					TssStream.WriteStream(TssTranslatedStream.Slice()).WriteByteRepeatedTo(0x00, TssStream.Length);
				});

				Patcher.TempFS.MoveFile("string_dic_es.so.temp", "string_dic_es.so", true);
			}

			Patcher.GameFileSystem.ReplaceFileWithStream("language/string_dic_uk.so", Patcher.TempFS.OpenFileRead("string_dic_es.so"));
		}
	}
}
