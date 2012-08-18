<?php

require_once(__DIR__ . '/../common/tss.php');
require_once(__DIR__ . '/../common/acme.php');
require_once(__DIR__ . '/../common/text_process.php');

$texts = array();

function addtext($ptr) {
	global $text_base, $tss;
	return fsreadsz($tss->f, $text_base + $ptr);
}

$block_hashes = (array)json_decode(file_get_contents('block_hash.json'));

$full_script_es = parse_acme_folder_ex(__DIR__ . '/../../ACME1/script.es/SRC', true);
$full_script_en = parse_acme_folder_ex(__DIR__ . '/../../ACME1/script.en/SRC', true);

$try_to_map_texts = array();

foreach ($full_script_en as $room_id => $room_texts) {
	foreach (array_keys($room_texts) as $room_text_id) {
		$room_text_en = explode("\n", $full_script_en[$room_id][$room_text_id]->text, 2);
		$room_text_es = explode("\n", $full_script_es[$room_id][$room_text_id]->text, 2);
		//echo "'{$full_script_en[$room_id][$room_text_id]}'\n";
		foreach ($room_text_en as $k => $v) {
			@$t_en = $room_text_en[$k];
			@$t_es = $room_text_es[$k];
			if (!empty($t_en) && !empty($t_es)) {
				$try_to_map_texts[processText($t_en)] = array(
					'text' => processText($t_es, $t_en),
					'acmeEntry' => $full_script_en[$room_id][$room_text_id],
				);
			}
		}
	}
		//exit;
}

/*
    [?(2)Our world, Terca Lumireis.] => ?(2)Nuestro mundo, Terca Lumireis.
*/

//print_r($try_to_map_texts);
//exit;

@mkdir('scenario_es', 0777);
@mkdir('expanded_script', 0777);

@`del scenario_es\\*.dat 2> NUL`;

$repeated_texts = array();

$fexpanded_global = fopen($expanded_file_name = __DIR__ . "/../../ACME2/scenario.json", 'wb');

$time = time();

