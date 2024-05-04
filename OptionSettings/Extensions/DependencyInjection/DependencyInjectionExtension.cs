namespace AMillo.OptionSettings.Extensions.DependencyInjection;

using AMillo.OptionSettings.Attributes;
using AMillo.OptionSettings.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

public static class DependencyInjectionExtension
{
    public static void AddOptionSettings(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOptionSettings(builder.Configuration);
    }

    public static void AddOptionSettingsFromAssemblies(this IHostApplicationBuilder builder, IEnumerable<Assembly> assemblies)
    {
        builder.Services.AddOptionSettingsFromAssemblies(assemblies, builder.Configuration);
    }

    public static void AddOptionSettingsFromAssembly(this IHostApplicationBuilder builder, Assembly assembly)
    {
        builder.Services.AddOptionSettingsFromAssemblies([assembly], builder.Configuration);
    }

    public static void AddOptionSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionSettingsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies(), configuration);
    }

    public static void AddOptionSettingsFromAssembly(this IServiceCollection services, Assembly assembly, IConfiguration configuration)
    {
        services.AddOptionSettingsFromAssemblies([assembly], configuration);
    }

    public static void AddOptionSettingsFromAssemblies(this IServiceCollection services, IEnumerable<Assembly> assemblies, IConfiguration configuration)
    {
        Type[] optionSettingsArray = GetOptionSettingsFromAssemblies(assemblies);

        if(optionSettingsArray == null || optionSettingsArray.Length == 0)
        {
            return;
        }

        OptionSettingsBuilder optionSettingsBuilder = new(services, configuration);

        foreach (Type optionSetting in optionSettingsArray)
        {
            optionSettingsBuilder.AddOptionSettings(optionSetting);
        }
    }

    private static Type[] GetOptionSettingsFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsClass && t.IsDefined(typeof(OptionSettingsAttribute), false))
            .ToArray();
    }
}
