/*
dmd test_tss.d
dmd -run test_tss.d string_dic_uk.so > string_dic_uk.so.txt

http://www.digitalmars.com/d/2.0/changelog.html
*/
import std.stdio, std.string, std.file, std.stream, std.intrinsic, std.conv, std.math;

/*

Tales of Symphonia 2:
.text2:80090E0C CScript__execute_single:                # CODE XREF: sub_80092180+88p

*/

T min(T)(T a, T b) {
	if (a < b) return a;
	return b;
}

struct uint_be {
	uint be_value;
	
	uint value() { return bswap(be_value); }
	
	string toString() { return to!(string)(value); }
	
	static assert (this.sizeof == 4);
}

struct TSS_HEADER {
	char    magic[4] = "TSS\0";
	uint_be script_start;
	uint_be code_start;
	uint_be text_start;
	uint_be entry_script_start;
	uint_be entry_ptr_end;
	uint_be text_len;
	uint_be sector_size;
	
	void dump() {
		writefln("MAGIC           : %s", magic);
		writefln("SCRIPT_START    : %08X", script_start.value);
		writefln("CODE_START      : %08X", code_start.value);
		writefln("TEXT_START      : %08X", text_start.value);
		writefln("ENTRY_CODE_START: %08X", entry_script_start.value);
		writefln("ENTRY_PTR_END   : %08X", entry_ptr_end.value);
		writefln("TEXT_LEN        : %08X", text_len.value);
		writefln("SECTOR_SIZE     : %08X", sector_size.value);
	}
	
	static assert (this.sizeof == 0x20);
}

enum OPCODE {
	NOP                = 0x00,
	OP                 = 0x01,
	PUSH               = 0x02,
	UNK_03             = 0x03,
	PUSH_ARRAY         = 0x04,
	CALL               = 0x05,
	RETURN             = 0x06,
	EXIT               = 0x07,
	JUMP_ALWAYS        = 0x08,
	JUMP_FALSE         = 0x09,
	JUMP_TRUE          = 0x0A,
	BRANCH_ALWAYS      = 0x0B,
	BRANCH_FALSE       = 0x0C,
	BRANCH_TRUE        = 0x0D,
	UNK_0E             = 0x0E,
	UNK_0F             = 0x0F,
	STACK_SUBSTRACT    = 0x10,
	START_FUNCTION_11  = 0x11, // START_FUNCTION
	DEBUG              = 0x12, // (CScript)Debug Code\n
	UNK_13             = 0x13, // (CScript)Error : Task Over. (Procedure call thread)\n
	UNK_14             = 0x14,
}

// TOS2.data6:80369DA8 op_01_calculate_table:.long op_01__CALCULATE_POINTER_00
enum OPERATION_TYPE { // 45 (0x2D)
	// Unary
	CALCULATE_POINTER = 0x00, // (void*)a   ??
	POST_INC          = 0x01, // a++
	POST_DEC          = 0x02, // a--
	PRE_INC           = 0x03, // ++a
	PRE_DEC           = 0x04, // --a
	NEGATE            = 0x05, // -a
	NOP               = 0x06, // (a)         ???
	BITWISE_NOT       = 0x07, // ~a
	NOT               = 0x08, // !a
	UNK_09            = 0x09, // ????? (int)??

	// Binary
	ASSIGN_ADD               = 0x0A, // a += b
	ASSIGN_SUB               = 0x0B, // a -= b
	ASSIGN_MULT              = 0x0C, // a *= b
	ASSIGN_DIV               = 0x0D, // a /= b
	ASSIGN_MOD               = 0x0E, // a %= b

	ASSIGN_AND               = 0x0F, // a &= b
	ASSIGN_OR                = 0x10, // a |= b
	ASSIGN_XOR               = 0x11, // a ^= b
	ASSIGN_SLW               = 0x12, // a <<= b
	ASSIGN_SRW               = 0x13, // a >>= b

	UNK_14            = 0x14, // ???????
	UNK_15            = 0x15, // ???????
	UNK_16            = 0x16, // ???????
	UNK_17            = 0x17, // ??????? a||b ???
	UNK_18            = 0x18, // ???????
	UNK_19            = 0x19, // ???????

