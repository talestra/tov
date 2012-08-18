module fps4;

import std.intrinsic, std.string, std.stream, std.conv, std.stdio, std.getopt, std.path, std.file, std.system;

class FPS4 {
	align(1) static struct ushortbe {
		ushort _value;
		ushort be() { return _value; }
		ushort le() { return cast(ushort)(((_value & 0xFF) << 8) | ((_value & 0xFF00) >> 8)); }
		ushort value() {
			return le;
		}
		ushort opCast() { return value; }
		string toString() { return to!(string)(value); }
		static assert (this.sizeof == _value.sizeof);
	}

	align(1) static struct uintbe {
		uint _value;
		uint be() { return _value; }
		uint le() { return bswap(_value); }
		uint value() {
			return le;
		}
		uint opCast() { return value; }
		string toString() { return to!(string)(value); }
		static assert (this.sizeof == _value.sizeof);
	}

	align (1) struct Header {
		char[4]   magic = "FPS4";
		uintbe    list_count;
		uintbe    list_start;
		uintbe    list_end;
		ushortbe  entry_length;
		ushortbe  format_type;
		uintbe    unknown;
		uintbe    filename_pos;
	}
	
	class FileEntry {
		int index;
		uint offset;
		uint sectorLength;
		uint length;
		string _name;
		
		void read(Stream stream) {
			scope streambe = new EndianStream(stream, Endian.BigEndian);
			streambe.read(offset);
			streambe.read(sectorLength);
			streambe.read(length);
			
			uint extra_entry_size = header.entry_length.value - 0x0C;
			
			switch (extra_entry_size) {
				case 0:
				break;
				case 4:
					uint _name_ptr;
					streambe.read(_name_ptr);
					if (_name_ptr != 0) {
						scope text_stream = new SliceStream(stream, _name_ptr, stream.size);
						char c;
						while (true) {
							text_stream.read(c);
							if (c == 0) break;
							_name ~= c;
						}
					}
				break;
				default:
					_name = to!(string)(streambe.readString(extra_entry_size).ptr);
				break;
			}
		}
		
		void write() {
			throw(new Exception("Not implemented"));
		}
		
		ubyte[] data() {
			return cast(ubyte[])stream.readString(length);
		}

		string name() {
			if (_name.length) return _name;
			return std.string.format("%04d.DAT", index);
		}
		Stream stream() {
			if (offset == 0xFFFFFFFF) return new MemoryStream();
			return new SliceStream(fpsstream, offset, offset + length);
		}
		string toString() { return std.string.format("FPS4.FileEntry(0x%08X, 0x%08X(0x%08X), '%s')", offset, length, sectorLength, name); }
	}
	
	Stream fpsstream;
	Header header;
	string name;
	FileEntry[] entries;
	FileEntry[string] entriesNamed;
	
	FileEntry opIndex(uint index) {
		return entries[index];
	}

	FileEntry opIndex(string namedIndex) {
		return entriesNamed[namedIndex];
	}
	
	public void load(Stream stream) {
		fpsstream = stream;
		stream.readExact(&header, header.sizeof);
		if (header.magic != Header.init.magic) throw(new Exception("Not a FPS4 file"));
		
		stream.position = header.list_start.value;
		entries.length = header.list_count.value - 1;
		foreach (index, ref entry; entries) {
			entry = new FileEntry;
			entry.index = index;
			entry.read(stream);
			entriesNamed[entry.name] = entry;
		}
		stream.position = header.filename_pos.value;
		name = to!(string)(stream.readString(0x40).ptr);
	}
	
	int opApply(int delegate(ref FileEntry) dg) {
		int result = 0;
		for (int i = 0; i < entries.length; i++) {
			result = dg(entries[i]);
			if (result) break;
		}
		return result;
	}

	uint length() {
		return entries.length;
	}
	
	string toString() {
		return "FPS4('" ~ name ~ "')";
	}
	
	static int main(string[] args) {
		void delegate() action;
		
		void actionHelp() {
			writefln("fps4 <switches> <file.svo> <pattern> [output folder]");
			writefln("");
			writefln("    -l  List files in archive");
			writefln("    -x  Extract files in archive to an output folder");
			//writefln("%s", args);
		}

		void actionProcess(bool extract) {
			if (args.length == 0) throw(new Exception("Must specify a file to list"));
			auto file    = args[0];
			auto pattern = (args.length > 1) ? args[1] : "*";
			auto output  = (args.length > 2) ? args[2] : format("%s.d", file);
			auto fps = new FPS4;
			fps.load(new std.stream.BufferedFile(file));
			int count;
			long selectedSize, totalSize;

			writefln("%s", fps);
			writefln("");
			if (extract) {
				try { std.file.mkdirRecurse(output); } catch { }
			}
			foreach (e; fps) {
				if (fnmatch(e.name, pattern)) {
					writef("%04d - %s", e.index, e);
					if (extract) {
						writef("...Extracting...");
						{
							std.file.write(format("%s/%s", output, e.name), e.data);
						}
						writef("Ok");
					}
					writefln("");
					selectedSize += e.length;
					count++;
				}
				totalSize += e.length;
			}
			if (count == 0) {
				writefln("Any item matched the pattern");
			}
			writefln("");
			writefln("Items: %d/%d using pattern '%s'", count, fps.length, pattern);
			writefln("Size : %.2f MB / %.2f MB", cast(real)selectedSize / 1024.0 / 1024.0, cast(real)totalSize / 1024.0 / 1024.0);
			//writefln("%s", args);
		}
		
		void doHelp() {
			action = &actionHelp;
		}
		
		void doList() {
			action = {
				actionProcess(false);
			};
		}
		
		void doExtract() {
			action = {
				actionProcess(true);
			};
		}
		
		action = &actionHelp;

		getopt(args,
			"l", &doList,
			"x", &doExtract,
			"h", &doHelp
		);
		
		args = args[1..$];

		action();

		/*
		if (list) {
			auto fps = new FPS4;
			fps.load(new std.stream.File("../UI.svo.bak"));
			writefln("%s", fps);
			foreach (e; fps) {
				writefln("%s", e);
			}
			return 0;
		}
		*/
		return 0;
	}
}

version (EXECUTABLE_FPS4) int main(string[] args) { return FPS4.main(args); }