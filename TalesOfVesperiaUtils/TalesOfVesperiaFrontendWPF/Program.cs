using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;

namespace TalesOfVesperiaFrontendWPF
{
    public class Program
    {
        [STAThreadAttribute]
        public static void Main()
        {
            Console.WriteLine("Starting...");
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

#if true
            if (Debugger.IsAttached)
            {
                App.Main();
            }
            else
#endif
            {
                try
                {
                    //throw(new Exception("Test Error"));
                    App.Main();
                }
                catch (Exception Exception)
                {
                    MessageBox.Show(Exception.ToString(), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
                    Environment.Exit(-1);
                }
            }
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            Console.WriteLine("Trying to load: '{0}'", assemblyName.Name);

            string path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
            {
                path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
            }

            using (var stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
    }
}
