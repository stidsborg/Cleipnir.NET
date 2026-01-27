using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Cleipnir.Flows.SourceGenerator.Utils;

namespace Cleipnir.Flows.SourceGenerator
{
    [Generator]
    public class FlowsSourceGenerator : IIncrementalGenerator
    {
        private const string ParamlessFlowType = "Cleipnir.Flows.Flow";
        private const string UnitFlowType = "Cleipnir.Flows.Flow`1";
        private const string ResultFlowType = "Cleipnir.Flows.Flow`2";
        private const string IgnoreAttribute = "Cleipnir.Flows.SourceGeneration.Ignore";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
                "Cleipnir.Flows.GenerateFlowsAttribute",
                (node, ctx) => node is ClassDeclarationSyntax,
                transform: (ctx, _) => Transform(ctx)
            ).Where(flowInformation => flowInformation is not null);

            context.RegisterSourceOutput(
                provider,
                (ctx, generatedFlowInformation) =>
                    ctx.AddSource(generatedFlowInformation!.FileName, generatedFlowInformation.GeneratedCode)
            );
        }

        private static GeneratedFlowInformation? Transform(GeneratorAttributeSyntaxContext context)
        {
            var compilation = context.SemanticModel.Compilation;
            var paramlessFlowTypeSymbol = compilation.GetTypeByMetadataName(ParamlessFlowType);
            var unitFlowTypeSymbol = compilation.GetTypeByMetadataName(UnitFlowType);
            var resultFlowTypeSymbol = compilation.GetTypeByMetadataName(ResultFlowType);
            var ignoreAttributeSymbol = compilation.GetTypeByMetadataName(IgnoreAttribute);

            if (paramlessFlowTypeSymbol == null || unitFlowTypeSymbol == null || resultFlowTypeSymbol == null)
                return null;

            var classDeclaration = (ClassDeclarationSyntax) context.TargetNode;
            if (classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
                return null;

            var accessibilityModifier = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))
                ? "public"
                : "internal";

            var semanticModel = context.SemanticModel;
            var flowType = (INamedTypeSymbol?)semanticModel.GetDeclaredSymbol(classDeclaration);

            if (flowType == null ||
                (!InheritsFromParamlessFlowType(flowType, paramlessFlowTypeSymbol) &&
                 !InheritsFromFlowType(flowType, unitFlowTypeSymbol) &&
                 !InheritsFromFlowType(flowType, resultFlowTypeSymbol)))
                return null;

            if (flowType.ContainingType != null || flowType.IsFileLocal)
                return null;

            if (ignoreAttributeSymbol != null)
            {
                var hasIgnoreAttribute = flowType
                    .GetAttributes()
                    .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, ignoreAttributeSymbol));
                if (hasIgnoreAttribute)
                    return null;
            }

            var baseType = flowType.BaseType;
            if (baseType == null)
                return null;

            if (InheritsFromParamlessFlowType(flowType, paramlessFlowTypeSymbol))
                return GenerateCode(
                    new FlowInformation(
                        flowType,
                        paramType: null,
                        parameterName: null,
                        resultType: null,
                        paramless: true,
                        accessibilityModifier
                    )
                );

            var baseTypeTypeArguments = baseType.TypeArguments;
            var paramType = baseTypeTypeArguments.Length > 0
                ? baseTypeTypeArguments[0] as INamedTypeSymbol
                : null;
            var resultType = baseTypeTypeArguments.Length == 2
                ? baseTypeTypeArguments[1] as INamedTypeSymbol
                : null;

            var runMethod = flowType.GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => m.Name == "Run" && m.IsOverride);

            if (runMethod == null || runMethod.Parameters.Length == 0)
                return null;

            var parameterName = runMethod.Parameters[0].Name;

            return
                GenerateCode(
                    new FlowInformation(
                        flowType,
                        paramType,
                        parameterName,
                        resultType,
                        paramless: false,
                        accessibilityModifier
                    )
                );
        }

        private static GeneratedFlowInformation GenerateCode(FlowInformation flowInformation)
        {
            var flowsName = $"{flowInformation.FlowTypeSymbol.Name}s";
            var flowsNamespace = GetNamespace(flowInformation.FlowTypeSymbol);
            var flowType = GetFullyQualifiedName(flowInformation.FlowTypeSymbol);
            var flowName = flowInformation.FlowTypeSymbol.Name;
            var paramType = flowInformation.ParamTypeSymbol == null
                ? null
                : GetFullyQualifiedName(flowInformation.ParamTypeSymbol);
            var resultType = flowInformation.ResultTypeSymbol != null
                ? GetFullyQualifiedName(flowInformation.ResultTypeSymbol)
                : null;

            var accessibilityModifier = flowInformation.AccessibilityModifier;

            string generatedCode;
            if (flowInformation.Paramless)
            {
                generatedCode =
$@"namespace {flowsNamespace}
{{
    #nullable enable
    [Cleipnir.Flows.SourceGeneration.SourceGeneratedFlowsAttribute]
    {accessibilityModifier} class {flowsName} : Cleipnir.Flows.Flows<{flowType}>
    {{
        public {flowsName}(Cleipnir.Flows.FlowsContainer flowsContainer, string flowName = ""{flowName}"", Cleipnir.Flows.FlowOptions? options = null)
            : base(flowName, flowsContainer, options) {{ }}
    }}
    #nullable restore
}}";
            }
            else if (resultType == null)
            {
                generatedCode =
$@"namespace {flowsNamespace}
{{
    #nullable enable
    [Cleipnir.Flows.SourceGeneration.SourceGeneratedFlowsAttribute]
    {accessibilityModifier} class {flowsName} : Cleipnir.Flows.Flows<{flowType}, {paramType}>
    {{
        public {flowsName}(Cleipnir.Flows.FlowsContainer flowsContainer, string flowName = ""{flowName}"", Cleipnir.Flows.FlowOptions? options = null)
            : base(flowName, flowsContainer, options) {{ }}
    }}
    #nullable restore
}}";
            }
            else
            {
                generatedCode =
$@"namespace {flowsNamespace}
{{
    #nullable enable
    [Cleipnir.Flows.SourceGeneration.SourceGeneratedFlowsAttribute]
    {accessibilityModifier} class {flowsName} : Cleipnir.Flows.Flows<{flowType}, {paramType}, {resultType}>
    {{
        public {flowsName}(Cleipnir.Flows.FlowsContainer flowsContainer, string flowName = ""{flowName}"", Cleipnir.Flows.FlowOptions? options = null)
            : base(flowName, flowsContainer, options) {{ }}
    }}
    #nullable restore
}}";
            }

            return new GeneratedFlowInformation(generatedCode, GetFileName(flowInformation));
        }
    }
}
