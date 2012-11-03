using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalesOfVesperiaUtils.Compression;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.Text;

namespace TalesOfVesperiaTranslationEngine.ChatSvo
{
	public class ChatSvo : PatcherComponent
	{
		public ChatSvo(Patcher Patcher)
			: base(Patcher)
		{
		}

		public void Handle()
		{
			int CompressionVersion = 15;
			int FallbackVersion = 3;
			var TranslatedChatSvo = new FPS4(Patcher.GameFileSystem.OpenFileRead("chat.svo")).ClearAllEntries();
			var OriginalChatSvo = new FPS4(Patcher.GameFileSystem.OpenFileRead("chat.svo"));

			OriginalChatSvo
				.AsParallel()
				.ForEach((ChatSvoEntry) =>
			{
				var Match = ChatSvoEntry.Name.RegexMatch(@"^(VC\d+B?)(UK)\.DAT$");
				if (Match == null) return;

				var CompleteFile = Match[0].Value;
				var ChatId = Match[1].Value;
				var EsFile = ChatId + "ES.DAT";
				
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
					Patcher.TempFS.WriteAllBytes(EsFile, TalesCompression.CreateFromVersion(CompressionVersion, FallbackVersion).EncodeBytes(Fps4.Save(false).ToArray()));
					Console.WriteLine("{0}...Ok", ChatId);
				}
				else
				{
					Console.WriteLine("{0}...Exists", ChatId);
				}
			});


			foreach (var ChatSvoEntry in OriginalChatSvo)
			{
				var VcMatch = ChatSvoEntry.Name.RegexMatch(@"^(VC\d+B?)(\w{2})\.DAT$");
				if (VcMatch != null)
				{
					if (VcMatch[2].Value == "UK")
					{
#if false
						var EsEntry = TranslatedChatSvo.CreateEntry(VcMatch[1] + "ES.DAT", Patcher.TempFS.OpenFile(VcMatch[1].Value + "ES.DAT", FileMode.Open));
						TranslatedChatSvo.CreateEntry(VcMatch[1] + "FR.DAT", EsEntry);
						TranslatedChatSvo.CreateEntry(VcMatch[1] + "DE.DAT", EsEntry);
						TranslatedChatSvo.CreateEntry(VcMatch[1] + "UK.DAT", EsEntry);
						TranslatedChatSvo.CreateEntry(VcMatch[1] + "US.DAT", EsEntry);
#else
						var EsEntry = TranslatedChatSvo.CreateEntry(VcMatch[1] + "DE.DAT", Patcher.TempFS.OpenFile(VcMatch[1].Value + "ES.DAT", FileMode.Open));
						foreach (var Ext in new[] { "FR", "UK", "US" })
						{
							var FileName = VcMatch[1] + Ext + ".DAT";
							if (OriginalChatSvo.Entries.ContainsKey(FileName))
							{
								TranslatedChatSvo.CreateEntry(FileName, EsEntry);
							}
						}
#endif
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

			if (!Patcher.TempFS.Exists("chat.es.svo"))
			{
				using (var EsSvoStream = Patcher.TempFS.OpenFile("chat.es.svo", FileMode.Create))
				{
					TranslatedChatSvo.SaveTo(EsSvoStream, true);
				}
			}

			Patcher.GameFileSystem.ReplaceFileWithStream("chat.svo", Patcher.TempFS.OpenFileRead("chat.es.svo"));
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
