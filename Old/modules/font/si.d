module si;

public import std.stream;
public import std.stdio;
public import std.intrinsic;
public import std.path;
public import std.math;
public import std.file;
public import std.process;
public import std.string;
public import std.system;
public import std.zlib;


int hex(char c) {
	if (c >= '0' && c <= '9') return (c - '0') + 0;
	if (c >= 'a' && c <= 'f') return (c - 'a') + 10;
	if (c >= 'A' && c <= 'F') return (c - 'A') + 10;
	return -1;
}
int hex(string s) {
	int r;
	foreach (c; s) {
		int v = hex(c);
		if (v >= 0) {
			r *= 0x10;
			r += v;
		}
	}
	return r;
}

int imin(int a, int b) { return (a < b) ? a : b; }
int imax(int a, int b) { return (a > b) ? a : b; }
int iabs(int a) { return (a < 0) ? -a : a; }

void cendian(ref ushort v, Endian endian) { if (endian != std.system.endian) v = (bswap(v) >> 16); }
void cendian(ref uint   v, Endian endian) { if (endian != std.system.endian) v = bswap(v); }

template TA(T) { ubyte[] TA(ref T t) { return cast(ubyte[])(&t)[0..1]; } }

template swap(T) { void swap(ref T t1, ref T t2) { T t = t1; t1 = t2; t2 = t;} }

class Bit {
	final static ulong MASK(ubyte size) {
		return ((1 << size) - 1);
	}
	
	final static ulong INS(ulong v, ubyte pos, ubyte size, int iv) {
		ulong mask = MASK(size);
		return (v & ~(mask << pos)) | ((iv & mask) << pos);
	}
	
	final static ulong EXT(ulong v, ubyte pos, ubyte size) {
		return (v >> pos) & MASK(size);
	}
	
	static long div_mult_ceil (long v, long mult, long div) { return cast(long)std.math.ceil (cast(real)(v * mult) / cast(real)div); }
	static long div_mult_round(long v, long mult, long div) { return cast(long)std.math.round(cast(real)(v * mult) / cast(real)div); }
	static long div_mult_floor(long v, long mult, long div) { return (v * mult) / div; }
	alias div_mult_floor div_mult;

	//////////////////////////

	final static ulong INS2(ulong v, ubyte pos, ubyte size, int iv, int base) {
		ulong mask = MASK(size);

		/*
		writefln("%d", iv);
		writefln("%d", mask);
		writefln("%d", base);
		writefln("--------");
		*/
	
		return INS(v, pos, size, cast(int)div_mult_ceil(iv, mask, base));
	}

	final static ulong EXT2(ulong v, ubyte pos, ubyte size, int base) {
		ulong mask = MASK(size);
		if (mask == 0) return 0;
		
		/*
		writefln("%d", EXT(v, pos, size));
		writefln("%d", base);
		writefln("%d", mask);
		writefln("--------");
		*/
		
		return div_mult_ceil(EXT(v, pos, size), base, mask);
	}
}

class ImageFileFormatProvider {
	static ImageFileFormat[string] list;

	static void registerFormat(ImageFileFormat iff) {
		list[iff.identifier] = iff;
	}

	static ImageFileFormat find(Stream s, int check_size = 1024) {
		auto ss = new SliceStream(s, 0);
		auto data = new ubyte[check_size];
		auto cs = new MemoryStream(data[0..ss.read(data)]);

		ImageFileFormat cff;
		int certain = 0;
		foreach (iff; list.values) {
			cs.position = 0;
			int c_certain = iff.check(cs);
			if (c_certain > certain) {
				cff = iff;
				certain = c_certain;
				if (certain >= 10) break;
			}
		}
		if (certain == 0) throw(new Exception("Unrecognized ImageFileFormat"));
		return cff;
	}

	static Image read(Stream s) { return find(s).read(s); }
	
	static Image read(string name) {
		Stream s = new BufferedFile(name);
		Image i = read(s);
		s.close();
		return i;
	}

	static ImageFileFormat opIndex(string idx) {
		if ((idx in list) is null) throw(new Exception(std.string.format("Unknown ImageFileFormat '%s'", idx)));
		return list[idx];
	}
}

// Abstract ImageFileFormat
abstract class ImageFileFormat {
	private this() { }
	
	bool update(Image i, Stream s) { throw(new Exception("Updating not implemented")); return false; }
	bool update(Image i, string name) { Stream s = new std.stream.File(name, FileMode.OutNew); bool r = update(i, s); s.close(); return r; }
	
	bool write(Image i, Stream s) { throw(new Exception("Writing not implemented")); return false; }
	bool write(Image i, string name) { Stream s = new std.stream.File(name, FileMode.OutNew); bool r = write(i, s); s.close(); return r; }
	
	Image read(Stream s) { throw(new Exception("Reading not implemented")); return null; }
	Image[] readMultiple(Stream s) { throw(new Exception("Multiple reading not implemented")); return null; }

	string identifier() { return "null"; }
	
	// 0 - impossible (discard)
	//
	// ... different levels of probability (uses the most probable)
	//
	// 10 - for sure (use this)
	int check(Stream s) { return 0; }
}

align(1) struct ColorFormat {
	align(1) struct Set {
		union {
			struct { ubyte r, g, b, a; }
			ubyte[4] vv;
		}
	}
	Set pos, len;
}

ColorFormat RGBA_8888 = { {0, 8, 16, 24}, {8, 8, 8, 8} };
ColorFormat RGBA_5551 = { {0, 5, 10, 15}, {5, 5, 5, 1} };
ColorFormat RGBA_5650 = { {0, 5, 11, 26}, {5, 6, 5, 0} };

align(1) struct RGBAf {
	union {
		struct { float r, g, b, a; }
		float[4] vv;
	}
	
	static ubyte clamp(float v) {
		float r = v * 0xFF;
		if (r > 0xFF) r = 0xFF;
		if (r < 0x00) r = 0x00;
		return cast(ubyte)r;
	}
	
	RGBA rgba() { return RGBA(clamp(r), clamp(g), clamp(b), clamp(a)); }
	
	static RGBAf opCall(float r, float g, float b, float a) {
		RGBAf c = {r, g, b, a};
		return c;
	}
	
	static RGBAf opCall(RGBA c) {
		RGBAf cf = void;
		for (int n = 0; n < 4; n++) cf.vv[n] = cast(float)c.vv[n] / 0xFF;
		return cf;
	}
	
	RGBAf opMul(float m) {
		//return RGBAf(r * m, g * m, b * m, a * m);
		return RGBAf(r * m, g * m, b * m, a);
	}
	
	RGBAf opAdd(RGBAf c) {
		RGBAf r = void;
		for (int n = 0; n < 4; n++) r.vv[n] = vv[n] + c.vv[n];
		return r;
	}
	
	static RGBAf over(RGBAf c1, RGBAf c2) {
		return c1 * c1.a + c2 * (c2.a * (1 - c1.a));
	}
	
	string toString() {
		return std.string.format("RGBA(%02X,%02X,%02X,%02X)", r, g, b, a);
	}	
}

// TrueColor pixel
align(1) struct RGBA {
	union {
		struct { ubyte r; ubyte g; ubyte b; ubyte a; }
		struct { byte _r; byte _g; byte _b; byte _a; }
		ubyte[4] vv;
		uint v;
		alias r R;
		alias g G;
		alias b B;
		alias a A;
	}
	
	ulong decode(ColorFormat format) {
		ulong rr;
		for (int n = 0; n < 4; n++) rr = Bit.INS2(rr, format.pos.vv[n], format.len.vv[n], vv[n], 0xFF);
		return rr;
	}
	
	static RGBA opCall(ColorFormat format, ulong data) {
		RGBA c = void;
		for (int n = 0; n < 4; n++) c.vv[n] = cast(ubyte)Bit.EXT2(data, format.pos.vv[n], format.len.vv[n], 0xFF);
		return c;
	}
	
	static RGBA opCall(ubyte r, ubyte g, ubyte b, ubyte a = 0xFF) {
		RGBA c = {r, g, b, a};
		return c;
	}

	static RGBA opCall(int r, int g, int b, int a = 0xFF) {
		RGBA c = {cast(ubyte)r, cast(ubyte)g, cast(ubyte)b, cast(ubyte)a};
		return c;
	}

	static RGBA opCall(uint v) {
		RGBA c = void;
		c.v = v;
		return c;
	}
	
	static RGBA opCall(string s) {
		RGBA c;
		for (int n = 0; n < s.length; n += 2) c.vv[n / 2] = cast(ubyte)hex(s[n..n + 2]);
		return c;
	}
	
	static RGBA toBGRA(RGBA c) {
		ubyte r = c.r;
		c.r = c.b;
		c.b = r;
		return c;
	}
	
	static int dist(RGBA a, RGBA b) {
		alias std.math.abs abs;
		return (
			abs(a._r - b._r) +
			abs(a._g - b._g) +
			abs(a._b - b._b) +
			abs(a._a - b._a) +
		0);
	}
	
	static RGBA mix(RGBA a, RGBA b, int div, int a_c, int b_c) {
		RGBA c = void;
		assert(a_c + b_c == div, format("%d+%d != %d", a_c, b_c, div));
		for (int n = 0; n < 4; n++) {
			c.vv[n] = cast(ubyte)(((a.vv[n] * a_c) + (b.vv[n] * b_c)) / div);
		}
		return c;
	}
	
	string toString() {
		return std.string.format("RGBA(%02X,%02X,%02X,%02X)", r, g, b, a);
	}
}

