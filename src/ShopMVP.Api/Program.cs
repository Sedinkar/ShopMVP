using System.Reflection;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

using ShopMVP.Api.Health;
using ShopMVP.Api.DatabaseOptions;

using ShopMVP.Infrastructure.DependencyInjection;




var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

//EF Core
/*
builder.Services.Configure<DatabaseOptions>(
    bind section "Databse"
    )
    */

builder.Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.Option))
    .ValidateDataAnnotations()
    .ValidateOnStart();
DatabaseOptions dbOptions = new DatabaseOptions();
builder.Configuration.GetSection(DatabaseOptions.Option).Bind(dbOptions);
DependencyInjection.AddPersistence(builder.Services, dbOptions.Default);

// MVC
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Problem Details
builder.Services.AddProblemDetails(options =>
 options.CustomizeProblemDetails = (context) =>
 {
     var traceId = context.HttpContext.TraceIdentifier;
     context.ProblemDetails.Extensions["traceId"] = traceId;
     if (string.IsNullOrEmpty(context.ProblemDetails.Instance))
     {
         context.ProblemDetails.Instance = context.HttpContext.Request.Path;
     }
     if (env.IsDevelopment() && context.Exception is not null)
     {
         context.ProblemDetails.Detail = context.Exception.Message;
     }
     if (context.ProblemDetails.Status is null)
     {
         context.ProblemDetails.Status = StatusCodes.Status500InternalServerError;
         if (context.HttpContext.Response.StatusCode == 200)
         {
             context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
         }
     }
 }
);
var hcBuilder = builder.Services.AddHealthChecks();
hcBuilder.AddCheck(name: "self", () => HealthCheckResult.Healthy(), tags: HealthTags.Live);
hcBuilder.AddCheck(name: "basic", () => HealthCheckResult.Healthy(), tags: HealthTags.Ready);
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ShopMVP API", Version = "v1", Description = "Public Web API for ShopMVP (MVP)" });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), includeControllerXmlComments: true);
});



var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseDeveloperExceptionPage();
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}
else if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/error");
}

app.Map("/error", async context =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    var path = context.Features.Get<IExceptionHandlerPathFeature>();
    var ex = feature?.Error;
    if (context.Response.StatusCode == 200)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    }
    context.Response.ContentType = "application/problem+json; charset=utf-8";
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";

    var problem = new ProblemDetails();

    problem.Type = "https://httpstatuses.com/500";
    problem.Title = "Internal Server Error";
    problem.Status = StatusCodes.Status500InternalServerError;
    problem.Instance = path?.Path ?? context.Request.Path;
    problem.Extensions["traceId"] = context.TraceIdentifier;

    ProblemDetailsContext pdc = new ProblemDetailsContext() { HttpContext = context, ProblemDetails = problem, Exception = ex };
    var pds = context.RequestServices.GetRequiredService<IProblemDetailsService>();
    await pds.WriteAsync(pdc);
});

//app.UseHttpsRedirection();
app.UseAuthorization();
static Task WriteResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";
    /* Just for practice, already disabled by default in AllowCachingResponses = false
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    */

    var options = new JsonWriterOptions { Indented = true };
    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("status");
        jsonWriter.WriteStringValue(healthReport.Status.ToString());
        jsonWriter.WritePropertyName("checks");
        jsonWriter.WriteStartArray();
        foreach (var healthReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("name");
            jsonWriter.WriteStringValue(healthReportEntry.Key);
            jsonWriter.WritePropertyName("status");
            jsonWriter.WriteStringValue(healthReportEntry.Value.Status.ToString());
            jsonWriter.WritePropertyName("duration");
            jsonWriter.WriteStringValue(healthReportEntry.Value.Duration.ToString("c"));
            jsonWriter.WriteEndObject();

        }
        jsonWriter.WriteEndArray();
        jsonWriter.WritePropertyName("totalDuration");
        jsonWriter.WriteStringValue(healthReport.TotalDuration.ToString("c"));
        jsonWriter.WriteEndObject();
        jsonWriter.Flush();
    }
    return context.Response.WriteAsync(Encoding.UTF8.GetString(memoryStream.ToArray()));
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    AllowCachingResponses = false, //default value
    Predicate = healthCheck => healthCheck.Tags.Contains("live"),
    ResponseWriter = WriteResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    AllowCachingResponses = false, //default value
    Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
    ResponseWriter = WriteResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.UseStatusCodePages(async context =>
{
    var code = context.HttpContext.Response.StatusCode;
    if (code != 404 && code != 405)
    { return; }
    context.HttpContext.Response.ContentType = "application/problem+json; charset=utf-8";
    context.HttpContext.Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0, must-revalidate";
    context.HttpContext.Response.Headers["Pragma"] = "no-cache";
    context.HttpContext.Response.Headers["Expires"] = "0";

    var path = context.HttpContext.Request.Path.Value;
    var problem = new ProblemDetails();

    problem.Type = $"https://httpstatuses.com/{code}";
    problem.Title = (code == StatusCodes.Status404NotFound) ? "Not Found" : "Method Not Allowed";
    problem.Status = code;
    problem.Instance = path;
    problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

    ProblemDetailsContext pdc = new ProblemDetailsContext() { HttpContext=context.HttpContext, ProblemDetails = problem, Exception = null };
    var pds = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
    await pds.WriteAsync(pdc);
});

app.MapControllers();

app.Run();