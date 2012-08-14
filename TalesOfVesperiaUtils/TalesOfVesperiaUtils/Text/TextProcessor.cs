using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TalesOfVesperiaUtils.Text
{
	public class TextProcessor
	{
		static protected TextProcessor _Instance;
		protected CharacterMapping CharacterMapping;
		protected DetectPitfalls DetectPitfalls;

		public TextProcessor()
		{
			this.CharacterMapping = CharacterMapping.Instance;
			this.DetectPitfalls = new DetectPitfalls();
		}

		static public TextProcessor Get()
		{
			if (_Instance == null)
			{
				_Instance = new TextProcessor();
			}
			return _Instance;
		}

		public String Process(String String)
		{
			return CharacterMapping.Instance.Map(String);
		}

		public String ProcessAndDetectPitfalls(String Base, String Modified)
		{
			DetectPitfalls.DetectAndFix(Base, ref Modified);
			return Process(Modified);
		}
	}
}
