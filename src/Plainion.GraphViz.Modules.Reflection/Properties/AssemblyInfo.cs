
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Security;
using System.Windows.Markup;


[assembly: AssemblyTitle( "Plainion.GraphViz.Modules.Reflection" )]
[assembly: ComVisible( false )]

// Specifies the location in which theme dictionaries are stored for types in an assembly.
[assembly: ThemeInfo(
    // Specifies the location of system theme-specific resource dictionaries for this project.
    // The default setting in this project is "None" since this default project does not
    // include these user-defined theme files:
    //     Themes\Aero.NormalColor.xaml
    //     Themes\Classic.xaml
    //     Themes\Luna.Homestead.xaml
    //     Themes\Luna.Metallic.xaml
    //     Themes\Luna.NormalColor.xaml
    //     Themes\Royale.NormalColor.xaml
    ResourceDictionaryLocation.None,

    // Specifies the location of the system non-theme specific resource dictionary:
    //     Themes\generic.xaml
    ResourceDictionaryLocation.SourceAssembly )]

[assembly: XmlnsDefinition("http://github.com/ronin4net/plainion/GraphViz/Packaging/Spec", "Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec")] 
 

[assembly: InternalsVisibleTo("Plainion.GraphViz.Modules.Reflection.Tests")]

