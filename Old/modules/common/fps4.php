<?php
require_once(__DIR__ . '/utils.php');

class FPS4 {
	public $f;
	public $files = array();

	public function __construct($file = NULL) {
		if ($file != NULL) $this->load($file);
	}
	
	public function load($file) {
		$this->f = $f = fopen($file, 'rb');
		if (fread($f, 4) != 'FPS4') throw(new Exception("Invalid FPS4"));
		$list_count   = fread4($f); // Number of elements in list
		$list_start   = fread4($f); // Start position of the list
		$list_end     = fread4($f); // End of the list / First file
		$entry_size   = fread2($f); // ??
		$entry_format = fread2($f); // ??
		$unk          = fread4($f); // ??
		$file_pos     = fread4($f); // Pointer to a string with the original filename
		fseek($f, $list_start);
		
		$entry_size_extra = $entry_size - 0x0C;
		
		for ($n = 0; $n < $list_count; $n++) {
			$file = (object)array();
			$file->offset        = fread4($f);
			$file->length_sector = fread4($f);
			$file->length_real   = fread4($f);
			
			switch ($entry_size_extra) {
				case 0:
					throw(new Exception("entry_size_extra: 0"));
				break;
				case 4:
					$file->name_offset = fread4($f);
					if ($file->name_offset == 0) {
						$file->index = $n;
						$file->name = sprintf('%04d', $n);
					} else {
						throw(new Exception("entry_size_extra: 4"));
					}
				break;
				default:
					$file->name = rtrim(fread($f, $entry_size_extra), "\0");
				break;
			}
			
			$this->files[] = $file;
			
			//printf("%08X, %08X, %08X: %s\n", $file_offset, $file_len_c, $file_len_u, $file_name);
		}
	}
	
	public function extractFile($file, $fullPath) {
		fseek($this->f, $file->offset);
		$data = fread($this->f, $file->length_real);
		file_put_contents($fullPath, $data);
	}
	
	public function extractTo($path, $filter = '*') {
		@mkdir($path, 0777, true);
		foreach ($this->files as $file) {
			if (!fnmatch($filter, $file->name)) continue;
			$fullPath = "{$path}/{$file->name}";
			echo "{$file->name}...";
			if (is_file($fullPath)) {
				echo "Already exists\n";
			} else {
				$this->extractFile($file, $fullPath);
				echo "Ok\n";
			}
		}
	}
}