static assert (RGBA.sizeof == 4);

// Abstract Image
abstract class Image {
	string id;
	Image[] childs;

	// Info
	ubyte bpp();
	int width();
	int height();

	// Data
	void set(int x, int y, uint v);
	uint get(int x, int y);
	ubyte[] _data() { return null; }

	void set32(int x, int y, RGBA c) {
		if (bpp == 32) { return set(x, y, c.v); }
		throw(new Exception("Not implemented (set32)"));
	}
	
	void readBlock(RGBA[] colors, int bx, int by, int bw, int bh) {
		int n = 0;
		foreach (y; 0..bh) {
			foreach (x; 0..bw) {
				colors[n++] = get32(bx + x, by + y);
			}
		}
	}

	RGBA get32(int x, int y) {
		if (bpp == 32) {
			RGBA c; c.v = get(x, y);
			return c;
		}
		throw(new Exception("Not implemented (get32)"));
	}
	
	Image filter(RGBA delegate(int, int, RGBA) func, bool duplicate = false) {
		if (duplicate) { return this.duplicate.filter(func, false); }
		int w = width, h = height;
		for (int y = 0; y < h; y++) {
			for (int x = 0; x < w; x++) {
				set32(x, y, func(x, y, get32(x, y)));
			}
		}
		return this;
	}
	
	Image duplicate() { assert(0 != 1, "duplicate not implemented"); return null; }

	RGBA getColor(int x, int y) {
		RGBA c;
		c.v = hasPalette ? color(get(x, y)).v : get(x, y);
		return c;
	}

	// Palette
	bool hasPalette() { return (bpp <= 8); }
	int ncolor() { return 0; }
	int ncolor(int n) { return ncolor; }
	RGBA color(int idx) { RGBA c; return c; }
	RGBA color(int idx, RGBA c) { return color(idx); }
	RGBA[] colors() {
		RGBA[] cc;
		for (int n = 0; n < ncolor; n++) cc ~= color(n);
		return cc;
	}

	static uint colorDist(RGBA c1, RGBA c2) {
		return (
			(
				iabs(c1.r * c1.a - c2.r * c2.a) +
				iabs(c1.g * c1.a - c2.g * c2.a) +
				iabs(c1.b * c1.a - c2.b * c2.a) +
				iabs(c1.a * c1.a - c2.a * c2.a) +
			0)
		);
	}

	RGBA[] createPalette(int count = 0x100) { throw(new Exception("Not implemented: createPalette")); }

	uint matchColor(RGBA c) {
		uint mdist = 0xFFFFFFFF;
		uint idx;
		for (int n = 0; n < ncolor; n++) {
			uint cdist = colorDist(color(n), c);
			if (cdist < mdist) {
				mdist = cdist;
				idx = n;
			}
		}
		return idx;
	}
	
	void blit(Image i, int px = 0, int py = 0, float alpha = 1.0) {
		int w = width, h = height;
		for (int y = 0; y < h; y++) {
			for (int x = 0; x < w; x++) {
				RGBAf c = RGBAf(get32(x, y));
				RGBAf c2 = RGBAf(i.get32(px + x, py + y));
				c.a *= alpha;
				c = RGBAf.over(c, c2);
				//writefln("%f, %f, %f, %f", c.r, c.g, c.b, c.a);
				i.set32(px + x, py + y, c.rgba);
			}
		}
	}

	void copyFrom(Image i, bool convertPalette = false) {
		int mw = imin(width, i.width);
		int mh = imin(height, i.height);

		//if (bpp != i.bpp) throw(new Exception(std.string.format("BPP mismatch copying image (%d != %d)", bpp, i.bpp)));

		if (i.hasPalette) {
			ncolor = i.ncolor;
			for (int n = 0; n < ncolor; n++) color(n, i.color(n));
		}

		/*if (hasPalette && !i.hasPalette) {
			i = toColorIndex(i);
		}*/

		if (convertPalette && hasPalette && !i.hasPalette) {
			foreach (idx, c; i.createPalette(ncolor)) color(idx, c);
		}

		if (hasPalette && i.hasPalette) {
			for (int y = 0; y < i.height; y++) for (int x = 0; x < i.width; x++) set(x, y, get(x, y));
		} else if (hasPalette) {
			for (int y = 0; y < i.height; y++) for (int x = 0; x < i.width; x++) set(x, y, matchColor(i.get32(x, y)));
		} else {
			for (int y = 0; y < i.height; y++) for (int x = 0; x < i.width; x++) set32(x, y, i.get32(x, y));
		}
	}
	
	static Image composite(Image color, Image alpha) {
		Image r = new Bitmap32(color.width, color.height);
		for (int y = 0; y < color.height; y++) {
			for (int x = 0; x < color.width; x++) {
				RGBA c = color.get32(x, y);
				RGBA a = alpha.get32(x, y);
				c.a = a.r;
				r.set32(x, y, c);
			}
		}
		return r;
	}
	
	void setChroma(RGBA c) {
		if (hasPalette) {
			foreach (idx, cc; colors) {
				if (cc == c) color(idx, RGBA(0, 0, 0, 0));
			}
		} else {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					if (get32(x, y) == c) set32(x, y, RGBA(0, 0, 0, 0));
				}
			}
		}
	}
	
	void write(string file, string format = null) {
		if (format is null) format = getExt(file);
		ImageFileFormatProvider[format].write(this, file);
	}
	void write(Stream file, string format) { ImageFileFormatProvider[format].write(this, file); }
	
	alias ImageFileFormatProvider.read read;
	
	bool check_bounds(int x, int y) { return !(x < 0 || y < 0 || x >= width || y >= height); }
	
	Image channel(int idx) {
		throw(new Exception("Channel getter not implemented"));
	}
}

// TrueColor Bitmap
class Bitmap32 : Image {
	RGBA[] data;
	int _width, _height;
	bool using_chroma = false;
	RGBA chroma;
	
	ubyte[] _data() { return cast(ubyte[])data; }
	
	RGBA *get_pos(int x, int y) {
		if (!check_bounds(x, y)) return null;
		return &data[y * _width + x];
	}

	class Channel : Image {
		int idx;
		
		this(int idx) {
			this.idx = idx;
		}
		
		bool hasPalette() { return true; }
		RGBA color(int n) { return RGBA(n, n, n, 0xFF); }
		
		ubyte bpp() { return 8; }
		int width() { return _width; }
		int height() { return _height; }
		
		void set(int x, int y, uint v) {
			auto p = get_pos(x, y);
			if (p !is null) p.vv[idx] = cast(ubyte)v;
		}

		uint get(int x, int y) {
			auto p = get_pos(x, y);
			if (p !is null) return p.vv[idx];
			return -1;
		}
	}

	Image channel(int idx) {
		return new Channel(idx);
	}

	ubyte bpp() { return 32; }
	int width() { return _width; }
	int height() { return _height; }
	
	override Image duplicate() {
		auto r = new Bitmap32(_width, _height);
		r.chroma = chroma;
		r.data = data.dup;
		return r;
	}
	
	void set(int x, int y, uint v) { if (check_bounds(x, y)) data[y * _width + x].v = v; }
	uint get(int x, int y) {
		if (!check_bounds(x, y)) return 0;
		uint c = data[y * _width + x].v;
		if (using_chroma && chroma.v == c) return RGBA(0, 0, 0, 0).v;
		return c;
	}
	
