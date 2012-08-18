<?php
	function fread4($f) { $v = unpack('N', fread($f, 4)); return $v[1]; }
	function fwrite4($f, $v) { fwrite($f, pack('N', $v)); }
	$f = fopen('scenario_uk.dat', 'r+b');
	// TO8SCEL.
	fread($f, 8);
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
		if ($n == 500 || $n == 501) {
			$data = file_get_contents(sprintf('%04d', $n));
			echo "$n...{$clen}\n";
			fseek($f, $ptr_ptr + 0x20 * $n + 4);
			fwrite4($f, strlen($data));
			fseek($f, $cptr + $data_ptr);
			echo "-" . strlen($data) . '-';
			fwrite($f, $data);
		}
	}
	//echo "$sum\n";
?>