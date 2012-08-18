import std.stdio, std.file, std.string, std.stream, std.string, std.c.string;

long parsehex(char[] s, bool ignore = false)
{
	long r;
	long sign = +1;
	foreach (c; s) {
		r <<= 4;
		if (c == '-') { sign *= -1; continue; }
		if (c >= '0' && c <= '9') {
			r |= c - '0';
		} else if (c >= 'a' && c <= 'f') {
			r |= c - 'a' + 10;
		} else if (c >= 'A' && c <= 'F') {
			r |= c - 'A' + 10;
		} else {
			//writefln(s);
			if (!ignore) throw(new Exception("Invalid value"));
		}
	}
	return sign * r;
}

long parsedec(char[] s, bool ignore = false)
{
	long r;
	long sign = +1;
	foreach (c; s) {
		r *= 10;
		if (c == '-') { sign *= -1; continue; }
		if (c >= '0' && c <= '9') {
			r += c - '0';
		}else {
			//writefln(s);
			if (!ignore) throw(new Exception("Invalid value"));
		}
	}
	return sign * r;
}

char[][] splittoks(char[] s, char[] toks = " \t\r\n,()=")
{
	char[][] r;
	char[] buf;
	char[0x100] lookup_toks;
	for (int n = 0; n < 0x100; n++) lookup_toks[n] = 0;
	foreach (tok; toks) lookup_toks[tok] = 1;
	//writefln(lookup_toks);
	//return r;
	
	void flush()
	{
		if (buf.length) r ~= buf;
		buf = "";
	}
	
	foreach (c; s) {
		if (lookup_toks[c]) {
			flush();
		} else {
			buf ~= c;
		}
	}
	flush();
	return r;
}

struct INS
{
	char[] type;
	char[][] extra;
	long[] params;
	alias params v;
	
	char[] toString() {
		char[] r = std.string.format("%s %s", type, params);
		if (extra.length) r ~= std.string.format(" ; %s", extra);
		return r;
	}
	
	uint opIndex(int n) { return v[n]; }
	
	static INS opCall(char[] type, long[] params = null, char[][] extra = null) {
		INS ins = void;
		ins.type = type;
		ins.params = params;
		ins.extra = extra;
		return ins;
	}
}

ubyte[] memory;

long[32] REGS;
long R_ctr;
long R_lr = 3;
long CR[8];

void exec_end() {
	writefln("REGS:");
	for (int n = 0; n < 32; n++) {
		writefln("  r%-2d   = 0x%08X (%d)", n, cast(uint)REGS[n], REGS[n]);
	}
	for (int n = 0; n < 8; n++) {
		writefln("  rCMP%d = %d", n, CR[n]);
	}
	writefln("  rCTR  = %d", R_ctr);
	writefln("  rLR   = 0x%08X", R_lr);
	throw(new Exception("BREAKED!"));
}

char[] binhex(ubyte[] data) {
	char[] r;
	foreach (c; data) r ~= std.string.format("%02X ", c);
	return strip(r);
}

long memread_r(uint offset, uint count = 4) {
	ulong r;
	if (offset + count > memory.length) {
		throw new Exception(std.string.format("Invalid address 0x%08X\n", offset));
	}
	if (offset >= 0x01700000 && offset <= 0x01C50000) {
		//if (trace)
		//writefln("||READ(%08X) <- [%s]", offset, binhex(memory[offset..offset + count]));
	}
	for (int n = 0; n < count; n++) {
		r <<= 8;
		r |= memory[offset + n];
	}
	return r;
}

long memwrite_r(long data, uint offset, uint count = 4) {
	long r = data;
	if (data != 0) {
		//writefln("%08X <- 0x%02X", offset, data);
	}
	if (offset >= 0x00000000 && offset < 0x01700000) {
		//if (trace)
		writefln("||WRITE(%08X) <- [%s]", offset, binhex(memory[offset..offset + count]));
	}
	for (int n = count; n > 0; n--) {
		//writefln("%08X: %02X", offset + n - 1, (r & 0xFF));
		memory[offset + n - 1] = (r & 0xFF);
		r >>= 8;
	}
	return data;
}

