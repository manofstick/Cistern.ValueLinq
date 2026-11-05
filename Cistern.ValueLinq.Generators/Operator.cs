namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Cistern.ValueLinq.Generators.TextHelper;

// the plumbing of a hand-written operator: a static partial class holding its Node, Push and Pull partial structs, the node's [Propagate] fields mirrored into the forwarder and the enumerator
// state is whatever fields the forwarder and the enumerator declare for themselves, started by their partial Start once the configuration is in place
static class Operator
{
    sealed class Model
    {
        public string Container = "";
        public string Namespace = "";

        public string Node = "";
        public List<string> NodeParameters = new();
        public string NodeConstraints = "";
        public int Source;                          // the index of the source parameter among the node's
        public string SourceField = "";
        public ITypeSymbol In = null!;
        public ITypeSymbol Out = null!;
        public List<(ITypeSymbol Type, string Name, bool ByRef)> Configuration = new();    // every field other than the source
        public List<(ITypeSymbol Type, string Name, bool ByRef)> Propagated = new();       // those of them Push and Pull hold

        public string? Forwarder;                   // null for a node with no forwarder, which writes CreateViaPush itself
        public List<string> ForwarderParameters = new();
        public string? OptionsField;                // the forwarder's ChainOptions field, filled from the push's options
        public string Enumerator = "";
        public INamedTypeSymbol EnumeratorSymbol = null!;
        public List<string> EnumeratorParameters = new();
        public int SourceEnumeratorIndex;           // the index of the source enumerator parameter among the enumerator's
        public List<string> EnumeratorExtras = new();           // the enumerator's parameters beyond the node's and its source's
        public string EnumeratorExtraConstraints = "";          // their constraints, from the hand-written partial
        public bool EnumeratorDeclaresConstraints;              // then the generated partial declares none, and the node's parameters are matched by name

        public bool WritesConstructor, WritesPush, WritesGetResult, WritesPushDispose, WritesCreate, WritesPullDispose, WritesRebuild, WritesRebuildChanges;
        // WritesInitialise: the forwarder writes Initialise or AllowInitialise; BeginsPush: it writes a Begin,
        // its own setup
        public bool WritesInitialise, BeginsPush;

        public Requests.Model Requests = new();

        public string SourceNode => NodeParameters[Source];
        public string Next => ForwarderParameters[Source];
        public string SourceEnumerator => EnumeratorParameters[SourceEnumeratorIndex];

        public IReadOnlyDictionary<string, string> ToForwarder
        {
            get
            {
                var renames =
                    NodeParameters
                    .Select((parameter, index) => (parameter, index))
                    .ToDictionary(
                        pair => pair.parameter,
                        pair => ForwarderParameters[pair.index]
                    );

                return renames;
            }
        }

        public IReadOnlyDictionary<string, string> ToEnumerator
        {
            get
            {
                if (EnumeratorDeclaresConstraints)
                    return Types.NoRenames;

                var renames =
                    NodeParameters
                    .Select((parameter, index) => (parameter, index))
                    .ToDictionary(
                        pair => pair.parameter,
                        pair => EnumeratorParameters[enumeratorIndex(pair.index)]
                    );

                return renames;

                int enumeratorIndex(int nodeIndex)
                {
                    int shifted =
                        nodeIndex <= Source
                            ? nodeIndex
                            : nodeIndex + 1;

                    return shifted;
                }
            }
        }

        public string NodeType => Generic(Node, NodeParameters);
        public string ForwarderType => Generic(Forwarder!, ForwarderParameters);
        public string EnumeratorType => Generic(Enumerator, EnumeratorParameters);

        public List<string> NodeWith(string source)
        {
            var parameters =
                NodeParameters
                .Select((parameter, index) => index == Source ? source : parameter)
                .ToList();

            return parameters;
        }
    }

