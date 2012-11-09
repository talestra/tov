using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine.Components
{
	public class PatchAll : PatcherComponent
	{
		public PatchAll(Patcher Patcher)
			: base(Patcher)
		{
		}

		public void Handle()
		{
            this.Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("Tales of Vesperia", new Action[] {
                this.CreateProto,
                new StringDic(Patcher).Handle,
                new MapSvo(Patcher).Handle,
                new CharaSvo(Patcher).Handle,
                new BtlSvo(Patcher).Handle,
                new EffectSvo(Patcher).Handle,
                new UiSvo(Patcher).Handle,
                new CommonSvo(Patcher).Handle,
                new ChatSvo(Patcher).Handle,
            });
		}

        public void CreateProto()
        {
            var EntriesByRoom = Patcher.EntriesByRoom;
        }
	}
}
