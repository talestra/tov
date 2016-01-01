using System.Collections.Generic;

namespace StripACMEScript
{
    public static class ToVJSON
    {
        public class Metainfo
        {
            public int id { get; set; }
        }

        public class Texts
        {
            public List<string> en { get; set; }
            public List<string> ja { get; set; }
            public List<string> es { get; set; }
        }

        public class Es
        {
            public bool done { get; set; }
            public string user { get; set; }
            public int time { get; set; }
        }

        public class Translated
        {
            public Es es { get; set; }
        }

        public class Es2
        {
            public bool done { get; set; }
            public string user { get; set; }
            public int time { get; set; }
        }

        public class Revised
        {
            public Es2 es { get; set; }
        }

        public class RootObject
        {
            public string text_path { get; set; }
            public string text_id { get; set; }
            public string linked_id { get; set; }
            public Metainfo metainfo { get; set; }
            public Texts texts { get; set; }
            public Translated translated { get; set; }
            public Revised revised { get; set; }
            public string project { get; set; }
        }
    }
}
