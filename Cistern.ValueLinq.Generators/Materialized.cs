namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Cistern.ValueLinq.Generators.TextHelper;

// the plumbing of an operator that materialises its source into another node and runs as that node: a static partial
// class holding its Node and Pull partial structs, the node's Materialize returning the node it becomes, and Pull's one
// field being that node's enumerator
static class Materialized
{
    sealed class Model
    {
        public string Container = "";
        public string Namespace = "";

        public string Node = "";
        public List<string> NodeParameters = new();
        public string NodeConstraints = "";
        public string SourceNode = "";
        public string SourceField = "";
        public ITypeSymbol Out = null!;
        public List<(ITypeSymbol Type, string Name, bool ByRef)> Configuration = new();    // every field other than the source

        public ITypeSymbol Materialized = null!;    // the node Materialize returns
        public string Enumerator = "";
        public INamedTypeSymbol EnumeratorSymbol = null!;
        public List<string> EnumeratorParameters = new();   // the node's, positionally, in Pull's own names
        public string EnumeratorField = "";
        public ITypeSymbol EnumeratorFieldType = null!;    // the materialised node's enumerator

        public bool WritesConstructor, WritesPush;

        public Requests.Model Requests = new();

        public string NodeType => $"{Node}<{string.Join(", ", NodeParameters)}>";

        public IReadOnlyDictionary<string, string> ToEnumerator =>
            NodeParameters
            .Select((parameter, index) => (parameter, index))
            .ToDictionary(pair => pair.parameter, pair => EnumeratorParameters[pair.index]);
    }

    public static GeneratedSource Generate(INamedTypeSymbol node, Compilation compilation)
    {
        var hintName =
            Types.HintName(node, "Materialized");

        if (Build(node, out var error) is not {} model)
            return GeneratedSource.Error(hintName, error, node);

        var source =
            new StringBuilder();

        var header =
            Generated.Header(model.Namespace);

        source.Append(header);

        source.Append($"{NewLine}static partial class {model.Container}{NewLine}{{");

        EmitNode(source, model);

        EmitEnumerator(source, model);

        source.Append($"}}{NewLine}");

        var generated =
            Requests.WithFailure(hintName, $"{source}", model.Requests, node);

        return generated;
    }

    private static Model? Build(INamedTypeSymbol node, out string error)
    {
        error = "";

        var container = node.ContainingType;
        if (container is not {
                IsStatic: true,
                Arity: 0,
                ContainingType: null
            }
        )
        {
            error = $"{node.Name} is not nested in a non-generic static class";
            return null;
        }

        var output =
            Types.Find(node, "Cistern.ValueLinq.INode`1");

        var sources =
            Types
            .InstanceFields(node)
            .Where(isNodeField)
            .ToList();
        if (output is null
            || sources is not [var sourceField])
        {
            error = $"{node.Name} is not an INode<T> with exactly one source node field";
            return null;
        }

        var sourceNode =
            (ITypeParameterSymbol)sourceField.Type;

        var methods =
            node
            .GetMembers()
            .OfType<IMethodSymbol>()
            .ToList();

        // the node it becomes: an INode of the same output, built from this node's configuration under the push's options
        var materialize =
            methods.FirstOrDefault(method =>
                method is { Name: "Materialize", IsStatic: false, Parameters: [{ RefKind: RefKind.In } options] }
                && options.Type is INamedTypeSymbol optionsType
                && Types.MetadataName(optionsType) == "Cistern.ValueLinq.ChainOptions");
        if (materialize is null)
        {
            error = $"{node.Name} has no Materialize(in ChainOptions)";
            return null;
        }

        var materializedOutput =
            Types.Find(materialize.ReturnType, "Cistern.ValueLinq.INode`1");
        if (materializedOutput is null
            || !SymbolEqualityComparer.Default.Equals(materializedOutput.TypeArguments[0], output.TypeArguments[0]))
        {
            error = $"{node.Name}.Materialize does not return an INode<{Types.Name(output.TypeArguments[0])}>";
            return null;
        }

        var enumerator =
            container
            .GetTypeMembers("Pull")
            .FirstOrDefault(type => type.Arity == node.Arity);
        if (enumerator is null)
        {
            error = $"{container.Name} has no Pull with {node.Arity} type parameters";
            return null;
        }

        // Pull's one field is the materialised node's enumerator, which is how its type is known here; Pull's type
        // parameters are the node's, positionally, so the two are compared in Pull's names
        var enumeratorFields =
            Types
            .InstanceFields(enumerator)
            .ToList();
        if (enumeratorFields is not [var enumeratorField])
        {
            error = $"{enumerator.Name} does not declare exactly one field, the enumerator of what {node.Name} materialises";
            return null;
        }

        var toEnumerator =
            node
            .TypeParameters
            .Select((parameter, index) => (parameter.Name, enumerator.TypeParameters[index].Name))
            .ToDictionary(pair => pair.Item1, pair => pair.Item2);

        var materializedInPull =
            Types.Name(materialize.ReturnType, toEnumerator);

        // a [Puller] enumerator's INodeEnumerator is in that generator's output, which this one does not see
        var enumerated =
            Types.Find(enumeratorField.Type, "Cistern.ValueLinq.INodeEnumerator`2")?.TypeArguments[0]
            ?? Puller.Node(enumeratorField.Type);
        if (enumerated is null
            || Types.Name(enumerated) != materializedInPull)
        {
            error = $"{enumerator.Name}.{enumeratorField.Name} is not an INodeEnumerator<{materializedInPull}, ...>";
            return null;
        }

        var misnamed =
            Operator.Misnamed(node, methods, "CreateViaPush", ["TPushResult", "TPushEnumerator", "TPushFiller"])
            ?? Operator.Reserved(node);
        if (misnamed is not null)
        {
            error = misnamed;
            return null;
        }

        var configuration =
            Types
            .InstanceFields(node)
            .Where(field => field.Name != sourceField.Name)
            .Select(describe)
            .ToList();

        var writesConstructor =
            node
            .InstanceConstructors
            .Any(constructor => constructor is { IsImplicitlyDeclared: false, Parameters.Length: > 0 });

        var writesPush =
            methods.Any(method => method.Name.EndsWith("CreateViaPush"));

        var requests =
            Requests.Read(node);

        var model =
            new Model
            {
                Container = container.Name,
                Namespace = container.ContainingNamespace.ToDisplayString(),
                Node = node.Name,
                NodeParameters = node.TypeParameters.Select(typeParameter => typeParameter.Name).ToList(),
                NodeConstraints = Types.Constraints(node.TypeParameters, Types.NoRenames),
                SourceNode = sourceNode.Name,
                SourceField = sourceField.Name,
                Out = output.TypeArguments[0],
                Configuration = configuration,
                Materialized = materialize.ReturnType,
                Enumerator = enumerator.Name,
                EnumeratorSymbol = enumerator,
                EnumeratorParameters = enumerator.TypeParameters.Select(typeParameter => typeParameter.Name).ToList(),
                EnumeratorField = enumeratorField.Name,
                EnumeratorFieldType = enumeratorField.Type,
                WritesConstructor = writesConstructor,
                WritesPush = writesPush,
                Requests = requests,
            };

        return model;

        static bool isNodeField(IFieldSymbol field)
        {
            var isNode =
                field.Type is ITypeParameterSymbol parameter
                && Types.Find(parameter, "Cistern.ValueLinq.INode`1") is not null;

            return isNode;
        }

        static (ITypeSymbol Type, string Name, bool ByRef) describe(IFieldSymbol field)
        {
            (ITypeSymbol Type, string Name, bool ByRef) described =
                (field.Type, field.Name, false);

            return described;
        }
    }

