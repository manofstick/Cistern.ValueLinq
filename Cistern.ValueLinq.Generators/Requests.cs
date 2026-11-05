namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Cistern.ValueLinq.Generators.TextHelper;

// a node's TryPushOptimization, from the [Optimization] interfaces it declares: per interface, a guard on the request's
// type calling the node's own non-public method of the interface method's signature, and the explicit implementation
// forwarding to that method, which is what ties the hand-written signature to the interface's. The result's type is
// the one the request's interface names, and every asker names it (Request.Result asserts so); a guard on it as well
// is not folded where the result is shared code (ToArray's T[], anything over a reference type), and reading the
// method's generic context for it keeps the dispatch from being inlined into its asker
static class Requests
{
    private const string Namespace = "Cistern.ValueLinq.Optimizations";
    public const string Optimization = "Cistern.ValueLinq.OptimizationAttribute";

    // the attributes whose Forwards hands an unanswered request to the node's Forward
    private static readonly string[] Forwarding =
    [
        "Cistern.ValueLinq.OperatorAttribute",
        "Cistern.ValueLinq.MaterializedAttribute",
        "Cistern.ValueLinq.AnswererAttribute",
    ];

    public sealed class Answer
    {
        public string Interface = "";               // the implemented interface, as written in the node
        public string Method = "";
        public string Request = "";
        public string Result = "";
        public string RequestName = "";
        public string ResultName = "";
        public bool ResultMaybeNull;                // [MaybeNullWhen(false)] on the interface's out parameter
    }

    public sealed class Model
    {
        public List<Answer> Answers = new();
        public bool Forwards;                       // Forwards = true: an unanswered request goes to Forward
        public bool WritesRequests;                 // the node writes TryPushOptimization itself, so none is emitted
        public string? Failure;                     // the first shape error; the rest of the node is still generated
    }

    // written in typeof and as a type argument, where a nullable annotation is not allowed
    private static readonly SymbolDisplayFormat Format = SymbolDisplayFormat.FullyQualifiedFormat;

    // [Answerer]: a hand-written node whose TryPushOptimization alone is generated
    public static GeneratedSource Generate(INamedTypeSymbol node, Compilation compilation)
    {
        var hintName =
            Types.HintName(node, "Answerer");

        var container = node.ContainingType;
        if (container is not {
                IsStatic: true,
                Arity: 0,
                ContainingType: null
            }
        )
        {
            return GeneratedSource.Error(hintName, $"{node.Name} is not nested in a non-generic static class", node);
        }

        var reserved =
            node
            .TypeParameters
            .FirstOrDefault(typeParameter => typeParameter.Name.StartsWith("TRequest"));
        if (reserved is not null)
            return GeneratedSource.Error(hintName, $"{node.Name}'s type parameter {reserved.Name} is a name the generated members use; a node's type parameters never start with TRequest", node);

        var model =
            Read(node);

        var parameters =
            node.TypeParameters is []
                ? ""
                : $"<{string.Join(", ", node.TypeParameters.Select(typeParameter => typeParameter.Name))}>";

        var body =
            new StringBuilder();

        Emit(body, model, callable: true);

        var source =
            new StringBuilder();

        var header =
            Generated.Header(container.ContainingNamespace.ToDisplayString());

        source.Append(header);

        source.Append($$"""

            static partial class {{container.Name}}
            {
                partial struct {{node.Name}}{{parameters}}
                {
            """);

        source.Append(body);

        source.Append($"    }}{NewLine}}}{NewLine}");

        var generated =
            WithFailure(hintName, $"{source}", model, node);

        return generated;
    }

    // the generated source, carrying the requests' shape error when there is one, so the error is reported beside
    // everything else the node gets rather than in place of it
    public static GeneratedSource WithFailure(string hintName, string text, Model model, INamedTypeSymbol node)
    {
        (string, Location)? failure =
            model.Failure is {} message
                ? (message, node.Locations.FirstOrDefault() ?? Location.None)
                : null;

        var generated =
            new GeneratedSource(hintName, text, failure);

        return generated;
    }

