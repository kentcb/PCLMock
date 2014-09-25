# Defining Mocks

## Overview

To define a mock, you need an interface to be mocked. You can also mock `virtual` members in classes that are not `sealed`, but this is not a recommended approach (more on this below). For the purposes of the ensuing discussion, we will assume the existance of the following interface:

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

    public override IAuthenticationService MockedObject
    {
        get { return this; }
    }

    public bool IsAuthenticated
    {
        get { return this.Apply(x => x.IsAuthenticated); }
    }
    
    public string UserName
    {
        get { return this.Apply(x => x.UserName); }
    }
    
    public ILogger Logger
    {
        get { return this.Apply(x => x.Logger); }
        set { this.Apply(x => x.Logger, value); }
    }
    
    public bool Authenticate(string user, string password, TimeSpan? timeout = default(TimeSpan?))
    {
        return this.Apply(x => x.Authenticate(user, password, timeout), user, password, timeout);
    }
}
```

As you can see, the basic approach is to implement each member to call `Apply()` on the `MockBase<T>` base class. The `MockBase<T>` implementation takes care of applying any expectations configured by the consumer of the mock, be they invoking callbacks, throwing exceptions, or returning values.

The `MockBehavior` passed into the base constructor is used to determine whether invocations against the mock *must* have expectations configured (`MockBehavior.Strict`) or not (`MockBehavior.Loose`). This is covered in more detail below. 

The `MockedObject` property must return an instance of `T`. When mocking an interface, it is easiest to just have the mock implement the interface and return `this` from `MockedObject`. When mocking a class, it is not so straightforward (see below).

Perhaps the least intuitive aspect of mock implementations is the need to pass parameters twice when calling `Apply`:

```C#
return this.Apply(x => x.Authenticate(user, password, timeout), user, password, timeout);
```

This is simply because the values of the parameters cannot be obtained from the lambda expression. In fact, the following would work equally well:

```C#
return this.Apply(x => x.Authenticate("", "", TimeSpan.Zero), user, password, timeout);
```

It is just easier to pass the parameters through to the lambda, and then pass them again so that those parameters can also be passed through to any callbacks expectations that require them.

## Advanced Mocking Techniques

### Mocking Classes

Although it is recommended that only interfaces be mocked, it is possible to mock `virtual` members in non-`sealed` classes. The approach is very similar to that of mocking interfaces, but the mock object cannot adopt the API of the mocked object because it can only inherit from `MockBase<T>` and not the type being mocked.

Here is an example:

```C#
public class AuthenticationService
{
    private bool isAuthenticated;
    private string userName;
    private ILogger logger;
    
    public virtual bool IsAuthenticated
    {
        get { return this.isAuthenticated; }
    }
    
    public virtual string UserName
    {
        get { return this.userName; }
    }
    
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
    
    public override AuthenticationService MockedObject
    {
        get { return this.mockedObject; }
    }
    
    private class AuthenticationServiceSubclass : AuthenticationService
    {
        private readonly AuthenticationServiceMock owner;
        
        public AuthenticationServiceSubclass(AuthenticationServiceMock owner)
        {
            this.owner = owner;
        }
        
        public override bool IsAuthenticated
        {
            get { return this.owner.Apply(x => x.IsAuthenticated); }
        }
        
        public override string UserName
        {
            get { return this.owner.Apply(x => x.UserName); }
        }
        
        public override bool Authenticate(string user, string password, TimeSpan? timeout = default(TimeSpan))
        {
            return this.owner.Apply(x => x.Authenticate(user, password, timeout), user, password, timeout);
        }
    }
}
```

As you can see, it is more onerous and limited defining mocks for classes. Any members that aren't `virtual` will not be mockable. And consumers of the mock need to dereference `MockedObject` to configure expectations:

```C#
var mock = new AuthenticationServiceMock();
mock.MockedObject.When(x => x.IsAuthenticated).Return(true);
```

### Mock Behavior

The `MockBase<T>` constructor takes a `MockBehavior` that dictates what it does if an invocation is made against a member for which no expectations have been configured. Using `MockBehavior.Strict` will require that expectations be configured, or else `MockBase<T>` will throw an exception. Using `MockBehavior.Loose` won't require any expectations be configured. If an invocation is made against a loose mock for which no return value has been configured, a default value is instead returned. That is, if the member returns type `T`, the invocation will return `default(T)`.

Often it is desirable for one's loose mocks to take on some default behavior that reduces the need for configurating rote expections within your test suite. This can be achieved using this pattern:

```C#
public class SomeMock : MockBase<ISomeInterface>, ISomeInterface
{
    public SomeMock(MockBehavior behavior = MockBehavior.Strict)
        : base(behavior)
    {
        if (behavior == MockBehavior.Loose)
        {
            // configure some default expectations here
            this.When(x => x.Foo).Return("bar");
        }
    }
    
    // other code here
}
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
