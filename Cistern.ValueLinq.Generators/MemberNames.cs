namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

// every implementation of the chain interfaces names its members' type parameters as the interfaces declare them, and as
// the generated members do, so the generated code never collides with a type's own and every operator reads the same
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MemberNames : DiagnosticAnalyzer
{
    // a chain builder's own parameters are TBuilt and TBuilder by design, so the reserved names are not held against it
    private static readonly (string Interface, string Member, string[] Names, bool OwnNamesReserved)[] Required =
    [
        ("Cistern.ValueLinq.INode", "TryPushOptimization", ["TRequest", "TRequestResult"], true),
        ("Cistern.ValueLinq.INode`1", "CreateViaPush", ["TPushResult", "TPushEnumerator", "TPushFiller"], true),
        ("Cistern.ValueLinq.IPushEnumerator`1", "GetResult", ["TPushResult"], true),
        ("Cistern.ValueLinq.IPullEnumerator`1", "Rebuild", ["TOwnNode", "TBuilt", "TBuilder"], true),
        ("Cistern.ValueLinq.IPullEnumerator`1", "TryPullOptimization", ["TRequest", "TOwnNode", "TBuilt", "TBuilder"], true),
        ("Cistern.ValueLinq.Chain.IChainBuilder`2", "Build", ["TNode", "TEnumerator"], false),
    ];

    // the names those members use, and so the names a node's, forwarder's or enumerator's own parameters never take;
    // TNode and TEnumerator are a node's and its enumerator's conventional names (ValueEnumerable's), so a chain
    // builder's Build takes them only on a generated builder, where the generator checks the node's own
    private static readonly string[] ReservedPrefixes = ["TPush", "TRequest"];
    private static readonly string[] ReservedNames = ["TOwnNode", "TBuilt", "TBuilder"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GeneratedSource.Shape];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
    }

    private static void Analyze(SymbolAnalysisContext context)
    {
        var type =
            (INamedTypeSymbol)context.Symbol;

        var implemented = false;

        foreach (var (interfaceName, member, names, ownNamesReserved) in Required)
        {
            var found =
                type
                .AllInterfaces
                .FirstOrDefault(candidate => Types.MetadataName(candidate.OriginalDefinition) == interfaceName);
            if (found is null)
                continue;

            implemented |= ownNamesReserved;

            var declared =
                found
                .GetMembers(member)
                .OfType<IMethodSymbol>();

            foreach (var method in declared)
            {
                if (type.FindImplementationForInterfaceMember(method) is not IMethodSymbol implementation)
                    continue;

                // the members this type declares itself that are the implementation, or override it down the chain;
                // one inherited unchanged is checked where it is declared
                var own =
                    type
                    .GetMembers(member)
                    .OfType<IMethodSymbol>()
                    .Where(candidate =>
                        SymbolEqualityComparer.Default.Equals(candidate, implementation)
                        || Overrides(candidate, implementation));

                foreach (var candidate in own)
                {
                    var written =
                        candidate
                        .TypeParameters
                        .Select(typeParameter => typeParameter.Name)
                        .ToList();
                    if (written.SequenceEqual(names))
                        continue;

                    var message =
                        $"{type.Name}.{member} names its type parameters <{string.Join(", ", written)}>; every implementation writes it as <{string.Join(", ", names)}>";

                    Report(context, candidate, message);
                }
            }
        }

        if (!implemented)
            return;

        var reserved =
            type
            .TypeParameters
            .FirstOrDefault(typeParameter =>
                ReservedNames.Contains(typeParameter.Name)
                || ReservedPrefixes.Any(prefix => typeParameter.Name.StartsWith(prefix)));
        if (reserved is null)
            return;

        var reservedMessage =
            $"{type.Name}'s type parameter {reserved.Name} is a name the chain interfaces' members use; a type's own parameters are never {string.Join(", ", ReservedNames)} and never start with {string.Join(" or ", ReservedPrefixes)}";

        Report(context, type, reservedMessage);
    }

    private static bool Overrides(IMethodSymbol method, IMethodSymbol implementation)
    {
        for (var overridden = method.OverriddenMethod; overridden is not null; overridden = overridden.OverriddenMethod)
        {
            if (SymbolEqualityComparer.Default.Equals(overridden.OriginalDefinition, implementation.OriginalDefinition))
                return true;
        }

        return false;
    }

    private static void Report(SymbolAnalysisContext context, ISymbol at, string message)
    {
        var location =
            at.Locations.FirstOrDefault() ?? Location.None;

        var diagnostic =
            Diagnostic.Create(GeneratedSource.Shape, location, message);

        context.ReportDiagnostic(diagnostic);
    }
}
