<?php

ob_start();
printf('<div style="width:%dpx;">', 128 * 8);
foreach (glob('UI3/*.png') as $file) {
	printf('<img src="%s" style="width:128px;height:128px;"/>', basename($file));
}
printf('</div>');
$data = ob_get_clean();
file_put_contents('UI3/items.html', $data);