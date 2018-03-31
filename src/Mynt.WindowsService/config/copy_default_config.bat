@REM @echo off
@REM Makes copies of all .default files without the .default extension, only if it doesn't already exist. Does the same recursively through all child folders.
@REM Borrowed from: https://stackoverflow.com/a/3920462
cd ..\..\config
for /r %%f in (*.default) do (
    if not exist "%%~pnf" (echo Copying %%~pnf.default to %%~pnf & copy "%%f" "%%~pnf" /y)
)
echo Verified config files.