    public static GeneratedSource Generate(INamedTypeSymbol node, Compilation compilation)
    {
        var hintName =
            Types.HintName(node, "Operator");

        if (Build(node, out var error) is not {} model)
            return GeneratedSource.Error(hintName, error, node);

        var body =
            new StringBuilder();

        EmitNode(body, model);

        EmitEnumerator(body, model);

        if (model.Forwarder is {} forwarder)
        {
            var renames =
                model.ToForwarder;

            EmitForwarder(body, model);

            var constraints =
                ForwarderConstraints(model);

            // the push's options first, held by ref as the node's own struct configuration is
            var optionsParameter =
                model.OptionsField is null
                    ? []
                    : new[] { new FillParameter("global::Cistern.ValueLinq.ChainOptions", "options", true) };

            var fillParameters =
                optionsParameter
                .Concat(model.Propagated.Select(configuration => createFillParameter(configuration, renames)))
                .ToList();

            Filler.Emit(body, forwarder, "Filler", model.ForwarderParameters, constraints, model.Next, fillParameters);
        }

        var builder =
            ChainBuilderModel(model);

        // a node that leaves itself out of the rebuilt chain writes Rebuild itself, and has no builder
        if (model.WritesRebuild)
        {
            if (!model.WritesRebuildChanges)
                ChainBuilder.EmitRebuildChanges(body, builder);
        }
        else
        {
            ChainBuilder.EmitRebuild(body, builder);

            ChainBuilder.EmitBuilder(body, builder);
        }

        var source =
            new StringBuilder();

        var header =
            Generated.Header(model.Namespace);

        source.Append(header);

        source.Append($"{NewLine}static partial class {model.Container}{NewLine}{{");

        source.Append(body);

        source.Append($"}}{NewLine}");

        var generated =
            Requests.WithFailure(hintName, $"{source}", model.Requests, node);

        return generated;

        static FillParameter createFillParameter((ITypeSymbol Type, string Name, bool ByRef) configuration, IReadOnlyDictionary<string, string> renames)
        {
            var type =
                Types.Name(configuration.Type, renames);

            var name =
                configuration.Name.TrimStart('_');

            var fillParameter =
                new FillParameter(type, name, configuration.ByRef);

            return fillParameter;
        }
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

        int sourceIndex =
            node
            .TypeParameters
            .ToList()
            .FindIndex(typeParameter => typeParameter.Name == sourceNode.Name);

        var forwarder =
            container
            .GetTypeMembers("Push")
            .FirstOrDefault(type => type.Arity == node.Arity);

        int enumeratorArity = node.Arity + 1;

        // the node's parameters and its source's enumerator, and, where the hand-written partial declares its
        // constraints, any others it needs (an inner sequence's enumerator)
        var enumerator =
            container
            .GetTypeMembers("Pull")
            .FirstOrDefault(type => type.Arity >= enumeratorArity);
        if (enumerator is null)
        {
            error =
                $"{container.Name} has no Pull with at least {enumeratorArity} type parameters";

            return null;
        }

        var enumeratorDeclaresConstraints =
            Types.Constraints(enumerator.TypeParameters, Types.NoRenames).Length > 0;

        int sourceEnumeratorIndex;
        List<ITypeParameterSymbol> extras;
        if (enumeratorDeclaresConstraints)
        {
            var nodeParameterNames =
                node
                .TypeParameters
                .Select(typeParameter => typeParameter.Name)
                .ToList();

            var missing =
                nodeParameterNames
                .Where(name => enumerator.TypeParameters.All(typeParameter => typeParameter.Name != name))
                .ToList();
            if (missing is not [])
            {
                error =
                    $"{enumerator.Name} declares constraints, so names the node's parameters as the node does, but has no {missing[0]}";

                return null;
            }

            sourceEnumeratorIndex =
                enumerator
                .TypeParameters
                .ToList()
                .FindIndex(enumeratesSourceNode);
            if (sourceEnumeratorIndex < 0)
            {
                error =
                    $"{enumerator.Name} has no parameter constrained to INodeEnumerator<{sourceNode.Name}, ...>";

                return null;
            }

            extras =
                enumerator
                .TypeParameters
                .Where((typeParameter, index) =>
                    index != sourceEnumeratorIndex
                    && !nodeParameterNames.Contains(typeParameter.Name))
                .ToList();
        }
        else
        {
            if (enumerator.Arity != enumeratorArity)
            {
                error =
                    $"{enumerator.Name} has parameters beyond the node's and its source enumerator, so must declare its constraints";

                return null;
            }

            sourceEnumeratorIndex = sourceIndex + 1;
            extras = new();
        }

        var methods =
            node
            .GetMembers()
            .OfType<IMethodSymbol>()
            .ToList();

        var namespaceName =
            container
            .ContainingNamespace
            .ToDisplayString();

        var sourceNodeInterface =
            Types.Find(sourceNode, "Cistern.ValueLinq.INode`1")!;

        var configuration =
            Types
            .InstanceFields(node)
            .Where(field => field.Name != sourceField.Name)
            .Select(describe)
            .ToList();

        var propagated =
            Types
            .InstanceFields(node)
            .Where(isPropagated)
            .Select(describe)
            .ToList();

        var writesConstructor =
            node
            .InstanceConstructors
            .Any(constructor => constructor is { IsImplicitlyDeclared: false, Parameters.Length: > 0 });

        var writesPush =
            methods.Any(method => method.Name.EndsWith("CreateViaPush"));

        // a node that pushes through its source in its place (Options) has no forwarder, and writes CreateViaPush itself
        if (forwarder is null && !writesPush)
        {
            error =
                $"{container.Name} has no Push with {node.Arity} type parameters";

            return null;
        }

        var forwarderMethods =
            forwarder?
            .GetMembers()
            .OfType<IMethodSymbol>()
            .ToList()
            ?? new();

        // a forwarder with elements to push after its source's writes its own GetResult; one that may hand its chain to
        // a sink writes the Dispose that knows which holds it
        var writesGetResult =
            forwarderMethods.Any(method => method.Name == "GetResult");

        var writesPushDispose =
            forwarderMethods.Any(method => method.Name == "Dispose");

        var writesInitialise =
            forwarderMethods.Any(method => method.Name is "Initialise" or "AllowInitialise");

        var beginsPush =
            forwarderMethods.Any(method => method.Name == "Begin");

        // a forwarder that pushes other sequences itself keeps the push's options to push them with
        var optionsFields =
            forwarder is null
                ? new()
                : Types
                  .InstanceFields(forwarder)
                  .Where(field =>
                      field.Type is INamedTypeSymbol named
                      && Types.MetadataName(named) == "Cistern.ValueLinq.ChainOptions")
                  .ToList();
        if (optionsFields.Count > 1)
        {
            error =
                $"{forwarder!.Name} has more than one ChainOptions field";

            return null;
        }

        // an enumerator that does not always create its source writes its own Create, and the Dispose that knows
        var enumeratorMethods =
            enumerator
            .GetMembers()
            .OfType<IMethodSymbol>()
            .ToList();

        var writesCreate =
            enumeratorMethods.Any(method => method.Name.EndsWith("Create"));

        var writesPullDispose =
            enumeratorMethods.Any(method => method.Name == "Dispose");

        var writesRebuild =
            enumeratorMethods.Any(method => method.Name.EndsWith("Rebuild"));

        var writesRebuildChanges =
            enumerator
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Any(property => property.Name.EndsWith("RebuildChanges"));

        // a member written in place of a generated one names its type parameters as the generated one does, so the
        // generated code never collides with a node's own and every operator reads the same
        var misnamed =
            Misnamed(node, methods, "CreateViaPush", ["TPushResult", "TPushEnumerator", "TPushFiller"])
            ?? Misnamed(forwarder, forwarderMethods, "GetResult", ["TPushResult"])
            ?? Misnamed(enumerator, enumeratorMethods, "Rebuild", ["TOwnNode", "TBuilt", "TBuilder"])
            ?? Reserved(node);
        if (misnamed is not null)
        {
            error = misnamed;
            return null;
        }

        var requests =
            Requests.Read(node);

        var model =
            new Model
            {
                Container = container.Name,
                Namespace = namespaceName,
                Node = node.Name,
                NodeParameters = parameterNames(node),
                NodeConstraints = Types.Constraints(node.TypeParameters, Types.NoRenames),
                Source = sourceIndex,
                SourceField = sourceField.Name,
                In = sourceNodeInterface.TypeArguments[0],
                Out = output.TypeArguments[0],
                Configuration = configuration,
                Propagated = propagated,
                Forwarder = forwarder?.Name,
                ForwarderParameters = forwarder is null ? new() : parameterNames(forwarder),
                OptionsField = optionsFields.SingleOrDefault()?.Name,
                Enumerator = enumerator.Name,
                EnumeratorSymbol = enumerator,
                EnumeratorParameters = parameterNames(enumerator),
                SourceEnumeratorIndex = sourceEnumeratorIndex,
                EnumeratorExtras = extras.Select(typeParameter => typeParameter.Name).ToList(),
                EnumeratorExtraConstraints = Types.Constraints(extras, Types.NoRenames),
                EnumeratorDeclaresConstraints = enumeratorDeclaresConstraints,
                WritesConstructor = writesConstructor,
                WritesPush = writesPush,
                WritesGetResult = writesGetResult,
                WritesPushDispose = writesPushDispose,
                WritesInitialise = writesInitialise,
                BeginsPush = beginsPush,
                WritesCreate = writesCreate,
                WritesPullDispose = writesPullDispose,
                WritesRebuild = writesRebuild,
                WritesRebuildChanges = writesRebuildChanges,
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

        bool enumeratesSourceNode(ITypeParameterSymbol typeParameter)
        {
            var nodeEnumerator =
                Types.Find(typeParameter, "Cistern.ValueLinq.INodeEnumerator`2");

            var enumerates =
                nodeEnumerator is { TypeArguments: [ITypeParameterSymbol enumerated, _] }
                && enumerated.Name == sourceNode.Name;

            return enumerates;
        }

        static bool isPropagated(IFieldSymbol field)
        {
            var propagates =
                Types.Propagate(field) is not null;

            return propagates;
        }

        static (ITypeSymbol Type, string Name, bool ByRef) describe(IFieldSymbol field)
        {
            var byRef =
                Types.IsPropagatedByRef(field);

            (ITypeSymbol Type, string Name, bool ByRef) described =
                (field.Type, field.Name, byRef);

            return described;
        }

        static List<string> parameterNames(INamedTypeSymbol type)
        {
            var names =
                type
                .TypeParameters
                .Select(typeParameter => typeParameter.Name)
                .ToList();

            return names;
        }
    }

    // the names the generated members use for their type parameters, and so the names a node's own never take
    private static readonly string[] ReservedPrefixes = ["TPush", "TRequest"];
    private static readonly string[] ReservedNames = ["TOwnNode", "TBuilt", "TBuilder", "TNode", "TEnumerator"];

    // the error for a hand-written member whose type parameters are not named as the generated one's, or null
    public static string? Misnamed(INamedTypeSymbol? owner, IReadOnlyList<IMethodSymbol> methods, string name, string[] expected)
    {
        var method =
            methods.FirstOrDefault(candidate => candidate.Name.EndsWith(name));
        if (method is null)
            return null;

        var names =
            method
            .TypeParameters
            .Select(typeParameter => typeParameter.Name)
            .ToList();
        if (names.SequenceEqual(expected))
            return null;

        var written =
            string.Join(", ", names);

        var required =
            string.Join(", ", expected);

        var error =
            $"{owner!.Name}.{name} names its type parameters <{written}>; every operator writes it as <{required}>";

        return error;
    }

    // the error for a node whose own type parameter takes a name the generated members use, or null
    public static string? Reserved(INamedTypeSymbol node)
    {
        var reserved =
            node
            .TypeParameters
            .Select(typeParameter => typeParameter.Name)
            .FirstOrDefault(name =>
                ReservedNames.Contains(name)
                || ReservedPrefixes.Any(prefix => name.StartsWith(prefix)));
        if (reserved is null)
            return null;

        var error =
            $"{node.Name}'s type parameter {reserved} is a name the generated members use; a node's type parameters are never {string.Join(", ", ReservedNames)} and never start with {string.Join(" or ", ReservedPrefixes)}";

        return error;
    }

    private static string Parameter(string fieldName) => fieldName.TrimStart('_');

    private static string Generic(string name, IEnumerable<string> arguments)
    {
        var joined =
            string.Join(", ", arguments);

        var type =
            $"{name}<{joined}>";

        return type;
    }

    private static string ForwarderConstraints(Model model)
    {
        var renames =
            model.ToForwarder;

        var others =
            model.NodeConstraints is []
                ? ""
                : Rename(model.NodeConstraints, model.NodeParameters, renames, model.SourceNode);

        var outName =
            Types.Name(model.Out, renames);

        var constraint =
            $"{NewLine}        where {model.Next} : IPushEnumerator<{outName}>";

        var constraints =
            $"{others}{constraint}";

        return constraints;
    }

    // the node's constraints in another struct's parameter names, less the source's own constraint
    public static string Rename(string constraints, IReadOnlyList<string> parameters, IReadOnlyDictionary<string, string> renames, string drop)
    {
        var separators = 
            new[] { NewLine };

        var dropped =
            $"where {drop} :";

        var alternatives =
            string.Join("|", parameters);

        var pattern =
            $@"\b({alternatives})\b";

        var renamedLines =
            constraints
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Where(isKept)
            .Select(renameLine);

        var prefixedLines =
            renamedLines.Select(prefixLine);

        var renamed = string.Concat(prefixedLines);

        return renamed;

        bool isKept(string line)
        {
            var trimmed =
                line.TrimStart();

            var kept =
                !trimmed.StartsWith(dropped);

            return kept;
        }

        string renameLine(string line)
        {
            var renamedLine =
                Regex.Replace(line, pattern, renameMatch);

            return renamedLine;
        }

        string renameMatch(Match match)
        {
            var renamedMatch =
                Types.Rename(match.Value, renames);

            return renamedMatch;
        }

        static string prefixLine(string line)
        {
            var prefixed =
                $"{NewLine}{line}";

            return prefixed;
        }
    }

    private static ChainBuilderModel ChainBuilderModel(Model model)
    {
        var builderParameters =
            model
            .NodeParameters
            .Where((_, index) => index != model.Source)
            .Concat(model.EnumeratorExtras)
            .Concat(["TBuilt", "TBuilder"])
            .ToList();

        var nodeConstraints =
            model.NodeConstraints is []
                ? ""
                : Rename(model.NodeConstraints, model.NodeParameters, Types.NoRenames, model.SourceNode);

        var builderConstraints =
            $"{nodeConstraints}{model.EnumeratorExtraConstraints}";

        var fields =
            model
            .Configuration
            .Select(createField)
            .ToList();

        var nodeWithTNode = model.NodeWith("TNode");

        var nodeOverTNode = Generic(model.Node, nodeWithTNode);

        var enumeratorArguments =
            model
            .EnumeratorParameters
            .Select(overTNode);

        var enumeratorOverTNode = Generic(model.Enumerator, enumeratorArguments);

        var builder =
            new ChainBuilderModel
            {
                EnumeratorName = model.Enumerator,
                EnumeratorParameters = model.EnumeratorParameters,
                NodeType = model.NodeType,
                SourceNode = model.SourceNode,
                SourceEnumerator = model.SourceEnumerator,
                SourceField = model.SourceField,
                In = Types.Name(model.In),
                Out = Types.Name(model.Out),
                BuilderName = "ChainBuilder",
                BuilderParameters = builderParameters,
                BuilderConstraints = builderConstraints,
                Fields = fields,
                NodeOverTNode = nodeOverTNode,
                EnumeratorOverTNode = enumeratorOverTNode,
            };

        return builder;

        static (string Type, string Name, bool ByRef) createField((ITypeSymbol Type, string Name, bool ByRef) configuration)
        {
            var type =
                Types.Name(configuration.Type);

            (string Type, string Name, bool ByRef) created =
                (type, configuration.Name, configuration.ByRef);

            return created;
        }

        string overTNode(string parameter)
        {
            var renamed =
                parameter switch
                {
                    _ when parameter == model.SourceNode => "TNode",
                    _ when parameter == model.SourceEnumerator => "TEnumerator",
                    var other => other,
                };

            return renamed;
        }
    }

    private static void EmitNode(StringBuilder source, Model model)
    {
        source.Append($$"""

                partial struct {{model.NodeType}}
                {
            """);

        if (!model.WritesConstructor)
            EmitConstructor(source, model.Node, model.SourceNode, model.SourceField, model.Configuration);

        Requests.Emit(source, model.Requests);

        if (!model.WritesPush)
        {
            var forwarderArguments =
                model.NodeWith("TPushEnumerator");

            var forwarder =
                Generic(model.Forwarder!, forwarderArguments);

            var fillerArguments =
                forwarderArguments.Concat(["TPushFiller"]);

            var filler =
                Generic("Filler", fillerArguments);

            var outName =
                Types.Name(model.Out);

            var propagatedNames =
                model
                .Propagated
                .Select(nameWithSeparator);

            var propagatedArguments = string.Join("", propagatedNames);

            var optionsArgument =
                model.OptionsField is null
                    ? ""
                    : "in options, ";

            source.Append($$"""

                        TPushResult INode<{{outName}}>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler) =>
                            {{model.SourceField}}.CreateViaPush<TPushResult, {{forwarder}}, {{filler}}>(in options, new {{filler}}({{optionsArgument}}{{propagatedArguments}}in filler));

                """);
        }

        source.Append($"    }}{NewLine}");

        static string nameWithSeparator((ITypeSymbol Type, string Name, bool ByRef) configuration)
        {
            var named =
                $"{configuration.Name}, ";

            return named;
        }
    }

    // the node's constructor: its source, then each configuration field, as parameters
    public static void EmitConstructor(StringBuilder source, string node, string sourceNode, string sourceField, IReadOnlyList<(ITypeSymbol Type, string Name, bool ByRef)> configuration)
    {
        var sourceParameter = Parameter(sourceField);

        var declaredConfiguration =
            configuration
            .Select(declareConfiguration);

        var configurationParameters =
            string.Join("", declaredConfiguration);

        source.Append($$"""

                    public {{node}}(in {{sourceNode}} {{sourceParameter}}{{configurationParameters}})
                    {
                        {{sourceField}} = {{sourceParameter}};

            """);

        foreach (var (_, name, _) in configuration)
        {
            var parameter = Parameter(name);

            source.Append($"            {name} = {parameter};{NewLine}");
        }

        source.Append($"        }}{NewLine}");

        static string declareConfiguration((ITypeSymbol Type, string Name, bool ByRef) configuration)
        {
            var type =
                Types.Name(configuration.Type);

            var parameter =
                Parameter(configuration.Name);

            var modifier =
                Modifier(configuration.ByRef);

            var declared =
                $", {modifier}{type} {parameter}";

            return declared;
        }
    }

    private static void EmitEnumerator(StringBuilder source, Model model)
    {
        var renames = model.ToEnumerator;

        var fields =
            model
            .Propagated
            .Select(qualifyField)
            .ToList();

        var outName =
            Types.Name(model.Out, renames);

        var inName =
            Types.Name(model.In, renames);

        // the partials of a struct that declare constraints declare the same ones, so where the hand-written one does,
        // this one declares none
        var constraints =
            model.EnumeratorDeclaresConstraints
                ? ""
                : $"{nodeConstraints()}{NewLine}        where {model.SourceEnumerator} : struct, IPullEnumerator<{inName}>, INodeEnumerator<{model.SourceNode}, {model.SourceEnumerator}>";

        source.Append($$"""

                // its fields are split between this declaration and the hand-written one, so their order is the runtime's to choose
                [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Auto)]
                partial struct {{model.EnumeratorType}}
                    : IPullEnumerator<{{outName}}>
                    , INodeEnumerator<{{model.NodeType}}, {{model.EnumeratorType}}>{{constraints}}
                {
                    private {{model.SourceEnumerator}} _enumerator;

            """);

        foreach (var (type, name, _) in fields)
            source.Append($"        private {type} {name};{NewLine}");

        var declaredFields =
            fields.Select(declareField);

        var constructorParameters =
            string.Join("", declaredFields);

        var readFields =
            fields.Select(readNodeField);

        var createArguments =
            string.Join("", readFields);

        source.Append($$"""

                    // starts whatever state the enumerator keeps, once its configuration is in place
                    partial void Start();

                    private {{model.Enumerator}}(in {{model.SourceEnumerator}} enumerator{{constructorParameters}})
                    {
                        _enumerator = enumerator;

            """);

        foreach (var (_, name, _) in fields)
        {
            var parameter = Parameter(name);
            source.Append($"            {name} = {parameter};{NewLine}");
        }

        source.Append($$"""
                        Start();
                    }

            """);

        if (!model.WritesCreate)
        {
            source.Append($$"""

                        static {{model.EnumeratorType}} INodeEnumerator<{{model.NodeType}}, {{model.EnumeratorType}}>.Create(in {{model.NodeType}} node) =>
                            new({{model.SourceEnumerator}}.Create(in node.{{model.SourceField}}){{createArguments}});

                """);
        }

        if (!model.WritesPullDispose)
        {
            source.Append("""

                        public void Dispose() => _enumerator.Dispose();

                """);
        }

        model.Requests.Failure ??= PullRequests.Emit(source, model.EnumeratorSymbol, outName);

        source.Append($$"""
                }

            """);

        string nodeConstraints()
        {
            var renamed =
                model.NodeConstraints is []
                    ? ""
                    : Rename(model.NodeConstraints, model.NodeParameters, renames, "");

            return renamed;
        }

        (string Type, string Name, bool ByRef) qualifyField((ITypeSymbol Type, string Name, bool ByRef) propagated)
        {
            var type =
                Types.Name(propagated.Type, renames);

            (string Type, string Name, bool ByRef) qualified = (type, propagated.Name, propagated.ByRef);

            return qualified;
        }

        static string declareField((string Type, string Name, bool ByRef) field)
        {
            var parameter = Parameter(field.Name);

            var modifier = Modifier(field.ByRef);

            var declared = $", {modifier}{field.Type} {parameter}";

            return declared;
        }

        static string readNodeField((string Type, string Name, bool ByRef) field)
        {
            var modifier = Modifier(field.ByRef);

            var read = $", {modifier}node.{field.Name}";

            return read;
        }
    }

    // a struct configuration is passed by in, to be held by ref or copied once into the forwarder or enumerator
    private static string Modifier(bool byRef) => byRef ? "in " : "";

    private static void EmitForwarder(StringBuilder source, Model model)
    {
        var renames = model.ToForwarder;

        var fields =
            model
            .Propagated
            .Select(qualifyField)
            .ToList();

        var inName =
            Types.Name(model.In, renames);

        var constraints =
            ForwarderConstraints(model);

        var optionsParameter =
            model.OptionsField is null
                ? []
                : new[] { "in global::Cistern.ValueLinq.ChainOptions options" };

        var declaredFields =
            optionsParameter
            .Concat(fields.Select(declareFillParameter));

        var fillParameters = string.Join(", ", declaredFields);

        source.Append($$"""

                // its fields are split between this declaration and the hand-written one, so their order is the runtime's to choose
                [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Auto)]
                partial struct {{model.ForwarderType}}
                    : IPushEnumerator<{{inName}}>{{constraints}}
                {
                    internal {{model.Next}} _next;

            """);

        foreach (var (type, name, _) in fields)
            source.Append($"        private {type} {name};{NewLine}");

        source.Append($$"""

                    // starts whatever state the forwarder keeps, once its configuration is in place; the slot it is filled into is zeroed
                    partial void Start();

                    // writes this forwarder's own fields into the chain being filled, and hands back the slot for its downstream
                    [global::System.Diagnostics.CodeAnalysis.UnscopedRef]
                    internal ref {{model.Next}} Fill({{fillParameters}})
                    {

            """);

        if (model.OptionsField is {} optionsField)
            source.Append($"            {optionsField} = options;{NewLine}");

        foreach (var (_, name, _) in fields)
        {
            var parameter = Parameter(name);

            source.Append($"            {name} = {parameter};{NewLine}");
        }

        source.Append($$"""
                        Start();
                        return ref _next;
                    }

            """);

        if (!model.WritesPushDispose)
        {
            source.Append("""

                        public void Dispose() => _next.Dispose();

                """);
        }

        if (!model.WritesGetResult)
        {
            source.Append("""
                        public TPushResult GetResult<TPushResult>() => _next.GetResult<TPushResult>();

                """);
        }

        // a forwarder's own setup that may run user code is its Begin, run once by Initialise and never by filling
        if (!model.WritesInitialise)
        {
            var allowInitialise =
                model.BeginsPush
                    ? "true"
                    : $"{model.Next}.AllowInitialise";

            var begin =
                model.BeginsPush
                    ? $"            Begin();{NewLine}"
                    : "";

            source.Append($$"""

                        public static bool AllowInitialise => {{allowInitialise}};

                        public void Initialise()
                        {
                {{begin}}            if ({{model.Next}}.AllowInitialise)
                                _next.Initialise();
                        }

                """);
        }

        source.Append("""
                }

            """);

        (string Type, string Name, bool ByRef) qualifyField((ITypeSymbol Type, string Name, bool ByRef) propagated)
        {
            var type =
                Types.Name(propagated.Type, renames);

            (string Type, string Name, bool ByRef) qualified =
                (type, propagated.Name, propagated.ByRef);

            return qualified;
        }

        static string declareFillParameter((string Type, string Name, bool ByRef) field)
        {
            var parameter =
                Parameter(field.Name);

            var modifier =
                Modifier(field.ByRef);

            var declared =
                $"{modifier}{field.Type} {parameter}";

            return declared;
        }
    }
}
