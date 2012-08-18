import std.string, std.stream, std.file, std.stdio, std.intrinsic, std.c.string, std.path;

/*void process_tss(char[] filename) {
	auto data = cast(ubyte[])read(filename);
	assert (cast(char[])data[0..4] == "TSS\0");
	writefln("Processing...%s", filename);

	uint* datai = cast(uint *)data.ptr;
	uint from  = bswap(datai[1]) / 4;
	uint to    = bswap(datai[2]) / 4;
	uint textp = bswap(datai[3]);

	assert (to <= data.length / 4);
	assert (from >= 0);
	assert (from <= to);
	assert (textp < data.length);

	for (int n = from; n < to; n++) {
		uint cval = bswap(datai[n]);
		if (cval == 0x08000000) {
			printf("----------------------------------------------------------------------\n");
		}
		if (cval == 0xFFFFFFFF) {
			uint id = bswap(datai[n - 12]);
			uint p1 = bswap(datai[n - 8]);
			uint p2 = bswap(datai[n - 4]);

			writefln(p1);
			writefln(p2);

			assert(textp + p1 < data.length);
			assert(textp + p2 < data.length);
			
			char *str1 = cast(char *)(data.ptr + textp + p1);
			char *str2 = cast(char *)(data.ptr + textp + p2);

			if (str2[0] != 0) printf("*%08X: %s\n", id, str2);
			//writefln("texy (%08X: %d, %d)", id, p1, p2);
		}
	}
}*/

void process_tss(char[] filename) {
	auto data = cast(ubyte[])read(filename);
	assert (cast(char[])data[0..4] == "TSS\0");
	writefln("Processing...%s", filename);

	uint* datai = cast(uint *)data.ptr;
	uint from  = bswap(datai[1]) / 4;
	uint to    = bswap(datai[2]) / 4;
	uint textp = bswap(datai[3]);

	assert (to <= data.length / 4);
	assert (from >= 0);
	assert (from <= to);
	assert (textp < data.length);

	for (int n = from; n < to; n++) {
		uint cval = bswap(datai[n]);
		if (cval == 0x08000000) {
			printf("----------------------------------------------------------------------\n");
		}
		if (cval == 0x02820000) {
		//if (cval == 0x02070000) {
		//if (cval == 0x0E000008) {
			uint p1 = bswap(datai[n + 1]);

			//writefln(p1);

			assert(textp + p1 < data.length);
			
			char *str1 = cast(char *)(data.ptr + textp + p1);

			if (str1[0] != 0) printf("*%08X: %s\n", 0, str1);
			//writefln("texy (%08X: %d, %d)", id, p1, p2);
		}
	}
}

void main() {
	//process_tss("c:/isos/360/string_dic_uk.so");
	foreach (name; listdir("scenario.uk")) {
		char[] file = std.string.format("scenario.uk/%s", name);
		process_tss(file);
		//writefln("script/%s", name);
	}
}