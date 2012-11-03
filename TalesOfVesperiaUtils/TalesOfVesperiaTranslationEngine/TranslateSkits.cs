#define REUSE_PAK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TalesOfVesperiaUtils.Formats.Packages;
using System.IO;
using CSharpUtils;
using TalesOfVesperiaUtils.Compression;
using System.Text.RegularExpressions;
using TalesOfVesperiaUtils.Formats;
using TalesOfVesperiaUtils.Text;
using System.Threading;

namespace TalesOfVesperiaTranslationEngine
{
#if false
	public class TranslateSkits
	{
		FPS4 PAK;
		ACME1 ACME1;
		Stream PAKOutput;
		TalesCompression TalesCompression;

		static public String TestInputFolder = @"C:\projects\svn.tales-tra.com\csharp\TalesOfVesperiaUtils\TestInput";
		static public String TestOutputFolder = @"C:\projects\svn.tales-tra.com\csharp\TalesOfVesperiaUtils\TestOutput";
		//static public String OriginalFile = @"J:\games\vesperia\chat.original.svo";
		//static public String TranslatedFile = @"J:\games\vesperia\chat.svo";
		static public String OriginalFile = @"c:\tov\chat.svo.bak";
		static public String TranslatedFile = @"c:\tov\chat.svo";

		static public void Backup()
		{
			if (!File.Exists(OriginalFile))
			{
				Console.Write("Backuping... '{0}' -> '{1}'...", TranslatedFile, OriginalFile);
				File.Copy(TranslatedFile, OriginalFile);
				Console.Write("Ok");
			}
		}

		public TranslateSkits(String ChatSvoOriginal, String ChatSvoTranslate, String AcmeSkits)
		{
			//Backup();
			Init(
				StreamChatSvoOriginal: File.OpenRead(ChatSvoOriginal),
				StreamChatSvoTranslate: File.Open(ChatSvoTranslate, FileMode.Create),
				StreamAcmeSkits: File.OpenRead(AcmeSkits)
			);
		}

		public TranslateSkits()
		{
			Backup();
			Init(
				StreamChatSvoOriginal: File.OpenRead(OriginalFile),
				StreamChatSvoTranslate: File.Open(TranslatedFile, FileMode.Create),
				StreamAcmeSkits: File.OpenRead(TestInputFolder + @"\tov_skits.zip")
			);
		}

		public TranslateSkits(Stream StreamChatSvoOriginal, Stream StreamChatSvoTranslate, Stream StreamAcmeSkits)
		{
			Init(StreamChatSvoOriginal, StreamChatSvoTranslate, StreamAcmeSkits);
		}

		protected void Init(Stream StreamChatSvoOriginal, Stream StreamChatSvoTranslate, Stream StreamAcmeSkits)
		{
			//TalesCompression = new TalesCompression1_3(3);
			TalesCompression = new TalesCompression15_Lzx();
			PAK = new FPS4(StreamChatSvoOriginal);
			PAKOutput = StreamChatSvoTranslate;
			ACME1 = new ACME1(StreamAcmeSkits, Encoding.GetEncoding("ISO-8859-1"));
		}

		protected void DisplayAllEntries()
		{
			var SelectAll = new Regex("^(VC.*)(FR|DE|US|UK)(.DAT)$");
			var Lists = new Dictionary<String, List<FPS4.Entry>>();

			foreach (var Entry in PAK.EntryList.Where(Entry => SelectAll.IsMatch(Entry.Name)))
			{
				var Hash = SelectAll.Match(Entry.Name).Groups[1].Value;
				//var SpanishName = SelectAll.Replace("VC100BUK.DAT", "$1ES$3");
				//Lists[Entry]
				if (!Lists.ContainsKey(Hash))
				{
					Lists[Hash] = new List<FPS4.Entry>();
				}
				Lists[Hash].Add(Entry);
			}

			foreach (var List in Lists)
			{
				foreach (var Entry in List.Value)
				{
					Console.Write("," + Entry.Name);
				}
				Console.WriteLine("");
			}

			Console.WriteLine();
			return;
		}