	alias createPalette1 createPalette;
	
	RGBA[] createPalette1(int count = 0x100) {
		RGBA[] r;
	
		int[RGBA] colors;
		RGBA[][int] colors_pos;
		foreach (c; data) {
			if (c in colors) colors[c]++;
			else colors[c] = 1;
		}
		colors = colors.rehash;
		
		colors_pos = null;
		foreach (c, n; colors) colors_pos[n] ~= c;
		
		int[] lengths = colors_pos.keys.sort.reverse;
		
		foreach (cc_count; lengths) {
			//writefln(cc_count);
			foreach (cc; colors_pos[cc_count]) {
				//writefln(cc);
				r ~= cc;
				if (r.length >= count) break;
			}
			if (r.length >= count) break;
		}
		
		return r;
	}
	
	RGBA[] createPalette2(int count = 0x100) {
		RGBA[] r;
		bool[] fixed;
		long[] scores;
	
		int[RGBA] colors;
		RGBA[][int] colors_pos;
		foreach (c; data) {
			if (c in colors) colors[c]++;
			else colors[c] = 1;
		}
		colors = colors.rehash;
		
		colors_pos = null;
		foreach (c, n; colors) {
			colors_pos[n] ~= c;
		}
		
		if (1 in colors_pos) {
			//if (colors.length - colors_pos[1].length > 256) {
			if (colors.length - colors_pos[1].length > 512) {
				colors_pos[1] = null;
			}
		}
		
		foreach (cc_count; colors_pos.keys.sort.reverse) foreach (cc; colors_pos[cc_count]) { r ~= cc; scores ~= cc_count; fixed ~= false; }
		
		for (int n = 0; n < count; n++) {
			if (fixed[n]) continue;
			uint lower_value = 0xFFFFFFFF, higher_value = 0x00000000;
			int lower_index = -1, higher_index = -1;
		
			for (int m = n + 1; m < count; m++) {
				if (fixed[m]) continue;
				uint c_dist = colorDist(r[n], r[m]);
				if (c_dist <= lower_value) {
					lower_value = c_dist;
					lower_index = m;
				}
			}
			
			for (int m = count; m < r.length; m++) {
				if (fixed[m]) continue;
				uint c_dist = colorDist(r[n], r[m]);
				if (c_dist >= higher_value) {
					higher_value = c_dist;
					higher_index = m;
				}
			}
			
			if (higher_index != -1 && lower_index != -1) {
				swap(r[lower_index], r[higher_index]);
				fixed[lower_index] = true;
			}

			writefln("%d, %d", lower_value, lower_index);
			
			//break;
		}
		
		writefln("%d", r.length);
		
		/*
		for (int n = 0; n < r.length; n++) {
			writefln("%s: %d", r[n], scores[n]);
		}
		*/
		
		return r[0..count];
	}
	
	Bitmap8 paletize(int ncolors = 0x100) {
		int[RGBA] colors;
		
		auto r = new Bitmap8(width, height);
		
		r.palette = createPalette(ncolors);
		
		foreach (c; data) colors[c] = 0;
		foreach (c; colors.keys) colors[c] = r.matchColor(c);
		
		for (int y = 0; y < _height; y++) for (int x = 0; x < _width; x++) r.set(x, y, colors[get32(x, y)]);
		
		return r;
	}
	
	override void setChroma(RGBA c) {
		using_chroma = true;
		chroma = c;
	}

	this(int w, int h) {
		_width = w;
		_height = h;
		data.length = w * h;
	}
	
	static Bitmap32 convert(Image i) {
		auto r = new Bitmap32(i.width, i.height);
		for (int y = 0; y < r._height; y++) for (int x = 0; x < r._width; x++) r.set32(x, y, i.get32(x, y));
		return r;
	}
}

// Palletized Bitmap
class Bitmap8 : Image {
	RGBA[] palette;
	ubyte[] data;
	int _width, _height;
	
	ubyte[] _data() { return cast(ubyte[])data; }

	override ubyte bpp() { return 8; }
	int width() { return _width; }
	int height() { return _height; }

	void set(int x, int y, uint v) { if (check_bounds(x, y)) data[y * _width + x] = cast(ubyte)v; }
	uint get(int x, int y) { return check_bounds(x, y) ? data[y * _width + x] : 0; }
	override RGBA get32(int x, int y) { return palette[get(x, y) % palette.length]; }
	
	override int ncolor() { return palette.length;}
	override int ncolor(int s) { palette.length = s; return s; }
	RGBA color(int idx) { return palette[idx]; }
	RGBA color(int idx, RGBA col) { return palette[idx] = col; }
	void colorSwap(int i1, int i2) {
		if (i1 >= palette.length || i2 >= palette.length) return;
		swap(palette[i1], palette[i2]);
	}
	
	this(int w, int h) {
		_width = w;
		_height = h;
		data.length = w * h;
	}
}


class ImageFileFormat_BMP : ImageFileFormat {
	align(1) struct BITMAPFILEHEADER {
		char[2] bfType;
		uint    bfSize;
		ushort  bfReserved1;
		ushort  bfReserved2;
		uint    bfOffBits;
	}
	
	align(1) struct BITMAPINFOHEADER {
		uint   biSize;
		int    biWidth;
		int    biHeight;
		ushort biPlanes;
		ushort biBitCount;
		uint   biCompression;
		uint   biSizeImage;
		int    biXPelsPerMeter;
		int    biYPelsPerMeter;
		uint   biClrUsed;
		uint   biClrImportant;
	}
	
	struct RGBQUAD {
		ubyte rgbBlue;
		ubyte rgbGreen;
		ubyte rgbRed;
		ubyte rgbReserved;
	}

	override string identifier() { return "bmp"; }
	
	Image read(Stream s) {
		Image i;
		BITMAPFILEHEADER h;
		BITMAPINFOHEADER ih;
		s.read(TA(h));
		s.read(TA(ih));
		
		if (ih.biCompression) throw(new Exception("BMP compression not supported"));
		if (ih.biPlanes > 1) throw(new Exception("Only supported 1 bitplane"));
		
		switch (ih.biBitCount) {
			default: case 4: throw(new Exception(std.string.format("BPP %d not supported", ih.biBitCount)));
			case 8:
				i = new Bitmap8(ih.biWidth, ih.biHeight);
				i.ncolor = ih.biClrUsed ? ih.biClrUsed : (1 << ih.biBitCount);
				for (int n = 0; n < i.ncolor; n++) {
					//RGBQUAD c;
					RGBA c;
					s.read(TA(c));
					c = RGBA.toBGRA(c);
					c.a = 0xFF;
					i.color(n, c);
				}
				ubyte[] data;
				data.length = ih.biWidth * ih.biHeight;
				
				s.position = h.bfOffBits;
				s.read(data);
				
				for (int y = ih.biHeight - 1, n = 0; y >= 0; y--) {
					for (int x = 0; x < ih.biWidth; x++, n++) {
						i.set(x, y, data[n]);
					}
				}
			break;
			case 24:
				i = new Bitmap32(ih.biWidth, ih.biHeight);
				ubyte[] data;
				data.length = ih.biWidth * ih.biHeight * 3;
				s.position = h.bfOffBits;
				s.read(data);
				for (int y = ih.biHeight - 1, n = 0; y >= 0; y--) {
					for (int x = 0; x < ih.biWidth; x++, n += 3) {
						RGBA c = *cast(RGBA *)(data.ptr + n);
						c.a = 0xFF;
						c = RGBA.toBGRA(c);
						i.set32(x, y, c);
					}
				}				
			break;
		}
		
		return i;
	}
	
	override int check(Stream s) {
		BITMAPFILEHEADER h;
		s.read(TA(h));
		return (h.bfType == "BM") ? 10 : 0;
	}
}

static this() {
	ImageFileFormatProvider.registerFormat(new ImageFileFormat_BMP);
}


align(1) struct GIM_IHeader {
	uint _u1;
	ushort type; ushort _u2;
	ushort width, height;
	ushort bpp;
	ushort xbs, ybs;

	ushort[0x17] _u5;
	
	static GIM_IHeader read(Stream s, Endian endian = Endian.LittleEndian) {
		GIM_IHeader header;
		s.read(TA(header));
		
		cendian(header._u1, endian);
		cendian(header.type, endian);
		cendian(header._u2, endian);
		cendian(header.width, endian);
		cendian(header.height, endian);
		cendian(header.bpp, endian);
		cendian(header.xbs, endian);
		cendian(header.ybs, endian);
		for (int n = 0; n < _u5.length; n++) cendian(header._u5[n], endian);
		
		foreach (v; header._u5) {
			//writefln("v:%d", v);
		}
		
		return header;
	}
}

