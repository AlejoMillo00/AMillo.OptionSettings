namespace AMillo.OptionSettings.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class OptionSettingsValidationAttribute : Attribute
{
    public string FailureMessage { get; init; } = string.Empty;
}
