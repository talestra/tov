@ECHO OFF
ECHO MERGE
PUSHD %~dp0
	SET BASE_FOLDER=%~dp0\Bin\Release
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
