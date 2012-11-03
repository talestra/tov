using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Streams;
using System.Runtime.InteropServices;
using System.IO;
using CSharpUtils.Endian;
using CSharpUtils.SpaceAssigner;
using TalesOfVesperiaTranslationUtils;
using TalesOfVesperiaUtils.Text;

namespace TalesOfVesperiaUtils.Formats.Script
{
	unsafe public partial class TSS
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		unsafe public struct HeaderStruct
		{
			//public fixed byte Magic[4];
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
			public string Magic;

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

		public IEnumerable<PushArrayInstructionNode> PushArrayInstructionNodes
		{
			get
			{
				return ReadInstructions()
					.Where(Instruction => Instruction is TSS.PushArrayInstructionNode)
					.Cast<TSS.PushArrayInstructionNode>()
					.Where(Instruction => Instruction.ValuesType == ValueType.String)
					.Where(Instruction => Instruction.ArrayNumberOfElements == 6)
				;
			}
		}

		public TSS Load(Stream Stream)
		{
			this.Stream = Stream;
			var BinaryReader = new BinaryReader(Stream);
			Header = Stream.ReadStruct<HeaderStruct>();

			if (Header.Magic != "TSS") throw(new Exception("Not a TSS file!"));

			this.CodeStream = new MemoryStream(SliceStream.CreateWithBounds(Stream, Header.CodeStart, Header.TextStart).ReadAll());
			
			var TextData = SliceStream.CreateWithLength(Stream, Header.TextStart, Header.TextLen).ReadAll();
			this.TextStream = new MemoryStream();
			this.TextStream.WriteBytes(TextData);
			this.TextStream.Position = 0;

			return this;
		}

		public MemoryStream Save()
		{
			var Stream = new MemoryStream();
			SaveTo(Stream);
			Stream.Position = 0;
			return Stream;
		}

		public void SaveTo(Stream Stream)
		{
			long OutStart = Stream.Position;
			Header.TextLen = (uint)TextStream.Length;
			
			Stream.WriteStruct(Header);

			Stream.WriteByteRepeatedTo((byte)0x00, OutStart + Header.CodeStart);
			Stream.WriteStream(CodeStream.Slice());

			Stream.WriteByteRepeatedTo((byte)0x00, OutStart + Header.TextStart);
			Stream.WriteStream(TextStream.Slice());
		}

		public int CountStringz(int TextOffset)
		{
			return (SliceStream.CreateWithLength(TextStream, TextOffset)).CountStringzBytes(AlignTo4: false, AlignPosition: (int)TextOffset);
		}

		public String ReadStringz(int TextOffset)
		{
			if (TextOffset < 0 || TextOffset >= TextStream.Length) return "";
			return (SliceStream.CreateWithLength(TextStream, TextOffset)).ReadStringz(Encoding: Encoding.UTF8);
		}

		public class StringInfo
		{
			private SliceStream PointerStream;
			public int StringzOffset;
			public int StringzLength;
			public string Text;

			public uint PointerValue 
			{
				get
				{
					return (uint)PointerStream.Slice().ReadStruct<uint_be>();
				}
			}

			public void WritePointerValue(uint value)
			{
				PointerStream.Slice().WriteStruct((uint_be)value);
			}

			public StringInfo(TSS Tss, int StringzOffset, SliceStream PointerStream)
			{
				this.PointerStream = PointerStream;

				if (this.PointerStream.Length != 4) throw (new InvalidOperationException("Invalid PointerStream. Length != 4"));
				if (this.PointerValue != StringzOffset) throw (new InvalidOperationException("StringzOffset Doesn't Match PointerStream Data"));

				this.StringzOffset = StringzOffset;
				this.StringzLength = Tss.CountStringz(StringzOffset);
				this.Text = Tss.ReadStringz(StringzOffset);
			}
		}

		public class TextEntry
		{
			public int TextType;
			public uint Id;
			public uint Id2;
			public uint Id3;
			public StringInfo[] Original;
			public StringInfo[] Translated;

			public StringInfo[] OriginalTranslated
			{
				get
				{
					return Original.Concat(Translated).ToArray();
				}
			}

			public TextEntry()
			{
			}

			public void TranslateWithTranslationEntry(TranslationEntry TranslationEntry)
			{
				for (int n = 0; n < Original.Length; n++)
				{
					Original[n].Text = "";
					Translated[n].Text = TextProcessor.Instance.ProcessAndDetectPitfalls(Translated[n].Text, TranslationEntry.texts.es[n]);
				}
			}

			public override string ToString()
			{
				return this.ToStringDefault();
			}
		}

		public List<TextEntry> ExtractTexts(bool HandleType1 = true, bool EmitSeparators = false)
		{
			var TextEntries = new List<TextEntry>();
			HandleTexts((TextEntry) =>
			{
				if (TextEntry == null && !EmitSeparators) return;
				TextEntries.Add(TextEntry);
			}, HandleType1);
			return TextEntries;
		}

		public void TranslateTexts(Action<TextEntry> TranslateTextEntry, bool HandleType1 = true)
		{
			var SpaceAssigner1D = new SpaceAssigner1D();
			var SpaceAssigner1DUniqueAllocator = new SpaceAssigner1DUniqueAllocatorStream(SpaceAssigner1D, TextStream);

			var Entries = new List<TextEntry>();

			HandleTexts((Entry) =>
			{
				if (Entry == null) return;
				Entries.Add(Entry);
				foreach (var StringInfo in Entry.OriginalTranslated)
				{
					//var StringLength = CountStringz((uint)StringOffset);
					//Console.WriteLine("{0}, {1}", StringInfo.StringzOffset, StringInfo.StringzLength);
					SpaceAssigner1D.AddAvailableWithLength(StringInfo.StringzOffset, StringInfo.StringzLength);
				}
				
				bool HasText = false;
				foreach (var StringInfo in Entry.OriginalTranslated)
				{
					if (StringInfo.Text.Length > 0) HasText = true;
				}
				
				if (HasText)
				{
					TranslateTextEntry(Entry);
				}
			}, HandleType1);

			SpaceAssigner1DUniqueAllocator.FillSpacesWithZeroes();
			SpaceAssigner1D.AddAvailableWithLength(TextStream.Length, 0x10000);

			foreach (var Entry in Entries)
			{
				foreach (var StringInfo in Entry.OriginalTranslated)
				{
					var TextSpace = SpaceAssigner1DUniqueAllocator.AllocateUnique(StringInfo.Text, Encoding.UTF8);
					StringInfo.WritePointerValue((uint)TextSpace.Min);
				}
			}
		}

		public void HandleTexts(Action<TextEntry> ActionTextEntry, bool HandleType1 = true)
		{
			uint TextId = 0;
			int Lang = 0;
			int TextType = -1;
			var Text1 = new StringInfo[] { };
			var Text2 = new StringInfo[] { };
			var Stack = new List<dynamic>();
			bool LastSeparator = true;
			foreach (var Instruction in ReadInstructions())
			{
				//Console.WriteLine("{0:X8}: {1}", Instruction.InstructionPosition, Instruction);

				switch (Instruction.Opcode)
				{
					case Opcode.UNK_0E:
						switch (Instruction.InstructionData & 0xFFFF)
						{
							case 0x07: Lang = 1; break;
							case 0x83: Lang = 2; break;
						}
						break;
					case Opcode.PUSH:
						{
							var PushInstruction = (PushInstructionNode)Instruction;
							if (PushInstruction.ParameterType == ValueType.String)
							{
								if (new[] { 1, 2 }.Contains(Lang))
								{
									var StringInfo = new StringInfo(this, PushInstruction.IntValue, CodeStream.SliceWithLength(PushInstruction.InstructionPosition + 4, 4));
									if (Lang == 1) Text1 = new[] { StringInfo };
									if (Lang == 2) Text2 = new[] { StringInfo };
								}
							}
							else
							{
								//Console.WriteLine("{0}", PushInstruction.ValueToPush.Value);
								Stack.Add(PushInstruction.ValueToPush.Value);
							}
							Lang = 0;
						}
						break;
					case Opcode.FUNCTION_START:
						{
							// Separator
							//Console.WriteLine("------------------------------------");
							if (!LastSeparator)
							{
								ActionTextEntry(null);
								LastSeparator = true;
							}
						}
						break;
					case Opcode.CALL:
						{
							var CallInstruction = (CallInstructionNode)Instruction;

							int TextFunc = 0;

							if ((CallInstruction.FunctionType == FunctionType.Native) && (CallInstruction.NativeFunction == 1)) TextFunc = 1;
							//if ((CallInstruction.FunctionType == FunctionType.Script) && ((new uint[] { 12, 0x12e30, }).Contains(CallInstruction.ScriptFunction))) TextFunc = 2;

							bool AddEntry = true;

							if (TextFunc > 0)
							{
								if (Text1.Length > 0 || Text2.Length > 0)
								{
									if (Stack.Count > 0)
									{
										if (TextFunc == 1)
										{
											//Console.WriteLine("TEXT![1]");
											if (!HandleType1) continue;
											//Console.WriteLine("TEXT![1]");

											TextId = 0;
											try
											{
												var TextIdStr = String.Format("{0}", Stack[0]);
												//Console.WriteLine("TEXT![1] : {0}", TextIdStr);
												TextId = uint.Parse(TextIdStr);
											}
											catch
											{
											}

											if (TextId < 00010001) {
												//Console.WriteLine("{0}", Stack.Implode(","));
												//continue;
												AddEntry = false;
											}
											//Console.WriteLine("TEXT![3]");
										}

										if (AddEntry)
										{
											ActionTextEntry(new TextEntry()
											{
												TextType = TextType,
												Id = TextId,
												Id2 = TextId,
												Original = Text1,
												Translated = Text2,
											});
											LastSeparator = false;
										}
									}
									//Console.WriteLine("############  AddText {0:D8}:('{1}', '{2}')", Stack[0], Text1.EscapeString(), Text2.EscapeString());
								}
							}
							Text1 = new StringInfo[0];
							Text2 = new StringInfo[0];
							TextType = -1;
							Stack.Clear();
						}
						break;
					case Opcode.PUSH_ARRAY:
						{
							var PushArrayInstruction = (PushArrayInstructionNode)Instruction;
							
							// Dialog. Using PTR1.
							if (PushArrayInstruction.ArrayNumberOfElements == 6)
							{
								var E = PushArrayInstruction.Elements.Cast<string>().ToArray();
								var IE = PushArrayInstruction.IntElements;
								TextType = IE[0];
								int Unk = IE[1];
								//Console.WriteLine("Dialog {0}, {1}, {2}", E[0], E[1], E[2]);
								//Console.WriteLine("Dialog {0}, {1}, {2}", E[3], E[4], E[5]);
								TextId = PushArrayInstruction.ArrayPointer + this.Header.TextStart;

								Text1 = new[] {
									new StringInfo(this, IE[2], TextStream.SliceWithLength(PushArrayInstruction.ArrayPointer + 4 * 2, 4)),
									new StringInfo(this, IE[3], TextStream.SliceWithLength(PushArrayInstruction.ArrayPointer + 4 * 3, 4)),
								};
								Text2 = new[] {
									new StringInfo(this, IE[4], TextStream.SliceWithLength(PushArrayInstruction.ArrayPointer + 4 * 4, 4)),
									new StringInfo(this, IE[5], TextStream.SliceWithLength(PushArrayInstruction.ArrayPointer + 4 * 5, 4)),
								};

								ActionTextEntry(new TextEntry()
								{
									TextType = TextType,
									Id = TextId,
									Id2 = PushArrayInstruction.ArrayPointer,
									Id3 = PushArrayInstruction.InstructionPosition,
									Original = Text1,
									Translated = Text2,
								});
								LastSeparator = false;

								Text1 = new StringInfo[0];
								Text2 = new StringInfo[0];
								TextType = -1;
								Stack.Clear();

								/*
								if (TextType != 0)
								{
									Console.WriteLine(TextType);
									Console.WriteLine("{0}", Text1.Implode("."));
									Console.WriteLine("{0}", Text2.Implode(","));
								}
								*/
								//Console.ReadKey();
							}
						}
						break;
				}
			}
		}

		public IEnumerable<InstructionNode> ReadInstructions()
		{
			var CodeStream = this.CodeStream.Slice();
			//yield return null;
			if (true)
			{
				while (!CodeStream.Eof())
				{
					uint InstructionPosition = (uint)CodeStream.Position;
					uint InstructionData = CodeStream.ReadStruct<uint_be>();
					int IntValue = -1;
					var InstructionOpcode = (Opcode)((InstructionData >> 24) & 0xFF);
					InstructionNode InstructionNode;
					InstructionNode = new InstructionNode()
					{
						TSS = this,
						InstructionPosition = InstructionPosition,
						Opcode = InstructionOpcode,
						InstructionData = InstructionData,
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
								InstructionNode.Parameter = CodeStream.ReadStruct<uint_be>();
								InstructionNode.IntValue = (int)(uint)InstructionNode.Parameter;
							}
							break;
						case Opcode.CALL:
							{
								InstructionNode.Parameter = CodeStream.ReadStruct<uint_be>();
								var NumberOfParameters = (byte)((InstructionData >> 16) & 0xFF);
								var NativeFunction = (short)(InstructionData & 0xFFFF);
								InstructionNode = new CallInstructionNode()
								{
									TSS = this,
									InstructionPosition = InstructionPosition,
									Opcode = InstructionOpcode,
									FunctionType = (NativeFunction != -1) ? TSS.FunctionType.Native : TSS.FunctionType.Script,
									NativeFunction = NativeFunction,
									IntValue = (NativeFunction != -1) ? NativeFunction : (int)(uint)InstructionNode.Parameter,
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
									InstructionPosition = InstructionPosition,
									Opcode = InstructionOpcode,
									InlineParam = InstructionData & 0xFFFF,
									OperationType = (OperationType)((InstructionData >> 16) & 0xFF),
									IntValue = (int)((InstructionData >> 16) & 0xFF),
								};
							}
							break;
						case Opcode.PUSH:
							{
								var ValueTypeInt = ((InstructionData >> 16) & 0xFF);

								// Read String
								if ((ValueTypeInt & 0x80) != 0)
								{
									//bool IsText = ((ValueTypeInt & 0x02) != 0);

									var Offset = CodeStream.ReadStruct<uint_be>();
									InstructionNode = new PushInstructionNode()
									{
										TSS = this,
										InstructionPosition = InstructionPosition,
										Opcode = Opcode.PUSH,
										Parameter = Offset,
										//ParameterType = IsText ? ValueType.TextString : ValueType.String,
										ParameterType = ValueType.String,
										IntValue = (int)(uint)Offset,
										ValueToPush = new DynamicValue(ValueType.String, ReadStringz((int)(uint)Offset)),
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
											Parameter = CodeStream.ReadStruct<uint_be>();
											IntValue = (int)(uint)Parameter;
											break;
										case ValueType.Float32:
											Parameter = CodeStream.ReadStruct<float_be>();
											IntValue = -1;
											break;
									}

									InstructionNode = new PushInstructionNode()
									{
										TSS = this,
										InstructionPosition = InstructionPosition,
										Opcode = Opcode.PUSH,
										Parameter = Parameter,
										ParameterType = ParameterType,
										IntValue = IntValue,
										InlineParam = null,
										ValueToPush = new DynamicValue(ParameterType, Parameter),
									};
								}
							}
							break;
						case Opcode.PUSH_ARRAY:
							{
								var ArrayPointer = CodeStream.ReadStruct<uint_be>();
								var ParameterTypeInt = (InstructionData >> 16) & 0xFF;
								var ParameterType = (ValueType)(ParameterTypeInt);
								uint TypeSize = GetTypeSize(ParameterTypeInt);
								uint ArrayNumberOfBytes = InstructionData & 0xFFFF;
								uint ArrayNumberOfElements = (TypeSize > 0) ? (ArrayNumberOfBytes / TypeSize) : 0;
								//var Elements = new List<dynamic>((int)ArrayNumberOfElements);

								InstructionNode = new PushArrayInstructionNode()
								{
									TSS = this,
									InstructionPosition = InstructionPosition,
									Opcode = Opcode.PUSH_ARRAY,
									ValuesType = ParameterType,
									ArrayPointer = ArrayPointer,
									ArrayNumberOfElements = ArrayNumberOfElements,
									ArrayNumberOfBytes = ArrayNumberOfBytes,
									IntValue = -1,
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
								InstructionNode.Parameter = CodeStream.ReadStruct<uint_be>();
								InstructionNode.IntValue = (int)(uint)InstructionNode.Parameter;
							}
							break;
						default:
							throw (new NotImplementedException("Unprocessed opcode: " + InstructionOpcode));
					}

					yield return InstructionNode;

					//Console.WriteLine(InstructionNode);
				}
			}
		}

		public static bool IsValid(byte[] Data)
		{
			try
			{
				var Header = StructUtils.BytesToStruct<HeaderStruct>(Data);
				if (Header.Magic != "TSS") return false;
				return true;
			}
			catch
			{
			}
			return false;
		}
	}
}
