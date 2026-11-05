namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Cistern.ValueLinq.Generators.TextHelper;

sealed class FillParameter(string type, string name, bool isIn)
{
    public string Type { get; } = type;
    public string Name { get; } = name;
    public bool IsIn { get; } = isIn;
}

// what a node hands its source in place of its forwarder: the arguments of the forwarder's Fill and a ref to the filler
// above; and, for a hand-written forwarder, the plumbing it shares with every other: its downstream field and the
// common Dispose and GetResult
static class Filler
{
    public static GeneratedSource Generate(INamedTypeSymbol forwarder)
    {
        var hintName =
            Types.HintName(forwarder, "Filler");

        var fill =
            forwarder
            .GetMembers("Fill")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(method =>
                !method.IsStatic
                && method.ReturnsByRef);
        if (fill is not { ReturnType: ITypeParameterSymbol next })
        {
            var message = $"{forwarder.Name} has no Fill returning ref to its downstream";
            var missingFill = GeneratedSource.Error(hintName, message, forwarder);

            return missingFill;
        }

        var takesRefOrOut =
            fill
            .Parameters
            .Any(parameter =>
                parameter.RefKind is not (RefKind.None or RefKind.In));
        if (takesRefOrOut)
        {
            var message = $"{forwarder.Name}.Fill takes a parameter by ref or out";
            var refParameter = GeneratedSource.Error(hintName, message, forwarder);

            return refParameter;
        }

        var parameters =
            fill
            .Parameters
            .Select(createFillParameter)
            .ToList();

        // the filler is written beside the forwarder in the operator's class, named for it: Push's is Filler,
        // HoldBackPush's HoldBackFiller
        var container = forwarder.ContainingType;
        if (container is not {
                IsStatic: true,
                Arity: 0,
                ContainingType: null
            }
        )
        {
            var message = $"{forwarder.Name} is not nested in a non-generic static class";
            var notNested = GeneratedSource.Error(hintName, message, forwarder);

            return notNested;
        }

        if (!forwarder.Name.EndsWith("Push"))
        {
            var message = $"{forwarder.Name} is not named ...Push, so its filler has no name";
            var unnamed = GeneratedSource.Error(hintName, message, forwarder);

            return unnamed;
        }

        // the forwarder's own plumbing is written into a second declaration of it
        var isPartial =
            forwarder
            .DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .OfType<StructDeclarationSyntax>()
            .Any(declaration => declaration.Modifiers.Any(SyntaxKind.PartialKeyword));
        if (!isPartial)
        {
            var message = $"{forwarder.Name} is not partial, so _next and its common members cannot be written beside it";
            var notPartial = GeneratedSource.Error(hintName, message, forwarder);

            return notPartial;
        }

        var pushEnumerator =
            Types.Find(forwarder, "Cistern.ValueLinq.IPushEnumerator`1");
        if (pushEnumerator is null)
        {
            var message = $"{forwarder.Name} is not an IPushEnumerator<T>";
            var notForwarder = GeneratedSource.Error(hintName, message, forwarder);

            return notForwarder;
        }

        if (Types.InstanceFields(forwarder).Any(field => field.Name == "_next"))
        {
            var message = $"{forwarder.Name} declares _next, which is written here";
            var declaresNext = GeneratedSource.Error(hintName, message, forwarder);

            return declaresNext;
        }

        var methods =
            forwarder
            .GetMembers()
            .OfType<IMethodSymbol>()
            .ToList();

        if (Operator.Misnamed(forwarder, methods, "GetResult", ["TPushResult"]) is {} misnamed)
        {
            var named = GeneratedSource.Error(hintName, misnamed, forwarder);

            return named;
        }

        var writesDispose =
            methods.Any(method => method.Name == "Dispose");

        var writesGetResult =
            methods.Any(method => method.Name == "GetResult");

        var source = new StringBuilder();

        var namespaceName =
            container
            .ContainingNamespace
            .ToDisplayString();

        var header =
            Generated.Header(namespaceName);

        source.Append(header);

        int stemLength =
            forwarder.Name.Length - "Push".Length;

        var fillerName =
            $"{forwarder.Name.Substring(0, stemLength)}Filler";

        var forwarderParameters =
            forwarder
            .TypeParameters
            .Select(typeParameter => typeParameter.Name)
            .ToList();

        var constraints =
            Types.Constraints(forwarder.TypeParameters, Types.NoRenames);

        source.Append($"{NewLine}static partial class {container.Name}{NewLine}{{");

        var writesInitialise =
            methods.Any(method => method.Name is "Initialise" or "AllowInitialise");

        EmitForwarder(source, forwarder.Name, forwarderParameters, next.Name, writesDispose, writesGetResult, writesInitialise);

        Emit(source, forwarder.Name, fillerName, forwarderParameters, constraints, next.Name, parameters);

        source.Append($"}}{NewLine}");

        var generated =
            new GeneratedSource(hintName, $"{source}");

        return generated;

        static FillParameter createFillParameter(IParameterSymbol parameter)
        {
            var type =
                Types.Name(parameter.Type);

            bool isIn =
                parameter.RefKind == RefKind.In;

            var fillParameter =
                new FillParameter(type, parameter.Name, isIn);

            return fillParameter;
        }
    }

