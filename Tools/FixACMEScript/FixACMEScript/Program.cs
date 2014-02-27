using System;
using System.IO;
using System.Text;

namespace FixACMEScript
{
    class Program
    {
        //static string txtPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FixACMEScript.txt");
        //static string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tov.json");

        static int Main(string[] args)
        {
            Console.WriteLine("Algunos textos en que deberían ser distintos entre sí, ACME los pone iguales.");
            Console.WriteLine("Y esto es un apaño cutre que pone los textos incorrectos como toca.\n");
            Console.WriteLine("FixACMEScript <ArchivoTxt> <ArchivoJson>\n");

            if (args.Length != 2) return 1;
            string txtPath = args[0];
            string jsonPath = args[1];

            if (!File.Exists(txtPath)) { Console.WriteLine("No se ha encontrado el archivo *.txt."); return 2; }
            if (!File.Exists(jsonPath)) { Console.WriteLine("No se ha encontrado el archivo *.json."); return 3; }

            string[] Fixes = File.ReadAllLines(txtPath, Encoding.UTF8);

            if (Fixes.Length % 1 == 1) { Console.WriteLine("El número de líneas en el archivo FixACMEScript.txt ha de ser par."); return 4; }

            StreamReader sr = new StreamReader(jsonPath, Encoding.ASCII);
            string Content = sr.ReadToEnd();
            sr.Close();

            for (int n = 0; n < Fixes.Length; n += 2)
            {
                Content = Content.Replace(Fixes[n], Fixes[n + 1]);
            }

            StreamWriter sw = new StreamWriter(jsonPath, false, Encoding.ASCII);
            sw.Write(Content);
            sw.Close();

            return 0;
        }
    }
}
