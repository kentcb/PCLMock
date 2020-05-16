namespace PCLMock.CodeGeneration.Console
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Buildalyzer;
    using Buildalyzer.Workspaces;
    using Logging;
    using Microsoft.Extensions.Logging;
    using PCLMock.CodeGeneration.Logging;
    using PowerArgs;

    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                var arguments = Args.Parse<Arguments>(args);
                var language = arguments.Language.GetValueOrDefault(DetermineLanguageByOutputFileName(arguments.OutputFile));
                var logSink = arguments.Verbose ? (ILogSink)ConsoleLogSink.Instance : NullLogSink.Instance;
                var loggerFactory = new LoggerFactory();
                loggerFactory.AddProvider(new LogSinkLoggerProvider(logSink));
                var options = new AnalyzerManagerOptions
                {
                    LoggerFactory = loggerFactory,
                };
                var manager = new AnalyzerManager(arguments.SolutionFile, options);
                var workspace = manager.GetWorkspace();
                var compilationTasks = workspace
                    .CurrentSolution
                    .Projects
                    .Select(project => project.GetCompilationAsync());
                var compilations = await Task.WhenAll(compilationTasks);
                var syntaxNodes = XmlBasedGenerator.GenerateMocks(
                    logSink,
                    language,
                    compilations.ToImmutableList(),
                    arguments.ConfigurationFile);
                var result = syntaxNodes
                    .Aggregate(
                        new StringBuilder(),
                        (acc, next) => acc.AppendLine(next.ToFullString()).AppendLine(),
                        acc => acc.ToString());

                var log = logSink.ToString();
                File.WriteAllText(arguments.OutputFile, result);
                return 0;
            }
            catch (ArgException ex)
            {
                WriteLine(OutputType.Error, ex.Message);
                WriteLine();
                WriteLine(ArgUsage.GenerateUsageFromTemplate<Arguments>().ToNormalString());
                return -1;
            }
            catch (Exception ex)
            {
                WriteLine(OutputType.Error, "Failed to generate code: {0}", ex);
                return -1;
            }
        }

        private static void WriteLine() =>
            WriteLine(OutputType.Normal, "");

        private static void WriteLine(string format, params object[] args) =>
            WriteLine(OutputType.Normal, format, args);

        private static void WriteLine(OutputType outputType, string format, params object[] args)
        {
            var previousColor = default(ConsoleColor);

            if (outputType == OutputType.Error)
            {
                previousColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine(format, args);

            if (outputType == OutputType.Error)
            {
                Console.ForegroundColor = previousColor;
            }
        }

        private static Language DetermineLanguageByOutputFileName(string fileName)
        {
            switch (Path.GetExtension(fileName))
            {
                case ".vb":
                    return Language.VisualBasic;
                default:
                    return Language.CSharp;
            }
        }

        private enum OutputType
        {
            Normal,
            Error
        }
    }
}