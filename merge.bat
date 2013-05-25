@ECHO OFF
ECHO MERGE
PUSHD %~dp0
	REM set bb.build.msbuild.exe=for /D %%D in (%SYSTEMROOT%\Microsoft.NET\Framework\v4*) do set msbuild.exe=%%D\MSBuild.exe
	REM reg query HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0 /v MSBuildToolsPath
	
	REM echo %bb.build.msbuild.exe%
	REM EXIT /B
	
	SET CONFIG=Release
	C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe TalesOfVesperia.sln /p:Configuration=%CONFIG%
	SET BASE_FOLDER=%~dp0\Bin\%CONFIG%
	SET FILES=
	SET FILES=%FILES% "%BASE_FOLDER%\TalesOfVesperiaFrontendWPF.exe"
	SET FILES=%FILES% "%BASE_FOLDER%\TalesOfVesperiaTranslationEngine.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\TalesOfVesperiaUtils.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\Newtonsoft.Json.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\protobuf-net.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSharpUtils.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSharpUtils.Drawing.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSharpUtils.Ext.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSharpUtils.Vfs.dll"
	

	SET TARGET=/targetplatform:v4,"%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
	"%~dp0\Tools\ILMerge.exe" %TARGET% /out:TalesOfVesperiaFrontendWPF.exe %FILES%
POPD
