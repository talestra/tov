<?php

require_once(__DIR__ . '/../common/text_process.php');

$texts = array();

$english_texts = array();

$jap_mapping = array();
foreach (file('../../texts/strings_us.txt') as $line) {
	$line = trim($line);
	$parts = explode(':', $line, 2);
	if (count($parts) >= 2) {
		$id = (int)$parts[0];
		if (!isset($jap_mapping[$id])) {
			$text = stripcslashes(substr($parts[1], 1, -1));
			$jap_mapping[$id] = $text;
		}
	}
}

//print_r($jap_mapping);
//exit;

$f = fopen(__DIR__ . '/../../texts/strings_uk.txt', 'rb');
while (!feof($f)) {
	@list($id, $text) = explode(':', trim(fgets($f)), 2);
	if (!strlen($id)) continue;
	if (substr($id, 0, 1) == '-') continue;
	$text = substr(trim($text), 1, -1);
	$english_texts[(int)$id] = stripcslashes($text);
}
//exit;

// MISC
{
	foreach (glob(__DIR__ . '/../../ACME1/misc.es/SRC/*.txt') as $file) {
		$id_high = (int)substr(basename($file), 0, 4);
		$parts = explode('## POINTER ', file_get_contents($file));
		foreach (array_slice($parts, 1) as $part) {
			list($header, $body) = explode("\n", $part, 2);
			$id_low = (int)$header;
			$body = trim(str_replace("\r\n", "\n", $body));
			$id = $id_high * 10000 + $id_low;
			$texts[$id] = $body;
		}
	}
}

// NPCS
{
	$acmeid_ids = array();

	foreach (file(__DIR__ . '/../../texts/string_dic_uk_NPC_map.txt') as $line_n => $line) {
		list($ids_str) = explode(':', trim($line));
		$ids = explode(',', $ids_str);
		$acmeid_ids[$line_n] = $ids;
	}

	//print_r($line_ids);

	foreach (glob(__DIR__ . '/../../ACME1/npc.es/SRC/*.txt') as $file) {
		$parts = explode('## POINTER ', file_get_contents($file));
		foreach (array_slice($parts, 1) as $part) {
			list($header, $body) = explode("\n", $part, 2);
			$acme_id = (int)$header;
			$body = trim(str_replace("\r\n", "\n", $body));
			//printf("%08d:'%s'\n", $id, addcslashes($body, "\n\r\t\'\""));
			foreach ($acmeid_ids[$acme_id] as $id) {
				$texts[$id] = $body;
			}
			//echo "$id\n";
		}
	}
}

// SKITS (TITLES)
$texts_skits = array();
{
	// 00250000
	//00250000:'Days Just Fly By...'
	//00250456:'Let's Turn Around'
	$skit_ab = json_decode(file_get_contents(__DIR__ . '/skit_ab.json'), true);
	foreach (glob(__DIR__ . '/../../ACME1/skits.es/SRC/*.txt') as $file) {
		$base_name = substr(basename($file), 0, -4);
		if (isset($skit_ab[$base_name])) {
			$id = (int)$skit_ab[$base_name];
			$parts = explode('## POINTER ', file_get_contents($file));
			foreach (array_slice($parts, 1, 1) as $part) {
				list($header, $body) = explode("\n", $part, 2);
				$body = trim(str_replace("\r\n", "\n", $body));
				$body = mb_convert_encoding($body, 'utf-8', 'ISO-8859-1');
				$body = fix_acme1_skits_texts($body);
				$texts_skits[$id] = $texts[$id] = $body;
			}
		}
	}
}

ksort($texts);

$fstrings_es = fopen(__DIR__ . '/../../texts/strings_es.txt', 'wb');
$fmisc_es_json = fopen(__DIR__ . '/../../ACME2/misc.json', 'wb');

$time = time();

foreach ($texts as $id => $text) {
	fprintf($fstrings_es, "%08d:'%s'\n", $id, addcslashes($text, "\n\r\t\'\""));
	fprintf($fmisc_es_json, "%s\n", json_encode(array(
		'text_path' => sprintf('misc/%04d', (int)($id / 1000) * 1000),
		'text_id' => sprintf('%08d', $id),
		'linked_id' => md5($text),
		'metainfo' => array(
			'id' => $id,
		),
		'texts' => array(
			'en' => array(fix_acme1_page($english_texts[$id])),
			'ja' => @array(fix_acme1_page($jap_mapping[$id])),
			'es' => array(fix_acme1_page($text)),
		),
		'translated' => array(
			'es' => array(
				'done' => true,
				'user' => 'unknown',
				'time' => $time,
			),
		),
		'revised' => array(
			'es' => array(
				'done' => true,
				'user' => 'unknown',
				'time' => $time,
			),
		),
	)));
}
