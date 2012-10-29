using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine
{
	public class PatcherComponent
	{
		protected Patcher Patcher;

		public PatcherComponent(Patcher Patcher)
		{
			this.Patcher = Patcher;
		}
	}
}
