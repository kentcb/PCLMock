namespace Kent.Boogaart.PCLMock.Utility
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    // a collection of FilteredContinuation objects
    internal sealed class FilteredContinuationCollection : Collection<FilteredContinuation>
    {
        private readonly IList<FilteredContinuation> items;

        public FilteredContinuationCollection()
        {
            this.items = new List<FilteredContinuation>();
        }
    }
}