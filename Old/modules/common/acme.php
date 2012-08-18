<?php

require_once(__DIR__ . '/utils.php');

class AcmeEntry {
	// Basic
	public $id_file;
	public $id_text;
	public $text;

	// Info
	public $translated = false;
	public $revised = false;
	public $user_translated = NULL;
	public $user_revised = NULL;
	public $time = 0;
}

function parse_acme_folder_ex($folder, $cast_int_id_file = false, $text_filter = NULL, $file_filter = NULL, $use_acme_object = true) {
	$ret = array();
	foreach (glob("{$folder}/*.txt") as $file) {
		$id_file = substr(basename($file), 0, -4);
		if ($cast_int_id_file) $id_file = (int)$id_file;
		//$id_high = (int)substr(basename($file), 0, 4);
		$parts = explode('## POINTER ', file_get_contents($file));
		
		$fileTexts = array();
		
		foreach (array_slice($parts, 1) as $part) {
			@list($header, $body) = explode("\n", $part, 2);
			$id_text = (int)$header;
			//$id_low = (int)$header;
			$body = rtrim(str_replace("\r\n", "\n", $body));
			if ($text_filter !== NULL) {
				$body = $text_filter($body);
			}
			if ($use_acme_object) {
				$acmeEntry = new AcmeEntry();
				$acmeEntry->id_file = $id_file;
				$acmeEntry->id_text = $id_text;
				$acmeEntry->text = $body;
				
				if (preg_match_all('@(\\w+):(\\w+)[;|\\]]@', $header, $matches, PREG_SET_ORDER)) {
					foreach ($matches as $match) {
						list(, $k, $v) = $match;
						switch ($k) {
							case 't': $acmeEntry->translated = (bool)$v; break;
							case 'r': $acmeEntry->revised = (bool)$v; break;
							case 'user': $acmeEntry->user_translated = $v; break;
							case 'ru': $acmeEntry->user_revised = $v; break;
							case 'time': $acmeEntry->time = $v; break;
						}
					}
					/*
					for ($n = 0; $n < count($matches[0]); $n++) {
						$k = $matches[1][$n]; $v = $matches[2][$n];
						switch () {
						}
					}
					*/
				}
				
				//0 [t:1;r:1;ru:evilluendas;user:soywiz;time:1304288528]
				
				$fileTexts[$id_text] = $acmeEntry;
			} else {
				$fileTexts[$id_text] = $body;
			}
			//$id = $id_high * 10000 + $id_low;
			//printf("%08d:'%s'\n", $id, addcslashes($body, "\n\r\t\'\""));
		}
		if ($file_filter !== NULL) $fileTexts = $file_filter($id_file, $fileTexts);
		$ret[$id_file] = $fileTexts;
	}
	
	return $ret;
}

function parse_acme_folder($folder, $cast_int_id_file = false, $text_filter = NULL, $file_filter = NULL) {
	return parse_acme_folder_ex($folder, $cast_int_id_file, $text_filter, $file_filter, false);
}