bool trace = false;

void execute(ref long PC, ref INS i)
{
	static uint ROT32(uint v, int p)
	{
		// Shift left.
		if (p < 0) {
			p = -p;
			return (v << p) | (v >> (32 - p));
		}
		// Shift right;
		else {
			return (v >> p) | (v << (32 - p));
		}
	}
	
	alias ROT32 ROT;

	static uint MASK(int p1, int p2)
	{
		assert(p1 <= p2);
		int len = (p2 - p1) + 1;
		int off = p1 - 1;
		return ((1 << len) - 1) << off;
	}

	long memread(uint offset, uint count = 4) {
		long r = memread_r(offset, count);
		//if (trace) writefln("::%08X == %08X", offset, r);
		return r;
	}
	
	long memwrite(long data, uint offset, uint count = 4) {
		//if (trace) writefln("::%08X <- %08X", offset, data);
		return memwrite_r(data, offset, count);
	}
	
	long setReg(int reg, long v) {
		if (reg == 0) return 0;
		if (reg == 1) {
			if (trace) writefln("                                                      <SP += %d>", v - REGS[reg]);
		}
		return REGS[reg] = v;
	}
	
	long COMP_U(uint a, uint b = 0) {
		if (a < b) {
			return -1;
		} else if (a > b) {
			return +1;
		} else {
			return 0;
		}
	}

	long COMP_S(long a, long b = 0) {
		return a - b;
	}
	
	long sR(int p, long v) {
		return setReg(i[p], v);
	}
	long sR0(long v) { return sR(0, v); }
	long R(int p) { return (i[p] != 0) ? REGS[i[p]] : 0; }
	ulong Ru(int p) { return R(p); }
	int I(int p) { return i[p]; }
	ushort Iu(int p) { return cast(ushort)cast(short)I(p); }
	void JUMP(uint addr) {
		//if (trace) writefln("-------------------------------");
		PC = addr;
	}
	
	void CMPREG_ASSUME(int len) {
		if (i.v.length < len) i.v = [cast(long)0] ~ i.v.dup;
	}
	
	//if (PC >= 0x820D5294) trace = true;
	//trace = true;
	
	//if (PC >= 0x820D5294) throw(new Exception("end"));
	
	if (trace) {
		switch (PC) {
			case 0x820D52B4: writefln(":read"); break;
			case 0x820D52CC: writefln(":uncomp"); break;
			case 0x820D52EC: writefln(":comp"); break;
			default:
			break;
		}
		writefln("  %08X: %s", PC, i);
	}

	
	
	//if (PC == 0x820D55B0) exec_end();
	//if (PC == 0x820D55B8) exec_end();
	//if (PC == 0x820D55FC) exec_end();
	
	// End of new compression.
	if (PC == 0x820D5604) exec_end();
	
	// End of decompress_chunk call.
	//if (PC == 0x823C7C40) exec_end();
	
	// test
	if (PC == 0x823C88D0) {
		/*
		int count = 0;
		//if (++count >= 100) exec_end();
		if (++count >= 0) exec_end();
		*/
		//exec_end();
	}
	
	if (PC == 0x823C7F18) {
		writefln(
			"decompress_chunk(\n"
			"    ptr_state = 0x%08X,\n"
			"    buf_U_len = %d,\n"
			"    buf_C_ptr = 0x%08X,\n"
			"    buf_C_len = %d,\n"
			"    buf_U_ptr = 0x%08X\n"
			")",
			REGS[3], REGS[4], REGS[5], REGS[6], REGS[7]
		);
		//exec_end();
	}
	if (PC == 0x823C8580) {
		writefln("compression_FFnew_inner_real_kind_final");
	}
	
	if (PC == 0x823C8858) {
		//writefln("cfunc_read_2_bytes_ext4_real");
	}
	
	//trace = true;
	
	if (PC >= 0x823C8580 && PC <= 0x823C87E8) {
		//trace = true;
	} else {
		//trace = false;
	}
	
	//if (PC == 0x823C7C3C) { PC += 4; return; } // bl      decompress_chunk # r3 = ptr_state_0x14
	
	PC += 4;
retry:
	switch (i.type)
	{
// ALU.
		case "mr."  : CR[0] = COMP_S(sR0(R(1)), 0); break;
		case "mr"   : sR0(R(1)); break;
		case "li"   : sR0(Iu(1)); break;
		case "lis"  : sR0(Iu(1) << 16); break;

		case "addi"   : sR0(R(1) + I(2)); break;
		case "addic." : CR[0] = COMP_S(sR0(R(1) + I(2)), 0); break; // TODO: carry
		case "add"    : sR0(R(1) + R(2)); break;
		case "add."   : CR[0] = COMP_S(sR0(R(1) + R(2)), 0); break;
		
		case "neg"    : sR0(~R(1) + 1); break;

		case "subf"   : sR0(R(2) - R(1)); break;
		case "subf."  : CR[0] = COMP_S(sR0(R(2) - R(1)), 0); break;
		case "subfic" : sR0(I(2) - R(1)); break; // TODO: carry

		case "oris" : sR0(R(1) | (Iu(2) << 16)); break;
		case "ori"  : sR0(R(1) | Iu(2)); break;
		case "or"   : sR0(R(1) | Ru(2)); break;
		
		case "and"  : sR0(R(1) & Ru(2)); break;
		
		case "xori" : sR0(R(1) ^ Iu(2)); break;

		case "cntlzw": {
			// CHECK!
			ulong c = Iu(1);
			int count = 0;
			while (!((c >>= 1) & 1)) count++;
			sR0(count);
		} break;
		
		case "extrwi": {
			i.type = "rlwinm";
			i.params = [I(0), I(1), I(3), 0, I(2) - 1];
			goto retry;
		} break;
		
		//.text:8237209C                 clrrwi  %r30, %r31, 7   # Clear Right Immediate

		case "clrrwi.":
		case "clrrwi":
			i.type = (i.type[i.type.length - 1] != '.') ? "rlwinm" : "rlwinm.";
			i.params = [I(0), I(1), 0, 0, 31 - I(2)];
			goto retry;
		break;
		case "clrlwi.":
		case "clrlwi":
			i.type = (i.type[i.type.length - 1] != '.') ? "rlwinm" : "rlwinm.";
			i.params = [I(0), I(1), 0, I(2), 31];
			goto retry;
		break;
		case "rotlwi"  : // Shift Right Immediate
			sR0(ROT(R(1), -I(2)));
		break;
		
		case "slw": sR0(R(1) << (R(2)& 0xFF)); break; // Shift Left Word
		case "srw": sR0(R(1) >> (R(2)& 0xFF)); break; // Shift Right Word

		// TO CHECK.
		case "extsb.":
		case "extsb": {
			long r;
			r = sR0(cast(long)cast(byte)(R(1) & 0xFF));
			//writefln("extsb: %d (%08X)", r, r);
			//if (i.type[i.type.length - 1] == '.') CR[0] = r;
			if (i.type[i.type.length - 1] == '.') CR[0] = COMP_S(r, 0);
		} break;

		case "extsh.":
		case "extsh": {
			long r;
			r = sR0(cast(long)cast(short)(R(1) & 0xFFFF));
			//writefln("extsb: %d (%08X)", r, r);
			//if (i.type[i.type.length - 1] == '.') CR[0] = r;
			if (i.type[i.type.length - 1] == '.') CR[0] = COMP_S(r, 0);
		} break;

		// TODO
		// WARNING!! Signed?
		case "cmpwi": CMPREG_ASSUME(3); CR[I(0)] = COMP_S(R(1), I(2)); break; // Compare Logical Word Immediate
		case "cmpw" : CMPREG_ASSUME(3); CR[I(0)] = COMP_S(R(1), R(2)); break; // Compare Logical Word
		
		case "cmplwi": CMPREG_ASSUME(3); CR[I(0)] = COMP_U(R(1), I(2)); break; // Compare Logical Word Immediate
		case "cmplw" : CMPREG_ASSUME(3); CR[I(0)] = COMP_U(R(1), R(2)); break; // Compare Logical Word

		case "slwi"  : sR0(R(1) << I(2)); break; // Shift Right Immediate
		case "srwi"  : sR0(R(1) >> I(2)); break; // Shift Right Immediate
		case "rlwinm.":
		case "rlwinm": {// Rotate Left Word Immediate then AND with Mask
			ulong mask = MASK(32 - i[4], 32 - i[3]);
			long r = sR0(ROT(R(1), -I(2)) & mask);
			
			switch (PC - 4) {
				case 0x823C7F4C: //.text:823C7F4C                 rlwinm  %r11, %r3, 0,21,22 # Rotate Left Word Immediate then AND with Mask
				case 0x823C7F58:
					//writefln("%08X", mask);
				break;
				default:
				break;
			}
			
			if (i.type == "rlwinm.") CR[0] = COMP_S(r, 0);
			if (trace) {
				//writefln(I(2));
				writefln("%08X", MASK(32 - i[4], 32 - i[3]));
				writefln(R(0));
			}
			//rlwinm  %r11, %r31, 0,23,23 # Rotate Left Word Immediate then AND with Mask
		} break;
		
		case "dcbt": // Data Cache Block Touch
			// ignore.
			// http://publib.boulder.ibm.com/infocenter/systems/index.jsp?topic=/com.ibm.aix.aixassem/doc/alangref/dcbt.htm
		break;
		case "dcbtst": // Data Cache Block Touch for Store
			// ignore.
		break;
		
		case "rlwimi": // Rotate Left Word Immediate then Mask Insert
			ulong mask = MASK(32 - i[4], 32 - i[3]);
			sR0((ROT(R(1), -I(2)) & mask) | (R(0) & ~mask));
		break;
		
		// insrwi  %r7, %r8, 8,16  # Insert from Right Immediate
		case "insrwi":
			i.type = "rlwimi";
			i.params = [I(0), I(1), 32 - (I(3) + I(2)), I(3), (I(3) + I(2)) - 1];
			goto retry;
		break;
		
// Special.
		case "mfctr": sR0(R_ctr); break;
		case "mtctr": R_ctr = R(0); break;
		
		case "mflr" :
			sR0(R_lr);
			if (trace) writefln("***************************************** %08X | SP(%08X)", R(0), REGS[1]);
		break;
		case "mtlr" :
			R_lr = R(0);
			if (trace) writefln("***************************************** %08X | SP(%08X)", R(0), REGS[1]);
		break;

// Memory transfer.
		case "stb"  : memwrite(R(0), R(2) + I(1), 1); break;
		case "stbx" : memwrite(R(0), R(2) + R(1), 1); break;
		case "stbu" : memwrite(R(0), R(2) + I(1), 1); sR(2, R(2) + I(1)); break;

		case "sth"  : memwrite(R(0), R(2) + I(1), 2); break;
		case "sthx" : memwrite(R(0), R(2) + R(1), 2); break;

		case "stw"  : memwrite(R(0), R(2) + I(1), 4); break;
		case "stwu" : memwrite(R(0), R(2) + I(1), 4); sR(2, R(2) + I(1)); break;
		case "stwx" : memwrite(R(0), R(2) + R(1), 4); break;

		case "std"  : memwrite(R(0), R(2) + I(1), 8); break;
		case "stdx" : memwrite(R(0), R(2) + R(1), 8); break;
		
		case "lbz"  : sR0(memread(R(2) + I(1), 1)); break;
		case "lbzx" : sR0(memread(R(2) + R(1), 1)); break;
		case "lbzu" : sR0(memread(R(2) + I(1), 1)); sR(2, R(2) + I(1)); break;

		case "lhzx" : sR0(memread(R(2) + R(1), 2)); break;
		case "lhz"  : sR0(memread(R(2) + I(1), 2)); break;
		
		// CHECK.
		case "lhax" : sR0(cast(long)cast(short)(memread(R(2) + R(1), 2) & 0xFFFF)); break;
		
		case "lwz"  : sR0(memread(R(2) + I(1), 4)); break;
		case "lwzx" : sR0(memread(R(2) + R(1), 4)); break;

		case "ld"   : sR0(memread(R(2) + I(1), 8)); break;

// Branches.
		case "bdnz" : if (R_ctr--       ) JUMP(I(0)); break;
		case "blt"  : CMPREG_ASSUME(2); if (CR[I(0)] <  0) JUMP(I(1)); break;
		case "bgt"  : CMPREG_ASSUME(2); if (CR[I(0)] >  0) JUMP(I(1)); break;
		case "bge"  : CMPREG_ASSUME(2); if (CR[I(0)] >= 0) JUMP(I(1)); break;
		case "ble"  : CMPREG_ASSUME(2); if (CR[I(0)] <= 0) JUMP(I(1)); break;
		case "bne"  : CMPREG_ASSUME(2); if (CR[I(0)] != 0) JUMP(I(1)); break;
		case "beq"  : CMPREG_ASSUME(2); if (CR[I(0)] == 0) JUMP(I(1)); break;
		case "b"    : JUMP(I(0)); break;
		case "bl"   : R_lr = PC; JUMP(I(0)); break;

		// lr.
		case "blr"  : JUMP(R_lr); break;
		case "beqlr": CMPREG_ASSUME(1); if (CR[I(0)] == 0) JUMP(R_lr); break;
		case "bnelr": CMPREG_ASSUME(1); if (CR[I(0)] != 0) JUMP(R_lr); break;
		case "bgtlr": CMPREG_ASSUME(1); if (CR[I(0)] >  0) JUMP(R_lr); break;
		
		
		case "nop": break;
		case "unknwon":
			writefln("FATAL ERROR! (unknwon instruction)");
		break;

		default:
			throw(new Exception(std.string.format("Unknown instruction (%s) at 0x%08X", i, PC - 4)));
		break;
	}
}

