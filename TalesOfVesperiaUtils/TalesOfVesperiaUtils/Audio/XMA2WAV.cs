using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaUtils.Audio
{
	public class XMA2WAV
	{
		static public string ToWavPath
		{
			get
			{
				var FilePath = Path.GetTempPath() + @"\xma2wav.exe";
				//Console.WriteLine("{0}", FilePath);
				if (!File.Exists(FilePath))
				{
					var Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TalesOfVesperiaUtils.towav.exe");
					if (Stream == null) throw(new Exception("Can't extract xma2wav.exe"));
					//Console.WriteLine("  {0}", Stream.Length);
					Stream.CopyToFile(FilePath); 
				}
				return FilePath;
			}
		}

		private static int ExecuteCommand(string Command, string WD = ".")
		{
			int ExitCode;
			ProcessStartInfo ProcessInfo;
			Process Process;

			ProcessInfo = new ProcessStartInfo("cmd.exe", "/C " + Command);
			ProcessInfo.CreateNoWindow = true;
			ProcessInfo.UseShellExecute = false;
			ProcessInfo.WorkingDirectory = WD;
			Process = Process.Start(ProcessInfo);
			//Process.WaitForExit(Timeout);
			Process.WaitForExit();
			ExitCode = Process.ExitCode;
			Process.Close();

			return ExitCode;
		}

		static public void ConvertXmaToWav(Stream Input, Stream Output)
		{
			var TempDirectory = Path.GetTempPath();
			var InFile = TempDirectory + "/xma2wav__temp.xma";
			var OutFile = TempDirectory + "/xma2wav__temp.wav";

			try { File.Delete(InFile); } catch { }
			try { File.Delete(OutFile); } catch { }

			Input.CopyToFile(InFile);
			ExecuteCommand(ToWavPath + " \"" + InFile + "\"", TempDirectory);
			if (!File.Exists(OutFile)) throw(new Exception("Error uncompressing XMA to WAV"));
			using (var OutStream = File.OpenRead(OutFile))
			{
				OutStream.CopyToFast(Output);
			}
			try { File.Delete(InFile); } catch { }
			try { File.Delete(OutFile); } catch { }
		}
	}
}
