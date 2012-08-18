<?php

require_once(__DIR__ . '/utils.php');

function extract_chtx($file) {
	list(,$start) = unpack('N', file_get_contents($file, 0, null, 0x4C, 4));
	$row = file_get_contents($file, 0, null, $start, 0x10);
	if (substr($row, 0, 8) != "TO8CHTX\0") die("Not a TO8CHTX\n");
	list(,$len) = unpack('N', substr($row, 8, 4));
	$data = file_get_contents($file, 0, null, $start, $len);
	if (preg_match('/^(.*)\\.DAT\\.u$/', $file, $matches)) {
		$name = "{$matches[1]}.chtx";
		file_put_contents($name, $data);
		//print_r($matches[1]);
		//printf("%08X\n", $start);
	
	}
}

function process_chtx($chtx_file) {
	//echo "$chtx_file\n";
	//$retval = 0;
	$f = fopen($chtx_file, 'rb');
	if (fread($f, 8) != "TO8CHTX\0") die("Not a chtx\n");
	$file_end    = fread4($f);
	$text_count  = fread4($f);
	$table_start = fread4($f);
	$text_start  = fread4($f);
	fseek($f, $table_start);
	
	$texts = array();
	
	for ($n = 0; $n < $text_count; $n++) {
		$ptr_title = fread4($f);
		$ptr_text_jp   = fread4($f);
		$ptr_text_en   = fread4($f);
		$chara_id  = fread4($f);
		$back = ftell($f);
		{
			fseek($f, $text_start + $ptr_title); $title = freadsz($f);
			fseek($f, $text_start + $ptr_text_jp); $text_jp = freadsz($f);
			fseek($f, $text_start + $ptr_text_en); $text_en = freadsz($f);
			//if ($text == 'Dummy') $retval = 1;
			$texts[] = (object)array(
				'title' => $title,
				'text_jp' => $text_jp,
				'text_en' => $text_en,
			);
		}
		fseek($f, $back);
	}
	return $texts;
}
