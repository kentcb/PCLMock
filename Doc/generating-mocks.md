# Generating Mocks

## Overview

**PCLMock** includes support for generating mock implementations from interfaces in your code base. The core of this code generation logic is contained in the `PCLMock.CodeGeneration` project, but you will typically use that library via a code generator "front-end". There are two code generator front-ends that ship with PCLMock:

1. A [C# Source Generator](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/) (currently in preview, intended to ship with C# 9).
2. A [.NET Core tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools), available via the `PCLMock.CodeGeneration.Console` package.

In both cases, these tools are driven by an XML configuration file.

## XML Configuration File

In order to know which interfaces should have mocks generated, and how those mocks should be named, the **PCLMock** code generators rely on an XML configuration file.

### Specifying Namespace Names

Generated mocks need not be in the same namespace as the interface from which they're generated. Your configuration can provide any number of transformations to apply to the namespace name. By default, a single transformation is included in *Mocks.xml*:

```XML
<NamespaceTransformations>
    <Transformation>
        <Pattern><![CDATA[(?<name>.+)]]></Pattern>
        <Replacement>${name}.Mocks</Replacement>
    </Transformation>
</NamespaceTransformations>
```

This transformation moves the mock into a `.Mocks` sub-namespace.

Each transformation consists of a pattern to match, and a replacement. The pattern is a regular expression. As in the example above, the pattern can include groups and the replacement text can refer to those groups.

If multiple transformations are provided, they are executed in order. The input to each transformation is the output from the previous transformation, and the first transformation receives the original namespace as its input.

### Specifying Names

The names of generated mocks can also be transformed in a similar manner to namespaces. By default, *Mocks.xml* includes these name transformations:

```XML
<NameTransformations>
    <Transformation>
        <Pattern><![CDATA[I(?<name>[A-Z].*)]]></Pattern>
        <Replacement>${name}</Replacement>
    </Transformation>
    <Transformation>
        <Pattern><![CDATA[(?<name>.+)]]></Pattern>
        <Replacement>${name}Mock</Replacement>
    </Transformation>
</NameTransformations>
```

The first transformation removes any "I" from the front of the name, assuming that "I" is followed by another capital letter. That means "IFoo" will become "Foo", but "Interruptible" will remain as is.

The second transformation appends "Mock" onto the name. Thus, an interface named "ISomeService" will result in a corresponding mock class named "SomeServiceMock".

### Specifying Interfaces

The configuration file also enables you to select which interfaces have corresponding mocks generated for them. You do this using filters. By default, *Mocks.xml* includes a single filter:

```XML
<Interfaces>
    <Include>
        <Pattern>.*</Pattern>
    </Include>
</Interfaces>
```

This filter just specifies that all interfaces will have corresponding mocks generated. If this is too inclusive, you can adjust the filters.

Each filter must be either an `Include` or `Exclude` element. In either case, the pattern is a regular expression. Filters are executed in the order they appear, so a latter filter can override the result of a former filter. The `string` passed into the filters is the assembly-qualified name of the interface. For example, "Foo.Bar.IBaz, MyAssembly". This makes it simple to include all interfaces in a specific assembly:

```XML
<Interfaces>
    <Include>
        <Pattern>.*, MyAssembly</Pattern>
    </Include>
</Interfaces>
```

A more complicated filter arrangement might look like this:

```XML
<Interfaces>
    <Include>
        <Pattern>.*, MyAssembly</Pattern>
    </Include>
    <Include>
        <Pattern>.*, MyOtherAssembly</Pattern>
    </Include>
    <Exclude>
        <Pattern>.*Ignore.*</Pattern>
    </Exclude>
</Interfaces>
```

In this example, all interfaces in two different assemblies will have mocks generated for them unless the word "Ignore" appears anywhere within their assembly-qualified name.

### Specifying Plugins

You can read all about plugins [here](plugins.md). To configure plugins, you specify their fully-qualified names in the `Plugins` element of the configuration file:

```XML
<Plugins>
    <Plugin>Your.Plugin.Class.Name, Your.Plugin.Assembly</Plugin>
</Plugins>
```

You can include any number of plugins and they will be executed in the order you specify. This is important if more than one plugin might produce specifications for the same member. You should identify which plugin's specifications should take precedence and list it after the other plugins.

## Generator Front-ends

### Source Generator

To use the source generator front-end, follow these steps:

1. Use [.NET 5 preview](https://dotnet.microsoft.com/download/dotnet/5.0) and C# 9 preview.
2. Include all relevant PCLMock projects in your solution: `PCLMock`, `PCLMock.CodeGeneration`, and `PCLMock.CodeGeneration.SourceGenerator`. NOTE: this step is only required while source generators are in early preview because the .NET team have not yet provided a mechanism to consume source generators from NuGet packages. Longer term, you won't have to bundle the code.
3. Add a configuration file to your project called `PCLMock.xml` and include it as an additional file:
    ```xml
    <ItemGroup>
      <AdditionalFiles Include="PCLMock.xml" />
    </ItemGroup>
    ```
4. Include the source generator (this step will simplify once source generators can be consumed from a NuGet):
    ```xml
    <ItemGroup>
      <Analyzer Include="$(OutDir)..\..\..\..\PCLMock.CodeGeneration.    SourceGenerator\bin\$(Configuration)\netstandard2.0\PCLMock.CodeGeneration.    SourceGenerator.dll" />
    </ItemGroup>
    ```

One limitation of using source generators (besides the fact that they're in preview) is that source generators can only augment code in the compiling project. This means that generated mocks _must_ reside in the same project as the code being mocked, rather than being incorporated into a separate project (usually a unit tests project). One mitigation would be to add a `Condition` to the `Analyzer` such that it is only included for certain builds where mocks are required, but not for builds intended for deployment.

### .NET Core tool

To install the PCLMock .NET Core tool globally:

```
dotnet tool install -g pclmock
```

You can then execute it with:

```
pclmock
```

See the console output for help, but an example of executing this tool is:

```
pclmock "Path\To\MySolution.sln" "Path\To\PCLMock.xml" "output.cs" -Verbose
```

## Supplementing Generated Code

Regardless of how you generate mock implementations, what you end up with are `partial` classes that extend `MockBase<T>`. Each takes a `MockBehavior` in its constructor and defaults it to `MockBehavior.Strict`.

You can configure expectations that apply to all mocks (regardless of behavior) by supplementing the generated `partial` class with a `partial` method called `ConfigureBehavior`. If you want to configure expectations that only apply to loose mocks, there is a corresponding `partial` method called `ConfigureLooseBehavior`. Therefore, you can configure your expectations as follows:

```C#
namespace Foo.Bar.Mocks
{
    public partial class SomeServiceMock
    {
        partial void ConfigureBehavior()
        {
            // these specifications apply to all instances of the mock, regardless of behavior
            this
                .When(x => x.Name)
                .Return("Kent");
        }

        partial void ConfigureLooseBehavior()
        {
            // these specifications apply only to loose instances of the mock
            this
                .When(x => x.Age)
                .Return(36);
        }
    }
}
```

## Supported Members

The code generator (regardless of which front-end is used) supports everything that **PCLMock** itself supports:

* `get`-only properties
* `set`-only properties
* `get`/`set` properties
* indexer properties (with any number of arguments)
* `void` methods
* non-`void` methods
* `ref`/`out` arguments to methods
* default argument values to methods
* generic interfaces (including type constraints)
* generic methods (including type constraints)

If the code generator comes across something the **PCLMock** [doesn't inherently support](defining-mocks.md#limitations), it will just ignore it. You can then supplement the generated partial class so that the mock successfully builds.