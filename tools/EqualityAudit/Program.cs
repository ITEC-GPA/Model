using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Architecture review R4: the uses of the default equality (Equals, ==, keyed collections and LINQ operators without a comparer) on the
// types derived from GPC.Model.ModelObject, outside the equality members themselves. Each use must be listed with its reason in the
// allowlist, so that a new one states its intent (ModelObjectIdentityComparer for identity, ContentEquals for content).
//   EqualityAudit check <allowlist.json> <project.csproj>...   exit 0 when the uses and the allowlist coincide
//   EqualityAudit list <project.csproj>...                     prints the uses in the allowlist format
if (args.Length < 2 || args[0] is not ("check" or "list")) { Console.Error.WriteLine("EqualityAudit check <allowlist.json> <project>... | list <project>..."); return 2; }
bool check = args[0] == "check";
var projects = args.Skip(check ? 2 : 1).Select(Path.GetFullPath).ToArray();
string root = Directory.GetCurrentDirectory();

var references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var ownAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
foreach (var project in projects)
{
    var json = JsonNode.Parse(MsBuild(project, "-t:ResolveAssemblyReferences", "-getItem:ReferencePath", "-getProperty:AssemblyName"))!;
    ownAssemblies.Add(json["Properties"]!["AssemblyName"]!.GetValue<string>());
    foreach (var item in json["Items"]!["ReferencePath"]!.AsArray()) references.Add(item!["FullPath"]!.GetValue<string>());
}
var metadata = references.Where(r => !ownAssemblies.Contains(Path.GetFileNameWithoutExtension(r))).Select(r => MetadataReference.CreateFromFile(r)).ToList();
var trees = projects.SelectMany(p => Directory.EnumerateFiles(Path.GetDirectoryName(p)!, "*.cs", SearchOption.AllDirectories))
    .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(d => d is "obj" or "bin") && !f.EndsWith("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
    .Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), new CSharpParseOptions(LanguageVersion.Latest), f)).ToList();