class GIM_Image : Image {
	Stream dstream;
	GIM_IHeader header;
	GIM_Image clut;
	uint[] data;
	Endian endian;

	ubyte bpp() { return cast(ubyte)header.bpp; }
	int width() { return cast(int)header.width; }
	int height() { return cast(int)header.height; }

	int ncolor() {
		if (clut is null) return 0;
		return clut.header.width * clut.header.height;
	}

	RGBA color(int idx) {
		RGBA c;

		if (clut is null) {
			c.r = c.g = c.b = c.a = cast(ubyte)((idx * 255) / (1 << header.bpp));
		} else {
			c.v = clut.get(idx, 0);
		}

		return c;
	}

	RGBA color(int idx, RGBA c) {
		if (clut !is null) clut.set(idx, 0, c.v);
		return c;
	}

	void readHeader(Stream s, Endian endian = Endian.LittleEndian) {
		header = GIM_IHeader.read(s, endian);
		data = new uint[header.width * header.height];
	}

	bool check(int x, int y) {
		return (x >= 0 && y >= 0 && x < header.width && y < header.height);
	}

	void set(int x, int y, uint v) {
		if (!check(x, y)) return;
		data[y * header.width + x] = v;
	}

	uint get(int x, int y) {
		if (!check(x, y)) return 0;
		return data[y * header.width + x];
	}

	override void set32(int x, int y, RGBA c) {
		//printf("%02X%02X%02X%02X:", c.r, c.g, c.b, c.a);
		if (bpp == 16) { set(x, y, c.v); return; }
		Image.set32(x, y, c);
	}

	void transferBlock(int sx, int sy, void[] data, bool read) {
		//writefln("%d, %d", sx, sy);
		switch (header.bpp) {
			case 32: {
				uint[] d4 = cast(uint[])data;
				for (int y = 0, n = 0; y < header.ybs; y++) for (int x = 0; x < header.xbs; x++, n++) {
					if (read) d4[n] = get(sx + x, sy + y);
					else set(sx + x, sy + y, d4[n]);
				}
			} break;
			case 16: {
				ushort[] d2 = cast(ushort[])data;
				for (int y = 0, n = 0; y < header.ybs; y++) for (int x = 0; x < header.xbs; x++, n++) {
					if (read) d2[n] = cast(ushort)get32(sx + x, sy + y).decode(RGBA_5650);
					else set32(sx + x, sy + y, RGBA(RGBA_5650, d2[n]));
				}
			} break;
			case 8: {
				ubyte[] d1 = cast(ubyte[])data;
				for (int y = 0, n = 0; y < header.ybs; y++) for (int x = 0; x < header.xbs; x++, n++) {
					if (read) d1[n] = cast(ubyte)get(sx + x, sy + y);
					else set(sx + x, sy + y, d1[n]);
				}
			} break;
			case 4: {
				ubyte[] d1 = cast(ubyte[])data;
				for (int y = 0, n = 0; y < header.ybs; y++) for (int x = 0; x < header.xbs; x += 2, n++) {
					if (read) {
						d1[n] = ((get(sx + x + 0, sy + y)) & 0xF) | (((get(sx + x + 1, sy + y)) & 0xF) << 4);
					} else {
						set(sx + x + 0, sy + y, (d1[n] >> 0) & 0xF);
						set(sx + x + 1, sy + y, (d1[n] >> 4) & 0xF);
					}
				}
			} break;
			default: {
				throw(new Exception(std.string.format("Unprocessed BPP (%d)", header.bpp)));
			} break;
		}
	}

	void read() {
		ubyte[] block;
		block.length = header.xbs * header.ybs * header.bpp / 8;

		dstream.position = 0;

		for (int y = 0; y < header.height; y += header.ybs) for (int x = 0; x < header.width; x += header.xbs) {
			//for (int n = 0; n < block.length; n++) block[n] = 0;
			dstream.read(block);
			transferBlock(x, y, block, false);
		}
	}

	void write() {
		ubyte[] block;
		block.length = header.xbs * header.ybs * header.bpp / 8;

		dstream.position = 0;

		for (int y = 0; y < header.height; y += header.ybs) for (int x = 0; x < header.width; x += header.xbs) {
			//for (int n = 0; n < block.length; n++) block[n] = 0;
			transferBlock(x, y, block, true);
			dstream.write(block);
		}

		if (clut) clut.write();
	}
}

class ImageFileFormat_GIM : ImageFileFormat {
	override string identifier() { return "gim"; }

	string header_le = cast(string)"MIG.00.1PSP\0\0\0\0\0";
	string header_be = cast(string)".GIM1.00\0PSP\0\0\0\0";
	Endian endian;

	Image[] imgs;

	void processStream(Stream s, int level = 0) {
		ushort unk0, type;
		uint len, unk1, unk2;
		Stream cs;

		debug(gim_stream) { string pad; for (int n = 0; n < level; n++) pad ~= " "; }

		GIM_Image img, clut;

		while (!s.eof) {
			int start = cast(int)s.position;

			s.read(type);
			s.read(unk0);
			s.read(len);
			s.read(unk1);
			s.read(unk2);
			
			cendian(type, endian);
			cendian(len, endian);
			cendian(unk1, endian);
			cendian(unk2, endian);

			cs = new SliceStream(s, start + 0x10, start + len);

			debug(gim_stream) writefln("%stype: %04X (%04X)", pad, type, len);

			switch (type) {
				case 0x02: // GimContainer
					processStream(cs, level + 1);
				break;
				case 0x03: // Image
					processStream(cs, level + 1);
				break;
				case 0x04: // ImagePixels
				case 0x05: // ImagePalette
				{
					// 0x40 bytes header
					GIM_Image i = new GIM_Image;
					i.readHeader(cs, endian);

					//writefln("POS: %08X", cs.position);

					//i.header.bpp = 8;

					debug(gim_stream) writefln("%s [%d] [%2d] (%dx%d) (%dx%d)", pad, i.header.type, i.header.bpp, i.header.width, i.header.height, i.header.xbs, i.header.ybs);

					//i.header.xbs = (1 << (i.header.type - 1));
					switch (i.header.type) {
						case 0x00: i.header.xbs =  8; break;
						case 0x03: i.header.xbs =  4; break;
						case 0x05: i.header.xbs = 16; break;
						case 0x04: i.header.xbs = 32; break;
						default: throw(new Exception(std.string.format("Unknown image type (%d)", i.header.type)));
					}

					//i.header.xbs = 256 / (i.header.ybs * i.header.bpp / 8);
					//writefln(i.header.xbs);

					i.dstream = new SliceStream(cs, GIM_IHeader.sizeof, len);

					i.read();

					if (type == 0x04) img = i; else clut = i;
				} break;
				case 0xFF: // Comments
					writefln("%d", cs.size);
				break;
				default:
				//throw(new Exception(std.string.format("Invalid GIM unknown chunk type:%04X", type)));
			}

			s.position = start + len;
		}

		if (img && clut) img.clut = clut;
		//writefln("%08X, %08X", cast(void *)img, cast(void *)clut);

		if (img) imgs ~= img;
	}

	Image[] readMultiple(Stream s) {
		if (!check(s)) throw(new Exception("Not a GIM file"));

		imgs.length = 0;
		processStream(s);

		return imgs;
	}

	override Image read(Stream s) {
		Image[] imgs = readMultiple(s);
		if (!imgs.length) throw(new Exception("Not found images in GIM file"));
		return imgs[0];
	}

	override bool write(Image i, Stream s) {
		GIM_Image ri = cast(GIM_Image)read(s);
		//writefln("%08X", i.get32(0, 0).v);
		ri.copyFrom(i);
		ri.write();
		return true;
	}

	override int check(Stream s) {
		auto cheader = s.readString(0x10);
		if (cheader == header_be) { endian = Endian.BigEndian   ; return 10; }
		if (cheader == header_le) { endian = Endian.LittleEndian; return 10; }
		return 0;
	}
}

static this() {
	ImageFileFormatProvider.registerFormat(new ImageFileFormat_GIM);
}


