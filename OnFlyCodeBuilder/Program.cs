namespace OnFlyCodeBuilder
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using Microsoft.CodeAnalysis.Emit;

	internal class Program
	{
		private static void Main(string[] args)
		{

			var console = SyntaxFactory.IdentifierName("Console");
			var writeline = SyntaxFactory.IdentifierName("WriteLine");
			var memberaccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, console, writeline);

			var argument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("A")));
			var argumentList = SyntaxFactory.SeparatedList(new[] { argument });

			var writeLineCall =
				SyntaxFactory.ExpressionStatement(
					SyntaxFactory.InvocationExpression(memberaccess,
						SyntaxFactory.ArgumentList(argumentList)));

			string assemblyName = $"cls{Guid.NewGuid():N}".ToUpper(CultureInfo.InvariantCulture);


			PredefinedTypeSyntax voidTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

			//MethodDeclarationSyntax method = SyntaxFactory.MethodDeclaration(voidTypeSyntax, "Main")
			//	.WithBody(SyntaxFactory.Block());

			MethodDeclarationSyntax methodDeclaration = SyntaxFactory.MethodDeclaration
			(
				attributeLists: SyntaxFactory.List<AttributeListSyntax>() , //new Microsoft.CodeAnalysis.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>(),
				modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
				returnType: SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
				explicitInterfaceSpecifier: (Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax)null,
				identifier: SyntaxFactory.Identifier("Main"),
				typeParameterList: (Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax)null,
				parameterList: SyntaxFactory.ParameterList(),
				constraintClauses: SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
				body: SyntaxFactory.Block(), //(Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax)null,
				expressionBody: (Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax)null,
				semicolonToken: SyntaxFactory.Token(SyntaxKind.None)
			).AddBodyStatements(writeLineCall);

			SyntaxList<AttributeListSyntax> attributeLists = SyntaxFactory.List<AttributeListSyntax>();
			SyntaxTokenList modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
			SyntaxToken classIdentifier = SyntaxFactory.Identifier("Program");
			TypeParameterListSyntax typeParameterListSyntax = SyntaxFactory.TypeParameterList(default(SeparatedSyntaxList<TypeParameterSyntax>));
			BaseListSyntax baseListSyntax = SyntaxFactory.BaseList(default(SeparatedSyntaxList<BaseTypeSyntax>));
			SyntaxList<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauseSyntaxs = SyntaxFactory.List<TypeParameterConstraintClauseSyntax>();

			//SyntaxFactory.ExpressionStatement(
			//SyntaxFactory.InvocationExpression()


			#region Class Defenition

			//SyntaxFactory.ClassDeclaration
			//(
			//	attributeLists: new Microsoft.CodeAnalysis.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>(),
			//	modifiers: new SyntaxTokenList(),
			//	keyword: SyntaxFactory.Token(SyntaxKind.ClassKeyword),
			//	identifier: SyntaxFactory.Identifier(""),
			//	typeParameterList: (Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax)null,
			//	baseList: (Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax)null,
			//	constraintClauses: new Microsoft.CodeAnalysis.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax>(),
			//	openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
			//	members: new Microsoft.CodeAnalysis.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax>(),
			//	closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
			//	semicolonToken: new Microsoft.CodeAnalysis.SyntaxToken()
			//);

			#endregion

			var classDeclaration = SyntaxFactory.ClassDeclaration
				(
					attributeLists: attributeLists,
					modifiers: modifiers,
					keyword: SyntaxFactory.Token(SyntaxKind.ClassKeyword),
					identifier: SyntaxFactory.Identifier("Program"),
					typeParameterList: null, //typeParameterListSyntax,
					baseList: null, //baseListSyntax,
					constraintClauses: typeParameterConstraintClauseSyntaxs, //default(SyntaxList<TypeParameterConstraintClauseSyntax>),
					openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
					members: default(SyntaxList<MemberDeclarationSyntax>),
					closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
					semicolonToken: SyntaxFactory.Token(SyntaxKind.None)
				)
				.AddMembers(methodDeclaration); //.AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword));

			NameSyntax namespaceName = SyntaxFactory.IdentifierName(assemblyName);

			NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory.NamespaceDeclaration
			(
				SyntaxFactory.Token(SyntaxKind.NamespaceKeyword),
				namespaceName,
				SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
				default(SyntaxList<ExternAliasDirectiveSyntax>),
				default(SyntaxList<UsingDirectiveSyntax>),
				default(SyntaxList<MemberDeclarationSyntax>),
				SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
				SyntaxFactory.Token(SyntaxKind.None)
			)
			.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")))
			.AddMembers(classDeclaration);


			CompilationUnitSyntax compilationUnitSyntax = SyntaxFactory.CompilationUnit()
				.AddMembers(namespaceDeclaration).NormalizeWhitespace("    ");

			SyntaxTree syntaxTree = SyntaxFactory.SyntaxTree(compilationUnitSyntax);


			MetadataReference[] references =
			{
				MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
			};

			Compilation compilation = CSharpCompilation.Create
			(
				assemblyName: assemblyName,
				syntaxTrees: new[] { syntaxTree },
				references: references,
				options: new CSharpCompilationOptions(OutputKind.ConsoleApplication/*, mainTypeName: assemblyName + ".Program", usings: new[] { "System" }*/)
			);

			EmitResult result;
			byte[] bytes;

			using (var ms = new MemoryStream())
			{
				result = compilation.Emit(ms);
				bytes = ms.ToArray();
			}

			if (result.Success)
			{
				Assembly assembly = Assembly.Load(bytes);
				assembly.EntryPoint.Invoke(null, /*new object[] { new[] { Environment.CurrentDirectory } }*/ null);
			}

			var diagnostics = result.Diagnostics.ToArray();

		}
	}
}
