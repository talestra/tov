using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSharpUtils;

namespace TalesOfVesperiaUtils.Compression.C
{
	unsafe public class lzx_vesp
	{
		public const int DECR_OK = 0;
		public const int DECR_DATAFORMAT = 1;
		public const int DECR_ILLEGALDATA = 2;
		public const int DECR_NOMEMORY    = 3;

		// some constants defined by the LZX specification
		public const int LZX_MIN_MATCH               = (2);
		public const int LZX_MAX_MATCH               = (257);
		public const int LZX_NUM_CHARS               = (256);
		public const int LZX_BLOCKTYPE_INVALID       = (0);  // also blocktypes 4-7 invalid
		public const int LZX_BLOCKTYPE_VERBATIM      = (1);
		public const int LZX_BLOCKTYPE_ALIGNED       = (2);
		public const int LZX_BLOCKTYPE_UNCOMPRESSED  = (3);
		public const int LZX_PRETREE_NUM_ELEMENTS    = (20);
		public const int LZX_ALIGNED_NUM_ELEMENTS    = (8);   // aligned offset tree #elements
		public const int LZX_NUM_PRIMARY_LENGTHS     = (7);   // this one missing from spec!
		public const int LZX_NUM_SECONDARY_LENGTHS   = (249); // length tree #elements

		// LZX huffman defines: tweak tablebits as desired
		public const int LZX_PRETREE_MAXSYMBOLS  = (LZX_PRETREE_NUM_ELEMENTS);
		public const int LZX_PRETREE_TABLEBITS   = (6);
		public const int LZX_MAINTREE_MAXSYMBOLS = (LZX_NUM_CHARS + 50*8);
		public const int LZX_MAINTREE_TABLEBITS  = (12);
		public const int LZX_LENGTH_MAXSYMBOLS   = (LZX_NUM_SECONDARY_LENGTHS+1);
		public const int LZX_LENGTH_TABLEBITS    = (12);
		public const int LZX_ALIGNED_MAXSYMBOLS  = (LZX_ALIGNED_NUM_ELEMENTS);
		public const int LZX_ALIGNED_TABLEBITS   = (7);

		public const int LZX_LENTABLE_SAFETY = (64); // we allow length table decoding overruns

		//#define LZX_DECLARE_TABLE(tbl) \
		//ushort tbl##_table[(1<<LZX_##tbl##_TABLEBITS) + (LZX_##tbl##_MAXSYMBOLS<<1)];\
		//byte tbl##_len  [LZX_##tbl##_MAXSYMBOLS + LZX_LENTABLE_SAFETY]

		unsafe public struct Table
		{
			public ushort[] table;
			public byte[] len;
			public uint bits;
			public uint symbols;
		}

		unsafe public class LZXstate
		{
			public byte[] window;         // the actual decoding window              
			public uint window_size;     // window size (32Kb through 2Mb)          
			public uint actual_size;     // window size when it was first allocated 
			public uint window_posn;     // current offset within the window        
			public uint R0, R1, R2;      // for the LRU offset system               
			public ushort main_elements;   // number of main tree elements            
			public int   header_read;     // have we started decoding at all yet?    
			//public ushort block_type;      // type of this block                      
			public uint block_type;      // type of this block                      
			public uint block_length;    // uncompressed length of this block       
			public uint block_remaining; // uncompressed bytes still left to decode 
			public uint frames_read;     // the number of CFDATA blocks processed   
			public int  intel_filesize;  // magic header value used for transform   
			public int  intel_curpos;    // current offset in transform space       
			public bool intel_started;   // have we seen any translatable data yet?

			public Table PRETREE = new Table()
			{
				table = new ushort[(1<<LZX_PRETREE_TABLEBITS) + (LZX_PRETREE_MAXSYMBOLS<<1)],
				len = new byte[LZX_PRETREE_MAXSYMBOLS + LZX_LENTABLE_SAFETY],
				bits = LZX_PRETREE_TABLEBITS,
				symbols = LZX_PRETREE_MAXSYMBOLS,
			};

			public Table MAINTREE = new Table()
			{
				table = new ushort[(1<<LZX_MAINTREE_TABLEBITS) + (LZX_MAINTREE_MAXSYMBOLS<<1)],
				len = new byte[LZX_MAINTREE_MAXSYMBOLS + LZX_LENTABLE_SAFETY],
				bits = LZX_MAINTREE_TABLEBITS,
				symbols = LZX_MAINTREE_MAXSYMBOLS,
			};

