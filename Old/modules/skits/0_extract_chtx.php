<?php

require_once(__DIR__ . '/../common/fps4.php');
require_once(__DIR__ . '/../common/utils.php');

@mkdir('chtx', 0777);

$chat_svo     = 'J:/games/vesperia/chat.svo';
$chat_svo_bak = "{$chat_svo}.bak";

require_bak($chat_svo);

if (!is_file('chat/VC981UK.DAT.u')) {
	$fps = new FPS4($chat_svo_bak);
	$fps->extractTo('chat', '*UK.DAT');

	foreach (glob('chat/*.DAT') as $file_src) {
		$file_dst = "{$file_src}.u";
		echo "{$file_dst}...";
		if (!is_file($file_dst)) {
			`..\..\..\lzx_vesp.exe "{$file_src}" "{$file_dst}"`;
		}
		echo "Ok\n";
	}
}

foreach (glob('chat/*.DAT.u') as $fps4File) {
	$base = substr(basename($fps4File), 0, -strlen('.DAT.u'));
	$outPath = __DIR__ . "/chtx/{$base}.chtx";
	echo "{$base}...";
	if (is_file($outPath)) {
		echo "Exists\n";
	} else {
		$fps4 = new FPS4($fps4File);
		$fps4->extractFile($fps4->files[3], $outPath);
		echo "Ok\n";
	}
}