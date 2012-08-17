using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace TalesOfVesperiaUtils.Text
{
	/// <summary>
	/// This class allows to perform the character mapping required to be able to use spanish characters on the game.
	/// It uses the CharacterMapping.xml that is embedded in this DLL. Actual process is described as a XML comment in that file.
	/// </summary>
	/// <seealso cref="CharacterMapping.xml"/>
	/// <see cref="http://stackoverflow.com/questions/96732/embedding-one-dll-inside-another-as-an-embedded-resource-and-then-calling-it-from"/>
	sealed public class CharacterMapping
	{
		/// <summary>
		/// 
		/// </summary>
		private static CharacterMapping _Instance;

		/// <summary>
		/// Gets the singleton instance.
		/// </summary>
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

		/// <summary>
		/// A Struct with the replacing pairs.
		/// </summary>
		private struct ReplacePair
		{
			public String From;
			public String To;
		}

		/// <summary>
		/// 
		/// </summary>
		private List<ReplacePair> ReplacePairs = new List<ReplacePair>();
		
		/// <summary>
		/// 
		/// </summary>
		//private Dictionary<string, string> FromTo = new Dictionary<string, string>();
		
		/// <summary>
		/// 
		/// </summary>
		//private Dictionary<string, string> ToFrom = new Dictionary<string, string>();

		/// <summary>
		/// Constructor. Processes the XML file with the mapping and updates the internal structures in order to enable Map/Unmap methods.
		/// </summary>
		private CharacterMapping()
		{
			var XML = XDocument.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("TalesOfVesperiaUtils.CharacterMapping.xml"));
			foreach (var XMLMap in XML.Descendants("map"))
			{
				var From = XMLMap.Attribute("from").Value;
				var To = XMLMap.Attribute("to").Value;
				ReplacePairs.Add(new ReplacePair() { From = From, To = To });
				//FromTo[From] = To;
				//FromTo[To] = From;
			}
		}

		/// <summary>
		/// Performs a From -&lt; To conversion.
		/// </summary>
		/// <param name="String"></param>
		/// <returns></returns>
		public String Map(String String)
		{
			foreach (var ReplacePair in ReplacePairs)
			{
				String = String.Replace(ReplacePair.From, ReplacePair.To);
			}
			return String;
		}

		/// <summary>
		/// Performs a To -&lt; From conversion.
		/// </summary>
		/// <param name="String"></param>
		/// <returns></returns>
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