// SPECS: http://www.libpng.org/pub/png/spec/iso/index-object.html
class ImageFileFormat_PNG : ImageFileFormat {
	void[] header = cast(void[])x"89504E470D0A1A0A";

	override string identifier() { return "png"; }

	align(1) struct PNG_IHDR {
		uint width;
		uint height;
		ubyte bps;
		ubyte ctype;
		ubyte comp;
		ubyte filter;
		ubyte interlace;
	}

	override bool write(Image i, Stream s) {
		PNG_IHDR h;

		void writeChunk(string type, void[] data = []) {
			uint crc = void;
			
			s.write(cast(uint)bswap((cast(ubyte[])data).length));
			s.write(cast(ubyte[])type);
			s.write(cast(ubyte[])data);

			ubyte[] full = cast(ubyte[])type ~ cast(ubyte[])data;
			crc = etc.c.zlib.crc32(0, cast(ubyte *)full.ptr, full.length);

			s.write(bswap(crc));
		}

		void writeIHDR() { writeChunk("IHDR", TA(h)); }
		void writeIEND() { writeChunk("IEND", []); }

		void writeIDAT() {
			ubyte[] data;

			data.length = i.height + i.width * i.height * 4;

			int n = 0;
			ubyte *datap = data.ptr;
			for (int y = 0; y < i.height; y++) {
				*datap = 0x00; datap++;
				for (int x = 0; x < i.width; x++) {
					if (i.hasPalette) {
						*datap = cast(ubyte)i.get(x, y); datap++;
					} else {
						RGBA cc = i.getColor(x, y);
						*datap = cc.r; datap++;
						*datap = cc.g; datap++;
						*datap = cc.b; datap++;
						*datap = cc.a; datap++;
					}
				}
			}

			writeChunk("IDAT", cast(void[])std.zlib.compress(data, 1));
		}

		void writePLTE() {
			ubyte[] data;
			data.length = i.ncolor * 3;
			ubyte* pdata = data.ptr;
			for (int n = 0; n < i.ncolor; n++) {
				RGBA c = i.color(n);
				*pdata = c.r; pdata++;
				*pdata = c.g; pdata++;
				*pdata = c.b; pdata++;
			}
			writeChunk("PLTE", data);
		}

		void writetRNS() {
			ubyte[] data;
			data.length = i.ncolor;
			ubyte* pdata = data.ptr;
			bool hasTrans = false;
			for (int n = 0; n < i.ncolor; n++) {
				RGBA c = i.color(n);
				*pdata = c.a; pdata++;
				if (c.a != 0xFF) hasTrans = true;
			}
			if (hasTrans) writeChunk("tRNS", data);
		}

		s.write(cast(ubyte[])header);
		h.width = bswap(i.width);
		h.height = bswap(i.height);
		h.bps = 8;
		h.ctype = (i.hasPalette) ? 3 : 6;
		h.comp = 0;
		h.filter = 0;
		h.interlace = 0;

		writeIHDR();
		if (i.hasPalette) {
			writePLTE();
			writetRNS();
		}
		writeIDAT();
		writeIEND();

		return true;
	}

	override Image read(Stream s) {
		PNG_IHDR h;

		uint Bpp;
		Image i;
		ubyte[] buffer;
		uint size, crc;
		ubyte[4] type;
		bool finished = false;

		if (!check(s)) throw(new Exception("Not a PNG file"));

		while (!finished && !s.eof) {
			s.read(size); size = bswap(size);
			s.read(type);
			uint pos = cast(uint)s.position;

			//writefln("%s", cast(string)type);

			switch (cast(string)type) {
				case "IHDR":
					s.read(TA(h));
					h.width = bswap(h.width); h.height = bswap(h.height);

					switch (h.ctype) {
						case 4: case 0: throw(new Exception("Grayscale images not supported yet"));
						case 2: Bpp = 3; break; // RGB
						case 3: Bpp = 1; break; // Index
						case 6: Bpp = 4; break; // RGBA
						default: throw(new Exception("Invalid image type"));
					}

					i = (Bpp == 1) ? cast(Image)(new Bitmap8(h.width, h.height)) : cast(Image)(new Bitmap32(h.width, h.height));
				break;
				case "PLTE":
					if (size % 3 != 0) throw(new Exception("Invalid Palette"));
					i.ncolor = size / 3;
					for (int n = 0; n < i.ncolor; n++) {
						RGBA c;
						s.read(c.r);
						s.read(c.g);
						s.read(c.b);
						c.a = 0xFF;
						i.color(n, c);
					}
				break;
				case "tRNS":
					if (Bpp == 1) {
						//if (size != i.ncolor) throw(new Exception(std.string.format("Invalid Transparent Data (%d != %d)", size, i.ncolor)));
						//for (int n = 0; n < i.ncolor; n++) {
						for (int n = 0; n < size; n++) {
							RGBA c = i.color(n);
							s.read(c.a);
							i.color(n, c);
						}
					} else {
						throw(new Exception(std.string.format("Invalid Transparent Data (%d != %d) 32bits", size, i.ncolor)));
					}
				break;
				case "IDAT":
					ubyte[] temp; temp.length = size;
					s.read(temp); buffer ~= temp;
				break;
				case "IEND":
					ubyte[] idata = cast(ubyte[])std.zlib.uncompress(buffer);
					ubyte *pdata = void;

					ubyte[] row, prow;

					prow.length = Bpp * (h.width + 1);
					row.length = prow.length;

					ubyte PaethPredictor(int a, int b, int c) {
						int babs(int a) { return (a < 0) ? -a : a; }
						int p = a + b - c; int pa = babs(p - a), pb = babs(p - b), pc = babs(p - c);
						if (pa <= pb && pa <= pc) return cast(ubyte)a; else if (pb <= pc) return cast(ubyte)b; else return cast(ubyte)c;
					}

					for (int y = 0; y < h.height; y++) {
						int x;

						pdata = idata.ptr + (1 + Bpp * h.width) * y;
						ubyte filter = *pdata; pdata++;
						
						//writefln("%d: %d", y, filter);
						
						switch (filter) {
							default: throw(new Exception(std.string.format("Row filter 0x%02d unsupported", filter)));
							case 0: for (x = Bpp; x < row.length; x++, pdata++) row[x] = cast(ubyte)(*pdata + 0); break; // Unfiltered
							case 1: for (x = Bpp; x < row.length; x++, pdata++) row[x] = cast(ubyte)(*pdata + row[x - Bpp]); break; // Sub
							case 2: for (x = Bpp; x < row.length; x++, pdata++) row[x] = cast(ubyte)(*pdata + prow[x]); break; // Up
							case 3: for (x = Bpp; x < row.length; x++, pdata++) row[x] = cast(ubyte)(*pdata + (row[x - Bpp] + prow[x]) / 2); break; // Average
							case 4: for (x = Bpp; x < row.length; x++, pdata++) row[x] = cast(ubyte)(*pdata + PaethPredictor(row[x - Bpp], prow[x], prow[x - Bpp])); break; // Paeth
						}

						prow[0..row.length] = row[0..row.length];

						ubyte *rowp = row.ptr + Bpp;
						for (x = 0; x < h.width; x++) {
							if (Bpp == 1) {
								i.set(x, y, *rowp++);
							} else {
								RGBA c;
								c.r = *rowp++;
								c.g = *rowp++;
								c.b = *rowp++;
								c.a = (Bpp == 4) ? *rowp++ : 0xFF;
								i.set(x, y, c.v);
							}
						}
					}
					//writefln("%d", pdata - idata.ptr);
					//writefln("%d", idata.length);
					finished = true;
				break;
				default: break;
			}
			s.position = pos + size;
			s.read(crc);
			//break;
		}

		return i;
	}

	override int check(Stream s) {
		ubyte[] cheader; cheader.length = header.length;
		s.read(cast(ubyte[])cheader);
		return (cheader == header) ? 10 : 0;
	}
}

static this() {
	ImageFileFormatProvider.registerFormat(new ImageFileFormat_PNG);
}


struct Rect { int x, y, w, h; long area() { return w * h; } string toString() { return std.string.format("(%d,%d)-(%d,%d)", x, y, w, h); } }

class PSDReader : FilterStream {
	this(Stream s, bool big_endian = true) {
		if (big_endian) s = new EndianStream(s, Endian.BigEndian);
		super(s);
	}
	this(ubyte[] data) { this(new MemoryStream(data)); }
	
