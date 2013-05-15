using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpUtils.Getopt;
using TalesOfVesperiaUtils;
using TalesOfVesperiaUtils.Formats.Packages;
using TalesOfVesperiaUtils.Compression;
using TalesOfVesperiaUtils.Formats.Script;

namespace Tss
{
    /// <summary>
    /// 
    /// </summary>
    class Program : GetoptCommandLineProgram
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SvoPath"></param>
        /// <param name="OutputDirectory"></param>
        [Command("-e", "--extract")]
        [Description("Extracts a TSS/Script file to a folder")]
        [Example("-e file.svo")]
        protected void ExtractTss(string TssPath)
        {
            (new TSS().Load(File.OpenRead(TssPath))).DumpScript(Console.Out);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            new Program().Run(args);
        }
    }
}
