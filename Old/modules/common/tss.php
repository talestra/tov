<?php
require_once(__DIR__ . '/utils.php');

// & XX02 + extend data

// 040C0018 00003EAC

// 0205XXXX            - ID - 16 bits
// 02070000 + XXXXXXXX - ID - 32 bits
// 0E000007            - JAP
// 0E000083            - LOC
// 02820000 + XXXXXXXX - TEXT_PTR

/*
Header (big endian):
4 bytes - MAGIC - Default: TSS\0
4 bytes - Start stream - 0x20
4 bytes - Script entry point (0x11)
4 bytes - Text Start / Script end
4 bytes - PTR ??? Stream (0x11)
4 bytes - PTR END Stream (0x00)
4 bytes - Text size
4 bytes - Unknown - Always 0x800

Stream types:
0x00_000000 - End?
0x10_000000 - Type1?
0x11_000000 - Type2?

After a 0x10_000000 follows a 32 bit value, indicating that valures referring to that stream will contain those bits.
*/

class TSS_OldBad extends TSS {
	protected function readHeader() {
		TSS::readHeader();
		$this->header->code_end = $this->header->code_start;
	}
}

class TSS {
	public $f;
	public $header_raw;
	public $header;
	public $ptrs;
	public $use_id;
	public $ptrs_real_count;
	public $script_ptrs, $script_ptrs2;
	public $used_ptr2;
	public $dialogs = array();

	static public $translate = array(
		"\x01" => "<01>",
		"\x02" => "<02>",
		"\x03" => "<03>",
		"\x04" => "<STR>",
		"\x05" => "<05>",
		"\x06" => "<06>",
		"\x07" => "<07>",
		"\x08" => "<08>",
		"\x09" => "<VOICE>",
		"\x0C" => "<PAGE>",
	);

	public function __construct($name, $use_id = false) {
		//echo "TSS('{$name}')\n";
		$this->use_id = $use_id;
		$this->f = fopen($name, 'rb');
		$this->readHeader();
		$this->locatePTRS();
	}

	protected function readHeader() {
		if (fread($this->f, 4) != "TSS\0") throw(new Exception("Not a TSS file"));
		/*
			uint_be script_start;
			uint_be code_start;
			uint_be text_start;
			uint_be entry_script_start;
			uint_be entry_ptr_end;
			uint_be text_len;
			uint_be sector_size;
		*/
		$this->header_raw = array_values(unpack('N7', fread($this->f, 7 * 4)));
		$this->header = (object)array();
		list(
			$this->header->script_start,
			$this->header->code_start,
			$this->header->text_start,
			$this->header->entry_script_start,
			$this->header->entry_ptr_end,
			$this->header->text_len,
			$this->header->sector_size,
		) = $this->header_raw;

		$this->header->code_end = $this->header->text_start;
	}
	
