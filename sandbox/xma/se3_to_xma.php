<?php

function fread4_be($f) { return unpack('N', fread($f, 4))[1]; }
function fread2_be($f) { return unpack('n', fread($f, 2))[1]; }

function fwrite4_be($f, $v) { fwrite($f, pack('N', $v)); }
function fwrite2_be($f, $v) { fwrite($f, pack('n', $v)); }
function fwrite4_le($f, $v) { fwrite($f, pack('V', $v)); }
function fwrite2_le($f, $v) { fwrite($f, pack('v', $v)); }

$file_in = '4';
$file_out = "{$file_in}.xma";
$f = fopen($file_in, 'rb');
fseek($f, 0xc);
$base_offset = fread4_be($f);
fseek($f, $base_offset, SEEK_SET);
fseek($f, $base_offset + 0x10, SEEK_SET);
$dataStart = fread4_be($f);

fseek($f, $base_offset + 0xEC, SEEK_SET);
$formatTag = fread2_be($f);
$wChannels = fread2_be($f);
$dwSamplesPerSec1 = fread2_be($f);
$dwSamplesPerSec2 = fread2_be($f);
$dwAvgBytesPerSec1 = fread2_be($f);
$dwAvgBytesPerSec2 = fread2_be($f);
$unk0 = fread4_be($f);
$unk1 = fread4_be($f);
$unk2 = fread4_be($f);
$unk3 = fread4_be($f);
$unk4 = fread2_be($f);
$unk5 = fread2_be($f);

fseek($f, $base_offset + $dataStart);
$data = fread($f, filesize($file_in));

$f = fopen($file_out, 'wb');
fwrite($f, 'RIFF');
fwrite4_le($f, 0);
fwrite($f, 'WAVE');
fwrite($f, 'fmt ');
fwrite4_le($f, 0x20);
fwrite2_le($f, $formatTag);
fwrite2_le($f, $wChannels);
fwrite2_le($f, $dwSamplesPerSec1);
fwrite2_le($f, $dwSamplesPerSec2);
fwrite2_le($f, $dwAvgBytesPerSec1);
fwrite2_be($f, $dwAvgBytesPerSec2);

fwrite4_le($f, $unk0);
fwrite4_le($f, $unk1);
fwrite4_le($f, $unk2);
fwrite4_le($f, $unk3);
fwrite2_be($f, $unk4);
fwrite2_le($f, $unk5);

fwrite($f, 'data');
fwrite4_le($f, strlen($data));
fwrite($f, $data);

$total_size = ftell($f);
fseek($f, 4);
fwrite4_le($f, $total_size);