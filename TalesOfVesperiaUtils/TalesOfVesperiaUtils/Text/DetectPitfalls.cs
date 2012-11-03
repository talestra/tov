using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSharpUtils;

namespace TalesOfVesperiaUtils.Text
{
	/// <summary>
	/// Class that detects problems on the translated text.
	/// 
	/// Sometimes in translated text, translators remove accidentally some special characters or misses some required opcodes.
	/// Or misses spaces at the end of the string and so.
	/// This class allows to detect those kind of problems allowing later testing to be easier and safer.
	/// </summary>
	sealed public class DetectPitfalls
	{
		/// <summary>
		/// 
		/// </summary>
		private Regex ReferencesRegex;

		/// <summary>
		/// 
		/// </summary>
		private Regex ReferencesPitfalledRegex;

		/// <summary>
		/// 
		/// </summary>
        public readonly Logger Logger = Logger.CreateAnonymousLogger();

		/// <summary>
		/// 
		/// </summary>
		public DetectPitfalls()
		{
			//ReferencesRegex = new Regex("(\x02|\x03)(\\(\\w+\\))", RegexOptions.Compiled | RegexOptions.Multiline);
			ReferencesRegex = new Regex(@"[\x01|\x02|\x03|\x04|\x05](\(\w+\))", RegexOptions.Compiled | RegexOptions.Multiline);
			ReferencesPitfalledRegex = new Regex(@"(^|[^\x01|\x02|\x03|\x04|\x05])(\(\w+\))", RegexOptions.Compiled | RegexOptions.Multiline);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Base"></param>
		/// <param name="Modified"></param>
		/// <returns></returns>
		public string Detect(String Base, String Modified)
		{
			DetectAndFix(Base, Modified);
			return Modified;
		}

		public string DetectAndFix(String Base, String Modified)
		{
			Base = Base
				.Replace("\x01", "<01>")
				.Replace("\x02", "<02>")
				.Replace("\x03", "<03>")
				.Replace("\x04", "<STR>")
				.Replace("\x05", "<05>")
				.Replace("\x06", "<06>")
				.Replace("\x07", "<07>")
				.Replace("\x08", "<08>")
				.Replace("\x08", "<VOICE>")
				.Replace("\x0B", "<0B>")
				.Replace("\x0C", "<PAGE>\n")
			;

			Logger.Info("Detecting Pitfalls: '{0}' -> '{1}'", Base, Modified);

			var BaseMatches = ReferencesRegex.Matches(Base);
			var ModifiedMatches = ReferencesRegex.Matches(Modified);
			if (BaseMatches.Count != ModifiedMatches.Count)
			{
				Logger.Warning(
					"Reference mismatch ({0} -> {1}) :: ('{2}' -> '{3}')",
					BaseMatches.Count,
					ModifiedMatches.Count,
					Base,
					Modified
				);

				var PitfallFixerDictionary = new Dictionary<String, String>();
				foreach (Match Match in BaseMatches)
				{
					PitfallFixerDictionary[Match.Groups[1].Value] = Match.Groups[0].Value;
					Logger.Info(
						"ADD_FIX: {0} -> {1}",
						Match.Groups[1].Value,
						Match.Groups[0].Value
					);
					//Match.
				}

				Modified = ReferencesPitfalledRegex.Replace(Modified, delegate(Match Match)
				{
					var PrependValue = Match.Groups[1].Value;
					var BaseValue = Match.Groups[2].Value;
					var AppendValue = "";
					String ReplaceValue = BaseValue;

					if (PitfallFixerDictionary.ContainsKey(BaseValue))
					{
						ReplaceValue = PitfallFixerDictionary[BaseValue];

						Logger.Info(
							"FIXED: '{0}' -> '{1}'",
							BaseValue,
							ReplaceValue
						);
					}
					else
					{
						Logger.Error(
							"CAN'T FIX: '{0}'",
							BaseValue
						);
					}

					return PrependValue + ReplaceValue + AppendValue;
				});

				Logger.Warning(" --> " + Modified);
			}

			return Modified;
		}
	}
}
