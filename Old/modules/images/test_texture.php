<?php

class BitExtract {
	public $bytes = array();
	public $byte;
	public $leftBits = 0;
	
	public function __construct($bytes = null) {
		if ($bytes !== null) $this->insertBytes($bytes);
	}

	public function left() {
		return count($this->bytes) * 8 + $this->leftBits;
	}
	
	public function insertBytes($bytes) {
		if (is_string($bytes)) $bytes = array_values(unpack('C*', $bytes));
		$this->bytes = array_merge($this->bytes, $bytes);
	}
	
	public function extract($bitsToExtract) {
		if ($this->leftBits == 0) {
			$this->leftBits = 8;
			$this->byte = array_shift($this->bytes);
		}
	
		if ($bitsToExtract <= $this->leftBits) {
			$v = $this->byte & ((1 << $bitsToExtract) - 1);
			$this->byte >>= $bitsToExtract;
			$this->leftBits -= $bitsToExtract;
			return $v;
		} else {
			$left = $this->leftBits;
			$v1 = $this->extract($left);
			$v2 = $this->extract($bitsToExtract - $left);
			return $v1 | ($v2 << $left);
		}
	}

	static function simpleExtract($value, $pos, $count) {
		return ($value >> $pos) & ((1 << $count) - 1);
	}

	static function simpleExtractScale($value, $pos, $count, $scale = 1.0) {
		$mask = ((1 << $count) - 1);
		return ((($value >> $pos) & $mask) / $mask) * $scale;
	}
	
	static public function unittest() {
		$bits = new BitExtract(array(0xFF, 0x00, 0xFF));
		printf("%d\n", $bits->extract(12));
		printf("%d\n", $bits->left());
	}
}

class RGBA {
	public $r, $g, $b, $a;
	public $vv = array(0, 0, 0, 0);
	public function __construct($r = 0, $g = 0, $b = 0, $a = 0) {
		$this->r = &$this->vv[0];
		$this->g = &$this->vv[1];
		$this->b = &$this->vv[2];
		$this->a = &$this->vv[3];
		$this->r = $r;
		$this->g = $g;
		$this->b = $b;
		$this->a = $a;
	}
	static public function decode565($v) {
		$c = new RGBA;
		$c->r = (int)BitExtract::simpleExtractScale($v,  0, 5, 0xFF);
		$c->g = (int)BitExtract::simpleExtractScale($v,  5, 6, 0xFF);
		$c->b = (int)BitExtract::simpleExtractScale($v, 11, 5, 0xFF);
		$c->a = 0xFF;
		return $c;
	}
	static public function mix(RGBA $a, RGBA $b, $div, $a_c, $b_c) {
		$c = new RGBA;
		//assert(a_c + b_c == div, format("%d+%d != %d", a_c, b_c, div));
		for ($n = 0; $n < 4; $n++) {
			$c->vv[$n] = (int)((($a->vv[$n] * $a_c) + ($b->vv[$n] * $b_c)) / $div);
		}
		return $c;
	}
	public function gdColor($i) {
		return imagecolorallocatealpha($i, $this->r, $this->g, $this->b, 127 - (($this->a / 255) * 127));
	}
}

class DXT5 {
	static public function decode($data) {
		if (strlen($data) != 0x10) throw(new Exception("Not a DXT5 block"));
		$av = array_fill(0, 8, 0);
		$a  = array_values(unpack('C2', substr($data, 0, 2)));
		// Alpha table.
		if ($a[0] > $a[1]) {
			$av[0] = $a[0];
			$av[1] = $a[1];
			for ($n = 0; $n < 6; $n++) $av[$n + 2] = (int)(((6 - $n) * $a[0] + ($n + 1) * $a[1]) / 7);
		} else {
			$av[0] = $a[0];
			$av[1] = $a[1];
			$av[6] = 0;
			$av[7] = 255;
			for ($n = 0; $n < 4; $n++) $av[$n + 2] = (int)(((4 - $n) * $a[0] + ($n + 1) * $a[1]) / 5);
		}
		$c = array_values(unpack('S2', substr($data, 8, 4)));
		$cv[0] = RGBA::decode565($c[0]);
		$cv[1] = RGBA::decode565($c[1]);
		if ($c[0] > $c[1]) {
			$cv[2] = RGBA::mix($cv[0], $cv[1], 3, 2, 1);
			$cv[3] = RGBA::mix($cv[0], $cv[1], 3, 1, 2);
		} else {
			$cv[2] = RGBA::mix($cv[0], $cv[1], 2, 1, 1);
			$cv[3] = new RGBA(0, 0, 0, 0);
		}
		
		$be_color = new BitExtract(substr($data, 12, 4));
		$be_alpha = new BitExtract(substr($data,  2, 6));
		for ($n = 0; $n < 16; $n++) {
			$color = $be_color->extract(2);
			$alpha = $be_alpha->extract(3);
			$t[$n]    = clone $cv[$color];
			//$t[$n]->a = 0xFF;
			$t[$n]->a = $av[$alpha];
		}
		return $t;
	}
	
