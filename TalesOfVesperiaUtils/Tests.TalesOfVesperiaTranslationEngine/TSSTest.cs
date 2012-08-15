using TalesOfVesperiaUtils.Formats.Script;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using CSharpUtils;

namespace TalesOfVesperiaTests
{
	[TestClass]
	public class TSSTest
	{
		[TestMethod]
		public void LoadTest()
		{
			var TSS = new TSS();
			TSS.Load(File.OpenRead(String.Format(@"{0}\BTL_EP_510_080.TSS", Utils.TestInputPath)));
			TSS.ProcessCode();
			var SelectedInstructions = TSS.PushArrayInstructionNodes
				.Where(Instruction => (Instruction.ArrayNumberOfElements == 6))
			;

			Assert.AreEqual(
				@"[" +
				@"''," +
				@"''," +
				@"'\u30a2\u30ec\u30af\u30bb\u30a4'," +
				@"'101(VB45_0703)\u304f\u3063\u2026\u2026\u3001\u3084\u3063\u3066\u304f\u308c\u305f\u306a\uff01'," +
				"'Alexei'," +
				"'101(VB45_0703)Aagh...not bad!'" +
				@"]"
				,
				SelectedInstructions.ElementAt(0).Elements.ToArray().ToJson(true)
			);
		}

		[TestMethod]
		public void FullTest()
		{
			var TSS = new TSS();
			TSS.Load(File.OpenRead(String.Format(@"{0}\string_dic_us.so", Utils.TestInputPath)));
		}

		[TestMethod]
		public void StringTests()
		{
			var TSS = new TSS();
			TSS.Load(File.OpenRead(String.Format(@"{0}\BTL_EP_510_080.TSS", Utils.TestInputPath)));
			TSS.ProcessCode();
			var SelectedInstructions = TSS.InstructionNodes
				.Where(
					Instruction => (
						(Instruction.Opcode == TSS.Opcode.PUSH)
						&& (Instruction.ParameterType.Value == TSS.ValueType.String)
					)
				)
				.Cast<TSS.PushInstructionNode>()
			;

			Assert.AreEqual<String>(SelectedInstructions.ElementAt(2).ValueToPush.Value, "MUS_F03_NOR_CAP3");
			//Console.WriteLine(StringInstructions.Implode("\r\n"));
		}
	}
}
