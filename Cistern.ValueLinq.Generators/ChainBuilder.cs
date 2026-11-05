namespace Cistern.ValueLinq.Generators;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Cistern.ValueLinq.Generators.TextHelper;

sealed class ChainBuilderModel
{
    public string EnumeratorName = "";
    public List<string> EnumeratorParameters = new();
    public string NodeType = "";                    // the node the enumerator enumerates, in the enumerator's parameters
    public string SourceNode = "";
    public string SourceEnumerator = "";
    public string SourceField = "";
    public string In = "";
    public string Out = "";
    public string BuilderName = "";
    public List<string> BuilderParameters = new();  // the enumerator's other parameters, then TBuilt and TBuilder
    public string BuilderConstraints = "";
    public List<(string Type, string Name, bool ByRef)> Fields = new();    // a struct configuration is held by ref readonly
    public string NodeOverTNode = "";
    public string EnumeratorOverTNode = "";
}

// a single-source pull enumerator's pass-through Rebuild, and the builder it hands its source, which rebuilds
// the node over whatever node comes back up from the node's fields other than its source
static class ChainBuilder
{
    // the pass-through alone, for an enumerator that writes its own Rebuild
    public static void EmitRebuildChanges(StringBuilder source, ChainBuilderModel model)
    {
        var enumeratorArguments =
            string.Join(", ", model.EnumeratorParameters);

        source.Append($$"""

                partial struct {{model.EnumeratorName}}<{{enumeratorArguments}}>
                {
                    static bool IPullEnumerator<{{model.Out}}>.RebuildChanges => {{model.SourceEnumerator}}.RebuildChanges;

                    static bool IPullEnumerator<{{model.Out}}>.PreferRebuilt => {{model.SourceEnumerator}}.PreferRebuilt;
                }

            """);
    }

    public static void EmitRebuild(StringBuilder source, ChainBuilderModel model)
    {
        var builderArguments =
            string.Join(", ", model.BuilderParameters);

        var builder =
            $"{model.BuilderName}<{builderArguments}>";

        var enumeratorArguments = 
            string.Join(", ", model.EnumeratorParameters);

        var ownFields =
            model
            .Fields
            .Select(readOwnField);

        var ownFieldArguments = string.Join("", ownFields);

        source.Append($$"""

                partial struct {{model.EnumeratorName}}<{{enumeratorArguments}}>
                {
                    static bool IPullEnumerator<{{model.Out}}>.RebuildChanges => {{model.SourceEnumerator}}.RebuildChanges;

                    static bool IPullEnumerator<{{model.Out}}>.PreferRebuilt => {{model.SourceEnumerator}}.PreferRebuilt;

                    static TBuilt IPullEnumerator<{{model.Out}}>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
                    {
                        ref readonly var own = ref global::Cistern.ValueLinq.Chain.ChainBuilder.As<TOwnNode, {{model.NodeType}}>(in node);
                        var above = new {{builder}}({{ownFieldArguments}}in builder);
                        return {{model.SourceEnumerator}}.Rebuild<{{model.SourceNode}}, TBuilt, {{builder}}>(in own.{{model.SourceField}}, in above);
                    }
                }

            """);

        static string readOwnField((string Type, string Name, bool ByRef) field)
        {
            var modifier = Modifier(field.ByRef);
            var read = $"{modifier}own.{field.Name}, ";
            return read;
        }
    }

    public static void EmitBuilder(StringBuilder source, ChainBuilderModel model)
    {
        var builderArguments =
            string.Join(", ", model.BuilderParameters);

        source.Append($$"""

                internal readonly ref struct {{model.BuilderName}}<{{builderArguments}}>
                    : IChainBuilder<{{model.In}}, TBuilt>{{model.BuilderConstraints}}
                    where TBuilder : IChainBuilder<{{model.Out}}, TBuilt>, allows ref struct
                {

            """);

        foreach (var (type, name, byRef) in model.Fields)
        {
            var modifier = byRef ? "ref readonly " : "";
            source.Append($"        private readonly {modifier}{type} {name};{NewLine}");
        }

        var declaredFields =
            model
            .Fields
            .Select(declareField);

        var constructorParameters = string.Join("", declaredFields);

        source.Append($$"""
                    private readonly ref readonly ErasedRef _builder;

                    public {{model.BuilderName}}({{constructorParameters}}in TBuilder builder)
                    {

            """);
        foreach (var (_, name, byRef) in model.Fields)
        {
            var parameter = Parameter(name);
            var modifier = byRef ? "ref " : "";
            source.Append($"            {name} = {modifier}{parameter};{NewLine}");
        }

        var sourceAssignment = 
            $"{model.SourceField} = node";

        var fieldAssignments =
            model
            .Fields
            .Select(assignField);

        var assignments =
            new[] { sourceAssignment }
            .Concat(fieldAssignments);

        var initializerBody = string.Join(", ", assignments);

        source.Append($$"""
                        _builder = ref Erase.Ref(in builder);
                    }

                    TBuilt IChainBuilder<{{model.In}}, TBuilt>.Build<TNode, TEnumerator>(in TNode node)
                        => Erase.Deref<TBuilder>(in _builder).Build<{{model.NodeOverTNode}}, {{model.EnumeratorOverTNode}}>(new {{model.NodeOverTNode}} { {{initializerBody}} });
                }

            """);

        static string declareField((string Type, string Name, bool ByRef) field)
        {
            var parameter = Parameter(field.Name);

            var modifier = Modifier(field.ByRef);

            var declared =
                $"{modifier}{field.Type} {parameter}, ";

            return declared;
        }

        static string assignField((string Type, string Name, bool ByRef) field)
        {
            var assignment =
                $"{field.Name} = {field.Name}";

            return assignment;
        }
    }

    private static string Parameter(string fieldName) => fieldName.TrimStart('_');

    private static string Modifier(bool byRef) => byRef ? "in " : "";
}
