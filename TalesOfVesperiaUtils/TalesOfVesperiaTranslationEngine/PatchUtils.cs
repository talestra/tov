using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TalesOfVesperiaTranslationEngine
{
	public class PatchUtils
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="BaseFileName"></param>
		/// <returns>The backup file name</returns>
		static public string BackupIfNotBackuped(string BaseFileName)
		{
			var BackupFileName = BaseFileName + ".bak";

			if (!File.Exists(BackupFileName))
			{
				Console.Write("Backuping... {0} -> {1}...", BaseFileName, BackupFileName);
				File.Copy(BaseFileName, BackupFileName);
				Console.WriteLine("Ok");
			}

			return BackupFileName;
		}
	}
}