			public Table LENGTH = new Table()
			{
				table = new ushort[(1<<LZX_LENGTH_TABLEBITS) + (LZX_LENGTH_MAXSYMBOLS<<1)],
				len = new byte[LZX_LENGTH_MAXSYMBOLS + LZX_LENTABLE_SAFETY],
				bits = LZX_LENGTH_TABLEBITS,
				symbols = LZX_LENGTH_MAXSYMBOLS,
			};

			public Table ALIGNED = new Table()
			{
				table = new ushort[(1<<LZX_ALIGNED_TABLEBITS) + (LZX_ALIGNED_MAXSYMBOLS<<1)],
				len = new byte[LZX_ALIGNED_MAXSYMBOLS + LZX_LENTABLE_SAFETY],
				bits = LZX_ALIGNED_TABLEBITS,
				symbols = LZX_ALIGNED_MAXSYMBOLS,
			};

			public LZXstate()
			{
			// TODO: Complete member initialization
			}
		}


		static readonly byte[] extra_bits = new byte[] {
			0,  0,  0,  0,  1,  1,  2,  2,  3,  3,  4,  4,  5,  5,  6,  6,
			7,  7,  8,  8,  9,  9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14,
			15, 15, 16, 16, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17,
			17, 17, 17
		};

		//00 00 00 00 01 01 02 02 03 03 04 04 05 05 06 06 07 07 08 08 09 09 0A 0A 0B 0B 0C 0C 0D 0D 0E 0E 0F 0F 10 10 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11

		static readonly uint[] position_base = new uint[] {
		#if false
		0x00000000, 0x00000001, 0x00000002, 0x00000004, 0x00000006, 0x0000000A, 0x0000000E, 0x00000016,
		0x0000001E, 0x0000002E, 0x0000003E, 0x0000005E, 0x0000007E, 0x000000BE, 0x000000FE, 0x0000017E,
		0x000001FE, 0x000002FE, 0x000003FE, 0x000005FE, 0x000007FE, 0x00000BFE, 0x00000FFE, 0x000017FE,
		0x00001FFE, 0x00002FFE, 0x00003FFE, 0x00005FFE, 0x00007FFE, 0x0000BFFE, 0x0000FFFE, 0x00017FFE,
		0x0001FFFE, 0x0002FFFE, 0x0003FFFE, 0x0005FFFE, 0x0007FFFE, 0x0009FFFE, 0x000BFFFE, 0x000DFFFE,
		0x000FFFFE, 0x0011FFFE, 0x0013FFFE, 0x0015FFFE, 0x0017FFFE, 0x0019FFFE, 0x001BFFFE, 0x001DFFFE,
		0x001FFFFE,
		#else
				0,       1,       2,      3,      4,      6,      8,     12,     16,     24,     32,       48,      64,      96,     128,     192,
			256,     384,     512,    768,   1024,   1536,   2048,   3072,   4096,   6144,   8192,    12288,   16384,   24576,   32768,   49152,
			65536,   98304,  131072, 196608, 262144, 393216, 524288, 655360, 786432, 917504, 1048576, 1179648, 1310720, 1441792, 1572864, 1703936,
		1835008, 1966080, 2097152
		#endif
		};

		static public LZXstate LZXinit(int window)
		{
			LZXstate pState = null;
			uint wndsize = (uint)(1 << window);
			int i, posn_slots;

			// LZX supports window sizes of 2^15 (32Kb) through 2^21 (2Mb)
			// if a previously allocated window is big enough, keep it    
			if (window < 15 || window > 21) return null;

			// allocate state and associated window
			pState = new LZXstate();
			pState.window = new byte[wndsize];
			pState.actual_size = wndsize;
			pState.window_size = wndsize;

			// calculate required position slots
			if (window == 20) posn_slots = 42;
			else if (window == 21) posn_slots = 50;
			else posn_slots = window << 1;

			// alternatively
			// posn_slots=i=0; while (i < wndsize) i += 1 << extra_bits[posn_slots++];

			// initialize other state
			pState.R0  =  pState.R1  = pState.R2 = 1;
			pState.main_elements   = (ushort)(LZX_NUM_CHARS + (posn_slots << 3));
			pState.header_read     = 0;
			pState.frames_read     = 0;
			pState.block_remaining = 0;
			pState.block_type      = LZX_BLOCKTYPE_INVALID;
			pState.intel_curpos    = 0;
			pState.intel_started   = false;
			pState.window_posn     = 0;

			// initialise tables to 0 (because deltas will be applied to them)
			for (i = 0; i < LZX_MAINTREE_MAXSYMBOLS; i++) pState.MAINTREE.len[i] = 0;
			for (i = 0; i < LZX_LENGTH_MAXSYMBOLS; i++)   pState.LENGTH.len[i]   = 0;

			return pState;
		}

