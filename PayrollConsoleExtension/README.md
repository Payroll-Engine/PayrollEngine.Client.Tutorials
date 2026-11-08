# Payroll Engine Client Tutorials - Payroll Console Extension

## Overview
Tutorial topic: Build a custom command for the Payroll Console

## Prerequisites
- Payroll Engine Backend running with known tenant
- Visual Studio with .NET 10
- Payroll Console 1.2 installed

## Learnings
- Create a custom console command
- Command parameters and toggles
- Query and display payroll data
- Extension folder with `extension.json` manifest
- Shared contract assemblies and own dependencies

## Extension Layout
Since Payroll Console 1.2 each extension lives in its own folder below the console `extensions` folder, with an
`extension.json` manifest listing the command assemblies and the SHA-256 hash of every file:
```
extensions/
  PayrollConsoleExtension/
    extension.json
    PayrollConsoleExtension.dll
    PayrollConsoleExtension.deps.json
    PayrollConsoleExtension.pdb
```

The project references `PayrollEngine.Client.Core` only. It is a contract assembly shared with the console
(like `PayrollEngine.Core` and `PayrollEngine.Lex`) and is not copied into the extension folder. An extension with
its own NuGet dependencies sets `CopyLocalLockFileAssemblies` to `true`: the dependencies are copied into the
extension folder and loaded in the isolated load context of the extension.

## Usage
1. Build the extension folder with its manifest
    ```cmd
    Extension.cmd
    ```
    ```bash
    ./Extension.sh
    ```
	- builds the project into `bin/Release/extensions/PayrollConsoleExtension`
	- creates the manifest with `PayrollConsole ExtensionManifest`
	- Linux/macOS: the console command is `PAYROLL_CONSOLE`, otherwise `pecmd` (see `register-pecmd.sh`) or `PayrollEngine.PayrollConsole`
2. Install the extension: pass the console `extensions` folder
    ```cmd
    Extension.cmd C:\PayrollEngine\PayrollConsole\extensions
    ```
    ```bash
    ./Extension.sh /opt/payrollengine/extensions
    ```
	- copies the extension folder and lists the extensions with `PayrollConsole Extensions`
3. Show the custom command help: `PayrollConsole Help ListTenants`
	- the help shows the source of the command (extension name and version)
4. Execute the custom command: `PayrollConsole ListTenants`
5. Execute with order toggle: `PayrollConsole ListTenants /createdDescending`

## Notes
- Rebuild the manifest after each build: a changed file is rejected with `Hash mismatch of file ...`
- Files in the extension folder that are missing in the manifest are rejected with `Undeclared file(s) ...`
- A command name already used by the console or another extension rejects the whole extension
- Assemblies placed directly into the `extensions` folder (layout before 1.2) are no longer loaded
