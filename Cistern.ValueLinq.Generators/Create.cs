namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Cistern.ValueLinq.Generators.TextHelper;

static class Create
{
    private const string Aggregator = "Cistern.ValueLinq.Chain.IInternal__Aggregator__NotForManualUse";
    private const string ImplementsDispose = "Cistern.ValueLinq.Chain.IInternal__ImplementsDispose__NotForManualUse";

    public static GeneratedSource Generate(INamedTypeSymbol type, bool aggregator)
    {
        var hintName =
            Types.HintName(type, aggregator ? "Aggregator" : "Create");

        var alsoAggregator =
            type
            .GetAttributes()
            .Any(attribute => attribute.AttributeClass?.ToDisplayString() == "Cistern.ValueLinq.AggregatorAttribute");

        // [Aggregator] reports the pair
        if (!aggregator && alsoAggregator)
            return new GeneratedSource(hintName, "");

        if (aggregator)
        {
            var alsoCreate =
                type
                .GetAttributes()
                .Any(attribute => attribute.AttributeClass?.ToDisplayString() == "Cistern.ValueLinq.CreateAttribute");
            if (alsoCreate)
                return GeneratedSource.Error(hintName, $"{type.Name} carries [Create] and [Aggregator]; [Aggregator] writes everything [Create] does", type);

            var declaresMarker =
                type
                .Interfaces
                .FirstOrDefault(declared => Types.MetadataName(declared) is Aggregator or ImplementsDispose);
            if (declaresMarker is not null)
                return GeneratedSource.Error(hintName, $"{type.Name} declares {declaresMarker.Name}, which [Aggregator] writes", type);
        }

        var notPartial =
            new[] { type }
            .Concat(containers(type))
            .FirstOrDefault(declared => !IsPartial(declared));
        if (notPartial is not null)
            return GeneratedSource.Error(hintName, $"{notPartial.Name} is not partial, so {type.Name}'s ICreate cannot be written beside it", notPartial);

        var declaresCreate =
            type
            .Interfaces
            .FirstOrDefault(declared => Types.MetadataName(declared.OriginalDefinition) == "Cistern.ValueLinq.Chain.ICreate`2");
        if (declaresCreate is not null)
            return GeneratedSource.Error(hintName, $"{type.Name} declares ICreate, which [Create] writes", type);

        var reserved =
            type
            .GetMembers()
            .FirstOrDefault(member =>
                !member.IsImplicitlyDeclared
                && member.Name is "Args" or "Create");
        if (reserved is not null)
            return GeneratedSource.Error(hintName, $"{type.Name} declares {reserved.Name}, which [Create] writes", reserved);

        var constructors =
            type
            .InstanceConstructors
            .Where(constructor => !constructor.IsImplicitlyDeclared)
            .ToList();

        if (constructors is [_, var second, ..])
            return GeneratedSource.Error(hintName, $"{type.Name} declares more than one constructor; [Create] builds it through its one constructor", second);

        if (constructors is not [var constructor])
            return GeneratedSource.Error(hintName, $"{type.Name} declares no constructor; [Create] builds it through its one constructor", type);

        var byReference =
            constructor
            .Parameters
            .FirstOrDefault(parameter => parameter.RefKind is RefKind.Ref or RefKind.Out);
        if (byReference is not null)
            return GeneratedSource.Error(hintName, $"{type.Name}'s constructor takes {byReference.Name} by ref or out, which a field of Args cannot be passed as", byReference);

        var source =
            new StringBuilder();

        var header =
            Generated.Header(type.ContainingNamespace.ToDisplayString());

        source.Append(header);

        var outer =
            containers(type)
            .Reverse()
            .ToList();

        var indent = "";

        foreach (var container in outer)
        {
            source.Append($"{NewLine}{indent}{Declaration(container)}{NewLine}{indent}{{");

            indent += "    ";
        }

        var self = $"{type.Name}{TypeParameters(type)}";

        var dispose =
            aggregator
                ? WrittenDispose(type)
                : null;

        source.Append($"{NewLine}{indent}{Declaration(type)}");
        source.Append($"{NewLine}{indent}    : global::Cistern.ValueLinq.Chain.ICreate<{self}, {self}.Args>");

        if (aggregator)
            source.Append($"{NewLine}{indent}    , global::{Aggregator}");

        if (dispose is not null)
            source.Append($"{NewLine}{indent}    , global::{ImplementsDispose}");

        source.Append($$"""

            {{indent}}{
            {{indent}}    public readonly struct Args
            {{indent}}    {
            """);

        foreach (var parameter in constructor.Parameters)
        {
            var fieldAttributes =
                parameter
                .GetAttributes()
                .Where(IsFieldNullability)
                .Select(Attribute);

            var attributes =
                string.Concat(fieldAttributes);

            source.Append($"{NewLine}{indent}        {attributes}public readonly {Types.Name(parameter.Type)} {FieldName(parameter)};");
        }

        if (constructor.Parameters is not [])
        {
            var declaredParameters =
                constructor
                .Parameters
                .Select(DeclareParameter);

            var parameterList =
                string.Join(", ", declaredParameters);

            source.Append($"{NewLine}{NewLine}{indent}        public Args({parameterList}){NewLine}{indent}        {{");

            foreach (var parameter in constructor.Parameters)
            {
                source.Append($"{NewLine}{indent}            this.{FieldName(parameter)} = {parameter.Name};");
            }

            source.Append($"{NewLine}{indent}        }}");
        }

        var passedArguments =
            constructor
            .Parameters
            .Select(PassArgument);

        var argumentList =
            string.Join(", ", passedArguments);

        source.Append($"{NewLine}{indent}    }}{NewLine}");
        source.Append($"{NewLine}{indent}    public static {self} Create(in Args args) => new {self}({argumentList});");

        if (aggregator && dispose is null)
            source.Append($"{NewLine}{NewLine}{indent}    public void Dispose() {{ }}");

        source.Append($"{NewLine}{indent}}}{NewLine}");

        foreach (var _ in outer)
        {
            indent = indent.Substring(4);

            source.Append($"{indent}}}{NewLine}");
        }

        (string, Location)? warning =
            dispose is not null && IsEmpty(dispose)
                ? ($"{type.Name} writes an empty Dispose, which marks it as releasing on Dispose and so runs it in place rather than from a local copy; [Aggregator] writes the empty one where none is written", dispose.Locations.FirstOrDefault() ?? Location.None)
                : null;

        var generated =
            new GeneratedSource(hintName, $"{source}", Warning: warning);

        return generated;

        static IEnumerable<INamedTypeSymbol> containers(INamedTypeSymbol nested)
        {
            for (var container = nested.ContainingType; container is not null; container = container.ContainingType)
                yield return container;
        }
    }