    private static void EmitNode(StringBuilder source, Model model)
    {
        source.Append($$"""

                partial struct {{model.NodeType}}
                {
            """);

        if (!model.WritesConstructor)
            Operator.EmitConstructor(source, model.Node, model.SourceNode, model.SourceField, model.Configuration);

        Requests.Emit(source, model.Requests);

        // materialised under the push's options, then pushed through as the node it became
        if (!model.WritesPush)
        {
            var outName =
                Types.Name(model.Out);

            var materialized =
                Types.Name(model.Materialized);

            source.Append($$"""

                        TPushResult INode<{{outName}}>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
                        {
                            var materialized = Materialize(in options);
                            return global::Cistern.ValueLinq.Helper.CreateViaPush<{{materialized}}, {{outName}}, TPushResult, TPushEnumerator, TPushFiller>(in options, in materialized, in filler);
                        }

                """);
        }

        source.Append($"    }}{NewLine}");
    }

    // built over the materialised node's enumerator, which every member forwards to; the chain below it was run to
    // materialise, so there is nothing to rebuild over a runtime container
    private static void EmitEnumerator(StringBuilder source, Model model)
    {
        // everything the node's side names is written in Pull's own parameter names
        var renames =
            model.ToEnumerator;

        var outName =
            Types.Name(model.Out, renames);

        var materialized =
            Types.Name(model.Materialized, renames);

        var enumeratorFieldType =
            Types.Name(model.EnumeratorFieldType);

        var arguments =
            string.Join(", ", model.EnumeratorParameters);

        var nodeType =
            $"{model.Node}<{arguments}>";

        var enumeratorType =
            $"{model.Enumerator}<{arguments}>";

        var constraints =
            model.NodeConstraints is []
                ? ""
                : Operator.Rename(model.NodeConstraints, model.NodeParameters, renames, "");

        source.Append($$"""

                partial struct {{enumeratorType}}
                    : IPullEnumerator<{{outName}}>
                    , INodeEnumerator<{{nodeType}}, {{enumeratorType}}>{{constraints}}
                {
                    private {{model.Enumerator}}(in {{enumeratorFieldType}} enumerator) => {{model.EnumeratorField}} = enumerator;

                    static {{enumeratorType}} INodeEnumerator<{{nodeType}}, {{enumeratorType}}>.Create(in {{nodeType}} node)
                    {
                        var own = node;
                        var materialized = own.Materialize(default(ChainOptions));
                        return new(global::Cistern.ValueLinq.Helper.CreatePull<{{materialized}}, {{enumeratorFieldType}}>(in materialized));
                    }

                    static bool IPullEnumerator<{{outName}}>.RebuildChanges => false;

                    static bool IPullEnumerator<{{outName}}>.PreferRebuilt => false;

                    static TBuilt IPullEnumerator<{{outName}}>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
                        => global::Cistern.ValueLinq.Chain.ChainBuilder.Identity<{{outName}}, {{nodeType}}, {{enumeratorType}}, TOwnNode, TBuilt, TBuilder>(in node, in builder);

                    public void Dispose() => {{model.EnumeratorField}}.Dispose();

                    public bool MoveNext() => {{model.EnumeratorField}}.MoveNext();

                    public {{outName}} Current => {{model.EnumeratorField}}.Current;

            """);

        model.Requests.Failure ??= PullRequests.Emit(source, model.EnumeratorSymbol, outName);

        source.Append($$"""
                }

            """);
    }
}
