namespace OnFlyCompiler
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using Microsoft.CodeAnalysis.Emit;
	using Microsoft.CodeAnalysis.Text;

	internal class Program
	{
		private static void Main(string[] args)
		{
			string assemblyName = "cls" + Guid.NewGuid().ToString("N"); //Path.GetRandomFileName();

			var sourceCode = CreateSourceCode(assemblyName);


			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

			var root = (CompilationUnitSyntax)syntaxTree.GetRoot();

			NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(assemblyName));


			var newCompilationUnit = SyntaxFactory.CompilationUnit()
				.AddMembers
				(
					namespaceDeclaration
						.AddUsings(root.Usings.ToArray())
						.AddMembers(root.Members[0])
				);

			syntaxTree = SyntaxFactory.SyntaxTree(newCompilationUnit); //CSharpSyntaxTree.Create(newCompilationUnit);


			var diagnostics = syntaxTree.GetDiagnostics().ToArray();


			//if (root.Kind() == SyntaxKind.CompilationUnit)
			//{
			//	CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax) root;
			//	Do(compilationUnit.Members[0]);
			//}

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

			byte[] bytes;
			var result = Emit(compilation, out bytes);

			if (result.Success)
			{
				Assembly assembly = Assembly.Load(/*ms.ToArray()*/ bytes);
				//var appType = assembly.GetType("App .Program");
				//var mainMethod = appType.GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new Type[0], null);
				//var retVal = mainMethod.Invoke(null, null);
				assembly.EntryPoint.Invoke(null, new object[] { new[] { Environment.CurrentDirectory } });
			}
			else
			{
				IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
					diagnostic.IsWarningAsError ||
					diagnostic.Severity == DiagnosticSeverity.Error);

				foreach (Diagnostic diagnostic in failures)
				{
					switch (diagnostic.Location.Kind)
					{
						case LocationKind.SourceFile:
						case LocationKind.XmlFile:
						case LocationKind.ExternalFile:
							FileLinePositionSpan lineSpan = diagnostic.Location.GetLineSpan();
							FileLinePositionSpan mappedLineSpan = diagnostic.Location.GetMappedLineSpan();
							if (lineSpan.IsValid && mappedLineSpan.IsValid)
							{
								string path;
								string basePath;
								if (mappedLineSpan.HasMappedPath)
								{
									path = mappedLineSpan.Path;
									basePath = lineSpan.Path;
								}
								else
								{
									path = lineSpan.Path;
									basePath = (string)null;
								}
								//return string.Format(formatter, "{0}{1}: {2}: {3}", (object)this.FormatSourcePath(path, basePath, formatter), (object)this.FormatSourceSpan(mappedLineSpan.Span, formatter), (object)this.GetMessagePrefix(diagnostic), (object)diagnostic.GetMessage((IFormatProvider)cultureInfo));
							}

							break;
					}
					Location location = diagnostic.Location;
					LinePosition startPosition = location.GetMappedLineSpan().Span.Start;

					TextSpan sourceSpan = location.SourceSpan;
					var location1 = location.SourceTree.GetLineSpan(sourceSpan);
					var sourceText = location.SourceTree.GetText().Lines.GetLineFromPosition(location.SourceSpan.Start).ToString().Substring(8, 7);

					Console.Error.WriteLine($"({startPosition.Line + 1},{startPosition.Character + 1}) {diagnostic.Id} : {diagnostic.GetMessage()}");
				}
			}
		}

		private static EmitResult Emit(Compilation compilation, out byte[] bytes)
		{
			EmitResult result;

			using (var ms = new MemoryStream())
			{
				result = compilation.Emit(ms);
				bytes = ms.ToArray();
			}

			return result;
		}

		private static bool TryReadFile(string filePath, out string sourceCode)
		{
			sourceCode = null;

			if (File.Exists(filePath))
			{
				try
				{
					sourceCode = File.ReadAllText(filePath);
					return true;
				}
				catch (IOException ioException)
				{
					Console.WriteLine(ioException.Message);
				}
			}
			else
			{
				Console.WriteLine("File is not found. '({0})'", filePath);
			}

			return false;
		}

		private static string CreateSourceCode(string @namespace)
		{
			string filePath = "./Code/Program.cs";

			string sourceCode;

			if (TryReadFile(filePath, out sourceCode))
			{
				return sourceCode;
				//var stringBuilder = new StringBuilder(sourceCode);
				//stringBuilder.Replace("${namespace}", @namespace);
				//return stringBuilder.ToString();
			}

			return string.Empty;
		}

		private static void Do(MemberDeclarationSyntax node)
		{
			SyntaxKind syntaxKind = node.Kind();

			switch (syntaxKind)
			{
				case SyntaxKind.ClassDeclaration:
					VisitClassDeclaration((ClassDeclarationSyntax)node);
					break;

				case SyntaxKind.NamespaceDeclaration:
					VisitNamespaceDeclaration((NamespaceDeclarationSyntax)node);
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		private static void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
		{
		}

		private static void VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			var parent = (CompilationUnitSyntax)node.Parent;

			NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(" cls" + Guid.NewGuid().ToString("N")));

			var newCompilationUnit = SyntaxFactory.CompilationUnit()
				.AddMembers
				(
					namespaceDeclaration
					.AddMembers
					(
						parent.Members.ToArray()
					)
				);

			CSharpSyntaxTree.Create(newCompilationUnit);

		}
	}
}
