using AMillo.ConfigurationSettings.Demo;
using AMillo.ConfigurationSettings.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddConfigurationSettings(); //Add all configuration classes marked with [ConfigurationSettings] attribute

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/sampleconfiguration", (IOptions<SampleConfiguration> options) =>
{
    return JsonSerializer.Serialize(options.Value);
})
.WithName("GetSampleConfiguration")
.WithOpenApi();

app.MapGet("/sampleconfiguration/monitor", (IOptionsMonitor<SampleConfiguration> options) =>
{
    return JsonSerializer.Serialize(options.CurrentValue);
})
.WithName("GetSampleConfigurationMonitor")
.WithOpenApi();

app.Run();