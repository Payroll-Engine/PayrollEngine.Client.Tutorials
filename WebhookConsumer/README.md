# Payroll Engine Client Tutorials - Webhook Consumer

## Overview
Tutorial topic: Consume and delegate webhooks with a custom web application

## Prerequisites
- Payroll Engine Backend running
- SQL Server with Hangfire database
- Visual Studio with .NET 10
- Tutorial: Console Application

## Learnings
- Webhook receiver endpoint
- Webhook message processing
- Verify signed webhook requests
- Sign the response of a function request
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

## Signed Webhooks
A webhook with a secret signs its requests with HMAC-SHA256:

| Header | Content |
|:--|:--|
| `X-PE-Timestamp` | Unix timestamp in seconds |
| `X-PE-Signature` | `v1=` + HMAC-SHA256 hex over `{timestamp}.{body}` |
| `X-PE-Response-Signature` | Response of a function request: `v1=` + HMAC-SHA256 hex over `{timestamp}.{requestSignature}.{responseBody}` |

The backend aborts a function request (`InvokeWebhook`) of a signed webhook without a valid response signature.

Setup:
1. Set the secret of the webhooks in the WebApp (webhook editor, field *Secret*)
	- the secret is write-only, the column *Signed* shows the signed webhooks
	- an exchange import does not contain the secret: set it again after each import
2. Configure the same secret in the user `secrets.json` of the *WebhookConsumer*:
    ```json
    "Webhook": {
      "Secret": "<your webhook secret>"
    }
    ```
3. Restart the *WebhookConsumer*

Receiver:
- `WebhookSignatureFilter` reads the raw request body before the model binding and verifies the signature and the age of the request (max. 5 minutes); an invalid request is rejected with `401 Unauthorized`
- `WebhooksController.CaseFunctionRequestWebhook` serializes the function result itself and signs exactly these bytes
- without configured secret the receiver accepts unsigned requests and sends unsigned responses
