using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils.Extensions;
using System.Runtime.InteropServices;
using System.IO;
using CSharpUtils.Endian;

namespace TalesOfVesperiaUtils.Formats.Script
{
	unsafe public partial class TSS
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		unsafe public struct HeaderStruct
		{
			public fixed byte Magic[4];
			public uint_be CodeStart;
			public uint_be CodeEntryPoint;
			public uint_be TextStart;
			public uint_be EntryCodeStart;
			public uint_be EntryPtrEnd;
			public uint_be TextLen;
			public uint_be SectorSize;
		}

		public Stream Stream;
		public Stream CodeStream;
		public Stream TextStream;
		public HeaderStruct Header;
		public List<InstructionNode> InstructionNodes = new List<InstructionNode>();

		public IEnumerable<PushArrayInstructionNode> PushArrayInstructionNodes
		{
			get
			{
				return InstructionNodes
					.Where(Instruction => Instruction is TSS.PushArrayInstructionNode)
					.Cast<TSS.PushArrayInstructionNode>()
				;
			}
		}

		public void Load(Stream Stream)
		{
			this.Stream = Stream;
			var BinaryReader = new BinaryReader(Stream);
			Header = Stream.ReadStruct<HeaderStruct>();

			this.CodeStream = SliceStream.CreateWithBounds(Stream, Header.CodeStart, Header.TextStart);
			this.TextStream = SliceStream.CreateWithLength(Stream, Header.TextStart, Header.TextLen);
		}

		public String ReadStringz(uint TextOffset)
		{
			return (SliceStream.CreateWithLength(TextStream, TextOffset)).ReadStringz(-1, Encoding.UTF8);
		}

		public void ProcessCode()
		{
			var CodeBinaryReader = new BinaryReader(CodeStream);
			while (!CodeStream.Eof())
			{
				uint InstructionData = CodeBinaryReader.ReadUint32Endian(Endianness.BigEndian);
				var InstructionOpcode = (Opcode)((InstructionData >> 24) & 0xFF);
				InstructionNode InstructionNode;
				InstructionNode = new InstructionNode()
				{
					TSS = this,
					Opcode = InstructionOpcode,
					InlineParam = InstructionData & 0xFFFFFF,
				};

				switch (InstructionOpcode)
				{
					case Opcode.RETURN:
					case Opcode.EXIT:
					case Opcode.NOP:
					case Opcode.UNK_0E:
					case Opcode.UNK_14:
					case Opcode.UNK_13:
						break;
					// TEXT RELATED?
					// SET_TEXT(0C0100): 00001318
					case Opcode.STACK_READ:
						{
							// uint text_block_addr = read_word;
							InstructionNode.Parameter = CodeBinaryReader.ReadUint32Endian(Endianness.BigEndian);
						}
						break;
					case Opcode.CALL:
						{
							InstructionNode.Parameter = CodeBinaryReader.ReadUint32Endian(Endianness.BigEndian);
							var NumberOfParameters = (byte)((InstructionData >> 16) & 0xFF);
							var NativeFunction = (short)(InstructionData & 0xFFFF);
							InstructionNode = new CallInstructionNode()
							{
								TSS = this,
								Opcode = InstructionOpcode,
								FunctionType = (NativeFunction != -1) ? TSS.FunctionType.Native : TSS.FunctionType.Script,
								NativeFunction = NativeFunction,
								NumberOfParameters = NumberOfParameters,
								ScriptFunction = InstructionNode.Parameter,
							};
						}
						break;
					case Opcode.OP:
						{
							InstructionNode = new OpInstructionNode()
							{
								TSS = this,
								Opcode = InstructionOpcode,
								InlineParam = InstructionData & 0xFFFF,
								OperationType = (OperationType)((InstructionData >> 16) & 0xFF),
							};
						}
						break;
					case Opcode.PUSH:
						{
							var ValueTypeInt = ((InstructionData >> 16) & 0xFF);

							if ((ValueTypeInt & 0x80) != 0)
							{
								//bool IsText = ((ValueTypeInt & 0x02) != 0);

								var Offset = CodeBinaryReader.ReadUint32Endian(Endianness.BigEndian);
								InstructionNode = new PushInstructionNode()
								{
									TSS = this,
									Opcode = Opcode.PUSH,
									Parameter = Offset,
									//ParameterType = IsText ? ValueType.TextString : ValueType.String,
									ParameterType = ValueType.String,
									ValueToPush = new DynamicValue(ValueType.String, ReadStringz(Offset)),
								};
							}
							else
							{
								var ParameterType = (ValueType)(ValueTypeInt & 0x7F);
								dynamic Parameter = (ushort)(InstructionData & 0xFFFF);

								switch (ParameterType)
								{
									case ValueType.VOID:
									case ValueType.NULL:
									case ValueType.UNDEFINED:
									case ValueType.FALSE:
									case ValueType.TRUE:
									case ValueType.Integer8Unsigned:
									case ValueType.Integer8Signed:
									case ValueType.Integer16Signed:
										break;
									case ValueType.Integer32:
									case ValueType.Integer32_2:
									case ValueType.Integer32Signed:
										Parameter = CodeBinaryReader.ReadUint32Endian(Endianness.BigEndian);
										break;
									case ValueType.Float32:
										Parameter = CodeBinaryReader.ReadSingleEndian(Endianness.BigEndian);
										break;
								}

								InstructionNode = new PushInstructionNode()
								{
									TSS = this,
									Opcode = Opcode.PUSH,
									Parameter = Parameter,
									ParameterType = ParameterType,
									InlineParam = null,
									ValueToPush = new DynamicValue(ParameterType, InstructionNode.Parameter),
								};
							}
						}
						break;
					case Opcode.PUSH_ARRAY:
						{
							var ArrayPointer = CodeBinaryReader.ReadUint32Endian(Endianness.BigEndian);
							var ParameterTypeInt = (InstructionData >> 16) & 0xFF;
							var ParameterType = (ValueType)(ParameterTypeInt);
							uint TypeSize = GetTypeSize(ParameterTypeInt);
							uint ArrayNumberOfBytes = InstructionData & 0xFFFF;
							uint ArrayNumberOfElements = (TypeSize > 0) ? (ArrayNumberOfBytes / TypeSize) : 0;
							//var Elements = new List<dynamic>((int)ArrayNumberOfElements);

							InstructionNode = new PushArrayInstructionNode()
							{
								TSS = this,
								Opcode = Opcode.PUSH_ARRAY,
								ValuesType = ParameterType,
								ArrayPointer = ArrayPointer,
								ArrayNumberOfElements = ArrayNumberOfElements,
								ArrayNumberOfBytes = ArrayNumberOfBytes,
								//Values = Elements,
							};
						}
						break;
					case Opcode.JUMP_ALWAYS:
					case Opcode.JUMP_FALSE:
					case Opcode.JUMP_TRUE:
					case Opcode.FUNCTION_START:
					case Opcode.STACK_SUBSTRACT: // POP_RELATED?
						{
							InstructionNode.Parameter = CodeBinaryReader.ReadUint32Endian(Endianness.BigEndian);
						}
						break;
					default:
						throw (new NotImplementedException("Unprocessed opcode: " + InstructionOpcode));
				}

				InstructionNodes.Add(InstructionNode);

				//Console.WriteLine(InstructionNode);
			}
		}
	}
}