    // an interface whose method is missing or misdeclared is reported and left out of the dispatch, so the only other
    // error is the compiler's for that interface member
    public static Model Read(INamedTypeSymbol node)
    {
        var model =
            new Model();

        var writesRequests =
            node
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Any(method => method.Name.EndsWith("TryPushOptimization"));
        if (writesRequests)
        {
            model.WritesRequests = true;
            model.Failure =
                $"{node.Name} writes TryPushOptimization, which is generated from the [Optimization] interfaces it declares; it writes only their methods";

            return model;
        }

        var forwards =
            node
            .GetAttributes()
            .Any(attribute =>
                attribute.AttributeClass?.ToDisplayString() is {} name
                && Forwarding.Contains(name)
                && attribute.NamedArguments.Any(argument => argument is { Key: "Forwards", Value.Value: true }));

        var forward =
            node
            .GetMembers("Forward")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(method => method.MethodKind == MethodKind.Ordinary);

        const string forwardDeclaration =
            "private bool Forward<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result) where TRequest : notnull";

        // a Forward of another signature fails to compile at the generated call
        string? forwardFailure =
            !forwards
                ? forward is null
                    ? null
                    : $"{node.Name} declares Forward, which only a node whose attribute sets Forwards = true is handed"
            : forward is null
                ? $"{node.Name} sets Forwards but declares no Forward; add: {forwardDeclaration}"
            : forward.DeclaredAccessibility == Accessibility.Public
                ? $"{node.Name}.Forward is public; it is the node's own. Declare it as: {forwardDeclaration}"
            : Operator.Misnamed(node, [forward], "Forward", ["TRequest", "TRequestResult"]) is {} misnamed
                ? $"{misnamed}. Declare it as: {forwardDeclaration}"
            : null;

        model.Failure = forwardFailure;
        model.Forwards = forwards && forwardFailure is null;

        var interfaces =
            node
            .Interfaces
            .Where(IsRequest)
            .ToList();

        foreach (var implemented in interfaces)
        {
            // an interface of another shape is reported where it is declared (RequestInterfaces)
            if (Shape(implemented) is not ({} declared, {} request, {} result))
                continue;

            var maybeNull =
                result
                .GetAttributes()
                .Any(attribute => attribute.AttributeClass?.Name == "MaybeNullWhenAttribute");

            // the method as the hand-written side declares it, ready to paste
            var declaration =
                Declaration(declared, request, result, maybeNull);

            var written =
                node
                .GetMembers(declared.Name)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(method =>
                    method is { MethodKind: MethodKind.Ordinary, IsStatic: false, Parameters: [{ RefKind: RefKind.In }, { RefKind: RefKind.Out }] }
                    && method.ReturnType.SpecialType == SpecialType.System_Boolean
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, request.Type)
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, result.Type));
            string? failure =
                written is null
                    ? $"{node.Name} implements {implemented.Name} but declares no method for it; add: {declaration}"
                : written.DeclaredAccessibility == Accessibility.Public
                    ? $"{node.Name}.{declared.Name} is public; it is the node's own, and {implemented.Name} is implemented by the generated explicit forwarder. Declare it as: {declaration}"
                : WritesExplicitly(node, implemented)
                    ? $"{node.Name} implements {implemented.Name}.{declared.Name} explicitly; the explicit implementation is generated, so declare it as: {declaration}"
                : null;
            if (failure is not null)
            {
                model.Failure ??= failure;
                continue;
            }

            var answer =
                new Answer
                {
                    Interface = Types.Name(implemented),
                    Method = declared.Name,
                    Request = request.Type.ToDisplayString(Format),
                    Result = result.Type.ToDisplayString(Format),
                    RequestName = request.Name,
                    ResultName = result.Name,
                    ResultMaybeNull = maybeNull,
                };

