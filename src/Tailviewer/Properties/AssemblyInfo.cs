using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("Application to view log files - live and offline")]
[assembly: AssemblyDescription("Application to view log files - live and offline")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Tailviewer")]
[assembly: AssemblyCopyright("Copyright © Simon Mießler 2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
	ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
	//(used if a resource is not found in the page, 
	// or application resource dictionaries)
	ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
	//(used if a resource is not found in the page, 
	// app, or any theme specific resource dictionaries)
	)]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("0.9.2.0")]
[assembly: AssemblyFileVersion("0.9.2.0")]

[assembly: InternalsVisibleTo("Tailviewer.Test,PublicKey=002400000480000094000000060200000024000052534131000400000100010051188f211b0628e26c2d3e134f62e9bc1e86af3f7a49bcbd9de1d6d2bd1f8ddcdae12e14be751c0b4a6e56a6409c8859705ee323fb30eb50df5ae79dc0e26171adeba4bbf45cba5f50c68e44d796d04a846f16446a055f3486de0a6fdaf14b655cbb741c4f96eeb01ed4e99b96f3fb23d7e58c9df71ae7ffa9601a8ff54ab7b7")]
[assembly: InternalsVisibleTo("Tailviewer.AcceptanceTests,PublicKey=002400000480000094000000060200000024000052534131000400000100010051188f211b0628e26c2d3e134f62e9bc1e86af3f7a49bcbd9de1d6d2bd1f8ddcdae12e14be751c0b4a6e56a6409c8859705ee323fb30eb50df5ae79dc0e26171adeba4bbf45cba5f50c68e44d796d04a846f16446a055f3486de0a6fdaf14b655cbb741c4f96eeb01ed4e99b96f3fb23d7e58c9df71ae7ffa9601a8ff54ab7b7")]
