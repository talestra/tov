@echo off
"%~dp0\tools\7z" a -tzip -mx=9 PatchData.zip -r -x!.git -x!obj -x!project/obj -xr!*.psd -xr!*.iros -x!tools -x!deploy.bat -x!.DS_Store -x!todo.txt -x!tov.json PatchData
REM "%~dp0\tools\lzma" e PatchData.zip PatchData.zip.lzma -d25 -mt4
"%~dp0\tools\CutreXORer" -e PatchData.zip
move /Y "PatchData.zip" "TalesOfVesperiaUtils\TalesOfVesperiaFrontendWPF\PatchData.bin"