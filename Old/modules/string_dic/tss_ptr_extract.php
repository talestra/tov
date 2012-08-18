<?php
require_once(__DIR__ . '/../common/tss.php');

$tss = new TSS('h:/games/vesperia/language/string_dic_uk.so.bak', true);
ob_start();
$data = $tss->extractTEXT();
file_put_contents('../../texts/strings_uk.txt', ob_get_clean());

$tss = new TSS('h:/games/vesperia/language/string_dic_us.bak', true);
ob_start();
$data = $tss->extractTEXT();
file_put_contents('../../texts/strings_us.txt', ob_get_clean());