	public function locatePTRS() {
		if (count($this->ptrs) != 0) return;
		$f = &$this->f;
		$id = $lang = 0;
		$this->ptrs = array();
		$this->script_ptrs = array();
		$this->ptrs_real_count = 0;

		fseek($f, $this->header->script_start);
		if ($this->header->text_start < $this->header->script_start) {
			//echo "Assert! ({$this->header->code_start} < {$this->header->script_start})\n";
			return;
		}
		$list = array_values(unpack('N*', fread($f, $this->header->code_end - $this->header->script_start)));
		
		for ($n = 0, $l = count($list); $n < $l; $n++) {
			$v = $list[$n];
			switch ($v >> 24) {
				case 0x0E:
					switch ($v & 0xFFFFF) {
						case 0x07: $lang = 1; break;
						case 0x83: $lang = 2; break;
					}
				break;
				// Type.
				case 0x11:
					$this->ptrs[$this->header->script_start + ($n + 0) * 4] = 'separator';
				break;
				case 0x04:
					// Dialog. Using PTR1.
					if ($v == 0x040C0018) {
						$ptr = $list[$n + 1];
						
						$this->script_ptrs[] = $script_ptr = ($this->header->script_start + ($n + 1) * 4);

						fseek($f, $this->header->text_start + $ptr);
						$type = fread4($f);
						//for ($m = 0; $m < $count - 1; $m++) $unk = fread4($f);
						
						$this->dialogs[$ptr] = array();

						for ($m = 0; $m < 5; $m++) {
							$current_pos = ftell($f);
							$c_ptr = fread4($f);
							if ($m >= 3) {
								if ($c_ptr != 0) {
									$this->ptrs[$current_pos] = array(
										'id'   => $id,
										'script_ptr' => $script_ptr,
										'lang' => $lang,
										'ptr'  => $c_ptr,
										'type' => $type,
										'num'  => $m,
										'end'  => ($m == 4),
									);
									$this->ptrs_real_count++;
								}
							}
							$this->dialogs[$ptr][$current_pos] = $c_ptr;
							
							if (!$this->use_id) $id++;
						}
					}
				break;
				case 0x05:
					// Dialog. (Extra?) Using PTR2.
					//printf("%08X\n", $v);
					if ($v == 0x0502FFFF) {
						// FIX!
						break; // Not using this one atm.
					
						$ptr = $list[$n + 1];
						if ($ptr < 0) break;
						
						fseek($f, $this->header->entry_ptr_end + $ptr);
						$type = fread4($f);
						// Invalid ptr?
						//if ($type >= 0x10) break;
						if ($type != 0x02) break;
						
						$cptr = $this->header->entry_ptr_end + $ptr;
						
						if (isset($this->used_ptr2[$cptr])) break;
						$this->used_ptr2[$cptr] = true;

						$this->script_ptrs2[] = ($this->header->script_start + ($n + 1) * 4);
						
						
						/*
						printf("%08X\n", $type);
						//printf("%08X\n", $ptr);
						//printf("%08X\n", $this->header->text_start + $ptr);
						*/
						if (0) {
							printf(
								"%08X: %08X\n",
								($this->header->script_start + ($n + 1) * 4),
								$this->header->entry_ptr_end + $ptr
							);
						}
						//print_r($this->script_ptrs);
						//break;
						//for ($m = 0; $m < $count - 1; $m++) $unk = fread4($f);

						for ($m = 0; $m < 4; $m++) {
							$c_ptr = fread4($f);
							if ($m >= 2) {
								if ($c_ptr != 0) {
									$this->ptrs[ftell($f) - 4] = array(
										'id'   => $id,
										'lang' => $lang,
										'ptr'  => $c_ptr,
										'type' => $type,
										'end'  => ($m == 4),
									);
									$this->ptrs_real_count++;
									//printf("  %08X\n", $c_ptr);
								}
							}
							
							if (!$this->use_id) $id++;
						}
					}
				break;
				case 0x02:
					// Texto
					if ($v == 0x02820000) {
						$ptr = $list[$n + 1];
						
						// 39CF8
						if ($this->use_id)
						{
							$this->ptrs[$this->header->script_start + ($n + 1) * 4] = array(
								'id'   => $id,
								'lang' => $lang,
								'ptr'  => $ptr,
								'end'  => false,
							);
							$this->ptrs_real_count++;
						}
						
						if (!$this->use_id) $id++;
						
						//printf("%08X %08X %08X\n", $id, $lang, $ptr);
						
						//$lang = 0;
						//$id = 0;
					}
					//
					else {
						if ($this->use_id) {
							$v2 = ($v >> 20);
							if ($v2 == 0x020) {
								// extends
								if (($v >> 16) & 2) {
									$id = $list[$n + 1];
								}
								// have
								else {
									$id = $v & 0xFFFF;
								}
							}
						}
					}
				break;
			}
		}
	}
	
	public function emitSeparator() {
		echo "---------------------------------------------\n";
	}
	
	public function emitId($id) {
		printf("%08d:", $id);
	}

	public function emitString($text) {
		echo "'" . addcslashes($text, "\0..\37") . "'\n";
	}

	public function emitIdString($id, $text) {
		$this->emitId($id);
		$this->emitString($text);
	}

	public function emitEnd() {
		echo "\n";
	}

	public function extractTEXT() {
		$f = &$this->f;
		$text_start = $this->header->text_start;
		$last_separator = false;
		foreach ($this->ptrs as $key => $ptr) {
			if (($ptr == 'separator') && !$last_separator) {
				$this->emitSeparator();
				$last_separator = true;
				continue;
			}
			
			$text_ptr = $text_start + $ptr['ptr'];
			fseek($f, $text_ptr);
			$text = strtr(freadsz($f), self::$translate);
			if ($this->use_id) {
				if (!strlen($text)) continue;
				//$this->emitId($ptr['id']);
				$id = $ptr['id'];
			} else {
				if ($ptr['ptr'] == 0) continue;
				//$this->emitId($key);
				$id = $key;
			}
			//addcslashes($not_escaped, "\0..\37!@\177..\377");
			$this->emitIdString($id, $text);
			//$this->emitString($text);
			if ($ptr['end']) $this->emitEnd();
			
			$last_separator = false;
		}
	}

