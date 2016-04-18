# Generating Mocks

## Overview

**PCLMock** includes support for generating mock implementations from interfaces in your code base. Generated code can be either C# or Visual Basic. You have two code generation options:

1. A T4-based approach, available via the `PCLMock.CodeGeneration.T4` package.
2. A console-based approach, available via the `PCLMock.CodeGeneration.Console` package.

The T4-based approach integrates into Visual Studio (Xamarin Studio is not currently supported) whereas the console-based approach is a solution-level tool that you can execute how you see fit. In both cases, these tools are driven by an XML configuration file.

## Configuration File

In order to know which interfaces should have mocks generated, and how those mocks should be named, the **PCLMock** code generators rely on an XML configuration file. When you install the T4-based code generator, you get a default configuration file called *Mocks.xml*.

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

## T4-based Generation

If you're using the T4-based generation approach, you will want to add the `PCLMock.CodeGeneration.T4` package to the project in which your mocks should reside. This is probably your unit test project.

Once added, you will see two new files in the root of your project:

* *Mocks.tt*
* *Mocks.xml*

*Mocks.xml* is the configuration file discussed above. *Mocks.tt* is the text template that instigates generation of the mocks. If you add a new interface or modify your configuration file, you should right-click *Mocks.tt* and select **Run Custom Tool** in order to re-generate mocks.

To change the language of the generated code, open *Mocks.tt* and modify the `language` variable as specified in the comments.

## Console-based Generation

The `PCLMock.CodeGeneration.Console` package can be added to any project because it is actually a solution-level package. This means no particular project will "own" this package but, rather, the solution will.

Once added, you'll find an executable called *PCLMockCodeGen.exe* within your solution's *packages\PCLMock.CodeGeneration.Console.$version$\tools* directory. Execution of this tool requires these parameters:

* The path of the solution for which mocks are being generated
* The path of the XML configuration file
* The path of the output file which will contain the generated code

You can also optionally force the language of the generated code, although it is inferred from the output file's extension.

An example of executing this tool is:

```
.\PCLMockCodeGen.exe "Path\To\MySolution.sln" "Path\To\Mocks.xml" "output.cs"
```

## Supplementing Generated Code

Regardless of how you generate mock implementations, what you end up with are `partial` classes that extend `MockBase<T>`. Each takes a `MockBehavior` in its constructor and defaults it to `MockBehavior.Strict`.

If you want to configure expectations for loose mocks, you can supplement the generated `partial` class with your own logic. Specifically, there is a `partial` method called `ConfigureLooseBehavior` that the mock will invoke during construction if it is passed `MockBehavior.Loose`. Therefore, you can configure your expectations as follows:

```C#
namespace Foo.Bar.Mocks
{
    public partial class SomeServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.Name)
                .Return("Kent");
        }
    }
}
```

## Supported Members

The code generator supports everything that **PCLMock** itself supports:

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

If the code generator comes across something the **PCLMock** doesn't inherently support (such as an event), it will just ignore it. You can then supplement the generated partial class so that the mock successfully builds.

A caveat to this is that duplicate members will be ignored, even if each is supported individually. For example:

```C#
public interface IFirst
{
    void SomeMethod();
}

public interface ISecond
{
    void SomeMethod();
}

public interface IThird : IFirst, ISecond
{
}
```

Here, the generated mock for `IThird` will _not_ include a `SomeMethod` implementation. This is because doing so would require either a single implementation, or multiple with one or more implemented explicitly. Either of these two options might result in a mock that is less useful to you than you would like, so the member is simply ignored. This forces you to provide the implementation yourself via a partial class, but affords you the flexibility to choose what that implementation looks like.