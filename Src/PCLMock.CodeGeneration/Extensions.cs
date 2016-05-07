namespace PCLMock.CodeGeneration
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    public static class Extensions
    {
        /// <summary>
        /// Gets a unique name that can be used within the scope of the specified symbol.
        /// </summary>
        /// <param name="within">
        /// The symbol within which the name must be unique.
        /// </param>
        /// <param name="proposed">
        /// A proposed (default) name.
        /// </param>
        /// <returns>
        /// A unique name.
        /// </returns>
        public static string GetUniqueName(this IPropertySymbol within, string proposed = "x")
        {
            while (within.Parameters.Any(x => x.Name == proposed))
            {
                proposed = "_" + proposed;
            }

            return proposed;
        }

        /// <summary>
        /// Gets a unique name that can be used within the scope of the specified symbol.
        /// </summary>
        /// <param name="within">
        /// The symbol within which the name must be unique.
        /// </param>
        /// <param name="proposed">
        /// A proposed (default) name.
        /// </param>
        /// <returns>
        /// A unique name.
        /// </returns>
        public static string GetUniqueName(this IMethodSymbol within, string proposed = "x")
        {
            while (within.Parameters.Any(x => x.Name == proposed))
            {
                proposed = "_" + proposed;
            }

            return proposed;
        }

        /// <summary>
        /// Gets a unique name that can be used within the scope of the specified symbol.
        /// </summary>
        /// <param name="within">
        /// The symbol within which the name must be unique.
        /// </param>
        /// <param name="proposed">
        /// A proposed (default) name.
        /// </param>
        /// <returns>
        /// A unique name.
        /// </returns>
        public static string GetUniqueName(this ISymbol within, string proposed = "x")
        {
            var propertySymbol = within as IPropertySymbol;

            if (propertySymbol != null)
            {
                return propertySymbol.GetUniqueName(proposed);
            }

            var methodSymbol = within as IMethodSymbol;

            if (methodSymbol != null)
            {
                return methodSymbol.GetUniqueName(proposed);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a unique name for a parameter within the scope of the specified symbol.
        /// </summary>
        /// <param name="within">
        /// The symbol within which the name must be unique.
        /// </param>
        /// <param name="parameterSymbol">
        /// The parameter symbol.
        /// </param>
        /// <returns>
        /// A unique name for the parameter.
        /// </returns>
        public static string GetNameForParameter(this IMethodSymbol within, IParameterSymbol parameterSymbol)
        {
            switch (parameterSymbol.RefKind)
            {
                case RefKind.None:
                    return parameterSymbol.Name;
                case RefKind.Ref:
                    return within.GetUniqueName(parameterSymbol.Name);
                case RefKind.Out:
                    return within.GetUniqueName(parameterSymbol.Name);
                default:
                    throw new NotSupportedException("Unknown parameter ref kind: " + parameterSymbol.RefKind);
            }
        }
    }
}