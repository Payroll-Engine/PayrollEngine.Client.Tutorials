# Payroll Engine Client Tutorials - Extended object model

## Overview
Tutorial topic: How the extend or map  the payroll model

## Prerequisites
- Payroll Engine Backend running with SimplePayroll
- Visual Studio with .NET 7
- Tutorial Create and modify objects

## Learnings
- Custom object attribute
- Map payroll object
- Query custom attribute

## Notes
- Preparation
	- Setup SimplePayroll
- Welcome - tutorial slide
	- How the extend or map the payroll model
	- Prerequisites
	- Learnings
- Visual Studio
	- Tutorial project `ExtendedObjectModel.csproj`
	- Tutorial notes in project folder README.md
- Application use cases
	- Extend Employee object with Erp Id (Guid)
	- Map payroll task to custom activity object
	- Query employee by Erp Id
- Program usage
	- program help
	- command line arguments
		- tenant: SimplePayroll
		- employee Erp id
	- run program
- Program structure
	- Get tenant
	- MyEmployee
		- property ErpId
			- get or set attribute value (Guid)
		- query employee service with MyEmployee
	- Activity
		- Custom type
			state code
			- id (Guid)
			- name
		- AutoMapper
			- object to objects mapper by field names
			- custom field mapping
			- NuGet reference
			- open source: https://automapper.org/
		- query tasks and map to activity: MapTaskToActivity
			- map Erp id (Guid) from task attribute
			- map state code (ActivityStateCode) from task schedule date presence
	- Employee query
		- employee querey by Erp Id
		- display employee details including the Erp Id
