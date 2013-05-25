:<scriptpng>
@echo off
setlocal enabledelayedexpansion
color 0f
set "name=ScriptPNG"
set "version=01.03.2012"
title %name% - %version%
set "lib=%~dp0lib\"
path %lib%;%path%
if "%~1" equ "threat" call:threat-run "%~2" %3 %4 & exit /b
set "threatpng=4"
set "multithreat=1"
set "scriptname=%~0"

:<counters>
set "file=0"
set "files=0"
set "inputpngsize=0"
set "outputpngsize=0"
set "finaltotal=0"
set "process=0"
set "processto=25"

:<params>
set "logfile=%temp%\images.csv"
set "pngcount=%temp%\pngcount"
set "separe=__________________________________________________________________________"

:<execute>
if .%1==. (
echo.
echo.
echo  Usage   : To use %name%, just drag-and-drop your files
echo.
echo            on the %name% file
echo.
echo.
echo            ---------------------------------------
echo  Formats : BMP,GIF,JPG,PCX,PBM,PGM,PNG,PNM,TIF,TGA
echo            ---------------------------------------
echo.
echo.
pause >nul
goto:eof
)

:<check-folder>
for %%a in (%*) do (
call:source "%%~a"
if defined ispng (
if not defined isfolder (
set /a "files+=1"
) else (
for /f "delims=" %%i in ('dir /b /s /a-d-h "%%~a\*.png" 2^>nul ^| find /c /v "" ') do set /a "files+=%%i"
)
)
)
if "%files%" equ "0" set "multithreat=0"
set "params1="
for %%a in (%*) do (
set "err="
1>nul 2>nul dir "%%~a" || (
goto:eof
)
if not defined err (
call:source "%%~a"
if not defined ispng set "err=2"
if not defined err (
if defined ispng if not defined png call:interface "%%~a"
set "params1=!params1! %%a"
)
)
)

