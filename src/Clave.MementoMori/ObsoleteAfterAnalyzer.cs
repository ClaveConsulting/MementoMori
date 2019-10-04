using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Clave.MementoMori
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ObsoleteAfterAnalyzer : DiagnosticAnalyzer
    {
        public static readonly Regex Regex = new Regex(@"(?<=after\s)(\d\d\d\d-\d\d-\d\d)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public const string ErrorId = "ObsoleteAfterError";
        public const string InfoId = "ObsoleteAfterInfo";

        private static readonly DiagnosticDescriptor ErrorRule = new DiagnosticDescriptor(
            id: ErrorId,
            title: "This is obsolete.",
            messageFormat: "This became obsolete on {0}.",
            category: "Technical debt",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Fix technical debt before the deadline.");

        private static readonly DiagnosticDescriptor InfoRule = new DiagnosticDescriptor(
            id: InfoId,
            title: "This will become obsolete.",
            messageFormat: "This will become obsolete after {0}.",
            category: "Technical debt",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Fix technical debt before the deadline.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ErrorRule, InfoRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(
                AnalyzeAttribute,
                SymbolKind.Method,
                SymbolKind.Field,
                SymbolKind.Event,
                SymbolKind.NamedType,
                SymbolKind.Property);
        }

        private static void AnalyzeAttribute(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol;

            var obsoleteAttributeType = context.Compilation.GetTypeByMetadataName("System.ObsoleteAttribute");

            var obsoleteAttributeData = symbol
                .GetAttributes()
                .FirstOrDefault(a => a.AttributeClass.Equals(obsoleteAttributeType));

            if (obsoleteAttributeData == null)
                return;

            var message = obsoleteAttributeData.ConstructorArguments
                .FirstOrDefault(a => a.Kind == TypedConstantKind.Primitive && a.Type.SpecialType == SpecialType.System_String);

            if (message.IsNull)
                return;

            AnalyzeMessage(context, obsoleteAttributeData.ApplicationSyntaxReference.GetSyntax().GetLocation(), message.Value as string);
        }

        private static void AnalyzeMessage(SymbolAnalysisContext context, Location location, string commentText)
        {
            if (!(MatchAfterDate(commentText) is string date)) return;
            if (!DateTime.TryParse(date, out var deadline)) return;

            var diagnostic = Diagnostic.Create(
                deadline < DateTime.Now.Date ? ErrorRule : InfoRule,
                location,
                date);

            context.ReportDiagnostic(diagnostic);
        }

        private static string MatchAfterDate(string commentText)
        {
            var match = Regex.Match(commentText);
            if (!match.Success) return null;

            return match.Groups[1].Value;

        }
    }
}