	UNK_1A            = 0x1A, // a && b ??
	UNK_1B            = 0x1B, // a || b ??
	UNK_1C            = 0x1C, // ???????
	UNK_1D            = 0x1D, // ???????
	UNK_1E            = 0x1E, // a %= b ???
	UNK_1F            = 0x1F, // ???????
	UNK_20            = 0x20, // ???????
	AND        = 0x21, // a &= b
	OR         = 0x22, // a |= b
	XOR        = 0x23, // a ^= b
	SLW        = 0x24, // a <<= b
	SRW        = 0x25, // a >>= b

	ARRAY_ACCESS      = 0x26, // a[b]
	UNK_27            = 0x27, // ???????        (CScript)Error : * [%04x]\n
	UNK_28            = 0x28, // ???????        (CScript)Error : Not supported.(Pointer x Pointer x Pointer)\n
	UNK_29            = 0x29, // ???????
	UNK_2A            = 0x2A, // ???????
	UNK_2B            = 0x2B, // ??????? embedded constant access in 2 bytes?       (CScript)Error : -> [%04x]\n
	UNK_2C            = 0x2C, // ???????

	// ...
	// Ternary
	// ...
	// Unknown
}

// Not sure about 'void', 'undefined', 'null', 'false', 'true'.
enum TYPE {
	VOID              = 0x00,
	Integer32         = 0x01,
	Integer8Unsigned  = 0x02,
	Integer8Signed    = 0x03,
	NULL              = 0x04,
	Integer16Signed   = 0x05,
	Integer32_2       = 0x06,
	Integer32Signed   = 0x07,
	Float32           = 0x08,
	FALSE             = 0x09,
	TRUE              = 0x0A,
	UNDEFINED         = 0x0B,
	String            = 0x0C,
}

uint getTypeSize(TYPE type) {
	uint offset = cast(uint)type;
	static list = [0, 4, 1, 1, 0, 2, 4, 4, 4, 1, 1, 0, 4];
	if ((offset < 0) || (offset >= list.length)) return 0;
	return list[offset];
}

struct TEXT_BLOCK { // relative to text_start
	struct BLOCK {
		uint_be title;
		uint_be text;
		void dump() {
			writefln("    !!BLOCK(%08X, %08X)", title.value, text.value);
		}
	}
	uint_be type;
	uint_be unk;
	BLOCK ja;
	BLOCK en;
	
	void dump() {
		writefln("!!TEXT_BLOCK(%08X, %08X)", type.value, unk.value);
		ja.dump();
		en.dump();
	}
}

/*
01 - OPERATION
02 -
	07 - SET_INTEGER? + PARAM
	?? - 
	82 - SET_STRING?  + PARAM
03 - SET_VARIABLE?
04 - TEXT_RELATED?
05 -
	WORD integrado (NPARAMS + NATIVE_FUNC_ID) si es función del juego, NATIVE_FUNC_ID es -1
	WORD extra     -1 si es función nativa, dirección si es función ingame
06 - RETURN
08 - CODE_PTR_RELATED (JUMP?)
07 - END_OF_SCRIPT
09 - CODE_PTR_RELATED (CONDITIONAL_JUMP?)
0E - PUSH
11 - START_FUNCTION
14 - POP
FF - STACK_POS += 24 bits

*/

ushort bswap16(ushort v) {
	return ((v >> 8) & 0xFF) | ((v & 0xFF) << 8);
}

string addcslashes(string str) {
	string ret;
	foreach (c; str) {
		switch (c) {
			case '\n': ret ~= "\\n"; break;
			case '\r': ret ~= "\\r"; break;
			case '\t': ret ~= "\\t"; break;
			default:
				ret ~= c;
			break;
		}
	}
	return ret;
}

