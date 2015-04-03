#I "Src/packages/FAKE.3.13.4/tools"
#r "FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.EnvironmentHelper
open Fake.MSBuildHelper
open Fake.NuGetHelper
open Fake.XUnitHelper

// properties
let semanticVersion = "2.0.0"
let version = (>=>) @"(?<major>\d*)\.(?<minor>\d*)\.(?<build>\d*).*?" "${major}.${minor}.${build}.0" semanticVersion
let configuration = getBuildParamOrDefault "configuration" "Release"
let deployToNuGet = getBuildParamOrDefault "deployToNuGet" "false"
let genDir = "Gen/"
let docDir = "Doc/"
let srcDir = "Src/"
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
    xUnit (fun p ->
        { p with
            ShadowCopy = false;
            HtmlOutput = true;
            XmlOutput = true;
            OutputDir = testDir
        })
        [srcDir @@ "Kent.Boogaart.PCLMock.UnitTests/bin" @@ configuration @@ "Kent.Boogaart.PCLMock.UnitTests.dll"]
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
    let workingDir = srcDir @@ "Kent.Boogaart.PCLMock/bin" @@ configuration

    !! (workingDir @@ "Kent.Boogaart.PCLMock.*")
        |> Zip workingDir (genDir @@ "PCLMock-" + semanticVersion + "-bin.zip")
)

Target "CreateNuGetPackages" (fun _ ->
    // copy files required in the various NuGets
    !! (srcDir @@ "Kent.Boogaart.PCLMock/bin" @@ configuration @@ "Kent.Boogaart.PCLMock.*")
        |> CopyFiles (nugetDir @@ "Kent.Boogaart.PCLMock/lib/portable-win+net40+sl50+WindowsPhoneApp81+wp80+MonoAndroid10+Xamarin10+MonoTouch10")
        
    !! (srcDir @@ "Kent.Boogaart.PCLMock.CodeGeneration.T4/bin" @@ configuration @@ "Mocks.*")
        |> CopyFiles (nugetDir @@ "Kent.Boogaart.PCLMock.CodeGeneration.T4/content")
    !! (srcDir @@ "Kent.Boogaart.PCLMock.CodeGeneration.T4/bin" @@ configuration @@ "*.*")
        -- ("**/*.xml")
        |> CopyFiles (nugetDir @@ "Kent.Boogaart.PCLMock.CodeGeneration.T4/tools")

    !! (srcDir @@ "Kent.Boogaart.PCLMock.CodeGeneration.Console/bin" @@ configuration @@ "*.*")
        -- ("**/*.xml")
        |> CopyFiles (nugetDir @@ "Kent.Boogaart.PCLMock.CodeGeneration.Console/tools")

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
            -- (srcDir @@ "**/obj/**")]
    sourceFiles
        |> CopyWithSubfoldersTo (nugetDir @@ "Kent.Boogaart.PCLMock")
    sourceFiles
        |> CopyWithSubfoldersTo (nugetDir @@ "Kent.Boogaart.PCLMock.CodeGeneration")

    // create the NuGets
    NuGet (fun p ->
        {p with
            Project = "Kent.Boogaart.PCLMock"
            Version = semanticVersion
            OutputPath = nugetDir
            WorkingDir = nugetDir @@ "Kent.Boogaart.PCLMock"
            SymbolPackage = NugetSymbolPackage.Nuspec
            Publish = System.Convert.ToBoolean(deployToNuGet)
        })
        (srcDir @@ "Kent.Boogaart.PCLMock.nuspec")

    NuGet (fun p ->
        {p with
            Project = "Kent.Boogaart.PCLMock.CodeGeneration.T4"
            Version = semanticVersion
            OutputPath = nugetDir
            WorkingDir = nugetDir @@ "Kent.Boogaart.PCLMock.CodeGeneration.T4"
            SymbolPackage = NugetSymbolPackage.None
            Dependencies =
                [
                    "Kent.Boogaart.PCLMock", semanticVersion
                ]
            Publish = System.Convert.ToBoolean(deployToNuGet)
        })
        (srcDir @@ "Kent.Boogaart.PCLMock.CodeGeneration.T4.nuspec")

    NuGet (fun p ->
        {p with
            Project = "Kent.Boogaart.PCLMock.CodeGeneration.Console"
            Version = semanticVersion
            OutputPath = nugetDir
            WorkingDir = nugetDir @@ "Kent.Boogaart.PCLMock.CodeGeneration.Console"
            SymbolPackage = NugetSymbolPackage.None
            Publish = System.Convert.ToBoolean(deployToNuGet)
        })
        (srcDir @@ "Kent.Boogaart.PCLMock.CodeGeneration.Console.nuspec")
)

// build order
"Clean"
    ==> "Build"
    ==> "ExecuteUnitTests"
    ==> "CreateArchives"
    ==> "CreateNuGetPackages"

RunTargetOrDefault "CreateNuGetPackages"