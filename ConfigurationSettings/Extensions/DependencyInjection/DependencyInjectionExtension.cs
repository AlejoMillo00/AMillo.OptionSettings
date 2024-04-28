namespace AMillo.ConfigurationSettings.Extensions.DependencyInjection;

using AMillo.ConfigurationSettings.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

public static class DependencyInjectionExtension
{
    public static void AddConfigurationSettings(this IHostApplicationBuilder builder)
    {
        builder.Services.AddConfigurationSettings(builder.Configuration);
    }

    public static void AddConfigurationSettingsFromAssemblies(this IHostApplicationBuilder builder, IEnumerable<Assembly> assemblies)
    {
        builder.Services.AddConfigurationSettingsFromAssemblies(assemblies, builder.Configuration);
    }

    public static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigurationSettingsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies(), configuration);
    }

    public static void AddConfigurationSettingsFromAssemblies(this IServiceCollection services, IEnumerable<Assembly> assemblies, IConfiguration configuration)
    {
        MethodInfo? configureMethod = GetConfigureMethod();

        if (configureMethod is null)
            throw new InvalidOperationException("Couldn't get Configure method from OptionsConfigurationServiceCollectionExtensions");

        foreach (Type configurationSetting in GetConfigurationSettings(assemblies))
        {
            MethodInfo configureMethodForCurrentSetting = configureMethod.MakeGenericMethod(configurationSetting);
            configureMethodForCurrentSetting.Invoke(null, [services, configuration.GetSection(GetSectionName(configurationSetting))]);
        }
    }

    private static MethodInfo? GetConfigureMethod()
    {
        return typeof(OptionsConfigurationServiceCollectionExtensions)
           .GetMethods()
           .FirstOrDefault(method => method.Name == "Configure"
           && method.IsGenericMethod
           && method.GetGenericArguments().Length == 1
           && method.GetParameters().Length == 2
           && method.GetParameters()[1].ParameterType == typeof(IConfiguration));
    }

    private static Type[] GetConfigurationSettings(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsClass && t.IsDefined(typeof(ConfigurationSettingsAttribute), false))
            .ToArray();
    }

    private static string GetSectionName(Type configurationSetting)
    {
        IList<CustomAttributeData> attributesData = configurationSetting.GetCustomAttributesData();

        CustomAttributeData injectableAttributeData = attributesData
            .First(a => a.AttributeType == typeof(ConfigurationSettingsAttribute));

        CustomAttributeTypedArgument sectionNameArgument = injectableAttributeData
            .ConstructorArguments
            .First(a => a.ArgumentType == typeof(string));

        return (string)(sectionNameArgument.Value ?? string.Empty);
    }
}