	public function extractDATA($set_hashes = false) {
		$f = &$this->f;
		$text_start = $this->header->text_start;
		$last_separator = false;
		$blocks = array();
		$strs = array();
		$last_ptr = array();
		foreach ($this->ptrs as $key => $ptr) {
			if (($ptr == 'separator') && !$last_separator) {
				$blocks[] = array();
				//echo "---------------------------------------------\n";
				$last_separator = true;
				continue;
			}
			
			$text_ptr = $text_start + $ptr['ptr'];
			fseek($f, $text_ptr);
			$text = strtr(freadsz($f), self::$translate);
			if ($this->use_id) {
				if (!strlen($text)) continue;
				$blocks[count($blocks) - 1][$text_ptr] = array($ptr['id']);
				//printf("%08d:", $ptr['id']);
				//exit;
			} else {
				if ($ptr['ptr'] == 0) continue;
				//printf("%08X:", $key);
				if ($set_hashes) {
					//echo "$key\n"; print_r($ptr);
					//$strs[$ptr['ptr']] = $text;
					$strs[$ptr['ptr']] = $text;
					$last_ptr = $ptr;
				} else {
					$strs[] = $text;
				}
			}
			//addcslashes($not_escaped, "\0..\37!@\177..\377");
			//echo "'" . addcslashes($text, "\0..\37") . "'\n";
			if ($ptr['end']) {
				if ($set_hashes) {
					//$blocks[$last_ptr['script_ptr']][] = $strs;
					$blocks[count($blocks) - 1][$last_ptr['script_ptr']] = $strs;
				} else {
					$blocks[count($blocks) - 1][] = $strs;
				}
				$strs = array();
				//echo "\n";
			}
			
			$last_separator = false;
		}
		if ($set_hashes) {
			$blocks_hash = array();
			foreach ($blocks as $block) {
				$hash = TSS::hash_block($block);
				if ($hash === null) continue;
				$blocks_hash[$hash] = $block;
			}
			return $blocks_hash;
		} else {
			return $blocks;
		}
	}

	static function hash_block($block) {
		$str = joinrecursive($block);
		if (!strlen($str)) return null;
		$hash = sprintf('%s_%s_%s', md5($str), sha1($str), strlen($str));
		return $hash;
	}
}

class Segments {
	public $segments = array();
	public $texts = array();
	public $base, $f;
	public $write_clear = array();
	public $write_ptrs = array();
	public $write_texts = array();
	
	function __construct($base = 0, $f = null) {
		$this->base = $base;
		$this->f = $f;
	}
	
	function __destruct() {
		$this->simplify_vector($this->write_clear);
		foreach ($this->write_clear as $v) {
			list($start, $end) = $v;
			fseek($this->f, $start);
			fwrite($this->f, str_repeat("\0", $end - $start));
		}
		
		foreach ($this->write_texts as $ptr => $text) {
			fseek($this->f, $ptr);
			fwrite($this->f, $text);
		}
		foreach ($this->write_ptrs as $ptr => $ptr_value) {
			fseek($this->f, $ptr);
			fwrite4($this->f, $ptr_value);
		}
	}

	static public function simplify_vector(&$s) {
		usort($s, function($a, $b) { return $a[0] - $b[0]; });
		for ($n = 1, $l = count($s); $n < $l; $n++) {
			if ($s[$n - 0][0] <= $s[$n - 1][1]) {
				$s[$n - 0] = array($s[$n - 1][0], $s[$n - 0][1]);
				$s[$n - 1] = null;
			}
		}
		$s = array_values(array_filter($s, function($v) { return $v !== null; }));
		return $s;
	}
	
	public function simplify() {
		Segments::simplify_vector($this->segments);
	}
	
	public function write_ptr($ptr_ptr, $text) {
		$text .= "\0";
		$ptr_text = $this->reserve($text);
		$this->write_texts[$this->base + $ptr_text] = $text;
		$this->write_ptrs[$ptr_ptr] = $ptr_text;
		return $ptr_text;
	}
	
	function removelen_simple($len) {
		foreach ($this->segments as $k => &$s) {
			$start = &$s[0];
			$end   = &$s[1];
			if (($end - $start) >= $len) {
				$start_back = $start;
				$start += $len;
				return $start_back;
			}
		}
		//print_r($this);
		throw(new Exception("Not enought space!"));
	}
	
	public function add($start, $end, $clear = false) {
		assert('$start <= $end');
		$this->segments[] = array($start, $end);
		if ($clear) $this->write_clear[] = array($this->base + $start, $this->base + $end);
	}
	
	public function addlen($start, $len, $clear = true) {
		return $this->add($start, $start + $len, $clear);
	}
	
	public function reserve($text) {
		if (!isset($this->texts[$text])) $this->texts[$text] = $this->removelen_simple(strlen($text));
		return $this->texts[$text];
	}
}

function joinrecursive($var) {
	if (is_array($var)) {
		$ret = '';
		foreach ($var as $n) $ret .= joinrecursive($n);
		return $ret;
	} else {
		return (string)$var;
	}
}

