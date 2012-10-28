using CSharpUtils.VirtualFileSystem;
using CSharpUtils.VirtualFileSystem.Local;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine
{
	public class Patcher
	{
		static public string PatcherPath;
		static public FileSystem TempFS;
		static public FileSystem PatcherDataFS;

		static Patcher()
		{
			PatcherPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			//PatcherDataFS = new LocalFileSystem(PatcherPath + "/../../Images", false);
			PatcherDataFS = new LocalFileSystem(PatcherPath + "/../../PatchData", false);
			TempFS = new LocalFileSystem(PatcherPath + "/Temp", true);
		}

		public Patcher()
		{
		}

		int Level = 0;

		public void Action(String Description, Action Action)
		{
			Console.WriteLine("{0}{1}...", new String(' ', Level * 2), Description);
			Level++;
			try
			{
				Action();
				//Console.WriteLine("Ok");
			}
			finally
			{
				Level--;
			}
		}
	}
}