		protected void HandleEntry(Regex SelectAll, Func<String, String, String> ReplaceSuffixName, FPS4.Entry EnglishEntry)
		{
			//Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
			using (EnglishEntry)
			{
				//Console.WriteLine("[1]");
				var BaseName = SelectAll.Match(EnglishEntry.Name).Groups[1].Value;
				var SpanishName = ReplaceSuffixName(EnglishEntry.Name, "ES");
				Console.WriteLine("{0} : {1}", SpanishName, Thread.CurrentThread.ManagedThreadId);

				var ACMEFiles = ACME1.FilesByIndex.Where(Item => Item.Name.Contains(BaseName));
				if (ACMEFiles.Count() == 1)
				{
					//Console.WriteLine("[2]");

					var EsTempFile = TestOutputFolder + @"\" + SpanishName;

#if REUSE_PAK
					if (!File.Exists(EsTempFile))
#endif
					{
						using (var CompressedSkitPAKStream = File.Open(EsTempFile, FileMode.Create))
						{
							//Console.WriteLine("[3]");
							var ACMEFile = ACMEFiles.ElementAt(0);
							//Console.WriteLine(SpanishName);
							using (var SkitPAK = new FPS4(TalesCompression.DecompressStream(EnglishEntry.Open())))
							using (var chtx = new TO8CHTX(SkitPAK[3].Open()))
							{
								//Console.WriteLine("[4]");
								try
								{
									chtx.TranslateWithAcmeFile(ACMEFile);
								}
								catch (Exception Exception)
								{
									Console.Error.WriteLine(Exception);
									Console.ReadKey();
								}

								//Console.WriteLine("[5]");
								using (var NewChtxStream = new MemoryStream())
								using (var SkitPAKStream = new MemoryStream())
								{
									chtx.Save(NewChtxStream);
									SkitPAK[3].SetStream(NewChtxStream);

									//Console.WriteLine("[6]");
									SkitPAK.Save(SkitPAKStream);

									TalesCompression.EncodeFile(SkitPAKStream, CompressedSkitPAKStream);
								}
							}
						}
					}
					var SpanishEntry = PAK.CreateEntry(SpanishName, File.Open(EsTempFile, FileMode.Open));
				}
				else
				{
					Console.WriteLine("WARNING. Untranslated Skit: ACMEFiles.Count() : {0} : {1}", ACMEFiles.Count(), BaseName);
					var SpanishEntry = PAK.CreateEntry(SpanishName, EnglishEntry.Open());
				}
			}
		}

		public void Process()
		{

			//DisplayAllEntries();
			var SelectAll = new Regex("^(VC.*)(FR|DE|US|UK)(.DAT)$");
			Func<String, String, String> ReplaceSuffixName = delegate(String Base, String Suffix) {
				return SelectAll.Replace(Base, "$1" + Regex.Escape(Suffix) + "$3");
			};

			Console.WriteLine("Main Thread: {0}", Thread.CurrentThread.ManagedThreadId);

			//Console.WriteLine("[a]");
//#if DEBUG
#if false
			foreach (var Entry in PAK.EntryList.Where(Entry => new Wildcard("*UK.DAT").IsMatch(Entry.Name)).ToArray())
			{
				HandleEntry(SelectAll, ReplaceSuffixName, Entry);
			}
#else
			PAK.EntryList.Where(Entry => new Wildcard("*UK.DAT").IsMatch(Entry.Name)).AsParallel().ForAll((Entry) => {
				HandleEntry(SelectAll, ReplaceSuffixName, Entry);
			});
#endif

			foreach (var Entry in PAK.EntryList.Where(Item => new Regex("^.*(FR|DE|US|UK).DAT$").IsMatch(Item.Name)))
			{
				var SpanishName = ReplaceSuffixName(Entry.Name, "ES");

				//Console.WriteLine(Entry.Name + " -> " + SpanishName);
				if (PAK.Entries.ContainsKey(SpanishName))
				{
					Entry.LinkedTo = PAK.Entries[SpanishName];
				}
				else
				{
					Entry.SetStream(new MemoryStream());
				}
				//Console.WriteLine(Entry);
				//Console.WriteLine(Item.Name + " -> " + LinkedName);
			}

			PAK.Save(PAKOutput);
		}
	}
#endif
}