	alias source s;

	ubyte  readu1()    { ubyte  v; s.read(v); return v; }
	byte   reads1()    { byte   v; s.read(v); return v; }
	ushort readu2()    { ushort v; s.read(v); return v; }
	short  reads2()    { short  v; s.read(v); return v; }
	uint   readu4()    { uint   v; s.read(v); return v; }
	int    reads4()    { int    v; s.read(v); return v; }
	string reads ()    { return cast(string)s.readString(readu2); }
	Rect   read_rect() { Rect r = void; r.y = reads4; r.x = reads4; r.h = reads4 - r.y; r.w = reads4 - r.x; return r; }
	void   readpad(int pad = 2) { while (s.position % pad) readu1; }
	ubyte[] readb(int l){ return cast(ubyte[])s.readString(l); }
	
	alias readu1 u1; alias reads1 s1;
	alias readu2 u2; alias reads2 s2;
	alias readu4 u4; alias reads4 s4;
	alias reads ss;
	alias read_rect rect;
	alias readString str;
	alias readb array;
	alias readpad pad;
	alias seekCur skip;
}

class SlicePSDReader : PSDReader {
	this(PSDReader s, long start, long length = 0) {
		super(new SliceStream(s.s, start, start + length), true);
	}
}

// http://www.pcpix.com/Photoshop/char.htm
// http://www.soft-gems.net:8080/browse/~raw,r=99/Library/GraphicEx/Source/GraphicEx.pas
// http://www.codeproject.com/KB/graphics/simplepsd.aspx
// http://www.codeproject.com/KB/graphics/PSDParser.aspx
class ImageFileFormat_PSD : ImageFileFormat {
	override string identifier() { return "psd"; }
	override int check(Stream s) { return (s.readString(4) == "8BPS") ? 10 : 0; }	

	enum ColorModes : ushort { Bitmap = 0, Grayscale = 1, Indexed = 2, RGB = 3, CMYK = 4, Multichannel = 7, Duotone = 8, Lab = 9 }
	enum Compression { None = 0, RLE, ZipNoPrediction, ZipPrediction, Jpeg }	

	override bool write(Image i, Stream s) {
		return false;
	}
	
	class Header {
		ushort ver;
		ushort channels, bpp;
		ushort width, height;
		
		this(PSDReader s) { read(s); }

		void read(PSDReader s) {
			assert(s.str(4) == "8BPS", "Not a PSD file");
			
			// Version
			ver = s.u2; assert(ver == 1, "Not a ver1 PSD");
			// ??
			s.str(6);
			// Channels
			channels = cast(ushort)s.u2;
			// Height
			height = cast(ushort)s.u4; assert(height < 16384);
			// Width
			width = cast(ushort)s.u4; assert(width < 16384);
			// Bits per pixel
			bpp = cast(ushort)s.u2;
			ColorModes cm = cast(ColorModes)s.u2;
		}
	}
	
	class Palette {
		RGBA[] colors;

		this(PSDReader s) { read(s); }
		
		void read(PSDReader s) {
			colors.length = s.u4;
			for (int n = 0; n < colors.length; n++) {
				colors[n] = RGBA(s.u1, s.u1, s.u1);
				//writefln(colors[n]);
			}		
		}
	}
	
	class Layer {
		int layer_id;
		Rect size;
		ushort num_channels;
		short usage;
		uint length;
		string blend;
		ubyte opacity, clipping, flags;
		uint extra_size;
		Image i;
		Mask mask;
		bool is_merged;
		
		this() { }
		
		this(int layer_id, PSDReader s) {
			this.layer_id = layer_id;
			read(s);
		}
		
		class Mask {
			Rect size, rect2;
			ubyte color;
			ubyte flags;
			ubyte flags2;
			ubyte maskbg;
			
			this(PSDReader s) { read(s); }
			
			void read(PSDReader s) {
				int nlength = s.u4; if (!nlength) return;
				
				int pos = cast(int)s.position;
				
				size = s.rect;
				color = s.u1;
				flags = s.u1;
				
				if (nlength == 20) {
					s.u2;
				} else if (nlength == 36) {
					flags2 = s.u1; //same flags as above according to docs!?!?
					maskbg = s.u1; //Real user mask background. Only 0 or 255 - ie bool?!?
					rect2  = s.rect; //new Rectangle(reader).ToERectangle(); //same as above rectangle according to docs?!?!
				}
				
				s.position = pos + nlength;
			}
		}
		
		class Channel {
			short usage;
			uint length;
		
			this(PSDReader s) { read(s); }
			
			void read(PSDReader s) {
				usage = s.s2;
				length = s.u4;			
			}
		}
		
		Channel[short] channels;
	
		void read(PSDReader s) {
			size = s.rect;
			
			//writefln("layer:%d (%s)", layer_id, size);
			
			num_channels = s.u2;
			for (int n = 0; n < num_channels; n++) {
				auto ch = new Channel(s);
				if (ch.usage == -2) continue;
				channels[ch.usage] = ch;
			}

			//writefln(": %d (%s)", nchan, size);

			string magic = cast(string)s.str(4); assert(magic == "8BIM", "Invalid image layer header");
			blend = cast(string)s.str(4); // 'levl'=Levels 'curv'=Curves 'brit'=Brightness/contrast 'blnc'=Color balance 'hue '=Old Hue/saturation, Photoshop 4.0 'hue2'=New Hue/saturation, Photoshop 5.0 'selc'=Selective color 'thrs'=Threshold 'nvrt'=Invert 'post'=Posterize
			
            opacity = s.u1;
			clipping = s.u1;
			flags = s.u1;
			s.u1;
			
			auto ss = new PSDReader(s.array(extra_size = s.u4));
			
			mask = new Mask(ss);
		}
		
		int decodeRLE(PSDReader s, ubyte[] data, int pos) {
			int len = s.u1;
			int count = 0;
			
			if (len == 128) return 0;

			if (len < 128) {
				len++;
				count += len;

				for (;len > 0; len--) data[pos++] = s.u1;
			} else {
				len = cast(ubyte)(len ^ 0xFF) + 2;
				count += len;
				
				for (ubyte v = s.u1; len > 0; len--) data[pos++] = v;
			}
			
			return count;
		}
		
		ubyte[] readPixelsChannel(PSDReader s, Compression compression) {
            int Bpc = header.bpp / 8;
            int bytesPerRow = size.w * Bpc;

            ubyte[] r = new ubyte[bytesPerRow * size.h];

            switch (compression) {
                case Compression.None:
					s.read(r);
				break;
                case Compression.RLE:
                    for (int i = 0; i < size.h; i++) {
                        int offset = i * bytesPerRow;
                        int numChunks = 0;
						int numDecodedBytes = 0;
                        while (numDecodedBytes < bytesPerRow) {
							numDecodedBytes += decodeRLE(s, r, offset + numDecodedBytes);
							numChunks++;
                        }
                    }
				break;
                case Compression.ZipNoPrediction: throw (new Exception("ZIP without prediction, no specification"));
                case Compression.ZipPrediction: throw (new Exception("ZIP with prediction, no specification"));
                default: throw (new Exception(format("Compression not defined: %d", compression)));
            }
			
			//writefln("bpp: %d", r);

			return r;
		}
		
		class PixelData {
			ubyte[][] channels;
			
			this(PSDReader s, int num_channels = 1, bool is_merged = false) {
				channels.length = num_channels;
				read(s);
			}
			
			void read(PSDReader s) {
				Compression compression;
				
				//writefln("is_merged:%d", is_merged);
				
				if (is_merged) {
					compression = cast(Compression)s.u2;
					for (int n = 0; n < channels.length; n++) s.skip(size.h * 2);
				}
			
				for (int n = 0; n < channels.length; n++) {
					//writefln("layer:%d channel:%d/%d", layer_id, n, channels.length);
					
					//(new File(format("layer_%d_channel_%d.data", layer_id, n), FileMode.OutNew)).copyFrom(new SliceStream(s, s.position));
					if (!is_merged) {
						compression = cast(Compression)s.u2;
						s.skip(size.h * 2);
					}
					channels[n] = readPixelsChannel(s, compression);
				}
			}
			
			void store(Bitmap32 i) {
				assert (channels.length >= 4, format("channels.length(%d) < 4", channels.length));
				int l = i.data.length; alias channels c;
				ubyte[] r, g, b, a;
				if (is_merged) {
					a = c[3]; r = c[0]; g = c[1]; b = c[2];
				} else {
					a = c[0]; r = c[1]; g = c[2]; b = c[3];
				}
				for (int n = 0; n < l; n++) i.data[n] = RGBA(r[n], g[n], b[n], a[n]);
			}
		}

