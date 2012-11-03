<?php

@mkdir('images', 0777, true);
//$path = "E:/isos/360/games/vesperia/";
$path = "E:/isos/360/games/vesperia.pal.ori/";

$iterator = new RecursiveIteratorIterator(new RecursiveDirectoryIterator($path), RecursiveIteratorIterator::CHILD_FIRST);
foreach ($iterator as $file) {
	if (substr($file, -4) != ".png") continue;
	$data = file_get_contents($file);
	$data_md5 = md5($data);
	$data_md5_file = "images/{$data_md5}.png";
	if (!file_exists($data_md5_file)) {
		file_put_contents($data_md5_file, $data);
	}
	echo "$file\n";
}