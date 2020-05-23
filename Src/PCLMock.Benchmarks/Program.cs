namespace PCLMock.Benchmarks
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Diagnosers;
    using BenchmarkDotNet.Engines;
    using BenchmarkDotNet.Loggers;
    using BenchmarkDotNet.Running;

    public static class Program
    {
        static void Main()
        {
            var config = new Config();

            BenchmarkRunner.Run<Construction>(config);
            BenchmarkRunner.Run<Specification>(config);
            BenchmarkRunner.Run<Application>(config);
            BenchmarkRunner.Run<Verification>(config);
            BenchmarkRunner.Run<Invocation>(config);
        }

        private sealed class Config : ManualConfig
        {
            public Config()
            {
                this.Add(ConsoleLogger.Default);
                this.Add(DefaultColumnProviders.Instance);
                this.Add(MemoryDiagnoser.Default);
            }
        }

        [SimpleJob(id: nameof(Construction), targetCount: 3, invocationCount: 100_000_000)]
        public class Construction
        {
            [Benchmark(Baseline = true)]
            public object Baseline() =>
                new object();

            [Benchmark]
            public IThing Mock() =>
                new ThingMock();
        }

        [SimpleJob(id: nameof(Specification), targetCount: 3, invocationCount: 10_000)]
        public class Specification
        {
            private readonly ThingMock mock;
            private readonly WhenContinuation<IThing, int> whenContinuation;

            public Specification()
            {
                this.mock = new ThingMock();
                this.whenContinuation = this.mock.When(x => x.IntProperty);
            }

            [Benchmark]
            public WhenContinuation<IThing, int> When_IntProperty() =>
                this.mock.When(x => x.IntProperty);

            [Benchmark]
            public WhenContinuation<IThing> When_VoidNoArgMethod() =>
                this.mock.When(x => x.VoidNoArgMethod());

            [Benchmark]
            public WhenContinuation<IThing, int> When_IntNoArgMethod() =>
                this.mock.When(x => x.IntNoArgMethod());

            [Benchmark]
            public WhenContinuation<IThing, int> When_IntSingleArgMethod() =>
                this.mock.When(x => x.IntSingleArgMethod(It.IsAny<float>()));

            [Benchmark]
            public WhenContinuation<IThing, int> When_IntMultiArgMethod() =>
                this.mock.When(x => x.IntMultiArgMethod(It.IsAny<float>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));

            [Benchmark]
            public void Return_Value() =>
                this.whenContinuation.Return(42);

            [Benchmark]
            public void Return_Func() =>
                this.whenContinuation.Return(() => 42);

            [Benchmark]
            public void Throw() =>
                this.whenContinuation.Throw();

            [Benchmark]
            public void Throw_SpecificException() =>
                this.whenContinuation.Throw(new InvalidOperationException());
        }

        [SimpleJob(id: nameof(Application), targetCount: 3, invocationCount: 10_000)]
        public class Application
        {
            private readonly ThingMock mock;

            public Application()
            {
                this.mock = new ThingMock(MockBehavior.Loose);
            }

            [Benchmark]
            public int Apply_IntProperty() =>
                this.mock.IntProperty;

            [Benchmark]
            public void Apply_VoidNoArgMethod() =>
                this.mock.VoidNoArgMethod();

            [Benchmark]
            public int Apply_IntNoArgMethod() =>
                this.mock.IntNoArgMethod();

            [Benchmark]
            public int Apply_IntSingleArgMethod() =>
                this.mock.IntSingleArgMethod(6.9f);

            [Benchmark]
            public int Apply_IntMultiArgMethod() =>
                this.mock.IntMultiArgMethod(6.9f, "Hello", 42, 6.9);
        }

        [SimpleJob(id: nameof(Verification), targetCount: 3, invocationCount: 100_000)]
        public class Verification
        {
            private readonly ThingMock mock;
            private readonly VerifyContinuation verifyContinuation;

            public Verification()
            {
                this.mock = new ThingMock(MockBehavior.Loose);
                this.verifyContinuation = this.mock.Verify(x => x.IntProperty);
            }

            [Benchmark]
            public VerifyContinuation Verify_IntProperty() =>
                this.mock.Verify(x => x.IntProperty);

            [Benchmark]
            public VerifyContinuation Verify_VoidNoArgMethod() =>
                this.mock.Verify(x => x.VoidNoArgMethod());

            [Benchmark]
            public VerifyContinuation Verify_IntNoArgMethod() =>
                this.mock.Verify(x => x.IntNoArgMethod());

            [Benchmark]
            public VerifyContinuation Verify_IntSingleArgMethod() =>
                this.mock.Verify(x => x.IntSingleArgMethod(It.IsAny<int>()));

            [Benchmark]
            public VerifyContinuation Verify_IntMultiArgMethod() =>
                this.mock.Verify(x => x.IntMultiArgMethod(It.IsAny<float>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));

            [Benchmark]
            public VerifyContinuation VerifyPropertySet_IntProperty() =>
                this.mock.VerifyPropertySet(x => x.IntProperty);

            [Benchmark]
            public void VerifyInvocationCount() =>
                // All verification continuation methods follow the same implementation pattern, so testing one is as good as testing any other.
                this.verifyContinuation.WasNotCalled();
        }

        [SimpleJob(id: nameof(Invocation), targetCount: 3, invocationCount: 10_000)]
        public class Invocation
        {
            private readonly ThingMock looseMock;
            private readonly ThingMock strictMock;

            public Invocation()
            {
                this.looseMock = new ThingMock(MockBehavior.Loose);
                this.strictMock = new ThingMock();
                this.strictMock.When(x => x.IntProperty).Return(42);
                this.strictMock.When(x => x.VoidNoArgMethod());
                this.strictMock.When(x => x.IntNoArgMethod()).Return(42);
                this.strictMock.When(x => x.IntSingleArgMethod(It.IsAny<float>())).Return(42);
                this.strictMock.When(x => x.IntMultiArgMethod(It.IsAny<float>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>())).Return(42);
            }

            [Benchmark]
            public int IntProperty_Loose() =>
                this.looseMock.IntProperty;

            [Benchmark]
            public int IntProperty_Strict() =>
                this.strictMock.IntProperty;

            [Benchmark]
            public void VoidNoArgMethod_Loose() =>
                this.looseMock.VoidNoArgMethod();

            [Benchmark]
            public void VoidNoArgMethod_Strict() =>
                this.strictMock.VoidNoArgMethod();

            [Benchmark]
            public int IntNoArgMethod_Loose() =>
                this.looseMock.IntNoArgMethod();

            [Benchmark]
            public int IntNoArgMethod_Strict() =>
                this.strictMock.IntNoArgMethod();

            [Benchmark]
            public int IntSingleArgMethod_Loose() =>
                this.looseMock.IntSingleArgMethod(6.9f);

            [Benchmark]
            public int IntSingleArgMethod_Strict() =>
                this.strictMock.IntSingleArgMethod(6.9f);

            [Benchmark]
            public int IntMultiArgMethod_Loose() =>
                this.looseMock.IntMultiArgMethod(6.9f, "hello", 42, 6.9);

            [Benchmark]
            public int IntMultiArgMethod_Strict() =>
                this.strictMock.IntMultiArgMethod(6.9f, "hello", 42, 6.9);
        }

        // Commit everything apart from the Lazy stuff
        // Compare Lazy vs non-Lazy holistically
    }
}