void main()
{
	INS[ulong] code;
	char[][long] labels_r;
	long[char[]] labels;
	long[char[]][long] labels_local;
	char[][char[]][long] variables_local;
	long local_func;
	char[][char[]] variables;
	int find_pos;
	
	char[] asm_input;
	asm_input = "comp_01.s";
	//asm_input = "default.asm";
	
	auto s = new BufferedStream(new File(asm_input, FileMode.In));
	long PC = 0;
	
	//
	labels["j_DbgBreakPoint"] = 0x1_00000000 | 1;
	labels["MmQueryAddressProtect"] = 0x1_00000000 | 2;
	//
	labels["memcpy"]  = 0x1_00000000 | 11;
	labels["memset"]  = 0x1_00000000 | 12;
	//
	labels["malloc"]  = 0x1_00000000 | 21;
	labels["__realloc"] = labels["realloc"] = 0x1_00000000 | 22;
	labels["free"]    = 0x1_00000000 | 23;
	
	foreach (analyze; [true, false]) {
		s.position = 0;
		while (!s.eof) {
			auto line = strip(s.readLine);
			if (line.length < 6) continue;
			if (line[0..6] == ".text:") {
				PC = parsehex(line[6..6 + 8]);
				line = strip(line[6 + 8..line.length]);
				if (!analyze) {
					if ((PC in code) is null) code[PC] = INS("nop", []);
				}
			}
			if ((find_pos = find(line, "#")) >= 0) {
				char[] comment = line[find_pos..line.length];
				line = strip(line[0..find_pos]);
				
				if ((find_pos = find(line, "S U B R O U T I N E")) >= 0) {
					local_func = PC;
				}
			}

			if (!line.length) continue;
			
			if ((find_pos = find(line, ":")) >= 0) {
				line = strip(line[0..find_pos]);
				// Only new labels.
				if ((line in labels) is null) {
					labels[line] = PC;
				} else {
					if (analyze) {
						writefln("Warning repeated label '%s'", line);
					}
				}
				labels_r[PC] = line;
				labels_local[local_func][line] = PC;
				continue;
			}
			
			char[][] extra;

			long getValue(char[] token) {
				while (1) {
					if ((local_func in variables_local) && (token in variables_local[local_func])) token = variables_local[local_func][token];
					else if (token in variables) token = variables[token];
					else break;
				}
				//if (token.length >= 4 && token[0..4] == "var_") token = "-0x" ~ token[4..token.length];
				if (token == "%sp") token = "%r1";
				if ((token.length >= 2) && (token[0..2] == "%r" || token[0..2] == "cr")) {
					return parsedec(token[2..token.length]);
				} else if ((token.length >= 3) && (token[0..2] == "0x" || token[0..3] == "-0x")) {
					return parsehex(token[2 + (token[0] == '-')..token.length]) * ((token[0] == '-') ? -1 : +1);
				} else {
					try {
						return parsedec(token);
					} catch {
						extra ~= token;
						//labels_local[local_func][line]
						if ((local_func in labels_local) && (token in labels_local[local_func])) {
							//writefln("label local");
							//throw(new Exception(std.string.format("Local label '%s'", token)));
							return labels_local[local_func][token];
						} else if (token in labels) {
							return labels[token];
						} else {
							//throw(new Exception(std.string.format("Unknown label '%s'", token)));
							writefln("Unknown label '%s'", token);
							return -1;
						}
					}
				}
			}
			
			if (!analyze) {
				if ((find_pos = find(line, "=")) >= 0) {
					if (line[0] != '.') {
						auto tokens = splittoks(line);
						variables_local[local_func][tokens[0]] = tokens[1];
						variables[tokens[0]] = tokens[1];
					}
				}
			
				auto tokens = splittoks(line);
				long[] params;
				if (tokens[0][0] == '.') {
					if (tokens[0] == ".set") {
						variables_local[local_func][tokens[1]] = tokens[2];
						variables[tokens[1]] = tokens[2];
					}
					code[PC] = INS("unknwon");
					continue;
				}
				foreach (k, token; tokens[1..tokens.length]) {
					try {
						auto ctokens = split(token, "+");
						long v = 0;
						foreach (ctoken; ctokens) v += getValue(ctoken);
						params ~= v;
					} catch (Exception e) {
						writefln("%s", tokens);
						writefln("%s", token);
						writefln("%s", e.toString);
						return;
					}
				}
				code[PC] = INS(tokens[0], params, extra);
			}
			PC += 4;
		}
	}
	
	memory.length = 32 * 1024 * 1024;
	ubyte[] data;
	//data = cast(ubyte[])read("emulator.d.c3");
	//data = cast(ubyte[])read("emulator.d.c1");
	//data = cast(ubyte[])read("emulator.d.c0");
	data = cast(ubyte[])read("data.cc");
	
	memory[0..0x10] = 0xFF;

	//data = data[9..data.length];
	if (0) {
		REGS[4] = 0x10;                    // buf_C_start
		REGS[5] = REGS[4] + data.length;  // buf_C_end
		REGS[3] = REGS[5];                // buf_U_ptr
	} else {
		REGS[3] = 0x10;                    // buf_U_ptr
		REGS[4] = 23 * 1024 * 1024;       // buf_C_start
		REGS[5] = REGS[4] + data.length;  // buf_C_end
	}

	memory[REGS[4]..REGS[5]] = data;
	
	REGS[1] = memory.length - 0x10; // SP
	
/*
# r3 = buf_U_ptr
# r4 = buf_C_start
# r5 = buf_C_end
# r6 = slidding_ptr
*/

	//PC = 0x820D51C0; // comp_01
	PC = 0x820D5570; // comp_select
	INS cins;
	ulong count = 0;
	long prev_PC;
	try {
		while (true)
		{
			count++;
			if (count >= 0x1000000) { writefln("Breaked execution at %d instructions.", count); break; }
			switch (PC) {
				// j_DbgBreakPoint
				case 1:
					PC = R_lr;
					switch (cast(uint)REGS[3]) {
						case 0x80004005: writefln("E_FAIL"); break;
						case 0x81DE2001: writefln("E_XMCDERR_MOREDATA"); break;
						case 0x8007000E: writefln("E_OUTOFMEMORY"); break;
					}
					throw(new Exception(std.string.format("j_DbgBreakPoint(0x%08X)", cast(uint)REGS[3])));
				break;
				// MmQueryAddressProtect
				case 2:
					//REGS[3] = 0x0000000; // TODO!
					REGS[3] = 0x8000600; // TODO!
					PC = R_lr;
				break;
				// memcpy.
				case 11:
					writefln("##memcpy(0x%08X, 0x%08X, %d)", REGS[3], REGS[4], REGS[5]);
					memcpy(
						memory.ptr + REGS[3], // r3 = dst_ptr
						memory.ptr + REGS[4], // r4 = src_ptr
						REGS[5] // r5 = length
					);
					PC = R_lr;
				break;
				// memset.
				case 12:
					writefln("##memset(0x%08X, 0x%02X, %d)", REGS[3], REGS[4], REGS[5]);
					memset(
						memory.ptr + REGS[3], // r3 = ptr
						REGS[4], // r4 = byte
						REGS[5] // r5 = count
					);
					PC = R_lr;
				break;
				// malloc
				case 21:
					PC = R_lr;
					throw(new Exception(std.string.format("malloc(%d)", REGS[3])));
				break;
				// malloc_specialcx 
				case 22:
					PC = R_lr;
					writefln("##malloc_special(0x%08X, %d)", REGS[4], REGS[3]);
					REGS[3] = 0x1E00000; // test
					//REGS[3] = 0;
					//throw(new Exception(std.string.format("malloc_special(0x%08X, %d)", REGS[4], REGS[3])));
				break;
				// free
				case 23:
					PC = R_lr;
					throw(new Exception(std.string.format("free(0x%08X)", REGS[3])));
				break;
				default: break;
			}

			if (!(PC in code)) {
				throw(new Exception(std.string.format("END AT %08X prev %08X", PC, prev_PC)));
				if (prev_PC in code) {
					//writefln(code[prev_PC]);
				}
				writefln();
				break;
			}

			//printf("%08X\r", PC);
			
			cins = code[PC];
			prev_PC = PC;
			execute(PC, code[PC]);
			
			// JUMP.
			if (PC != prev_PC + 4) {
				if (trace) {
					writefln("-------------------------");
					if (PC in labels_r) {
						writefln("  %s:", labels_r[PC]);
					} else {
						writefln("  loc_%08X:", PC);
					}
					writefln("-------------------------");
				}
			}
		}
	} catch (Exception e) {
		writefln("+++++++++++++++++++++++++++++++++++++++++++++++++++++++");
		writefln("%08X: %s", PC - 4, cins);
		writefln(e);
	}

	std.file.write("mem.dump", memory);
}