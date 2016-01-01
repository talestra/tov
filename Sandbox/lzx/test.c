#include <stdio.h>
#include <stdlib.h>
#ifdef _MSC_VER
# include "msstdint.h"
#else /* _MSC_VER */
# include <stdint.h>
#endif /* _MSC_VER */
#include <string.h> /* for memset on Linux */
#include <assert.h>
#include <math.h>

#include "lzxc.h"

int _lzxc_get_bytes_t(void *arg, int n, void *buf) {
	//printf("%d\n", n);
	return fread(buf, 1, n, (FILE *)arg);
}
int _lzxc_at_eof_t(void *arg) {
	return feof((FILE *)arg);
}
int _lzxc_put_bytes_t(void *arg, int n, void *buf) {
	//printf("%d\n", n);
	return fwrite(buf, 1, n, (FILE *)arg);
	//return 
}
void _lzxc_mark_frame_t(void *arg, uint32_t uncomp, uint32_t comp) {
}


void main() {
	lzxc_data *lzxd;
	lzxc_results lzxr;
	FILE *fin;
	FILE *fout;
	
	fin  = fopen("E:/isos/360/vesperia/common.svo.d/TEXTURE.DAT.u", "rb");
	fout = fopen("E:/isos/360/vesperia/common.svo.d/TEXTURE.DAT.c2", "wb");
	
	lzxc_init(&lzxd,
		17, 
		_lzxc_get_bytes_t, fin,
		_lzxc_at_eof_t,
		_lzxc_put_bytes_t, fout,
		_lzxc_mark_frame_t, NULL
	);
	
	while (!feof(fin))
	{
		printf("%d Kb      \r", ftell(fin) / 1024);
		lzxc_compress_block(lzxd, 0x8000, 0);
		fflush(fout);
	}
	
	lzxc_finish(lzxd, &lzxr);
	
	printf("len_uncompressed_input: %d\n", lzxr.len_uncompressed_input);
	printf("len_compressed_output : %d\n", lzxr.len_compressed_output);
}