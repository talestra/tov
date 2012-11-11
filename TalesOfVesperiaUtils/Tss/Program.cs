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
            var Tss = new TSS().Load(File.OpenRead(TssPath));

			var Instructions = Tss.ReadInstructions().ToArray();
			var JumpLabels = new HashSet<uint>();
			var FunctionLabels = new HashSet<uint>();

			foreach (var Instruction in Instructions)
			{
				switch (Instruction.Opcode)
				{
					case TSS.Opcode.JUMP_ALWAYS:
					case TSS.Opcode.JUMP_FALSE:
					case TSS.Opcode.JUMP_TRUE:
						JumpLabels.Add((uint)Instruction.IntValue);
						break;
					case TSS.Opcode.CALL:
						{
							var CallInstruction = (TSS.CallInstructionNode)Instruction;
							if (CallInstruction.FunctionType == TSS.FunctionType.Script)
							{
								FunctionLabels.Add((uint)CallInstruction.ScriptFunction);
							}
						}
						break;
				}
			}

			foreach (var Instruction in Instructions)
            {
				if (JumpLabels.Contains(Instruction.InstructionPosition))
				{
					Console.WriteLine("Label({0:X8}):", Instruction.InstructionPosition);
				}
				if (FunctionLabels.Contains(Instruction.InstructionPosition))
				{
					Console.WriteLine("Function({0:X8}):", Instruction.InstructionPosition);
				}
				switch (Instruction.Opcode)
				{
					case TSS.Opcode.JUMP_ALWAYS:
					case TSS.Opcode.JUMP_FALSE:
					case TSS.Opcode.JUMP_TRUE:
						Console.WriteLine("\t{0:X8}: {1} Label({2:X8})", Instruction.InstructionPosition, Instruction.Opcode, (uint)Instruction.IntValue);
						break;
					case TSS.Opcode.CALL:
						{
							var CallInstruction = (TSS.CallInstructionNode)Instruction;
							if (CallInstruction.FunctionType == TSS.FunctionType.Script)
							{
								Console.WriteLine("\t{0:X8}: CALL_SCRIPT Function({1:X8}) ({2})", Instruction.InstructionPosition, CallInstruction.ScriptFunction, CallInstruction.NumberOfParameters);
							}
							else
							{
								Console.WriteLine("\t{0:X8}: CALL_NATIVE {1:X8} ({2})", Instruction.InstructionPosition, CallInstruction.NativeFunction, CallInstruction.NumberOfParameters);
							}
						}
						break;
					default:
						Console.WriteLine("\t{0:X8}: {1} ({2})", Instruction.InstructionPosition, Instruction.Opcode, String.Join(", ", Instruction.Parameters));
						break;
				}
            }
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
