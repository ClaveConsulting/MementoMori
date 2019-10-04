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

namespace Clave.MementoMori
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TodoBeforeCodeFixProvider)), Shared]
    public class TodoBeforeCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Snooze";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TodoBeforeAnalyzer.ErrorId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var syntaxToken = root.FindToken(diagnosticSpan.Start);
            var trivia = syntaxToken.Parent.FindTrivia(diagnosticSpan.Start);

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => Snooze(context.Document, root, trivia),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static Task<Document> Snooze(Document contextDocument, SyntaxNode root, SyntaxTrivia trivia)
        {
            var newComment = TodoBeforeAnalyzer.Regex.Replace(trivia.ToFullString(), $"{DateTime.Now.AddDays(1):yyyy-MM-dd}");

            var newTrivia = root.ReplaceTrivia(trivia, SyntaxFactory.Comment(newComment));

            return Task.FromResult(contextDocument.WithSyntaxRoot(newTrivia));
        }
    }
}
