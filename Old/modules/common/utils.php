<?php

function fread4($f) {
	$d = fread($f, 4);
	if (strlen($d) < 4) return false;
	return (($v = unpack('N', $d)) !== false) ? $v[1] : false;
}
function fread2($f) {
	$d = fread($f, 2);
	if (strlen($d) < 2) return false;
	return (($v = unpack('n', $d)) !== false) ? $v[1] : false;
}
function freadsz($f, $max = 0x100000) { $s = ''; while (!feof($f) && (($c = fread($f, 1)) != "\0") && (strlen($s) < $max)) $s .= $c; return $s; }
function fwrite4($f, $v) { fwrite($f, pack('N', $v)); }
function fsreadsz($f, $seek) { fseek($f, $seek); return freadsz($f); }

/*
function fread4($f) { return (($v = unpack('N', fread($f, 4))) !== false) ? $v[1] : false; }
function freadsz($f) { $s = ''; while (($c = fread($f, 1)) != "\0") $s .= $c; return $s; }
*/

function require_bak($file) {
	$file_bak = "{$file}.bak";
	if (!is_file($file_bak)) {
		copy($file, $file_bak);
	}
}
