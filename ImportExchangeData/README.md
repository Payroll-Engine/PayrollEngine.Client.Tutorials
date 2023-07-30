# Payroll Engine Client Tutorials - Import exchange data

## Overview
Tutorial topic: Import employee wage changes from JSON file

## Prerequisites
- Payroll Engine Backend running with SimplePayroll
- Visual Studio with .NET 7
- Tutorial Create and modify objects

## Learnings
- Read exchange model
- Transform exchange object
- Using JSON files

## Notes
- Preparation
	- Setup SimplePayroll
- Welcome - tutorial slide
	- Manage user tasks using a CSV file
	- Prerequisites
	- Learnings
- Visual Studio
	- Tutorial project `ImportExchangeData.csproj`
	- Tutorial notes in project folder README.md
- Application use cases
	- Read case changes from JSON
	- Validate case values
		- check should be also in the case validation script
	- Create case change
- Exchange model
	- model overview image
- Program usage
	- program help
	- command line arguments
		- tenant: SimplePayroll
		- json: `Cases.json`
			- two month wage changes
			- one below min month wage
	- run program
		- created case values
		- min wage correction
- Program structure
	- Get tenant
	- Read exchange JSON from Cases.json
		- exchange reader class
		- read any exchange object
	- Setup case values
		- Case value
			- value as json
			- value type
		- Exchange visitor
			- handler for any exchange object
		- visit case value
			- check tenant
			- case value convert (json > decimal)
			- check and fix min wage (decimal > json)
			- store invalid request month wage in attribute
		- result is a list of tuple/pair
			- case change setup
			- case value setup
	- Import case changes
		- exchange import class
		- read exchange object
	- Display case changes
		- table layout
		- note with min. month wage fix