    // the hand-written forwarder's other declaration: its downstream, and the common members it does not write
    private static void EmitForwarder(StringBuilder source, string forwarderName, IReadOnlyList<string> forwarderParameters, string next, bool writesDispose, bool writesGetResult, bool writesInitialise)
    {
        var forwarderArguments =
            string.Join(", ", forwarderParameters);

        source.Append($$"""

                // its fields are split between this declaration and the hand-written one, so their order is the runtime's to choose
                [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Auto)]
                partial struct {{forwarderName}}<{{forwarderArguments}}>
                {
                    internal {{next}} _next;

            """);

        if (!writesDispose)
        {
            source.Append("""

                        public void Dispose() => _next.Dispose();

                """);
        }

        if (!writesGetResult)
        {
            source.Append("""

                        public TPushResult GetResult<TPushResult>() => _next.GetResult<TPushResult>();

                """);
        }

        if (!writesInitialise)
        {
            source.Append($$"""

                        public static bool AllowInitialise => {{next}}.AllowInitialise;

                        public void Initialise()
                        {
                            if ({{next}}.AllowInitialise)
                                _next.Initialise();
                        }

                """);
        }

        source.Append("""
                }

            """);
    }

    public static void Emit(StringBuilder source, string forwarderName, string fillerName, IReadOnlyList<string> forwarderParameters, string forwarderConstraints, string next, IReadOnlyList<FillParameter> parameters)
    {
        var forwarderArguments =
            string.Join(", ", forwarderParameters);

        var forwarder =
            $"{forwarderName}<{forwarderArguments}>";

        var fillerParameters =
            forwarderParameters.Concat(["TFiller"]);

        var fillerArguments =
            string.Join(", ", fillerParameters);

        var filler =$"{fillerName}<{fillerArguments}>";

        source.Append($$"""

                internal readonly ref struct {{filler}}
                    : IPushFiller<{{forwarder}}>{{forwarderConstraints}}
                    where TFiller : IPushFiller<{{next}}>, allows ref struct
                {

            """);

        foreach (var parameter in parameters)
        {
            var fieldModifier =
                parameter.IsIn
                    ? "ref readonly "
                    : "";

            source.Append($"        private readonly {fieldModifier}{parameter.Type} _{parameter.Name};{NewLine}");
        }

        var declaredParameters =
            parameters.Select(declareParameter);

        var constructorParameters =
            string.Join("", declaredParameters);

        source.Append($$"""
                    private readonly ref readonly ErasedRef _next;

                    public {{fillerName}}({{constructorParameters}}in TFiller next)
                    {

            """);

        foreach (var parameter in parameters)
        {
            var assignmentModifier =
                parameter.IsIn
                    ? "ref "
                    : "";

            source.Append($"            _{parameter.Name} = {assignmentModifier}{parameter.Name};{NewLine}");
        }

        var passedArguments =
            parameters.Select(passArgument);

        var fillArguments =
            string.Join(", ", passedArguments);

        source.Append($$"""
                        _next = ref Erase.Ref(in next);
                    }

                    void IPushFiller<{{forwarder}}>.Fill(ref {{forwarder}} target) =>
                        Erase.Deref<TFiller>(in _next).Fill(ref target.Fill({{fillArguments}}));

                    void IPushFiller<{{forwarder}}>.Started() =>
                        Erase.Deref<TFiller>(in _next).Started();
                }

            """);

        static string declareParameter(FillParameter parameter)
        {
            var modifier =
                parameter.IsIn
                    ? "in "
                    : "";

            var declared =
                $"{modifier}{parameter.Type} {parameter.Name}, ";

            return declared;
        }

        static string passArgument(FillParameter parameter)
        {
            var modifier =
                parameter.IsIn
                    ? "in "
                    : "";

            var passed =
                $"{modifier}_{parameter.Name}";

            return passed;
        }
    }
}
