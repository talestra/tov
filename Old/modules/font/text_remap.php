<?php

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

function textRemap($text) {
	$text = strtr($text, array(
		"\xC3\xB1" => "\xC3\xA3",
		"\xC3\x91" => "\xC3\x83",
		"\xC2\xA1" => "\xC3\x8F",
		"\xC2\xBF" => "\xC3\x8B",
	));
	//echo "$text\n";
	return $text;
}
