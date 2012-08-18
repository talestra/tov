<?php

/*
See also: script_reinsert.php
*/

/*
php scenario_reinsert.php
php to8scel_reinsert.php
*/

require_once(__DIR__ . '/../common/tss.php');
require_once(__DIR__ . '/../common/acme.php');
require_once(__DIR__ . '/../common/text_process.php');

$texts = array();

/*
foreach (file('texts/script_translate_test_es.txt') as $line) {
	if (substr($line, 0, 3) == "\xEF\xBB\xBF") $line = substr($line, 3);
	$line = trim($line);
	if (!strlen($line)) continue;
	if ($line[0] == '=') continue;
	if (substr($line, 0, 8) == 'scenario') {
		$file = $line;
		continue;
	}
	if (preg_match('@^([0-9A-Fa-f]{8}):\'(.*)\'$@', $line, $matches)) {
		$id   = hexdec($matches[1]);
		$text = strtr(stripcslashes($matches[2]), array_flip(TSS::$translate));
		$texts[$file][$id] = $text;
	}
	else {
		die("Unknown line type\n");
	}
}
*/

/*function fsreadsz($f, $seek) {
	fseek($f, $seek);
	return freadsz($f);
}*/

function addtext($ptr) {
	global $segments, $text_base, $tss;
	$segments->addlen($ptr, strlen($text = fsreadsz($tss->f, $text_base + $ptr)) + 1);
	return $text;
}

$block_hashes = (array)json_decode(file_get_contents('block_hash.json'));

$full_script = parse_acme_folder('script.es/SRC', true);
//$translate_rev = array_flip(TSS::$translate);
//print_r($full_script); exit;

@mkdir('scenario_es', 0777);

`del scenario_es\\*.dat`;

foreach (glob('scenario_uk/*.dat.u') as $tss_file) {
	$tss_file_translated = sprintf('scenario_es/%s', basename($tss_file));
	copy($tss_file, $tss_file_translated);
	$tss_file_size = filesize($tss_file_translated);

	$tss = new TSS($tss_file, false);
	//$tss = new TSS_OldBad($tss_file, false);
	$text_base = $tss->header_raw[2];
	$segments = new Segments($text_base, fopen($tss_file_translated, 'r+b'));
	$segments->addlen($tss_file_size - $text_base, 0x10000, false);
	
	//print_r($tss->script_ptrs);
	
	printf("%s->%s\n", $tss_file, $tss_file_translated);
	
	foreach ($tss->extractDATA(true) as $block_hash => $block) {
		$room_id = $block_hashes[$block_hash];
		echo "  {$room_id}\n";
		$text_id = 0;
		foreach ($block as $ref => $texts2) {
			fseek($tss->f, $ref);
			$textblock_ptr = $text_base + fread4($tss->f);

			fseek($tss->f, $textblock_ptr);
			$type = fread4($tss->f);
			$unk  = fread4($tss->f);
			$text_ptr = array(); for ($n = 0; $n < 4; $n++) $text_ptr[$n] = fread4($tss->f);
			$text     = array(); for ($n = 0; $n < 4; $n++) $text[$n]     = addtext($text_ptr[$n]);
			$segments->simplify();
			
			// Remove japanese text.
			$text[1] = $text[0] = '';
			
			$translated_body = &$full_script[$room_id][$text_id];
			if (!isset($translated_body)) {
				echo "ROOM_ID: {$room_id}, TEXT_ID: {$text_id}\n";
				print_r($text);
				exit;
			}

			$translated_body = processText($translated_body);
			if (empty($text[2])) {
				$text[3] = $translated_body;
			} else {
				list($text[2], $text[3]) = explode("\n", $translated_body, 2);
			}
			//echo "$translated_body\n";
			
			$text_id++;
			
			for ($n = 0; $n < 4; $n++) {
				$text_ptr_ptr = $textblock_ptr + 8 + 4 * $n;
				//if (isset($ctexts[$text_ptr_ptr])) $text[$n] = $ctexts[$text_ptr_ptr];
				$ptr_text = $segments->write_ptr($text_ptr_ptr, $text[$n]);
				//if (basename($file) == '0501.dat.u') printf("%08X\n", $text_base + $ptr_text);
			}
		}
		//print_r($block);
	}
}

/*
exit;

foreach ($texts as $file => $ctexts) {
	$file2 = 'scenario_es/' . basename($file);
	copy($file, $file2);

	//if (basename($file) != '0500.dat.u') continue;
	
	echo "$file\n";

	$tss = new TSS($file, false);
	$text_base = $tss->header[2];
	//print_r($tss->script_ptrs); print_r($ctexts);
	$segments = new Segments($text_base, fopen($file2, 'r+b'));
	$segments->addlen(filesize($file2) - $text_base, 0x10000, false);
	
	foreach ($tss->script_ptrs as $ref) {
		fseek($tss->f, $ref);
		$textblock_ptr = $text_base + fread4($tss->f);

		fseek($tss->f, $textblock_ptr);
		$type = fread4($tss->f);
		$unk  = fread4($tss->f);
		$text_ptr = array(); for ($n = 0; $n < 4; $n++) $text_ptr[$n] = fread4($tss->f);
		$text     = array(); for ($n = 0; $n < 4; $n++) $text[$n]     = addtext($text_ptr[$n]);
		$segments->simplify();
		
		// Remove japanese text.
		$text[1] = $text[0] = '';
		
		for ($n = 0; $n < 4; $n++) {
			$text_ptr_ptr = $textblock_ptr + 8 + 4 * $n;
			if (isset($ctexts[$text_ptr_ptr])) $text[$n] = $ctexts[$text_ptr_ptr];
			$ptr_text = $segments->write_ptr($text_ptr_ptr, $text[$n]);
			//if (basename($file) == '0501.dat.u') printf("%08X\n", $text_base + $ptr_text);
		}
	}
	//if (basename($file) == '0501.dat.u') { print_r($segments); exit; }
	//print_r($segments);
	//exit;
}
*/