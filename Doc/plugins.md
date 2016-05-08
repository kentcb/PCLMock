# Plugins

## Overview

PCLMock's code generator includes support for plugins. These are implementations of `PCLMock.CodeGeneration.IPlugin`. Plugins are able to participate in the code generation process, generating specifications for the target mock.

PCLMock itself comes with several plugins, discussed below. They are enabled by default, but you can always disable them or replace them with your own.

## The `IPlugin` Interface

Plugins must implement the `PCLMock.CodeGeneration.IPlugin` interface. If you're writing your own plugin, you can get this interface by adding the `PCLMock.CodeGeneration` NuGet package.

The `IPlugin` interface defines members that allow you to generate code that applies to all mock instances, or only loose mocks. In either case, you need to return an instance of `Microsoft.CodeAnalysis.SyntaxNode` containing the code you wish to inject into the mock.

## Built-in Plugins

PCLMock itself comes with a couple of plugins. These are enabled by default and are both intended to avoid common traps with respect to asynchronous code and mocks.

### The `Collections` Plugin

This plugin can be configured thusly:

```XML
<Plugins>
    <Plugin>PCLMock.CodeGeneration.Plugins.Collections, PCLMock.CodeGeneration</Plugin>
</Plugins>
```

Its purpose is to ensure that any method or property returning a standard collection type will return a default, empty instance of an appropriate collection rather than `null`. The supported collection types are:

* `System.Collections.Generic.IEnumerable<T>`
* `System.Collections.Generic.ICollection<T>`
* `System.Collections.Generic.IReadOnlyCollection<T>`
* `System.Collections.Generic.IList<T>`
* `System.Collections.Generic.IReadOnlyList<T>`
* `System.Collections.Generic.IDictionary<TKey, TValue>`
* `System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>`
* `System.Collections.Generic.ISet<T>`
* `System.Collections.Generic.IImmutableList<T>`
* `System.Collections.Generic.IImmutableDictionary<TKey, TValue>`
* `System.Collections.Generic.IImmutableQueue<T>`
* `System.Collections.Generic.IImmutableSet<T>`
* `System.Collections.Generic.IImmutableStack<T>`

Consider the following interface:

```C#
public interface ICustomerService
{
    IReadOnlyList<Customer> Customers
    {
        get;
    }
}
```

Without this plugin, the generated mock will return `null` when you dereference the `Customers` property. This is likely at odds with what you would expect, since framework guidelines stipulate that members returning collections should not return `null`. Thus, any consuming code that assumes it won't receive `null` will throw an exception.

### The `TaskBasedAsynchrony` Plugin

This plugin can be configured thusly:

```XML
<Plugins>
    <Plugin>PCLMock.CodeGeneration.Plugins.TaskBasedAsynchrony, PCLMock.CodeGeneration</Plugin>
</Plugins>
```

Its purpose is to ensure that any method or property that returns `Task` or `Task<T>` has a default specification such that it will return `Task<T>` rather than `null`. Consider the following interface:

```C#
public interface ISomeService
{
    Task DoSomethingAsync();
}
```

Without this plugin, the mock generated for this interface will return `null` when you call `DoSomethingAsync`. This means that any consuming code that awaits (or chains onto) such a call will crash. These problems can be difficult to track down and when you do you would normally just supplement the generated mock code with a specification:

```C#
this
    .When(x => x.DoSomethingAsync())
    .Return(Task.FromResult(false));
```

The `TaskBasedAsyncrony` plugin saves you needing to do this manually. It will generate the above specification for you. Moreover, it will work for any member returning a concrete `Task<T>` (such as `Task<int>`).

It will not generate any specifications for set-only properties or members that use custom `Task` subclasses.

### The `ObservableBasedAsynchrony` Plugin

This plugin can be configured thusly:

```XML
<Plugins>
    <Plugin>PCLMock.CodeGeneration.Plugins.ObservableBasedAsynchrony, PCLMock.CodeGeneration</Plugin>
</Plugins>
```

Its purpose is to ensure that any method or property that returns `IObservable<T>` has a default specification such that it will return a valid observable rather than `null`. Consider the following interface:

```C#
public interface ICustomerService
{
    IObservable<Customer> Customers
    {
        get;
    }

    IObservable<Unit> DeleteCustomerAsync(int id);
}
```

Without this plugin, the mock generated for this interface will return `null` when you dereference `Customers` or call `DeleteCustomerAsync`. This means that any consuming code that awaits (or chains onto) such a call will likely crash. These problems can be difficult to track down and when you do you would normally just supplement the generated mock code with specifications:

```C#
this
    .When(x => x.Customers)
    .Return(Observable.Empty<Customer>());
this
    .When(x => x.DeleteCustomerAsync(It.IsAny<int>()))
    .Return(Observable.Return(Unit.Default));
```

The `ObservableBasedAsyncrony` plugin saves you needing to do this manually. It will generate the above specification for you.

Notice how the two members have different observables returned. The `ObservableBasedAsynchrony` plugin makes inferences about the semantics of the observable from the member type. Observables returned from properties typically have collection semantics, so an empty observable is returned. Observables returned from methods typically have asychronous operation semantics, so the observable returned here contains a single, default item.

This plugin will not generate any specifications for set-only properties or members that use custom `IObservable<T>` subtypes.