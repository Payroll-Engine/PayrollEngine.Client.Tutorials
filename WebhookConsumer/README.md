# Payroll Engine Client Tutorials - Webhook Consumer

## Usage
1. Start the Payroll Engine Backend
2. Setup Hangfire SQL Server database
	- new empty database `WebhookDemo`
3. Configure hangfire database connection
	- `appsettings.json` or better user `secrets.json`:
    ```
       "ConnectionStrings": {
         "HangfireConnection": "Server=localhost;Database=WebhookDemo;Integrated Security=SSPI;"
       }
    ```
4. Start project *WebhookConsumer*
	- the first run creates the hangfire SQL database schema
5. Execute backend `*.payrolls\WebhookPayroll\import.cmd*`
    - json output: `https://localhost:44396/webhooks`
    - hangfire jobs: `https://localhost:44396/hangfire`
