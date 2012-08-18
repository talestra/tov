import std.stdio, std.string, std.stream, std.file, std.intrinsic;

//debug = debug_ins;
//debug = debug_io;

debug (debug_ins) {
	static int do_debug_ins = 1;
} else {
	static int do_debug_ins = 0;
}

static ulong MASK(ubyte begin, ubyte end) {
	int b = 31 - end;
	int e = 31 - begin;
	ulong v = ((1 << (e - b)) - 1) << b;
	//writefln("%032b", v);
	return v;
}

static uint extract_bits(uint v, ubyte from, ubyte to)
{
	return (v >> (31 - to)) & ((1 << (to - from)) - 1);
}

static uint extract_bits_se(uint v, ubyte from, ubyte to)
{
	uint rv = (v >> (31 - to)) & ((1 << (to - from)) - 1);
	if ((rv >> (31 - from - 1)) & 1) {
	}
	return rv;
}

static ulong SPR[0x10]; // special registers (ctr, lr, xer)
static ulong R[0x20];   // 32 registros generales
static int   CR[8];     // 8 registros de comparación
static ulong PC;        // PC. Program Counter.

// R[1] = SP

class Memory {
	ubyte[] data;
	ulong start = 0x80000000;
	this() {
		data = new ubyte[0x3000000];
	}
	ubyte[] opSlice(ulong start, ulong end) {
		return data[start - start..end - start];
	}
	ubyte* ptr(ulong addr) {
		if (addr < start || addr >= start + data.length) return null;
		return &data[addr - start];
	}
	void store(ulong addr, ubyte[] buf) {
		ubyte *p = ptr(addr);
		debug (debug_io) writefln("STORE:%08X: %s", addr, buf);
		if (p !is null) {
			p[0..buf.length] = buf;
		} else {
			writefln("Can't store at 0x%08X", addr);
		}
	}
	void write1(ulong addr, ubyte v) {
		//writefln("%08X <- %02X", addr, v);
		auto p = cast(ubyte *)ptr(addr);
		if (p !is null) *p = v;
	}
	void write4(ulong addr, uint v) {
		auto p = cast(uint *)ptr(addr);
		if (p !is null) *p = v;
	}
	void write8(ulong addr, ulong v) {
		auto p = cast(ulong *)ptr(addr);
		if (p !is null) *p = v;
	}
	ubyte read1(ulong addr) {
		auto p = cast(ubyte *)ptr(addr);
		return (p != null) ? *p : 0;
	}
	uint read4(ulong addr) {
		auto p = cast(uint *)ptr(addr);
		return (p != null) ? *p : 0;
	}
	uint read4r(ulong addr) {
		uint v = std.intrinsic.bswap(read4(addr));
		//writefln("READ: %08X: %08X", addr, v);
		return v;
	}
	uint read8(ulong addr) {
		return read4r(addr + 0) | (read4r(addr + 4) << 4);
	}
	void add(ulong addr, ubyte[] data_add) {
		if (addr < start) return;
		if (addr >= start + data.length) return;
		this.data[addr - start .. addr - start + data_add.length] = data_add;
	}
}

/*class Memory {
	ubyte[][ulong] data;
	
	uint read4(ulong ptr) {
		foreach (start, data; this.data) {
			if (ptr < start) continue;
			if (ptr >= start + data.length) continue;
			return *cast(uint *)&data[ptr - start];
		}
		return 0;
	}
	
	uint read4r(ulong ptr) { return std.intrinsic.bswap(read4(ptr)); }
	void add(ulong ptr, ubyte[] data) { this.data[ptr] = data; }
}*/

struct bits32 {
	uint v;
	uint opSlice(ubyte from, ubyte to) {
		ubyte count = (to - from) + 1;
		uint mask = (1 << count) - 1;
		//writefln();
		return (v >> (31 - to)) & mask;
	}
	bool opIndex(ubyte idx) { return ((v >> (31 - idx)) & 1) != 0; }
	char[] toString() { return std.string.format("%032b", v); }
}

