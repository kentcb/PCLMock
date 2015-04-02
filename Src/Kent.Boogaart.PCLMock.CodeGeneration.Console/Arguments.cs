namespace Kent.Boogaart.PCLMock.CodeGeneration.Console
{
    using System.Collections.Generic;
    using PowerArgs;

    public sealed class Arguments
    {
        [ArgRequired]
        [ArgExistingFile]
        [ArgDescription("The solution file to examine for potential mocks to be generated.")]
        [ArgPosition(0)]
        public string SolutionFile
        {
            get;
            set;
        }

        [ArgRequired]
        [ArgExistingFile]
        [ArgDescription("The XML input file describing which mocks should be generated, and how they should be named.")]
        [ArgPosition(1)]
        public string ConfigurationFile
        {
            get;
            set;
        }

        [ArgRequired]
        [ArgDescription("The output file containing the generated code.")]
        [ArgPosition(2)]
        public string OutputFile
        {
            get;
            set;
        }

        [ArgDescription("Optionally force the language in which to generate mocks.")]
        public Language? Language
        {
            get;
            set;
        }
    }
}