		static public void LZXteardown(LZXstate pState)
		{
			/*
			if (pState)
			{
				free(pState.window);
				free(pState);
			}
			*/
		}

		static public int LZXreset(LZXstate pState)
		{
			int i;

			pState.R0  =  pState.R1  = pState.R2 = 1;
			pState.header_read     = 0;
			pState.frames_read     = 0;
			pState.block_remaining = 0;
			pState.block_type      = LZX_BLOCKTYPE_INVALID;
			pState.intel_curpos    = 0;
			pState.intel_started   = false;
			pState.window_posn     = 0;

			for (i = 0; i < LZX_MAINTREE_MAXSYMBOLS + LZX_LENTABLE_SAFETY; i++) pState.MAINTREE.len[i] = 0;
			for (i = 0; i < LZX_LENGTH_MAXSYMBOLS + LZX_LENTABLE_SAFETY; i++)   pState.LENGTH.len[i]   = 0;

			return DECR_OK;
		}


		public const uint UWORD_BITS = 32;

		static public void ENSURE_BITS(ref int bitsleft, ref byte* inpos, ref uint bitbuf, int n)
		{
			while (bitsleft < (n)) {
				bitbuf |= (uint)(((inpos[1]<<8)|inpos[0]) << (int)(UWORD_BITS-16 - bitsleft));
				bitsleft += 16; inpos+=2;
			}
		}

		static public uint PEEK_BITS(ref int bitsleft, ref byte* inpos, ref uint bitbuf, int n)
		{
			return (bitbuf >> (int)(UWORD_BITS - (n)));
		}

		static public void REMOVE_BITS(ref int bitsleft, ref byte* inpos, ref uint bitbuf, int n)
		{
			bitbuf <<= (n);
			bitsleft -= (n);
		}

		static public void READ_BITS(ref int bitsleft, ref byte* inpos, ref uint bitbuf, ref uint v, int n)
		{
			ENSURE_BITS(ref bitsleft, ref inpos, ref bitbuf, n);
			(v) = PEEK_BITS(ref bitsleft, ref inpos, ref bitbuf, n);
			REMOVE_BITS(ref bitsleft, ref inpos, ref bitbuf, n);
		}

		static public void READ_BITS(ref int bitsleft, ref byte* inpos, ref uint bitbuf, ref int v, int n)
		{
			ENSURE_BITS(ref bitsleft, ref inpos, ref bitbuf, n);
			(v) = (int)PEEK_BITS(ref bitsleft, ref inpos, ref bitbuf, n);
			REMOVE_BITS(ref bitsleft, ref inpos, ref bitbuf, n);
		}

		static public ushort[] SYMTABLE(Table tbl)
		{
			return tbl.table;
		}

		static public byte[] LENTABLE(Table tbl)
		{
			return tbl.len;
		}

		static public uint MAXSYMBOLS(Table tbl)
		{
			return tbl.symbols;
		}

		static public uint TABLEBITS(Table tbl)
		{
			return tbl.bits;
		}

		static public void BUILD_TABLE(Table tbl)
		{
			if (make_decode_table(
				MAXSYMBOLS(tbl),
				TABLEBITS(tbl),
				LENTABLE(tbl),
				SYMTABLE(tbl)
			)) {
				throw(new InvalidDataException());
			}
		}

		static public void READ_HUFFSYM(out ushort[] hufftbl, ref int bitsleft, ref byte* inpos, ref uint bitbuf, ref uint i, ref uint j, Table tbl, out int var)
		{
			ENSURE_BITS(ref bitsleft, ref inpos, ref bitbuf, 16);
			hufftbl = SYMTABLE(tbl);
			if ((i = hufftbl[PEEK_BITS(ref bitsleft, ref inpos, ref bitbuf, (int)TABLEBITS(tbl))]) >= MAXSYMBOLS(tbl))
			{ 
				j = (uint)(1 << (int)(UWORD_BITS - TABLEBITS(tbl)));
				do {
					j >>= 1;
					i <<= 1;
					i |= (((bitbuf & j) != 0) ? (uint)1 : (uint)0);
					if (j == 0) {
						throw (new InvalidDataException());
					}
				} while ((i = hufftbl[i]) >= MAXSYMBOLS(tbl));                  
			}                                                               
			j = LENTABLE(tbl)[(var) = (int)i];                                   
			REMOVE_BITS(ref bitsleft, ref inpos, ref bitbuf, (int)j);                                                 
		}

