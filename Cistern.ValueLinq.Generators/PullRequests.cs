namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

// the pull members every enumerator is given rather than inheriting, whichever generator writes the rest of it:
// TryPushRest declining, and TryPullOptimization from the [PullOptimization] interfaces the enumerator declares, as
// Requests writes a node's TryPushOptimization: per interface, a guard on the request's type calling the enumerator's own
// non-public static method of the interface method's signature, and the explicit implementation forwarding to it
static class PullRequests
{
    public const string PullOptimization = "Cistern.ValueLinq.PullOptimizationAttribute";

    // the type parameters of a pull request's method, and with TRequest those of the generated dispatch
    public static readonly string[] MethodTypeParameters = ["TOwnNode", "TBuilt", "TBuilder"];

    private static readonly SymbolDisplayFormat Format = SymbolDisplayFormat.FullyQualifiedFormat;

    // element is IPullEnumerator's type argument as the enumerator's declaration names it; the failure is the first
    // shape error, the members being written regardless
    public static string? Emit(StringBuilder source, INamedTypeSymbol enumerator, string element)
    {
        string? failure = null;

        var methods =
            enumerator
            .GetMembers()
            .OfType<IMethodSymbol>()
            .ToList();

        if (!methods.Any(method => method.Name.EndsWith("TryPushRest")))
        {
            source.Append($$"""

                        bool global::Cistern.ValueLinq.IPullEnumerator<{{element}}>.TryPushRest(global::Cistern.ValueLinq.Containers.ErasedChain.PushSink<{{element}}> sink) => false;

                """);
        }

        if (methods.Any(method => method.Name.EndsWith("TryPullOptimization")))
            return $"{enumerator.Name} writes TryPullOptimization, which is generated from the [PullOptimization] interfaces it declares; it writes only their methods";

        var answers =
            new StringBuilder();

        var forwarders =
            new StringBuilder();

        foreach (var implemented in enumerator.Interfaces.Where(IsRequest))
        {
            // an interface of another shape is reported where it is declared (RequestInterfaces)
            if (Shape(implemented) is not {} declared)
                continue;

            var request =
                declared.Parameters[0].Type.ToDisplayString(Format);

            var written =
                methods
                .FirstOrDefault(method =>
                    method is { Name: var name, MethodKind: MethodKind.Ordinary, IsStatic: true, Arity: 3 }
                    && name == declared.Name
                    && method.Parameters.Length == 4
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, declared.Parameters[0].Type));
            if (written is null)
            {
                failure ??= $"{enumerator.Name} implements {implemented.Name} but declares no static {declared.Name}<TOwnNode, TBuilt, TBuilder>(in {declared.Parameters[0].Type.Name} request, in TOwnNode node, in TBuilder builder, out TBuilt built) for it";
                continue;
            }

            if (written.DeclaredAccessibility == Accessibility.Public)
            {
                failure ??= $"{enumerator.Name}.{declared.Name} is public; it is the enumerator's own, and {implemented.Name} is implemented by the generated explicit forwarder";
                continue;
            }

            answers.Append($$"""
                            if (typeof(TRequest) == typeof({{request}}))
                                return {{declared.Name}}<TOwnNode, TBuilt, TBuilder>(in global::Cistern.ValueLinq.Optimizations.Request.As<TRequest, {{request}}>(in request), in node, in builder, out built);


                """);

            forwarders.Append($$"""

                        static bool {{implemented.ToDisplayString(Format)}}.{{declared.Name}}<TOwnNode, TBuilt, TBuilder>(in {{request}} request, in TOwnNode node, in TBuilder builder, [global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TBuilt built) =>
                            {{declared.Name}}<TOwnNode, TBuilt, TBuilder>(in request, in node, in builder, out built);

                """);
        }

        source.Append($$"""

                    static bool global::Cistern.ValueLinq.IPullEnumerator<{{element}}>.TryPullOptimization<TRequest, TOwnNode, TBuilt, TBuilder>(in TRequest request, in TOwnNode node, in TBuilder builder, [global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TBuilt built)
                    {

            """);

        source.Append(answers);

        source.Append("""
                        built = default;
                        return false;
                    }

            """);

        source.Append(forwarders);

        return failure;
    }

    public static bool IsRequest(INamedTypeSymbol type)
    {
        var isRequest =
            type
            .OriginalDefinition
            .GetAttributes()
            .Any(attribute => attribute.AttributeClass?.ToDisplayString() == PullOptimization);

        return isRequest;
    }

    // a pull request interface's one method: static, generic over TOwnNode, TBuilt and TBuilder, taking the request, the
    // node and the builder by in and the built chain by out, returning bool
    public static IMethodSymbol? Shape(INamedTypeSymbol type)
    {
        if (type.GetMembers().ToList() is not [IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: true } declared]
            || declared.ReturnType.SpecialType != SpecialType.System_Boolean
            || !declared.TypeParameters.Select(typeParameter => typeParameter.Name).SequenceEqual(MethodTypeParameters)
            || declared.Parameters is not [{ RefKind: RefKind.In }, { RefKind: RefKind.In } node, { RefKind: RefKind.In } builder, { RefKind: RefKind.Out } built]
            || node.Type.Name != "TOwnNode"
            || builder.Type.Name != "TBuilder"
            || built.Type.Name != "TBuilt")
        {
            return null;
        }

        return declared;
    }
}
