using CSharpUtils;
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
		//private Regex FindCurlyRegex;

		/// <summary>
		/// 
		/// </summary>
		private PitfallDetector PitfallDetector;


		/// <summary>
		/// 
		/// </summary>
		private TextProcessor()
		{
			this.CharacterMapping = CharacterMapping.Instance;
			this.PitfallDetector = new PitfallDetector();
			//this.FindCurlyRegex = new Regex(@"<(\w+)>", RegexOptions.Compiled);
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
			String = String.RegexReplace(@"(<PAGE>)\s+", (Groups) =>
			{
				return Groups[1].Value;
			});
			String = String.RegexReplace(@"<(\w+)>", (Groups) => {
				var Key = Groups[1].Value;
				switch (Key)
				{
					case "01": Key = "\x01"; break;
					case "02": Key = "\x02"; break;
					case "03": Key = "\x03"; break;
					case "STR": Key = "\x04"; break;
					case "05": Key = "\x05"; break;
					case "06": Key = "\x06"; break;
					case "07": Key = "\x07"; break;
					case "08": Key = "\x08"; break;
					case "VOICE": Key = "\x09"; break;
					case "0B": Key = "\x0B"; break;
					case "PAGE": Key = "\x0C"; break;
					case "shop": Key = "<shop>"; break;
					case "inn": Key = "<inn>"; break;
					case "con": Key = "<con>"; break;
					case "mini": Key = "<mini>"; break;
					default: throw (new NotImplementedException(String.Format("Not implemented <{0}>", Key)));
				}
				return Key;
			});
			// Non-breaking-space -> space
			String = String.Replace("\u00A0", " ");
			String = CharacterMapping.Instance.Map(String);
			return String;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Original"></param>
		/// <param name="Modified"></param>
		/// <returns></returns>
		public String ProcessAndDetectPitfalls(String Original, String Modified, bool ThrowExceptions = false)
		{
			var Messages = PitfallDetector.Detect(Modified, Original);
			foreach (var Message in Messages) Console.Error.WriteLine("{0}", Message);
			if (ThrowExceptions && Messages.Count > 0) throw (new Exception(Messages.Implode(",")));
			var DetectPitfalls = new DetectPitfalls();
			DetectPitfalls.Logger.Enabled = true;
			DetectPitfalls.Logger.OnLog += (Level, Message, Stack) =>
			{
				if (Level == Logger.Level.Info) return;
				//ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
				{
					Console.Error.WriteLine("{0} - {1}", Level, Message);
				}
				//);
			};
			var Text2 = DetectPitfalls.DetectAndFix(Original, Modified);
			return Process(Text2);
		}
	}
}
