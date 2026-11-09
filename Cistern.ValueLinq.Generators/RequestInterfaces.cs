namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

// a [PushOptimization] or [PullOptimization] interface is checked where it is declared, so a node or enumerator
// declaring it is wired from its one method
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RequestInterfaces : DiagnosticAnalyzer
{
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

        string? failure =
            Requests.IsRequest(type)
                ? type.TypeKind != TypeKind.Interface
                    ? $"{type.Name} carries [PushOptimization], which marks a request's interface"
                : Requests.Shape(type) is null
                    ? $"{type.Name} is not an interface of one non-generic method taking the request by in and the result by out and returning bool"
                : null
            : PullRequests.IsRequest(type)
                ? type.TypeKind != TypeKind.Interface
                    ? $"{type.Name} carries [PullOptimization], which marks a pull request's interface"
                : PullRequests.Shape(type) is null
                    ? $"{type.Name} is not an interface of one static method <TOwnNode, TBuilt, TBuilder> taking the request, the TOwnNode node and the TBuilder builder by in and the TBuilt built by out and returning bool"
                : null
            : null;
        if (failure is null)
            return;

        var location =
            type.Locations.FirstOrDefault() ?? Location.None;

        var diagnostic =
            Diagnostic.Create(GeneratedSource.Shape, location, failure);

        context.ReportDiagnostic(diagnostic);
    }
}
