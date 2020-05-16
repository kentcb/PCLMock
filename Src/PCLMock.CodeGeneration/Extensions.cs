namespace PCLMock.CodeGeneration
{
    using System;
    using System.Collections.Generic;
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

        /// <summary>
        /// Finds all instances of a type named <paramref name="fullyQualifiedMetadataName"/> in all references in
        /// <paramref name="within"/>, optionally including the containing assembly of <paramref name="within"/> itself.
        /// </summary>
        /// <remarks>
        /// This extension is required because the default behavior of <see cref="Compilation.GetTypeByMetadataName"/> is useless. See
        /// https://github.com/dotnet/roslyn/issues/3864 for more info.
        /// </remarks>
        /// <param name="within">
        /// The <see cref="Compilation"/> in which to find types.
        /// </param>
        /// <param name="fullyQualifiedMetadataName">
        /// The name of the type to find.
        /// </param>
        /// <param name="includeSelfAssembly">
        /// If <see langword="true"/>, searches the assembly in which <paramref name="within"/> is defined.
        /// </param>
        /// <returns>
        /// All types matching <paramref name="fullyQualifiedMetadataName"/>.
        /// </returns>
        public static IEnumerable<INamedTypeSymbol> GetTypesByMetadataName(this Compilation within, string fullyQualifiedMetadataName, bool includeSelfAssembly = false) =>
            within
                .References
                .Select(within.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .Select(assemblySymbol => assemblySymbol.GetTypeByMetadataName(fullyQualifiedMetadataName))
                .Concat(
                    includeSelfAssembly
                        ? new[] { within.Assembly.GetTypeByMetadataName(fullyQualifiedMetadataName) }
                        : Array.Empty<INamedTypeSymbol>())
                .Where(namedType => namedType != null);

        /// <summary>
        /// Gets a type named <paramref name="fullyQualifiedMetadataName"/> within <paramref name="within"/>, with the source assembly
        /// defined in order of preference by <paramref name="preferredAssemblyNames"/>. If a matching type name is found in an assembly
        /// not contained in <paramref name="preferredAssemblyNames"/>, it is still included but takes a lower precedence than types
        /// in assemblies that are contained in <paramref name="preferredAssemblyNames"/> (and an undefined precedence relative to other
        /// matching types that are not).
        /// </summary>
        /// <param name="within">
        /// The <see cref="Compilation"/> in which to find the type.
        /// </param>
        /// <param name="fullyQualifiedMetadataName">
        /// The fully qualified name of the type.
        /// </param>
        /// <param name="preferredAssemblyNames">
        /// The assemblies in which the type must be found, in order of preference.
        /// </param>
        /// <returns>
        /// A <see cref="INamedTypeSymbol"/>, or <see langword="null"/> if no matching type could be found.
        /// </returns>
        public static INamedTypeSymbol GetPreferredTypeByMetadataName(this Compilation within, string fullyQualifiedMetadataName, string[] preferredAssemblyNames, bool includeSelfAssembly = false)
        {
            var assemblyRanksByName = preferredAssemblyNames
                .Select((assemblyName, index) => (rank: index, assemblyName))
                .ToDictionary(kvp => kvp.assemblyName, kvp => kvp.rank);

            var result = within
                .GetTypesByMetadataName(fullyQualifiedMetadataName, includeSelfAssembly: includeSelfAssembly)
                .Select(
                    type =>
                    {
                        if (!assemblyRanksByName.TryGetValue(type.ContainingAssembly.Name, out var rank))
                        {
                            return (rank: -1, type);
                        }

                        return (rank, type);
                    })
                .OrderBy(rankedInfo => rankedInfo.rank)
                .FirstOrDefault();

            return result.type;
        }

        /// <summary>
        /// Do something for every item in <paramref name="enumerable"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The item type.
        /// </typeparam>
        /// <param name="enumerable">
        /// The enumerable.
        /// </param>
        /// <param name="action">
        /// The action to invoke for every item in <paramref name="enumerable"/>.
        /// </param>
        /// <returns>
        /// An enumerable that, when materialized, invokes <paramref name="action"/> for every item in <paramref name="enumerable"/>.
        /// </returns>
        public static IEnumerable<T> Do<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
                yield return item;
            }
        }
    }
}