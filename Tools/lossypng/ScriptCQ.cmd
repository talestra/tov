::##########################################################################################
:: Script Parameters
::##########################################################################################

:verbose
@echo off

::##########################################################################################
:: User Settings
::##########################################################################################
:: PNG32 Optimization
set AlphaChannelQuality=90
set Png32RgbFiltering=0
:: PNG8 and PNG32 Compression
set PngCompression7zip=1
set PngMaxCompression=0
:: Conversion to PNG8 with Alpha values
set Png8NumberOfColors=256
set Png8Dithering=0
set Png8DitheringValue=75
:: PNG8 Quantizer Settings
set Png8Quantizer=64
set Png8ErrorP=25
set Png8MaxError=8

::##########################################################################################
:: Script Parameters
::##########################################################################################

:script
setlocal enabledelayedexpansion
set pngdir="%~dp0lib\"
path %pngdir%;%path% >nul

:name
set soft=ScriptCQ
set vers=19.11.2011

:title
title %soft% - %vers%
color 0f

::##########################################################################################
:: User Interface if Execution
::##########################################################################################

:execute
if .%1==. (
echo.
echo.
echo  Usage   : To use %soft%, just drag-and-drop your files
echo.
echo            on the %soft% file
echo.
echo.
echo            ---
echo  Formats : PNG
echo            ---
echo.
echo.
pause >nul
goto end
)

::##########################################################################################
:: Counters, Check Formats
::##########################################################################################

:counters
set /a pc=0
set /a file=0
set /a files=0
set /a finaltotal=0
for %%i in (%*) do set /a files+=1
set separe=__________________________________________________________________________

:format
if /i %~x1 neq .png goto formats

::##########################################################################################
:: User Interface
::##########################################################################################

:iuser
echo.
echo.
echo  %soft% - %vers%
echo.
echo.
echo.
echo  [1] PNG32 Quality 90%%        [2] PNG32 Quality 80%%
echo.
echo.
echo  [3] PNG32 Quality 70%%        [4] PNG32 Quality 60%%
echo.
echo.
echo  [5] PNG32 Quality 50%%        [6] PNG32 Quality 40%%
echo.
echo.
echo  [7] PNG32 Quality 30%%        [8] Convert to PNG8
echo.
echo.
echo.

:choice
set /p number=# Enter an option [1-8]:  
if %number%==1 set qua=90& goto png32
if %number%==2 set qua=80& goto png32
if %number%==3 set qua=70& goto png32
if %number%==4 set qua=60& goto png32
if %number%==5 set qua=50& goto png32
if %number%==6 set qua=40& goto png32
if %number%==7 set qua=30& goto png32
if %number%==8 goto png8
cls
goto iuser

::##########################################################################################
:: PNG32
::##########################################################################################
:png32
cls
echo.
cecho {white} %soft% - %vers% - PNG32 [Q:{yellow}%qua%{white} A:{yellow}%AlphaChannelQuality%{white} F:{yellow}%Png32RgbFiltering%{white} 7:{yellow}%PngCompression7zip%{white} X:{yellow}%PngMaxCompression%{white}]
echo.
set start=%time%

:png32-run
if /i %~x1 neq .png goto formats
set /a file+=1
set /a sizein=%~z1
set name=%~n1
title %file%/%files% - %pc% %%
echo.
echo  "%name%.png"
echo  In : %sizein% Bytes

:: <script>
copy /b /y "%1" "%~dp0%name%-%qua%.png" 1>nul 2>nul
pngout -q -s4 -c6 -k0 -force "%~dp0%name%-%qua%.png" >nul
title %file%/%files% - 20 %% - %~z1 Bytes
	imagew -quiet -cccolor %qua% -ccalpha %AlphaChannelQuality% "%~dp0%name%-%qua%.png" "%~dp0%name%-%qua%.png" >nul
	title %file%/%files% - 40 %% - %~z1 Bytes
