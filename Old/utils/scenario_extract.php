<?php
if (count($argv) < 2) {
	//$argv[1]
	echo "scenario_extract <path_to_scenario.dat> [<output_folder>]\n";
	exit;
}

$to8scel_file = $argv[1];
if (isset($argv[2])) {
	$output_folder = $argv[2];
} else {
	$output_folder = pathinfo($to8scel_file, PATHINFO_FILENAME);
}

function fread4($f) { $v = unpack('N', fread($f, 4)); return $v[1]; }
$f = fopen($to8scel_file, 'rb');
// TO8SCEL.
$magic = fread($f, 8);
if ($magic != "TO8SCEL\0") throw(new Exception("Not a TO8SCEL file."));
$file_size = fread4($f);
$ptr_ptr   = fread4($f);
$ptr_count = fread4($f);
$data_ptr  = fread4($f);
$data_len  = fread4($f);

for ($n = 0; $n < $ptr_count; $n++) {
	fseek($f, $ptr_ptr + 0x20 * $n);
	$cptr = fread4($f);
	$clen = fread4($f);
	$ulen = fread4($f);
	echo "$n...{$clen}\n";
	//if ($n >= 505) break;
	if ($cptr != 0) {
		//$sum += $clen;
		$cname = sprintf('temp.file', $n);
		$uname = sprintf("{$output_folder}/%04d.u", $n);
		if (!file_exists($cname) || !file_exists($uname)) {
			fseek($f, $cptr + $data_ptr);
			$data = fread($f, $clen);
			file_put_contents($cname, $data);
			//echo "$clen\n";
			if (!file_exists($uname)) {
				@mkdir(dirname($uname), 0777, true);
				`comptoe.exe -d {$cname} {$uname}`;
			}
			unlink($cname);
		}
	}
	//if ($n > 400) break;
}
//echo "$sum\n";