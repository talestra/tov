::##########################################################################################
:: Script Parameters
::##########################################################################################

:verbose
@echo off

:script
setlocal enabledelayedexpansion
set pngdir="%~dp0lib\"
path %pngdir%;%path% >nul

:name
set soft=ScriptJPG
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
echo            ---------------------------------------
echo  Formats : BMP,GIF,JPG,PCX,PBM,PGM,PNG,PNM,TIF,TGA
echo            ---------------------------------------
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
if /i %~x1 neq .jpg goto formats

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
echo  [1] Optimize JPG          [2] JPG Quality 90%%
echo.
echo.
echo  [3] JPG Quality 80%%       [4] JPG Quality 70%%
echo.
echo.
echo  [5] JPG Quality 60%%       [6] JPG Quality 50%%
echo.
echo.
echo.

:choice
set /p number=# Enter an option [1-6]:  
if %number%==1 goto optimize
if %number%==2 set qua=90 & goto jpgconv
if %number%==3 set qua=80 & goto jpgconv
if %number%==4 set qua=70 & goto jpgconv
if %number%==5 set qua=60 & goto jpgconv
if %number%==6 set qua=50 & goto jpgconv
cls
goto iuser

::##########################################################################################
:: Optimize JPG
::##########################################################################################
:optimize
cls
echo.
echo  %soft% - %vers% - Optimize JPG
echo.
set start=%time%

:optimize-run
if /i %~x1 neq .jpg goto formats
set /a file+=1
set /a sizein=%~z1
set name=%~n1
copy /b /y %1 %1.bak 1>nul 2>nul
title %file%/%files% - 0 %%
echo.
echo  "%name%.jpg"
echo  In : %sizein% Bytes

:: <script>
copy /b /y "%1" "%~dp0%name%-1.jpg" 1>nul 2>nul
pngout -q -s4 -c4 "%~dp0%name%-1.jpg" >nul
if errorlevel 3 goto optimize-nogray
del /a /f "%~dp0%name%-1.jpg" 1>nul 2>nul
del /a /f "%~dp0%name%-1.png" 1>nul 2>nul
jpegtran -optimize -grayscale "%1" "%~dp0%name%-opt.jpg" >nul
title %file%/%files% - 30 %%
call :compare "%1" "%~dp0%name%-opt.jpg"
jpegtran -optimize -grayscale -progressive "%1" "%~dp0%name%-pro.jpg" >nul
title %file%/%files% - 60 %%
call :compare "%1" "%~dp0%name%-pro.jpg"
goto optimize-end

:optimize-nogray
del /a /f "%~dp0%name%-1.jpg" 1>nul 2>nul
del /a /f "%~dp0%name%-1.png" 1>nul 2>nul
jpegtran -optimize "%1" "%~dp0%name%-opt.jpg" >nul
title %file%/%files% - 30 %%
call :compare "%1" "%~dp0%name%-opt.jpg"
jpegtran -optimize -progressive "%1" "%~dp0%name%-pro.jpg" >nul
title %file%/%files% - 60 %%
call :compare "%1" "%~dp0%name%-pro.jpg"

:optimize-end
title %file%/%files% - 90 %%
jscl -r -j -cp "%1" >nul
set /a sizeout=%~z1
:: </script>

