#include "xgraphics_test.h"

void test_xy() {
	int n;
	int width = 16;
	int height = 15;
	int depth = 1;
	int texelpitch = 16;
	for (n = 0; n < 256; n++) {
		int x = XGAddress2DTiledX(n, width, texelpitch);
		int y = XGAddress2DTiledY(n, width, texelpitch);
		printf("n=%d: (x=%d, y=%d)\n", n, x, y);
	}
}

void test_offset() {
	int x, y, n;
	int width = 16;
	int height = 15;
	int depth = 1;
	int texelpitch = 16;
	
	printf("size: %d\n", XGAddress2DTiledExtent(width, height, texelpitch));

	for (y = 0; y < 16; y++) {
		for (x = 0; x < 16; x++) {
			n = XGAddress2DTiledOffset(x, y, width, texelpitch);
			printf("(x=%d, y=%d): n=%d\n", x, y, n);
		}
	}
}

int main(char** argv, int argc) {
	//test_xy();
	test_offset();
	return 0;
}