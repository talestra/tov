<?php

$jtag_vesperia_folder = "J:/games/vesperia";

$tss_file_src = "{$jtag_vesperia_folder}/language/string_dic_uk.so.bak";
$tss_file_dst = "{$jtag_vesperia_folder}/language/string_dic_uk.so";

require_once(__DIR__ . '/../common/tss.php');
require_once(__DIR__ . '/../common/text_process.php');

$texts = array();

//foreach (file('texts/strings_es_test.txt') as $line) {
foreach (file(__DIR__ . '/../../texts/strings_es.txt') as $linen => $line) {
	if (substr($line, 0, 3) == "\xEF\xBB\xBF") $line = substr($line, 3);
	$line = trim($line);
	if (!strlen($line)) continue;
	if ($line[0] == '=') continue;
	if (preg_match('@^([0-9A-Fa-f]{8}):\'(.*)\'$@', $line, $matches)) {
		$id   = sprintf('%u', $matches[1]);
		$text = strtr(stripcslashes($matches[2]), array_flip(TSS::$translate));
		$texts[$id] = $text;
	}
	else {
		die("Unknown line type : {$linen}:'{$line}'\n");
	}
}

function addtext($ptr) {
	global $segments, $text_base, $tss;
	$segments->addlen($ptr, strlen($text = fsreadsz($tss->f, $text_base + $ptr)) + 2);
	return $text;
}

copy($tss_file_src, $tss_file_dst);
$tss = new TSS($tss_file_dst, true);

$text_base = $tss->header_raw[2];
$segments = new Segments($text_base, fopen($tss_file_dst, 'r+b'));
$segments->addlen(filesize($tss_file_dst) - $text_base, 0x10000, false);
echo 1;

// Create pointers.
foreach ($tss->ptrs as $ptr => $ptr_info) {
	if ($ptr_info == 'separator') continue;
	if ($ptr_info['lang'] == 1) continue;
	if (!isset($texts[$ptr_info['id']])) continue;

	//printf("%08X\n", $ptr);
	$text_en = addtext($ptr_info['ptr']);
	//$text_es = $texts[$ptr_info['id']];
}

echo 2;

$segments->simplify();

//foreach ($segments->segments as $segment) printf("%d-%d\n", $segment[0], $segment[1]);
//print_r($segments);

//echo 3; exit;
echo 3;

foreach ($tss->ptrs as $ptr => $ptr_info) {
	if ($ptr_info == 'separator') continue;
	if ($ptr_info['lang'] == 1) continue;
	if (!isset($texts[$ptr_info['id']])) continue;

	$text_es = processText($texts[$ptr_info['id']]);
	
	//echo '.';
	
	$segments->write_ptr($ptr, $text_es);
	//print_r($ptr_info);
	//exit;
}

echo 4;