:: <calcul>
set /a total=(%sizein%-%sizeout%)
if %sizeout% geq %sizein% (
del /f %1 1>nul 2>nul
rename %1.bak "%~nx1" 1>nul 2>nul
cecho {white} Out: %sizeout% Bytes - {red}Skipped{white}
echo.
echo  %separe%
)
if %sizeout% lss %sizein% (
del /f %1.bak 1>nul 2>nul
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
goto optimize-run
:: </file>

::##########################################################################################
:: Convert JPG
::##########################################################################################
:jpgconv
cls
echo.
echo  %soft% - %vers% - JPG Converter
echo.
set start=%time%

:jpgconv-run
if /i %~x1 neq .jpg goto formats
set /a file+=1
set /a sizein=%~z1
set name=%~n1
copy /b /y %1 %1.bak 1>nul 2>nul
title %file%/%files% - 0 %%
echo.
echo  "%name%.jpg"
echo  In : %sizein% Bytes

:: <script>
copy /b /y %1 "%~dp0%name%-1.jpg" 1>nul 2>nul
imagew -quiet -jpegquality %qua% %1 %1 >nul
title %file%/%files% - 60 %%
pngout -q -s4 -c4 "%~dp0%name%-1.jpg" >nul
if errorlevel 3 goto convert-nogray
del /a /f "%~dp0%name%-1.jpg" 1>nul 2>nul
del /a /f "%~dp0%name%-1.png" 1>nul 2>nul
jpegtran -optimize -grayscale "%1" "%~dp0%name%-opt.jpg" >nul
title %file%/%files% - 80 %%
call :compare "%1" "%~dp0%name%-opt.jpg"
jpegtran -optimize -grayscale -progressive "%1" "%~dp0%name%-pro.jpg" >nul
title %file%/%files% - 90 %%
call :compare "%1" "%~dp0%name%-pro.jpg"
goto convert-end

:convert-nogray
del /a /f "%~dp0%name%-1.jpg" 1>nul 2>nul
del /a /f "%~dp0%name%-1.png" 1>nul 2>nul
jpegtran -optimize "%1" "%~dp0%name%-opt.jpg" >nul
title %file%/%files% - 80 %%
call :compare "%1" "%~dp0%name%-opt.jpg"
jpegtran -optimize -progressive "%1" "%~dp0%name%-pro.jpg" >nul
title %file%/%files% - 90 %%
call :compare "%1" "%~dp0%name%-pro.jpg"

:convert-end
title %file%/%files% - 100 %%
jscl -r -j -cp "%1" >nul
set /a sizeout=%~z1
:: </script>

:: <calcul>
set /a total=(%sizein%-%sizeout%)
if %sizeout% geq %sizein% (
del /f %1 1>nul 2>nul
rename %1.bak "%~nx1" 1>nul 2>nul
cecho {white} Out: %sizeout% Bytes - {red}Skipped{white}
echo.
echo  %separe%
)
if %sizeout% lss %sizein% (
del /f %1.bak 1>nul 2>nul
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
goto jpgconv-run
:: </file>

::##########################################################################################
:: Format Converter
::##########################################################################################
:formats
cls
echo.
echo  %soft% - %vers% - Format Converter
echo.
set start=%time%

:formats-run
set /a file+=1
set /a sizein=%~z1
set name=%~n1
set ext=%~x1
title %file%/%files% - 0 %%
echo.
echo  "%name%%ext%"
echo  In : %sizein% Bytes

:: <script>
if %ext% equ .bmp (
cjpeg -quality 95 "%1" "%~dp0%name%.jpg" >nul
title %file%/%files% - 40 %%	
goto formats-optimize
)
if %ext% equ .png (
	copy /b /y %1 "%~dp0%name%-1.png" 1>nul 2>nul
	imagew -quiet -bkgd fff "%~dp0%name%-1.png" "%~dp0%name%-1.png" >nul
	title %file%/%files% - 20 %%
pngout -q -c2 -s4 -force -y "%~dp0%name%-1.png" >nul
png2bmp -q "%~dp0%name%-1.png" -o "%~dp0%name%-1.bmp"
del /f "%~dp0%name%-1.png" 1>nul 2>nul
title %file%/%files% - 30 %%
	cjpeg -quality 95 "%~dp0%name%-1.bmp" "%~dp0%name%.jpg" >nul
	del /f "%~dp0%name%-1.bmp" 1>nul 2>nul
	title %file%/%files% - 40 %%
goto formats-optimize
)
if %ext% equ .gif (
copy /b /y "%1" "%~dp0%name%-1.gif" 1>nul 2>nul
pngout -q -s4 -force -y "%~dp0%name%-1.gif" >nul
del /f "%~dp0%name%-1.gif" 1>nul 2>nul
title %file%/%files% - 20 %%
	png2bmp -q "%~dp0%name%-1.png" -o "%~dp0%name%.bmp"
	del /f "%~dp0%name%-1.png" 1>nul 2>nul
	title %file%/%files% - 30 %%
cjpeg -quality 95 "%~dp0%name%.bmp" "%~dp0%name%.jpg" >nul
del /f "%~dp0%name%.bmp" 1>nul 2>nul
title %file%/%files% - 40 %%
goto formats-optimize
)
if %ext% equ .pcx (
copy /b /y "%1" "%~dp0%name%-1.pcx" 1>nul 2>nul
pngout -q -s4 -force -y "%~dp0%name%-1.pcx" >nul
del /f "%~dp0%name%-1.pcx" 1>nul 2>nul
title %file%/%files% - 20 %%
	png2bmp -q "%~dp0%name%-1.png" -o "%~dp0%name%.bmp"
	del /f "%~dp0%name%-1.png" 1>nul 2>nul
	title %file%/%files% - 30 %%
cjpeg -quality 95 "%~dp0%name%.bmp" "%~dp0%name%.jpg" >nul
del /f "%~dp0%name%.bmp" 1>nul 2>nul
title %file%/%files% - 40 %%
goto formats-optimize
)
if %ext% equ .tga (
copy /b /y "%1" "%~dp0%name%-1.tga" 1>nul 2>nul
pngout -q -s4 -force -y "%~dp0%name%-1.tga" >nul
del /f "%~dp0%name%-1.tga" 1>nul 2>nul
title %file%/%files% - 20 %%
	png2bmp -q "%~dp0%name%-1.png" -o "%~dp0%name%.bmp"
	del /f "%~dp0%name%-1.png" 1>nul 2>nul
	title %file%/%files% - 30 %%
cjpeg -quality 95 "%~dp0%name%.bmp" "%~dp0%name%.jpg" >nul
del /f "%~dp0%name%.bmp" 1>nul 2>nul
title %file%/%files% - 40 %%
goto formats-optimize
)
if %ext% equ .tif (
copy /b /y "%1" "%~dp0%name%-1.tif" 1>nul 2>nul
optipng -quiet -o1 "%~dp0%name%-1.tif" >nul
del /f "%~dp0%name%-1.tif" 1>nul 2>nul
title %file%/%files% - 20 %%
	png2bmp -q "%~dp0%name%-1.png" -o "%~dp0%name%.bmp"
	del /f "%~dp0%name%-1.png" 1>nul 2>nul
	title %file%/%files% - 30 %%
cjpeg -quality 95 "%~dp0%name%.bmp" "%~dp0%name%.jpg" >nul
del /f "%~dp0%name%.bmp" 1>nul 2>nul
title %file%/%files% - 40 %%
goto formats-optimize
)
if %ext% equ .tiff (
copy /b /y "%1" "%~dp0%name%-1.tiff" 1>nul 2>nul
optipng -quiet -o1 "%~dp0%name%-1.tiff" >nul
del /f "%~dp0%name%-1.tiff" 1>nul 2>nul
title %file%/%files% - 20 %%
	png2bmp -q "%~dp0%name%-1.png" -o "%~dp0%name%.bmp"
	del /f "%~dp0%name%-1.png" 1>nul 2>nul
	title %file%/%files% - 30 %%
cjpeg -quality 95 "%~dp0%name%.bmp" "%~dp0%name%.jpg" >nul
del /f "%~dp0%name%.bmp" 1>nul 2>nul
title %file%/%files% - 40 %%
goto formats-optimize
)
if %ext% equ .pbm (
copy /b /y "%1" "%~dp0%name%-1.pbm" 1>nul 2>nul
optipng -quiet -o1 "%~dp0%name%-1.pbm" >nul
del /f "%~dp0%name%-1.pbm" 1>nul 2>nul
title %file%/%files% - 20 %%
	png2bmp -q "%~dp0%name%-1.png" -o "%~dp0%name%.bmp"
	del /f "%~dp0%name%-1.png" 1>nul 2>nul
	title %file%/%files% - 30 %%
cjpeg -quality 95 "%~dp0%name%.bmp" "%~dp0%name%.jpg" >nul
del /f "%~dp0%name%.bmp" 1>nul 2>nul
title %file%/%files% - 40 %%
goto formats-optimize
)
if %ext% equ .pgm (
copy /b /y "%1" "%~dp0%name%-1.pgm" 1>nul 2>nul
optipng -quiet -o1 "%~dp0%name%-1.pgm" >nul
del /f "%~dp0%name%-1.pgm" 1>nul 2>nul
title %file%/%files% - 20 %%
	png2bmp -q "%~dp0%name%-1.png" -o "%~dp0%name%.bmp"
	del /f "%~dp0%name%-1.png" 1>nul 2>nul
	title %file%/%files% - 30 %%
cjpeg -quality 95 "%~dp0%name%.bmp" "%~dp0%name%.jpg" >nul
del /f "%~dp0%name%.bmp" 1>nul 2>nul
title %file%/%files% - 40 %%
goto formats-optimize
)
if %ext% equ .pnm (
copy /b /y "%1" "%~dp0%name%-1.pnm" 1>nul 2>nul
optipng -quiet -o1 "%~dp0%name%-1.pnm" >nul
del /f "%~dp0%name%-1.pnm" 1>nul 2>nul
title %file%/%files% - 20 %%
	png2bmp -q "%~dp0%name%-1.png" -o "%~dp0%name%.bmp"
	del /f "%~dp0%name%-1.png" 1>nul 2>nul
	title %file%/%files% - 30 %%
cjpeg -quality 95 "%~dp0%name%.bmp" "%~dp0%name%.jpg" >nul
del /f "%~dp0%name%.bmp" 1>nul 2>nul
title %file%/%files% - 40 %%
goto formats-optimize
)
goto not-supported

:formats-optimize
copy /b /y "%~dp0%name%.jpg" "%~dp0%name%-1.jpg" 1>nul 2>nul
pngout -q -s4 -c4 "%~dp0%name%-1.jpg" >nul
if errorlevel 3 goto formats-nogray
del /a /f "%~dp0%name%-1.jpg" 1>nul 2>nul
del /a /f "%~dp0%name%-1.png" 1>nul 2>nul
jpegtran -optimize -grayscale "%~dp0%name%.jpg" "%~dp0%name%-opt.jpg" >nul
title %file%/%files% - 60 %%
call :compare "%~dp0%name%.jpg" "%~dp0%name%-opt.jpg"
jpegtran -optimize -grayscale -progressive "%~dp0%name%.jpg" "%~dp0%name%-pro.jpg" >nul
title %file%/%files% - 80 %%
call :compare "%~dp0%name%.jpg" "%~dp0%name%-pro.jpg"
goto formats-clean

:formats-nogray
del /a /f "%~dp0%name%-1.jpg" 1>nul 2>nul
del /a /f "%~dp0%name%-1.png" 1>nul 2>nul
jpegtran -optimize "%~dp0%name%.jpg" "%~dp0%name%-opt.jpg" >nul
title %file%/%files% - 60 %%
call :compare "%~dp0%name%.jpg" "%~dp0%name%-opt.jpg"
jpegtran -optimize -progressive "%~dp0%name%.jpg" "%~dp0%name%-pro.jpg" >nul
title %file%/%files% - 80 %%
call :compare "%~dp0%name%.jpg" "%~dp0%name%-pro.jpg"

:formats-clean
title %file%/%files% - 90 %%
jscl -r -j -cp "%~dp0%name%.jpg" >nul
set /a sizeout=%~z1
:: </script>

:: <calcul>
:formats-end
call :check "%1" "%~dp0%name%.jpg"
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
goto formats-run
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

:not-supported
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