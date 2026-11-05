namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;
using static Cistern.ValueLinq.Generators.TextHelper;

// a hand-written pull enumerator's interfaces, IPullEnumerator over its Current's type and, where it has a
// Create(in TNode node), INodeEnumerator over that node with the explicit Create forwarding to its own; and the members
// that are the same for every enumerator it does not write: a rebuild that changes nothing (its own node passed back,
// or for an enumerator with no node a throw, as nothing rebuilds it), an empty Dispose, and the members every
// enumerator is given (PullRequests)
static class Puller
{
    private const string Attribute = "Cistern.ValueLinq.PullerAttribute";
    private const string PullEnumerator = "Cistern.ValueLinq.IPullEnumerator`1";
    private const string NodeEnumerator = "Cistern.ValueLinq.INodeEnumerator`2";

    private static readonly SymbolDisplayFormat Format = SymbolDisplayFormat.FullyQualifiedFormat;

    public static GeneratedSource Generate(INamedTypeSymbol enumerator, Compilation compilation)
    {
        var hintName =
            Types.HintName(enumerator, "Puller");

        var reserved =
            enumerator
            .TypeParameters
            .FirstOrDefault(typeParameter =>
                typeParameter.Name.StartsWith("TRequest")
                || PullRequests.MethodTypeParameters.Contains(typeParameter.Name));
        if (reserved is not null)
            return GeneratedSource.Error(hintName, $"{enumerator.Name}'s type parameter {reserved.Name} is a name the generated members use", enumerator);

        var declared =
            enumerator
            .Interfaces
            .FirstOrDefault(implemented => Types.MetadataName(implemented.OriginalDefinition) is PullEnumerator or NodeEnumerator);
        if (declared is not null)
            return GeneratedSource.Error(hintName, $"{enumerator.Name} declares {declared.Name}, which [Puller] declares from its Current and its Create", enumerator);

        // the element is Current's type
        var currents =
            enumerator
            .GetMembers("Current")
            .OfType<IPropertySymbol>()
            .Where(property => !property.IsStatic)
            .ToList();
        if (currents is not [var current])
            return GeneratedSource.Error(hintName, $"{enumerator.Name} does not declare exactly one Current, whose type [Puller] takes as its element", enumerator);

        // the node is Create's, absent for an enumerator with no node
        var creates =
            Creates(enumerator);
        if (creates is [_, var second, ..])
            return GeneratedSource.Error(hintName, $"{enumerator.Name} declares more than one Create(in TNode node); [Puller] takes the node from its one Create", second);
        if (creates is [{ DeclaredAccessibility: Accessibility.Public } publicCreate])
            return GeneratedSource.Error(hintName, $"{enumerator.Name}.Create is public; it is the enumerator's own, and INodeEnumerator is implemented by the generated explicit forwarder", publicCreate);

        var nodeType =
            creates is [var create]
                ? create.Parameters[0].Type.ToDisplayString(Format)
                : null;

        var containers =
            Containers(enumerator);

        var notPartial =
            containers
            .Prepend(enumerator)
            .FirstOrDefault(type => !Create.IsPartial(type));
        if (notPartial is not null)
            return GeneratedSource.Error(hintName, $"{notPartial.Name} is not partial, so {enumerator.Name}'s pull members cannot be written beside it", notPartial);

        var element =
            Types.Name(current.Type);

        var self =
            enumerator.ToDisplayString(Format);

        var members =
            enumerator
            .GetMembers()
            .ToList();

        bool writes(string name) => members.Any(member => member.Name.EndsWith(name));

        var body =
            new StringBuilder();

        if (!writes("RebuildChanges"))
        {
            body.Append($$"""

                        static bool global::Cistern.ValueLinq.IPullEnumerator<{{element}}>.RebuildChanges => false;

                """);
        }

        if (!writes("PreferRebuilt"))
        {
            body.Append($$"""

                        static bool global::Cistern.ValueLinq.IPullEnumerator<{{element}}>.PreferRebuilt => false;

                """);
        }

        if (nodeType is not null)
        {
            body.Append($$"""

                        static {{self}} global::Cistern.ValueLinq.INodeEnumerator<{{nodeType}}, {{self}}>.Create(in {{nodeType}} node) =>
                            Create(in node);

                """);
        }

        if (!writes("Rebuild"))
        {
            var rebuild =
                nodeType is null
                    ? "throw new global::System.InvalidOperationException(\"This enumerator has no node, so is never rebuilt\")"
                    : $"global::Cistern.ValueLinq.Chain.ChainBuilder.Identity<{element}, {nodeType}, {self}, TOwnNode, TBuilt, TBuilder>(in node, in builder)";

            body.Append($$"""

                        static TBuilt global::Cistern.ValueLinq.IPullEnumerator<{{element}}>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder) =>
                            {{rebuild}};

                """);
        }

        (string, Location)? warning = null;

        var dispose =
            members
            .OfType<IMethodSymbol>()
            .FirstOrDefault(method =>
                !method.IsStatic
                && method.Parameters is []
                && method.Name.EndsWith("Dispose"));
        if (dispose is null)
        {
            body.Append("""

                        public void Dispose() { }

                """);
        }
        else if (Create.IsEmpty(dispose))
        {
            warning = ($"{enumerator.Name} writes an empty Dispose, which [Puller] writes where none is written", dispose.Locations.FirstOrDefault() ?? Location.None);
        }

        var failure =
            PullRequests.Emit(body, enumerator, element);

        var source =
            new StringBuilder();

        source.Append(Generated.Header(enumerator.ContainingNamespace.ToDisplayString()));

        var indent = "";

        foreach (var container in containers.AsEnumerable().Reverse())
        {
            source.Append($"{NewLine}{indent}{Create.Declaration(container)}{NewLine}{indent}{{");
            indent += "    ";
        }

        var interfaces =
            nodeType is null
                ? $"{NewLine}{indent}    : global::Cistern.ValueLinq.IPullEnumerator<{element}>"
                : $"{NewLine}{indent}    : global::Cistern.ValueLinq.IPullEnumerator<{element}>{NewLine}{indent}    , global::Cistern.ValueLinq.INodeEnumerator<{nodeType}, {self}>";

        source.Append($"{NewLine}{indent}{Create.Declaration(enumerator)}{interfaces}{NewLine}{indent}{{");
        source.Append(body);
        source.Append($"{indent}}}{NewLine}");

        foreach (var _ in containers)
        {
            indent = indent.Substring(4);
            source.Append($"{indent}}}{NewLine}");
        }

        (string, Location)? located =
            failure is null
                ? null
                : (failure, enumerator.Locations.FirstOrDefault() ?? Location.None);

        var generated =
            new GeneratedSource(hintName, $"{source}", located, warning);

        return generated;
    }

