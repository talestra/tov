using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils;
using System.IO;
using CSharpUtils.Endian;

namespace TalesOfVesperiaUtils.Formats.Script
{
	public partial class TSS
	{

		public class InstructionNode
		{
			public TSS TSS;
			public uint InstructionPosition;
			public Opcode Opcode;
			public dynamic InlineParam;
			public ValueType? ParameterType;
			public dynamic Parameter;
			public uint InstructionData;
			public int IntValue;

			virtual protected String GetParams()
			{
				var String = "";
				String += "Opcode=" + Opcode;
				if (InlineParam != null) String += ", InlineParam=" + InlineParam;
				if (ParameterType != null) String += ", ParameterType=" + ParameterType.Value;
				if (Parameter != null)
				{
					String += ", Parameter=" + Parameter;
				}
				return String;
			}

			public override string ToString()
			{
				return this.GetType().Name + "(" + GetParams() + ")";
			}
		}

		public class PushInstructionNode : InstructionNode
		{
			public DynamicValue ValueToPush;

			protected override string GetParams()
			{
				return base.GetParams() + ", ValueToPush=" + ValueToPush;
			}
		}

		public class PushArrayInstructionNode : InstructionNode
		{
			public ValueType ValuesType;
			public uint ArrayPointer;
			public uint ArrayNumberOfElements;
			public uint ArrayNumberOfBytes;
			private dynamic[] _Elements;
			public dynamic[] Elements
			{
				get
				{
					if (_Elements == null) _Elements = GetElements(AsInt: false).ToArray();
					return _Elements;
				}
			}

			private int[] _IntElements;
			public int[] IntElements
			{
				get
				{
					if (_IntElements == null) _IntElements = GetElements(AsInt: true).Cast<int>().ToArray();
					return _IntElements;
				}
			}

			private IEnumerable<dynamic> GetElements(bool AsInt)
			{
				var ArrayStream = SliceStream.CreateWithLength(TSS.TextStream, ArrayPointer, ArrayNumberOfBytes);
				{
					switch (ValuesType)
					{
						case ValueType.String:
							if (!AsInt)
							{
								for (int n = 0; n < ArrayNumberOfElements; n++) yield return TSS.ReadStringz(ArrayStream.ReadStruct<uint_be>());
							}
							else
							{
								for (int n = 0; n < ArrayNumberOfElements; n++) yield return (int)(uint)ArrayStream.ReadStruct<uint_be>();
							}
							break;
						case ValueType.Integer32Signed:
							for (int n = 0; n < ArrayNumberOfElements; n++) yield return (int)(uint)ArrayStream.ReadStruct<uint_be>();
							break;
						case ValueType.Float32:
							if (!AsInt)
							{
								for (int n = 0; n < ArrayNumberOfElements; n++) yield return (float)ArrayStream.ReadStruct<float_be>();
							}
							else
							{
								for (int n = 0; n < ArrayNumberOfElements; n++) yield return (int)(uint)ArrayStream.ReadStruct<uint_be>();
							}
							break;
						default:
							{
								Console.WriteLine("Unhandled " + ValuesType);
								for (int n = 0; n < ArrayNumberOfElements; n++) yield return "<TODO " + "Unhandled " + ValuesType + ">";
							}
							break;
							//throw (new Exception("Unhandled " + ValuesType));
					}
				}
			}

			protected override string GetParams()
			{
				return base.GetParams() + ", ValuesType=" + ValuesType + ", Count=" + ArrayNumberOfElements + ", IntElements=" + IntElements.Implode(",") + ", Elements=" + Elements.Implode(",");
			}
		}

		public class OpInstructionNode : InstructionNode
		{
			public OperationType OperationType;

			protected override string GetParams()
			{
				return base.GetParams() + ", OperationType=" + OperationType;
			}
		}

		public class CallInstructionNode : InstructionNode
		{
			public byte NumberOfParameters;
			public FunctionType FunctionType;
			public short NativeFunction;
			public uint ScriptFunction;

			protected override string GetParams()
			{
				return base.GetParams() + ", NumberOfParameters=" + NumberOfParameters + ", FunctionType=" + FunctionType + ", NativeFunction=" + NativeFunction + ", ScriptFunction=" + ScriptFunction;
			}
		}
	}
}