truepng -quiet -o1 "%~dp0%name%-%qua%.png" >nul
title %file%/%files% - 70 %% - %~z1 Bytes
set sizecurrent=%~z1
if %Png32RgbFiltering% equ 1 (
	pngout -q -c6 -s4 -force "%~dp0%name%-%qua%.png" >nul
	cryopng -i0 -zc9 -zm8 -zs1 -f1 -force "%~dp0%name%-%qua%.png" -out "%~dp0%name%-1.png" 1>nul 2>nul
	truepng -quiet -o1 -na "%~dp0%name%-1.png" >nul
	title %file%/%files% - 74 %% - %sizecurrent% Bytes
cryopng -i0 -zc9 -zm8 -zs1 -f4 -force "%~dp0%name%-%qua%.png" -out "%~dp0%name%-4.png" 1>nul 2>nul
truepng -quiet -o1 -na "%~dp0%name%-4.png" >nul
call :compare "%~dp0%name%-%qua%.png" "%~dp0%name%-1.png"
call :compare "%~dp0%name%-%qua%.png" "%~dp0%name%-4.png"
title %file%/%files% - 78 %% - %sizecurrent% Bytes
)
	if %PngCompression7zip% equ 1 (
	advdef -q -z -4 "%~dp0%name%-%qua%.png" >nul
	title %file%/%files% - 80 %% - %~z1 Bytes
	)
if %PngMaxCompression% equ 1 (
copy /b /y "%~dp0%name%-%qua%.png" "%~dp0%name%-t.png" 1>nul 2>nul
copy /b /y "%~dp0%name%-%qua%.png" "%~dp0%name%-w.png" 1>nul 2>nul
	truepng -quiet -i0 -zc9 -zm3-9 -zs0-3 -fe -nc -na -force "%~dp0%name%-t.png" >nul
	title %file%/%files% - 85 %% - %sizecurrent% Bytes
pngwolf --zlib-level=9 --zlib-memlevel=3-9 --zlib-strategy=0-1 --7zip-mpass=2 --in="%~dp0%name%-w.png" --out="%~dp0%name%-w.png" >nul
title %file%/%files% - 88 %% - %sizecurrent% Bytes
	pngout -q -s0 -ks -k1 -f6 "%~dp0%name%-t.png"
	title %file%/%files% - 94 %% - %sizecurrent% Bytes
pngout -q -s0 -ks -k1 -f6 "%~dp0%name%-w.png"
title %file%/%files% - 98 %% - %sizecurrent% Bytes
call :compare "%~dp0%name%-%qua%.png" "%~dp0%name%-t.png"
call :compare "%~dp0%name%-%qua%.png" "%~dp0%name%-w.png"
truepng -quiet -nz -md remove all "%~dp0%name%-%qua%.png" >nul
)
1>nul 2>nul defluff < "%~dp0%name%-%qua%.png" > "%~dp0%name%-1.png"
call :compare "%~dp0%name%-%qua%.png" "%~dp0%name%-1.png"
deflopt -k "%~dp0%name%-%qua%.png" >nul
1>nul 2>nul defluff < "%~dp0%name%-%qua%.png" > "%~dp0%name%-1.png"
call :compare "%~dp0%name%-%qua%.png" "%~dp0%name%-1.png"
deflopt -k "%~dp0%name%-%qua%.png" >nul
call :check "%1" "%~dp0%name%-%qua%.png"
:: </script>

:: <calcul>
set /a total=(%sizein%-%sizeout%)
if %sizeout% geq %sizein% (
cecho {white} Out: {red}%sizeout%{white} Bytes
echo.
echo  %separe%
)
if %sizeout% lss %sizein% (
cecho {white} Out:{lime} %sizeout%{white} Bytes - %total% Bytes saved
echo.
echo  %separe%
set /a finaltotal+=%total%
)
:: </calcul>

:: <file>
shift
if .%1==. goto close
set /a pc=0
goto png32-run
:: </file>

::##########################################################################################
:: PNG8
::##########################################################################################
:png8
cls
echo.
cecho {white} %soft% - %vers% - PNG8 [C:{yellow}%Png8NumberOfColors%{white} D:{yellow}%Png8Dithering%{white},{yellow}%Png8DitheringValue%{white} Q:{yellow}%Png8Quantizer%{white} E:{yellow}%Png8ErrorP%{white} Me:{yellow}%Png8MaxError%{white} 7:{yellow}%PngCompression7zip%{white} X:{yellow}%PngMaxCompression%{white}]
echo.
set start=%time%

:png8-run
if /i %~x1 neq .png goto formats
set /a file+=1
set /a sizein=%~z1
set name=%~n1
title %file%/%files% - %pc% %%
echo.
echo  "%name%.png"
echo  In : %sizein% Bytes

