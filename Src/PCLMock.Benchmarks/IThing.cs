namespace PCLMock.Benchmarks
{
    public interface IThing
    {
        int IntProperty
        {
            get;
            set;
        }

        void VoidNoArgMethod();

        int IntNoArgMethod();

        int IntSingleArgMethod(float f);

        int IntMultiArgMethod(float f, string s, int i, double d);
    }
}