struct sbits32 {
	uint v;
	uint opSlice(ubyte from, ubyte to) {
		ubyte count = (to - from) + 1;
		uint mask = (1 << count) - 1;
		uint r = (v >> (31 - to)) & mask;
		if (r >> (count - 1)) {
			/*
			writefln("%d..%d", from, to);
			writefln("v: %032b", v);
			writefln(((31 - from) + count + 1));
			writefln(v >> ((31 - from) + count + 1));
			*/
			//writefln("[%08X]", r);
			if (do_debug_ins) {
				/*
				writefln("%032b", v >> ((31 - from) + count + 1));
				writefln("%032b", ~((1 << (31 - from - 1)) - 1));
				writefln("from: %d", from);
				writefln("from: %d", to);
				*/
			}
			r |= ~((1 << (count)) - 1);
		}
		//writefln("[%08X]", r);
		return r;
	}
	bool opIndex(ubyte idx) { return ((v >> (31 - idx)) & 1) != 0; }
	char[] toString() { return std.string.format("%032b", v); }
}

struct bits8 {
	uint v;
	uint opSlice(ubyte from, ubyte to) {
		ubyte count = (to - from) + 1;
		uint mask = (1 << count) - 1;
		//writefln();
		return (v >> from) & mask;
	}
	bool opIndex(ubyte idx) { return ((v >> idx) & 1) != 0; }
	char[] toString()
	{
		return std.string.format("%08b", v);
	}
}

static Memory memory;

