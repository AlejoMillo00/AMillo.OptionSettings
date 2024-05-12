<a name="readme-top"></a>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h3 align="center">OptionSettings</h3>

  <p align="center">
    <a href="https://github.com/AlejoMillo00/AMillo.OptionSettings/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    ·
    <a href="https://github.com/AlejoMillo00/AMillo.OptionSettings/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>

## About The Project

OptionSettings is a simple feature that allows you to register your configuration/setting classes without having to add them to the Program / Startup file, keeping them clean and smooth.

* Follow best practices using Options Pattern
* Cleaner and readable Program / Startup files, keep them small.
* Make your configuration/setting classes ready-to-use just as you finish creating them, you don't even need to go into the Program / Startup file.

## Getting Started
### Installation
- .NET CLI
  ```sh
  dotnet add package AMillo.OptionSettings --version 1.0.0
  ```
- Package Manager
  ```sh
  Install-Package AMillo.OptionSettings -Version 1.0.0
  ```
### Usage
1. Add the following <strong>using</strong> directive on your Program.cs / Startup.cs file
   <pre lang="cs">using AMillo.OptionSettings.Extensions.DependencyInjection;</pre>
2. Call the <strong>AddOptionSettings</strong> extension method using one of the following overloads
   - builder.AddOptionSettings()
     <pre lang="cs">
     //Add all configuration classes marked with [OptionSettings] attribute from all assemblies in the current AppDomain
     //Uses builder.Configuration by default to bind the settings
     builder.AddOptionSettings();</pre>
   - builder.AddOptionSettingsFromAssemblies(IEnumerable<Assembly> assemblies)
     <pre lang="cs">
     //Add all configuration classes marked with [OptionSettings] attribute from specified assemblies
     //Uses builder.Configuration by default to bind the settings
     builder.AddOptionSettingsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()); 
     </pre>
   - builder.AddOptionSettingsFromAssembly(Assembly assembly)
     <pre lang="cs">
     //Add all configuration classes marked with [OptionSettings] attribute from specified assembly
     //Uses builder.Configuration by default to bind the settings
     builder.AddOptionSettingsFromAssembly(typeof(Program).Assembly);
     </pre>
   - builder.Services.AddOptionSettings(IConfiguration configuration)
     <pre lang="cs">
     //Add all configuration classes marked with [OptionSettings] attribute from all assemblies in the current AppDomain
     //Also uses the specified configuration to bind the settings
     builder.Services.AddOptionSettings(builder.Configuration);
     </pre>
   - builder.Services.AddOptionSettingsFromAssembly(Assembly assembly, IConfiguration configuration)
     <pre lang="cs">
     //Add all configuration classes marked with [OptionSettings] attribute from specified assembly
     //Also uses the specified configuration to bind the settings
     builder.Services.AddOptionSettingsFromAssembly(typeof(Program).Assembly, builder.Configuration);
     </pre>
   - builder.Services.AddOptionSettingsFromAssemblies(IEnumerable<Assembly> assemblies, IConfiguration configuration)
     <pre lang="cs">
     //Add all configuration classes marked with [ConfigurationSettings] attribute from specified assemblies
     //Also uses the specified configuration to bind the settings
     builder.Services.AddOptionSettingsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies(), builder.Configuration);
     </pre>
3. Mark your configuration class with the <strong>[OptionSettings]</strong> attribute.
   <pre lang="cs">
      using AMillo.OptionSettings.Attributes;

      [OptionSettings(sectionName: Constants.AppSettings.Sample)]
      internal sealed class SampleConfiguration
      {
          public string SampleKey { get; set; } = string.Empty;
          public int SampleNumber { get; set; } = 0;
      }
   </pre>
   
4. That's it! Now you can start using you configuration class following the Options pattern with IOptions, IOptionsMonitor or IOptionsSnapshot.
     
### Important

You need to specify the <strong>"sectionName"</strong> to the attribute's constructor, this value needs to match the section in your AppSettings file from where you want your configuration/setting class get configured. <br/><br/>
This is how the AppSettings.json file looks like for the previous example:
<pre lang="json">
  "SampleConfiguration": {
    "SampleKey": "SomeKey",
    "SampleNumber": 1234567890
  }
