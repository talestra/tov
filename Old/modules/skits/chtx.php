<?php

require_once(__DIR__ . '/../common/utils.php');
require_once(__DIR__ . '/../common/chtx.php');
require_once(__DIR__ . '/../common/text_process.php');
require_once(__DIR__ . '/../common/acme.php');

// extract_chtx();

@$skit_titles = array_map(function($a) { list(,$a) = explode(':', $a, 2); return trim($a); }, file('skits.txt'));

$acme = parse_acme_folder(__DIR__ . '/../../ACME1/skits.es/SRC', false, function($text) {
	return fix_acme1_skits_texts($text);
}, function(&$name, $texts) {
	//echo "$name\n";
	$name = substr($name, 3);
	return $texts;
});

//print_r($acme); exit;

$fskits_json = fopen(__DIR__ . '/../../ACME2/skits.json', 'wb');

$time = time();

foreach (glob("chtx/*.chtx") as $k => $file) {
	$basefile = substr(basename($file), 0, -strlen('UK.chtx'));
	//echo "$basefile\n";
	//printf("%s\n", str_repeat('-', 79));
	//printf(" %s\n", $file);
	
	//printf(" %s\n", $skit_titles[$k++]);
	//printf("%s\n\n", str_repeat('-', 79));
	
	$texts = process_chtx($file);
	array_unshift($texts, $skit_titles[$k]);

	//extract_chtx($file);
	//print_r($texts);
	//exit;
	
	foreach ($texts as $k => $text) {
		$title   = @fix_acme1_page($text->title);
		$text_jp = @fix_acme1_page($text->text_jp);
		$text_en = @fix_acme1_page($text->text_en);
		$text_es = fix_acme1_page($acme[$basefile][$k]);
		fprintf($fskits_json, "%s\n", json_encode(array(
			'text_path' => sprintf('skits/%s', $basefile),
			'text_id' => sprintf('%03d', $k),
			'linked_id' => md5($text_en),
			'metainfo' => array(
				'index' => $k,
			),
			'texts' => array(
				'ja' => array($title, $text_jp),
				'en' => array($title, $text_en),
				'es' => array($title, $text_es),
			),
			'translated' => array(
				'es' => array(
					'done' => !empty($text_es),
					'user' => 'unknown',
					'time' => $time,
				),
			),
			'revised' => array(
				'es' => array(
					'done' => !empty($text_es),
					'user' => 'unknown',
					'time' => $time,
				),
			),
		)));
	}
}
