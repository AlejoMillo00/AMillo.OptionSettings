namespace AMillo.OptionSettings.Builders;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

internal static class GenericMethodsBuilder
{
    internal static MethodInfo GetAddOptionsMethod()
    {
        return typeof(OptionsServiceCollectionExtensions)
            .GetMethods()
            .First(method => {
                if (method.Name != "AddOptions" || !method.IsGenericMethod)
                {
                    return false;
                }

                ParameterInfo[] parameters = method.GetParameters();
                return method.GetGenericArguments().Length == 1
                && parameters.Length == 1
                && parameters[0].ParameterType == typeof(IServiceCollection);
            });
    }

    internal static MethodInfo GetBindMethod()
    {
        return typeof(OptionsBuilderConfigurationExtensions)
            .GetMethods()
            .First(method => {
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

    internal static MethodInfo GetValidateDataAnnotationsMethod()
    {
        return typeof(OptionsBuilderDataAnnotationsExtensions)
                    .GetMethods()
                    .First(method => method.Name == "ValidateDataAnnotations"
                    && method.IsGenericMethod
                    && method.GetGenericArguments().Length == 1
                    && method.GetParameters().Length == 1);
    }

    internal static MethodInfo GetValidateOnStartMethod()
    {
        return typeof(OptionsBuilderExtensions)
                    .GetMethods()
                    .First(method => method.Name == "ValidateOnStart"
                    && method.IsGenericMethod
                    && method.GetGenericArguments().Length == 1
                    && method.GetParameters().Length == 1);
    }
}
