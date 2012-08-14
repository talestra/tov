using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Endian;

namespace TalesOfVesperiaUtils.Formats.Script
{
	public partial class TSS
	{
		public enum Opcode
		{
			NOP = 0x00,
			OP = 0x01,
			PUSH = 0x02,

			/// <summary>
			/// Negative values are function parameters. Positive are the current stack. ??
			/// </summary>
			STACK_READ = 0x03,
			PUSH_ARRAY = 0x04,
			CALL = 0x05,
			RETURN = 0x06,
			EXIT = 0x07,
			JUMP_ALWAYS = 0x08,
			JUMP_FALSE = 0x09,
			JUMP_TRUE = 0x0A,
			BRANCH_ALWAYS = 0x0B,
			BRANCH_FALSE = 0x0C,
			BRANCH_TRUE = 0x0D,
			UNK_0E = 0x0E,
			UNK_0F = 0x0F,
			STACK_SUBSTRACT = 0x10,
			FUNCTION_START = 0x11, // FUNCTION_START
			DEBUG = 0x12, // (CScript)Debug Code\n
			UNK_13 = 0x13, // (CScript)Error : Task Over. (Procedure call thread)\n
			UNK_14 = 0x14,
		}

		// TOS2.data6:80369DA8 op_01_calculate_table:.long op_01__CALCULATE_POINTER_00
		public enum OperationType // 45 (0x2D)
		{
			// Unary
			CALCULATE_POINTER = 0x00, // (void*)a   ??
			POST_INC = 0x01, // a++
			POST_DEC = 0x02, // a--
			PRE_INC = 0x03, // ++a
			PRE_DEC = 0x04, // --a
			NEGATE = 0x05, // -a
			NOP = 0x06, // (a)         ???
			BITWISE_NOT = 0x07, // ~a
			NOT = 0x08, // !a
			UNK_09 = 0x09, // ????? (int)??

			// Binary
			ADD = 0x0A, // a+b
			SUB = 0x0B, // a-b
			MULT = 0x0C, // a*b
			DIV = 0x0D, // a/b
			MOD = 0x0E, // a%b

			AND = 0x0F, // a&b
			OR = 0x10, // a|b
			XOR = 0x11, // a^b
			SLW = 0x12, // a<<b
			SRW = 0x13, // a>>b

			UNK_14 = 0x14, // ???????
			UNK_15 = 0x15, // ???????
			UNK_16 = 0x16, // ???????
			UNK_17 = 0x17, // ??????? a||b ???
			UNK_18 = 0x18, // ???????
			UNK_19 = 0x19, // ???????

			UNK_1A = 0x1A, // a && b ??
			UNK_1B = 0x1B, // a || b ??
			UNK_1C = 0x1C, // ???????
			UNK_1D = 0x1D, // ???????
			UNK_1E = 0x1E, // a %= b ???
			UNK_1F = 0x1F, // ???????
			UNK_20 = 0x20, // ???????
			ASSIGN_AND = 0x21, // a &= b
			ASSIGN_OR = 0x22, // a |= b
			ASSIGN_XOR = 0x23, // a ^= b
			ASSIGN_SLW = 0x24, // a <<= b
			ASSIGN_SRW = 0x25, // a >>= b

			ARRAY_ACCESS = 0x26, // a[b]
			UNK_27 = 0x27, // ???????        (CScript)Error : * [%04x]\n
			UNK_28 = 0x28, // ???????        (CScript)Error : Not supported.(Pointer x Pointer x Pointer)\n
			UNK_29 = 0x29, // ???????
			UNK_2A = 0x2A, // ???????
			UNK_2B = 0x2B, // ??????? embedded constant access in 2 bytes?       (CScript)Error : -> [%04x]\n
			UNK_2C = 0x2C, // ???????

			// ...
			// Ternary
			// ...
			// Unknown
		}

		// Not sure about 'void', 'undefined', 'null', 'false', 'true'.
		public enum ValueType : uint
		{
			VOID = 0x00,
			Integer32 = 0x01,
			Integer8Unsigned = 0x02,
			Integer8Signed = 0x03,
			NULL = 0x04,
			Integer16Signed = 0x05,
			Integer32_2 = 0x06,
			Integer32Signed = 0x07,
			Float32 = 0x08,
			FALSE = 0x09,
			TRUE = 0x0A,
			UNDEFINED = 0x0B,
			String = 0x0C,
			TextString = 0x82,
			//StringEx         = 0x80,
		}

		public class DynamicValue
		{
			public ValueType ValueType;
			public dynamic Value;

			public DynamicValue(ValueType ValueType, dynamic Value)
			{
				this.ValueType = ValueType;
				this.Value = Value;
			}

			public override string ToString()
			{
				if (Value is String)
				{
					return ValueType + "('" + Value + "')";
				}
				else
				{
					return ValueType + "(" + Value + ")";
				}
			}
		}

		public struct TextBlock
		{
			public struct Block
			{
				public uint_be title;

				public uint_be text;
			}

			public uint_be type;

			public uint_be unk;

			public Block ja;
			public Block en;
		}

		public enum FunctionType
		{
			Script = 0,
			Native = 1,
		}

		public uint GetTypeSize(uint ValueType)
		{
			if (ValueType >= TypeSizes.Length) return 0;
			return TypeSizes[ValueType];
		}

		public uint GetTypeSize(ValueType ValueType)
		{
			return GetTypeSize((uint)ValueType);
		}


		readonly static uint[] TypeSizes = new uint[]
		{
			0, 4, 1, 1, 0, 2, 4, 4, 4, 1, 1, 0, 4
		};
	}
}
