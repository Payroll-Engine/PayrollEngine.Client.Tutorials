@echo off
rem Build the extension folder with its manifest: bin\Release\extensions\PayrollConsoleExtension
rem Usage: Extension.cmd [PayrollConsole extensions folder]
setlocal
set name=PayrollConsoleExtension
set target=%~dp0bin\Release\extensions\%name%

dotnet build "%~dp0%name%.csproj" -c Release -o "%target%" --nologo
if errorlevel 1 goto end

PayrollConsole ExtensionManifest "%target%" Name:%name%
if errorlevel 1 goto end

if "%~1" == "" goto end
xcopy "%target%" "%~1\%name%\" /e /i /y
PayrollConsole Extensions

:end
endlocal
