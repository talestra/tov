#define TRACE_COMPRESSION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;
using System.Diagnostics;

namespace TalesOfVesperiaUtils.Compression.C
{
	public unsafe partial class complib
	{
		/*
		#define cleanup(err) { error = err; goto _cleanup; }
		//#define get(c) { if (insp >= inst) cleanup(SUCCESS); c = *(insp++); }
		#define put2(c) { if (ousp >= oust) cleanup(SUCCESS); *(ousp++) = c; }
		#define put(c) { put2(c); State.text_buf[r++] = c; r &= (N - 1); }
		#define get(c) { if (insp >= inst) break; c = *(insp++); }
		//#define put(c) { if (ousp >= oust) break; *(ousp++) = c; State.text_buf[r++] = c; r &= (N - 1); }

		FILE *profilef;
		*/

		public const int SUCCESS               =  0;
		public const int ERROR_FILE_IN = -1;
		public const int ERROR_FILE_OUT = -2;
		public const int ERROR_MALLOC = -3;
		public const int ERROR_BAD_INPUT = -4;
		public const int ERROR_UNKNOWN_VERSION = -5;
		public const int ERROR_FILES_MISMATCH = -6;

		public const int WindowSize = 0x1000;
		public const int NIL = WindowSize;
		public const int MF = 0x12;
		public const int MAX_DUP = (0x100 + 0x12);

		unsafe public class LzState
		{
			public int F, T;
			public int textsize, codesize, printcount;
			public byte[] Window = new byte[WindowSize + MF - 1];
			public int match_position;
			public int match_length;
			public int[] lson = new int[WindowSize + 1];
			public int[] rson = new int[WindowSize + 257];
			public int[] dad = new int[WindowSize + 1];
		}

		public static LzState LzStateCreate()
		{
			var State = new LzState();
			State.textsize = 0;
			State.codesize = 0;
			State.printcount = 0;
			//profilef = NULL;
			return State;
		}

		public static void LzStateDelete(LzState State)
		{
			//free(State);
		}

		[DebuggerHidden()]
		public static void ThrowError(int error)
		{
			throw(new Exception("Error compressing : " + GetErrorString(error)));
		}

		public static string GetErrorString(int error)
		{
			switch (error) {
				case SUCCESS:               return "Success";
				case ERROR_FILE_IN:         return "Error with input file";
				case ERROR_FILE_OUT:        return "Error with output file";
				case ERROR_MALLOC:          return "Malloc failure";
				case ERROR_BAD_INPUT:       return "Bad Input";
				case ERROR_UNKNOWN_VERSION: return "Unknown version";
				case ERROR_FILES_MISMATCH:  return "Mismatch";
				default:                    return "Unknown error";
			}
		}

		public static void FillTextBuffer(LzState State)
		{
			fixed (byte* text_buf = State.Window)
			{
				int n, p;
				for (n = 0, p = 0; n != 0x100; n++, p += 8) { text_buf[p + 6] = text_buf[p + 4] = text_buf[p + 2] = text_buf[p + 0] = (byte)n; text_buf[p + 7] = text_buf[p + 5] = text_buf[p + 3] = text_buf[p + 1] = 0; }
				for (n = 0; n != 0x100; n++, p += 7) { text_buf[p + 6] = text_buf[p + 4] = text_buf[p + 2] = text_buf[p + 0] = (byte)n; text_buf[p + 5] = text_buf[p + 3] = text_buf[p + 1] = 0xff; }
				while (p != WindowSize) text_buf[p++] = 0;
			}
		}

		public static void PrepareVersion(LzState State, int version)
		{
			if (State != null) State.T = 2;
			switch (version) {
				case 1:  if (State != null) State.F  = 0x12; break;
				case 3:  if (State != null) State.F  = 0x11; break;
				default: ThrowError(ERROR_UNKNOWN_VERSION); break;
			}
		}

		public static void InitTree(LzState State)
		{
			fixed (int* rson = State.rson)
			fixed (int* dad = State.dad)
			{
				int i;
				for (i = WindowSize + 1; i <= WindowSize + 256; i++) rson[i] = NIL;
				for (i = 0; i < WindowSize; i++) dad[i] = NIL;
			}
		}