    // the node a [Puller] enumerator enumerates, as its INodeEnumerator will name it, in the type's own arguments (a
    // constructed type's Create is read with them substituted); null for any other type, or one with no node
    public static ITypeSymbol? Node(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named
            || !named.OriginalDefinition.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == Attribute))
        {
            return null;
        }

        var node =
            Creates(named) is [var create]
                ? create.Parameters[0].Type
                : null;

        return node;
    }

    // a static Create(in TNode node) returning the enumerator
    private static System.Collections.Generic.List<IMethodSymbol> Creates(INamedTypeSymbol enumerator)
    {
        var creates =
            enumerator
            .GetMembers("Create")
            .OfType<IMethodSymbol>()
            .Where(method =>
                method is { IsStatic: true, MethodKind: MethodKind.Ordinary, Parameters: [{ RefKind: RefKind.In }] }
                && SymbolEqualityComparer.Default.Equals(method.ReturnType, enumerator))
            .ToList();

        return creates;
    }

    private static System.Collections.Generic.List<INamedTypeSymbol> Containers(INamedTypeSymbol type)
    {
        var containers =
            new System.Collections.Generic.List<INamedTypeSymbol>();

        for (var outer = type.ContainingType; outer is not null; outer = outer.ContainingType)
            containers.Add(outer);

        return containers;
    }
}
