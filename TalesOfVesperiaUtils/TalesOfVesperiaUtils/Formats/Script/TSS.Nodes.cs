using CSharpUtils.Endian;
using CSharpUtils.Streams;
using System;
using System.Collections.Generic;
using System.Linq;

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

			virtual public String GetParams()
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

            virtual public object[] Parameters
            {
                get
                {
                    if (ParameterType != null)
                    {
                        return new object[] { ParameterType.Value };
                    }
                    else if (Parameter != null)
                    {
                        return new object[] { Parameter };
                    }
					else if (InlineParam != null)
					{
						return new object[] { InlineParam };
					}

                    return new object[] { };
                }
            }

			public override string ToString()
			{
				return this.GetType().Name + "(" + GetParams() + ")";
			}
		}

		public class PushInstructionNode : InstructionNode
		{
			public DynamicValue ValueToPush;

            override public object[] Parameters
            {
                get
                {
                    return new object[] { ValueToPush };
                }
            }

			public override string GetParams()
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

            override public object[] Parameters
            {
                get
                {
                    return Elements;
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
								for (int n = 0; n < ArrayNumberOfElements; n++) yield return TSS.ReadStringz((int)(uint)ArrayStream.ReadStruct<uint_be>());
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
								Console.Error.WriteLine("Unhandled " + ValuesType + "(" + ArrayPointer + "," + ArrayNumberOfBytes + ")[" + ArrayNumberOfElements + "]");
								//if (ArrayNumberOfElements == 0)
								//{
								//	yield return "<TODO " + "Unhandled " + ValuesType + "(" + ArrayPointer + "," + ArrayNumberOfBytes + ")>";
								//}
								//else
								{
									yield return "<TODO " + "Unhandled " + ValuesType + ">";
									for (int n = 0; n < ArrayNumberOfBytes; n++) yield return "" + ArrayStream.ReadStruct<byte>();
								}
							}
							break;
							//throw (new Exception("Unhandled " + ValuesType));
					}
				}
			}

            public override string GetParams()
			{
				return base.GetParams() + ", ValuesType=" + ValuesType + ", Count=" + ArrayNumberOfElements + ", IntElements=" + IntElements.Implode(",") + ", Elements=" + Elements.Implode(",");
			}
		}

		public class OpInstructionNode : InstructionNode
		{
			public OperationType OperationType;

            override public object[] Parameters
            {
                get
                {
                    return new object[] { OperationType };
                }
            }

            public override string GetParams()
			{
				return base.GetParams() + ", OperationType=" + OperationType;
			}
		}

		public class CallInstructionNode : InstructionNode
		{
			public byte NumberOfParameters;
			public FunctionType FunctionType;
			public short NativeFunction;
			public int ScriptFunction;

            override public object[] Parameters
            {
                get
                {
                    return new object[] { NumberOfParameters, FunctionType, NativeFunction, ScriptFunction };
                }
            }

            public override string GetParams()
			{
				return base.GetParams() + ", NumberOfParameters=" + NumberOfParameters + ", FunctionType=" + FunctionType + ", NativeFunction=" + NativeFunction + ", ScriptFunction=" + ScriptFunction;
			}
		}
	}
}
