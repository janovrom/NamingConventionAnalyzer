using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace NamingFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PrivateCodeFixProvider)), Shared]
    public class PrivateCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(PrivateConstAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            SyntaxToken token = root.FindToken(diagnosticSpan.Start);

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.PrivateTitle, c => FixPrivateField(context.Document, token, c),
                PrivateAnalyzer.DiagnosticId), diagnostic);
        }

        private async Task<Solution> FixPrivateField(Document document, SyntaxToken token, CancellationToken cancellationToken)
        {
            string newName = token.ValueText;
            if (!token.ValueText.StartsWith("_"))
            {
                newName = $"_{char.ToLowerInvariant(token.ValueText[0])}{token.ValueText.Substring(1)}";
            }
            else
            {
                if (token.ValueText.Length > 2)
                    newName = $"_{char.ToLowerInvariant(token.ValueText[1])}{token.ValueText.Substring(2)}";
                else
                    newName = $"_{char.ToLowerInvariant(token.ValueText[1])}";
            }

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var symbol = semanticModel.GetDeclaredSymbol(token.Parent, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(originalSolution, symbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}
