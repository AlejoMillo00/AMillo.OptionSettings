namespace AMillo.ConfigurationSettings.Extensions.DependencyInjection;

using AMillo.ConfigurationSettings.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
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

    public static void AddConfigurationSettingsFromAssembly(this IHostApplicationBuilder builder, Assembly assembly)
    {
        builder.Services.AddConfigurationSettingsFromAssemblies([assembly], builder.Configuration);
    }

    public static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigurationSettingsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies(), configuration);
    }

    public static void AddConfigurationSettingsFromAssembly(this IServiceCollection services, Assembly assembly, IConfiguration configuration)
    {
        services.AddConfigurationSettingsFromAssemblies([assembly], configuration);
    }

    public static void AddConfigurationSettingsFromAssemblies(this IServiceCollection services, IEnumerable<Assembly> assemblies, IConfiguration configuration)
    {
        MethodInfo? addOptionsMethod = GetAddOptionsMethod();
        ArgumentNullException.ThrowIfNull(addOptionsMethod);

        MethodInfo? bindMethod = GetBindMethod();
        ArgumentNullException.ThrowIfNull(bindMethod);

        MethodInfo? validateDataAnnotationsMethod = GetValidateDataAnnotationsMethod();
        MethodInfo? validateOnStartMethod = GetValidateOnStartMethod();

        foreach (Type configurationSetting in GetConfigurationSettings(assemblies))
        {
            (string sectionName, bool validateDataAnnotations, bool validateOnStart) = 
                GetAttributeData(configurationSetting);

            MethodInfo configureMethodForCurrentSetting = addOptionsMethod
                .MakeGenericMethod(configurationSetting);

            object? optionsBuilder = configureMethodForCurrentSetting
                .Invoke(null, [services]);

            ArgumentNullException.ThrowIfNull(optionsBuilder);

            MethodInfo bindMethodForCurrentSetting = bindMethod
                .MakeGenericMethod(configurationSetting);

            optionsBuilder = bindMethodForCurrentSetting
                .Invoke(null, [optionsBuilder, 
                    configuration.GetSection(sectionName)]);

            ArgumentNullException.ThrowIfNull(optionsBuilder);

            MethodInfo[] customValidationMethods = configurationSetting
                 .GetMethods()
                 .Where(m => m.CustomAttributes.Count() == 1 
                    && m.CustomAttributes.First().AttributeType == typeof(ConfigurationSettingValidationAttribute))
                 .ToArray();

            foreach (MethodInfo customValidationMethod in customValidationMethods)
            {               
                Type funcType = typeof(Func<,>).MakeGenericType(configurationSetting, typeof(bool));
                Delegate validationDelegate = Delegate.CreateDelegate(funcType, customValidationMethod);

                Type optionsBuilderType = typeof(OptionsBuilder<>).MakeGenericType(configurationSetting);

                string failureMessage = GetCustomValidationMethodFailureMessage(customValidationMethod);

                if (string.IsNullOrEmpty(failureMessage))
                {
                    MethodInfo? validateMethod = optionsBuilderType
                        .GetMethods()
                        .FirstOrDefault(m => {
                            if(m.Name != "Validate" || m.IsGenericMethod)
                            {
                                return false;
                            }

                            ParameterInfo[] parameters = m.GetParameters();
                            return parameters.Length == 1
                                && parameters[0].ParameterType == funcType;
                        });
                    ArgumentNullException.ThrowIfNull(validateMethod);

                    validateMethod
                        .Invoke(optionsBuilder, [validationDelegate]);
                } 
                else
                {
                    MethodInfo? validateMethodWithFailureMessage = optionsBuilderType
                        .GetMethods()
                        .FirstOrDefault(m => {
                            if (m.Name != "Validate" || m.IsGenericMethod)
                            {
                                return false;
                            }

                            ParameterInfo[] parameters = m.GetParameters();
                            return parameters.Length == 2
                                && parameters[0].ParameterType == funcType
                                && parameters[1].ParameterType == typeof(string);
                        });
                    ArgumentNullException.ThrowIfNull(validateMethodWithFailureMessage);

                    validateMethodWithFailureMessage
                        .Invoke(optionsBuilder, [validationDelegate, failureMessage]);
                }
            }

            if (validateDataAnnotations)
            {
                ArgumentNullException.ThrowIfNull(validateDataAnnotationsMethod);

                MethodInfo validateDataAnnotationsMethodForCurrentSetting = validateDataAnnotationsMethod
                .MakeGenericMethod(configurationSetting);

                optionsBuilder = validateDataAnnotationsMethodForCurrentSetting
                    .Invoke(null, [optionsBuilder]);
            }

            if (validateOnStart)
            {
                ArgumentNullException.ThrowIfNull(validateOnStartMethod);

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
            .FirstOrDefault(method => {
                if(method.Name != "AddOptions" || !method.IsGenericMethod)
                {
                    return false;
                }

                ParameterInfo[] parameters = method.GetParameters();
                return method.GetGenericArguments().Length == 1
                && parameters.Length == 1
                && parameters[0].ParameterType == typeof(IServiceCollection);
            });
    }

    private static MethodInfo? GetBindMethod()
    {
        return typeof(OptionsBuilderConfigurationExtensions)
            .GetMethods()
            .FirstOrDefault(method => {
                if (method.Name != "Bind" || !method.IsGenericMethod)
                {
                    return false;
                }
                ParameterInfo[] parameters = method.GetParameters();
                return method.GetGenericArguments().Length == 1
                && parameters.Length == 2
                && parameters[1].ParameterType == typeof(IConfiguration);
            });
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

        CustomAttributeData configurationSettingAttributeData = attributesData
            .First(a => a.AttributeType == typeof(ConfigurationSettingsAttribute));

        CustomAttributeTypedArgument sectionNameArgument = configurationSettingAttributeData
            .ConstructorArguments
            .First();

        CustomAttributeNamedArgument validateDataAnnotationsArgument = configurationSettingAttributeData
            .NamedArguments
            .First(a => a.MemberName == "ValidateDataAnnotations" && a.TypedValue.ArgumentType == typeof(bool));

        CustomAttributeNamedArgument validateOnStartArgument = configurationSettingAttributeData
            .NamedArguments
            .Last(a => a.MemberName == "ValidateOnStart" && a.TypedValue.ArgumentType == typeof(bool));

        return (
            (string)(sectionNameArgument.Value ?? string.Empty), 
            (bool)(validateDataAnnotationsArgument.TypedValue.Value ?? false),
            (bool)(validateOnStartArgument.TypedValue.Value ?? false));
    }

    private static string GetCustomValidationMethodFailureMessage(MethodInfo customValidationMethod)
    {
        IList<CustomAttributeData> attributesData = customValidationMethod.GetCustomAttributesData();

        CustomAttributeData configurationSettingAttributeData = attributesData
            .First(a => a.AttributeType == typeof(ConfigurationSettingValidationAttribute));

        CustomAttributeNamedArgument failureMessageArgument = configurationSettingAttributeData
            .NamedArguments
            .Last(a => a.MemberName == "FailureMessage" && a.TypedValue.ArgumentType == typeof(string));

        return (string)(failureMessageArgument.TypedValue.Value ?? "A validation error has ocurred.");
    }
}
