<?php
$skit_a = array();
foreach (array_map('trim', file('skit_a.txt')) as $line) {
	list($k, $v) = explode(':', $line); $v = substr($v, 1, -1);
	$skit_a[$v] = $k;
}

$skit_b = array();
foreach (array_map('trim', file('skit_b.txt')) as $line) {
	list($k, $v) = explode(':', $line); $v = substr($v, 1, -1);
	$skit_b[$v] = $k;
}

$skit_ab = array();

foreach ($skit_b as $text => $file) {
	$id = $skit_a[$text];
	$skit_ab[$file] = $id;
}

file_put_contents('../string_dic/skit_ab.json', json_encode($skit_ab));

//print_r($skit_a);