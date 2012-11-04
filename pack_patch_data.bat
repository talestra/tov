@echo off
"%~dp0\tools\7z" a -tzip -mx=0 PatchData.zip -r -x!.git -x!obj -x!project/obj -xr!*.psd -xr!*.iros -x!tools -x!deploy.bat -x!.DS_Store PatchData
"%~dp0\tools\lzma" e PatchData.zip PatchData.zip.lzma -d25 -mt4