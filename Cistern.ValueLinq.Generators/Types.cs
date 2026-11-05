namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using static Cistern.ValueLinq.Generators.TextHelper;

// how a type is written into generated source, with some of its type parameters renamed
static class Types
{
    private static readonly SymbolDisplayFormat Format =
        SymbolDisplayFormat
        .FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static readonly IReadOnlyDictionary<string, string> NoRenames =
        new Dictionary<string, string>();

    public static string Name(ITypeSymbol type) => type.ToDisplayString(Format);

    public static string Name(ITypeSymbol type, IReadOnlyDictionary<string, string> renames)
    {
        if (renames.Count == 0
            || type.SpecialType != SpecialType.None)
        {
            return Name(type);
        }

        var nullable =
            type.NullableAnnotation == NullableAnnotation.Annotated
            && !type.IsValueType
                ? "?"
                : "";

        switch (type)
        {
            case ITypeParameterSymbol parameter:
                var parameterName =
                    Rename(parameter.Name, renames);

                var renamedParameter = $"{parameterName}{nullable}";

                return renamedParameter;

            case IArrayTypeSymbol array:
                var elementName =
                    Name(array.ElementType, renames);

                int commaCount = array.Rank - 1;

                var commas =
                    new string(',', commaCount);

                var arrayName = $"{elementName}[{commas}]{nullable}";

                return arrayName;

            case INamedTypeSymbol tuple when tuple.IsTupleType:
                var elementNames =
                    tuple
                    .TupleElements
                    .Select(nameElement);

                var joinedElements =
                    string.Join(", ", elementNames);

                var tupleName = $"({joinedElements}){nullable}";

                return tupleName;

            case INamedTypeSymbol named:
                var container = nameContainer(named);

                var arguments = nameArguments(named);

                var namedName = $"{container}{named.Name}{arguments}{nullable}";

                return namedName;

            default:
                return Name(type);
        }

        string nameElement(IFieldSymbol element)
        {
            var elementTypeName =
                Name(element.Type, renames);

            var label =
                element.IsExplicitlyNamedTupleElement
                    ? $" {element.Name}"
                    : "";

            var written = $"{elementTypeName}{label}";

            return written;
        }

        string nameContainer(INamedTypeSymbol namedType)
        {
            var containerName =
                namedType switch
                {
                    { ContainingType: {} outer } => $"{Name(outer, renames)}.",
                    { ContainingNamespace.IsGlobalNamespace: true } => "global::",
                    _ => $"global::{namedType.ContainingNamespace.ToDisplayString()}.",
                };

            return containerName;
        }

        string nameArguments(INamedTypeSymbol namedType)
        {
            if (namedType.TypeArguments is [])
                return "";

            var argumentNames =
                namedType
                .TypeArguments
                .Select(argument => Name(argument, renames));

            var joinedArguments =
                string.Join(", ", argumentNames);

            var written = $"<{joinedArguments}>";

            return written;
        }
    }

    public static string Rename(string name, IReadOnlyDictionary<string, string> renames)
    {
        if (renames.TryGetValue(name, out var renamed))
            return renamed;

        return name;
    }

    public static string Constraints(IEnumerable<ITypeParameterSymbol> parameters, IReadOnlyDictionary<string, string> renames)
    {
        var lines =
            parameters
            .Select(parameter => Constraint(parameter, renames))
            .Where(constraint => constraint.Length > 0)
            .Select(constraint => $"{NewLine}        {constraint}");

        var constraints = string.Concat(lines);

        return constraints;
    }

    private static string Constraint(ITypeParameterSymbol parameter, IReadOnlyDictionary<string, string> renames)
    {
        var parts = new List<string>();

        string? maybePrimaryConstraint =
            parameter switch
            {
                { HasReferenceTypeConstraint: true, ReferenceTypeConstraintNullableAnnotation: NullableAnnotation.Annotated } => "class?",
                { HasReferenceTypeConstraint: true } => "class",
                { HasUnmanagedTypeConstraint: true } => "unmanaged",
                { HasValueTypeConstraint: true } => "struct",
                { HasNotNullConstraint: true } => "notnull",
                _ => null
            };
        if (maybePrimaryConstraint is {} primaryConstraint)
            parts.Add(primaryConstraint);

        var constraintTypes =
            parameter
            .ConstraintTypes
            .Select(type => Name(type, renames));

        parts.AddRange(constraintTypes);

        if (parameter.HasConstructorConstraint)
            parts.Add("new()");

        if (parameter.AllowsRefLikeType)
            parts.Add("allows ref struct");

        var parameterName =
            Rename(parameter.Name, renames);

        var joinedParts =
            string.Join(", ", parts);

        var clause =
            parts is []
                ? ""
                : $"where {parameterName} : {joinedParts}";

        return clause;
    }

    // the type, or the first of its interfaces (or of a type parameter's constraints'), whose definition is the one named
    public static INamedTypeSymbol? Find(ITypeSymbol type, string metadataName)
    {
        var candidates =
            type switch
            {
                ITypeParameterSymbol parameter => parameter.ConstraintTypes.SelectMany(withInterfaces),
                _ => withInterfaces(type),
            };

        var found =
            candidates
            .OfType<INamedTypeSymbol>()
            .FirstOrDefault(candidate => MetadataName(candidate.OriginalDefinition) == metadataName);

        return found;

        static IEnumerable<ITypeSymbol> withInterfaces(ITypeSymbol symbol)
        {
            var interfaces =
                new[] { symbol }.Concat(symbol.AllInterfaces);

            return interfaces;
        }
    }

    public static string MetadataName(INamedTypeSymbol definition)
    {
        var namespaceName =
            definition
            .ContainingNamespace
            .ToDisplayString();

        var name = $"{namespaceName}.{definition.MetadataName}";

        return name;
    }

    public static AttributeData? Propagate(IFieldSymbol field)
    {
        var propagate =
            field
            .GetAttributes()
            .FirstOrDefault(attribute => MetadataName(attribute.AttributeClass!) == "Cistern.ValueLinq.PropagateAttribute");

        return propagate;
    }

    // [Propagate(ByRef = true)]: the filler and the chain builder hold the field by ref readonly, where the node holds it
    public static bool IsPropagatedByRef(IFieldSymbol field)
    {
        var byRef =
            Propagate(field) is {} propagate
            && propagate.NamedArguments.Any(argument => argument is { Key: "ByRef", Value.Value: true });

        return byRef;
    }

    public static IEnumerable<IFieldSymbol> InstanceFields(INamedTypeSymbol type)
    {
        var fields =
            type
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field =>
                !field.IsStatic
                && !field.IsConst
                && !field.IsImplicitlyDeclared);

        return fields;
    }

    public static string HintName(INamedTypeSymbol type, string part)
    {
        var name =
            type
            .MetadataName
            .Replace('`', '_');

        for (var outer = type.ContainingType; outer is not null; outer = outer.ContainingType)
        {
            var outerName =
                outer
                .MetadataName
                .Replace('`', '_');

            name = $"{outerName}.{name}";
        }

        var namespaceName =
            type
            .ContainingNamespace
            .ToDisplayString();

        var hintName = $"{namespaceName}.{name}.{part}.g.cs";

        return hintName;
    }
}