// SPR = Special Purporse Register
// CTR = Count Register
// LR  = Link Register
// XER = Fixed-Point Exception Register
const uint CTR = 9;
const uint LR  = 8;
const uint XER = 1;
void decode(ulong addr, uint i)
{
	bits32 I = {i};
	sbits32 IS = {i};
	uint op_primary = I[0..5];
	
	void DO_COMP_U(ubyte bf, ulong v0, ulong v1) {
		//writefln("COMP: %d, %d", v0, v1);
		if      (v0 < v1) CR[bf] = 0b100;
		else if (v0 > v1) CR[bf] = 0b010;
		else              CR[bf] = 0b001;
	}
	
	bool BRANCH(bits8 bo, ubyte bi, bool lk, ulong eaddr) {
		// Decrement counter.
		bool cond_ok = true;
		
		// Decrement the CTR.
		if (!bo[2]) {
			SPR[CTR]--;
			if (do_debug_ins) writefln("COUNTER: %d", SPR[CTR]);
			cond_ok = cond_ok && (bo[1] == (SPR[CTR] == 0));
		}
		
		//writefln(bi);
		if (!bo[4]) {
			ubyte bf = (bi >> 2);
			ubyte bt = bi & 3;
			bool cond = false;
			//writefln(bt);
			switch (bt) {
				case 0: cond = (CR[bf] == 0b100); break; // lesser
				case 1: cond = (CR[bf] == 0b010); break; // bigger
				case 2: cond = (CR[bf] == 0b001); break; // equal
				case 3: cond = (CR[bf] == 0b000); break; // ??
			}
			if (do_debug_ins) {
				writefln("COND: %s : %02b", cond, bt);
				writefln("CR[%d] = %d", bf, CR[bf]);
			}
			cond_ok = cond_ok && !((bo[3] != 0) != cond);
		}
		
		// Link. Store next pointer to SPR.
		//if (lk) SPR[LR] = PC + 4;
		
		if (do_debug_ins) writefln("JUMP: %s: 0x%08X", cond_ok, eaddr);

		// JUMP
		if (cond_ok) {
			// Link. Store next pointer to SPR.
			if (lk) SPR[LR] = PC + 4;
			PC = eaddr;
			return true;
		} else {
			return false;
		}
	}
	
	switch (op_primary) {
		case 19: {
			int op_extend = I[21..30];
			switch (op_extend) {
				case 16: { // bclr|l BO,BI,BH || Branch Conditional to Link Register
					bits8 bo = {I[6..10]};
					uint  bi = I[11..15];
					bool  lk = I[31];

					if (do_debug_ins) writefln("%08X: bclr%s %05b, %d", addr, lk ? "l" : "", bo.v, bi);
					
					if (BRANCH(bo, bi, lk, SPR[LR])) return;
				} break;
				case 528: { // bcctr|l BO,BI,BH || Branch Conditional to Link Register
					bits8 bo = {I[6..10]};
					uint  bi = I[11..15];
					uint  bh = I[19..20];
					bool  lk = I[31];

					if (do_debug_ins) writefln("%08X: bcctr%s %05b, %d, %d", addr, lk ? "l" : "", bo.v, bi, bh);
					
					//writefln("%08X", SPR[CTR]);
					//writefln("%08X", R[4]);
					if (BRANCH(bo, bi, lk, SPR[CTR])) return;
				} break;
				default: {
					writefln("%08X: !!!op_extend_19(%d)", addr, op_extend);
					assert(0 == 1);
				}
			}
		} break;
		case 16: { // bcx
			bits8 bo = {I[6..10]};
			uint  bi = I[11..15];
			uint  bd = IS[16..29];
			bool  aa = I[30];
			bool  lk = I[31];
			uint  eaddr = aa ? (bd * 4) : (addr + bd * 4);
			
			if (do_debug_ins) {
				/*
				writefln("%032b", bi);
				writefln("%032b", IS[16..29]);
				writefln("%032b", I[16..29]);
				*/
			}

			if (do_debug_ins) writefln("%08X: bc%s%s %05b, %d, 0x%08X", addr, lk ? "l" : "", aa ? "a" : "", bo.v, bi, eaddr);
			
			if (BRANCH(bo, bi, lk, eaddr)) return;
		} break;
		case 18: { // branch?
			int  li = I[6..29];
			bool aa = I[30];
			bool lk = I[31];
			uint eaddr = addr + li * 4;
			// 823AAF6C
			//writefln("%08X : %d", addr + li * 4, lk);s
			if (do_debug_ins) writefln("%08X: b%s 0x%08X", addr, lk ? "l" : "", eaddr);
			if (lk) SPR[8] = PC + 4;
			PC = addr + li * 4;
			return;
		} break;
		/*
		case 30: {
			assert(0 == 1);
		} break;
		*/
		case 30: { // std (STore Double)
			uint s = I[6..10];
			uint a = I[11..15];
			uint d = I[16..31];
			if (do_debug_ins) writefln("%08X: std %%r%d, %d(%%r%d)", addr, s, d, a);
		} break;
		case 38: { // stb (STore Byte)
			uint rs = I[6..10];
			uint ra = I[11..15];
			uint d  = IS[16..31];
			if (do_debug_ins) writefln("%08X: stb %%r%d, %d(%%r%d)", addr, rs, d, ra);
			memory.write1(R[ra] + d, R[rs]);
		} break;
		case 58: {
			int op_extend = I[30..31];
			//writefln(op_extend);
			switch (op_extend) {
				case 0: { // ld || Load Doubleword
					int rt = I[6..10];
					int ra = I[11..15];
					int ds = IS[16..29];
					if (do_debug_ins) writefln("%08X: ld %%r%d, %d(%%r%d)", addr, rt, ds, ra);
					R[rt] = memory.read8(R[ra] + ds);
					writefln("%016X", R[rt]);
				} break;
				/*
				case 1: { // ldu || Load Doubleword with Update
				} break;
				case 2: { // lwa || Load Word Algebraic
				} break;
				*/
				default: {
					// invalid
					assert(0 == 1);
				} break;
			}
		} break;
		case 59: {
			assert(0 == 1);
		} break;
		case 36: { // stw RS,D(RA) || Store Word
			int rs = I[6..10];
			int ra = I[11..15];
			int d  = IS[16..31];
			//writefln("%032b", IS[16..31]);
			if (do_debug_ins) writefln("%08X: stw %%r%d, %d(%%r%d)", addr, rs, d, ra);
			memory.write4(R[ra] + d, R[rs]);
		} break;
		case 62: {
			int op_extend = I[30..31];
			//writefln(op_extend);
			switch (op_extend) {
				case 0: { // std || Store Doubleword
					int rs = I[6..10];
					int ra = I[11..15];
					int ds = IS[16..29];
					//writefln("%032b", I[16..31]);
					//writefln("%032b", IS[16..31]);
					if (do_debug_ins) writefln("%08X: std %%r%d, %d(%%r%d)", addr, rs, ds, ra);
					if (ra == 0) R[0] = 0;
					memory.write8(R[ra] + ds, R[rs]);
				} break;
				/*
				case 1: { // stdu || Store Doubleword with Update
				} break;
				*/
				default: {
					// invalid
					assert(0 == 1);
				} break;
			}
		} break;
		case 63: {
			assert(0 == 1);
		} break;
		case 31: { // op_extend?
			static char[][] SPRNAMES = ["", "xer", "", "", "", "", "", "", "lr", "ctr", "", "", "", "", "", ""];
			int op_extend = I[21..30];
			switch (op_extend)
			{
				case 32: { // cmpl BF,L,RA,RB
					int  bf = I[6..8];
					bool l  = I[10]; // long?
					int  ra = I[11..15];
					int  rb = I[16..20];
					
					DO_COMP_U(bf, l ? R[ra] : cast(uint)R[ra], l ? R[rb] : cast(uint)R[rb]);
				} break;
				case 215: { // stbx
					uint rs = I[6..10];
					uint ra = I[11..15];
					uint rb = I[16..20];
					if (do_debug_ins) writefln("%08X: stbx %%r%d, %%r%d, %%r%d", addr, rs, ra, rb);
					memory.write8(R[ra] + R[rb], R[rs]);
				} break;
				case 266: { // add
					uint d = I[6..10];
					uint a = I[11..15];
					uint b = I[16..20];
					bool oe = I[21];
					bool rc = I[31];
					if (do_debug_ins) writefln("%08X: add%s%s %%r%d, %%r%d, %%r%d", addr, (oe ? "o" : ""), (rc ? "." : ""), d, a, b);
					R[d] = R[a] + R[b];
				} break;
				case 339: { // mfspr
					uint spr = (I[11..20] >> 5) & 0b1111;
					uint d = I[6..10];
					bool reserved = I[31];
					R[d] = SPR[spr];
					if (do_debug_ins) writefln("%08X: mf%s %%r%d", addr, SPRNAMES[spr], d);
				} break;
				case 467: { // mtspr
					uint spr = (I[11..20] >> 5) & 0b1111;
					uint s = I[6..10];
					bool reserved = I[31];
					SPR[spr] = R[s];
					if (do_debug_ins) writefln("%08X: mt%s %%r%d", addr, SPRNAMES[spr], s);
				} break;
				case 444: { // or
					uint s = I[6..10];
					uint a = I[11..15];
					uint b = I[16..20];
					bool rc = I[31];
					if (s == b) {
						if (do_debug_ins) writefln("%08X: mr %%r%d, %%r%d", addr, a, s);
					} else {
						if (do_debug_ins) writefln("%08X: or %%r%d, %%r%d, %%r%d", addr, a, s, b);
					}
					R[a] = R[s] | R[b];
				} break;
				default: {
					writefln("!!!op_extend_31(%d)", op_extend);
					assert(0 == 1);
				} break;
			}
		} break;
		case 14: { // addi
			uint d = I[6..10];
			uint a = I[11..15];
			uint simm = I[16..31];
			if (a == 0) {
				if (do_debug_ins) writefln("%08X: li %%r%d, 0x%X", addr, d, simm);
			} else {
				if (do_debug_ins) writefln("%08X: addi %%r%d, %%r%d, 0x%X", addr, d, a, simm);
			}
			R[0] = 0;
			R[d] = R[a] + simm;
		} break;
		case 21: { // rlwinm || rlwinm.
			// clrlsldi Rx,Ry,b,n || rldic Rx,Ry,n,b-n
			// clrlwi  %r8, %r11, 24
			int  rs = I[6..10];
			int  ra = I[11..15];
			int  sh = I[16..20];
			int  mb = I[21..25];
			int  me = I[26..30];
			bool rc = I[31];
			if (do_debug_ins)
			{
				writefln("%08X: rlwinm%s %%r%d, %%r%d, %d, %d, %d", addr, rc ? "." : "", ra, rs, sh, mb, me);
			}
			//writefln(sh);
			R[ra] = (R[rs] << sh) & MASK(mb, me);
			
			//if (rc == 1) CR[0] = 1;
		} break;
		case 10: { // cmpli BF,L,RA,UI
			int  bf = I[6..8];
			bool l  = I[10]; // long?
			int  ra = I[11..15];
			uint  ui = I[16..31];
			
			if (do_debug_ins) writefln("%08X: cmpli cr%d, %%r%d, 0x%04X", addr, bf, ra, ui);
			DO_COMP_U(bf, l ? R[ra] : cast(uint)R[ra], ui);
		} break;
		case 32: {
			uint rt = I[6..10];
			uint ra = I[11..15];
			int  d  = IS[16..31];
			if (do_debug_ins) writefln("%08X: lwz %%r%d, %d(%%r%d)", addr, rt, d, ra);
			R[rt] = memory.read4r(R[ra] + d);
		} break; // lwz RT,D(RA)
		case 34: { // lbz RT,D(RA) || Load Byte and Zero D-form
			uint rt = I[6..10];
			uint ra = I[11..15];
			int  d  = IS[16..31];
			if (do_debug_ins) writefln("%08X: lbz %%r%d, %d(%%r%d)", addr, rt, d, ra);
			R[rt] = memory.read1(R[ra] + d);
		} break;
		case 24: { // ori RA,RS,UI || OR Immediate D-form
			int  rs = I[6..10];
			int  ra = I[11..15];
			int  ui = I[16..31];
			if (do_debug_ins) writefln("%08X: ori %%r%d, %%r%d, %04X", addr, ra, rs, ui);
			R[ra] = R[rs] | ui;
		} break;
		default:
			writefln("%08X: !!!!op_primary(%d)", addr, op_primary);
			assert (op_primary != 0);
		break;
	}
	PC += 4;
}

void execute()
{
	//do_debug_ins = 1;

	//for (int n = 0; n < 1000000; n++) {
	while (true) {
		if (PC == 0x820C5344) {
			//do_debug_ins = 1;
			//return;
		}
		decode(PC, memory.read4r(PC));
		//writefln("%08X", PC);
	}
}

void main() {
	memory = new Memory();
	// 
	memory.add(0x82000000, cast(ubyte[])std.file.read("default.exe"));
	//memory.add(0x820D51C0, cast(ubyte[])std.file.read("comp1_820D51C0"));
	//memory.add(0x823AAF6C, cast(ubyte[])x"FBA1FFE0FBC1FFE8FBE1FFF09181FFF84E800020");
	PC = 0x820D51C0;
	R[6] = 0x800F0000;
	R[1] = 0x86000000;
	try {
		execute();
	} catch (Exception e) {
		writefln("%s", e.toString);
	}
	for (int n = 0; n < 32; n++) {
		writefln("%02d: %08X", n, cast(uint)R[n]);
	}
	write("temp.out", memory[0x800F0000..0x800F0000 + 0x4000]);
	//memory.data[]

	//while (1) { }
	/*
	bits i = {0b_11100000_00000000_00000000_00000000};
	writefln(i[0..1]);
	*/
}