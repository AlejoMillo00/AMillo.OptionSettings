namespace AMillo.ConfigurationSettings.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ConfigurationSettingValidationAttribute : Attribute
{
    public string FailureMessage { get; init; } = string.Empty;
}
