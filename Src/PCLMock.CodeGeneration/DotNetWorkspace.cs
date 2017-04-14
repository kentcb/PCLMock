namespace PCLMock.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using Microsoft.DotNet.ProjectModel;

    // adapted from https://github.com/OmniSharp/omnisharp-roslyn/blob/a4dced28f5f3b38d806134e89a6896c9d8075997/src/OmniSharp.DotNet/DotNetWorkspace.cs
    public sealed class DotNetWorkspace : Workspace
    {
        public DotNetWorkspace(string initialPath)
            : base(ProjectReaderSettings.ReadFromEnvironment(), true)
        {
        }

        public IReadOnlyList<ProjectContext> GetProjectContexts(string projectPath) =>
            (IReadOnlyList<ProjectContext>)GetProjectContextCollection(projectPath)?.ProjectContexts.AsReadOnly() ?? Array.Empty<ProjectContext>();

        protected override IEnumerable<ProjectContext> BuildProjectContexts(Project project)
        {
            foreach (var framework in project.GetTargetFrameworks())
            {
                yield return CreateBaseProjectBuilder(project)
                    .AsDesignTime()
                    .WithTargetFramework(framework.FrameworkName)
                    .Build();
            }
        }
    }
}