</pre>

## Validations
OptionSettings has full support for you to validate your configuration classes as much as you like.

#### 1. Set the <strong>ValidationMode</strong> in the <strong>[OptionSettings]</strong> attribute:
- <strong>ValidationMode.None</strong>: This is the default, with this option your configuration class will not be validated at all.
<pre lang="cs">
  using AMillo.OptionSettings.Attributes;
  using AMIllo.OptionSettings.Enums;
  
  [OptionSettings(
      sectionName: Constants.AppSettings.Sample, 
      ValidationMode = ValidationMode.None)]
  internal sealed class SampleConfiguration { }
</pre>
- <strong>ValidationMode.Startup</strong>: The values will be validated when the application starts.
<pre lang="cs">
  using AMillo.OptionSettings.Attributes;
  using AMIllo.OptionSettings.Enums;

  [OptionSettings(
      sectionName: Constants.AppSettings.Sample, 
      ValidationMode = ValidationMode.Startup)]
  internal sealed class SampleConfiguration { }
</pre>
- <strong>ValidationMode.Runtime</strong>: The values will be validated on runtime when trying to access the configuration class.
<pre lang="cs">
  using AMillo.OptionSettings.Attributes;
  using AMIllo.OptionSettings.Enums;

  [OptionSettings(
      sectionName: Constants.AppSettings.Sample, 
      ValidationMode = ValidationMode.Runtime)]
  internal sealed class SampleConfiguration { }
</pre>

*NOTE*: Personally. I recommend using <strong>ValidationMode.Startup</strong> so the application can not run with invalid configuration values and fails as soon as possible, at startup time.

#### 2. Add DataAnnotations to validate your configuration class properties:
<pre lang="cs">
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
  }
</pre>

#### 3. You can also add multiple validation methods and mark them with the *[OptionSettingsValidation]* attribute. This allows you validate your configuration class values in more complex ways.
- The methods must have the following signature:
  <pre lang="cs">
    bool AnyMethodName(TOptions options)
  </pre>
  Where <strong>TOptions</strong> is your configuration class.
- You can pass a <strong>FailureMessage</strong> to the <strong>[OptionSettingsValidation]</strong> attribute if you want to show a custom message if the validation fails.
  <pre lang="cs">
    [OptionSettingsValidation(FailureMessage = "Custom failure messsage")]
  </pre>
- The methods only returns <strong>true</strong> indicating the validation succeeded or <strong>false</strong> indicating the validation has failed.
- These validations methods will run depending on the value you have configured into the <strong>ValidationMode</strong> option.
  
<pre lang="cs">
  using AMillo.OptionSettings.Attributes;
  using AMIllo.OptionSettings.Enums;
  using System.ComponentModel.DataAnnotations;
  
  [OptionSettings(
    sectionName: Constants.AppSettings.Sample, 
    ValidationMode = ValidationMode.Runtime)]
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
</pre>

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Contact

Alejo Millo - alejo.millo@outlook.com

<!-- MARKDOWN LINKS & IMAGES -->
[contributors-shield]: https://img.shields.io/github/contributors/AlejoMillo00/AMillo.OptionSettings.svg?style=for-the-badge
[contributors-url]: https://github.com/AlejoMillo00/AMillo.OptionSettings/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/AlejoMillo00/AMillo.OptionSettings.svg?style=for-the-badge
[forks-url]: https://github.com/AlejoMillo00/AMillo.OptionSettings/network/members
[stars-shield]: https://img.shields.io/github/stars/AlejoMillo00/AMillo.OptionSettings.svg?style=for-the-badge
[stars-url]: https://github.com/AlejoMillo00/AMillo.OptionSettings/stargazers
[issues-shield]: https://img.shields.io/github/issues/AlejoMillo00/AMillo.OptionSettings.svg?style=for-the-badge
[issues-url]: https://github.com/AlejoMillo00/AMillo.OptionSettings/issues
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/in/alejo-millo-77371a196/