            model.Answers.Add(answer);
        }

        return model;
    }

    public static bool IsRequest(INamedTypeSymbol type)
    {
        var isRequest =
            type
            .OriginalDefinition
            .GetAttributes()
            .Any(attribute => attribute.AttributeClass?.ToDisplayString() == Optimization);

        return isRequest;
    }

    // a request interface's one method: non-generic, taking the request by in and the result by out, returning bool
    public static (IMethodSymbol Declared, IParameterSymbol Request, IParameterSymbol Result)? Shape(INamedTypeSymbol type)
    {
        if (type.GetMembers().ToList() is not [IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: false, IsGenericMethod: false } declared]
            || declared.ReturnType.SpecialType != SpecialType.System_Boolean
            || declared.Parameters is not [{ RefKind: RefKind.In } request, { RefKind: RefKind.Out } result])
        {
            return null;
        }

        return (declared, request, result);
    }

    // types as a node's file writes them: a request or result type of this namespace as Optimizations.X (the node files
    // are in sibling namespaces), anything else by its name and containing types, with the node's own type parameters
    private static readonly SymbolDisplayFormat Written =
        new(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    private static string Declaration(IMethodSymbol declared, IParameterSymbol request, IParameterSymbol result, bool maybeNull)
    {
        var attribute =
            maybeNull
                ? "[MaybeNullWhen(false)] "
                : "";

        var declaration =
            $"private bool {declared.Name}(in {Write(request.Type)} {request.Name}, {attribute}out {Write(result.Type)} {result.Name})";

        return declaration;

        static string Write(ITypeSymbol type)
        {
            var written =
                type.ToDisplayString(Written);

            var qualified =
                type.ContainingNamespace?.ToDisplayString() == Namespace
                    ? $"Optimizations.{written}"
                    : written;

            return qualified;
        }
    }

    // the explicit implementations the hand-written side declares; the generator does not see its own output
    private static bool WritesExplicitly(INamedTypeSymbol node, INamedTypeSymbol implemented)
    {
        var writes =
            node
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Any(method =>
                method.MethodKind == MethodKind.ExplicitInterfaceImplementation
                && method.ExplicitInterfaceImplementations.Any(member =>
                    SymbolEqualityComparer.Default.Equals(member.ContainingType, implemented)));

        return writes;
    }

    // the generated methods' type parameters are named with a TRequest prefix, which no node's are. An [Answerer]'s
    // dispatch is public, so a node built in place to answer a request (a container over another's parts) is called
    // directly; reached through a constraint instead, shared code would find it by a second dictionary lookup and call
    // it indirectly
    public static void Emit(StringBuilder source, Model model, bool callable = false)
    {
        if (model.WritesRequests)
            return;

        if (callable)
        {
            source.Append("""

                        public bool TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TRequestResult result)
                            where TRequest : notnull
                        {

                """);
        }
        else
        {
            source.Append("""

                        bool global::Cistern.ValueLinq.INode.TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TRequestResult result)
                        {

                """);
        }

        foreach (var answer in model.Answers)
        {
            // a result the answer may leave null (ToArray's T[]) is written through a nullable location
            var written =
                answer.ResultMaybeNull
                    ? $"{answer.Result}?"
                    : answer.Result;

            source.Append($$"""
                            if (typeof(TRequest) == typeof({{answer.Request}}))
                                return {{answer.Method}}(in global::Cistern.ValueLinq.Optimizations.Request.As<TRequest, {{answer.Request}}>(in request), out global::Cistern.ValueLinq.Optimizations.Request.Result<TRequestResult, {{written}}>(out result));


                """);
        }

        if (model.Forwards)
        {
            source.Append("""
                            return Forward<TRequest, TRequestResult>(in request, out result);
                        }

                """);
        }
        else
        {
            source.Append("""
                            result = default;
                            return false;
                        }

                """);
        }

        foreach (var answer in model.Answers)
        {
            var attribute =
                answer.ResultMaybeNull
                    ? "[global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] "
                    : "";

            source.Append($$"""

                        bool {{answer.Interface}}.{{answer.Method}}(in {{answer.Request}} {{answer.RequestName}}, {{attribute}}out {{answer.Result}} {{answer.ResultName}}) =>
                            {{answer.Method}}(in {{answer.RequestName}}, out {{answer.ResultName}});

                """);
        }

    }
}