    // Dispose(), or IDisposable.Dispose implemented explicitly, in any part of the type
    private static IMethodSymbol? WrittenDispose(INamedTypeSymbol type)
    {
        var written =
            type
            .GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(method =>
                !method.IsImplicitlyDeclared
                && !method.IsStatic
                && method.Parameters is []
                && (method.Name == "Dispose"
                    || method.ExplicitInterfaceImplementations.Any(implemented => implemented.Name == "Dispose")));

        return written;
    }

    public static bool IsEmpty(IMethodSymbol method)
    {
        var isEmpty =
            method
            .DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .OfType<MethodDeclarationSyntax>()
            .Any(declaration => declaration is { ExpressionBody: null, Body.Statements: [] });

        return isEmpty;
    }

    public static bool IsPartial(INamedTypeSymbol type)
    {
        var isPartial =
            type
            .DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .Any(declaration => declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

        return isPartial;
    }

    // the constraints are left out: the hand-written part carries them for every part
    public static string Declaration(INamedTypeSymbol type)
    {
        var modifiers =
            type switch
            {
                { IsStatic: true } => "static ",
                { IsReadOnly: true, IsRefLikeType: true } => "readonly ref ",
                { IsReadOnly: true } => "readonly ",
                { IsRefLikeType: true } => "ref ",
                _ => "",
            };

        var kind =
            type.TypeKind == TypeKind.Struct
                ? "struct"
                : "class";

        var declaration = $"{modifiers}partial {kind} {type.Name}{TypeParameters(type)}";

        return declaration;
    }

    private static string TypeParameters(INamedTypeSymbol type)
    {
        if (type.TypeParameters is [])
            return "";

        var names =
            type
            .TypeParameters
            .Select(typeParameter => typeParameter.Name);

        var parameters = $"<{string.Join(", ", names)}>";

        return parameters;
    }

    private static string FieldName(IParameterSymbol parameter)
    {
        var name =
            parameter.Name.Substring(0, 1).ToUpperInvariant() + parameter.Name.Substring(1);

        return name;
    }

    private static string DeclareParameter(IParameterSymbol parameter)
    {
        var modifier =
            parameter.RefKind == RefKind.None
                ? ""
                : "in ";

        var defaultValue =
            parameter.HasExplicitDefaultValue
                ? $" = {DefaultValue(parameter)}"
                : "";

        var parameterAttributes =
            parameter
            .GetAttributes()
            .Select(Attribute);

        var attributes =
            string.Concat(parameterAttributes);

        var declared = $"{attributes}{modifier}{Types.Name(parameter.Type)} {parameter.Name}{defaultValue}";

        return declared;
    }

    // the field holds what the parameter was given (a default of T), so it takes the parameter's null-state annotation
    private static bool IsFieldNullability(AttributeData attribute)
    {
        var isFieldNullability =
            attribute.AttributeClass?.ToDisplayString() is
                "System.Diagnostics.CodeAnalysis.AllowNullAttribute"
                or "System.Diagnostics.CodeAnalysis.DisallowNullAttribute"
                or "System.Diagnostics.CodeAnalysis.MaybeNullAttribute"
                or "System.Diagnostics.CodeAnalysis.NotNullAttribute";

        return isFieldNullability;
    }

    private static string Attribute(AttributeData attribute)
    {
        var positional =
            attribute
            .ConstructorArguments
            .Select(argument => argument.ToCSharpString());

        var named =
            attribute
            .NamedArguments
            .Select(argument => $"{argument.Key} = {argument.Value.ToCSharpString()}");

        var arguments =
            string.Join(", ", positional.Concat(named));

        var written = $"[global::{attribute.AttributeClass!.ToDisplayString()}({arguments})] ";

        return written;
    }

    private static string DefaultValue(IParameterSymbol parameter)
    {
        if (parameter.ExplicitDefaultValue is not {} value)
            return "default";

        var literal =
            SymbolDisplay.FormatPrimitive(value, quoteStrings: true, useHexadecimalNumbers: false);

        var typed = $"({Types.Name(parameter.Type)})({literal})";

        return typed;
    }

    private static string PassArgument(IParameterSymbol parameter)
    {
        var modifier =
            parameter.RefKind == RefKind.None
                ? ""
                : "in ";

        var passed = $"{modifier}args.{FieldName(parameter)}";

        return passed;
    }
}
