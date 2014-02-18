using System;
using System.IO;

namespace CutreXORer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2 && (args[0] == "-e" || args[0] == "-d"))
            {
                if (File.Exists(args[1]))
                {
                    byte[] data = File.ReadAllBytes(args[1]);
                    
                    byte[] XORBytes = { 0x54, 0x61, 0x4c, 0x65, 0x53, 0x74, 0x52, 0x61, 0x4d, 0x6f, 0x4c, 0x61, 0x4d, 0x61, 0x5a, 0x6f };

                    if (args[0] == "-e")
                    {
                        for (int n = 0; n < data.Length; n++)
                        {
                            data[n] = (byte)(data[n] ^ XORBytes[n % 16]);
                            data[n] = (byte)((data[n] << 6) | ((data[n] & 0xc) << 2) | ((data[n] & 0x30) >> 2) | ((data[n] & 0xc0) >> 6));  //12345678 -> 78563412
                        }
                    }
                    else
                    {
                        for (int n = 0; n < data.Length; n++)
                        {
                            data[n] = (byte)((data[n] << 6) | ((data[n] & 0xc) << 2) | ((data[n] & 0x30) >> 2) | ((data[n] & 0xc0) >> 6));  //78563412 -> 12345678
                            data[n] = (byte)(data[n] ^ XORBytes[n % 16]);
                        }
                    }

                    File.WriteAllBytes(args[1], data);

                    Console.WriteLine("El archivo se ha procesado con éxito.");
                }
                else
                {
                    Console.WriteLine("El archivo especificado no existe.");
                }
            }
            else
            {
                Console.WriteLine("Programa para hacer un cifrado sencillo al PatchData.zip del Vesperia.\n");
                Console.WriteLine("Uso:");
                Console.WriteLine("Codificar:   CutreXORer -e <archivo>");
                Console.WriteLine("Decodificar: CutreXORer -d <archivo>");
            }

            //Console.Read();
        }
    }
}