var compilation = CSharpCompilation.Create("EqualityAudit", trees, metadata, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
if (errors.Count > 0)
{
    // The analysis needs a complete semantic model: unresolved code would hide uses.
    foreach (var e in errors.Take(10)) Console.Error.WriteLine(e);
    Console.Error.WriteLine($"EqualityAudit: {errors.Count} compilation errors, analysis not reliable.");
    return 1;
}
var modelObject = compilation.GetTypeByMetadataName("GPC.Model.ModelObject") ?? throw new InvalidOperationException("GPC.Model.ModelObject not found.");

bool IsModel(ITypeSymbol? t)
{
    for (var b = t; b is not null; b = b.BaseType) if (SymbolEqualityComparer.Default.Equals(b, modelObject)) return true;
    return false;
}
string[] equalityMembers = ["Equals", "GetHashCode", "operator==", "operator!=", "ContentEquals"];
string[] comparerSensitive = ["Distinct", "GroupBy", "ToDictionary", "ToLookup", "Contains", "Except", "Intersect", "Union", "IndexOf", "LastIndexOf", "Remove", "SequenceEqual", "ToHashSet"];
var uses = new List<(string File, string Member, string Kind, string Type, string Text)>();
void Add(SyntaxNode node, string kind, ITypeSymbol type)
{
    var owner = node.Ancestors().FirstOrDefault(a => a is BaseMethodDeclarationSyntax or PropertyDeclarationSyntax or IndexerDeclarationSyntax);
    string member = owner switch
    {
        MethodDeclarationSyntax m => m.Identifier.Text, OperatorDeclarationSyntax o => "operator" + o.OperatorToken.Text,
        ConstructorDeclarationSyntax => ".ctor", PropertyDeclarationSyntax p => p.Identifier.Text, IndexerDeclarationSyntax => "this[]", _ => "(campo)"
    };
    if (equalityMembers.Contains(member)) return;
    string file = Path.GetRelativePath(root, node.SyntaxTree.FilePath).Replace('\\', '/');
    uses.Add((file, member, kind, type.ToDisplayString(), string.Join(' ', node.ToString().Split((char[])['\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))));
}
foreach (var tree in trees)
{
    var model = compilation.GetSemanticModel(tree);
    foreach (var node in tree.GetRoot().DescendantNodes())
    {
        switch (node)
        {
            case BaseObjectCreationExpressionSyntax creation:
                if (model.GetTypeInfo(creation).Type is INamedTypeSymbol { IsGenericType: true } created
                    && created.OriginalDefinition.ToDisplayString() is var definition
                    && (definition.StartsWith("System.Collections.Generic.HashSet<") || definition.StartsWith("System.Collections.Generic.Dictionary<")
                        || definition.StartsWith("System.Collections.Concurrent.ConcurrentDictionary<"))
                    && IsModel(created.TypeArguments[0])
                    && model.GetSymbolInfo(creation).Symbol is IMethodSymbol ctor && !ctor.Parameters.Any(p => p.Type.Name is "IEqualityComparer"))
                    Add(creation, "collection", created.TypeArguments[0]);
                break;
            case InvocationExpressionSyntax invocation when model.GetSymbolInfo(invocation).Symbol is IMethodSymbol method:
                if (method.Name == "Equals" && method.Parameters.Length == 1 && !method.IsStatic && invocation.Expression is MemberAccessExpressionSyntax access
                    && model.GetTypeInfo(access.Expression).Type is { } receiver && IsModel(receiver))
                { Add(invocation, "Equals", receiver); break; }
                if (!comparerSensitive.Contains(method.Name) || method.Parameters.Any(p => p.Type.Name is "IEqualityComparer" or "IComparer")) break;
                ITypeSymbol? element = null;
                if (method.IsExtensionMethod && method.ContainingType.Name == "Enumerable")
                    element = method.Name is "GroupBy" or "ToDictionary" or "ToLookup" ? method.TypeArguments.ElementAtOrDefault(1) : method.TypeArguments.ElementAtOrDefault(0);
                // HashSet and Dictionary: the comparer is chosen at construction (rule "collection"), not in Contains or Remove.
                else if (method.ContainingType is { IsGenericType: true } container && container.Name is "List" or "Collection" or "ICollection" or "IList")
                    element = container.TypeArguments[0];
                if (element is not null && IsModel(element)) Add(invocation, "linq/" + method.Name, element);
                break;
            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.EqualsExpression) || binary.IsKind(SyntaxKind.NotEqualsExpression):
                if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression) || binary.Right.IsKind(SyntaxKind.NullLiteralExpression)) break;
                if (model.GetSymbolInfo(binary).Symbol is IMethodSymbol { MethodKind: MethodKind.UserDefinedOperator }
                    && model.GetTypeInfo(binary.Left).Type is { } left && IsModel(left))
                    Add(binary, "operator" + binary.OperatorToken.Text, left);
                break;
        }
    }
}
var groups = uses.GroupBy(u => (u.File, u.Member, u.Kind, u.Type)).OrderBy(g => g.Key.File, StringComparer.Ordinal).ThenBy(g => g.Key.Member, StringComparer.Ordinal)
    .ThenBy(g => g.Key.Kind, StringComparer.Ordinal).ToList();
if (!check)
{
    var list = new JsonArray(groups.Select(g => (JsonNode)new JsonObject
    {
        ["file"] = g.Key.File, ["member"] = g.Key.Member, ["kind"] = g.Key.Kind, ["type"] = g.Key.Type, ["count"] = g.Count(), ["reason"] = "",
        ["examples"] = new JsonArray(g.Select(u => (JsonNode)u.Text).Distinct().Take(3).ToArray())
    }).ToArray());
    Console.WriteLine(list.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
    return 0;
}
var allowed = JsonNode.Parse(File.ReadAllText(args[1]))!["entries"]!.AsArray().Select(e => new
{
    Key = (e!["file"]!.GetValue<string>(), e["member"]!.GetValue<string>(), e["kind"]!.GetValue<string>(), e["type"]!.GetValue<string>()),
    Count = e["count"]!.GetValue<int>(), Reason = e["reason"]!.GetValue<string>()
}).ToList();
var failures = new List<string>();
foreach (var entry in allowed.Where(a => string.IsNullOrWhiteSpace(a.Reason))) failures.Add($"voce senza motivo: {entry.Key}");
foreach (var g in groups)
{
    var entry = allowed.FirstOrDefault(a => a.Key == g.Key);
    if (entry is null) failures.Add($"uso non elencato: {g.Key.File} {g.Key.Member} {g.Key.Kind} su {g.Key.Type} ({g.Count()}): {g.First().Text}");
    else if (entry.Count != g.Count()) failures.Add($"conteggio cambiato: {g.Key.File} {g.Key.Member} {g.Key.Kind} su {g.Key.Type}: {g.Count()} invece di {entry.Count}");
}
foreach (var entry in allowed.Where(a => !groups.Any(g => g.Key == a.Key))) failures.Add($"voce superata, da togliere: {entry.Key}");
Console.WriteLine($"EqualityAudit: {trees.Count} file, {uses.Count} usi dell'uguaglianza predefinita su ModelObject fuori dai membri di uguaglianza, {allowed.Count} voci ammesse.");
foreach (var f in failures) Console.WriteLine("  " + f);
return failures.Count == 0 ? 0 : 1;

static string MsBuild(string project, params string[] arguments)
{
    var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
    foreach (var a in new[] { "msbuild", project, "-nologo", "-p:Configuration=Release" }.Concat(arguments)) start.ArgumentList.Add(a);
    using var process = Process.Start(start)!;
    string output = process.StandardOutput.ReadToEnd(); process.WaitForExit();
    if (process.ExitCode != 0) throw new InvalidOperationException($"dotnet msbuild {project}: {output} {process.StandardError.ReadToEnd()}");
    return output;
}
