<?php

require_once(__DIR__ . '/utils.php');

// Ã -> Ñ
// ã -> ñ
// Ï -> ¡
// Ë -> ¿

// C3 B1 - ñ
// C3 91 - Ñ
// C2 A1 - ¡
// C2 BF - ¿

// C3 A3 - ã
// C3 83 - Ã	
// C3 8F - Ï
// C3 8B - Ë

function fix_utf8($text) {
	//var_dump(mb_detect_encoding($text));
	//echo "\n";
	$encoding_to = 'UTF-8';
	$encoding_from = mb_detect_encoding($text);
	if ($encoding_from === false) {
		$encoding_from = 'ISO-8859-1';
	}
	
	return mb_convert_encoding($text, $encoding_to, $encoding_from);
}

function pitfallFixer($text, $original_text) {
	$start_spaces = preg_match('@^\\s*@', $original_text);
	$end_spaces   = preg_match('@\\s*$@', $original_text);
	$text = $start_spaces[0] . trim($text) . $end_spaces[0];
	return $text;
}

function processText($text, $original_text = NULL) {
	if ($original_text === NULL) $original_text = $text;

	// Pitfall fixer
	$text = preg_replace('@\\s+<PAGE>\\s+@msi', '<PAGE>', $text);
	$text = preg_replace('@<VOICE>\\s+@msi', '<VOICE>', $text);
	
	// Comprobar dobles espacios
	//strpos('  ');

	$text = strtr($text, array(
		"\xC3\xB1" => "\xC3\xA3",
		"\xC3\x91" => "\xC3\x83",
		"\xC2\xA1" => "\xC3\x8F",
		"\xC2\xBF" => "\xC3\x8B",
		
		"<01>" => "\x01",
		"<02>" => "\x02",
		"<03>" => "\x03",
		"<STR>" => "\x04",
		"<05>" => "\x05",
		"<06>" => "\x06",
		"<07>" => "\x07",
		"<08>" => "\x08",
		"<VOICE>" => "\x09",
		"<0B>" => "\x0B",
		"<PAGE>" => "\x0C",
	));
	
	// Fix initial spaces
	if (preg_match('@^\\s+@', $original_text, $matches)) {
		$text = $matches[0] . ltrim($text);
	}
	
	// Fix end spaces
	if (preg_match('@\\s+$@', $original_text, $matches)) {
		$text = rtrim($text) . $matches[0];
	}
	
	//echo "$text\n";
	return $text;
}

function unprocessText($text) {
	$text = strtr($text, array(
		"\xC3\xA3" => "\xC3\xB1",
		"\xC3\x83" => "\xC3\x91",
		"\xC3\x8F" => "\xC2\xA1",
		"\xC3\x8B" => "\xC2\xBF",
		
		"\x01" => "<01>",
		"\x02" => "<02>",
		"\x03" => "<03>",
		"\x04" => "<STR>",
		"\x05" => "<05>",
		"\x06" => "<06>",
		"\x07" => "<07>",
		"\x08" => "<08>",
		"\x09" => "<VOICE>",
		"\x0B" => "<0B>",
		"\x0C" => "\n<PAGE>\n",
	));
	//echo "$text\n";
	return fix_utf8($text);
}

function quotec($text) {
	return addcslashes($text, "\n\r\t\'\"");
}

function fix_acme1_skits_texts($text) {
	$text = preg_replace_callback('@\\(\\w+\\)@', function($v) {
		return '<STR>' . $v[0];
	}, $text);
	$text = str_replace('<STR><STR>', '<STR>', $text);
	return mb_convert_encoding($text, 'UTF-8', 'ISO-8859-1');
}

function fix_acme1_page($text) {
	return fix_utf8(preg_replace('@\\s*<PAGE>\\s*@', "\n<PAGE>\n", $text));
}