		void readPixels(PSDReader s) {
			//Compression compression = cast(Compression)s.u2; assert (compression == Compression.Rle, "Unprocessed compression");
			
			//(new File(format("layer_%d.data", layer_id), FileMode.OutNew)).copyFrom(new SliceStream(s, s.position));
			
			i = new Bitmap32(size.w, size.h);
			
			auto pd = new PixelData(s, num_channels, is_merged); pd.store(cast(Bitmap32)i);
			//writefln("aaaaa");
			
			foreach (ch; channels) {
                if (ch.usage == -2) continue;
				//ch.Data = px.GetChannelData(i++);
            }

			//writefln("area:", mask.size.area);
			if (mask && mask.size.area) {
				auto pd2 = new PixelData(s, 1);
			}

			//foreach (n, c; i.data) i.data[n] = RGBA.toBGRA(c);
			
			//writefln("out");
			//i.write("out2.png");
		}
	}
	
	Header header;
	Palette pal;
	Layer[] layers;
	
	class Resource {
		ushort id;
		string name;
		ubyte[] data;
	
		this(PSDReader s) { read(s); }
		
		void read(PSDReader s) {
			string magic = cast(string)s.readString(4);
			
			assert(magic == "8BIM", "Invalid Resource");
			
			id = s.u2;
			name = s.ss;
			ubyte[] data;
			data.length = s.u4;
			s.read(data);
		}
	}
	
	void ReadResources(PSDReader ss) {
		int res_length = ss.u4;
		auto s = new SlicePSDReader(ss, ss.position, res_length); ss.skip(res_length);
		
		while (!s.eof) {
			new Resource(s);
			s.pad(2);
		}
	}
	
	void ReadLayers1(PSDReader ss) {
		uint size = ss.u4;
		//writefln("size:%d", size);
		
		auto s = new SlicePSDReader(ss, ss.position, size); ss.skip(size);

		if (false)
		{
			int num_layers = s.s2;
			bool skip_first_alpha = true;
			
			if (num_layers < 0) {
				skip_first_alpha = false;
				num_layers = -num_layers;
			}
			
			layers.length = num_layers;
			
			for (int n = 0; n < layers.length; n++) layers[n] = new Layer(n, s);
			for (int n = 0; n < layers.length; n++) layers[n].readPixels(s);
		}
	}
	
	void ReadLayers(PSDReader ss) {
		uint total_layers_size = ss.u4;
		//writefln("total_layers_size:%d", total_layers_size);
		
		auto s = new SlicePSDReader(ss, ss.position, total_layers_size); ss.skip(total_layers_size);
		
		ReadLayers1(s);

		// Mask
		int mask_length = s.u4;
		s.skip(mask_length);
		//writefln(mask_length);
		
		//writefln(s.position);
		
		//(new File(format("out3.bin"), FileMode.OutNew)).copyFrom(new SliceStream(s, s.position));
	}
	
	override Image read(Stream _s) {
		auto s = new PSDReader(_s);
		
		header = new Header(s);
		pal = new Palette(s);
		ReadResources(s);
		ReadLayers(s);
		
		//writefln(s.position);
		
		Layer l = new Layer();
		l.is_merged = true;
		l.size = Rect(0, 0, header.width, header.height);
		l.num_channels = header.channels;
		l.readPixels(s);
		
		return l.i;
	}
}

static this() {
	ImageFileFormatProvider.registerFormat(new ImageFileFormat_PSD);
}


// http://local.wasp.uwa.edu.au/~pbourke/dataformats/tga/
class ImageFileFormat_TGA : ImageFileFormat {
	override string identifier() { return "tga"; }

	align(1) struct TGA_Header {
		ubyte idlength;           // 0
		ubyte colourmaptype;      // 1
		ubyte datatypecode;       // 2
		short colourmaporigin;    // 3-4
		short colourmaplength;    // 5-6
		ubyte colourmapdepth;     // 7
		short x_origin;           // 8-9
		short y_origin;           // 10-11
		short width;              // 12-13
		short height;             // 14-15
		ubyte bitsperpixel;       // 16
		ubyte imagedescriptor;    // 17
	   
		private alias imagedescriptor id;

		int  atr_bits()          { return cast(int)Bit.EXT(id, 0, 3); }
		int  atr_bits(int v)     { id = cast(ubyte)Bit.INS(id, 0, 3, v); return atr_bits; }

		bool flip_y()            { return cast(int)Bit.EXT(id, 5, 1) == 0; }
		bool flip_y(bool v)      { id = cast(ubyte)Bit.INS(id, 5, 1, !v); return flip_y; }

		int  interleaving()      { return cast(int)Bit.EXT(id, 6, 2); }
		int  interleaving(int v) { id = cast(ubyte)Bit.INS(id, 6, 2, v); return interleaving; }
	}
	
	static assert (TGA_Header.sizeof == 18);

	override bool write(Image i, Stream s) {
		TGA_Header h;

		h.idlength = 0;
		h.x_origin = 0;
		h.y_origin = 0;
		h.width    = cast(short)i.width;
		h.height   = cast(short)i.height;
		//h.imagedescriptor = 0b_00_1_0_1000;
		h.flip_y   = false;
		
		if (i.hasPalette) {
			h.colourmaptype = 1;
			h.datatypecode = 1;
			h.colourmaporigin = 0;
			h.colourmaplength = cast(short)i.ncolor;
			h.colourmapdepth = 24;
			h.bitsperpixel = 8;
			
			h.imagedescriptor |= 8;
			//h.imagedescriptor = 8;
		} else {
			h.colourmaptype = 0;
			h.datatypecode = 2;
			h.colourmaplength = 0;
			h.colourmapdepth = 0;
			h.bitsperpixel = 32;
		}

		s.write(TA(h));
		
		// CLUT
		if (i.hasPalette) {
			for (int n = 0; n < h.colourmaplength; n++) {
				auto c = RGBA.toBGRA(i.color(n));
				s.write(TA(c)[0..(h.colourmapdepth / 8)]);
			}
		}

		ubyte[] data;
		data.length = h.width * h.height * (i.hasPalette ? 1 : 4);
		//writef("(%dx%d)", h.width, h.height);

		ubyte *ptr = data.ptr;
		if (i.hasPalette) {
			for (int y = 0; y < h.height; y++) for (int x = 0; x < h.width; x++) {
				*ptr = cast(ubyte)i.get(x, y);
				ptr++;
			}
		} else {
			for (int y = 0; y < h.height; y++) for (int x = 0; x < h.width; x++) {
				RGBA c; c.v = i.get(x, y);
				*cast(uint *)ptr = RGBA.toBGRA(c).v;
				ptr += 4;
			}
		}

		s.write(data);
		
		s.write(cast(ubyte[])x"000000000000000054525545564953494F4E2D5846494C452E00");

		return false;
	}
	
