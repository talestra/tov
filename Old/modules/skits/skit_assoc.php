<?php
function fread4($f) { return (($v = unpack('N', fread($f, 4))) !== false) ? $v[1] : false; }
function freadsz($f) { $s = ''; while (($c = fread($f, 1)) != "\0") $s .= $c; return $s; }
$skit_ids = array();
$skit_titles = array();
foreach (file('skits.txt') as $line) {
	list($id, $text) = array_map('trim', explode(':', $line, 2));
	$id = hexdec(substr($id, 1));
	$skit_ids[$id] = $text;
}
//print_R($skit_ids);
//exit;
$list = array();
//$f = fopen('skit_dat.bin', 'rb');

$f = fopen('CHAT.DAT.u', 'rb');

function extract_segment($type, $offset, $count) {
	global $f, $list;
	fseek($f, $offset);
	for ($n = 0; $n < $count; $n++) {
		list(,$a,$b,$c) = unpack('N3', fread($f, 12));
		$list[]  = array($type, $a, $b, $c);
	}
}

function extract_chtx($file) {
	list(,$start) = unpack('N', file_get_contents($file, 0, null, 0x4C, 4));
	$row = file_get_contents($file, 0, null, $start, 0x10);
	if (substr($row, 0, 8) != "TO8CHTX\0") die("Not a TO8CHTX\n");
	list(,$len) = unpack('N', substr($row, 8, 4));
	$data = file_get_contents($file, 0, null, $start, $len);
	if (preg_match('/^(.*)\\.DAT\\.u$/', $file, $matches)) {
		$name = "{$matches[1]}.chtx";
		file_put_contents($name, $data);
		//print_r($matches[1]);
		//printf("%08X\n", $start);
	}
}

function process_chtx($chtx_file) {
	//echo "$chtx_file\n";
	$retval = 0;
	$f = fopen($chtx_file, 'rb');
	if (fread($f, 8) != "TO8CHTX\0") die("Not a chtx\n");
	$file_end    = fread4($f);
	$text_count  = fread4($f);
	$table_start = fread4($f);
	$text_start  = fread4($f);
	fseek($f, $table_start);
	for ($n = 0; $n < $text_count; $n++) {
		$title_ptr = fread4($f);
		$text_jp   = fread4($f);
		$text_en   = fread4($f);
		$unk       = fread4($f);
		$back = ftell($f);
		{
			fseek($f, $text_start + $title_ptr); $title = freadsz($f);
			fseek($f, $text_start + $text_en); $text = freadsz($f);
			printf("## POINTER %d\n", $n + 1);
			echo str_replace("\4", '', sprintf("%s\n%s\n\n", $title, $text));
			if ($text == 'Dummy') $retval = 1;
		}
		fseek($f, $back);
		//echo file_get_contents($chtx_file, 0, null, $table_start + $text_ptr, $text_end - $text_ptr);
		//printf("%08X\n", ftell($f));
	}
	return $retval;
}
// extract_chtx();

@mkdir('skt/SRC', 0777, true);

extract_segment(0, 0x91A8, 0x3F0 / 12); // check
extract_segment(1, 0x981C, 0x504 / 12);
extract_segment(2, 0x9DC8, 0x150 / 12);
extract_segment(3, 0xA074, 0x2B8 / 12);
extract_segment(4, 0xA3C8, 0x138 / 12);
extract_segment(5, 0xA7B8, 0x570 / 12);
extract_segment(6, 0xAD78, 0x09C / 12);

//print_r($list);

$ctype = false;
foreach ($list as $e) {
	list($type, $title, $flag, $id) = $e;
	if ($type !== $ctype) {
		//printf("--------------------\n");
		$ctype = $type;
	}
	fseek($f, $title); $base = freadsz($f);
	
	echo "$type, $title, $flag, $id\n";
	
	//echo "$base\n";
/*
	ob_start();
	{
		printf("## POINTER %d\n", 0);
		echo str_replace("\4", '', sprintf("%s\n\n", $skit_ids[$id]));
		process_chtx("chtx/{$base}UK.chtx");
	}
	$data = ob_get_clean();
	file_put_contents(sprintf('skt/SRC/T%d@%s.txt', $type, $base), $data);
	*/
}

//printf("--------------------");
/*
foreach (glob("chtx/*.chtx") as $file) {
	list($base, $ext) = explode('.', substr($file, 5), 2);
	$base = substr($base, 0, -2);
	$acme_file = "T{$skit_titles[$base][0]}@{$base}";
	$skit_title = $skit_titles[$base][1];
	
	echo "$acme_file\n";
	
	//printf("%s\n", str_repeat('-', 79));
	//printf(" %s\n", $file);
	//printf(" %s\n", $skit_titles[$base][1]);
	
	//printf("%s\n\n", str_repeat('-', 79));
	//extract_chtx($file);
	if (process_chtx($file)) {
	}
}
*/