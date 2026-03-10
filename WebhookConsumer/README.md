# Payroll Engine Client Tutorials - Webhook Consumer

## Overview
Tutorial topic: Consume and delegate webhooks with a custom web application

## Prerequisites
- Payroll Engine Backend running
- SQL Server with Hangfire database
- Visual Studio with .NET 8
- Tutorial: Console Application

## Learnings
- Webhook receiver endpoint
- Webhook message processing
- Background job processing with Hangfire

## Usage
1. Start the Payroll Engine Backend
2. Setup Hangfire SQL Server database
	- new empty database `WebhookDemo`
3. Configure hangfire database connection
	- `appsettings.json` or better user `secrets.json`:
    ```json
    "ConnectionStrings": {
      "HangfireConnection": "Server=localhost;Database=WebhookDemo;Integrated Security=SSPI;"
    }
    ```
4. Start project *WebhookConsumer*
	- the first run creates the hangfire SQL database schema
5. Import the webhook payroll using `Payroll\Import.pecmd`
    - json output: `https://localhost:44396/webhooks`
    - hangfire jobs: `https://localhost:44396/hangfire`
