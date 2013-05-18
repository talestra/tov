#include <idc.idc>

static IsPointer(value)
{
	return value >= 0x82000000 && value < 0x83000000;
}

static DetectShiftJisString(n)
{
	auto m;
	for (m = 0; m < 1024; m++) {
		auto b = Byte(n + m);
		//print(n + m);
		if (b == 0) return m;
		if (b < 32) return -1;
		if (b == 0x80) return -1;
		if (b == 0xA0) return -1;
		if (b >= 0xF0 && b <= 0xFF) return -1;
		if ((b >= 0x81 && b <= 0x9F) || (b >= 0xE0 && b <= 0xEF)) {
			auto b2 = Byte(n + m + 1);
			if ((b2 >= 0x9F) && (b2 <= 0xFC)) {
				// Ok
			} else if ((b2 >= 0x40) && (b2 <= 0x9E) && b2 != 0x7F) {
				// Ok
			} else {
				return -1;
			}
			/*
			// Even
			if ((b % 2) == 0) {
				if ((b2 >= 0x9F) && (b2 <= 0xFC)) {
					// Ok
				} else {
					// Invalid
					return -1;
				}
			}
			// Odd
			else {
				if ((b2 >= 0x40) && (b2 <= 0x9E) && b2 != 0x7F) {
					// Ok
				} else {
					// Invalid
					return -1;
				}
			}
			*/
			
			m = m + 1;
		}
	}
	return -1;
}

static NextAligned(value, alignment) {
	while ((value % alignment) != 0) {
		value = value + 1;
	}
	return value;
}

static TryMakeString(n) {
	auto len = DetectShiftJisString(n);
	if (len >= 3) {
		auto alignedLen = NextAligned(len, 4);
		
		MakeUnknown(n, len, DOUNK_SIMPLE);
		MakeStr(n, n + len);
		
		return n + alignedLen;
	}
	return n + 4;
}

static ProcessAddress(n)
{
	
	if (IsPointer(Dword(n)) || (Dword(n) >= 0x40000000 && Dword(n) < 0x40100000)) {
		MakeUnknown(n, 4, DOUNK_SIMPLE);
		MakeDword(n);
		return n + 4;
	} else if (Byte(n + 0) == 0 && Byte(n + 1) == 0) {
		MakeUnknown(n, 4, DOUNK_SIMPLE);
		MakeDword(n);
		return n + 4;
	} else {
		return TryMakeString(n);
	}
}

static main()
{
	auto n;
	for (n = 0x82000600; n < 0x82089468; ) n = ProcessAddress(n);
	for (n = 0x82089600; n < 0x820A0088; ) n = ProcessAddress(n);
	
	//ProcessAddress(0x8201A1C0);
	
	//ProcessAddress(0x8201A1F0);
	//print(DetectShiftJisString(0x82009738));
}

/*
.rdata   82000600 82089468 R . . . L para 00000001 public DATA 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.pdata   82089600 820A0088 R . . . L para 00000002 public DATA 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.text    820B0000 824D0B44 R . X . L para 00000003 public CODE 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.embsec_ 824D0C00 824E8A64 R . X . L para 00000004 public CODE 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.embsec_ 824E8C00 82506AEC R . X . L para 00000005 public CODE 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.embsec_ 82506C00 825086B8 R . X . L para 00000006 public CODE 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.embsec_ 82508800 8250F45C R . X . L para 00000007 public CODE 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.embsec_ 8250F600 82525EA4 R . X . L para 00000008 public CODE 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.embsec_ 82526000 82527278 R . X . L para 00000009 public CODE 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.embsec_ 82527400 82527648 R . X . L para 0000000A public CODE 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.embsec_ 82527800 8252A4C4 R . X . L para 0000000B public CODE 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.data    82530000 82EAE4B8 R W . . L para 0000000C public DATA 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.tls     82EAE600 82EAE621 R W . . L para 0000000D public DATA 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000
.XBMOVIE 82EAE800 82EAE80C R W . . L para 0000000E public DATA 32 00000000 00000000 00000000 00000000 00000000 00000000 00000000

*/