foreach (glob('scenario_uk/*.dat.u') as $tss_file) {
	$tss_basename = basename($tss_file);
	$tss_file_translated = sprintf('scenario_es/%s', $tss_basename);
	copy($tss_file, $tss_file_translated);
	$tss_file_size = filesize($tss_file_translated);

	$tss = new TSS($tss_file, false);
	$text_base = $tss->header_raw[2];
	
	//$fexpanded = null;
	
	foreach ($tss->extractDATA(true) as $block_hash => $block) {
		$room_id = &$block_hashes[$block_hash];
		if (!isset($room_id)) {
			$room_id = -999999999999;
		}
		//echo "  {$room_id}\n";
		$text_id = 0;
		foreach ($block as $ref => $texts2) {
			fseek($tss->f, $ref);
			$textblock_ptr = $text_base + fread4($tss->f);

			fseek($tss->f, $textblock_ptr);
			$type = fread4($tss->f);
			$unk  = fread4($tss->f);
			$text_ptr = array(); for ($n = 0; $n < 4; $n++) $text_ptr[$n] = fread4($tss->f);
			$text     = array(); for ($n = 0; $n < 4; $n++) $text[$n]     = addtext($text_ptr[$n]);
			
			// Remove japanese text.
			//$text[1] = $text[0] = '';
			
			//echo "'{$spanish_text_body}'\n";
			//echo $text;
			//exit;
			
			$acmeEntry = null;
			
			$translated_body = &$full_script_es[$room_id][$text_id]->text;
			if (!isset($translated_body)) {
				//echo "ROOM_ID: {$room_id}, TEXT_ID: {$text_id}\n";
				//print_r($text);
				//exit;
				
				$english_text_title = $text[2];
				$english_text_body  = $text[3];
				//echo "'{$english_text_body}'\n";
				if (!isset($try_to_map_texts[$english_text_body])) {
					list($text[4], $text[5]) = array('', '');
				} else {
					$spanish_text_title = @$try_to_map_texts[$english_text_title]['text'];
					$spanish_text_body = @$try_to_map_texts[$english_text_body]['text'];
					
					$acmeEntry = @$try_to_map_texts[$english_text_body]['acmeEntry'];
					list($text[4], $text[5]) = array($spanish_text_title, $spanish_text_body);
				}
			} else {
				$acmeEntry = @$full_script_es[$room_id][$text_id];
				$translated_body = $translated_body;
				if (empty($text[2])) {
					$text[4] = '';
					$text[5] = processText($translated_body, $text[3]);
				} else {
					$list = explode("\n", $translated_body, 2);
					@$text[4] = processText($list[0], $text[2]);
					@$text[5] = processText($list[1], $text[3]);
				}
			}
			//echo "$translated_body\n";
			
			$text_block = array(
				'text_path' => sprintf('scenario/%04d', (int)$tss_basename),
				'text_id' => sprintf('%08X', $textblock_ptr),
				'linked_id' => md5($text[2] . $text[3]),
				'metainfo' => array(
					'textblock_ptr' => $textblock_ptr,
				),
				'texts' => array(
					'ja' => array(unprocessText($text[0]), unprocessText($text[1])),
					'en' => array(unprocessText($text[2]), unprocessText($text[3])),
					'es' => array(unprocessText($text[4]), unprocessText($text[5])),
				),
				'translated' => array(
					'ja' => null,
					'en' => null,
					'es' => null,
				),
				'revised' => array(
					'ja' => null,
					'en' => null,
					'es' => null,
				),
			);
			
			if ($acmeEntry !== null) {
				//echo "acmeEntry\n";
				$text_block['translated']['es'] = array(
					'done' => $acmeEntry->translated,
					'user' => $acmeEntry->user_translated,
					'time' => $acmeEntry->time,
				);

				$text_block['revised']['es'] = array(
					'done' => $acmeEntry->revised,
					'user' => $acmeEntry->user_revised,
					'time' => $acmeEntry->time,
				);
			}
			
			for ($n = 0; $n < 2; $n++) {
				$text_block['texts']['es'][$n] = pitfallFixer($text_block['texts']['es'][$n], $text_block['texts']['en'][$n]);
			}
			
			//print_r($text_block);
			
			if (
				(!empty($text_block['texts']['en'][1]) && empty($text_block['texts']['es'][1]))
				|| (!empty($text_block['texts']['en'][0]) && empty($text_block['texts']['es'][0]))
			) {
				//print_r($text_block);

				//if (0)
				{
					$text_block_text = "{$text_block['texts']['en'][0]}\n{$text_block['texts']['en'][1]}";
					$repeated_texts[$text_block_text] = $text_block_text;
					//echo $text_block;
					//echo "\n";
					//echo "\n";
				}
			}
			/*
			if ($fexpanded === null) {
				$fexpanded = fopen($expanded_file_name = "expanded_script/{$tss_basename}.txt", 'wb');
			}
			fprintf($fexpanded, "%s\n", json_encode($text_block));
			*/
			fprintf($fexpanded_global, "%s\n", json_encode($text_block));
			//echo json_encode($text) . "\n";
			//print_r($text);
			
			$text_id++;
			
			for ($n = 0; $n < 4; $n++) {
				$text_ptr_ptr = $textblock_ptr + 8 + 4 * $n;
				//if (isset($ctexts[$text_ptr_ptr])) $text[$n] = $ctexts[$text_ptr_ptr];
				//$ptr_text = $segments->write_ptr($text_ptr_ptr, $text[$n]);
				//if (basename($file) == '0501.dat.u') printf("%08X\n", $text_base + $ptr_text);
			}
		}
		//print_r($block);
	}
}

/*
foreach ($repeated_texts as $text) {
	echo "{$text}\n";
}
*/