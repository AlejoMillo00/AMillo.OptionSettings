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
        MethodInfo? configureMethod = GetAddOptionsMethod();

        if (configureMethod is null)
        {
            throw new InvalidOperationException(
                "Couldn't get AddOptions generic methods.");
        }

        MethodInfo? bindMethod = GetBindMethod();

        if (bindMethod is null)
        {
            throw new InvalidOperationException(
                "Couldn't get Bind generic methods.");
        }

        MethodInfo? validateDataAnnotationsMethod = GetValidateDataAnnotationsMethod();
        MethodInfo? validateOnStartMethod = GetValidateOnStartMethod();

        foreach (Type configurationSetting in GetConfigurationSettings(assemblies))
        {
            (string sectionName, bool validateDataAnnotations, bool validateOnStart) = 
                GetAttributeData(configurationSetting);

            MethodInfo configureMethodForCurrentSetting = configureMethod
                .MakeGenericMethod(configurationSetting);

            object? optionsBuilder = configureMethodForCurrentSetting
                .Invoke(null, [services]);

            if (optionsBuilder is null)
            {
                throw new InvalidOperationException(
                    $"Couldn't get options builder for setting of type: {configurationSetting.FullName}");
            }

            MethodInfo bindMethodForCurrentSetting = bindMethod
                .MakeGenericMethod(configurationSetting);

            optionsBuilder = bindMethodForCurrentSetting
                .Invoke(null, [optionsBuilder, 
                    configuration.GetSection(sectionName)]);

            if (validateDataAnnotations)
            {
                if (validateDataAnnotationsMethod is null)
                    throw new InvalidOperationException(
                        "Couldn't get ValidateDataAnnotations generic method.");

                MethodInfo validateDataAnnotationsMethodForCurrentSetting = validateDataAnnotationsMethod
                .MakeGenericMethod(configurationSetting);

                optionsBuilder = validateDataAnnotationsMethodForCurrentSetting
                    .Invoke(null, [optionsBuilder]);
            }

            if (validateOnStart)
            {
                if (validateOnStartMethod is null)
                    throw new InvalidOperationException(
                        "Couldn't get ValidateOnStart generic method.");

                MethodInfo validateOnStartMethodForCurrentSetting = validateOnStartMethod
                .MakeGenericMethod(configurationSetting);

                validateOnStartMethodForCurrentSetting
                    .Invoke(null, [optionsBuilder]);
            }
        }
    }

    private static MethodInfo? GetAddOptionsMethod()
    {
        return typeof(OptionsServiceCollectionExtensions)
            .GetMethods()
            .FirstOrDefault(method => method.Name == "AddOptions"
            && method.IsGenericMethod
            && method.GetGenericArguments().Length == 1
            && method.GetParameters().Length == 1
            && method.GetParameters()[0].ParameterType == typeof(IServiceCollection));
    }

    private static MethodInfo? GetBindMethod()
    {
        return typeof(OptionsBuilderConfigurationExtensions)
            .GetMethods()
            .FirstOrDefault(method => method.Name == "Bind"
            && method.IsGenericMethod
            && method.GetGenericArguments().Length == 1
            && method.GetParameters().Length == 2
            && method.GetParameters()[1].ParameterType == typeof(IConfiguration));
    }

    private static MethodInfo? GetValidateDataAnnotationsMethod()
    {
        return typeof(OptionsBuilderDataAnnotationsExtensions)
                    .GetMethods()
                    .FirstOrDefault(method => method.Name == "ValidateDataAnnotations"
                    && method.IsGenericMethod
                    && method.GetGenericArguments().Length == 1
                    && method.GetParameters().Length == 1);
    }

    private static MethodInfo? GetValidateOnStartMethod()
    {
        return typeof(OptionsBuilderExtensions)
                    .GetMethods()
                    .FirstOrDefault(method => method.Name == "ValidateOnStart"
                    && method.IsGenericMethod
                    && method.GetGenericArguments().Length == 1
                    && method.GetParameters().Length == 1);
    }

    private static Type[] GetConfigurationSettings(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsClass && t.IsDefined(typeof(ConfigurationSettingsAttribute), false))
            .ToArray();
    }

    private static (string sectionName, bool validateDataAnnotations, bool validateOnStart) GetAttributeData(Type configurationSetting)
    {
        IList<CustomAttributeData> attributesData = configurationSetting.GetCustomAttributesData();

        CustomAttributeData injectableAttributeData = attributesData
            .First(a => a.AttributeType == typeof(ConfigurationSettingsAttribute));

        CustomAttributeTypedArgument sectionNameArgument = injectableAttributeData
            .ConstructorArguments
            .First();

        CustomAttributeNamedArgument validateDataAnnotationsArgument = injectableAttributeData
            .NamedArguments
            .First(a => a.MemberName == "ValidateDataAnnotations" && a.TypedValue.ArgumentType == typeof(bool));

        CustomAttributeNamedArgument validateOnStartArgument = injectableAttributeData
            .NamedArguments
            .Last(a => a.MemberName == "ValidateOnStart" && a.TypedValue.ArgumentType == typeof(bool));

        return (
            (string)(sectionNameArgument.Value ?? string.Empty), 
            (bool)(validateDataAnnotationsArgument.TypedValue.Value ?? false),
            (bool)(validateOnStartArgument.TypedValue.Value ?? false));
    }
}
