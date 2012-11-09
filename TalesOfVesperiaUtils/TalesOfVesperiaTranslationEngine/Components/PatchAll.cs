using CSharpUtils.VirtualFileSystem;
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

        public enum GameVersion
        {
            None,
            Other,
            TOV_EUR,
            TOV_Unknown,
            TOV_USA,
            TOV_JAP,
        }

        public void CheckFileSystemVesperiaExceptions(FileSystem Iso)
        {
            switch (CheckFileSystemVesperia(Iso))
            {
                case PatchAll.GameVersion.TOV_EUR:
                    break;
                case PatchAll.GameVersion.TOV_USA:
                case PatchAll.GameVersion.TOV_JAP:
                case PatchAll.GameVersion.TOV_Unknown:
                    throw (new Exception("Not Tales of Vesperia (PAL Version)"));
                case PatchAll.GameVersion.None:
                    throw (new Exception("Not a 360 Game"));
                default:
                    throw (new Exception("Unknown Vesperia ISO"));
            }
        }

        public GameVersion CheckFileSystemVesperia(FileSystem Iso)
        {
            if (!Iso.Exists("default.xex"))
            {
                return GameVersion.None;
            }

            if (!Iso.Exists("chara.svo"))
            {
                return GameVersion.Other;
            }

            if (!Iso.Exists("language/scenario_fr.dat"))
            {
                return GameVersion.TOV_Unknown;
            }

            return GameVersion.TOV_EUR;
        }

        public void CreateProto()
        {
            var EntriesByRoom = Patcher.EntriesByRoom;
        }
	}
}
