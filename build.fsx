#I "Src/packages/FAKE/tools"
#r "FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.EnvironmentHelper
open Fake.MSBuildHelper
open Fake.NuGetHelper
open Fake.Testing.XUnit2

// properties
let semanticVersion = "5.0.0-alpha"
let version = (>=>) @"(?<major>\d*)\.(?<minor>\d*)\.(?<build>\d*).*?" "${major}.${minor}.${build}.0" semanticVersion
let configuration = getBuildParamOrDefault "configuration" "Release"
// can be set by passing: -ev deployToNuGet true
let deployToNuGet = getBuildParamOrDefault "deployToNuGet" "false"
let genDir = "Gen/"
let docDir = "Doc/"
let srcDir = "Src/"
let packagesDir = "Src/packages/"
let testDir = genDir @@ "Test"
let nugetDir = genDir @@ "NuGet"

Target "Clean" (fun _ ->
    CleanDirs[genDir; testDir; nugetDir]

    build (fun p ->
        { p with
            Verbosity = Some(Quiet)
            Targets = ["Clean"]
            Properties = ["Configuration", configuration]
        })
        (srcDir @@ "PCLMock.sln")
)

// would prefer to use the built-in RestorePackages function, but it restores packages in the root dir (not in Src), which causes build problems
Target "RestorePackages" (fun _ ->
    !! "./**/packages.config"
    |> Seq.iter (
        RestorePackage (fun p ->
            { p with
                OutputPath = (srcDir @@ "packages")
            })
        )
)

Target "Build" (fun _ ->
    // generate the shared assembly info
    CreateCSharpAssemblyInfoWithConfig (srcDir @@ "AssemblyInfoCommon.cs")
        [
            Attribute.Version version
            Attribute.FileVersion version
            Attribute.Configuration configuration
            Attribute.Company "Kent Boogaart"
            Attribute.Product "PCLMock"
            Attribute.Copyright "© Copyright. Kent Boogaart."
            Attribute.Trademark ""
            Attribute.Culture ""
            Attribute.StringAttribute("NeutralResourcesLanguage", "en-US", "System.Resources")
            Attribute.StringAttribute("AssemblyInformationalVersion", semanticVersion, "System.Reflection")
        ]
        (AssemblyInfoFileConfig(false))

    build (fun p ->
        { p with
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            Properties =
                [
                    "Optimize", "True"
                    "DebugSymbols", "True"
                    "Configuration", configuration
                ]
        })
        (srcDir @@ "PCLMock.sln")
)

Target "ExecuteUnitTests" (fun _ ->
    xUnit2 (fun p ->
        { p with
            ShadowCopy = false;
//            HtmlOutputPath = Some testDir;
//            XmlOutputPath = Some testDir;
        })
        [srcDir @@ "PCLMock.UnitTests/bin" @@ configuration @@ "PCLMock.UnitTests.dll"]
)

Target "CreateArchives" (fun _ ->
    // source archive
    !! "**/*.*"
        -- ".git/**"
        -- (genDir @@ "**")
        -- (srcDir @@ "**/.vs/**")
        -- (srcDir @@ "packages/**/*")
        -- (srcDir @@ "**/*.suo")
        -- (srcDir @@ "**/*.csproj.user")
        -- (srcDir @@ "**/*.gpState")
        -- (srcDir @@ "**/bin/**")
        -- (srcDir @@ "**/obj/**")
        |> Zip "." (genDir @@ "PCLMock-" + semanticVersion + "-src.zip")

    // binary archive
    let workingDir = srcDir @@ "PCLMock/bin" @@ configuration

    !! (workingDir @@ "PCLMock.*")
        |> Zip workingDir (genDir @@ "PCLMock-" + semanticVersion + "-bin.zip")
)

