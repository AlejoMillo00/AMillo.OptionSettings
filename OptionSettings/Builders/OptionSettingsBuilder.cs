namespace AMillo.OptionSettings.Builders;

using AMillo.OptionSettings.Attributes;
using AMillo.OptionSettings.Models;
using AMIllo.OptionSettings.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

internal sealed class OptionSettingsBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;
    private readonly MethodInfo _addOptionsGenericMethod;
    private readonly MethodInfo _bindGenericMethod;
    private readonly MethodInfo _validateDataAnnotationsGenericMethod;
    private readonly MethodInfo _validateOnStartGenericMethod;

    private Type? _currentOptionSettings;
    private OptionSettingsAttributeData? _currentOptionSettingsData;
    private object? _optionsBuilder;

    private Type? _validateFuncType;
    private Type? _optionsBuilderType;
    private MethodInfo? _currentCustomValidationMethod;
    private Delegate? _currentValidateDelegate;
    private MethodInfo? _currentInvokableValidateMethod;
    private string _currentValidateFailureMessage = string.Empty;

    internal OptionSettingsBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
        _addOptionsGenericMethod = GenericMethodsBuilder.GetAddOptionsMethod();
        _bindGenericMethod = GenericMethodsBuilder.GetBindMethod();
        _validateDataAnnotationsGenericMethod = GenericMethodsBuilder.GetValidateDataAnnotationsMethod();
        _validateOnStartGenericMethod = GenericMethodsBuilder.GetValidateOnStartMethod();
    }

    internal void AddOptionSettings(Type optionSettings)
    {
        SetUpCurrentOptionSettings(optionSettings);
        InvokeAddOptionsMethod();
        InvokeBindMethod();

        if (ShouldSkipValidations())
        {
            return;
        }

        InvokeAllValidateMethods();
        InvokeValidateDataAnnotationsMethod();

        if (ShouldValidateOnStart())
        {
            InvokeValidateOnStartMethod();
        }   
    }

    private void SetUpCurrentOptionSettings(Type optionSettings)
    {
        _currentOptionSettings = optionSettings;
        _currentOptionSettingsData = GetAttributeData();
        _validateFuncType = GetValidateFuncType();
        _optionsBuilderType = GetOptionsBuilderType();
    }

    private void InvokeAddOptionsMethod()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettings);

        MethodInfo addOptionsMethod = _addOptionsGenericMethod
                .MakeGenericMethod(_currentOptionSettings);

        _optionsBuilder = addOptionsMethod.Invoke(null, [_services]);
    }

    private void InvokeBindMethod()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettings);
        ArgumentNullException.ThrowIfNull(_currentOptionSettingsData);

        MethodInfo bindMethod = _bindGenericMethod
                .MakeGenericMethod(_currentOptionSettings);

        _optionsBuilder = bindMethod
            .Invoke(null, [_optionsBuilder,
                    _configuration.GetSection(_currentOptionSettingsData.SectionName)]);
    }

    private bool ShouldSkipValidations()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettingsData);

        return _currentOptionSettingsData.ValidationMode == ValidationMode.None;
    }

    private void InvokeAllValidateMethods()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettings);
        ArgumentNullException.ThrowIfNull(_optionsBuilder);

        foreach (MethodInfo customValidationMethod in GetCustomValidationMethods())
        {
            SetCurrentCustomValidationMethod(customValidationMethod);
            SetCurrentValidateDelegate();
            SetCurrentValidateFailureMessage();
            SetCurrentInvokableValidateMethod();
            InvokeCurrentValidateMethod();
        }
    }

    private void InvokeValidateDataAnnotationsMethod()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettings);
        ArgumentNullException.ThrowIfNull(_optionsBuilder);

        MethodInfo validateDataAnnotationsMethod = _validateDataAnnotationsGenericMethod
            .MakeGenericMethod(_currentOptionSettings);

        _optionsBuilder = validateDataAnnotationsMethod.Invoke(null, [_optionsBuilder]);
    }

    private bool ShouldValidateOnStart()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettingsData);

        return _currentOptionSettingsData.ValidationMode == ValidationMode.Startup;
    }

    private void InvokeValidateOnStartMethod()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettings);
        ArgumentNullException.ThrowIfNull(_optionsBuilder);

        MethodInfo validateOnStartMethod = _validateOnStartGenericMethod
            .MakeGenericMethod(_currentOptionSettings);

        validateOnStartMethod.Invoke(null, [_optionsBuilder]);
    }

    private OptionSettingsAttributeData GetAttributeData()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettings);

        IList<CustomAttributeData> attributesData = _currentOptionSettings.GetCustomAttributesData();

        CustomAttributeData configurationSettingAttributeData = attributesData
            .First(a => a.AttributeType == typeof(OptionSettingsAttribute));

        CustomAttributeTypedArgument sectionName = configurationSettingAttributeData
            .ConstructorArguments
            .First();

        CustomAttributeNamedArgument validationMode = configurationSettingAttributeData
            .NamedArguments
            .First(a => a.MemberName == "ValidationMode" && a.TypedValue.ArgumentType == typeof(ValidationMode));

        return new OptionSettingsAttributeData
        {
            SectionName = (string)(sectionName.Value ?? string.Empty),
            ValidationMode = (ValidationMode)(validationMode.TypedValue.Value ?? ValidationMode.None),
        };
    }

    private Type GetValidateFuncType()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettings);

        return typeof(Func<,>).MakeGenericType(_currentOptionSettings, typeof(bool));
    }

    private Type GetOptionsBuilderType()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettings);

        return typeof(OptionsBuilder<>).MakeGenericType(_currentOptionSettings);
    }

    private MethodInfo[] GetCustomValidationMethods()
    {
        ArgumentNullException.ThrowIfNull(_currentOptionSettings);

        return _currentOptionSettings
          .GetMethods()
          .Where(m => m.CustomAttributes.Count() == 1
            && m.CustomAttributes.First().AttributeType == typeof(OptionSettingsValidationAttribute))
          .ToArray();
    }

    private void SetCurrentCustomValidationMethod(MethodInfo customValidationMethod)
    {
        _currentCustomValidationMethod = customValidationMethod;
    }

    private void SetCurrentValidateDelegate()
    {
        ArgumentNullException.ThrowIfNull(_validateFuncType);
        ArgumentNullException.ThrowIfNull(_currentCustomValidationMethod);

        _currentValidateDelegate = Delegate.CreateDelegate(_validateFuncType, _currentCustomValidationMethod);
    }

    private void SetCurrentValidateFailureMessage()
    {
        _currentValidateFailureMessage = GetCurrentCustomValidationMethodFailureMessage();
    }

    private void SetCurrentInvokableValidateMethod()
    {
        _currentInvokableValidateMethod = GetValidateMethod();
    }

    private void InvokeCurrentValidateMethod()
    {
        ArgumentNullException.ThrowIfNull(_currentValidateDelegate);
        ArgumentNullException.ThrowIfNull(_currentInvokableValidateMethod);

        if (UseCustomValidateFailureMessage())
        {
            _currentInvokableValidateMethod.Invoke(_optionsBuilder, [_currentValidateDelegate, _currentValidateFailureMessage]);
            return;
        }

        _currentInvokableValidateMethod.Invoke(_optionsBuilder, [_currentValidateDelegate]);
    }

    private string GetCurrentCustomValidationMethodFailureMessage()
    {
        ArgumentNullException.ThrowIfNull(_currentCustomValidationMethod);

        IList<CustomAttributeData> attributesData = _currentCustomValidationMethod.GetCustomAttributesData();

        CustomAttributeData configurationSettingAttributeData = attributesData
            .First(a => a.AttributeType == typeof(OptionSettingsValidationAttribute));

        CustomAttributeNamedArgument failureMessageArgument = configurationSettingAttributeData
            .NamedArguments
            .Last(a => a.MemberName == "FailureMessage" && a.TypedValue.ArgumentType == typeof(string));

        return (string)(failureMessageArgument.TypedValue.Value ?? string.Empty);
    }

    private MethodInfo GetValidateMethod()
    {
        ArgumentNullException.ThrowIfNull(_optionsBuilderType);
        ArgumentNullException.ThrowIfNull(_validateFuncType);

        return _optionsBuilderType
            .GetMethods()
            .First(m => {
                if (m.Name != "Validate" || m.IsGenericMethod)
                {
                    return false;
                }

                ParameterInfo[] parameters = m.GetParameters();

                if (UseCustomValidateFailureMessage())
                {
                    return parameters.Length == 2
                        && parameters[0].ParameterType == _validateFuncType
                        && parameters[1].ParameterType == typeof(string);
                }

                return parameters.Length == 1
                    && parameters[0].ParameterType == _validateFuncType;
            });
    }

    private bool UseCustomValidateFailureMessage()
    {
        return !string.IsNullOrEmpty(_currentValidateFailureMessage);
    }
}
