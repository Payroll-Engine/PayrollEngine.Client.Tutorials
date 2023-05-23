# Payroll Engine Client Tutorials - Created and modify objects

## Overview

Tutorial topic: Manage user tasks using a CSV file

## Prerequisites

- Payroll Engine Backend running with SimplePayroll
- Visual Studio with .NET 7
- Client services tutorial: Client objects and services

## Learnings

- Create objects
- Update objects
- Using CSV files

## Notes
- Preparation
	- Setup SimplePayroll
- Welcome - tutorial slide
	- Manage user tasks using a CSV file
	- Prerequisites
	- Learnings
- Visual Studio
	- Tutorial project CreateAndModifyObjects.csproj
	- Tutorial notes in project folder README.md
- Application use cases
	- Create new tasks
	- Reschedule task
	- Complete task
- Program usage
	- command line argument help /h
		- HelpAsync
		- MandatoryArgumentCount
		- run program
	- command line argument tenant: SimplePayroll
		- run program
	- command line arguments CSV file: Tasks.csv
		- CSV rows
			- complete existing task AhvTask
			- add new tasks for 3 other users
		- run program
- Program structure
	- Get tenant
	- Get users
		- active users
	- Get open tasks
		- active tasks
		- not completed (OData filter)
	- Get updated tasks from CSV
		- CsvSerializer
		- setup updated tasks
		- support for completed tasks: date < now
		- 
	- Update tasks
		- Create task
		- Updated task
			- only on changes
		- return merged (open/updated) tasks
	- Display tasks
		- table layout
		- red: delayed task
		- green: ongoing or completed task
