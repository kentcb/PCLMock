namespace Kent.Boogaart.PCLMock
{
    internal interface IArgumentFilter
    {
        bool Matches(object argument);
    }
}