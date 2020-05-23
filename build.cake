using System;

// Parameters.
var projectName = "PCLMock";
var versionMajorMinorPatch = "5.1.3";
var versionSuffix = "-alpha";
var semanticVersion = versionMajorMinorPatch + versionSuffix;
var version = versionMajorMinorPatch + ".0" + versionSuffix;
var configuration = EnvironmentVariable("CONFIGURATION") ?? "Release";
var nugetSource = "https://www.nuget.org/api/v2/package";

// To push to NuGet, run with: & {$env:NUGET_API_KEY="$KEY"; ./build.ps1}
var nugetApiKey = EnvironmentVariable("NUGET_API_KEY");

// Paths.
var srcDir = Directory("Src");
var solution = srcDir + File(projectName + ".sln");

Setup(context => Information("Building {0} version {1}.", projectName, version));

Teardown(context => Information("Build {0} finished.", version));

Task("Clean")
    .Does(() => DotNetCoreClean(solution));

Task("Pre-Build")
    .Does(
        () =>
        {
        });

Task("Build")
    .IsDependentOn("Pre-Build")
    .Does(
        () =>
        {
            var msBuildSettings = new DotNetCoreMSBuildSettings();
            msBuildSettings.Properties["Version"] = new string[]
            {
                version
            };

            var settings = new DotNetCoreBuildSettings
            {
                Configuration = configuration,
                MSBuildSettings = msBuildSettings,
            };

            DotNetCoreBuild(solution, settings);
        });

Task("Test")
    .IsDependentOn("Build")
    .Does(
        () =>
        {
            var testProject = srcDir + Directory("PCLMock.UnitTests") + File("PCLMock.UnitTests.csproj");
            DotNetCoreTest(testProject);
        });

Task("Push")
    .IsDependentOn("Test")
    .WithCriteria(nugetApiKey != null)
    .Does(
        () =>
        {
            var settings = new DotNetCoreNuGetPushSettings
            {
                ApiKey = nugetApiKey,
                Source = nugetSource,
            };

            var nuGetPackages = new []
            {
                srcDir + Directory("PCLMock/bin") + Directory(configuration) + File("PCLMock." + semanticVersion + ".nupkg"),
                srcDir + Directory("PCLMock.CodeGeneration/bin") + Directory(configuration) + File("PCLMock.CodeGeneration." + semanticVersion + ".nupkg"),
                srcDir + Directory("PCLMock.CodeGeneration.Console/bin") + Directory(configuration) + File("PCLMock.CodeGeneration.Console." + semanticVersion + ".nupkg"),
                srcDir + Directory("PCLMock.CodeGeneration.SourceGenerator/bin") + Directory(configuration) + File("PCLMock.CodeGeneration.SourceGenerator." + semanticVersion + ".nupkg"),
            };

            foreach (var nuGetPackage in nuGetPackages)
            {
                DotNetCoreNuGetPush(nuGetPackage, settings);
            }
        });

Task("Default")
    .IsDependentOn("Push");

RunTarget(Argument("target", "Default"));