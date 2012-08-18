<?php
	function parse_num($v) {
		if (substr($v, 0, 2) == '0x') return hexdec(substr($v, 2));
		//if (substr($v, 0, 1) == '0') return octdec($v);
		return $v;
	}
	class EMU {
		public $labels = array();
		public $code = array();
		public $R = array();
		public $SPR = array();
		public $CR = array();
		public $PC = 0x00000000;
		
		function __construct() {
			for ($n = 0; $n < 32; $n++) $this->R[$n] = 0x00000000;
			for ($n = 0; $n < 8; $n++) $this->CR[$n] = 0x00000000;
			$this->SPR['CTR'] = 0x00000000;
			$this->SPR['LR']  = 0x00000000;
			$this->SPR['XER'] = 0x00000000;
		}

		function parse($lines) {
			foreach ($lines as $line) {
				list($line) = array_map('trim', explode('#', $line, 2));
				if (!strlen($line)) continue;
				@list($addr, $i) = array_map('trim', explode(' ', $line, 2));
				if ($i === null || !strlen($i)) continue;
				if ($i[0] == '.') continue;
				@list(,$addr) = explode(':', $addr, 2); $addr = hexdec($addr);
				// Label
				if (substr($i, -1) == ':') {
					$this->labels[substr($i, 0, -1)] = $addr;
					continue;
				}
				@list($op, $params) = array_map('trim', explode(' ', $i, 2));
				$params = array_map(function($v) { return str_replace('%r', '', trim($v)); }, preg_split('/[,\\(\\)]/', $params));
				$this->code[$addr] = array($op, $params);
			}
		}
		
		function execute_i($i) {
			//print_r($i);
			if (method_exists($this, "I_{$i[0]}")) {
				call_user_func_array(array($this, "I_{$i[0]}"), $i[1]);
			} else {
				echo "Unknown {$i[0]}\n";
			}
			$this->PC += 4;
		}
		
		function execute($pc) {
			$this->PC = $pc;
			for ($n = 0; $n < 1000; $n++) {
				if (!isset($this->code[$this->PC])) {
					echo "Invalid adress {$this->PC}\n";
					//continue;
					break;
				}
				$this->execute_i($this->code[$this->PC]);
			}
		}
		
		function jump($l) {
			$this->PC = $this->labels[$l] - 4;
		}
		
		function I_mflr($r) { $this->R[$r] = $this->SPR['LR']; }
		function I_li($r, $v) { $this->R[$r] = parse_num($v); }
		function I_bl($l) { $this->jump($l); }
	}
	
	$emu = new EMU();
	$emu->parse(file('comp1.asm'));
	$emu->execute(0x820D51C0);
?>