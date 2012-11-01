using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TalesOfVesperiaUtils.Text
{
	/// <summary>
	/// Class that aggregates all the text processing stuff that has to be executed before
	/// inserting the text in game.
	/// </summary>
	sealed public class TextProcessor
	{
		/// <summary>
		/// 
		/// </summary>
		static private TextProcessor _Instance;

		/// <summary>
		/// 
		/// </summary>
		private CharacterMapping CharacterMapping;

		/// <summary>
		/// 
		/// </summary>
		private DetectPitfalls DetectPitfalls;

		private Regex FindCurlyRegex;

		private PitfallDetector PitfallDetector;


		/// <summary>
		/// 
		/// </summary>
		private TextProcessor()
		{
			this.CharacterMapping = CharacterMapping.Instance;
			this.DetectPitfalls = new DetectPitfalls();
			this.PitfallDetector = new PitfallDetector();
			this.FindCurlyRegex = new Regex(@"<(\w+)>", RegexOptions.Compiled);
		}

		/// <summary>
		/// 
		/// </summary>
		static public TextProcessor Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance = new TextProcessor();
				}
				return _Instance;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="String"></param>
		/// <returns></returns>
		public String Process(String String)
		{
			String = String.RegexReplace(@"<(\w+)>", (Groups) => {
				var Key = Groups[1].Value;
				switch (Key)
				{
					case "01": Key = "\x01"; break;
					case "STR": Key = "\x04"; break;
					default: throw (new NotImplementedException(String.Format("Not implemented <{0}>", Key)));
				}
				return Key;
			});
			String = CharacterMapping.Instance.Map(String);
			return String;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Base"></param>
		/// <param name="Modified"></param>
		/// <returns></returns>
		public String ProcessAndDetectPitfalls(String Base, String Modified)
		{
			foreach (var Message in PitfallDetector.Detect(Modified, Base))
			{
				Console.Error.WriteLine("{0}", Message);
			}
			return Process(DetectPitfalls.DetectAndFix(Base, Modified));
		}
	}
}