		public static void InsertNode(LzState State, int r)
		{
			int  i, p, cmp;
			byte  *key;

			fixed (byte* text_buf = State.Window)
			fixed (int* lson = State.rson)
			fixed (int* rson = State.rson)
			fixed (int* dad = State.dad)
			{
				cmp = 1; key = &text_buf[r]; p = WindowSize + 1 + key[0];
				rson[r] = lson[r] = NIL; State.match_length = 0;

				while (true) {
					if (cmp >= 0) {
						if (rson[p] != NIL) p = rson[p];
						else { rson[p] = r; dad[r] = p; return; }
					} else {
						if (lson[p] != NIL) p = lson[p];
						else { lson[p] = r; dad[r] = p; return; }
					}

					for (i = 1; i < State.F; i++) if ((cmp = key[i] - text_buf[p + i]) != 0) break;

					if (i > State.match_length) {
						State.match_position = p;
						if ((State.match_length = i) >= State.F) break;
					}
				}

				dad[r] = dad[p]; lson[r] = lson[p]; rson[r] = rson[p];
				dad[lson[p]] = r; dad[rson[p]] = r;

				if (rson[dad[p]] == p) rson[dad[p]] = r; else lson[dad[p]] = r;

				dad[p] = NIL;
			}
		}

		public static void DeleteNode(LzState State, int p)
		{
			int  q;

			fixed (byte* text_buf = State.Window)
			fixed (int* lson = State.rson)
			fixed (int* rson = State.rson)
			fixed (int* dad = State.dad)
			{
				if (dad[p] == NIL) return;

				if (rson[p] == NIL) q = lson[p];
				else if (lson[p] == NIL) q = rson[p];
				else {
					q = lson[p];
					if (rson[q] != NIL) {
						do { q = rson[q]; } while (rson[q] != NIL);
						rson[dad[q]] = lson[q]; dad[lson[q]] = dad[q];
						lson[q] = lson[p]; dad[lson[p]] = q;
					}
					rson[q] = rson[p]; dad[rson[p]] = q;
				}
				dad[q] = dad[p];

				if (rson[dad[p]] == p) rson[dad[p]] = q; else lson[dad[p]] = q;
				dad[p] = NIL;
			}
		}

		public static byte[] Encode(int version, byte[] In)
		{
			var Out = new byte[((In.Length * 9) / 8 + 1)];
			fixed (byte* InPointer = In)
			fixed (byte* OutPointer = Out)
			{
				int outl = Out.Length;
				int result = Encode(version, InPointer, In.Length, OutPointer, ref outl);
				if (result != SUCCESS)
				{
					throw(new Exception("Error! : " + GetErrorString(result)));
				}
				Out = Out.Slice(0, (int)outl);
			}
			return Out;
		}

