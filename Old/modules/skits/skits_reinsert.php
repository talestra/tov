<?php
// (YUR)(RAP)(EST)(FRE)(EST_P)(KAR)(RIT)(RAV)(JUD)(JUD_P)(EST_T)(BAU)

function process_text($text) {
	static $l = array();
	$text = preg_replace_callback('@\\([^\\)]+\\)@', function($t) use (&$l) {
		$v = $t[0];
		if (!isset($l[$v])) {
			$l[$v] = true;
			echo "$v\n";
		}
		return "\x04{$v}";
	}, $text);
	return $text;
}

function create_chtx($name, $texts) {
	$f = fopen("chtx_es/{$name}US.chtx", 'wb');
	$fr = fopen("chtx/{$name}UK.chtx", 'rb');
	fwrite($f, "TO8CHTX\0");
	fwrite($f, pack('N*',
		0,                  // Total file size.
		count($texts),      // Count
		$ptr_start = 0x20,  // Pointer start
		$txt_start = 0x20 + count($texts) * 0x10, // Text start.
		0, 0                // Pad
	));
	fseek($f, $txt_start);
	fwrite($f, "dummy\0");
	$ptrs = array();
	foreach ($texts as $text) {
		list($title, $text) = explode("\n", $text, 2);
		$ptr1 = ftell($f) - $txt_start; fwrite($f, process_text($title) . "\0");
		$ptr2 = ftell($f) - $txt_start; fwrite($f, process_text($text) . "\0");
		$ptrs[] = array($ptr1, $ptr2);
	}
	fseek($f, $ptr_start);
	foreach ($ptrs as $ptr) {
		fseek($fr, ftell($f) + 12);
		list(,$chara_id) = unpack('N', fread($fr, 4));
		fwrite($f, pack('N*', $ptr[0], 5, $ptr[1], $chara_id));
	}
	$end = ftell($f);
	fseek($f, 8);
	fwrite($f, pack('N', $end));
	//print_r($texts);
}

foreach (glob("skt/SRC/*.txt") as $f) {
	$data = file_get_contents($f);
	$texts = array_slice(array_map('trim', preg_split('@## POINTER (\\d+).*\n@Umsi', $data)), 2);
	$lf = basename($f);
	if (preg_match('#^.*@(.*)\\.#', $lf, $matches)) create_chtx($matches[1], $texts);
	//exit;
}