	static public function unittest() {
		DXT5::decode(pack('H*', '0100000000000000FFFFEF7DAAAAAAAA'));
	}
}

//DXT5::unittest(); exit;


function showFormatInfo($tex_format) {
	$format  = extract_bits($tex_format,  0, 5);
	$endian  = extract_bits($tex_format,  6, 2);
	$tiled   = extract_bits($tex_format,  8, 1);
	$signX   = extract_bits($tex_format,  9, 2);
	$signY   = extract_bits($tex_format, 11, 2);
	$signZ   = extract_bits($tex_format, 13, 2);
	$signW   = extract_bits($tex_format, 15, 2);
	$numForm = extract_bits($tex_format, 17, 1);
	$swizzX  = extract_bits($tex_format, 18, 3);
	$swizzY  = extract_bits($tex_format, 21, 3);
	$swizzZ  = extract_bits($tex_format, 24, 3);
	$swizzW  = extract_bits($tex_format, 27, 3);
	printf("Format: %d\n", $format);
	printf("Endian: %d\n", $endian);
	printf("Tiled: %d\n", $tiled);
	printf("numForm: %d\n", $numForm);
	printf("signXYZW: %d, %d, %d, %d\n", $signX, $signY, $signZ, $signW);
	printf("swizzXYZW: %d, %d, %d, %d\n", $swizzX, $swizzY, $swizzZ, $swizzW); // ABGR(0, 1, 2, 3)
}

function XGLog2LE16($TexelPitch) {
    return ($TexelPitch >> 2) + (($TexelPitch >> 1) >> ($TexelPitch >> 2));
}

function XGAddress2DTiledX(
    $Offset,        // Tiled memory offset in texels/blocks
    $Width,         // Width of the image in texels/blocks
    $TexelPitch     // Size of an image texel/block in bytes
) {
    $AlignedWidth = ($Width + 31) & ~31;

    $LogBpp       = XGLog2LE16($TexelPitch);
    $OffsetB      = $Offset << $LogBpp;
    $OffsetT      = (($OffsetB & ~4095) >> 3) + (($OffsetB & 1792) >> 2) + ($OffsetB & 63);
	$OffsetM      = $OffsetT >> (7 + $LogBpp);

    $MacroX       = (($OffsetM % ($AlignedWidth >> 5)) << 2);
    $Tile         = (((($OffsetT >> (5 + $LogBpp)) & 2) + ($OffsetB >> 6)) & 3);
    $Macro        = ($MacroX + $Tile) << 3;
    $Micro        = (((($OffsetT >> 1) & ~15) + ($OffsetT & 15)) & (($TexelPitch << 3) - 1)) >> $LogBpp;

    return $Macro + $Micro;
}

