<?php

/*
See also: scenario_reinsert.php
*/

require_once(__DIR__ . '/tss.php');

$texts = array();

/*foreach (file('texts/script_translate_test_es.txt') as $line) {
	if (substr($line, 0, 3) == "\xEF\xBB\xBF") $line = substr($line, 3);
	$line = trim($line);
	if (!strlen($line)) continue;
	if ($line[0] == '=') continue;
	if (substr($line, 0, 8) == 'scenario') {
		$file = $line;
		continue;
	}
	if (substr($line, 0, 1) == '#') continue;
	if (preg_match('@^([0-9A-Fa-f]{8}):\'(.*)\'$@', $line, $matches)) {
		$id   = ($matches[1]);
		$text = strtr(stripcslashes($matches[2]), array_flip(TSS::$translate));
		$texts[$file][$id] = $text;
	}
	else {
		die("Unknown line type\n");
	}
}*/

function addtext($ptr) {
	global $segments, $text_base, $tss;
	$segments->addlen($ptr, strlen($text = fsreadsz($tss->f, $text_base + $ptr)) + 1);
	return $text;
}

foreach ($texts as $file => $ctexts) {
	$file2 = 'scenario_es/' . basename($file);
	//file_put_contents($file2, file_get_contents($file));
	copy($file, $file2);
	
	//echo "$file\n";

	$tss = new TSS($file, false);
	$text_base = $tss->header[2];
	$ptr_base  = $tss->header[4];
	//print_r($tss->script_ptrs); print_r($ctexts);
	$segments = new Segments($text_base, fopen($file2, 'r+b'));
	$segments->addlen(filesize($file2) - $text_base, 0x10000, false);
	
	foreach (array(
		array($tss->script_ptrs, $text_base, 1),
		//array($tss->script_ptrs2, $ptr_base, 0),
	) as $ptrs_extra) {
		list($lptrs, $cbase, $skip_count) = $ptrs_extra;
		//printf("---------\n");

		foreach ($lptrs as $ref) {
			fseek($tss->f, $ref);
			$unbased_ptr = fread4($tss->f);
			$textblock_ptr = $text_base + $unbased_ptr;
			$ptrblock_ptr  = $ptr_base + $unbased_ptr;
			//printf("%08X\n", $cbase + $unbased_ptr);

			fseek($tss->f, $cbase + $unbased_ptr);
			$type = fread4($tss->f);
			//$unk  = fread4($tss->f);
			fseek($tss->f, $skip_count * 4, SEEK_CUR);
			$text_ptr = array(); for ($n = 0; $n < 4; $n++) $text_ptr[$n] = fread4($tss->f);
			$text     = array(); for ($n = 0; $n < 4; $n++) {
				if ($text_ptr[$n] >= 2 * 1024 * 1024) {
					die("Invalid pointer!\n");
				}
				$text[$n]     = addtext($text_ptr[$n]);
			}
			$segments->simplify();
			
			$jap_text = $jap_title = '';
			
			for ($n = 0; $n < 4; $n++) {
				$text_ptr_ptr = $textblock_ptr + 8 + 4 * $n;
				if (isset($ctexts[$text_ptr_ptr])) $text[$n] = $ctexts[$text_ptr_ptr];
				$ptr_text = $segments->write_ptr($text_ptr_ptr, $text[$n]);
				//if (basename($file) == '0501.dat.u') printf("%08X\n", $text_base + $ptr_text);
			}
		}
	}
	//die("Test end!\n");
	/*if (basename($file) == '0501.dat.u') {
		print_r($segments);
		exit;
	}*/
	//print_r($segments);
	//exit;
}