![Logo](Art/Logo150x150.png "Logo")

# PCLMock

[![Build status](https://ci.appveyor.com/api/projects/status/wj9tyg3m99jogmqw?svg=true)](https://ci.appveyor.com/project/kentcb/pclmock)

## What?

**PCLMock** is a lightweight, but powerful mocking framework targeting .NET Standard 1.0 (it was originally a Portable Class Library, hence the name).

## Why?

At the time of inception, existing mocking frameworks (such as [Moq](https://github.com/Moq/moq4)) rely heavily on reflection and other mechanisms that are not available on more limited .NET runtimes, such as Mono. Writing mocks without the aid of a framework is laborious, error-prone, and results in inconsistent code. **PCLMock** aims to fill this gap.

## Where?

The easiest way to get **PCLMock** is via [NuGet](http://www.nuget.org/packages/PCLMock/):

```PowerShell
Install-Package PCLMock
```

There are also packages specific to [code generation](Doc/generating-mocks.md).

## How?

Mocks can be created automatically via [code generation](Doc/generating-mocks.md) or manually. Generally speaking, you will want to use one of the code generation packages to generate the bulk of your mock implementation. If, instead, you want to define mocks manually, read the documentation on [defining mocks](Doc/defining-mocks.md).

Test code can utilize the mocks in various ways. Here is a typical example:

```C#
[Fact]
public void some_test()
{
    var mockService = new SomeServiceMock();
	mockService
	    .When(x => x.Login(It.IsAny<string>(), "123456"))
	    .Return(true);

    var sut = new Foo(mockService);

    // some test code here

    mockService
        .Verify(x => x.Login("me", "123456"))
        .WasNotCalled();
}
```

For a detailed discussion, read the documentation on [using mocks](Doc/using-mocks.md).

## Who?

**PCLMock** is created and maintained by [Kent Boogaart](http://kent-boogaart.com). Issues and pull requests are welcome.