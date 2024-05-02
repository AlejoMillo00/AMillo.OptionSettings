using AMillo.ConfigurationSettings.Demo;
using AMillo.ConfigurationSettings.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Add all configuration classes marked with [ConfigurationSettings] attribute from specified assemblies
//Uses builder.Configuration by default to bind the settings
//builder.AddConfigurationSettingsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

//Add all configuration classes marked with [ConfigurationSettings] attribute from specified assembly
//Uses builder.Configuration by default to bind the settings
//builder.AddConfigurationSettingsFromAssembly(typeof(Program).Assembly);

//Add all configuration classes marked with [ConfigurationSettings] attribute from all assemblies in the current AppDomain
//Uses builder.Configuration by default to bind the settings
builder.AddConfigurationSettings();

//Add all configuration classes marked with [ConfigurationSettings] attribute from all assemblies in the current AppDomain
//Also uses the specified configuration to bind the settings
//builder.Services.AddConfigurationSettings(builder.Configuration);

//Add all configuration classes marked with [ConfigurationSettings] attribute from specified assembly
//Also uses the specified configuration to bind the settings
//builder.Services.AddConfigurationSettingsFromAssembly(typeof(Program).Assembly, builder.Configuration);

//Add all configuration classes marked with [ConfigurationSettings] attribute from specified assemblies
//Also uses the specified configuration to bind the settings
//builder.Services.AddConfigurationSettingsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies(), builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<SampleConfiguration>()
    .Validate((some) =>
    {
        return true;
    }, "NO SE PUEDE");

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