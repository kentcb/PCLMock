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

A callback can also be provided so that the return value can be determined dynamically:

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

## Matching Arguments

When specifying expectations, you can use the `It` class to define how arguments in your expectation are to be filtered. Here are some examples:

```C#
// accept any string at all
mock.When(x => x.SomeMethod(It.IsAny<string>())).Return(1);

// accept only a specific string
mock.When(x => x.SomeMethod(It.Is("foo"))).Return(1);

// same as above, only easier to read and write
mock.When(x => x.SomeMethod("foo")).Return(1);

// accept any of a set of values
mock.When(x => x.SomeMethod(It.IsIn("foo", "bar")).Return(1);

// accept values matching a regular expression
mock.When(x => x.SomeMethod(It.IsLike("[Hh]ello")).Return(1);
```

Property setters are specified slightly differently:

```C#
// accept any string at all
mock.WhenPropertySet(x => x.SomeProperty).Do(...);

// accept only the specified string
mock.WhenPropertySet(x => x.SomeProperty, It.Is("foo")).Do(...);

// same as above
mock.WhenPropertySet(x => x.SomeProperty, "foo").Do(...);

// accept any of a set of values
mock.WhenPropertySet(x => x.SomeProperty, It.IsIn("foo", "bar").Do(...);

// accept values matching a regular expression
mock.WhenPropertySet(x => x.SomeProperty, It.IsLike("[Hh]ello").Do(...);
```

There are a whole range of methods on the `It` class to help you specify constraints against arguments:

* `IsAny<T>`
* `Is<T>`
* `IsNot<T>`
* `IsNull<T>`
* `IsNotNull<T>`
* `IsOfType<T>`
* `IsNotOfType<T>`
* `IsLessThan<T>`
* `IsLessThanOrEqualTo<T>`
* `IsGreaterThan<T>`
* `IsGreaterThanOrEqualTo<T>`
* `IsBetween<T>`
* `IsNotBetween<T>`
* `IsLike`

Multiple expectations can be configured, each one for different arguments:

```C#
mock.When(x => x.SomeMethod("foo")).Return(1);
mock.When(x => x.SomeMethod("bar")).Return(2);

Assert.Equals(1, mock.SomeMethod("foo"));
Assert.Equals(2, mock.SomeMethod("bar"));
```

Note that order is important. Later expectations take precedence over earlier ones:

```C#
// these are the wrong way around to illustrate
mock.When(x => x.SomeMethod("foo")).Return(1);
mock.When(x => x.SomeMethod(It.IsAny<string>())).Return(2);

// this will fail because 2 will be returned!
Assert.Equals(1, mock.SomeMethod("foo"));
```