using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Clave.MementoMori
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TodoBeforeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly Regex Regex = new Regex(@"(?<=before\s)(\d\d\d\d-\d\d-\d\d)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public const string ErrorId = "TodoBeforeError";
        public const string InfoId = "TodoBeforeInfo";

        private static readonly DiagnosticDescriptor ErrorRule = new DiagnosticDescriptor(
            id: ErrorId,
            title: "TODO has not been done.",
            messageFormat: "This should have been done before {0}.",
            category: "Technical debt",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Fix technical debt before the deadline.");

        private static readonly DiagnosticDescriptor InfoRule = new DiagnosticDescriptor(
            id: InfoId,
            title: "TODO has to be done.",
            messageFormat: "Remember to do this before {0}.",
            category: "Technical debt",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Fix technical debt before the deadline.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ErrorRule, InfoRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(AnalyzeTree);
        }

        private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
        {
            var allTrivia = context.Tree
                .GetCompilationUnitRoot(context.CancellationToken)
                .DescendantTrivia();

            foreach (var trivia in allTrivia)
            {
                switch (trivia.Kind())
                {
                    case SyntaxKind.SingleLineCommentTrivia:
                        AnalyzeComment(context, trivia.GetLocation(), GetSingleLineCommentText(trivia.ToString()));
                        continue;

                    case SyntaxKind.MultiLineCommentTrivia:
                        AnalyzeComment(context, trivia.GetLocation(), GetMultiLineCommentText(trivia.ToString()));
                        continue;

                    default:
                        continue;
                }
            }
        }

        private static string GetMultiLineCommentText(string text)
        {
            return text.Substring(2, text.Length - 4).TrimStart();
        }

        private static string GetSingleLineCommentText(string text)
        {
            return text.TrimStart('/').TrimStart();
        }

        private static void AnalyzeComment(SyntaxTreeAnalysisContext context, Location location, string commentText)
        {
            if (!commentText.StartsWith("TODO", StringComparison.OrdinalIgnoreCase)) return;
            if (!(MatchBeforeDate(commentText) is string date)) return;
            if (!DateTime.TryParse(date, out var deadline)) return;

            var diagnostic = Diagnostic.Create(
                deadline < DateTime.Now.Date ? ErrorRule : InfoRule,
                location,
                date);

            context.ReportDiagnostic(diagnostic);
        }

        private static string MatchBeforeDate(string commentText)
        {
            var match = Regex.Match(commentText);
            if (!match.Success) return null;

            return match.Groups[1].Value;

        }
    }
}
