using System.Reflection;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

using ShopMVP.Api.Health;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
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
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
static Task WriteResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";
    /* Just for practice, already disabled by default AllowCachingResponses = false
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

app.MapControllers();

app.Run();