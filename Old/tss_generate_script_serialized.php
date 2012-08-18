<?php
require_once(__DIR__ . '/tss.php');

//$tss = new TSS('string_dic_uk.so', true); $tss->extractTEXT(); exit;

//$tss = new TSS('scenario_es/0500.dat.u', false); $tss->extractTEXT(); exit;

$blocks = array();

foreach (glob('scenario_uk/*.dat.u') as $file) {
	$tss = new TSS($file, false);
	list($base) = explode('.', basename($file));
	$blocks[$base] = $tss->extractDATA();
	//$f = fopen("SRC/{$base}.txt", 'wb');
	if ($tss->ptrs_real_count) {
		/*
		echo "=====================================\n";
		echo "{$file}\n";
		echo "=====================================\n\n";
		$tss->extractTEXT();
		*/
		print_r($blocks[$base]);
	}
	//print_r($tss->extractDATA());
	//fclose($f);
	//break;
}
file_put_contents('script.serialized', serialize($blocks));
exit;
//$tss = new TSS('scenario_uk/0026.dat.u', false); $tss->extractTEXT();
//$tss = new TSS('string_dic_uk.so', true);