function XGAddress2DTiledY(
    $Offset,        // Tiled memory offset in texels/blocks
    $Width,         // Width of the image in texels/blocks
    $TexelPitch     // Size of an image texel/block in bytes
) {
    $AlignedWidth = ($Width + 31) & ~31;

    $LogBpp       = XGLog2LE16($TexelPitch);
    $OffsetB      = $Offset << $LogBpp;
    $OffsetT      = (($OffsetB & ~4095) >> 3) + (($OffsetB & 1792) >> 2) + ($OffsetB & 63);
    $OffsetM      = $OffsetT >> (7 + $LogBpp);

    $MacroY       = (($OffsetM / ($AlignedWidth >> 5)) << 2);
    $Tile         = (($OffsetT >> (6 + $LogBpp)) & 1) + ((($OffsetB & 2048) >> 10));
    $Macro        = ($MacroY + $Tile) << 3;
    $Micro        = (((($OffsetT & ((($TexelPitch << 6) - 1) & ~31)) + (($OffsetT & 15) << 1)) >> (3 + $LogBpp)) & ~1);

    return $Macro + $Micro + (($OffsetT & 16) >> 4);
}

/*
foreach (glob('UI/FONTTEX*.TXV') as $file) {
	$f = fopen($file, 'rb');
	$i = imagecreatetruecolor(2048, 2048);
	imagelayereffect($i, IMG_EFFECT_REPLACE);
	imagealphablending($i, false);
	imagesavealpha($i, true);

	$n = 0;
	foreach (range(0, 2048 - 1, 4) as $y) {
		printf("%d\r", $y);
		//if ($y >= 128) break;
		foreach (range(0, 2048 - 1, 4) as $x) {
			$cl = DXT5::decode(fread($f, 16));
			$cl_n = 0;
			$x = XGAddress2DTiledX($n, 512, 16) * 4;
			$y = XGAddress2DTiledY($n, 512, 16) * 4;
			$n++;
			foreach (range(0, 4 - 1, 1) as $sy) {
				foreach (range(0, 4 - 1, 1) as $sx) {
					imagesetpixel($i, $x + $sx, $y + $sy, $cl[$cl_n++]->gdColor($i));
				}
			}
		}
	}
	imagepng($i, "{$file}.png");
	//break;
}
system("UI\\FONTTEX00.TXV.png");
*/

foreach (glob('UI/ICONSORT.TXV') as $file) {
	echo "$file...\n";
	list($w, $h) = array(256, 256);
	$i = imagecreatetruecolor($w, $h);
	imagelayereffect($i, IMG_EFFECT_REPLACE);
	imagealphablending($i, false);
	imagesavealpha($i, true);
	
	$data = array_values(unpack('V*', file_get_contents($file)));
	foreach ($data as $k => $cv) {
		$x = XGAddress2DTiledX($k, $w, 4);
		$y = XGAddress2DTiledY($k, $h, 4);
		$a = ($cv >>  0) & 0xFF;
		$r = ($cv >>  8) & 0xFF;
		$g = ($cv >> 16) & 0xFF;
		$b = ($cv >> 24) & 0xFF;
		imagesetpixel($i, $x, $y, imagecolorallocatealpha($i, $r, $g, $b, 127 - ($a / 255) * 127));
	}

	imagepng($i, "{$file}.png");
}

/*
foreach (array(
	'cook_beefbowl' => '18280186',
	'fonttex01'     => '1A200154',
) as $file => $tex_format_hex) {
	list(,$tex_format) = unpack('N', pack('H*', $tex_format_hex));
	printf("File: %08X : %s\n", $tex_format, $file);
	showFormatInfo($tex_format);
	printf("\n");
}

foreach (glob('UI/COOK_BEEFBOWL.TXV') as $file) {
	echo "$file...\n";
	$i = imagecreatetruecolor(128, 128);
	imagelayereffect($i, IMG_EFFECT_REPLACE);
	imagealphablending($i, false);
	imagesavealpha($i, true);
	
	$data = array_values(unpack('V*', file_get_contents($file)));
	foreach ($data as $k => $cv) {
		$x = XGAddress2DTiledX($k, 128, 4);
		$y = XGAddress2DTiledY($k, 128, 4);
		$a = ($cv >>  0) & 0xFF;
		$r = ($cv >>  8) & 0xFF;
		$g = ($cv >> 16) & 0xFF;
		$b = ($cv >> 24) & 0xFF;
		imagesetpixel($i, $x, $y, imagecolorallocatealpha($i, $r, $g, $b, 127 - ($a / 255) * 127));
	}

	imagepng($i, "{$file}.png");
}
*/