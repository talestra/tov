#include "xgraphics_test.h"

int main(char** argv, int argc) {
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
	return 0;
}