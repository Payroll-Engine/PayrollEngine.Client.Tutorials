// switch between direct and delayed message 
#define ENQUEUE_MESSAGE

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
// ReSharper disable All

namespace PayrollEngine.Client.Tutorial.WebhookConsumer.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class WebhooksController(IBackgroundJobClient backgroundJobs) : ControllerBase
{
    // no concurrency support: demo only!
    private static readonly List<Tuple<DateTime, object>> Messages = [];

    // definitions
    private const decimal MinWage = 2000M;

    public IBackgroundJobClient BackgroundJobs { get; } = backgroundJobs;

    /// <summary>Get all received webhook messages</summary>
    /// <returns>The webhook messages</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Tuple<DateTime, object>[] GetWebhookMessages()
    {
        return Messages.ToArray();
    }

    /// <summary>Webhook case function request</summary>
    /// <remarks>Request body contains the webhook message</remarks>
    /// <param name="message">The webhook message</param>
    /// <returns>The webhook function result</returns>
    [HttpPost("casefunction")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public ActionResult<object> CaseFunctionRequestWebhook([FromBody][Required] WebhookRuntimeMessage message)
    {
        if (message == null)
        {
            return BadRequest("missing webhook message");
        }

        object functionResult = message.RequestOperation switch
        {
            "MinWage" => MinWage,
            _ => null
        };

        AddMessage(message);
        return Created(string.Empty, functionResult);
    }

    /// <summary>
    /// Webhook case added
    /// </summary>
    /// <remarks>
    /// Request body contains the webhook message
    /// </remarks>
    /// <param name="message">The webhook message</param>
    /// <returns>Created</returns>
    [HttpPost("caseadded")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public ActionResult CaseAddedWebhook([FromBody][Required] WebhookRuntimeMessage message)
    {
        if (message == null)
        {
            return BadRequest("missing webhook message");
        }

        if (Enum.TryParse<WebhookAction>(message.ActionName, out var action) && action == WebhookAction.CaseChangeAdded)
        {
            var caseChangeAdded = JsonSerializer.Deserialize<CaseChange>(message.RequestMessage,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            if (caseChangeAdded != null)
            {
                switch (caseChangeAdded.ValidationCaseName)
                {
                    case "Monatslohn":
                        // custom action
                        break;
                    case "Monatsfaktor":
                        // custom action
                        break;
                    case "AHV":
                        // custom action
                        break;
                }
            }
        }

        AddMessage(message);
        return Ok();
    }

    /// <summary>Webhook payrun function request</summary>
    /// <remarks>Request body contains the webhook message</remarks>
    /// <param name="message">The webhook message</param>
    /// <returns>The webhook function result</returns>
    [HttpPost("payrunfunction")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public ActionResult<object> PayrunFunctionRequestWebhook([FromBody][Required] WebhookRuntimeMessage message)
    {
        if (message == null)
        {
            return BadRequest("missing webhook message");
        }

        // test delay
        //System.Threading.Tasks.Task.Delay(5000).Wait();

        object functionResult = message.RequestOperation switch
        {
            "MinWage" => MinWage,
            _ => null
        };

        AddMessage(message);
        return Created(string.Empty, functionResult);
    }

    /// <summary>Webhook report function request</summary>
    /// <remarks>Request body contains the webhook message</remarks>
    /// <param name="message">The webhook message</param>
    /// <returns>The webhook function result</returns>
    [HttpPost("reportfunction")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public ActionResult<object> ReportFunctionRequestWebhook([FromBody][Required] WebhookRuntimeMessage message)
    {
        if (message == null)
        {
            return BadRequest("missing webhook message");
        }

        object functionResult = message.RequestOperation switch
        {
            "MinWage" => MinWage,
            _ => null
        };

        AddMessage(message);
        return Created(string.Empty, functionResult);
    }

    /// <summary>Webhook payroll task change</summary>
    /// <remarks>Request body contains the webhook message</remarks>
    /// <param name="message">The webhook message</param>
    /// <returns>The webhook function result</returns>
    [HttpPost("taskchange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public ActionResult TaskChangeWebhook([FromBody][Required] WebhookRuntimeMessage message)
    {
        if (message == null)
        {
            return BadRequest("missing webhook message");
        }

        var task = JsonSerializer.Deserialize<Task>(message.RequestMessage,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (task == null)
        {
            return NotFound();
        }

        AddMessage(message);
        return Ok();
    }

    private void AddMessage(WebhookRuntimeMessage message)
    {

#if ENQUEUE_MESSAGE
        // immediate job
        BackgroundJobs.Enqueue(() => Console.WriteLine($"Webhook: {message.ActionName}"));
#else
        // scheduled job
        BackgroundJobs.Schedule(() => Console.WriteLine($"Webhook: {message.ActionName}"),
       TimeSpan.FromSeconds(10));
#endif

        // logging
        Log.Information(message.ToString());

        // user notification
        Messages.Insert(0, new(DateTime.UtcNow, message));
    }

    /// <summary>
    /// immediate job
    /// </summary>
#pragma warning disable IDE0051
    private void AddRecurringMessage() =>
#pragma warning restore IDE0051
        RecurringJob.AddOrUpdate(
            "MyRecurringJob",
            () => Console.WriteLine("Recurring!"),
            Cron.Minutely);
}