# Payroll Engine Client Tutorials - Payroll Console Extension

## Overview
Tutorial topic: Build a custom command for the Payroll Console

## Prerequisites
- Payroll Engine Backend running with known tenant
- Visual Studio with .NET 8
- Payroll Console installed

## Learnings
- Create a custom console command
- Command parameters and toggles
- Query and display payroll data

## Usage
1. Build the project
2. Copy the output file `PayrollConsoleExtension.dll` to the Payroll Console `extensions` subfolder
3. Show the custom command help: `PayrollConsole Help ListTenants`
4. Execute the custom command: `PayrollConsole ListTenants`
5. Execute with order toggle: `PayrollConsole ListTenants /createdDescending`
