using AMIllo.OptionSettings.Enums;

namespace AMillo.OptionSettings.Models;

internal sealed class OptionSettingsAttributeData
{
    public string SectionName { get; init; } = string.Empty;
    public ValidationMode ValidationMode { get; init; } = ValidationMode.None;
}
