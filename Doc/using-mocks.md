# Using Mocks

Once a mock has been [defined](defining-mocks.md), you can of course use it in your unit tests for anything that depends on the mocked interface (or class). For example:

```C#
var mock = new AuthenticationServiceMock();
var sut = new MyViewModel(mock);
```

Here we have a view model that requires an instance of `IAuthenticationService`.

## Returning Specific Values

To stipulate that an invocation return a specific value:

```C#
// a string property
mock.When(x => x.SomeProperty).Return("foo");

// a parameterless string method
mock.When(x => x.SomeMethod()).Return("foo");

// a string method that takes two parameters
mock.When(x => x.SomeMethod(It.IsAny<int>(), It.IsAny<string>()).Return("foo");
```

A callback can also be provided:

```C#
var valueToReturn = "foo";
mock.When(x => x.SomeMethod()).Return(() => valueToReturn + "bar");
```

If the expectation is against a method that takes parameters, the callback can accept those parameters:

```C#
mock.When(x => x.SomeMethod(It.IsAny<int>(), It.IsAny<string>()).Return((int i, string s) => i == 0 ? "zero" : "not zero");
```

## Throwing Exceptions

To stipulate that an invocation throws an exception:

```C#
mock.When(x => x.SomeProperty).Throw(new InvalidOperationException("some message"));
```

If you just want to throw an exception and don't particularly care about the exception type: 

```C#
mock.When(x => x.SomeProperty).Throw();
```

## Invoking Callbacks

To stipulate that an invocation results in a callback being invoked:

```C#
var called = false;
mock.When(x => x.SomeProperty).Do(() => called = true);
```

If the expectation is against a method that takes parameters, the callback can accept those parameters:

```C#
var calledWithZero = false;
mock.When(x => x.SomeMethod(It.IsAny<int>(), It.IsAny<string>()).Do((int i, string s) => calledWithZero = i == 0);
```

After your `Do` expectation, you can continue with a `Return` or `Throw` if relevant:

```C#
var calledWithZero = false;
mock.When(x => x.SomeMethod(It.IsAny<int>(), It.IsAny<string>())
    .Do((int i, string s) => calledWithZero = i == 0)
    .Return("foo");
```