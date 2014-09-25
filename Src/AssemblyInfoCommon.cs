using System;
using System.Reflection;
using System.Resources;

// this is used to version artifacts. AssemblyInformationalVersion should use semantic versioning (http://semver.org/)
[assembly: AssemblyInformationalVersion("0.0.1-alpha")]
[assembly: AssemblyVersion("0.0.1.0")]

[assembly: AssemblyCompany("Kent Boogaart")]
[assembly: AssemblyProduct("PCLMock")]
[assembly: AssemblyCopyright("© Copyright. Kent Boogaart.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en-US")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif