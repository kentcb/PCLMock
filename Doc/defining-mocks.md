# Defining Mocks

## Overview

Typically you would use [code generation](generating-mocks.md) to create the bulk of your mock code. However, you can also define mocks manually yourself if desired. Since the code generator only supports generating mocks from interfaces, manually defining mocks can get you out of a pinch if you need to mock a class.

To define a mock manually, you first need an interface to be mocked. You can also mock `virtual` members in classes that are not `sealed`, but this is not a recommended approach (more on this below). For the purposes of the ensuing discussion, we will assume the existence of the following interface:

```C#
public interface IAuthenticationService
{
    bool IsAuthenticated
    {
        get;
    }
    
    string UserName
    {
        get;
    }
    
    ILogger Logger
    {
        get;
        set;
    }
    
    bool Authenticate(string user, string password, TimeSpan? timeout = default(TimeSpan?));
}
```

Given this interface, we can define a mock as follows:

```C#
public class AuthenticationServiceMock : MockBase<IAuthenticationService>, IAuthenticationService
{
	public AuthenticationServiceMock(MockBehavior behavior = MockBehavior.Strict)
        : base(behavior)
    {
    }

    public bool IsAuthenticated => this.Apply(x => x.IsAuthenticated);
    
    public string UserName => this.Apply(x => x.UserName);
    
    public ILogger Logger
    {
        get { return this.Apply(x => x.Logger); }
        set { this.ApplyPropertySet(x => x.Logger, value); }
    }
    
    public bool Authenticate(string user, string password, TimeSpan? timeout = default(TimeSpan?)) =>
        this.Apply(x => x.Authenticate(user, password, timeout));
}
```

As you can see, the basic approach is to implement each member to call `Apply()` or `ApplyPropertySet()` on the `MockBase<T>` base class. The `MockBase<T>` implementation takes care of applying any specifications configured by the consumer of the mock, be they invoking callbacks, throwing exceptions, or returning values.

The `MockBehavior` passed into the base constructor is used to determine whether invocations against the mock *must* have specifications configured (`MockBehavior.Strict`) or can *optionally* provide specifications (`MockBehavior.Loose`). This is covered in more detail below. 

## Advanced Mocking Techniques

### Mock Behavior

The `MockBase<T>` constructor takes a `MockBehavior` that dictates what will happen if an invocation is made against a member for which no specifications have been configured. Using `MockBehavior.Strict` will *require* that specifications be configured, otherwise an exception will be thrown when accessing any member for which a specification has not been configured. Using `MockBehavior.Loose` won't require any specifications be provided. If an invocation is made against a loose mock for which no return value has been specified, a default value is instead returned. That is, if the member returns type `T`, the invocation will return `default(T)`.

Often it is desirable for one's loose mocks to take on some default behavior that reduces the need for configuring rote specifications within your test suite. This can be achieved using this pattern:

```C#
public class SomeMock : MockBase<ISomeInterface>, ISomeInterface
{
    public SomeMock(MockBehavior behavior = MockBehavior.Strict)
        : base(behavior)
    {
        if (behavior == MockBehavior.Loose)
        {
			this.ConfigureLooseBehavior();
        }
    }
    
	private void ConfigureLooseBehavior()
	{
        // configure some default specifications here
        this.When(x => x.Foo)
            .Return("bar");
	}

    // other code here
}
```

### Mocking Classes

Although it is recommended that only interfaces be mocked, it is possible to mock `virtual` members in non-`sealed` classes. The approach is very similar to that of mocking interfaces, but the mock object cannot adopt the API of the mocked object because it can only inherit from `MockBase<T>` and not the type being mocked.

Here is an example:

```C#
public class AuthenticationService
{
    private bool isAuthenticated;
    private string userName;
    private ILogger logger;
    
    public virtual bool IsAuthenticated => this.isAuthenticated;
    
    public virtual string UserName => this.userName;
    
    // non-virtual, so can't be mocked
    public ILogger Logger
    {
        get { return this.logger; }
        set { this.logger = value; }
    }
    
    public virtual bool Authenticate(string user, string password, TimeSpan? timeout = default(TimeSpan))
    {
        if (user == "Kent" && password == "Winter is coming")
        {
            return true;
        }
        
        return false;
    }
}

public class AuthenticationServiceMock : MockBase<AuthenticationService>
{
    private readonly AuthenticationServiceSubclass mockedObject;
    
    public AuthenticationServiceMock(MockBehavior behavior = MockBehavior.Strict)
        : base(behavior)
    {
        this.mockedObject = new AuthenticationServiceSubclass(this);
    }
    
    public override AuthenticationService MockedObject => this.mockedObject;
    
    private class AuthenticationServiceSubclass : AuthenticationService
    {
        private readonly AuthenticationServiceMock owner;
        
        public AuthenticationServiceSubclass(AuthenticationServiceMock owner)
        {
            this.owner = owner;
        }
        
        public override bool IsAuthenticated => this.owner.Apply(x => x.IsAuthenticated);
        
        public override string UserName => this.owner.Apply(x => x.UserName);
        
        public override bool Authenticate(string user, string password, TimeSpan? timeout = default(TimeSpan)) =>
            this.owner.Apply(x => x.Authenticate(user, password, timeout));
    }
}
```

As you can see, it is more onerous and limited defining mocks for classes. Any members that aren't `virtual` will not be mockable. Moreover, we had to override `MockedObject` to help the mocking infrastructure obtain an instance of the type being mocked. We didn't have to do this when mocking interfaces because the default implementation of `MockBase<T>.MockedObject` automatically attempts to convert the mock object to an instance of the mocked type. Since our interface-based mock object also implemented the mocked interface, this conversion succeeds. But when mocking classes we have to explicitly help out.

Finally, note that consumers of the mock need to dereference `MockedObject` to configure expectations:

```C#
var mock = new AuthenticationServiceMock();
mock.MockedObject.When(x => x.IsAuthenticated).Return(true);
```

### Mocking `ref` and `out` Parameters

Methods with `ref` and `out` parameters can be mocked with a little bit of extra effort:

```C#
public void SomeMethodWithOutParameter(out string s)
{
    string sOut;
    s = this.GetOutParameterValue<string>(x => x.SomeMethodWithOutParameter(out sOut), parameterIndex: 0);
    this.Apply(x => x.SomeMethodWithOutParameter(out sOut));
}

public void SomeMethodWithRefParameter(ref string s)
{
    var sRef = default(string);
    s = this.GetRefParameterValue<string>(x => x.SomeMethodWithRefParameter(ref sRef), parameterIndex: 0);
    this.Apply(x => x.SomeMethodWithRefParameter(ref sRef));
}
```

Consumers can then specify what values should be assigned to the parameters as follows:

```C#
var mock = ...;
string s;
mock.When(x => x.SomeMethodWithOutParameter(out s)).AssignOutOrRefParameter(0, "value");

mock.SomeMethodWithOutParameter(out s);
Assert.Equals("value", s);
```

When calling `AssignOutOrRefParameter`, parameters must be identified by their index (`0` in the above example). Note that it is not possible to match on `out` and `ref` parameters - there is always an implicit "allow all" matcher for such parameters.


### Mocking indexer properties

Indexer properties can be mocked quite naturally:

```C#
public int this[int first, int second]
{
	get { return this.Apply(x => x[first, second]); }
	set { this.ApplyPropertySet(x => x[first, second], value); }
}
```

And consumers can use this as follows:

```C#
var mock = ...;
mock.When(x => x[2, 3]).Return(48);
mock.When(x => x[1, 13]).Return(190);

Assert.Equal(190, mock[1, 13]);
Assert.Equal(48, mock[2, 3]);
```

## Limitations

There are some things that cannot currently be mocked, or can only be mocked in a limited fashion. They are documented below.

### Set-only properties

Properties with only a `set` accessor cannot be mocked. This is due to expression tree limitations. Only properties with a `get` accessor can appear inside an expression tree, so there is no way to call `Apply` with an expression tree that resolves to the property.

The code generator ignores `set`-only properties, so you can always supplement the generated code with a `set`-only property with an implementation of your choosing.

### Events

Again, limitations in expression trees prevent a viable means of resolving events. This means that there is also no way to call `Apply` and pass in an expression that resolves to the event in question.

The code generator ignores events, so you can supplement the generated code with event implementations of your own.

It is conceivable that *some* event mocking support will be added in the future. For example, `RaiseEvent` methods might be generated on the mock. This would allow you to simulate scenarios in which the system under test should react to an event, but it won't allow you to dictate what happens when an event handler is added or removed. In addition, verification scenarios wouldn't be supported (such as verifying that an event handler is detached when a component is disposed).