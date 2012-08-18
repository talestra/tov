<?php
function fread4($f) { return (($v = unpack('N', fread($f, 4))) !== false) ? $v[1] : false; }
function freadsz($f) { $s = ''; while (($c = fread($f, 1)) != "\0") $s .= $c; return $s; }

function extract_to8scel($name) {
	$file_scel_base = "j:/games/vesperia/language/{$name}.dat";
	$file_scel_bak = "j:/games/vesperia/language/{$name}.dat.bak";
	if (!is_file($file_scel_bak)) {
		copy($file_scel_base, $file_scel_bak);
	}

	$f = fopen($file_scel = $file_scel_bak, 'rb');
	if (fread($f, 8) != "TO8SCEL\0") die("Not a TO8SCEL file\n");
	$total_size = fread4($f);
	$ptr_table  = fread4($f);
	$list_count = fread4($f); // 00 00 05 A5
	$data_start = fread4($f);
	$data_end_  = fread4($f);
	@mkdir($name);

	fseek($f, $ptr_table);
	for ($n = 0; $n < $list_count; $n++) {
		$file_start = fread4($f);
		$file_sizec = fread4($f);
		$file_sizeu = fread4($f);
		fseek($f, 4 * 5, SEEK_CUR);
		if ($file_start != 0) {
			printf("%04d: %08X: %08X, %08X\n", $n, $data_start + $file_start, $file_sizec, $file_sizeu);
			$data = file_get_contents($file_scel, 0, null, $data_start + $file_start, $file_sizec);
			$file = sprintf('%s/%04d.dat', $name, $n);
			if (!file_exists("{$file}.u")) {
				if (!file_exists($file)) file_put_contents($file, $data);
				`..\\..\\..\\comptoe.exe -d {$file} {$file}.u`;
				@unlink($file);
			}
		}
	}
}

extract_to8scel('scenario_uk');
//extract_to8scel('scenario_fr');
//extract_to8scel('scenario_de');