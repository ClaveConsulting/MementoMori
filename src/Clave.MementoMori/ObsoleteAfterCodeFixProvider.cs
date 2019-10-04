using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Clave.MementoMori
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ObsoleteAfterCodeFixProvider)), Shared]
    public class ObsoleteAfterCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Snooze";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ObsoleteAfterAnalyzer.ErrorId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            if (root.FindNode(diagnosticSpan) is AttributeSyntax attributeSyntax)
            {
                if (attributeSyntax.ArgumentList.Arguments.FirstOrDefault().Expression is LiteralExpressionSyntax messageArgument)
                {
                    // Register a code action that will invoke the fix.
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: Title,
                            createChangedDocument: c => Task.FromResult(context.Document.WithSyntaxRoot(Snooze(root, attributeSyntax, messageArgument.Token.Value as string))),
                            equivalenceKey: Title),
                        diagnostic);
                }
            }
        }

        private static SyntaxNode Snooze(SyntaxNode root, AttributeSyntax attributeSyntax, string message)
        {
            var newMessage = ObsoleteAfterAnalyzer.Regex.Replace(message, $"{DateTime.Now.AddDays(1):yyyy-MM-dd}");

            var newAttribute = attributeSyntax
                .WithArgumentList(SyntaxFactory.AttributeArgumentList())
                .AddArgumentListArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(newMessage))))
                .NormalizeWhitespace();

            return root.ReplaceNode(attributeSyntax, newAttribute);
        }
    }
}
