/*
cls && dmd si.d -run dxt.d
*/
import std.stdio, std.string, std.file, std.stream, std.bitmanip, std.process, std.intrinsic, std.zlib;

import si;

ColorFormat RGB_565 = { {0, 5, 11, 0}, {5, 6, 5, 0} };

align(1) struct ushort_be {
	ushort be_value;

	static ushort bswap16(ushort v) { return ((v >> 8) & 0xFF) | ((v << 8) & 0xFF00); }
	ushort be() { return be_value; }
	ushort le() { return bswap16(be_value); }

	ushort native() { version (BigEndian) { return be; } else { return le; } }
	ushort native(ushort value) { version (BigEndian) {  be_value = value; } else { be_value = bswap16(value); } return value; }

	ubyte low () { return (native >> 0) & 0xFF; }
	ubyte high() { return (native >> 8) & 0xFF; }
	
	alias native value;
	
	static assert(this.sizeof == 2);
}

align(1) struct DXT5 { // 4x4 pixels
	ushort_be alpha;
	ushort_be alpha_data[3];
	ushort_be colors[2];
	ushort_be color_data[2];
	
	static assert(this.sizeof == 0x10);
	
	void encode(in  RGBA[16] input) {
		/*
		{
			ubyte min_alpha = 0xFF, max_alpha = 0x00;
			foreach (ref color; input) {
				min_alpha = min(min_alpha, color.a);
				max_alpha = max(max_alpha, color.a);
			}
		}
		*/
		assert(0, "Not implemented");
	}
	
	void encodeSimpleUnoptimizedWhiteAlpha(in RGBA[16] input) {
		alpha.value = 0x00FF;
		colors[1].value = colors[0].value = cast(ushort)RGBA(0xFF, 0xFF, 0xFF, 0xFF).decode(RGB_565);
		// 255 0 218 182 145 109 72 36
		ubyte[8] lookup = [1, 7, 6, 5, 4, 3, 2, 0];
		ubyte[16] alpha_data_transfer;
		foreach (k, color; input) {
			int e = ((color.a * 7) / 255);
			alpha_data_transfer[k] = lookup[e];
		}
		transferAlphaData(alpha_data_transfer, false);
	}
	
	static void EXT_INS(T, T2)(ref T container, ref T2 value, bool extract, uint offset, uint len, uint offset_value = 0) {
		auto mask = cast(T)((1 << len) - 1);
		if (extract) {
			value &= ~(mask << offset_value);
			value |= cast(T2)(((container >> offset) & mask) << offset_value);
		} else {
			container = cast(T)((container & ~(mask << offset)) | (((value >> offset_value) & mask) << offset));
		}
	}

	void transferColorData(ref ubyte[16] color_data_transfer, bool extract) {
		foreach (m; 0..2) {
			for (int n = 0; n < 16; n += 2) {
				ushort value = color_data[m].value;
				EXT_INS(value, color_data_transfer[n], extract, n, 2);
				if (!extract) color_data[m].value = value;
			}
		}
	}
	
	void transferAlphaData(ref ubyte[16] alpha_data_transfer, bool extract) {
		void exs_ins2(uint m, uint n, uint offset, uint len, uint offset_value = 0) {
			ushort value = alpha_data[m].value;
			EXT_INS(value, alpha_data_transfer[n], extract, offset, len, offset_value);
			if (!extract) alpha_data[m].value = value;
		}
	
		exs_ins2(0, 0 , 0 , 3, 0);
		exs_ins2(0, 1 , 3 , 3, 0);
		exs_ins2(0, 2 , 6 , 3, 0);
		exs_ins2(0, 3 , 9 , 3, 0);
		exs_ins2(0, 4 , 12, 3, 0);
		exs_ins2(0, 5 , 15, 1, 0); exs_ins2(1, 5, 0, 2, 1);
		
		exs_ins2(1, 6 , 2 , 3, 0);
		exs_ins2(1, 7 , 5 , 3, 0);
		exs_ins2(1, 8 , 8 , 3, 0);
		exs_ins2(1, 9 , 11, 3, 0);
		exs_ins2(1, 10, 14, 2, 0); exs_ins2(2, 10, 0, 1, 2);

		exs_ins2(2, 11, 1 , 3, 0);
		exs_ins2(2, 12, 4 , 3, 0);
		exs_ins2(2, 13, 7 , 3, 0);
		exs_ins2(2, 14, 10, 3, 0);
		exs_ins2(2, 15, 13, 3, 0);
	}
	
	void decode(out RGBA[16] output) {
		ubyte alpha_table[8] = void;
		RGBA  color_table[4] = void;
		
		// Color table.
		color_table[0] = RGBA(RGB_565, colors[0].value);
		color_table[1] = RGBA(RGB_565, colors[1].value);
		if (colors[0].value > colors[1].value) {
			color_table[2] = RGBA.mix(color_table[0], color_table[1], 3, 2, 1);
			color_table[3] = RGBA.mix(color_table[0], color_table[1], 3, 1, 2);
		} else {
			color_table[2] = RGBA.mix(color_table[0], color_table[1], 2, 1, 1);
			color_table[3] = RGBA(0, 0, 0, 0);
		}

		ubyte[16] color_data_transfer;
		transferColorData(color_data_transfer, true);
		foreach(n, cc; color_data_transfer) output[n] = color_table[cc];

		// Alpha table.
		ubyte alpha_0 = alpha.low;
		ubyte alpha_1 = alpha.high;

		if (alpha_0 > alpha_1) {
			alpha_table[0] = alpha_0;
			alpha_table[1] = alpha_1;
			for (int n = 0; n < 6; n++) alpha_table[n + 2] = cast(ubyte)(((6 - n) * alpha_0 + (n + 1) * alpha_1) / 7);
		} else {
			alpha_table[0] = alpha_0;
			alpha_table[1] = alpha_1;
			for (int n = 0; n < 4; n++) alpha_table[n + 2] = cast(ubyte)(((4 - n) * alpha_0 + (n + 1) * alpha_1) / 5);
			alpha_table[6] = 0;
			alpha_table[7] = 255;
		}
		
		//writefln("%d", alpha_table);

		ubyte[16] alpha_data_transfer;
		transferAlphaData(alpha_data_transfer, true);
		foreach(n, cc; alpha_data_transfer) output[n].a = alpha_table[cc];
	}

	static assert (this.sizeof == 0x10);
}