:<formats>
if not defined params1 (
echo.
echo.
echo  %name% - %version%
echo.
:formats-run
echo.
echo  In : "%~n1%~x1"
echo  Out: "%~n1.png"
if %~x1 equ .bmp (
pngoptimizer -file:"%~f1" >nul
truepng -quiet -zc9 -zm8-9 -zs0-3 -f0,5 -i0 -md remove all "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
truepng -quiet -nz -md remove all "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
if %~x1 equ .gif (
pngoptimizer -file:"%~f1" >nul
apngopt "%~dp0%~n1.png" "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
if %~x1 equ .jpg (
pngout -q -s3 -force -y "%~f1" >nul
truepng -quiet -zc9 -zm8-9 -zs0-3 -f0,5 -i0 -md remove all "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
truepng -quiet -nz -md remove all "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
if %~x1 equ .jpeg (
pngout -q -s3 -force -y "%~f1" >nul
truepng -quiet -zc9 -zm8-9 -zs0-3 -f0,5 -i0 -md remove all "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
truepng -quiet -nz -md remove all "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
if %~x1 equ .pcx (
pngout -q -s3 -force -y "%~f1" >nul
truepng -quiet -zc9 -zm8-9 -zs0-3 -f0,5 -i0 -md remove all "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
truepng -quiet -nz -md remove all "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
if %~x1 equ .tga (
pngout -q -s3 -force -y "%~f1" >nul
truepng -quiet -zc9 -zm8-9 -zs0-3 -f0,5 -i0 -md remove all "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
truepng -quiet -nz -md remove all "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
if %~x1 equ .tif (
optipng -quiet -zc1 -zm1 -zs1 -f5 "%~f1" >nul
truepng -quiet -zc9 -zm8-9 -zs0-3 -f0,5 -i0 -md remove all "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
truepng -quiet -nz -md remove all "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
if %~x1 equ .tiff (
optipng -quiet -zc1 -zm1 -zs1 -f5 "%~f1" >nul
truepng -quiet -zc9 -zm8-9 -zs0-3 -f0,5 -i0 -md remove all "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
truepng -quiet -nz -md remove all "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
if %~x1 equ .pbm (
optipng -quiet -zc1 -zm1 -zs1 -f5 "%~f1" >nul
truepng -quiet -zc9 -zm8-9 -zs0-3 -f0,5 -i0 -md remove all "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
truepng -quiet -nz -md remove all "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
if %~x1 equ .pgm (
optipng -quiet -zc1 -zm1 -zs1 -f5 "%~f1" >nul
truepng -quiet -zc9 -zm8-9 -zs0-3 -f0,5 -i0 -md remove all "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
truepng -quiet -nz -md remove all "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
if %~x1 equ .pnm (
optipng -quiet -zc1 -zm1 -zs1 -f5 "%~f1" >nul
truepng -quiet -zc9 -zm8-9 -zs0-3 -f0,5 -i0 -md remove all "%~dp0%~n1.png" >nul
advdef -q -z -4 "%~dp0%~n1.png" >nul
truepng -quiet -nz -md remove all "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
1>nul 2>nul defluff < "%~dp0%~n1.png" > "%~f1.000"
call:compare "%~dp0%~n1.png" "%~f1.000"
deflopt -k "%~dp0%~n1.png" >nul
echo  %separe%
goto:formats-next
)
cls
echo.
echo  %name% does not support this format.
echo.
pause >nul
goto:eof
:formats-next
shift
if .%1==. goto formats-end
goto formats-run

:formats-end
echo.
echo.
echo. Job List Finished.
echo.
title %name% - %version% - Finished.
pause >nul
goto:eof
)

:<temp-path>
if not exist "%temp%" 1>nul 2>&1 md "%temp%"
1>nul 2>&1 del /f /q "%temp%\*"
if %multithreat% neq 0 >"%logfile%" echo.
if not defined png set "png=1"
cls

:<first-echo>
echo.
echo.
echo  %name% - %version%
echo.
call:set-title
set start=%time%
for %%a in (%*) do (
call:source "%%~a"
if defined ispng if "%png%" neq "a" call:pnginfolder "%%~a"
)

:threat-wait
call:set-title
set "threat="
for /l %%z in (1,1,%threatpng%) do if exist "%temp%\threatpng%%z.lck" (set "threat=1") else call:typelog & call:set-title
if defined threat >nul 2>&1 ping -n 1 -w 500 127.255.255.255 & goto:threat-wait
call:end
pause>nul & exit /b

:threat-creation
call:set-title
if %2 equ 1 call:threat-run "%~3" %1 1 & call:typelog & exit /b
for /l %%z in (1,1,%2) do (
if not exist "%temp%\threat%1%%z.lck" (
call:typelog
>"%temp%\threat%1%%z.lck" echo : %~3
start "" /b /low cmd.exe /c ""%scriptname%" threat "%~3" %1 %%z "
exit /b
)
)
1>nul 2>&1 ping -n 1 -w 500 127.255.255.255
goto:threat-creation

:typelog
if exist "%temp%\typelog.lck" exit /b 
>"%temp%\typelog.lck" echo.typelog %typenum%
if not defined typenum set "typenum=1"
for /f "usebackq skip=%typenum% tokens=1-5 delims=;" %%b in ("%logfile%") do (
echo.
echo  "%%~b"
echo  In  : %%c Bytes
if %%d geq %%c (
echo  Out : %%d Bytes - Skipped
)
if %%d lss %%c (
echo  Out : %%d Bytes - %%e Bytes saved
)
echo  %separe%
set /a "typenum+=1"
)
1>nul 2>&1 del /f /q "%temp%\typelog.lck"
exit /b

:threat-run
call:set-title
if /i "%2" equ "png" call:pngrun %1 %3 & call:countmore "%pngcount%"
if exist "%temp%\threat%2%3.lck" >nul 2>&1 del /f /q "%temp%\threat%2%3.lck"
exit /b

:countmore
if %multithreat% equ 0 exit /b
call:waiting "%~1.lck"
>"%~1.lck" echo.%~1
>>"%pngcount%" echo.1
1>nul 2>&1 del /f /q "%~1.lck"
exit /b

:waiting
if exist "%~1" (1>nul 2>&1 ping -n 1 -w 500 127.255.255.255 & goto:waiting)
exit /b

:source
set "ispng="
set "isfolder="
1>nul 2>nul dir /ad "%~1" && set "isfolder=1"
if not defined isfolder (
if /i "%~x1" equ ".png" set "ispng=1"
) else (
1>nul 2>nul dir /b /s /a-d-h "%~1\*.png" && set "ispng=1"
)
exit /b

:interface
cls
echo.
echo.
echo  %name% - %version%
echo.
echo.
echo.
echo  [1] Fastest      [2] Fast      [3] Normal
echo.
echo.
echo  [4] Intense      [5] Max       [6] Xtreme      [0] Ultra
echo.
echo.
echo. [7] PNG24+A      [8] PNG8      [9] APNG
echo.
echo.
echo.

set png=
set /p png=# Enter an option [0-9]: 
echo.
exit /b

:set-title
set "file=0"
set /a process+=1
if %processto% lss %process% set /a processto+=25
for %%b in ("%pngcount%") do set /a "file=%%~zb/3" 2>nul
if %png% equ 0 set "mode=Ultra"
if %png% equ 1 set "mode=Fastest"
if %png% equ 2 set "mode=Fast"
if %png% equ 3 set "mode=Normal"
if %png% equ 4 set "mode=Intense"
if %png% equ 5 set "mode=Max"
if %png% equ 6 set "mode=Xtreme"
if %png% equ 7 set "mode=Color Quantization"
if %png% equ 8 set "mode=Convert to PNG8"
if %png% equ 9 set "mode=aPNG Optimizer"
title [%file%/%files%] %name% - %version% - %mode% : %process%/%processto%
exit /b

:pnginfolder
if defined isfolder (
for /f "delims=" %%i in ('dir /b /s /a-d-h "%~1\*.png" ') do (
call:threat-creation png %threatpng% "%%~fi"
set /a "file+=1" & call:set-title
)
) else (
call:threat-creation png %threatpng% "%~1"
set /a "file+=1" & call:set-title
)
exit /b

:pngrun
call:set-title
call:openlog "%~1"
copy /b /y "%~f1" "%~f1.bak" 1>nul 2>nul
if %png% equ 0 call:Ultra "%temp%\%~nx1" >nul
if %png% equ 1 call:Fastest "%temp%\%~nx1" >nul
if %png% equ 2 call:Fast "%temp%\%~nx1" >nul
if %png% equ 3 call:Normal "%temp%\%~nx1" >nul
if %png% equ 4 call:Intense "%temp%\%~nx1" >nul
if %png% equ 5 call:Max "%temp%\%~nx1" >nul
if %png% equ 6 call:Xtreme "%temp%\%~nx1" >nul
if %png% equ 7 call:PNG24Alpha "%temp%\%~nx1" >nul
if %png% equ 8 call:ConvertPNG8 "%temp%\%~nx1" >nul
if %png% equ 9 call:aPNGOpt "%temp%\%~nx1" >nul
1>nul 2>nul defluff < "%temp%\%~nx1" > "%temp%\%~nx1.tmp"
call :compare "%temp%\%~nx1" "%temp%\%~nx1.tmp"
deflopt -k "%temp%\%~nx1" >nul
1>nul 2>nul defluff < "%temp%\%~nx1" > "%temp%\%~nx1.tmp"
call :compare "%temp%\%~nx1" "%temp%\%~nx1.tmp"
deflopt -k "%temp%\%~nx1" >nul
1>nul move /y "%temp%\%~nx1" "%~f1"
if %png% lss 9 (
truepng -nz -md remove all "%~f1" >nul
)
call:compare "%~f1" "%~f1.bak" >nul
call:savelog "%~f1" !origsize!
exit /b

:compare
if %~z1 leq %~z2 (del /f /q %2 1>nul 2>nul) else (1>nul 2>nul move /y %2 %1)
exit /b

:check
set /a sizeout=%~z2
exit /b

:openlog
set "origsize=%~z1"
copy /b /y "%~f1" "%temp%\%~nx1" 1>nul 2>nul
exit /b

:savelog
set /a "change=%2-%~z1"
if %multithreat% neq 0 (
	if exist "%temp%\typelog.lck" (1>nul 2>&1 ping -n 1 -w 500 127.255.255.255 & goto:savelog)
	>"%temp%\typelog.lck" echo.savelog %~1
	>>"%logfile%" echo.%~n1.png;%2;%~z1;%change%
	1>nul 2>&1 del /f /q "%temp%\typelog.lck"
)
exit /b

:end
set finish=%time%
if %multithreat% neq 0 for /f "usebackq tokens=1-5 delims=;" %%a in ("%logfile%") do (
if /i "%%~xa" equ ".png" set /a "inputpngsize+=%%b" & set /a "outputpngsize+=%%c"
)
set /a "finaltotal=%inputpngsize%-%outputpngsize%" 2>nul
set /a "kilobytes=finaltotal/1024"

1>nul 2>&1 del /f /q "%logfile%" "%pngcount%"
echo.
echo.
echo  Total: %kilobytes% KB [%finaltotal% Bytes] saved.
echo.
echo.
echo  Started at  : %start%
echo  Finished at : %finish%
echo.
title %name% - %version% - Finished.
pause >nul
goto:eof

:Fastest
call:set-title
:: <script>
pngout -q -s4 -f0 -c6 -k0 -force "%~f1" >nul
truepng -quiet -zc8-9 -zm3-9 -zs0-3 -f0,5 -a0 -i0 -md remove all -force "%~f1" >nul
:: </script>
exit /b

:Fast
call:set-title
:: <script>
pngout -q -s4 -f0 -c6 -k0 -force "%~f1" >nul
truepng -quiet -zc8-9 -zm3-9 -zs0-3 -f0,5 -fs:7 -a0 -i0 -md remove all -force "%~f1" >nul
advdef -q -z -4 "%~f1" >nul
:: </script>
exit /b

:Normal
call:set-title
:: <script>
pngout -q -s4 -f0 -c6 -k0 -force "%~f1" >nul
truepng -quiet -zc8-9 -zm3-9 -zs0-3 -f0,5 -fs:7 -a1 -i0 -md remove all -force "%~f1" >nul
	pngout -q -l "%~f1" >"%~f1.00s"
	for /f "tokens=1 delims=/c" %%i in (%~f1.00s) do set color=%%i >nul
	del /a /f "%~f1.00s" 1>nul 2>nul
	set size-s3a=%~z1
	pngout -q -s3 "%~f1" >nul
	set size-s3b=%~z1
	if %size-s3b% lss %size-s3a% (
	for /l %%h in (1,1,10) do pngout -q -s3 -n%%h "%~f1" >nul
	exit /b
	)
	if %color% equ 2 (
	if %~z1 lss 1024 (
	truepng -quiet -zc9 -zm3-9 -zs2-3 -f1,2,4,5 -i0 -md remove all -force "%~f1" >nul
	pngout -q -ks -k0 -f6 "%~f1" >nul
	exit /b
	)
	)
pngout -q -ks -kp -k0 -f6 "%~f1" >nul
:: </script>
exit /b

:Intense
call:set-title
:: <script>
pngout -q -s4 -f0 -c6 -k0 -force "%~f1" >nul
truepng -quiet -zc8-9 -zm3-9 -zs0-3 -f0,5 -fs:7 -a1 -i0 -md remove all -force "%~f1" >nul
	pngout -q -l "%~f1" >"%~f1.00s"
	for /f "tokens=1 delims=/c" %%i in (%~f1.00s) do set color=%%i >nul
	del /a /f "%~f1.00s" 1>nul 2>nul
	set size-s3a=%~z1
	pngout -q -s3 "%~f1" >nul
	set size-s3b=%~z1
	if %size-s3b% lss %size-s3a% (
	for /l %%h in (1,1,10) do pngout -q -s3 -n%%h "%~f1" >nul
	exit /b
	)
	if %color% equ 2 (
	if %~z1 lss 1024 (
	truepng -quiet -zc9 -zm3-9 -zs2-3 -f1,2,4,5 -i0 -md remove all -force "%~f1" >nul
	pngout -q -ks -k0 -f6 "%~f1" >nul
	exit /b
	)
	)
pngout -q -ks -kp -k0 -f6 "%~f1" >nul
:: </script>
exit /b

:Max
call:set-title
:: <script>
pngout -q -s4 -f0 -c6 -k0 -force "%~f1" >nul
truepng -quiet -zc8-9 -zm3-9 -zs0-3 -fe -fs:7 -a1 -i0 -md remove all -force "%~f1" >nul
	pngout -q -l "%~f1" >"%~f1.00s"
	for /f "tokens=1 delims=/c" %%i in (%~f1.00s) do set color=%%i >nul
	del /a /f "%~f1.00s" 1>nul 2>nul
	set size-s3a=%~z1
	pngout -q -s3 "%~f1" >nul
	set size-s3b=%~z1
	if %size-s3b% lss %size-s3a% (
	for /l %%h in (1,1,10) do pngout -q -s3 -n%%h "%~f1" >nul
	exit /b
	)
	if %color% equ 2 (
	if %~z1 lss 1024 (
	truepng -quiet -zc9 -zm3-9 -zs2-3 -f1,2,4,5 -i0 -md remove all -force "%~f1" >nul
	pngout -q -ks -k0 -f6 "%~f1" >nul
	exit /b
	)
	)
pngout -q -ks -kp -k0 -f6 "%~f1" >nul
:: </script>
exit /b

:Xtreme
call:set-title
:: <script>
pngout -q -s4 -f0 -c6 -k0 -force "%~f1" >nul
set LargeFile=0
if %~z1 GTR %LargeFileSize% set LargeFile=1
set /a Huff_MaxBlocks=%~z1/256
if %Huff_MaxBlocks% GTR 512 set Huff_MaxBlocks=512
truepng -quiet -zc9 -zm9 -zs0-3 -fe -fs:7 -i0 -a1 -md remove all "%~f1" >nul
pngwolf --7zip-mpass=15 --zlib-level=9 --in="%~f1" --out="%~f1" >nul
pngout -q -k0 -ks -kp -f6 -s0 "%~f1" >nul
pngout -q -l "%~f1" >"%~f1.00s"
for /f "tokens=1 delims=/c" %%i in (%~f1.00s) do set color=%%i >nul
del /a /f "%~f1.00s" 1>nul 2>nul
	set size-s3a=%~z1
	pngout -q -s3 "%~f1" >nul
	set size-s3b=%~z1
	if %size-s3b% lss %size-s3a% (
	for /l %%h in (1,1,15) do pngout -q -s3 -n%%h "%~f1" >nul
	for /L %%i in (1,1,%Random_Trials%) do pngout -q -k1 -ks -s3 -r "%~f1" >nul	
	exit /b
	)
	if %color% equ 2 (
	if %~z1 lss 1024 (
	truepng -quiet -zc9 -zm3-9 -zs2-3 -f1,2,4,5 -i0 -md remove all -force "%~f1" >nul
	pngout -q -ks -k0 -f6 "%~f1" >nul
	goto T1_Step1_RGB
	)
	if %~z1 gtr 4096 (
	copy /b /y "%~f1.bak" "%~f1.w2.png" 1>nul 2>nul
	pngwolf --7zip-mpass=15 --zlib-level=9 --in="%~f1.w2.png" --out="%~f1.w2.png" >nul
	pngout -q -ks -k0 -f6 "%~f1.w2.png" >nul
	call :compare "%~f1" "%~f1.w2.png"
	goto T1_Step1_RGB
	)
	)
if %color% equ 0 goto T1_Step1_Gray
if %color% equ 4 goto T1_Step1_GrayA
if %color% equ 3 goto T1_Step1_Pal
if %color% equ 2 goto T1_Step1_RGB
if %color% equ 6 goto T1_Step1_RGBA
:T1_Step1_Gray
if %LargeFile%==0 (
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c0 -d1 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c0 -d2 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c0 -d4 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c0 -d8 "%~f1" >nul
)
if %LargeFile%==1 (
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c0 -d1 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c0 -d2 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c0 -d4 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c0 -d8 "%~f1" >nul
)
goto T1_Step2
:T1_Step1_GrayA
if %LargeFile%==0 for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c4 "%~f1" >nul
if %LargeFile%==1 for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c4 "%~f1" >nul
goto T1_Step2
:T1_Step1_Pal
if %LargeFile%==0 (
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c3 -d1 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c3 -d2 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c3 -d4 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c3 -d8 "%~f1" >nul
)
if %LargeFile%==1 (
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c3 -d1 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c3 -d2 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c3 -d4 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c3 -d8 "%~f1" >nul
)
goto T1_Step2
:T1_Step1_RGB
if %LargeFile%==0 for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c2 "%~f1" >nul
if %LargeFile%==1 for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c2 "%~f1" >nul
goto T1_Step2
:T1_Step1_RGBA
if %LargeFile%==0 for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c6 "%~f1" >nul
if %LargeFile%==1 for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c6 "%~f1" >nul
goto T1_Step2
:T1_Step2
if %LargeFile%==0 (
for %%i in (0,3) do for /L %%j in (0,1,5) do pngout -q -k1 -ks -s%%i -b256 -f%%j "%~f1" >nul
pngout -q -ks -k0 -f6 "%~f1" >nul
)
if %LargeFile%==1 (
for %%i in (0,256) do for /L %%j in (1,1,4) do pngout -q -k1 -ks -s1 -b%%i -f%%j "%~f1" >nul
for %%i in (128) do for /L %%j in (0,1,5) do pngout -q -k1 -ks -s1 -b%%i -f%%j "%~f1" >nul
start /belownormal /b /wait pngout -q -k1 -ks -s0 -n1 "%~f1" >nul
pngout -q -ks -k0 -f6 "%~f1" >nul
)
:T1_End
set Huff_Blocks=1
set Huff_Best=1
set Huff_Count=0
set Huff_Base=%~z1
:T2_Step1_Loop
set /a Huff_Blocks+=1
start /belownormal /b /wait pngout -q -k1 -ks -s3 -n%Huff_Blocks% "%~f1" >nul
pngout -q -k1 -ks -s0 -n%Huff_Blocks% "%~f1" >nul
if ERRORLEVEL 2 set /a Huff_Count+=1
if %~z1 LSS %Huff_Base% (
set Huff_Count=0
set Huff_Base=%~z1
set Huff_Best=%Huff_Blocks%
)
if %Huff_Blocks% GEQ %Huff_MaxBlocks% goto T2_Step2
if %Huff_Count% GEQ %Huffman_Trials% goto T2_Step2
goto T2_Step1_Loop
:T2_Step2
set /a Huff_Blocks=%Huff_Best%-1
set Huff_Count=1
if %Huff_Best% LEQ 1 (
set Huff_Blocks=1
set Huff_Count=2
)
:T2_Step2_Loop
for /L %%i in (1,1,10) do for %%j in (0,2,3) do pngout -q -k1 -ks -s%%j -n%Huff_Blocks% -r "%~f1" >nul
set /a Huff_Blocks+=1 & set /a Huff_Count+=1
if %Huff_Count% GTR 3 goto T2_End
goto T2_Step2_Loop
:T2_End
:: <script>
exit /b

:Ultra
:: <script>
pngout -q -s4 -f0 -c6 -k0 -force "%~f1" >nul
set LargeFile=0
if %~z1 GTR %LargeFileSize% set LargeFile=1
set /a Huff_MaxBlocks=%~z1/256
if %Huff_MaxBlocks% GTR 512 set Huff_MaxBlocks=512
truepng -quiet -zc9 -zm9 -zs0-3 -fe -fs:7 -i0 -a1 -md remove all "%~f1" >nul
pngwolf --7zip-mpass=15 --zlib-level=9 --in="%~f1" --out="%~f1" >nul
pngout -q -k0 -ks -kp -f6 -s0 "%~f1" >nul
pngout -q -l "%~f1" >"%~f1.00s"
for /f "tokens=1 delims=/c" %%i in (%~f1.00s) do set color=%%i >nul
del /a /f "%~f1.00s" 1>nul 2>nul
	set size-s3a=%~z1
	pngout -q -s3 "%~f1" >nul
	set size-s3b=%~z1
	if %size-s3b% lss %size-s3a% (
	for /l %%h in (1,1,15) do pngout -q -s3 -n%%h "%~f1" >nul
	for /L %%i in (1,1,%Random_Trials%) do pngout -q -k1 -ks -s3 -r "%~f1" >nul	
	exit /b
	)
	if %color% equ 2 (
	if %~z1 lss 1024 (
	truepng -quiet -zc9 -zm3-9 -zs2-3 -f1,2,4,5 -i0 -md remove all -force "%~f1" >nul
	pngout -q -ks -k0 -f6 "%~f1" >nul
	goto XT1_Step1_RGB
	)
	if %~z1 gtr 4096 (
	copy /b /y "%~f1.bak" "%~f1.w2.png" 1>nul 2>nul
	pngwolf --7zip-mpass=15 --zlib-level=9 --in="%~f1.w2.png" --out="%~f1.w2.png" >nul
	pngout -q -ks -k0 -f6 "%~f1.w2.png" >nul
	call :compare "%~f1" "%~f1.w2.png"
	goto XT1_Step1_RGB
	)
	)
if %color% equ 0 goto XT1_Step1_Gray
if %color% equ 4 goto XT1_Step1_GrayA
if %color% equ 3 goto XT1_Step1_Pal
if %color% equ 2 goto XT1_Step1_RGB
if %color% equ 6 goto XT1_Step1_RGBA
:XT1_Step1_Gray
if %LargeFile%==0 (
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c0 -d1 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c0 -d2 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c0 -d4 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c0 -d8 "%~f1" >nul
)
if %LargeFile%==1 (
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c0 -d1 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c0 -d2 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c0 -d4 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c0 -d8 "%~f1" >nul
)
goto XT1_Step2
:XT1_Step1_GrayA
if %LargeFile%==0 for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c4 "%~f1" >nul
if %LargeFile%==1 for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c4 "%~f1" >nul
goto XT1_Step2
:XT1_Step1_Pal
if %LargeFile%==0 (
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c3 -d1 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c3 -d2 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c3 -d4 "%~f1" >nul
for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c3 -d8 "%~f1" >nul
)
if %LargeFile%==1 (
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c3 -d1 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c3 -d2 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c3 -d4 "%~f1" >nul
for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c3 -d8 "%~f1" >nul
)
goto XT1_Step2
:XT1_Step1_RGB
if %LargeFile%==0 for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c2 "%~f1" >nul
if %LargeFile%==1 for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c2 "%~f1" >nul
goto XT1_Step2
:XT1_Step1_RGBA
if %LargeFile%==0 for %%i in (1,2) do for /L %%j in (0,1,5) do pngout -q -k1 -s0 -n%%i -f%%j -c6 "%~f1" >nul
if %LargeFile%==1 for %%i in (0,256) do for %%j in (0,5) do pngout -q -k1 -s1 -b%%i -f%%j -c6 "%~f1" >nul
goto XT1_Step2
:XT1_Step2
if %LargeFile%==0 (
for %%i in (0,3) do for /L %%j in (0,1,5) do pngout -q -k1 -ks -s%%i -b256 -f%%j "%~f1" >nul
pngout -q -ks -k0 -f6 "%~f1" >nul
)
if %LargeFile%==1 (
for %%i in (0,256) do for /L %%j in (1,1,4) do pngout -q -k1 -ks -s1 -b%%i -f%%j "%~f1" >nul
for %%i in (128) do for /L %%j in (0,1,5) do pngout -q -k1 -ks -s1 -b%%i -f%%j "%~f1" >nul
pngout -q -k1 -ks -s0 -n1 "%~f1" >nul
pngout -q -ks -k0 -f6 "%~f1" >nul
)
:XT1_End
set Huff_Blocks=1
set Huff_Best=1
set Huff_Count=0
set Huff_Base=%~z1
:XT2_Step1_Loop
set /a Huff_Blocks+=1
start /belownormal /b /wait pngout -q -k1 -ks -s3 -n%Huff_Blocks% "%~f1" >nul
pngout -q -k1 -ks -s0 -n%Huff_Blocks% "%~f1" >nul
if ERRORLEVEL 2 set /a Huff_Count+=1
if %~z1 LSS %Huff_Base% (
set Huff_Count=0
set Huff_Base=%~z1
set Huff_Best=%Huff_Blocks%
)
if %Huff_Blocks% GEQ %Huff_MaxBlocks% goto XT2_Step2
if %Huff_Count% GEQ %Huffman_Trials% goto XT2_Step2
goto XT2_Step1_Loop
:XT2_Step2
set /a Huff_Blocks=%Huff_Best%-1
set Huff_Count=1
if %Huff_Best% LEQ 1 (
set Huff_Blocks=1
set Huff_Count=2
)
:XT2_Step2_Loop
for /L %%i in (1,1,10) do for %%j in (0,2,3) do pngout -q -k1 -ks -s%%j -n%Huff_Blocks% -r "%~f1" >nul
set /a Huff_Blocks+=1 & set /a Huff_Count+=1
if %Huff_Count% GTR 3 goto XT2_End
goto XT2_Step2_Loop
:XT2_End
for /L %%i in (1,1,50) do pngout -q -k0 -ks -s0 -r "%~f1" >nul
:: </script>
exit /b

:PNG24Alpha
call:set-title
:: <script>
pngout -q -s4 -f0 -c6 -k0 -force "%~f1" >nul
truepng -info "%~f1" >1.txt
for /f "tokens=4 delims=|" %%i in (1.txt) do set numb=%%i
1>nul 2>nul set /a colors=%numb%
set /a finalcolor=%colors%/2
if %finalcolor% LSS 5000 set finalcolor=1000
truepng -quiet -zc8-9 -zm3-9 -zs0-3 -f0,5 -cq c=%finalcolor% -a1 -i0 -md remove all -force "%~f1" >nul
pngout -q -ks -kp -k0 -f6 "%~f1" >nul
del /a /f "1.txt" 1>nul 2>nul
:: </script>
exit /b

:ConvertPNG8
call:set-title
:: <script>
pngout -q -s4 -f0 -c6 -k0 -force "%~f1" >nul
truepng -quiet -zc8-9 -zm3-9 -zs0-3 -f0,5 -cq -a1 -i0 -md remove all -force "%~f1" >nul
pngout -q -ks -kp -k0 -f6 "%~f1" >nul
:: </script>
exit /b

:aPNGOpt
call:set-title
:: <script>
apngopt "%~f1" "%~f1" >nul
advdef -q -z -4 "%~f1" >nul
apng2pngseq "%~f1" >nul
for %%a in (frame*.png) do pngout -q -ks -kp %%a >nul
for %%a in (frame*.png) do deflopt -k "%%a"
move /y "apng2pngseq.txt" "pngseq2apng.txt" 1>nul 2>nul
pngseq2apng >nul
del /a /f "frame*.png" 1>nul 2>nul
del /a /f "pngseq2apng.txt" 1>nul 2>nul
:: </script>
exit /b