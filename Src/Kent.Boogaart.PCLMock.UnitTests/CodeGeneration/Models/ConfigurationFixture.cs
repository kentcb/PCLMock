namespace Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.Models
{
    using System.Xml.Linq;
    using Kent.Boogaart.PCLMock.CodeGeneration.Models;
    using Xunit;

    public sealed class ConfigurationFixture
    {
        [Fact]
        public void from_xdocument_works_as_expected()
        {
            var inputResourceName = "Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.Models.ConfigurationFixtureResources.Input.txt";

            using (var inputStream = this.GetType().Assembly.GetManifestResourceStream(inputResourceName))
            {
                var document = XDocument.Load(inputStream);
                var configuration = Configuration.FromXDocument(document);

                Assert.Equal(2, configuration.NamespaceTransformations.Count);
                Assert.Equal("(?<name>.+)", configuration.NamespaceTransformations[0].Pattern);
                Assert.Equal("${name}.Mocks", configuration.NamespaceTransformations[0].Replacement);
                Assert.Equal("Up", configuration.NamespaceTransformations[1].Pattern);
                Assert.Equal("Down", configuration.NamespaceTransformations[1].Replacement);

                Assert.Equal(3, configuration.NameTransformations.Count);
                Assert.Equal("I(?<name>[A-Z].*)", configuration.NameTransformations[0].Pattern);
                Assert.Equal("${name}", configuration.NameTransformations[0].Replacement);
                Assert.Equal("(?<name>[A-Z].*)\\<.*\\>", configuration.NameTransformations[1].Pattern);
                Assert.Equal("${name}", configuration.NameTransformations[1].Replacement);
                Assert.Equal("(?<name>.+)", configuration.NameTransformations[2].Pattern);
                Assert.Equal("${name}Mock", configuration.NameTransformations[2].Replacement);

                Assert.Equal(2, configuration.InterfaceFilters.Count);
                Assert.Equal(FilterType.Include, configuration.InterfaceFilters[0].Type);
                Assert.Equal(".*", configuration.InterfaceFilters[0].Pattern);
                Assert.Equal(FilterType.Exclude, configuration.InterfaceFilters[1].Type);
                Assert.Equal("FooBar", configuration.InterfaceFilters[1].Pattern);
            }
        }
    }
}