Target "CreateNuGetPackages" (fun _ ->
    // copy files required in the various NuGets
    !! (srcDir @@ "PCLMock/bin" @@ configuration @@ "PCLMock.*")
        |> CopyFiles (nugetDir @@ "PCLMock/lib/portable-win+net40+sl50+WindowsPhoneApp81+wp80+MonoAndroid+Xamarin.iOS10+MonoTouch")

    !! (srcDir @@ "PCLMock.CodeGeneration/bin" @@ configuration @@ "PCLMock.CodeGeneration.*")
        |> CopyFiles (nugetDir @@ "PCLMock.CodeGeneration/lib/net45")

    !! (srcDir @@ "PCLMock.CodeGeneration.T4/bin" @@ configuration @@ "Mocks.*")
        |> CopyFiles (nugetDir @@ "PCLMock.CodeGeneration.T4/content")
    !! (srcDir @@ "PCLMock.CodeGeneration.T4/bin" @@ configuration @@ "*.*")
        -- ("**/*.xml")
        |> CopyFiles (nugetDir @@ "PCLMock.CodeGeneration.T4/tools")

    !! (srcDir @@ "PCLMock.CodeGeneration.Console/bin" @@ configuration @@ "*.*")
        -- ("**/*.xml")
        |> CopyFiles (nugetDir @@ "PCLMock.CodeGeneration.Console/tools")

    // copy source
    let sourceFiles =
        [!! (srcDir @@ "**/*.*")
            -- ".git/**"
            -- (srcDir @@ "**/.vs/**")
            -- (srcDir @@ "packages/**/*")
            -- (srcDir @@ "**/*.suo")
            -- (srcDir @@ "**/*.csproj.user")
            -- (srcDir @@ "**/*.gpState")
            -- (srcDir @@ "**/bin/**")
            -- (srcDir @@ "**/obj/**")
            -- (srcDir @@ "PCLMock.CodeGeneration.*/**")
            -- (srcDir @@ "PCLMock.UnitTests/**")]
    sourceFiles
        |> CopyWithSubfoldersTo (nugetDir @@ "PCLMock")
    sourceFiles
        |> CopyWithSubfoldersTo (nugetDir @@ "PCLMock.CodeGeneration")

    // create the NuGets
    NuGet (fun p ->
        {p with
            Project = "PCLMock"
            Version = semanticVersion
            OutputPath = nugetDir
            WorkingDir = nugetDir @@ "PCLMock"
            SymbolPackage = NugetSymbolPackage.Nuspec
            Publish = System.Convert.ToBoolean(deployToNuGet)
        })
        (srcDir @@ "PCLMock.nuspec")

    NuGet (fun p ->
        {p with
            Project = "PCLMock.CodeGeneration"
            Version = semanticVersion
            OutputPath = nugetDir
            WorkingDir = nugetDir @@ "PCLMock.CodeGeneration"
            SymbolPackage = NugetSymbolPackage.Nuspec
            Dependencies =
                [
                    "Microsoft.CodeAnalysis", GetPackageVersion packagesDir "Microsoft.CodeAnalysis"
                    "Microsoft.CodeAnalysis.Analyzers", GetPackageVersion packagesDir "Microsoft.CodeAnalysis.Analyzers"
                    "Microsoft.CodeAnalysis.Common", GetPackageVersion packagesDir "Microsoft.CodeAnalysis.Common"
                    "Microsoft.CodeAnalysis.CSharp", GetPackageVersion packagesDir "Microsoft.CodeAnalysis.CSharp"
                    "Microsoft.CodeAnalysis.CSharp.Workspaces", GetPackageVersion packagesDir "Microsoft.CodeAnalysis.CSharp.Workspaces"
                    "Microsoft.CodeAnalysis.VisualBasic", GetPackageVersion packagesDir "Microsoft.CodeAnalysis.VisualBasic"
                    "Microsoft.CodeAnalysis.VisualBasic.Workspaces", GetPackageVersion packagesDir "Microsoft.CodeAnalysis.VisualBasic.Workspaces"
                    "Microsoft.CodeAnalysis.Workspaces.Common", GetPackageVersion packagesDir "Microsoft.CodeAnalysis.Workspaces.Common"
                    "Microsoft.Composition", GetPackageVersion packagesDir "Microsoft.Composition"
                    "System.Collections", GetPackageVersion packagesDir "System.Collections"
                    "System.Collections.Immutable", GetPackageVersion packagesDir "System.Collections.Immutable"
                    "System.Diagnostics.Debug", GetPackageVersion packagesDir "System.Diagnostics.Debug"
                    "System.Globalization", GetPackageVersion packagesDir "System.Globalization"
                    "System.IO", GetPackageVersion packagesDir "System.IO"
                    "System.Linq", GetPackageVersion packagesDir "System.Linq"
                    "System.Reflection", GetPackageVersion packagesDir "System.Reflection"
                    "System.Reflection.Extensions", GetPackageVersion packagesDir "System.Reflection.Extensions"
                    "System.Reflection.Metadata", GetPackageVersion packagesDir "System.Reflection.Metadata"
                    "System.Reflection.Primitives", GetPackageVersion packagesDir "System.Reflection.Primitives"
                    "System.Resources.ResourceManager", GetPackageVersion packagesDir "System.Resources.ResourceManager"
                    "System.Runtime", GetPackageVersion packagesDir "System.Runtime"
                    "System.Runtime.Extensions", GetPackageVersion packagesDir "System.Runtime.Extensions"
                    "System.Runtime.InteropServices", GetPackageVersion packagesDir "System.Runtime.InteropServices"
                    "System.Text.Encoding", GetPackageVersion packagesDir "System.Text.Encoding"
                    "System.Text.Encoding.Extensions", GetPackageVersion packagesDir "System.Text.Encoding.Extensions"
                    "System.Threading", GetPackageVersion packagesDir "System.Threading"
                    "PCLMock", semanticVersion
                ]
            Publish = System.Convert.ToBoolean(deployToNuGet)
        })
        (srcDir @@ "PCLMock.CodeGeneration.nuspec")

    NuGet (fun p ->
        {p with
            Project = "PCLMock.CodeGeneration.T4"
            Version = semanticVersion
            OutputPath = nugetDir
            WorkingDir = nugetDir @@ "PCLMock.CodeGeneration.T4"
            SymbolPackage = NugetSymbolPackage.None
            Dependencies =
                [
                    "PCLMock", semanticVersion
                ]
            Publish = System.Convert.ToBoolean(deployToNuGet)
        })
        (srcDir @@ "PCLMock.CodeGeneration.T4.nuspec")

    NuGet (fun p ->
        {p with
            Project = "PCLMock.CodeGeneration.Console"
            Version = semanticVersion
            OutputPath = nugetDir
            WorkingDir = nugetDir @@ "PCLMock.CodeGeneration.Console"
            SymbolPackage = NugetSymbolPackage.None
            Publish = System.Convert.ToBoolean(deployToNuGet)
        })
        (srcDir @@ "PCLMock.CodeGeneration.Console.nuspec")
)

// build order
"Clean"
    ==> "RestorePackages"
    ==> "Build"
    ==> "ExecuteUnitTests"
    ==> "CreateArchives"
    ==> "CreateNuGetPackages"

RunTargetOrDefault "CreateNuGetPackages"