		static public void READ_LENGTHS(LZXstate pState, ref uint bitbuf, ref int bitsleft, ref byte* inpos, ref lzx_bits lb, Table tbl, uint first, uint last)
		{
			fixed (lzx_bits* lbp = &lb)
			{
				lb.bb = bitbuf;
				lb.bl = bitsleft;
				lb.ip = inpos;
				if (lzx_read_lens(pState, LENTABLE(tbl), (first), (last), lbp))
				{
					throw (new InvalidDataException());
				}
				bitbuf = lb.bb;
				bitsleft = lb.bl;
				inpos = lb.ip;
			}
		}

		static bool make_decode_table(uint nsyms, uint nbits, byte[] length, ushort[] table)
		{
			ushort sym;
			uint leaf;
			byte bit_num = 1;
			uint fill;
			uint pos         = 0; // the current position in the decode table
			uint table_mask  = (uint)(1 << (int)nbits);
			uint bit_mask    = table_mask >> 1; // don't do 0 length codes
			uint next_symbol = bit_mask; // base of allocation for int codes

			// fill entries for codes short enough for a direct mapping
			while (bit_num <= nbits) {
				for (sym = 0; sym < nsyms; sym++) {
					if (length[sym] == bit_num) {
						leaf = pos;

						if ((pos += bit_mask) > table_mask) return true; // table overrun

						// fill all possible lookups of this symbol with the symbol itself
						fill = bit_mask;
						while (fill-- > 0) table[leaf++] = sym;
					}
				}
				bit_mask >>= 1;
				bit_num++;
			}

			// if there are any codes inter than nbits
			if (pos != table_mask) {
				// clear the remainder of the table
				for (sym = (ushort)pos; sym < table_mask; sym++) table[sym] = 0;

				// give ourselves room for codes to grow by up to 16 more bits
				pos <<= 16;
				table_mask <<= 16;
				bit_mask = 1 << 15;

				while (bit_num <= 16) {
					for (sym = 0; sym < nsyms; sym++) {
						if (length[sym] == bit_num) {
							leaf = pos >> 16;
							for (fill = 0; fill < bit_num - nbits; fill++) {
								// if this path hasn't been taken yet, 'allocate' two entries
								if (table[leaf] == 0) {
									table[(next_symbol << 1)] = 0;
									table[(next_symbol << 1) + 1] = 0;
									table[leaf] = (ushort)(next_symbol++);
								}
								// follow the path and select either left or right for next bit
								leaf = (uint)(table[leaf] << 1);
								if (((pos >> (int)(15-fill)) & 1) != 0) leaf++;
							}
							table[leaf] = sym;

							if ((pos += bit_mask) > table_mask) return true; /* table overflow */
						}
					}
					bit_mask >>= 1;
					bit_num++;
				}
			}

			// full table?
			if (pos == table_mask) return false;

			// either erroneous table, or all elements are 0 - let's find out.
			for (sym = 0; sym < nsyms; sym++) if (length[sym] != 0) return true;
			return false;
		}

		public struct lzx_bits {
			public uint bb;
			public int bl;
			public byte *ip;
		}

		static bool lzx_read_lens(LZXstate pState, byte[] lens, uint first, uint last, lzx_bits *lb)
		{
			uint i = 0,j = 0, x = 0, y = 0;
			int z = 0;

			uint bitbuf = lb->bb;
			int bitsleft = lb->bl;
			byte *inpos = lb->ip;
			ushort[] hufftbl;

			for (x = 0; x < 20; x++) {
				READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref y, 4);
				LENTABLE(pState.PRETREE)[x] = (byte)y;
			}
			BUILD_TABLE(pState.PRETREE);

