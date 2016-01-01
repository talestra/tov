<?php

$fileName = __DIR__ . '/../../PatchData/Text/tov.json';

$lines = file($fileName);
$lines2 = array();

foreach ($lines as $line) {
	$row = json_decode(trim($line), true);
	$row2 = array();
	foreach (array('project', 'text_path', 'text_id', 'linked_id', 'metainfo', 'texts', 'translated', 'revised') as $key) {
		$row2[$key] = $row[$key];
		unset($row[$key]);
	}
	if (!empty($row)) die("ERROR!" . print_r($row, true));
	$lines2[] = json_encode($row2);
}

file_put_contents($fileName, implode("\n", $lines2));

/*
{
	"linked_id":"8dc88c20bb2390c137614cb1d9c0ac5a",
	"metainfo":{"textblock_ptr":16532},
	"project":"tov",
	"revised":{"es":{"done":true,"user":"unknown","time":1305373846}},
	"text_id":"00004094",
	"text_path":"battle\/BTL_EP_0070_010",
	"texts":{"ja":["\u30dc\u30c3\u30b3\u30b9","<VOICE>(VA070_005)\u4e0b\u3063\u7aef\u306f\u304a\u307e\u3048\u3060\u308d\u3046\u304c\u2026\u2026\uff01"],"en":["Boccos","<VOICE>(VA070_005)You're the peon...!"],"es":["Boccos","<VOICE>(VA070_005)\u00a1T\u00fa s\u00ed que eres un edec\u00e1n!"]},
	"translated":{"es":{"done":true,"user":"unknown","time":1305373846}}
}
*/