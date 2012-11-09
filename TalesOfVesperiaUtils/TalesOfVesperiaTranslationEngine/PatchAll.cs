using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine
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
                new StringDic.StringDic(Patcher).Handle,
                new MapSvo.MapSvo(Patcher).Handle,
                new CharaSvo.CharaSvo(Patcher).Handle,
                new BtlSvo.BtlSvo(Patcher).Handle,
                new EffectSvo.EffectSvo(Patcher).Handle,
                new UiSvo.UiSvo(Patcher).Handle,
                new CommonSvo.CommonSvo(Patcher).Handle,
                new ChatSvo.ChatSvo(Patcher).Handle,
            });
		}

        public void CreateProto()
        {
            var EntriesByRoom = Patcher.EntriesByRoom;
        }
	}
}
