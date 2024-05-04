namespace AMillo.OptionSettings.Demo;

using AMillo.OptionSettings.Attributes;
using AMIllo.OptionSettings.Enums;
using System.ComponentModel.DataAnnotations;

[OptionSettings(
    sectionName: Constants.AppSettings.Sample, 
    ValidationMode = ValidationMode.Startup)]
internal sealed class SampleConfiguration
{
    [Required]
    [MaxLength(20)]
    public string SampleKey { get; set; } = string.Empty;

    [Required]
    [Range(0, 10)]
    public int SampleNumber { get; set; } = 0;

    public string SampleString { get; set; } = string.Empty;

    [OptionSettingsValidation(FailureMessage = "SampleString can't contain vowels.")]
    public static bool ValidateSampleString(SampleConfiguration options)
    {
        HashSet<char> vowels = ['a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U'];

        foreach (char @char in options.SampleString)
        {
            if (vowels.Contains(@char))
            {
                return false;
            }
        }

        return true;
    }

    [OptionSettingsValidation(FailureMessage = "SampleNumber must be 1 when SampleKey is 'one'")]
    public static bool ValidateSampleNumber(SampleConfiguration options)
    {
        if(options.SampleKey == "one" && options.SampleNumber != 1)
        {
            return false;
        }

        return true;
    }
}