			for (x = first; x < last; ) {
				// static public void READ_HUFFSYM(
				READ_HUFFSYM(out hufftbl, ref bitsleft, ref inpos, ref bitbuf, ref i, ref j, pState.PRETREE, out z);
				if (z == 17) {
					READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref y, 4); y += 4;
					while (y-- > 0) lens[x++] = 0;
				}
				else if (z == 18) {
					READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref y, 5); y += 20;
					while (y-- > 0) lens[x++] = 0;
				}
				else if (z == 19) {
					READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref y, 1); y += 4;
					READ_HUFFSYM(out hufftbl, ref bitsleft, ref inpos, ref bitbuf, ref i, ref j, pState.PRETREE, out z);
					z = lens[x] - z; if (z < 0) z += 17;
					while (y-- > 0) lens[x++] = (byte)z;
				}
				else {
					z = lens[x] - z; if (z < 0) z += 17;
					lens[x++] = (byte)z;
				}
			}

			lb->bb = bitbuf;
			lb->bl = bitsleft;
			lb->ip = inpos;
			return false;
		}

		static public int LZXdecompress(LZXstate pState, byte *inpos, byte *outpos, int inlen, int outlen)
		{
			fixed (byte* window = pState.window)
			{
				byte* endinp = inpos + inlen;
				byte* runsrc;
				byte* rundest;
				ushort[] hufftbl; // used in READ_HUFFSYM macro as chosen decoding table

				uint window_posn = pState.window_posn;
				uint window_size = pState.window_size;
				uint R0 = pState.R0;
				uint R1 = pState.R1;
				uint R2 = pState.R2;

				uint bitbuf;
				int bitsleft;
				uint match_offset, i = 0, j = 0, k = 0; /* ijk used in READ_HUFFSYM macro */
				var lb = default(lzx_bits); /* used in READ_LENGTHS macro */

				int togo = outlen, this_run, main_element, aligned_bits;
				int match_length, length_footer, extra, verbatim_bits = 0;
				int copy_length;

				//INIT_BITSTREAM();
				bitsleft = 0;
				bitbuf = 0;

				// read header if necessary
				if (pState.header_read == 0)
				{
					i = j = 0;
					READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref k, 1);
					if (k != 0)
					{
						READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref i, 16);
						READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref j, 16);
					}
					pState.intel_filesize = (int)((i << 16) | j); // or 0 if not encoded
					pState.header_read = 1;
				}

				// main decoding loop
				while (togo > 0)
				{
					// last block finished, new block expected
					if (pState.block_remaining == 0)
					{
						READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref pState.block_type, 3);
						READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref i, 16);
						READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref j, 8);
						pState.block_remaining = pState.block_length = (i << 8) | j;

						switch (pState.block_type)
						{
							case LZX_BLOCKTYPE_ALIGNED:
								for (i = 0; i < 8; i++)
								{
									READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref j, 3);
									LENTABLE(pState.ALIGNED)[i] = (byte)j;
								}
								BUILD_TABLE(pState.ALIGNED);
								/* rest of aligned header is same as verbatim */
								goto case LZX_BLOCKTYPE_VERBATIM;

							case LZX_BLOCKTYPE_VERBATIM:
								READ_LENGTHS(pState, ref bitbuf, ref bitsleft, ref inpos, ref lb, pState.MAINTREE, 0, 256);
								READ_LENGTHS(pState, ref bitbuf, ref bitsleft, ref inpos, ref lb, pState.MAINTREE, 256, pState.main_elements);
								BUILD_TABLE(pState.MAINTREE);
								if (LENTABLE(pState.MAINTREE)[0xE8] != 0) pState.intel_started = true;

								READ_LENGTHS(pState, ref bitbuf, ref bitsleft, ref inpos, ref lb, pState.LENGTH, 0, LZX_NUM_SECONDARY_LENGTHS);
								BUILD_TABLE(pState.LENGTH);
								break;

							case LZX_BLOCKTYPE_UNCOMPRESSED:
								pState.intel_started = true; /* because we can't assume otherwise */
								ENSURE_BITS(ref bitsleft, ref inpos, ref bitbuf, 16); // get up to 16 pad bits into the buffer
								if (bitsleft > 16) inpos -= 2; /* and align the bitstream! */
								R0 = (uint)(inpos[0] | (inpos[1] << 8) | (inpos[2] << 16) | (inpos[3] << 24)); inpos += 4;
								R1 = (uint)(inpos[0] | (inpos[1] << 8) | (inpos[2] << 16) | (inpos[3] << 24)); inpos += 4;
								R2 = (uint)(inpos[0] | (inpos[1] << 8) | (inpos[2] << 16) | (inpos[3] << 24)); inpos += 4;
								break;

							default:
								return DECR_ILLEGALDATA;
						}
					}

					if (inpos > endinp)
					{
						if (inpos > (endinp + 2) || bitsleft < 16) return DECR_ILLEGALDATA;
					}

					while ((this_run = (int)pState.block_remaining) > 0 && togo > 0)
					{
						if (this_run > togo) this_run = togo;
						togo -= this_run;
						pState.block_remaining -= (uint)this_run;

						/* apply 2^x-1 mask */
						window_posn &= window_size - 1;
						/* runs can't straddle the window wraparound */
						if ((window_posn + this_run) > window_size)
							return DECR_DATAFORMAT;

						switch (pState.block_type)
						{

							case LZX_BLOCKTYPE_VERBATIM:
								while (this_run > 0)
								{
									READ_HUFFSYM(out hufftbl, ref bitsleft, ref inpos, ref bitbuf, ref i, ref j, pState.MAINTREE, out main_element);

									if (main_element < LZX_NUM_CHARS)
									{
										/* literal: 0 to LZX_NUM_CHARS-1 */
										window[window_posn++] = (byte)main_element;
										this_run--;
									}
									else
									{
										/* match: LZX_NUM_CHARS + ((slot<<3) | length_header (3 bits)) */
										main_element -= LZX_NUM_CHARS;

										match_length = main_element & LZX_NUM_PRIMARY_LENGTHS;
										if (match_length == LZX_NUM_PRIMARY_LENGTHS)
										{
											READ_HUFFSYM(out hufftbl, ref bitsleft, ref inpos, ref bitbuf, ref i, ref j, pState.LENGTH, out length_footer);
											match_length += length_footer;
										}
										match_length += LZX_MIN_MATCH;

										match_offset = (uint)(main_element >> 3);

										if (match_offset > 2)
										{
											/* not repeated offset */
											if (match_offset != 3)
											{
												extra = extra_bits[match_offset];
												READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref verbatim_bits, extra);
												match_offset = (uint)(position_base[match_offset] - 2 + verbatim_bits);
											}
											else
											{
												match_offset = 1;
											}

											/* update repeated offset LRU queue */
											R2 = R1; R1 = R0; R0 = match_offset;
										}
										else if (match_offset == 0)
										{
											match_offset = R0;
										}
										else if (match_offset == 1)
										{
											match_offset = R1;
											R1 = R0; R0 = match_offset;
										}
										else /* match_offset == 2 */
										{
											match_offset = R2;
											R2 = R0; R0 = match_offset;
										}

										rundest = window + window_posn;
										this_run -= match_length;

										/* copy any wrapped around source data */
										if (window_posn >= match_offset)
										{
											/* no wrap */
											runsrc = rundest - match_offset;
										}
										else
										{
											runsrc = rundest + (window_size - match_offset);
											copy_length = (int)(match_offset - window_posn);
											if (copy_length < match_length)
											{
												match_length -= copy_length;
												window_posn += (uint)copy_length;
												while (copy_length-- > 0) *rundest++ = *runsrc++;
												runsrc = window;
											}
										}
										window_posn += (uint)match_length;

										/* copy match data - no worries about destination wraps */
										while (match_length-- > 0) *rundest++ = *runsrc++;

									}
								}
								break;

							case LZX_BLOCKTYPE_ALIGNED:
								while (this_run > 0)
								{
									READ_HUFFSYM(out hufftbl, ref bitsleft, ref inpos, ref bitbuf, ref i, ref j, pState.MAINTREE, out main_element);

									if (main_element < LZX_NUM_CHARS)
									{
										/* literal: 0 to LZX_NUM_CHARS-1 */
										window[window_posn++] = (byte)main_element;
										this_run--;
									}
									else
									{
										/* match: LZX_NUM_CHARS + ((slot<<3) | length_header (3 bits)) */
										main_element -= LZX_NUM_CHARS;

										match_length = main_element & LZX_NUM_PRIMARY_LENGTHS;
										if (match_length == LZX_NUM_PRIMARY_LENGTHS)
										{
											READ_HUFFSYM(out hufftbl, ref bitsleft, ref inpos, ref bitbuf, ref i, ref j, pState.LENGTH, out length_footer);
											match_length += length_footer;
										}
										match_length += LZX_MIN_MATCH;

										match_offset = (uint)(main_element >> 3);

										if (match_offset > 2)
										{
											/* not repeated offset */
											extra = extra_bits[match_offset];
											match_offset = position_base[match_offset] - 2;
											if (extra > 3)
											{
												/* verbatim and aligned bits */
												extra -= 3;
												READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref verbatim_bits, extra);
												match_offset += (uint)(verbatim_bits << 3);

												READ_HUFFSYM(out hufftbl, ref bitsleft, ref inpos, ref bitbuf, ref i, ref j, pState.ALIGNED, out aligned_bits);
												match_offset += (uint)aligned_bits;
											}
											else if (extra == 3)
											{
												/* aligned bits only */
												READ_HUFFSYM(out hufftbl, ref bitsleft, ref inpos, ref bitbuf, ref i, ref j, pState.ALIGNED, out aligned_bits);
												match_offset += (uint)aligned_bits;
											}
											else if (extra > 0)
											{ /* extra==1, extra==2 */
												/* verbatim bits only */
												READ_BITS(ref bitsleft, ref inpos, ref bitbuf, ref verbatim_bits, extra);
												match_offset += (uint)verbatim_bits;
											}
											else /* extra == 0 */
											{
												/* ??? */
												match_offset = 1;
											}

											/* update repeated offset LRU queue */
											R2 = R1; R1 = R0; R0 = match_offset;
										}
										else if (match_offset == 0)
										{
											match_offset = R0;
										}
										else if (match_offset == 1)
										{
											match_offset = R1;
											R1 = R0; R0 = match_offset;
										}
										else /* match_offset == 2 */
										{
											match_offset = R2;
											R2 = R0; R0 = match_offset;
										}

										rundest = window + window_posn;
										this_run -= match_length;

										/* copy any wrapped around source data */
										if (window_posn >= match_offset)
										{
											/* no wrap */
											runsrc = rundest - match_offset;
										}
										else
										{
											runsrc = rundest + (window_size - match_offset);
											copy_length = (int)(match_offset - window_posn);
											if (copy_length < match_length)
											{
												match_length -= copy_length;
												window_posn += (uint)copy_length;
												while (copy_length-- > 0) *rundest++ = *runsrc++;
												runsrc = window;
											}
										}
										window_posn += (uint)match_length;

										/* copy match data - no worries about destination wraps */
										while (match_length-- > 0) *rundest++ = *runsrc++;

									}
								}
								break;

							case LZX_BLOCKTYPE_UNCOMPRESSED:
								if ((inpos + this_run) > endinp) return DECR_ILLEGALDATA;
								PointerUtils.Memcpy(window + window_posn, inpos, (int)this_run);
								inpos += this_run; window_posn += (uint)this_run;
								break;

							default:
								return DECR_ILLEGALDATA; /* might as well */
						}

					}
				}

				if (togo != 0) return DECR_ILLEGALDATA;
				PointerUtils.Memcpy(outpos, window + ((window_posn == 0) ? window_size : window_posn) - outlen, (int)outlen);

				pState.window_posn = window_posn;
				pState.R0 = R0;
				pState.R1 = R1;
				pState.R2 = R2;

				/* intel E8 decoding */
				if ((pState.frames_read++ < 32768) && pState.intel_filesize != 0)
				{
					if (outlen <= 6 || !pState.intel_started)
					{
						pState.intel_curpos += outlen;
					}
					else
					{
						byte* data = outpos;
						byte* dataend = data + outlen - 10;
						int curpos = pState.intel_curpos;
						int filesize = pState.intel_filesize;
						int abs_off, rel_off;

						pState.intel_curpos = curpos + outlen;

						while (data < dataend)
						{
							if (*data++ != 0xE8) { curpos++; continue; }
							abs_off = data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
							if ((abs_off >= -curpos) && (abs_off < filesize))
							{
								rel_off = (abs_off >= 0) ? abs_off - curpos : abs_off + filesize;
								data[0] = (byte)rel_off;
								data[1] = (byte)(rel_off >> 8);
								data[2] = (byte)(rel_off >> 16);
								data[3] = (byte)(rel_off >> 24);
							}
							data += 4;
							curpos += 5;
						}
					}
				}
				return DECR_OK;
			}
		}
	}
}
