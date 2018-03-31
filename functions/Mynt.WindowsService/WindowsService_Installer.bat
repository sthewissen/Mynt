@echo off
rem Thanks to https://stackoverflow.com/a/26676983
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"

if '%errorlevel%' NEQ '0' (
    echo This bat file must be ran as an Adminstrator
) else (
	SET INSTALLUTILDIR=
	@for /F "tokens=1,2*" %%i in ('reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v "InstallPath"') DO (
		if "%%i"=="InstallPath" (
			SET "INSTALLUTILDIR=%%k"
		)
	)

	if exist Mynt.WindowsService.exe (	
		%INSTALLUTILDIR%\installutil Mynt.WindowsService.exe
	) else (
		echo You have ran this file in a location where 'Mynt.WindowsService.exe' does not exist.
		echo When in development you should run this from the build output folder, eg. bin/debug/
	)
)

echo on