<?php

$file_scenario = 'h:/games/vesperia/language/scenario_es.dat';
$file_map_svo = 'h:/games/vesperia/MAP.SVO';

/*
php scenario_reinsert.php
php to8scel_reinsert.php

Importante.
No se usa language/scenario_uk.dat sino el SCENARIO.DAT embebido en MAP.SVO.

fps4 -l l:\games\vesperia\map.svo scenario*
FPS4('../Release/ïñÆ╩/map.svo')

0482 - FPS4.FileEntry(0x360C9000, 0x03D2DED0(0x03D2E000), 'SCENARIO.DAT')
*/
function fwrite4($f, $v) { fwrite($f, pack('N', $v)); }
function fread4($f) { return (($v = unpack('N', fread($f, 4))) !== false) ? $v[1] : false; }
function freadsz($f) { $s = ''; while (($c = fread($f, 1)) != "\0") $s .= $c; return $s; }

// makecab /D CompressionMemory=17 /D CompressionType=LZX 0000.dat.u
function cab_to_newc($file_cab, $file_new, $size_u = 0) {
	$fr = fopen($file_cab, 'rb');
	$fw = fopen($file_new, 'wb');
	fseek($fr, 0x44 + 2 + 1);
	//fseek($fr, 0x44 + 4);
	fseek($fw, 0x34);
	while (!feof($fr)) {
		fread($fr, 4);
		list(,$len_c,$len_u) = unpack('v2', fread($fr, 4));
		$data = fread($fr, $len_c);
		//printf("%08X %08X\n", $len_c, $len_u);
		if ($len_u > 0x8000) {
			printf("Data error!\n");
			exit;
		}
		if ($len_u < 0x8000) {
			fwrite($fw, "\xFF" . pack('c', $len_u) . pack('c', $len_c));
		} else {
			fwrite($fw, pack('n', $len_c));
		}

		fwrite($fw, $data);
		
		if ($len_u < 0x8000) break;
	}
	
	$size_file = ftell($fw);
	
	fseek($fw, 0x00); fwrite($fw, "\x0F\xF5\x12\xEE\x01\x02\x00\x00");
	fseek($fw, 0x10); fwrite($fw, pack('N*', 0x20000, 0x80000, 0, $size_u));
	fseek($fw, 0x20); fwrite($fw, pack('N*', 0, $size_file - 0x20, $size_u, $size_file - 0x20 - 4, $size_file - 0x20 - 4));
}
function newc_compress($file_old, $file_new) {
	$size_u = filesize($file_old);
	copy($file_old, 'xxxxxxxx.u');
	// cabarc -o -m LZX:17 n data.uu.cab data.uu
	`cabarc -o -m LZX:17 n xxxxxxxx.u_ xxxxxxxx.u`;
	`makecab /D CompressionMemory=17 /D CompressionType=LZX xxxxxxxx.u`;
	unlink('xxxxxxxx.u');
	cab_to_newc('xxxxxxxx.u_', $file_new, $size_u);
	unlink('xxxxxxxx.u_');
}
//newc_compress('data.uu', 'data.cc.newc');
//cab_to_newc('xxxxxxxx.u_', 'test.newc');

function compress($file_src, $file_dst) {
	if (0) {
		if (filesize($file_src) < 0x8000) {
			`..\\comptoe.exe -c1 {$file_src} {$file_dst}`;
		} else {
			newc_compress($file_src, $file_dst);
		}
	} else if (1) {
		`..\\..\\..\\comptoe.exe -c1 {$file_src} {$file_dst}`;
		printf("(1)");
	} else {
		`..\\comptoe.exe -c1 {$file_src} {$file_dst}.c1`;
		`..\\comptoe.exe -c3 {$file_src} {$file_dst}.c3`;
		if (filesize("{$file_dst}.c1") < filesize("{$file_dst}.c3")) {
			copy("{$file_dst}.c1", "{$file_dst}");
			printf("(1)");
		} else {
			copy("{$file_dst}.c3", "{$file_dst}");
			printf("(3)");
		}
		unlink("{$file_dst}.c1");
		unlink("{$file_dst}.c3");
	}
}

$start = 0x20;
$count = 0x5A5;
$data_start = $start + $count * 0x20;
$data_start_add = 0x10;
//$f = fopen('c:/isos/360/scenario_es.dat', 'wb');
if (true) {
	$f = fopen($file_scenario, 'wb');
	if (!$f) exit;
	fwrite($f, "TO8SCEL\0");
	fwrite4($f, 0); // size (motorola/big endian)
	fwrite4($f, $start);

	fwrite4($f, $count);
	fwrite4($f, $data_start);
	fwrite4($f, 0); // size (intel/little endian)
	fwrite4($f, 0); // dummy

	fseek($f, $data_start + $data_start_add);
	for ($n = 0; $n < $count; $n++) {
		$file_src = sprintf("scenario_es\\%04d.dat.u", $n);
		$file_dst = sprintf("scenario_es\\%04d.dat", $n);
		printf("%s...", $file_src);
		if (file_exists($file_src)) {
			if (!file_exists($file_dst) || (filemtime($file_src) != filemtime($file_dst))) {
				compress($file_src, $file_dst);
				touch($file_dst, filemtime($file_src), filemtime($file_src));
			}
		} else {
			printf("(0)");
			file_put_contents($file_dst, '');
		}
		
		$size_dst = @filesize($file_dst);
		$size_src = @filesize($file_src);
		
		$back_p = ftell($f);
		while (($back_p % 0x10) != 0) $back_p++;
		{
			fseek($f, $start + $n * 0x20);
			if (($size_dst == 0) && ($size_src == 0)) {
				fwrite4($f, 0);
			} else {
				fwrite4($f, $back_p - $data_start);
			}
			fwrite4($f, $size_dst);
			fwrite4($f, $size_src);
		}
		fseek($f, $back_p);
		fwrite($f, file_get_contents($file_dst));
		printf("\n");
	}
	while ((ftell($f) % 0x10) != 0) fwrite($f, "\0");
	$end = ftell($f);
	fseek($f, 0x08); fwrite($f, pack('N', $end));
	fseek($f, 0x18); fwrite($f, pack('V', $end));
	fclose($f);
}

if (1) {
	echo "Duplicating...scenario_uk, scenario_us...";
	copy($file_scenario, sprintf('%s/scenario_uk.dat', dirname($file_scenario)));
	copy($file_scenario, sprintf('%s/scenario_us.dat', dirname($file_scenario)));
	echo "Ok\n";
	echo "Writing...{$file_map_svo}...";
	$f = fopen($file_map_svo, 'r+b');
	fseek($f, 0x360C9000);
	if (fread($f, 7) != 'TO8SCEL') throw(new Exception("Invalid ISO"));
	fwrite($f, file_get_contents($file_scenario));
	fclose($f);
	echo "Ok\n";
}