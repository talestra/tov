<?php

function freadi8($f) { return ord(fread($f, 2)); }
function freadi16($f) { return unpack('n', fread($f, 2))[1]; }
function freadi32($f) { return unpack('N', fread($f, 4))[1]; }
function freadf32($f) { return unpack('f', pack('V', freadi32($f)))[1]; }

foreach (['startup_uk', 'startup_de', 'startup_fr'] as $base) {
	ob_start();
	$f = fopen($base, 'rb');
	if ($base == 'startup_fr') {
		fseek($f, 0x2ACE0);
	} else {
		fseek($f, 0x2AC60);
	}
	for ($n = 0; $n < 44; $n++) {
		$name = rtrim(fread($f, 32), "\0");
		$ints1 = [];
		for ($m = 0; $m < 10; $m++) $ints1[] = freadi32($f);
		$f1 = freadf32($f);
		$f2 = freadf32($f);
		$ints2 = [];
		for ($m = 0; $m < 20; $m++) $ints2[] = freadi32($f);
		$f3 = freadf32($f);
		$ints3 = [];
		for ($m = 0; $m < 4; $m++) $ints3[] = freadi32($f);
		echo "$n: $name, (" . implode(',', $ints1) . "), $f1, $f2, (" . implode(',', $ints2) . "), $f3, (" . implode(',', $ints3) . ")\n";
	}
	fclose($f);
	file_put_contents("{$base}.txt", ob_get_clean());
}