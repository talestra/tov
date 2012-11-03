using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using ProtoBuf;

namespace TalesOfVesperiaTranslationUtils
{
	[ProtoContract]
	public class TranslationEntry
	{
		[ProtoContract]
		public class MetaInfo
		{
			[ProtoMember(1)] public int textblock_ptr;
		}

		[ProtoContract]
		public class TranslationInfo
		{
			[ProtoMember(1)] public bool done;
			[ProtoMember(2)] public string user;
			[ProtoMember(3)] public int time;
		}

		[ProtoContract]
		public class Texts
		{
			[ProtoMember(1)] public string[] es;
			[ProtoMember(2)] public string[] en;
			//[ProtoMember(3)] public string[] ja;
		}

		[ProtoContract]
		public class TranslationInfoList
		{
			//[ProtoMember(1)] public TranslationInfo es;
			//[ProtoMember(2)] public TranslationInfo en;
			//[ProtoMember(3)] public TranslationInfo ja;
		}

		[ProtoMember(1)] public string text_path;
		[ProtoMember(2)] public string text_id;
		[ProtoIgnore]
		[ProtoMember(3)]
		public string linked_id;
		//[ProtoMember(4)] public string project;
		[ProtoMember(5)] public Texts texts;
		//[ProtoMember(6)] public TranslationInfoList translated;
		//[ProtoMember(7)] public TranslationInfoList revised;
	}


	[ProtoContract]
	public class TranslationEntryList
	{
		[ProtoMember(1)]
		public TranslationEntry[] Items;

		[ProtoIgnore]
		private Dictionary<string, TranslationEntry> _ItemsById;

		[ProtoIgnore]
		public Dictionary<string, TranslationEntry> ItemsById
		{
			get
			{
				if (_ItemsById == null)
				{
					_ItemsById = Items.CreateDictionary(Item => Item.text_id);
				}

				return _ItemsById;
			}
		}
	}

	public class JsonTranslations
	{
		static public IEnumerable<TranslationEntry> ReadAllJson(Stream Stream)
		{
			var Reader = new StreamReader(Stream);

			while (!Reader.EndOfStream)
			{
				var Line = Reader.ReadLine();
				var Entry = JsonConvert.DeserializeObject<TranslationEntry>(Line);
				yield return Entry;
			}
		}

		static public void JsonToProtocolBuffer(Stream JsonStream, Stream ProtocolStream)
		{
			WriteProto(ProtocolStream, ReadAllJson(JsonStream).ToArray());
		}

		static public void JsonToProtocolBuffer(string JsonFileName, string ProtocolFileName)
		{
			using (var Input = File.Open(JsonFileName, FileMode.Open, FileAccess.Read))
			using (var Output = File.Open(ProtocolFileName, FileMode.Create, FileAccess.Write))
			{
				JsonToProtocolBuffer(Input, Output);
			}
		}

		static public TranslationEntryList ReadProto(Stream Stream)
		{
			return Serializer.Deserialize<TranslationEntryList>(Stream);
		}

		static public void WriteProto(Stream Stream, TranslationEntry[] Items)
		{
			Serializer.Serialize(Stream, new TranslationEntryList()
			{
				Items = Items,
			});

		}
	}
}
