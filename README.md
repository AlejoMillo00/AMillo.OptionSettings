<a name="readme-top"></a>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/AlejoMillo00/AMillo.OptionSettings">
    <img src="images/logo.png" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">OptionSettings</h3>

  <p align="center">
    <a href="https://github.com/AlejoMillo00/AMillo.OptionSettings/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    ·
    <a href="https://github.com/AlejoMillo00/AMillo.OptionSettings/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>

## About The Project

OptionSettings is a simple feature that allows you to register your configuration/setting classes without having to add them to the Program / Startup file, keeping them clean and smooth.

Here's why this is good:
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

You need to specify the <strong>"sectionName"</strong> to the attribute's constructor, this value needs to match the section in your AppSettings file from where you want your configuration/setting class get configured. <br /><br/>
This is how the AppSettings.json file looks like for the previous example:
<pre lang="json">
  "SampleConfiguration": {
    "SampleKey": "SomeKey",
    "SampleNumber": 1234567890
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
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
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
