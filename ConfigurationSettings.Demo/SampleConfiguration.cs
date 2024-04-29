namespace AMillo.ConfigurationSettings.Demo;

using AMillo.ConfigurationSettings.Attributes;
using System.ComponentModel.DataAnnotations;

[ConfigurationSettings(
    sectionName: Constants.AppSettings.Sample, 
    ValidateDataAnnotations = true,
    ValidateOnStart = true)]
internal sealed class SampleConfiguration
{
    [Required]
    [MaxLength(20)]
    public string SampleKey { get; set; } = string.Empty;

    [Required]
    [Range(0, 10)]
    public int SampleNumber { get; set; } = 0;
}
