using CSharpUtils.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Formats.Script;

namespace TalesOfVesperiaTranslationEngine.Components
{
	public class StringDic : PatcherComponent
	{
		public StringDic(Patcher Patcher)
			: base(Patcher)
		{
		}

		public void Handle()
		{
            this.Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("string_dic", new Action[] {
                this.Handle1,
                this.Handle2,
                this.Handle3,
            });
		}

        private void Handle1()
        {
            if (!Patcher.TempFS.Exists("string_dic_uk.so"))
            {
                FileSystem.CopyFile(Patcher.GameFileSystem, "language/string_dic_uk.so", Patcher.TempFS, "string_dic_uk.so");
            }

            FileSystem.CopyFile(Patcher.TempFS, "string_dic_uk.so", Patcher.TempFS, "string_dic_es.so.temp");
        }

        private void Handle2()
        {
            if (!Patcher.TempFS.Exists("string_dic_es.so"))
            {
                var TotalCount = Patcher.EntriesByRoom.Where(Room => Room.Key.StartsWith("misc/")).Sum(Item => Item.Value.Count);
                this.Patcher.ProgressHandler.AddProgressLevel("Translating String.dic", TotalCount, () =>
                {
                    long OriginalLength = Patcher.TempFS.GetFileInfo("string_dic_uk.so").Size;
                    long TranslatedLength = OriginalLength;

                    Patcher.TempFS.OpenFileRWScope("string_dic_es.so.temp", (TssStream) =>
                    {
                        var Tss = new TSS().Load(TssStream);
                        Tss.TranslateTexts((TextEntry) =>
                        {
                            this.Patcher.ProgressHandler.IncrementLevelProgress();

                            var RoomPath = String.Format("misc/{0}", (TextEntry.Id / 1000) * 1000);
                            var TextId = String.Format("{0:D8}", TextEntry.Id);

                            var TranslationRoom = Patcher.EntriesByRoom[RoomPath];
                            var TranslationEntry = TranslationRoom[TextId];

                            TextEntry.TranslateWithTranslationEntry(TranslationEntry);
                        }, (String) => {
							return null;
						}, HandleType1: true, AddAdditionalSpace: true);

                        var TssTranslatedStream = Tss.Save();
                        OriginalLength = TssStream.Length;
                        TranslatedLength = TssTranslatedStream.Length;

                        TssStream.Position = 0;
                        TssStream.WriteStream(TssTranslatedStream.Slice()).WriteByteRepeatedTo(0x00, TssStream.Length);
                    });

                    Console.WriteLine("Old: {0}", OriginalLength);
                    Console.WriteLine("New: {0}", TranslatedLength);

                    if (TranslatedLength > OriginalLength)
                    {
                        throw (new Exception(String.Format("Translated string_dic is bigger than the original one. {0} > {1}", TranslatedLength, OriginalLength)));
                    }

                    Patcher.TempFS.MoveFile("string_dic_es.so.temp", "string_dic_es.so", true);
                });
            }
        }

        private void Handle3()
        {
            Patcher.ProgressHandler.AddProgressLevel("Replace string_dic_uk.so", 1, () =>
            {
                Patcher.GameFileSystem.ReplaceFileWithStream("language/string_dic_uk.so", Patcher.TempFS.OpenFileRead("string_dic_es.so"), (Current, Total) =>
                {
                    Patcher.ProgressHandler.SetLevelProgressTo(Current, Total);
                });
            });
        }
	}
}
