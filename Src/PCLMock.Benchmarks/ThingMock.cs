namespace PCLMock.Benchmarks
{
    public sealed class ThingMock : MockBase<IThing>, IThing
    {
        public ThingMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
        }

        public int IntProperty
        {
            get => this.Apply(x => x.IntProperty);
            set => this.ApplyPropertySet(x => x.IntProperty, value);
        }

        public void VoidNoArgMethod() =>
            this.Apply(x => x.VoidNoArgMethod());

        public int IntNoArgMethod() =>
            this.Apply(x => x.IntNoArgMethod());

        public int IntSingleArgMethod(float f) =>
            this.Apply(x => x.IntSingleArgMethod(f));

        public int IntMultiArgMethod(float f, string s, int i, double d) =>
            this.Apply(x => x.IntMultiArgMethod(f, s, i, d));
    }
}
