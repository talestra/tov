<?php
//Remove:00260000-00280000
$last_group = '';
$subgroup = 0;
$line_count = 0;
foreach (file('strings_uk.txt') as $line) { $line = trim($line);
	if (substr($line, 0, 1) == '-') continue;

	preg_match("@^(\\d+):'(.*)'$@", $line, $matches);
	list($group, $ptr) = array((int)($matches[1] / 10000), $matches[1] % 10000);
	$text = strtr(rtrim(stripcslashes($matches[2])), array('<PAGE>' => "<PAGE>\n", "\xB" => '<0B>'));

	// Script skip.
	if ($group == 26) continue;
	if ($group == 27) continue;
	
	$reopen = false;

	if ($group != $last_group) { $last_group = $group; $subgroup = 0; $reopen = true; printf("  dif_group\n"); }
	if ($line_count >= 500) { $reopen = true; $subgroup++; printf("  line_count(%d)>=200\n", $line_count); }

	if ($reopen) {
		$f = fopen($file = sprintf('SRC/%04d_%02d.txt', $group, $subgroup), 'wb');
		$line_count = 0;
		printf("Open...%s\n", $file);
	}
	//print_r($matches);
	
	fprintf($f, "## POINTER %d\n%s\n\n", $ptr, $text);
	$line_count += (substr_count($text, "\n") + 1) + 4;
}