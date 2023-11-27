# Payroll Engine Client Tutorials - Scripting Development

## Overview
Tutorial topic: Develop payroll reports with Visual Studio

## Prerequisites
- Payroll Engine Backend running with SimplePayroll
- Visual Studio with .NET 8
- Tutorial Console Application

## Learnings
- Scripts in Visual Studio
- Invoke and debug scripts
- Invocation input and output data

## Notes
- Preparation
	- Setup SimplePayroll
- Welcome - tutorial slide
	- Develop payroll reports with Visual Studio
	- Prerequisites
	- Learnings
- Client Services
	- Scripting Development
- Functions
	- Tutorial: report function
- Reporting
	- Build: setup report parameters
	- Start: setup report queries
	- End: build result data set
- Application use cases
	- Code a report in Visual Studio
	- Debug a report in Visual Studio
- Visual Studio
	- Tutorial project `ScriptingDevelopment.csproj`
	- Tutorial notes in project folder README.md
- SimplePayroll WageTypesReport
	- json example
	- Query
		- Regulations
		- Query name: table name in dataset
		- Query value: Rest API web method name
	- Parameters
		- TenantId
		- RegulationId
		- Regulations.Filter
- Program usage
	- program help
	- command line arguments
- Program
	- help: command line arguments
		- ScriptMode
		- ReportName
		- QueryFileName
		- ResultFileName
	- debug program
		- Invoke report start function
		    - ReportStartFunctionInvoker
			- debug breakpoint
				- QuickWatch this.Runtime
			- view output file with query results
		- Invoke report end function
		    - ReportEndFunctionInvoker
			- input file with query results
			- debug breakpoint
				- QuickWatch regulations collection
			- view output file with report results
		- Invoke report start/end function
		    - ReportStartFunctionInvoker
		    - ReportEndFunctionInvoker
			- debug breakpoint start
			- debug breakpoint end
