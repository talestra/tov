using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSharpUtils;

namespace TalesOfVesperiaUtils.Text
{
	public class DetectPitfalls
	{
		public Regex ReferencesRegex;
		public Regex ReferencesPitfalledRegex;
		public Logger Logger;

		public DetectPitfalls()
		{
			//ReferencesRegex = new Regex("(\x02|\x03)(\\(\\w+\\))", RegexOptions.Compiled | RegexOptions.Multiline);
			Logger = new Logger(CSharpUtils.Logger.Level.Error);
			ReferencesRegex = new Regex(@"[\x01|\x02|\x03|\x04|\x05](\(\w+\))", RegexOptions.Compiled | RegexOptions.Multiline);
			ReferencesPitfalledRegex = new Regex(@"(^|[^\x01|\x02|\x03|\x04|\x05])(\(\w+\))", RegexOptions.Compiled | RegexOptions.Multiline);
		}

		public void Detect(String Base, String Modified)
		{
			DetectAndFix(Base, ref Modified);
		}

		public void DetectAndFix(String Base, ref String Modified)
		{
			Logger.Log(Logger.Level.Info, "Detecting Pitfalls: '{0}' -> '{1}'", Base, Modified);

			var BaseMatches = ReferencesRegex.Matches(Base);
			var ModifiedMatches = ReferencesRegex.Matches(Modified);
			if (BaseMatches.Count != ModifiedMatches.Count)
			{
				Logger.Log(
					Logger.Level.Warning, 
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
					Logger.Log(
						Logger.Level.Info,
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

						Logger.Log(
							Logger.Level.Info,
							"FIXED: '{0}' -> '{1}'",
							BaseValue,
							ReplaceValue
						);
					}
					else
					{
						Logger.Log(
							Logger.Level.Error,
							"CAN'T FIX: '{0}'",
							BaseValue
						);
					}

					return PrependValue + ReplaceValue + AppendValue;
				});

				Logger.Log(Logger.Level.Warning, " --> " + Modified);
			}
		}
	}
}
