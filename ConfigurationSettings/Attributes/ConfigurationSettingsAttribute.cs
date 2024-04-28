namespace AMillo.ConfigurationSettings.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ConfigurationSettingsAttribute(string sectionName) : Attribute
{
    private readonly string _sectionName = sectionName;
    public string SectionName {  get { return _sectionName; } }
}
