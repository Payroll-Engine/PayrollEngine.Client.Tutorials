# Payroll Engine Client Tutorials - Client objects and services

## Overview

Tutorial topic: Display tenant employees with Payroll Engine client objects and services

## Prerequisites

- Payroll Engine Backend running with known tenant
- Visual Studio with .NET 7
- Client services tutorial: Console Application

## Learnings

- Client object model
- Client services
- Retrieve single object
- Query multiple objects

## Notes

- Welcome - tutorial slide
	- Display tenant employees with Payroll Engine client objects and services
	- Prerequisites
	- Learnings
- API object model - Swagger UI
	- API object reference: Swagger Schema
	- API endpoints reference: Swagger Operations
- Client object model - HTML Help
	- PayrollEngine.Client.Model namespace
	- Basic object attributes
		- Unique id
		- Active/Inactive
		- Created/Updated
			- All DateTime values in UTC
	- Model tree
		- tenant model
		- regulation model
	- API model vs client model
		- exchange attributes
- Client services - HTML Help
	- PayrollEngine.Client.Service namespace
	- Client service context
		- Client object model hierarchy
		- Path components of endpoint urls
- Visual Studio
	- Tutorial project ClientObjectsAndServices.csproj
	- Tutorial notes in project folder README.md
	- Runtime context PayrollHttpClient
		- provided by ConsoleProgram
		- CRUD using the Http client GET/POST/PUT/DELETE
		- contains API controller endpoints
	- Start tutorial exe
- Retrieve single object
	- Service context
	- Get object by id
		- cross-tenant access
	- Get objects bi identifier/name
		- Get tenant by identifier
	- Xxx vs XxxSet objects
		- Combine multiple requests
		- Case vs. CaseSet, including Case Fields and Case Relations
- Query multiple objects
	- Enhance query performance
	- Reduce trafic volume with less data
	- Query with OData
		- ODataQuery.txt
		- sort order
		- pagging filters
		- filtering (advanced topic)
		- field select (advanced topic)
		- count mode (advanced topic)
	- Query employees of tenant
		- Service context
	- Query top filter by command line argument
