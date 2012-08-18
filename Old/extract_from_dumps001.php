<?php
	// common.svo:15E000
	// crash1_xboxkrnl.exe.dmp:2902EEC
	// 00 54 98 0F
	// 01 62 6E 00
	$f = fopen('common.svo', 'rb');
	fseek($f, 0x0015E000);
	file_put_contents('data.cc', fread($f, 0x0054980F + 0x34));

	$f = fopen('crash1_xboxkrnl.exe.dmp', 'rb');
	fseek($f, 0x02902EEC);
	file_put_contents('data.uu', fread($f, 0x01626E00));
?>