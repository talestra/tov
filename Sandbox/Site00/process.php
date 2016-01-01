<?php

function freadi8($f) { return ord(fread($f, 2)); }
function freadi16($f) { return unpack('n', fread($f, 2))[1]; }
function freadi32($f) { return unpack('N', fread($f, 4))[1]; }
function freadf32($f) { return unpack('f', pack('V', freadi32($f)))[1]; }

$points = array();

$offset_x = 100;
$offset_y = 600;

$scale_x = 6;
$scale_y = 6;

$f = fopen('SITE00.DAV.old', 'rb');
// 11 * 4 = 44 = 0x2C
for ($n = 0; $n < 0xA3; $n++) {
	
	$px = freadf32($f);
	$py = freadf32($f);
	$pz = freadf32($f);

	$nx = freadf32($f);
	$ny = freadf32($f);
	$nz = freadf32($f);

	$unk = freadi32($f);

	$cr = freadf32($f);
	$cg = freadf32($f);
	$cb = freadf32($f);
	$ca = freadf32($f);
	
	$points[] = (object)array(
		'px' => $px * $scale_x + $offset_x,
		'py' => $py * $scale_y + $offset_y,
	);

	printf(
		"(%f, %f, %f)-(%f, %f, %f)-(%08X)-(%f, %f, %f, %f)\n",
		$px, $py, $pz,
		$nx, $ny, $nz,
		$unk,
		$cr, $cg, $cb, $ca
	);
}

fseek($f, 0x1c68);

$indices = [];

$i = imagecreatetruecolor(1024, 1024);

for ($n = 0; $n < 200; $n++) {
	$index = freadi16($f);
	if ($index == 0xFFFF) {
		//print_r($indices);
		for ($n = 0; $n < count($indices) - 2; $n++) {
			$p1 = $indices[$n + 0];
			$p2 = $indices[$n + 1];
			$p3 = $indices[$n + 2];
			//imageline($i, $points[$p1]->px, $points[$p1]->py, $points[$p2]->px, $points[$p2]->py, imagecolorallocate($i, 255, 0, 0));
			//imageline($i, $points[$p2]->px, $points[$p2]->py, $points[$p3]->px, $points[$p3]->py, imagecolorallocate($i, 255, 0, 0));
			//imageline($i, $points[$p3]->px, $points[$p3]->py, $points[$p1]->px, $points[$p1]->py, imagecolorallocate($i, 255, 0, 0));
			$ps = [
				$points[$p1]->px, $points[$p1]->py,
				$points[$p2]->px, $points[$p2]->py,
				$points[$p3]->px, $points[$p3]->py,
			];
			
			imagefilledpolygon($i, $ps, 3, imagecolorallocate($i, 255, 0, 0));
		}
		$indices = [];
		//break;
	} else {
		$indices[] = $index;
	}
}

imagepng($i, 'test.png');

fseek($f, 0x2000);

$i = imagecreatetruecolor(128, 128);
for ($y = 0; $y < 128; $y++) {
	for ($x = 0; $x < 128; $x++) {
		$a = freadi8($f);
		$r = freadi8($f);
		$g = freadi8($f);
		$b = freadi8($f);
		imagesetpixel($i, $x, $y, imagecolorallocatealpha($i, $r, $g, $b, $a / 2));
	}
}
//imagepng($i, 'tex.png');