	override Image read(Stream s) {
		TGA_Header h; s.read(TA(h));

		// Skips Id Length field
		s.seek(h.idlength, SeekPos.Current);
		
		assert (h.width <= 4096);
		assert (h.height <= 4096);
		
		assert (h.x_origin == 0);
		assert (h.y_origin == 0);

		RGBA readcol(int depth) {
			RGBA c;
			switch (depth) {
				case 16:
				break;
				case 24:
					s.read(TA(c)[0..3]);
					c = RGBA.toBGRA(c);
					c.a = 0xFF;					
				break;
				case 32:
					s.read(TA(c)[0..4]);
					c = RGBA.toBGRA(c);
				break;
				default: throw(new Exception(format("Invalid TGA Color Map Depth %d", h.colourmapdepth)));
			}
			return c;
		}
		
		int readcols(RGBA[] r, int depth) {
			for (int n = 0; n < r.length; n++) {
				r[n] = readcol(depth);
			}
			return r.length;
		}

		int y_from, y_to, y_inc;
			
		if (h.flip_y) {
			y_from = h.height - 1;
			y_to = -1;
			y_inc = -1;
		} else {
			y_from = 0;
			y_to = h.height;
			y_inc = 1;
		}		
		
		switch (h.datatypecode) {
			case 0: // No image data included.
			{
				return null;
			}
			break;
			case 1: // Uncompressed, color-mapped images.
			{
				auto i = new Bitmap8(h.width, h.height);
				
				if (h.colourmaporigin + h.colourmaplength > 0x100) {
					throw(new Exception("Not implemented multibyte mapped images"));
				}
				
				i.ncolor = h.colourmaporigin + h.colourmaplength;
				
				for (int n = 0; n < h.colourmaplength; n++) {
					i.color(n + h.colourmaporigin, readcol(h.colourmapdepth));
				}

				auto row = new ubyte[h.width];

				for (int y = y_from; y != y_to; y += y_inc) {
					s.read(row);
					for (int x = 0; x < h.width; x++) {
						i.set(x, y, row[x]);
					}
				}
				
				return i;
			}
			break;
			case 2: // Uncompressed, RGB images.
			{
				auto i = new Bitmap32(h.width, h.height);
				
				auto row = new RGBA[h.width];
				for (int y = y_from; y != y_to; y += y_inc) {
					readcols(row, h.bitsperpixel);
					for (int x = 0; x < h.width; x++) {
						i.set32(x, y, row[x]);
					}
				}
				//writefln(h.bitsperpixel);
				
				return i;
			}
			break;
			case  3:  // Uncompressed, black and white images.
			case  9:  // Runlength encoded color-mapped images.
			case 10:  // Runlength encoded RGB images.
			case 11:  // Compressed, black and white images.
			case 32:  // Compressed color-mapped data, using Huffman, Delta, and runlength encoding.
			case 33:  // Compressed color-mapped data, using Huffman, Delta, and runlength encoding.  4-pass quadtree-type process.
			break;
			default: throw(new Exception(format("Invalid tga colour map type: %d", h.datatypecode)));
		}

		throw(new Exception(format("Unimplemented tga colour map type: %d", h.datatypecode)));
		return null;
	}
	
	override int check(Stream s) {
		TGA_Header h; s.read(TA(h));
		switch (h.datatypecode) {
			default: return 0;
			case 0, 1, 2, 3, 9, 10, 11, 32, 33: break;
		}

		if (h.width > 4096 || h.height > 4096) return 0;
		
		return 5;
	}	
}

static this() {
	ImageFileFormatProvider.registerFormat(new ImageFileFormat_TGA);
}


align(1) struct TIM2Header {
	char[4]  FileId = "TIM2"; //  ID of the File (must be 'T', 'I', 'M' and '2')
	ubyte    FormatVersion; // Version number of the format
	ubyte    FormatId;      // ID of the format
	ushort   Pictures;      // Number of picture data
	ubyte[8] pad;           // Padding (must be 0x00)
}

align(1) struct TIM2EntryHeader {
	uint   TotalSize;   // Total size of the picture data in bytes
	uint   ClutSize;    // CLUT data size in bytes
	uint   ImageSize;   // Image data size in bytes
	ushort HeaderSize;  // Header size in bytes
	ushort ClutColors;  // Total color number in CLUT data
	ubyte  PictFormat;  // ID of the picture format (must be 0)
	ubyte  MipMapTexs;  // Number of MIPMAP texture
	ubyte  ClutType;    // Type of the CLUT data
	ubyte  ImageType;   // Type of the Image data
	ushort ImageWidth;  // Width of the picture
	ushort ImageHeight; // Height of the picture

	ubyte GsTex0[8];    // Data for GS TEX0 register
	ubyte GsTex1[8];    // Data for GS TEX1 register
	uint  GsRegs;       // Data for GS TEXA, FBA, PABE register
	uint  GsTexClut;    // Data for GS TEXCLUT register
}

align(1) struct TIM2MipMapHeader{
	ulong GsMiptbp1;
	ulong GsMiptbp2;
	uint  MMImageSize[0];
}

align(1) struct TIM2ExtHeader {
	ubyte[4] ExHeaderId = ['e', 'X', 't', 0];
	uint     UserSpaceSize;
	uint     UserDataSize;
	uint     Reserved;
}

//debug = tm2_stream;

class ImageFileFormat_TM2 : ImageFileFormat {
	override string identifier() { return "tm2"; }

	string header = "TIM2";
	
	override bool update(Image i, Stream s) {
		return false;
	}

	override Image read(Stream s) {
		Image ic = new Bitmap8(1, 1);
		TIM2Header h;
		
		s.read(TA(h));
		if (h.FileId != "TIM2") throw(new Exception("File isn't a TIM2 one"));
		
		int pcount = h.Pictures;
		while (pcount--) {
			ubyte[] palette;
			ubyte[] dimage;

			// Leemos el header
			TIM2EntryHeader teh; s.read(TA(teh));
			s.seek(teh.HeaderSize - teh.sizeof, SeekPos.Current);

			// Leemos la imagen
			dimage.length = teh.ImageSize; s.read(dimage);
			
			//writefln(teh.ImageType);
			
			switch (teh.ImageType) {
				default: throw(new Exception(std.string.format("Unknown TIM2 Image Type 0x%02X", teh.ImageType)));
				case 0x05: // con paleta (4 bits) 8bpp
					Bitmap8 i = new Bitmap8(teh.ImageWidth, teh.ImageHeight);
					
					switch (teh.ClutType) {
						default: throw(new Exception(std.string.format("Unknown TIM2 Clut Type 0x%02X", teh.ClutType)));
						case 0x02: case 0x03:
							uint pbpp = (teh.ClutType + 1);
							//writefln("TYPE:", pbpp);
							//s.seek(teh.ClutSize - pbpp * 0x10, SeekPos.Current);

							// Leemos la paleta
							palette.length = teh.ClutSize; s.read(palette);

							//writefln("%d", teh.ClutColors);
							i.ncolor = teh.ClutColors;

							for (int y = 0, n = 0; y < teh.ImageHeight; y++) {
								for (int x = 0; x < teh.ImageWidth; x++, n++) {
									i.set(x, y, dimage[n]);
								}
							}

							for (int n = 0; n < i.ncolor; n++) {
								RGBA c = RGBA(palette[n * pbpp + 0], palette[n * pbpp + 1], palette[n * pbpp + 2]);
								if (pbpp > 3) c.a = palette[n * pbpp + 3];
								i.color(n, c);
							}
							
							// Unswizzle
							for (int n = 8; n < 256; n += 4 * 8) for (int m = 0; m < 8; m++) i.colorSwap(n + m, n + m + 8);
						break;
					}
					
					ic.childs ~= i;
				break;
				case 0x04: // con paleta (4 bits) 4bpp
					Bitmap8 i = new Bitmap8(teh.ImageWidth, teh.ImageHeight);

					switch (teh.ClutType) {
						default: throw(new Exception(std.string.format("Unknown TIM2 Clut Type 0x%02X", teh.ClutType)));
						case 0x02: case 0x03:
							uint pbpp = (teh.ClutType + 1);

							palette.length = teh.ClutSize; s.read(palette);

							i.ncolor = teh.ClutColors;
							for (int y = 0, n = 0; y < teh.ImageHeight; y++) {
								for (int x = 0; x < teh.ImageWidth; x += 2, n++) {
									i.set(x + 0, y, (dimage[n] & 0x0F) >> 0);
									i.set(x + 1, y, (dimage[n] & 0xF0) >> 4);
								}
							}

							for (int n = 0; n < i.ncolor; n++) {
								RGBA c = RGBA(palette[n * pbpp + 0], palette[n * pbpp + 1], palette[n * pbpp + 2]);
								if (pbpp > 3) c.a = palette[n * pbpp + 3];
								i.color(n, c);
								//writefln("%02X%02X%02X%02X", c.r, c.g, c.b, c.a);
							}
						break;
					}
					
					ic.childs ~= i;
				break;
				case 0x03: // a 32 bits
					Bitmap32 i = new Bitmap32(teh.ImageWidth, teh.ImageHeight);
					for (int y = 0, n = 0; y < teh.ImageHeight; y++) {
						for (int x = 0; x < teh.ImageWidth; x++, n += 4) {
							RGBA c = RGBA(dimage[n + 0], dimage[n + 1], dimage[n + 2], dimage[n + 3]);
							i.set(x, y, c.v);
						}
					}
					
					ic.childs ~= i;
				break;				
			}
			
		}
		
		return ic;
	}

	override int check(Stream s) { return (s.readString(header.length) == header) ? 10 : 0; }
}

static this() {
	ImageFileFormatProvider.registerFormat(new ImageFileFormat_TM2);
}