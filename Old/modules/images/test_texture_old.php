<?php

function extract_bits($value, $pos, $count) {
	return ($value >> $pos) & ((1 << $count) - 1);
}

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

foreach (array(
	'cook_beefbowl' => '18280186',
	'fonttex01'     => '1A200154',
) as $file => $tex_format_hex) {
	list(,$tex_format) = unpack('N', pack('H*', $tex_format_hex));
	printf("File: %08X : %s\n", $tex_format, $file);
	showFormatInfo($tex_format);
	printf("\n");
}

function processBlock($x, $y, $sizex, $sizey) {
	global $f, $i;
	/*if ($size > 1) {
		$size2 = $size / 2;
		processBlock($x + $size2 * 0, $y + $size2 * 0, $size2);
		processBlock($x + $size2 * 1, $y + $size2 * 0, $size2);
		processBlock($x + $size2 * 0, $y + $size2 * 1, $size2);
		processBlock($x + $size2 * 1, $y + $size2 * 1, $size2);
		return;
	}*/
	for ($by = 0; $by < $sizey; $by++) {
		for ($bx = 0; $bx < $sizex; $bx++) {
			//list(,$r,$g,$b,$a) = $v = unpack('C4', fread($f, 4));
			@list(,$a,$r,$g,$b) = $v = unpack('C4', fread($f, 4));
			//list(,$l) = $v = unpack('C1', fread($f, 1)); $r = $g = $b = $l;
			//print_r($v);
			$c = imagecolorallocatealpha($i, $r, $g, $b, 127 - ($a / 255) * 127);
			//$c = imagecolorallocate($i, $r, $g, $b);
			imagesetpixel($i, $x + $bx, $y + $by, $c);
		}
	}
}

//function xranged($m, $M, $step) { return range($m, $M - 1, $step); }

class Matrix {
	public $w, $h;
	public $rows;

	public function __construct($w, $h) {
		$this->w = $w;
		$this->h = $h;
		$this->rows = array_fill(0, $h, array_fill(0, $w, 0));
	}

	public function set($x, $y, $v) {
		$this->rows[$y][$x] = $v;
	}
	
	public function get($x, $y) {
		return $this->rows[$y][$x];
	}
	
	public function data() {
		$ret = array();
		foreach ($this->rows as $r) $ret = array_merge($ret, $r);
		return $ret;
	}
}

function createUnswizzleTable() {
	$matrix = new Matrix(32, 32);
	$table = array_fill(0, 32, array_fill(0, 32, 0));
	$n = 0;
	$y2 = 0;
	for ($r = 0; $r < 2; $r++) {
		for ($q = 0; $q < 2; $q++) {
			for ($m = 0; $m < 2; $m++) {
				for ($z = 0; $z < 2; $z++) {
					for ($x = 0; $x < 32; $x += 4) {
						$table[$y2 + 0][$x + 0] = $n++;
						$table[$y2 + 0][$x + 1] = $n++;
						$table[$y2 + 0][$x + 2] = $n++;
						$table[$y2 + 0][$x + 3] = $n++;
						$table[$y2 + 1][$x + 0] = $n++;
						$table[$y2 + 1][$x + 1] = $n++;
						$table[$y2 + 1][$x + 2] = $n++;
						$table[$y2 + 1][$x + 3] = $n++;
					}
					$y2 += 2;
				}
			}
		}
	}
	for ($y2 = 8; $y2 < 32; $y2 += 16) {
		for ($y = 0; $y < 8; $y += 2) {
			$row1 = $table[$y2 + $y + 0];
			$row2 = $table[$y2 + $y + 1];
			$table[$y2 + $y + 1] = array_merge(array_slice($row2, 16, 16), array_slice($row2, 0, 16));
			$table[$y2 + $y + 0] = array_merge(array_slice($row1, 16, 16), array_slice($row1, 0, 16));
		}
	}
	return $table;
}

function createUnswizzleVector() {
	$ret = array();
	foreach (createUnswizzleTable() as $r) $ret = array_merge($ret, $r);
	return $ret;
}

function createUnswizzleTableImage() {
	$i = imagecreatetruecolor(32, 32);
	foreach (createUnswizzleTable() as $y => $yvec) {
		foreach ($yvec as $x => $n) {
			$c = (($n % 128) / 127) * 255;
			imagesetpixel($i, $x, $y, imagecolorallocate($i, $c, $c, $c));
		}
	}
	imagepng($i, 'unswizzle.png');
}

function unswizzleBlock($data) {
	static $table;
	if (!isset($table)) $table = createUnswizzleVector();
	$data2 = array();
	for ($n = 0; $n < count($data); $n++) {
		$data2[$n] = $data[$table[$n]];
	}
	return $data2;
}

//$mode = 'create_upload';
$mode = 'extract_beef';

function putpixel($i, $x, $y, $cv) {
	$b = ($cv >> 24) & 0xFF;
	$g = ($cv >> 16) & 0xFF;
	$r = ($cv >>  8) & 0xFF;
	$a = ($cv >>  0) & 0xFF;
	imagesetpixel($i, $x, $y, imagecolorallocate($i, $r, $g, $b));
}

switch ($mode) {
	case 'extract_beef':
		//foreach (glob('UI3/ITEM*.TXV') as $file) {
		foreach (glob('UI/COOK_BEEFBOWL.TXV') as $file) {
			echo "$file...\n";
			createUnswizzleTableImage();
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

			/*
			$f = fopen($file, 'rb');
			foreach (range(0, 128 - 1, 32) as $y) foreach (range(0, 128 - 1, 32) as $x) {
				$data = array_values(unpack('V*', fread($f, 32 * 32 * 4)));
				$data = unswizzleBlock($data);
				for ($by = 0, $n = 0; $by < 32; $by++) {
					for ($bx = 0; $bx < 32; $bx++, $n++) {
						@$cv = $data[$n];
						$b = ($cv >> 24) & 0xFF;
						$g = ($cv >> 16) & 0xFF;
						$r = ($cv >>  8) & 0xFF;
						$a = ($cv >>  0) & 0xFF;
						imagesetpixel($i, $x + $bx, $y + $by, imagecolorallocatealpha($i, $r, $g, $b, 127 - ($a / 255) * 127));
					}
				}
			}
			*/

			imagepng($i, "{$file}.png");
		}
	break;
	case 'create_upload':
		$f = fopen('black128x128.txv', 'wb');
		$i = imagecreatetruecolor(128, 128);
		//$nblocks = 64;
		for ($n = 0, $y = 0; $y < 128; $y++) {
			for ($x = 0; $x < 128; $x++, $n++) {
				list($a, $r, $g, $b) = array(0xFF, 0x00, 0x00, 0x00);
				//$block = (int)($n / (32 * 8));
				$b = $g = $r = ($x / 127) * 255;
				//$r = $x * 2;
				//$b = $y * 2;
				fwrite($f, pack('C4', $a, $r, $g, $b));
				imagesetpixel($i, $x, $y, imagecolorallocate($i, $r, $g, $b));
			}
		}
		fclose($f);
		imagepng($i, 'black128x128.txv.png');

		$f = fopen('L:/vesperia/UI.svo', 'r+b');
		fseek($f, 0x056F7800);
		fwrite($f, file_get_contents('black128x128.txv'));
		//056F7000, 00000800, 000000C0: ITEM_APPLEGUMI.TXM
		//056F7800, 00010000, 00010000: ITEM_APPLEGUMI.TXV
	break;
	default: die('Unknown mode');
}