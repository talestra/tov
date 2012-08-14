using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace TalesOfVesperiaUtils.Text
{
	/// <summary>
	/// </summary>
	/// <see cref="http://stackoverflow.com/questions/96732/embedding-one-dll-inside-another-as-an-embedded-resource-and-then-calling-it-from"/>
	public class CharacterMapping
	{
		private static CharacterMapping _Instance;
		public static CharacterMapping Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance =  new CharacterMapping();
				}
				return _Instance;
			}
		}

		struct ReplacePair
		{
			public String From;
			public String To;
		}

		List<ReplacePair> ReplacePairs = new List<ReplacePair>();

		private CharacterMapping()
		{
			var XML = XDocument.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("TalesOfVesperiaUtils.CharacterMapping.xml"));
			foreach (var XMLMap in XML.Descendants("map"))
			{
				var From = XMLMap.Attribute("from").Value;
				var To = XMLMap.Attribute("to").Value;
				ReplacePairs.Add(new ReplacePair() { From = From, To = To });
			}
		}

		public String Map(String String)
		{
			foreach (var ReplacePair in ReplacePairs)
			{
				String = String.Replace(ReplacePair.From, ReplacePair.To);
			}
			return String;
		}

		public String Unmap(String String)
		{
			foreach (var ReplacePair in ReplacePairs)
			{
				String = String.Replace(ReplacePair.To, ReplacePair.From);
			}
			return String;
		}
	}
}
