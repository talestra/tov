using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaUtils.Text
{
	public enum Severity
	{
		Notice,
		Warning,
		Error,
	}

	public struct Message
	{
		public Severity Severity;
		public string Text;

		public Message(Severity Severity, int MatchPosition, string Message, params object[] Parameters)
		{
			this.Severity = Severity;
			this.Text = String.Format("{0} at {1}", String.Format(Message, Parameters), MatchPosition);
		}

		public override string ToString()
		{
			return String.Format("{0} - {1}", Severity, Text);
		}
	}
}
