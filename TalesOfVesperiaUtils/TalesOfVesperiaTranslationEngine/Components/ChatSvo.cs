using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Compression;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.Text;

namespace TalesOfVesperiaTranslationEngine.Components
{
	public class ChatSvo : PatcherComponent
	{
		public ChatSvo(Patcher Patcher)
			: base(Patcher)
		{
		}

        FPS4 TranslatedChatSvo;
        FPS4 OriginalChatSvo;
        const string PreppendTempFile = "CHAT_ES/";

        public void Handle()
        {
			Patcher.Action("chat.svo", () =>
			{
                TranslatedChatSvo = new FPS4(Patcher.GameFileSystem.OpenFileRead("chat.svo")).ClearAllEntries();
                OriginalChatSvo = new FPS4(Patcher.GameFileSystem.OpenFileRead("chat.svo"));

                Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("chat.svo",
                    Handle1,
                    Handle2,
                    Handle3
                );
            });
        }

		private void Handle1()
		{
			Patcher.TempFS.CreateDirectory("CHAT_ES", 0777, false);

			Patcher.Action("Translating Skits...", () =>
			{
                Patcher.ProgressHandler.AddProgressLevel("Traduciendo skits", OriginalChatSvo.Count(), () =>
                {
                    Patcher.ParallelForeach("Translating", OriginalChatSvo, (ChatSvoEntry) => ChatSvoEntry.Name, (ChatSvoEntry) =>
                    {
                        var Match = ChatSvoEntry.Name.RegexMatch(@"^(VC\d+B?)(UK)\.DAT$");
                        if (Match != null)
                        {
                            var CompleteFile = Match[0].Value;
                            var ChatId = Match[1].Value;
                            var EsFile = PreppendTempFile + ChatId + "ES.DAT";

                            Console.WriteLine("{0}...", ChatId);

                            if (!Patcher.TempFS.Exists(EsFile))
                            {
                                var Fps4 = new FPS4(TalesCompression.DecompressStream(OriginalChatSvo[CompleteFile].Open()));
                                {
                                    var Chtx = new TO8CHTX();
                                    Chtx.Load(Fps4["3"].Open());
                                    // Translate
                                    {
                                        foreach (var Entry in Patcher.EntriesByRoom["skits/" + ChatId].Values)
                                        {
                                            int TextId = int.Parse(Entry.text_id) - 1;
                                            if (TextId >= 0)
                                            {
                                                //Chtx[TextId].Title = "";
                                                //Chtx[TextId].Title = TextProcessor.Instance.ProcessAndDetectPitfalls(Chtx[TextId].Title, Entry.texts.es[0].TrimEnd(' ', '\t', '\n', '\r', '.'));
                                                //Chtx[TextId].Title = TextProcessor.Instance.ProcessAndDetectPitfalls(Chtx[TextId].Title, Entry.texts.es[0]);
                                                Chtx[TextId].Title = "";
                                                Chtx[TextId].TextOriginal = "";
                                                Chtx[TextId].TextTranslated = TextProcessor.Instance.ProcessAndDetectPitfalls(Chtx[TextId].TextTranslated, Entry.texts.es[1]);
                                            }
                                        }
                                    }
                                    //ChtxStream.SetLength(0);
                                    var ChtxStream = new MemoryStream();
                                    Chtx.SaveTo(ChtxStream);
                                    ChtxStream.Position = 0;
                                    Fps4["3"].SetStream(ChtxStream);
                                }
                                Patcher.TempFS.WriteAllBytes(EsFile, TalesCompression.CreateFromVersion(Patcher.CompressionVersion, Patcher.CompressionFallback).EncodeBytes(Fps4.Save(false).ToArray()));
                                Console.WriteLine("{0}...Ok", ChatId);
                            }
                            else
                            {
                                Console.WriteLine("{0}...Exists", ChatId);
                            }
                        }

                        Patcher.ProgressHandler.IncrementLevelProgress();
                    });
                });
			});
		}

        private void Handle2()
        {
            Patcher.Action("Building Spanish chat.svo...", () =>
            {
                if (!Patcher.TempFS.Exists("chat.es.svo"))
                {
                    foreach (var ChatSvoEntry in OriginalChatSvo)
                    {
                        var VcMatch = ChatSvoEntry.Name.RegexMatch(@"^(VC\d+B?)(\w{2})\.DAT$");
                        if (VcMatch != null)
                        {
                            if (VcMatch[2].Value == "UK")
                            {
                                var EsEntry = TranslatedChatSvo.CreateEntry(VcMatch[1] + "DE.DAT", Patcher.TempFS.OpenFile(PreppendTempFile + VcMatch[1].Value + "ES.DAT", FileMode.Open));
                                foreach (var Ext in new[] { "FR", "UK", "US" })
                                {
                                    var FileName = VcMatch[1] + Ext + ".DAT";
                                    if (OriginalChatSvo.Entries.ContainsKey(FileName))
                                    {
                                        TranslatedChatSvo.CreateEntry(FileName, EsEntry);
                                    }
                                }
                            }
                            else
                            {
                                if (!OriginalChatSvo.Entries.ContainsKey(VcMatch[1] + "UK.DAT"))
                                {
                                    TranslatedChatSvo.CreateEntry(ChatSvoEntry.Name, ChatSvoEntry.Open());
                                }
                            }
                        }
                        else
                        {
                            TranslatedChatSvo.CreateEntry(ChatSvoEntry.Name, ChatSvoEntry.Open());
                        }
                    }

                    using (var EsSvoStream = Patcher.TempFS.OpenFile("chat.es.svo", FileMode.Create))
                    {
                        TranslatedChatSvo.SaveTo(EsSvoStream, true);
                    }
                }
            });
        }

        private void Handle3()
        {
            Patcher.ProgressHandler.AddProgressLevel("Actualizando chat.svo", 1, () =>
            {
                Patcher.Action("Updating chat.svo...", () =>
                {
                    Patcher.GameFileSystem.ReplaceFileWithStream("chat.svo", Patcher.TempFS.OpenFileRead("chat.es.svo"), (Current, Total) =>
                    {
                        Patcher.ProgressHandler.SetLevelProgressTo(Current, Total);
                    });
                });
            });
#if false
			//Patcher.TempFS.OpenFileRead("chat.es.svo").CopyTo(Patcher.GameFileSystem.OpenFileRW("chat.svo"));

			//Patcher.TempFS.WriteAllBytes("VC001US.DAT", TalesCompression.DecompressStream(ChatSvo["VC001US.DAT"].Open()).ReadAll());
			Patcher.GameAccessPath("chat.svo", () =>
			{
				var ChatId = "VC001";
				//Patcher.GameGetFile(ChatId + "US.DAT", (File) => { });

				Patcher.GameAccessPath(ChatId + "US.DAT", () =>
				{
					Patcher.GameGetFile("3", (ChtxStream) =>
					{
						var Chtx = new TO8CHTX();
						Chtx.Load(ChtxStream);
						// Translate
						{
							foreach (var Entry in Patcher.EntriesByRoom["skits/" + ChatId])
							{
								int TextId = int.Parse(Entry.text_id) - 1;
								if (TextId >= 0)
								{
									Chtx[TextId].Title = Entry.texts.es[0].TrimEnd(' ', '\t', '\n', '\r', '.');
									Chtx[TextId].TextOriginal = "";
									Chtx[TextId].TextTranslated = Entry.texts.es[1];
								}
							}
						}
						//ChtxStream.SetLength(0);
						ChtxStream.Position = 0;
						Chtx.Save(ChtxStream);
					});
					//Console.WriteLine("Done");
				});
			});
#endif
        }
    }
}
