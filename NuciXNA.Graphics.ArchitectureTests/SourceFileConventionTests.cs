using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace NuciXNA.Graphics.ArchitectureTests
{
    /// <summary>
    /// Roslyn-based source conventions over the handwritten production, unit-test, and architecture-test sources.
    /// Constants are permitted in test sources because NUnit attribute arguments require compile-time values.
    /// </summary>
    [TestFixture]
    public sealed class SourceFileConventionTests
    {
        private const int MaximumParameterCount = 7;

        private const int MaximumConditionalLength = 72;

        private const int MaximumSignatureLength = 96;

        /// <summary>
        /// The documented public drawing API mirrors SpriteBatch.Draw and SpriteBatch.DrawString and cannot be
        /// regrouped without a breaking change to the NuciXNA.Graphics NuGet contract; these members are therefore
        /// the only narrowly scoped exemption from the parameter limit.
        /// </summary>
        private static readonly string[] ParameterLimitExemptions =
        [
            "TextureDrawer.Draw",
            "StringDrawer.Draw"
        ];

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingNamespaces_ThenEachFileUsesOneBlockNamespace(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                NamespaceDeclarationSyntax[] namespaces = [.. root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()];
                List<string> violations = [];

                if (root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Any())
                {
                    violations.Add($"{tree.FilePath}: Use a block namespace, not a file-scoped namespace.");
                }

                if (namespaces.Length != 1)
                {
                    violations.Add($"{tree.FilePath}: Declare exactly one block namespace.");
                }

                if (namespaces.SelectMany(declaration => declaration.Usings).Any())
                {
                    violations.Add($"{tree.FilePath}: Place all using directives outside the namespace.");
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingDirectives_ThenNoRegionOrNullableDirectiveIsUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                SyntaxTrivia[] forbiddenDirectives = [.. tree.GetRoot().DescendantTrivia().Where(trivia =>
                    trivia.IsKind(SyntaxKind.NullableDirectiveTrivia) ||
                    trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) ||
                    trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))];

                Assert.That(
                    forbiddenDirectives,
                    Is.Empty,
                    $"{tree.FilePath}: '#nullable' and '#region' directives are prohibited.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingTypes_ThenExplicitTypesAndNoPartialDeclarationsAreUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (VariableDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<VariableDeclarationSyntax>())
                {
                    if (declaration.Type.IsVar)
                    {
                        violations.Add($"{Describe(tree, declaration)}: Declare the explicit type instead of 'var'.");
                    }
                }

                foreach (TypeDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    if (declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                    {
                        violations.Add($"{Describe(tree, declaration)}: Partial declarations are prohibited.");
                    }
                }

                foreach (GlobalStatementSyntax statement in tree.GetRoot().DescendantNodes().OfType<GlobalStatementSyntax>())
                {
                    violations.Add($"{Describe(tree, statement)}: Top-level statements are prohibited.");
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingForbiddenForms_ThenForbiddenExpressionsAndParametersAreAbsent(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
                {
                    string reason = GetForbiddenFormReason(node);

                    if (!string.IsNullOrEmpty(reason))
                    {
                        violations.Add($"{Describe(tree, node)}: {reason}");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingControlFlow_ThenEveryBodyIsBraced(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
                {
                    StatementSyntax? body = GetControlFlowBody(node);

                    if (body is not null && body is not BlockSyntax)
                    {
                        violations.Add($"{Describe(tree, node)}: Enclose the control-flow body in braces.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingSignatures_ThenAtMostSevenParametersAreDeclared(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (ParameterListSyntax parameters in tree.GetRoot().DescendantNodes().OfType<ParameterListSyntax>())
                {
                    if (parameters.Parameters.Count <= MaximumParameterCount ||
                        IsParameterLimitExemption(tree, parameters))
                    {
                        continue;
                    }

                    violations.Add(
                        $"{Describe(tree, parameters)}: Declare at most {MaximumParameterCount} parameters; "
                            + "group cohesive inputs into a request or options type.");
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingMembers_ThenAccessibilityAndKindOrderAreExplicit(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (MemberDeclarationSyntax member in tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>())
                {
                    if (IsAccessibilityScopedMember(member) &&
                        !DeclaresAccessibility(member))
                    {
                        violations.Add($"{Describe(tree, member)}: Declare the member accessibility explicitly.");
                    }
                }

                foreach (TypeDeclarationSyntax typeDeclaration in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    violations.AddRange(GetKindOrderViolations(tree, typeDeclaration));
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingBodies_ThenSingleStatementMethodsAndComputedPropertiesUseExpressions(
            string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (MethodDeclarationSyntax method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (IsSingleTerminalStatementBody(method.Body))
                    {
                        violations.Add($"{Describe(tree, method)}: Use an expression body for single-statement methods.");
                    }
                }

                foreach (PropertyDeclarationSyntax property in tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>())
                {
                    if (IsSingleReturnGetterOnlyProperty(property))
                    {
                        violations.Add($"{Describe(tree, property)}: Use an expression body for computed read-only properties.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingAsyncUsage_ThenNoAsyncOrCancellationTypesAreUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
                {
                    if (node is MethodDeclarationSyntax method &&
                        method.Modifiers.Any(SyntaxKind.AsyncKeyword))
                    {
                        violations.Add($"{Describe(tree, method)}: Prefer synchronous methods over async/await.");
                    }

                    if (node is IdentifierNameSyntax identifier &&
                        string.Equals(identifier.Identifier.ValueText, "CancellationToken", StringComparison.Ordinal))
                    {
                        violations.Add($"{Describe(tree, identifier)}: Do not use CancellationToken unless required.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        public void GivenProductionSources_WhenCheckingConstants_ThenNoneAreDeclaredAsConst(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                FieldDeclarationSyntax[] constFields = [.. tree.GetRoot().DescendantNodes()
                    .OfType<FieldDeclarationSyntax>()
                    .Where(field => field.Modifiers.Any(SyntaxKind.ConstKeyword))];

                Assert.That(
                    constFields,
                    Is.Empty,
                    $"{tree.FilePath}: Express constants as static expression-bodied properties, never const.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingPlacement_ThenNamespacesMirrorTheFolderStructure(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                NamespaceDeclarationSyntax namespaceDeclaration = tree.GetCompilationUnitRoot()
                    .DescendantNodes()
                    .OfType<NamespaceDeclarationSyntax>()
                    .Single();
                string relativePath = ArchitectureResources.GetResourcePaths(sourcePrefix)
                    .Single(path => string.Equals(path, tree.FilePath, StringComparison.Ordinal));
                string relativeFolder = GetRelativeFolder(relativePath);
                string expectedNamespace = GetProjectNamespace(sourcePrefix);

                if (!string.Equals(relativeFolder, ".", StringComparison.Ordinal))
                {
                    expectedNamespace += "." + relativeFolder.Replace('/', '.');
                }

                Assert.That(
                    namespaceDeclaration.Name.ToString(),
                    Is.EqualTo(expectedNamespace),
                    $"{tree.FilePath}: The namespace must mirror the project and folder structure.");
                Assert.That(
                    namespaceDeclaration.Name.ToString().Split('.')
                        .Intersect(["Interfaces", "Enums", "Enumerations"], StringComparer.Ordinal),
                    Is.Empty,
                    $"{tree.FilePath}: Interfaces and enumerations must be colocated with their domain.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingUsings_ThenGroupsAreOrderedAndAlphabetical(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = GetUsingGroupViolations(tree);

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingWhitespace_ThenPortableFormattingIsUsed(string sourcePrefix)
        {
            foreach (string source in ArchitectureResources.GetResourcePaths(sourcePrefix))
            {
                List<string> violations = GetWhitespaceViolations(sourcePrefix, source);

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingFormatting_ThenThresholdsAreRespected(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (IfStatementSyntax statement in tree.GetRoot().DescendantNodes().OfType<IfStatementSyntax>())
                {
                    violations.AddRange(CheckOpeningLineLength(tree, statement, MaximumConditionalLength));
                }

                foreach (ParameterListSyntax parameters in tree.GetRoot().DescendantNodes().OfType<ParameterListSyntax>())
                {
                    violations.AddRange(CheckOpeningLineLength(tree, parameters, MaximumSignatureLength));
                }

                foreach (ArgumentListSyntax arguments in tree.GetRoot().DescendantNodes().OfType<ArgumentListSyntax>())
                {
                    violations.AddRange(CheckOpeningLineLength(tree, arguments, MaximumSignatureLength));
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        public void GivenProductionSources_WhenCheckingDocumentation_ThenEveryPublicTypeIsDocumented(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (MemberDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>())
                {
                    if (type is not BaseTypeDeclarationSyntax declaration ||
                        !declaration.Modifiers.Any(SyntaxKind.PublicKeyword) ||
                        IsDocumented(declaration))
                    {
                        continue;
                    }

                    violations.Add($"{Describe(tree, declaration)}: Add XML summary documentation to the public type.");
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        private static IEnumerable<string> CheckOpeningLineLength(SyntaxTree tree, SyntaxNode node, int maximum)
        {
            if (node.ToString().Contains('\n'))
            {
                yield break;
            }

            if (node is IfStatementSyntax && !HasMultipleClauses(node))
            {
                yield break;
            }

            if (node is ParameterListSyntax parameterList &&
                parameterList.Parameters.Count < 2)
            {
                yield break;
            }

            if (node is ArgumentListSyntax argumentList &&
                argumentList.Arguments.Count < 2)
            {
                yield break;
            }

            int listLength = node.ToString().Length;

            if (listLength <= maximum)
            {
                yield break;
            }

            if (node is IfStatementSyntax)
            {
                yield return $"{Describe(tree, node)}: Split the multi-clause condition exceeding {maximum} characters, one clause per line.";
                yield break;
            }

            yield return $"{Describe(tree, node)}: Split the list of {listLength} characters, one item per line.";
        }

        private static bool HasMultipleClauses(SyntaxNode node)
            => node.DescendantNodes().OfType<BinaryExpressionSyntax>()
                .Any(expression => expression.IsKind(SyntaxKind.LogicalAndExpression) ||
                    expression.IsKind(SyntaxKind.LogicalOrExpression));

        private static bool IsParameterLimitExemption(SyntaxTree tree, ParameterListSyntax parameters)
        {
            if (parameters.Parent is not MethodDeclarationSyntax method)
            {
                return false;
            }

            TypeDeclarationSyntax? declaringType = method.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            string typeName = string.Empty;

            if (declaringType is not null)
            {
                typeName = declaringType.Identifier.ValueText;
            }

            return ParameterLimitExemptions.Contains($"{typeName}.{method.Identifier.ValueText}", StringComparer.Ordinal);
        }

        private static IEnumerable<SyntaxTree> CollectParseTrees(string sourcePrefix)
        {
            IReadOnlyList<string> sources = ArchitectureResources.GetResourcePaths(sourcePrefix);

            Assert.That(
                sources,
                Is.Not.Empty,
                $"Violated convention: no source files were found under '{sourcePrefix}'.");

            return sources.Select(source => CSharpSyntaxTree.ParseText(
                ArchitectureResources.ReadResource(sourcePrefix, source),
                path: source));
        }

        private static List<string> GetWhitespaceViolations(string sourcePrefix, string source)
        {
            string content = ArchitectureResources.ReadResource(sourcePrefix, source)
                .Replace("\r\n", "\n", StringComparison.Ordinal);
            List<string> violations = [];
            string[] lines = content.Split('\n');

            if (!content.EndsWith('\n'))
            {
                violations.Add($"{source}: Terminate the file with an empty line.");
            }

            if (content.Contains("\n\n\n", StringComparison.Ordinal))
            {
                violations.Add($"{source}: At most one consecutive blank line is permitted.");
            }

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex += 1)
            {
                string line = lines[lineIndex];
                string indentation = line.Substring(0, line.Length - line.TrimStart().Length);

                if (indentation.Contains('\t'))
                {
                    violations.Add($"{source}:{lineIndex + 1}: Use four spaces, never tabs.");
                }
                else if (indentation.Length % 4 != 0)
                {
                    violations.Add($"{source}:{lineIndex + 1}: Indent with a multiple of four spaces.");
                }

                if (line.Length != line.TrimEnd().Length)
                {
                    violations.Add($"{source}:{lineIndex + 1}: Remove trailing whitespace.");
                }

                if (IsBlankLineBeforeClosingDelimiter(lines, lineIndex))
                {
                    violations.Add($"{source}:{lineIndex + 1}: Remove the blank line before the closing delimiter.");
                }
            }

            return violations;
        }

        private static bool IsBlankLineBeforeClosingDelimiter(string[] lines, int lineIndex)
        {
            if (lineIndex < 1 ||
                lineIndex + 1 >= lines.Length ||
                !string.IsNullOrWhiteSpace(lines[lineIndex]))
            {
                return false;
            }

            if (lineIndex > 0 && string.IsNullOrWhiteSpace(lines[lineIndex - 1]))
            {
                return false;
            }

            string nextContent = lines[lineIndex + 1].TrimStart();

            return nextContent.Length > 0 && "}])".Contains(nextContent[0], StringComparison.Ordinal);
        }

        private static List<string> GetUsingGroupViolations(SyntaxTree tree)
        {
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            List<string> violations = [];
            List<List<string>> groups = [];
            List<int> groupRanks = [];
            List<int> groupStartLines = [];

            foreach (UsingDirectiveSyntax directive in root.Usings)
            {
                if (directive.Alias is not null ||
                    directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                {
                    continue;
                }

                string name = directive.Name!.ToString();
                int line = directive.GetLocation().GetLineSpan().StartLinePosition.Line;

                if (groupStartLines.Count == 0 || line - groupStartLines[^1] > 1)
                {
                    groups.Add([]);
                    groupRanks.Add(GetGroupRank(name));
                    groupStartLines.Add(line);
                }

                groups[^1].Add(name);
            }

            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex += 1)
            {
                if (!IsOrdered(groups[groupIndex]))
                {
                    violations.Add($"{tree.FilePath}: Alphabetise the using directives within the group.");
                }

                if (groupIndex > 0 &&
                    groupRanks[groupIndex] < groupRanks[groupIndex - 1])
                {
                    violations.Add(
                        $"{tree.FilePath}: Order groups as System, Microsoft, third-party, then the current solution.");
                }
            }

            if (root.Usings.Count > 0)
            {
                int lastUsingLine = root.Usings[^1].GetLocation().GetLineSpan().StartLinePosition.Line;
                int namespaceLine = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single()
                    .GetLocation().GetLineSpan().StartLinePosition.Line;

                if (namespaceLine - lastUsingLine != 2)
                {
                    violations.Add($"{tree.FilePath}: Separate the last using and the namespace with exactly one blank line.");
                }
            }

            return violations;
        }

        private static bool IsOrdered(List<string> names)
        {
            for (int index = 1; index < names.Count; index += 1)
            {
                if (string.Compare(names[index - 1], names[index], StringComparison.OrdinalIgnoreCase) > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetGroupRank(string namespaceName)
        {
            if (namespaceName.StartsWith("System", StringComparison.Ordinal))
            {
                return 0;
            }

            if (namespaceName.StartsWith("Microsoft", StringComparison.Ordinal))
            {
                return 1;
            }

            if (namespaceName.StartsWith("NuciXNA.Graphics", StringComparison.Ordinal))
            {
                return 3;
            }

            return 2;
        }

        private static List<string> GetKindOrderViolations(SyntaxTree tree, TypeDeclarationSyntax typeDeclaration)
        {
            List<string> violations = [];
            int previousKindIndex = -1;

            foreach (MemberDeclarationSyntax member in typeDeclaration.Members.Where(IsAccessibilityScopedMember))
            {
                int kindIndex = GetKindIndex(member);

                if (kindIndex < previousKindIndex)
                {
                    violations.Add(
                        $"{Describe(tree, member)}: Order members as fields, properties, events, constructors, then methods.");
                }

                previousKindIndex = kindIndex;
            }

            return violations;
        }

        private static int GetKindIndex(MemberDeclarationSyntax member)
            => member switch
            {
                FieldDeclarationSyntax => 0,
                PropertyDeclarationSyntax or IndexerDeclarationSyntax => 1,
                EventFieldDeclarationSyntax or EventDeclarationSyntax => 2,
                ConstructorDeclarationSyntax or DestructorDeclarationSyntax => 3,
                _ => 4
            };

        private static string GetForbiddenFormReason(SyntaxNode node)
            => node switch
            {
                ConditionalExpressionSyntax => "Replace the conditional expression with if/else.",
                TupleTypeSyntax or TupleExpressionSyntax => "Use a dedicated type instead of a tuple.",
                AccessorDeclarationSyntax accessor when accessor.IsKind(SyntaxKind.InitAccessorDeclaration)
                    => "Use set instead of init.",
                BinaryExpressionSyntax expression when expression.IsKind(SyntaxKind.CoalesceExpression)
                    => "Replace the null-coalescing operator with explicit control flow.",
                BinaryExpressionSyntax expression when IsNullComparison(expression)
                    => "Use 'is null' or 'is not null'.",
                ParameterSyntax parameter when parameter.Default is not null
                    => "Use overloads instead of optional parameters.",
                ParameterSyntax parameter when parameter.Modifiers.Any(SyntaxKind.RefKeyword)
                    => "Ref parameters are prohibited.",
                ThrowStatementSyntax statement when statement.Expression is IdentifierNameSyntax
                    => "Use a bare rethrow to preserve the stack trace.",
                _ => string.Empty
            };

        private static bool IsNullComparison(BinaryExpressionSyntax expression)
        {
            if (!expression.IsKind(SyntaxKind.EqualsExpression) &&
                !expression.IsKind(SyntaxKind.NotEqualsExpression))
            {
                return false;
            }

            return expression.Left.IsKind(SyntaxKind.NullLiteralExpression) ||
                expression.Right.IsKind(SyntaxKind.NullLiteralExpression);
        }

        private static StatementSyntax? GetControlFlowBody(SyntaxNode node)
            => node switch
            {
                IfStatementSyntax statement => statement.Statement,
                ElseClauseSyntax clause when clause.Statement is not IfStatementSyntax => clause.Statement,
                ForStatementSyntax statement => statement.Statement,
                CommonForEachStatementSyntax statement => statement.Statement,
                WhileStatementSyntax statement => statement.Statement,
                DoStatementSyntax statement => statement.Statement,
                UsingStatementSyntax statement => statement.Statement,
                LockStatementSyntax statement => statement.Statement,
                _ => null
            };

        private static bool IsSingleTerminalStatementBody(BlockSyntax? body)
            => body is not null &&
                body.Statements.Count == 1 &&
                (body.Statements[0] is ReturnStatementSyntax || body.Statements[0] is ThrowStatementSyntax);

        private static bool IsSingleReturnGetterOnlyProperty(PropertyDeclarationSyntax property)
        {
            if (property.ExpressionBody is not null ||
                property.AccessorList is null ||
                property.AccessorList.Accessors.Count != 1)
            {
                return false;
            }

            AccessorDeclarationSyntax accessor = property.AccessorList.Accessors[0];

            return accessor.IsKind(SyntaxKind.GetAccessorDeclaration) &&
                accessor.Body is not null &&
                accessor.Body.Statements.Count == 1 &&
                accessor.Body.Statements[0] is ReturnStatementSyntax;
        }

        private static bool IsDocumented(BaseTypeDeclarationSyntax declaration)
        {
            SyntaxTriviaList leadingTrivia = declaration.GetLeadingTrivia();

            if (leadingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)))
            {
                return true;
            }

            return declaration.AttributeLists.Count > 0 &&
                declaration.AttributeLists[0].GetLeadingTrivia().Any(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
        }

        private static bool IsAccessibilityScopedMember(MemberDeclarationSyntax member)
            => member is FieldDeclarationSyntax ||
                member is PropertyDeclarationSyntax ||
                member is EventFieldDeclarationSyntax ||
                member is ConstructorDeclarationSyntax ||
                member is MethodDeclarationSyntax ||
                member is OperatorDeclarationSyntax;

        private static bool DeclaresAccessibility(MemberDeclarationSyntax member)
        {
            SyntaxTokenList modifiers = member switch
            {
                FieldDeclarationSyntax field => field.Modifiers,
                PropertyDeclarationSyntax property => property.Modifiers,
                EventFieldDeclarationSyntax eventField => eventField.Modifiers,
                ConstructorDeclarationSyntax constructor => constructor.Modifiers,
                MethodDeclarationSyntax method => method.Modifiers,
                OperatorDeclarationSyntax operatorDeclaration => operatorDeclaration.Modifiers,
                _ => default
            };

            return modifiers.Any(modifier =>
                modifier.IsKind(SyntaxKind.PublicKeyword) ||
                modifier.IsKind(SyntaxKind.ProtectedKeyword) ||
                modifier.IsKind(SyntaxKind.InternalKeyword) ||
                modifier.IsKind(SyntaxKind.PrivateKeyword));
        }

        private static string GetProjectNamespace(string sourcePrefix)
            => sourcePrefix switch
            {
                ArchitectureResources.UnitTestSourcePrefix => "NuciXNA.Graphics.UnitTests",
                ArchitectureResources.ArchitectureTestSourcePrefix => "NuciXNA.Graphics.ArchitectureTests",
                _ => "NuciXNA.Graphics"
            };

        private static string GetRelativeFolder(string relativePath)
        {
            int separatorIndex = relativePath.LastIndexOf('/');

            if (separatorIndex < 0)
            {
                return ".";
            }

            return relativePath.Substring(0, separatorIndex);
        }

        private static string Describe(SyntaxTree tree, SyntaxNode node)
            => $"{tree.FilePath}:{node.GetLocation().GetLineSpan().StartLinePosition.Line + 1}";
    }
}
