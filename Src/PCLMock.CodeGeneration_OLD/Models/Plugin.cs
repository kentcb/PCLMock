namespace PCLMock.CodeGeneration.Models
{
    public sealed class Plugin
    {
        private readonly string assemblyQualifiedName;

        public Plugin(string assemblyQualifiedName)
        {
            this.assemblyQualifiedName = assemblyQualifiedName;
        }

        public string AssemblyQualifiedName => this.assemblyQualifiedName;
    }
}