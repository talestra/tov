<?php
$find = "For the people are no match";
//$find = "Look! The aque blastia";
//$find = "Ted\0";
//$find = "VC001";

if (stripos(file_get_contents('string_dic_uk.so'), $find) !== false) {
	echo "string!\n";
}

foreach (glob('scenario_uk/*.u') as $file) {
	if (stripos(file_get_contents($file), $find) !== false) {
		echo "$file\n";
	}
}
?>