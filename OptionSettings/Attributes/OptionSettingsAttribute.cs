using AMIllo.OptionSettings.Enums;

namespace AMillo.OptionSettings.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class OptionSettingsAttribute(string sectionName) : Attribute
{
    private readonly string _sectionName = sectionName;
    public string SectionName {  get { return _sectionName; } }
    public ValidationMode ValidationMode { get; init; } = ValidationMode.None;
}
