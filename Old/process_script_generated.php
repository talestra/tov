<?php

/*
tss
ACME
scenario_es
scenario_uk
*/

require_once(__DIR__ . '/tss.php');

@mkdir('blocks', 0777, true);

$hashed_blocks = array();

$data = unserialize(file_get_contents('script.serialized'));

foreach ($data as $file => $blocks) {
	//echo "$file\n";
	foreach ($blocks as $block) {
		$hash = TSS::hash_block($block);
		if ($hash === null) continue;
		$hashed_blocks[$hash] = $block;
	}
}

//echo count($hashed_blocks);

$hashes = array();
foreach ($hashed_blocks as $hash => $block) {
	$hashes[$hash] = count($hashes);
}

file_put_contents('block_hash.json', json_encode($hashes));

foreach (array_values($hashed_blocks) as $blockid => $block) {
	$f = fopen(sprintf('blocks/%03d.txt', $blockid), 'wb');
	foreach ($block as $k => $v) {
		fprintf($f, "## POINTER %d\n", $k);
		//if (strpos(implode("\n", $v), '%')) print_r($v);
		fprintf($f, "%s", implode("\n", $v) . "\n\n");
	}
	fclose($f);
	//print_r($block);
}

//file_put_contents('script_unique.serialized', serialize($hashed_blocks));