alias uint UINT;

UINT XGLog2LE16(UINT TexelPitch) {
    return (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
}

UINT XGAddress2DTiledX(
    UINT Offset,        // Tiled memory offset in texels/blocks
    UINT Width,         // Width of the image in texels/blocks
    UINT TexelPitch     // Size of an image texel/block in bytes
) {
    UINT AlignedWidth;
    UINT LogBpp;
    UINT OffsetB;
    UINT OffsetT;
    UINT OffsetM;
    UINT Tile;
    UINT Macro;
    UINT Micro;
    UINT MacroX;

    AlignedWidth = (Width + 31) & ~31;

    LogBpp       = XGLog2LE16(TexelPitch);
    OffsetB      = Offset << LogBpp;
    OffsetT      = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
    OffsetM      = OffsetT >> (7 + LogBpp);

    MacroX       = ((OffsetM % (AlignedWidth >> 5)) << 2);
    Tile         = ((((OffsetT >> (5 + LogBpp)) & 2) + (OffsetB >> 6)) & 3);
    Macro        = (MacroX + Tile) << 3;
    Micro        = ((((OffsetT >> 1) & ~15) + (OffsetT & 15)) & ((TexelPitch << 3) - 1)) >> LogBpp;

    return Macro + Micro;
}

UINT XGAddress2DTiledY(
    UINT Offset,        // Tiled memory offset in texels/blocks
    UINT Width,         // Width of the image in texels/blocks
    UINT TexelPitch     // Size of an image texel/block in bytes
    )
{
    UINT AlignedWidth;
    UINT LogBpp;
    UINT OffsetB;
    UINT OffsetT;
    UINT OffsetM;
    UINT Tile;
    UINT Macro;
    UINT Micro;
    UINT MacroY;

    AlignedWidth = (Width + 31) & ~31;

    LogBpp       = XGLog2LE16(TexelPitch);
    OffsetB      = Offset << LogBpp;
    OffsetT      = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
    OffsetM      = OffsetT >> (7 + LogBpp);

    MacroY       = ((OffsetM / (AlignedWidth >> 5)) << 2);
    Tile         = ((OffsetT >> (6 + LogBpp)) & 1) + (((OffsetB & 2048) >> 10));
    Macro        = (MacroY + Tile) << 3;
    Micro        = ((((OffsetT & (((TexelPitch << 6) - 1) & ~31)) + ((OffsetT & 15) << 1)) >> (3 + LogBpp)) & ~1);

    return Macro + Micro + ((OffsetT & 16) >> 4);
}

void extract_new() {
	RGBA[16] block;
	foreach (file; listdir("UI", "UI\\FONT*.TXV.new")) {
		writefln("%s", file);
		auto data = std.file.read(file);
		auto dxt5array = (cast(DXT5 *)(data.ptr))[0..data.length / DXT5.sizeof];
		auto dxt5array_out = new DXT5[512 * 512];
		
		auto i  = new Bitmap32(2048, 2048);
		auto b  = new Bitmap32(4, 4);
		b.data = block;
		try {
			foreach (dxt5_n, dxt5; dxt5array) {
				int rx = XGAddress2DTiledX(dxt5_n, i.width / 4, 16);
				int ry = XGAddress2DTiledY(dxt5_n, i.width / 4, 16);

				dxt5.decode(block);
				dxt5array_out[dxt5_n].encodeSimpleUnoptimizedWhiteAlpha(block);
				
				i.blit(b, rx * 4, ry * 4);
			}
		} catch (Exception e) {
		}
		//i.savePNG(file ~ ".png", 1);
	}
}

/*
fps4 -l l:\games\vesperia\UI.svo fonttex10*
FPS4('../Release/ïñÆ╩/UI.svo')

0144 - FPS4.FileEntry(0x036C9000, 0x000000BC(0x00000800), 'FONTTEX10.TXM')
0145 - FPS4.FileEntry(0x036C9800, 0x00400000(0x00400000), 'FONTTEX10.TXV')

Items: 2/3268 using pattern 'fonttex10*'
Size : 4.00 MB / 504.22 MB
*/

void extract_original() {
	RGBA[16] block;
	
	void write_file(ubyte[] data, string file) {
		writefln("%s", file);
		//auto data = std.file.read(file);
		auto dxt5array = (cast(DXT5 *)(data.ptr))[0..data.length / DXT5.sizeof];
		auto dxt5array_out = new DXT5[512 * 512];
		
		auto i  = new Bitmap32(2048, 2048);
		auto b  = new Bitmap32(4, 4); b.data = block;
		try {
			foreach (dxt5_n, dxt5; dxt5array) {
				//writefln("[1]");
				int rx = XGAddress2DTiledX(dxt5_n, i.width / 4, 16);
				int ry = XGAddress2DTiledY(dxt5_n, i.width / 4, 16);

				dxt5.decode(block);
				dxt5array_out[dxt5_n].encodeSimpleUnoptimizedWhiteAlpha(block);
				
				//writefln("%d, %d (%d, %d)", rx, ry, b.width, b.height);
				b.blit(i, rx * 4, ry * 4);
			}
		} catch (Exception e) {
		}
		i.write("UI/" ~ file ~ ".TXV.ori.png");
		std.file.write("UI/" ~ file ~ ".TXV.ori.TXV", cast(void[])dxt5array_out);
	}
	
	scope f = new std.stream.File("c:/tov/UI.svo.bak", FileMode.In);
	f.position = 0x036C9800;
	scope data = new ubyte[2048 * 2048];
	f.read(data);
	write_file(data, "FONTTEX05");
	
	/*
	foreach (file; listdir(r"..\..\UI", r"..\..\UI\FONT*.TXV")) {
		write_file(std.file.read(file), file);
	}
	*/
}

void reinsert_mod() {
	writefln("UI/FONTTEX05.TXV.mod.TXV...");
	string new_png_file = "UI/FONTTEX10.TXV.mod.png";
	string new_txv_file = "UI/FONTTEX05.TXV.mod.TXV";
	{
		RGBA[16] block;
		scope i = Image.read(new_png_file);
		scope f = new std.stream.File(new_txv_file, FileMode.OutNew);
		DXT5 dxt5;
		foreach (dxt5_n; 0..512* 512) {
			int rx = XGAddress2DTiledX(dxt5_n, i.width / 4, 16);
			int ry = XGAddress2DTiledY(dxt5_n, i.width / 4, 16);
			i.readBlock(block, rx * 4, ry * 4, 4, 4);
			dxt5.encodeSimpleUnoptimizedWhiteAlpha(block);
			f.write(TA(dxt5));
		}
		f.close();
	}
	{
		writefln("Updating UI.svo...");
		scope f = new std.stream.File("c:/tov/UI.svo", FileMode.In | FileMode.Out);
		f.position = 0x036C9800;
		f.writeString(cast(string)std.file.read(new_txv_file));
		f.flush();
		f.close();
	}
}

void main() {
	extract_original();
	reinsert_mod();
	//auto i = Image.read("UI/FONTDUMMY.TXV.png");
	//auto i = Image.read("UI/FONTTEX10.mod.png");
	//i.write("mytest.png");
	//extract_original();
	//extract_new();
	//auto i = Image.loadPNG(new BufferedFile("UI/FONTTEX10.mod.png", FileMode.In));
	//auto i = Image.loadPNG(new BufferedFile("UI/FONTDUMMY.TXV.png", FileMode.In));
	//i.savePNG("mytest.png");
	
	// UI/FONTTEX10.mod.png
}