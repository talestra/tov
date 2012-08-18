<?php
require_once(__DIR__ . '/../common/tss.php');
require_once(__DIR__ . '/../common/text_process.php');

function addtext($ptr) {
	global $tss;
	$text = fsreadsz($tss->f, $tss->header->text_start + $ptr);
	return $text;
}

// PUSH_ARRAY(0C0018):
/*
  000348FC: PUSH_ARRAY(0C0018): data_00003E50
    0: // (data_00000000)''
    1: // (data_0000000B)''
    2: // (data_00003DE6)'アレクセイ'
    3: // (data_00003DF6)'\t(VB45_0703)くっ……、やってくれたな！'
    4: // (data_00003E2A)'Alexei'
    5: // (data_00003E31)'\t(VB45_0703)Aagh...not bad!'
*/

@mkdir('btl_texts', 0777);

$spanish = array();
foreach (file(__DIR__ . '/../../ACME2/battle_en_es.json') as $line) {
	$data = json_decode(trim($line), true);
	$spanish[$data['text_path']][$data['text_id']] = $data['texts']['es'];
}

$fjson = fopen(__DIR__ . '/../../ACME2/battle.json', 'wb');

$time = time();

foreach (glob('btl.svo.d/BTL_PACK_UK.DAT.d/0003.DAT.d/*.u') as $tss_full_file_name) {
	$tss_base_file_name = basename($tss_full_file_name);
	$tss = new TSS($tss_full_file_name, true);
	$text_base = $tss->header->text_start;

	if (count($tss->dialogs)) {
		//echo "{$tss_full_file_name}...\n";
		//ob_start();
		foreach ($tss->dialogs as $dialog_code_ptr => $dialog) {
			$dialog_values = array_values($dialog);
			//print_r($dialog);
			$title_ja = fsreadsz($tss->f, $text_base + $dialog_values[1]);
			$text_ja = fsreadsz($tss->f, $text_base + $dialog_values[2]);

			$title_en = fsreadsz($tss->f, $text_base + $dialog_values[3]);
			$text_en = fsreadsz($tss->f, $text_base + $dialog_values[4]);
			//printf("'%s'\n", quotec(unprocessText($title)));
			//printf("'%s'\n", quotec(unprocessText($text)));
			//echo "\n";
			
			$text_path = sprintf('battle/%s', substr($tss_base_file_name, 0, -2));
			$text_id = sprintf('%08X', $dialog_code_ptr);
			
			$text_block = array(
				'text_path' => $text_path,
				'text_id' => $text_id,
				'linked_id' => md5($title_en . $text_en),
				'metainfo' => array(
					'textblock_ptr' => $dialog_code_ptr,
				),
				'texts' => array(
					'ja' => array(unprocessText($title_ja), unprocessText($text_ja)),
					'en' => array(unprocessText($title_en), unprocessText($text_en)),
					'es' => $spanish[$text_path][$text_id],
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
			);
			
			fprintf($fjson, "%s\n", json_encode($text_block));
		}
		//file_put_contents("btl_texts/{$tss_base_file_name}.txt", ob_get_clean());
	}
}

/*
foreach ($tss->ptrs as $code_ptr_offset => $info) {
	if ($info == 'separator') continue;
	if (isset($info['script_ptr'])) {
		printf("%08X\n", $text_base + $info['ptr']);
		fseek($tss->f, $text_base + $info['ptr']);
		for ($n = 0; $n < 6; $n++) {
			$v = fread4($tss->f);
			printf("  %08X\n", $v);
		}
		print_r($info);
	}
}
*/