void processScript(ubyte[] data) {
	uint[uint] opcode_usage;
	uint[][uint] labels_code_code;
	uint[][uint] labels_code_data;
	uint[][uint] labels_data_data;

	auto header = cast(TSS_HEADER*)data.ptr;
	assert(header.magic == TSS_HEADER.init.magic);

	string get_string(uint offset) {
		string ret;
		for (; offset < header.text_len.value; offset++) {
			char c = data[header.text_start.value + offset];
			if (c == '\0') break;
			ret ~= c;
		}
		return ret;
	}
	
	string get_string_addslashes(uint offset) {
		return addcslashes(get_string(offset));
	}

	void dump_header() {
		writefln("== HEADER ===========================");
		header.dump();
	}
	
	void dump_data_texts() {
		writefln("== TEXTS ============================");
		char[] texts = cast(char[])data[header.text_start.value .. header.text_start.value + header.text_len.value];
		int text_from = 0;
		bool unused_offsets[uint];
		foreach (a, k; labels_data_data) {
			unused_offsets[a] = true;
		}
		for (int n = 0; n < texts.length; n++) {
			//writefln("%d", texts[n]);
			//if (texts[n..n + 2] == "\0\0") {
			if ((n in labels_code_data) || (n in labels_data_data)) {
				unused_offsets.remove(n);
				writefln(":data_%08X:", n);
				string s = get_string(n);
				writefln("  %08X: '%s'", n, addcslashes(s));
				n += s.length;
			}
			/*if (texts[n..n + 1] == "\0") {
				writefln("  %08X: '%s'", text_from, addcslashes(cast(string)texts[text_from..n]));
				//n++;
				text_from = n + 1;
			}*/
		}
		
		foreach (offset; unused_offsets.keys.sort) {
			writefln(":unk_data_%08X:", offset);
			string s = get_string(offset);
			writefln("  %08X: '%s'", offset, addcslashes(s));
		}
		
		int columns = 16;
		
		for (int n = 0; n < data.length; n += columns) {
			writef("%08X: ", n);
			for (int k = 0; k < columns; k++) {
				ubyte c = 0;
				try { c = data[n + k]; } catch { }
				if (((k % 16) == 0)) printf(" ");
				if (((k % 8) == 0)) printf(" ");
				if (((k % 4) == 0)) printf(" ");
				writef(" %02X", c);
			}
			writef(" ");
			for (int k = 0; k < columns; k++) {
				auto c = 0;
				try { c = data[n + k]; } catch { }
				//if (((k % 16) == 0)) printf(" ");
				//if (((k % 8) == 0)) printf(" ");
				//if (((k % 4) == 0)) printf(" ");
				printf("%c", (c < 32) ? '.' : c);
			}
			writefln("");
		}
	}

	void dump_data_pointers() {
		int start = header.text_start.value;
		int end = header.text_start.value + header.text_len.value - (4 - header.text_len.value % 4);
		writefln("== POINTERS ========================= (%08X-%08X)", start, end);
		//uint[] pointers = cast(uint[])data[start .. end];
		uint[] pointers = new uint[(end - start) / 4];
		(cast(byte *)pointers.ptr)[0..end - start] = (cast(byte *)data.ptr)[0..end - start];
		//pointers[$ - 1] = 0x00004275;
		writefln("=====================================");
		for (int n = 0; n < pointers.length; n++) {
			uint offset = n * 4;
			uint pointer = bswap(pointers[n]);
			if ((offset in labels_code_data) || (offset in labels_data_data)) writefln(":data_%08X:", offset);
			if ((pointers[n] > 0) && (pointer < header.text_len.value)) {
				writefln("%08X: data_%08X", offset, pointer);
				labels_data_data[pointer] ~= offset;
			}
		}
	}

	void dump_script() {
		writefln("== SCRIPT ===========================");
		auto check = cast(uint[])data[header.script_start.value .. header.script_start.value + header.text_start.value];

		foreach (display; [false, true]) {
			for (int n = 0; n < check.length;) {
				uint read_word() { return bswap(check[n++]); }

				uint offset = n * 4;
			
				uint c2 = read_word;
				//writefln("%08X ", c2);
				//writefln("");
				//writefln("%08X", c >> 8);
				uint iopcode = (c2 >> 24);
				if ((iopcode in opcode_usage) is null) opcode_usage[iopcode] = 0;
				opcode_usage[iopcode]++;
				auto opcode = cast(OPCODE)iopcode;
				
				if (offset == header.code_start.value) {
					if (display) writefln("::entry_point:");
				}
				
				if (offset in labels_code_code) {
					if (display) writefln("::code_%08X:", offset);
				}
				
				// TOS2.data6:80369E8C opcode_switch_table:.long read_opcode   # DATA XREF: CScript__execute_single+30o
				switch (opcode) {
					case OPCODE.NOP: // NOP
					break;
					case OPCODE.OP: { // OPERATION
						auto type = cast(OPERATION_TYPE)((c2 >> 16) & 0xFF);
					
						//writefln("%08X: OPERATION %06X", offset, c2 & 0xFFFFFF);
						switch (type) {
							default:
								if (display) writefln("  %08X: %s(%s) : %04X", offset, to!string(opcode), to!string(type), c2 & 0xFFFF);
							break;
						}
					} break;
					// STACK LENGTH = 0x100
					case OPCODE.PUSH: { // PUSH_VALUE
						TYPE  type = TYPE.VOID;
						int   value = 0;
						bool  extended = false;
						ubyte kind  = ((c2 >> 16) & 0xFF);
						uint  param = (c2 & 0xFFFF);
						uint param2;
						
						// String??
						if (kind & 0x80) {
							param2 = read_word;
							//throw(new Exception("0x80"));
							type = TYPE.String;
							labels_code_data[param2] ~= offset;
							if (display) writefln("  %08X: PUSH_VALUE(%s) : %08X, data_%08X // '%s'", offset, to!string(type), c2, param2, get_string_addslashes(param2));
						}
						else {
							type = cast(TYPE)kind;
							switch (type) {
								case TYPE.VOID:
								break;
								case TYPE.Integer32:
									param = read_word;
								break;
								case TYPE.Integer8Unsigned:
								break;
								case TYPE.Integer8Signed:
								break;
								case TYPE.NULL:
								break;
								case TYPE.Integer16Signed:
								break;
								case TYPE.Integer32_2:
									param = read_word;
								break;
								case TYPE.Integer32Signed:
									param = read_word;
								break;
								case TYPE.Float32:
									// 0000EB54: PUSH_VALUE(Float32) : (40490FD9) : 3.141592
									// M_PI
									param = read_word;
								break;
								case TYPE.FALSE:
								break;
								case TYPE.TRUE:
								break;
								case TYPE.UNDEFINED:
								break;
							}

							if (type == TYPE.Float32) {
								if (display) writefln("  %08X: PUSH_VALUE(%s) : %f", offset, to!string(type), *cast(float*)&param);
							} else {
								if (display) writefln("  %08X: PUSH_VALUE(%s) : %08X", offset, to!string(type), param);
							}
						}
					} break;
					case OPCODE.PUSH_ARRAY: {
						auto type   = cast(TYPE)((c2 >> 16) & 0xFF);
						uint nbytes = c2 & 0xFFFF;
						uint type_size = getTypeSize(type);
						uint nelements = type_size ? (nbytes / type_size) : 0;
						uint array_pointer = read_word;
						if (!display) {
							labels_code_data[array_pointer] ~= offset;
						} else {
							writefln("  %08X: %s(%06X): data_%08X", offset, to!string(opcode), c2 & 0xFFFFFF, array_pointer);
							foreach (m; 0..nelements) {
								void* element_ptr = data.ptr + header.text_start.value + array_pointer + m * type_size;
								switch (type) {
									case TYPE.VOID:
										writefln("    %d: // void", m);
									break;
									case TYPE.Integer32: {
										auto value = bswap(*cast(uint *)element_ptr);
										writefln("    %d: // (%08X)'%s'", m, value);
									} break;
									case TYPE.Integer8Unsigned: {
										auto value = *cast(ubyte *)element_ptr;
										writefln("    %d: // %02X", m, value);
									} break;
									case TYPE.Integer8Signed: {
										auto value = *cast(byte *)element_ptr;
										writefln("    %d: // %d", m, value);
									} break;
									case TYPE.NULL:
										writefln("    %d: // null", m);
									break;
									case TYPE.Integer16Signed:
										writefln("    %d: // Integer16Signed(...)", m);
									break;
									case TYPE.Integer32_2:
										writefln("    %d: // Integer32_2(...)", m);
									break;
									case TYPE.Integer32Signed: {
										auto value = cast(int)bswap(*cast(uint *)element_ptr);
										writefln("    %d: // Integer32Signed(%d)", m, value);
									} break;
									case TYPE.Float32:
										writefln("    %d: // float32(...)", m);
									break;
									case TYPE.FALSE:
										writefln("    %d: // false", m);
									break;
									case TYPE.TRUE:
										writefln("    %d: // true", m);
									break;
									case TYPE.UNDEFINED:
										writefln("    %d: // undefined", m);
									break;
									case TYPE.String:
										uint text_ptr = bswap(*cast(uint *)element_ptr);
										writefln("    %d: // (data_%08X)'%s'", m, text_ptr, get_string_addslashes(text_ptr));
									break;
								}
							}
						}
					} break;
					case OPCODE.UNK_03: {
						uint text_block_addr = read_word;
						if (display) writefln("  %08X: %s(%06X): %08X", offset, to!string(opcode), c2 & 0xFFFFFF, text_block_addr);
						//if (((c2 >> 16) & 0xFF) == 0x0C)
						{
							auto tb = cast(TEXT_BLOCK*)(data.ptr + header.text_start.value + text_block_addr);
							//text_ptr = bswap(check[n]);
							if (display) {
								/*
								tb.dump();
								writefln("       // '%s'", get_string_addslashes(tb.ja.title.value));
								writefln("       // '%s'", get_string_addslashes(tb.ja.text.value));
								writefln("       // '%s'", get_string_addslashes(tb.en.title.value));
								writefln("       // '%s'", get_string_addslashes(tb.en.text.value));
								*/
							}
						}
						// SET_TEXT(0C0100): 00001318
					} break;
					case OPCODE.STACK_SUBSTRACT: { // POP_RELATED?
						uint parameter = read_word;
						if (display) writefln("  %08X: %s(%06X): %08X", offset, to!string(opcode), c2 & 0xFFFFFF, parameter);
					} break;
					case OPCODE.CALL: {
						uint parameter = read_word;
						short nparams = (c2 >> 16) & 0xFF;
						short func    = bswap16(c2 & 0xFFFF);
						if (func == -1) {
							if (display) writefln("  %08X: CALL_SCRIPT(params:%d, function:%08X)(%06X)", offset, nparams, parameter, c2 & 0xFFFFFF);
						} else {
							if (display) writefln("  %08X: CALL_NATIVE(params:%d, function:%04X)(%06X)", offset, nparams, func, c2 & 0xFFFFFF);
						}
					} break;
					case OPCODE.RETURN:
						if (display) writefln("  %08X: RETURN %06X", offset, c2 & 0xFFFFFF);
					break;
					case OPCODE.EXIT:
						if (display) writefln("  %08X: END_OF_SCRIPT %d", offset, (c2 && 0xFFFFFF));
					break;
					case OPCODE.JUMP_ALWAYS, OPCODE.JUMP_FALSE, OPCODE.JUMP_TRUE: {
						uint parameter = read_word;
						if (!display) {
							labels_code_code[parameter] ~= offset;
						} else {
							writefln("--%08X: %s(%06X): code_%08X (pos in file %08X)", offset, to!string(opcode), c2 & 0xFFFFFF, parameter, offset + header.script_start.value);
						}
					} break;
					case OPCODE.BRANCH_ALWAYS, OPCODE.BRANCH_FALSE, OPCODE.BRANCH_TRUE: {
						if (display) writefln("--%08X: %s(%06X)", offset, to!string(opcode), c2 & 0xFFFFFF);
					} break;
					/*case 0x0B: { // ?? Very simple
					} break:*/
					case OPCODE.UNK_0E: // PUSH
						if (display) writefln("  %08X: %s (%06X)", offset, to!string(opcode), c2 & 0xFFFFFF);
					break;
					case OPCODE.START_FUNCTION_11: {
						uint parameter = read_word;
						if (display) writefln("----------------------------------------------------");
						if (display) writefln("  %08X: %s (%08X)", offset, to!string(opcode), parameter);
					} break;
					case OPCODE.UNK_14:
						if (display) writefln("  %08X: POP %06X", offset, c2 & 0xFFFFFF);
					break;
					default:
						if (display) writefln("  %08X: UNK_%02X (%06X)", offset, opcode, c2 & 0xFFFFFF);
					break;
				}
			}
		}
	}

	void dump_opcode_usage() {
		writefln("== OPCODE USAGE =====================");
		foreach (v; opcode_usage.keys.sort) {
			writefln("VALUE:%02X : %d", v, opcode_usage[v]);
		}
	}

	dump_header();
	dump_script();
	dump_data_pointers();
	dump_data_texts();
	dump_opcode_usage();
	writefln("=====================================");
}

void processScriptFile(string filename) {
	auto data = cast(ubyte[])std.file.read(filename);
	processScript(data);
}

int main(string[] argv) {
	if (argv.length < 2) {
		writefln("dump_tss.exe <tss_file>");
		return -1;
	}
	processScriptFile(argv[1]);
	//C:\projects\talestra\tov\scenario_us\1270.u
	//processScriptFile("string_dic_uk.so");
	//processScriptFile(r"L:\games\vesperia\btl.svo.d\BTL_PACK_UK.DAT.d\0003.DAT.d\BTL_EP_0070_010.u");
	//processScriptFile("scenario_uk/0500.dat.u");
	//processScriptFile("scenario_uk/1270.dat.u");
	return 0;
}
