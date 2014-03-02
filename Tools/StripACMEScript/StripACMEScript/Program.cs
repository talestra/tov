using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StripACMEScript
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1 && args.Length != 2)
            {
                Console.WriteLine("Quita la información no necesaria del tov.json para el parcheador.");
                Console.WriteLine("Nos ahorramos unos 24mb sin comprimir y 4/5mb comprimidos.\n");
                Console.WriteLine("Uso: StripACMEScript <InputFile> [<OutputFile>]");
                Console.WriteLine("Si no se especifica OutputFile, se sobreescribirá el InputFile.\n\n");

                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("El InputFile introducido no existe.");
                return;
            }

            string lol = "LOL\n\u00a1LOL";
            string pattern = Regex.Escape(@lol);

            string[] Input = File.ReadAllLines(args[0]);
            string[] Output = new string[Input.Length];

            JsonSerializerSettings js = new JsonSerializerSettings();
            js.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
            js.NullValueHandling = NullValueHandling.Ignore;
            
            for (int n = 0; n < Input.Length; n++)
            {
                if (Input[n].Length > 0)
                {
                    ToVJSON.RootObject t = JsonConvert.DeserializeObject<ToVJSON.RootObject>(Input[n], js);
                    ToVJSON.RootObject ct = new ToVJSON.RootObject();
                    t.texts.en = null;
                    t.texts.ja = null;
                    ct.texts = t.texts;
                    ct.text_id = t.text_id;
                    ct.text_path = t.text_path;

                    Output[n] = JsonConvert.SerializeObject(ct, Formatting.None, js);
                }
            }

            if (args.Length == 1)
            {
                File.WriteAllLines(args[0], Output, Encoding.ASCII);
            }
            else
            {
                File.WriteAllLines(args[1], Output, Encoding.ASCII);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("El script se ha limpiado de información innecesaria correctamente.");
        }
    }
}
