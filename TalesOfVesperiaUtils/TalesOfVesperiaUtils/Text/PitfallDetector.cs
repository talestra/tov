using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TalesOfVesperiaUtils.Text
{
	public class PitfallDetector
	{
		static Regex ExtractTagRegex = new Regex(@"<(\w+)>(\(([\w]+?)\))?", RegexOptions.Compiled | RegexOptions.Multiline);

		List<Message> Messages = new List<Message>();

		private void AddMessage(Severity Severity, int MatchPosition, string Message, params object[] Parameters)
		{
			Messages.Add(new Message(Severity, MatchPosition, Message, Parameters));
		}

		static public List<Message> Detect(string Translated, string Original = null)
		{
			return new PitfallDetector().DetectInternal(Translated, Original);
		}

		protected List<Message> DetectInternal(string Translated, string Original = null)
		{
			var Matches = ExtractTagRegex.Matches(Translated);

			foreach (var Match in Matches.Cast<Match>())
			{
				//Console.WriteLine(Match);
				var Type = Match.Groups[1].Value;
				var Parameter = Match.Groups[3].Value;
				var MatchPosition = Match.Index;
				switch (Type)
				{
					case "inn":
					case "con":
					case "shop":
					case "mini":
						break;

					case "PAGE":
						break;
					case "01":
						break;
					case "02":
						switch (Parameter)
						{
							case "0": // ?
							case "1": // ?
							//case "4": // ?
							case "6": // ?
								break;
							default:
								AddMessage(Severity.Error, MatchPosition, "Invalid 02 parameter '{0}'", Parameter);
								break;
						}
						break;
					case "03": // Color?
						switch (Parameter)
						{
							case "0": // ?
							case "2": // ?
							case "4": // ?
							case "6": // ?
								break;
							default:
								AddMessage(Severity.Error, MatchPosition, "Invalid 03 parameter '{0}'", Parameter);
								break;
						}
						break;
					case "0B":
						switch (Parameter)
						{
							case "0": // ?
							case "1": // ?
							case "2": // ?
							case "3": // ?
							case "4": // ?
							case "5": // ?
								break;
							default:
								AddMessage(Severity.Error, MatchPosition, "Invalid 0B parameter '{0}'", Parameter);
								break;
						}
						break;
					case "VOICE":
						if (Parameter == "") AddMessage(Severity.Error, MatchPosition, "Voice parameter doesn't have a parameter");
						break;
					case "STR":
						switch (Parameter)
						{
							case "FRE": // Flynn
							case "RAP": // Rapede
							case "RAV": // Raven
							case "BAU": // ?
							case "JUD": // Judith
							case "JUD_P": // Judith ?
							case "YUR": // Yuri
							case "KAR": // Karl
							case "RIT": // Rita
							case "EST": // Estellise
							case "PAT": // Paty?
							case "EST_P": // Estellise Princess?
							break;
							default:
							AddMessage(Severity.Error, MatchPosition, "Invalid STR parameter '{0}'", Parameter);
								break;
						}
						break;
					case "06":
						switch (Parameter)
						{
							case "ATK": // Attack Button

							case "SEL": // Select

							case "MEN": // Menu?

							case "LTR": // ?
							case "LTL": // ?
							case "LTU": // ?
							case "LTD": // ?
							case "LTN": // ?

							case "LBH": // ?
							case "LBN": // ?
							case "LBU": // ?
							case "LBD": // ?
							case "LBR": // ?
							case "LBL": // ?

							case "RTD": // ?
							case "RTN": // ?
							case "RTH": // ?
							case "RTR": // ?
							case "RTL": // ?
							case "RTU": // ?

							case "RBL": // ?
							case "RBU": // ?
							case "RBD": // ?
							case "RBR": // ?

							case "L1": // ?
							case "L2": // ?
							case "L3": // ?

							case "R1": // ?
							case "R2": // ?
							case "R3": // ?


							case "GUD": // ?
							case "ART": // Art Button

							case "SC1": // ?
							case "SC2": // ?
							case "SC3": // ?
							case "SC4": // ?
							case "SC5": // ?
							case "SC6": // ?
							case "SC7": // ?

							case "ST1": // ?
							case "ST2": // ?
							case "ST3": // ?
							case "ST4": // ?
							case "ST5": // ?
							case "ST6": // ?
							case "ST7": // ?
							case "STA": // ?

							case "MC1": // ?
							case "MC2": // ?
							case "MC3": // ?
							case "MC4": // ?
							case "MC5": // ?
							case "MC6": // ?
							case "MC7": // ?
							case "MC8": // ?
							case "MC9": // ?

							case "FS1": // ?
							case "FS2": // ?
							case "FS3": // ?

							case "EL1": // ?
							case "EL2": // ?
							case "EL3": // ?
							case "EL4": // ?
							case "EL5": // ?
							case "EL6": // ?
							case "EL7": // ?

							case "CBL1": // ?
							case "CBL2": // ?
							case "CBR1": // ?
							case "CBR2": // ?

								break;
							default:
								AddMessage(Severity.Error, MatchPosition, "Invalid 06 parameter '{0}'", Parameter);
								break;
						}
						break;
					default:
						AddMessage(Severity.Error, MatchPosition, "Unknown opcode type '{0}' with parameter '{1}'", Type, Parameter);
						break;
				}
				//Console.WriteLine(Type + " : " + Parameter);
			}
			return Messages;
		}
	}
}
