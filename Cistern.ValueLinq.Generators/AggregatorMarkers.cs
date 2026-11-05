namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

// the aggregator markers are written by [Aggregator] alone, so what they say follows from the aggregator as written
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AggregatorMarkers : DiagnosticAnalyzer
{
    private static readonly string[] Markers =
    [
        "Cistern.ValueLinq.Chain.IInternal__Aggregator__NotForManualUse",
        "Cistern.ValueLinq.Chain.IInternal__ImplementsDispose__NotForManualUse",
    ];

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

        var marker =
            type
            .AllInterfaces
            .FirstOrDefault(candidate => Markers.Contains(Types.MetadataName(candidate)));
        if (marker is null)
            return;

        var isAggregator =
            type
            .GetAttributes()
            .Any(attribute => attribute.AttributeClass?.ToDisplayString() == "Cistern.ValueLinq.AggregatorAttribute");
        if (isAggregator)
            return;

        var message =
            $"{type.Name} implements {marker.Name}, which only [Aggregator] writes";

        var location =
            type.Locations.FirstOrDefault() ?? Location.None;

        var diagnostic =
            Diagnostic.Create(GeneratedSource.Shape, location, message);

        context.ReportDiagnostic(diagnostic);
    }
}
