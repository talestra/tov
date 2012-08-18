<?php
require_once(__DIR__ . '/../common/tss.php');

class TSS2 extends TSS {
	public $texts;

	public function emitSeparator() {
	}

	public function emitIdString($id, $text) {
		if ($id >= 260000 && $id < 280001) {
			$this->texts[$text][] = $id;
		}
	}

	public function emitEnd() {
	}
}

$tss = new TSS2('j:/games/vesperia/language/string_dic_uk.so.bak', true);
$tss->extractTEXT();
$texts = array();
foreach ($tss->texts as $text => $ids) {
	$ids_str = implode(',', $ids);
	printf("%s:'%s'\n", $ids_str, addcslashes($text, "\0..\37"));
	$texts[] = $text;
}

/*
@mkdir("tov_npc/SRC", 0777, true);
foreach (array_chunk($texts, 100, true) as $chunk_id => $chunk_texts) {
	$f = fopen(sprintf('tov_npc/SRC/%04d.txt', $chunk_id), 'wb');
	foreach ($chunk_texts as $text_id => $text) {
		fprintf($f, "## POINTER %d\n", $text_id);
		fprintf($f, "%s\n\n", $text);
	}
	fclose($f);
}
*/
