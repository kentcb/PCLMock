﻿namespace PCLMock.CodeGeneration
{
    using System;
    using Microsoft.CodeAnalysis;

    public static class LanguageExtensions
    {
        public static string ToSyntaxGeneratorLanguageName(this Language @this)
        {
            switch (@this)
            {
                case Language.CSharp:
                    return LanguageNames.CSharp;
                case Language.VisualBasic:
                    return LanguageNames.VisualBasic;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}