		public static int Encode(int version, void* In, int inl, void* Out, ref int outl)
		{
			var State = LzStateCreate();
			fixed (byte* text_buf = State.Window)
			{
				byte *insp; byte *inst; byte *ousp; byte *oust; byte *inspb; byte *insplb;
				int i, c, len = 0, r, s, last_match_length, dup_match_length = 0, code_buf_ptr, dup_last_match_length = 0;
				var code_buf = new byte[1 + 8 * 5];
				byte mask;
				int error = SUCCESS;

				/*
				#define cleanup(err) { error = err; goto _cleanup; }
				#define put2(c) { if (ousp >= oust) cleanup(SUCCESS); *(ousp++) = c; }
				#define put(c) { put2(c); State.text_buf[r++] = c; r &= (N - 1); }
				#define get(c) { if (insp >= inst) break; c = *(insp++); }
				*/
				/*
				Action<int> put2 = (int cc) => {
					if (ousp >= oust) cleanup(SUCCESS); *(ousp++) = c;
				};
				Action<int> put = (int cc) => {
					fixed (byte* text_buf2 = State.text_buf)
					{
						put2(cc); text_buf2[r++] = (byte)cc; r &= (N - 1);
					}
				};
				*/

				inst = (insplb = inspb = insp = (byte *)In) + inl; oust = (ousp = (byte *)Out) + outl;
	
				if (version == 0)
				{
					while (true)
					{
						int left = (int)(inst - insp);
						int left8 = left;
						//printf("left8:%d\n", left8);
						if (left8 > 8) left8 = 8;
						//put2(((1 << left8) - 1) << (8 - left8));
						if (ousp >= oust) { error = SUCCESS; goto _cleanup; }
						*(ousp++) = (byte)(((1 << left8) - 1) << (0));

						for (i = 0; i < 8; i++)
						{
							if (insp >= inst) break; c = *(insp++);
							if (ousp >= oust) { error = SUCCESS; goto _cleanup; }
							*(ousp++) = (byte)(c);
						}

						if (insp >= inst) break;
					}

					if (insp != inst)
					{
						LzStateDelete(State);
						Console.Error.WriteLine("(insp != inst) ({0} != {1})\n", (uint)insp, (uint)inst);
						ThrowError(ERROR_BAD_INPUT);
					}

					outl = (int)(ousp - (byte *)Out);

					LzStateDelete(State);
					return error;
				}
	
				FillTextBuffer(State);
				PrepareVersion(State, version);
				InitTree(State);

				code_buf[0] = 0x00;
				code_buf_ptr = mask = 1;
				s = 0; r = WindowSize - State.F;

				//printf("%d\n", r);

				for (len = 0; len < State.F; len++)
				{
					if (insp >= inst) break; c = *(insp++);
					text_buf[r + len] = (byte)c;
				}

				if ((State.textsize = len) == 0) return SUCCESS;

				for (i = 1; i <= State.F; i++) InsertNode(State, r - i);
				InsertNode(State, r);

				do
				{
					if (version >= 3)
					{
						if (insplb - inspb <= 0)
						{
							insplb = inspb + 1;
							while ((insplb < inst) && (*insplb == *inspb)) insplb++;
						}

						dup_match_length = (int)(insplb - inspb);
					}

					if (State.match_length > len) State.match_length = len;

					if (version >= 3 && dup_match_length > MAX_DUP) dup_match_length = MAX_DUP;

					if (version >= 3 && dup_match_length > (State.T + 1) && dup_match_length >= State.match_length)
					{
						if (dup_match_length >= (inst - insp)) dup_match_length--;
					}
					else
					{
						if (State.match_length >= (inst - insp)) State.match_length--;
					}
					/*
					if (version >= 3 && dup_match_length > (State.T + 1) && dup_match_length >= State.match_length) {
						if (dup_match_length >= (inst - insp)) dup_match_length -= 8;
					} else {
						if (State.match_length >= (inst - insp)) State.match_length -= 8;
					}
					*/

					if (version >= 3 && dup_match_length > (State.T + 1) && dup_match_length >= State.match_length)
					{
						State.match_length = dup_match_length;
						State.match_position = r;

						if (State.match_length <= 0x12)
						{
							code_buf[code_buf_ptr++] = text_buf[r];
							code_buf[code_buf_ptr++] = (byte)(0x0f | (((State.match_length - (State.T + 1)) & 0xf) << 4));
						}
						else
						{
							code_buf[code_buf_ptr++] = (byte)(State.match_length - 0x13);
							code_buf[code_buf_ptr++] = 0x0f;
							code_buf[code_buf_ptr++] = text_buf[r];
						}
					}
					else if (State.match_length > State.T)
					{
						code_buf[code_buf_ptr++] = (byte)State.match_position;
						code_buf[code_buf_ptr++] = (byte)(((State.match_position >> 4) & 0xf0) | ((State.match_length - (State.T + 1)) & 0x0f));
					}
					else
					{
						code_buf[0] |= mask;
						State.match_length = 1;
						code_buf[code_buf_ptr++] = text_buf[r];
					}

					if ((mask <<= 1) == 0) {
						for (i = 0; i < code_buf_ptr; i++) {
							if (ousp >= oust) { error = SUCCESS; goto _cleanup; }
							*(ousp++) = (byte)(code_buf[i]);
						}
						State.codesize += code_buf_ptr;
						code_buf[0] = 0x00; code_buf_ptr = mask = 1;
					}

					last_match_length = State.match_length;
					for (i = 0; i < last_match_length; i++)
					{
						if (insp >= inst) break;
						c = *(insp++);
						DeleteNode(State, s); text_buf[s] = (byte)c;

						if (s < State.F - 1) text_buf[s + WindowSize] = (byte)c;

						s = (s + 1) & (WindowSize - 1);  r = (r + 1) & (WindowSize - 1);
						inspb++;
						InsertNode(State, r);
					}

					State.textsize += i;

					while (i++ < last_match_length) {
						DeleteNode(State, s); s = (s + 1) & (WindowSize - 1); r = (r + 1) & (WindowSize - 1);
						inspb++;
						if (--len > 0) InsertNode(State, r);
					}
				} while (len > 0);

				if (code_buf_ptr > 1) {
					for (i = 0; i < code_buf_ptr; i++) {
						if (ousp >= oust) { error = SUCCESS; goto _cleanup; }
						*(ousp++) = (byte)(code_buf[i]);
					}
					State.codesize += code_buf_ptr;
				}

			_cleanup:

				if (State == null)
				{
					ThrowError(ERROR_BAD_INPUT);
				}

				outl = (int)(ousp - (byte *)Out);
				LzStateDelete(State);

				if (insp != inst)
				{
					Console.Error.WriteLine("(insp != inst) ({0} != {1})\n", (uint)insp, (uint)inst);
					ThrowError(ERROR_BAD_INPUT);
				}

				return SUCCESS;
			}
		}
	}
}
