using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TalesOfVesperiaUtils.Compression.C
{
	public unsafe partial class complib
	{
		public static byte[] Decode(int version, byte[] In)
		{
			var Out = new byte[In.Length * 32];
			fixed (byte* InPointer = In)
			fixed (byte* OutPointer = Out)
			{
				int outl = Out.Length;
				int result = Decode(version, InPointer, In.Length, OutPointer, ref outl);
				if (result != SUCCESS)
				{
					throw (new Exception("Error!" + GetErrorString(result)));
				}
				Out = Out.Slice(0, (int)outl);
			}
			return Out;
		}

		static public int Decode(int version, void* In, int InputLength, void* Out, ref int OutputLength)
		{
			byte* InputCurrent;
			byte* InputEnd;
			byte* OutputCurrent;
			byte* OutputEnd;
			uint Flags = 0, i, j, k, r, Current;
			int error = SUCCESS;
			LzState State = LzStateCreate();
			/*
				#define cleanup(err) { error = err; goto _cleanup; }
				#define put2(c) { if (ousp >= oust) cleanup(SUCCESS); *(ousp++) = c; }
				#define put(c) { put2(c); State.text_buf[r++] = c; r &= (N - 1); }
				#define get(c) { if (insp >= inst) break; c = *(insp++); }
			*/

			fixed (byte* Window = State.Window)
			{
				InputEnd = (InputCurrent = (byte*)In) + InputLength;
				OutputEnd = (OutputCurrent = (byte*)Out) + OutputLength;

				FillTextBuffer(State);
				PrepareVersion(State, version);
				r = (uint)(WindowSize - State.F);

				for (; ; )
				{
					if (((Flags >>= 1) & 0x100) == 0)
					{
						if (InputCurrent >= InputEnd) break; Current = *(InputCurrent++);
						Flags = Current | 0xff00;
					}

					bool IsUncompressed = (Flags & 1) != 0;

					if (IsUncompressed)
					{
						if (InputCurrent >= InputEnd) break;

						Current = *(InputCurrent++);

						if (OutputCurrent >= OutputEnd)
						{
							error = SUCCESS;
							goto _cleanup;
						}

						Console.WriteLine("BYTE(0x{0:X2}) : r=0x{1:X3}", Current, r);
						
						*(OutputCurrent++) = (byte)Current;

						Window[r++] = (byte)Current;
						r &= (WindowSize - 1);
						continue;
					}

					if (InputCurrent + 1 >= InputEnd) break;
					i = *(InputCurrent++);
					j = *(InputCurrent++);

					i |= (j & 0xf0) << 4;
					j = (uint)((j & 0x0f) + State.T);

					if (version == 1 || j < (State.F))
					{
						Console.WriteLine("LZ(offset=0x{0:X3}, length={1}) : r=0x{2:X3}", i, j + 1, r);
						for (k = 0; k <= j; k++)
						{
							Current = Window[(i + k) % WindowSize];
							if (OutputCurrent >= OutputEnd)
							{
								error = SUCCESS;
								goto _cleanup;
							}
							*(OutputCurrent++) = (byte)Current;
							Window[r++] = (byte)Current;
							r &= (WindowSize - 1);

						}
						continue;
					}

					if (i < 0x100)
					{
						if (InputCurrent >= InputEnd) break;
						j = *(InputCurrent++);
						i += (uint)(State.F + 1);
					}
					else
					{
						j = i & 0xff;
						i = (uint)((i >> 8) + State.T);
					}

					Console.WriteLine("RLE(byte=0x{0:X2}, length={1}) : r=0x{2:X3}", (byte)j, i + 1, r);
					for (k = 0; k <= i; k++)
					{
						if (OutputCurrent >= OutputEnd)
						{
							error = SUCCESS;
							goto _cleanup;
						}
						*(OutputCurrent++) = (byte)j;
						Window[r++] = (byte)j;
						r &= (WindowSize - 1);
					}
				}

			_cleanup:

				if (InputCurrent != InputEnd)
				{
					ThrowError(ERROR_BAD_INPUT);
				}

				OutputLength = (int)(OutputCurrent - (byte*)Out);
				LzStateDelete(State);

				return error;
			}
		}

	}
}
