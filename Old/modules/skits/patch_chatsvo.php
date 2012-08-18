<?php
$svo_file_src  = 'c:/isos/360/chat.svo.bak';
$svo_file_dst  = 'c:/isos/360/chat.svo';
$svo_patch_tst = 'chat.svo.d/VC700US.DAT';

if (!file_exists($svo_file_src)) die("Can't find '{$svo_file_src}'");
if (!file_exists($svo_file_dst)) die("Can't find '{$svo_file_dst}'");
if (!file_exists($svo_patch_tst)) die("Can't find '{$svo_patch_tst}'");

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

function fread4($f) { return (($v = unpack('N', fread($f, 4))) !== false) ? $v[1] : false; }
function freadsz($f, $max = 0x10000) { $s = ''; while (!feof($f) && (($c = fread($f, 1)) != "\0") && (strlen($s) < $max)) $s .= $c; return $s; }

class FPS4 {
	public $list = array();
	public $unk0, $unk1;
	public $ori_file_name;
	
	function __construct() {
	}
	
	function load($svo_file_name) {
		$f = fopen($svo_file_name, 'rb');
		if (fread($f, 4) != 'FPS4') throw(new Exception("Invalid FPS4"));
		$list_count = fread4($f); // Number of elements in list
		$list_start = fread4($f); // Start position of the list
		$list_end   = fread4($f); // End of the list / First file
		$this->unk0 = fread4($f); // ??
		$this->unk1 = fread4($f); // ??
		$file_pos   = fread4($f); // Pointer to a string with the original filename
		fseek($f, $file_pos);
		$this->ori_file_name = freadsz($f);
		fseek($f, $list_start);
		for ($n = 0; $n < $list_count; $n++) {
			$file_offset = fread4($f);
			$file_len_c  = fread4($f);
			$file_len_u  = fread4($f);
			$file_name   = rtrim(fread($f, 0x20), "\0");
			$this->list[$file_name] = array($svo_file_name, $file_offset, $file_len_c, $file_len_u);
		}
	}
	
	static function extract($entry) {
		list($file_name, $file_offset, $file_len_c, $file_len_u) = $entry;
		return file_get_contents($file_name, 0, NULL, $file_offset, $file_len_c);
	}
	
	static function align($pos, $align = 0x800) {
		return ceil($pos / $align) * $align;
	}
	
	function save($svo_file_name) {
		$f = fopen($svo_file_name, 'wb');
		$first_entry     = 0x1C;
		$ori_file_name  = $first_entry + 0x2c * count($this->list);
		$fist_file_data = self::align($ori_file_name + strlen($this->ori_file_name));
		
		fwrite($f, 'FPS4');
		fwrite($f, pack('N*', count($this->list)));
		fwrite($f, pack('N*', $first_entry));
		fwrite($f, pack('N*', $fist_file_data));
		fwrite($f, pack('N*', $this->unk0, $this->unk1));
		fwrite($f, pack('N*', $ori_file_name));
		
		$clist = array();
		fseek($f, $fist_file_data);
		foreach ($this->list as $file => $entry) {
			if (!is_array($entry)) {
				// alias
				$clist[$file] = $entry;
			} else {
				// Physical, not an alias.
				fseek($f, self::align(ftell($f)));
				$clist[$file] = array(ftell($f), $entry[2], $entry[3]);
				fwrite($f, self::extract($entry));
			}
		}

		fseek($f, $ori_file_name);
		fwrite($f, $this->ori_file_name . "\0");

		fseek($f, $first_entry);
		foreach ($clist as $file => $entry) {
			while (!is_array($entry)) {
				if (!isset($clist[$entry])) die("Error! '{$entry}'\n");
				$entry = $clist[$entry];
			}
			fwrite($f, pack('N*', $entry[0], $entry[1], $entry[2]));
			fwrite($f, str_pad($file, 0x20, "\0", STR_PAD_RIGHT));
			//print_r($entry); exit;
		}
	}
	
	function link($to, $from) {
		if ($to == $from || !isset($this->list[$from])) return false;
		$this->list[$to] = $from;
		return true;
	}
}

$fps = new FPS4();
echo "Loading...";
$fps->load($svo_file_src);
echo "Ok\n";

// Repoint FR, DE and UK.
echo "Linking...";
foreach ($fps->list as $file => $entry) {
	if (preg_match('@^VC(\\d+)(FR|UK|DE).DAT$@', $file, $matches)) {
		(
			$fps->link($file, "VC{$matches[1]}US.DAT")
			|| $fps->link($file, "VC{$matches[1]}UK.DAT")
		);
	}
}
echo "Ok\n";

echo "Linking2...";
{
	$up = &$fps->list['VC700US.DAT'];
	$up[0] = $svo_patch_tst;
	$up[1] = 0;
	$up[2] = filesize($svo_patch_tst);
}
echo "Ok\n";
//print_r($fps->list['VC700US.DAT']);
//exit;

echo "Saving...";
$fps->save($svo_file_dst);
echo "Ok\n";
//print_r($fps);