:: <script>
copy /b /y "%1" "%~dp0%name%-png8.png" 1>nul 2>nul
	if %Png8Dithering% equ 1 (
	imagequ /c%Png8NumberOfColors% /e%Png8ErrorP% /q%Png8Quantizer% /m%Png8MaxError% /d%Png8DitheringValue% %1 "%~dp0%name%-png8.png" >nul
	title %file%/%files% - 40 %% - %~z1 Bytes
	set sizecurrent=%~z1
	goto png8-compression
	)
imagequ /c%Png8NumberOfColors% /e%Png8ErrorP% /q%Png8Quantizer% /m%Png8MaxError% %1 "%~dp0%name%-png8.png" >nul
title %file%/%files% - 40 %% - %~z1 Bytes
set sizecurrent=%~z1
:png8-compression
	if %PngCompression7zip% equ 1 (
	truepng -quiet -o1 "%~dp0%name%-png8.png" >nul
	advdef -q -z -4 "%~dp0%name%-png8.png" >nul
	title %file%/%files% - 80 %% - %~z1 Bytes
	)
if %PngMaxCompression% equ 1 (
copy /b /y "%~dp0%name%-png8.png" "%~dp0%name%-t.png" 1>nul 2>nul
copy /b /y "%~dp0%name%-png8.png" "%~dp0%name%-w.png" 1>nul 2>nul
	for %%i in (1,2,4,8) do for %%j in (4,3,2,1,0) do pngout -q -c3 -d%%i -f0 -n%%j "%~dp0%name%-png8.png" >nul
	truepng -quiet -i0 -zc9 -zm3-9 -zs0-3 -fe -nc -na -force "%~dp0%name%-t.png" >nul
	title %file%/%files% - 85 %% - %sizecurrent% Bytes
pngwolf --zlib-level=9 --zlib-memlevel=3-9 --zlib-strategy=0-1 --7zip-mpass=2 --in="%~dp0%name%-w.png" --out="%~dp0%name%-w.png" >nul
title %file%/%files% - 88 %% - %sizecurrent% Bytes
	pngout -q -s0 -ks -k1 -f6 "%~dp0%name%-t.png"
	title %file%/%files% - 94 %% - %sizecurrent% Bytes
pngout -q -s0 -ks -k1 -f6 "%~dp0%name%-w.png"
title %file%/%files% - 98 %% - %sizecurrent% Bytes
call :compare "%~dp0%name%-png8.png" "%~dp0%name%-t.png"
call :compare "%~dp0%name%-png8.png" "%~dp0%name%-w.png"
truepng -quiet -nz -md remove all "%~dp0%name%-png8.png" >nul
)
1>nul 2>nul defluff < "%~dp0%name%-png8.png" > "%~dp0%name%-1.png"
call :compare "%~dp0%name%-png8.png" "%~dp0%name%-1.png"
deflopt -k "%~dp0%name%-png8.png" >nul
1>nul 2>nul defluff < "%~dp0%name%-png8.png" > "%~dp0%name%-1.png"
call :compare "%~dp0%name%-png8.png" "%~dp0%name%-1.png"
deflopt -k "%~dp0%name%-png8.png" >nul
call :check "%1" "%~dp0%name%-png8.png"
:: </script>

:: <calcul>
set /a total=(%sizein%-%sizeout%)
if %sizeout% geq %sizein% (
cecho {white} Out: {red}%sizeout%{white} Bytes
echo.
echo  %separe%
)
if %sizeout% lss %sizein% (
cecho {white} Out:{lime} %sizeout%{white} Bytes - %total% Bytes saved
echo.
echo  %separe%
set /a finaltotal+=%total%
)
:: </calcul>

:: <file>
shift
if .%1==. goto close
set /a pc=0
goto png8-run
:: </file>

::##########################################################################################
:: Comparator
::##########################################################################################

:compare
if %~z1 lss %~z2 (
del /f /q %2 1>nul 2>nul
) else (move /y %2 %1 1>nul 2>nul)
exit /b

:check
set /a sizeout=%~z2
exit /b

::##########################################################################################
:: Not Supported
::##########################################################################################

:formats
cls
title %soft% - Not Supported
echo.
echo.
echo  %soft% does NOT support this format
echo.
pause >nul
goto:eof

::##########################################################################################
:: Procedure End
::##########################################################################################

:close
set /a kilobytes=%finaltotal%/1024
title Finished - %soft% - %vers%
echo.
echo.
echo  Total: %kilobytes% KB [%finaltotal% Bytes] saved.
echo.
echo.
echo  Started at  : %start%
echo  Finished at : %time%
echo.
pause >nul
goto:eof

:end
endlocal
exit