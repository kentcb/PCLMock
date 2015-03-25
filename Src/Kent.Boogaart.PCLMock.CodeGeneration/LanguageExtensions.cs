namespace Kent.Boogaart.PCLMock.CodeGeneration
{
    using System;

    internal static class LanguageExtensions
    {
        public static string ToSyntaxGeneratorLanguageName(this Language @this)
        {
            switch (@this)
            {
                case Language.CSharp:
                    return "C#";
                case Language.VisualBasic:
                    return "Visual Basic";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}