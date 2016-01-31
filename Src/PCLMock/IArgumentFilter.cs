namespace PCLMock
{
    internal interface IArgumentFilter
    {
        bool Matches(object argument);
    }
}