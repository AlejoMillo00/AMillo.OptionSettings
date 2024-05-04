namespace AMillo.OptionSettings.Models;

using AMIllo.OptionSettings.Enums;

internal sealed class OptionSettingsAttributeData
{
    public string SectionName { get; init; } = string.Empty;
    public ValidationMode ValidationMode { get; init; } = ValidationMode.None;
}
