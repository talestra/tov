using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils.Extensions;
using System.IO;

namespace TalesOfVesperiaUtils.Formats.Script
{
	public partial class TSS
	{

		public class InstructionNode
		{
			public TSS TSS;
			public Opcode Opcode;
			public dynamic InlineParam;
			public ValueType? ParameterType;
			public dynamic Parameter;

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
			public List<dynamic> Elements
			{
				get
				{
					var Elements = new List<dynamic>();
					var ArrayStream = SliceStream.CreateWithLength(TSS.TextStream, ArrayPointer, ArrayNumberOfBytes);
					var ArrayReader = new BinaryReader(ArrayStream);
					for (int n = 0; n < ArrayNumberOfElements; n++)
					{
						switch (ValuesType)
						{
							case ValueType.String:
								var TextPointer = ArrayReader.ReadUint32Endian(Endianness.BigEndian);
								Elements.Add(TSS.ReadStringz(TextPointer));
								break;
							default:
								throw(new Exception("Unhandled " + ValuesType));
						}
					}
					return Elements;
				}
			}

			protected override string GetParams()
			{
				return base.GetParams() + ", ValuesType=" + ValuesType + ", Elements=" + Elements.Implode(",");
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
