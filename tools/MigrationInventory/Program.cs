using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

if (args.Length != 2) throw new ArgumentException("MigrationInventory <isolated-workspace> <output.json>");
var workspace = Path.GetFullPath(args[0]);
var roots = new[] {
    "BridGHe/src/BridGHe.Common/Models", "BridGHe/src/BridGHe.Common/MovingLoads", "BridGHe/src/BridGHe.Common/Checks", "BridGHe/src/BridGHe.Common/Geometry",
    "Arachne/src/Arachne.Common/Models", "tensilelib/TensileLib/Geometry", "tensilelib/TensileLib/FormFinding" };
var declarations = new List<object>();
int fields = 0;
foreach (var folder in roots) foreach (var file in Directory.EnumerateFiles(Path.Combine(workspace, folder), "*.cs", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.Ordinal))
{
    var bytes = File.ReadAllBytes(file); var tree = CSharpSyntaxTree.ParseText(Encoding.UTF8.GetString(bytes)); var syntax = tree.GetRoot();
    var errors = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
    if (errors.Length > 0) throw new InvalidOperationException(file + ": " + string.Join("; ", errors.Select(e => e.ToString())));
    foreach (var type in syntax.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
    {
        string ns = string.Join(".", type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(n => n.Name.ToString()));
        string TypeName(BaseTypeDeclarationSyntax t) => t.Identifier.Text + (t is TypeDeclarationSyntax d && d.TypeParameterList != null ? "`" + d.TypeParameterList.Parameters.Count : "");
        string name = string.Join("+", type.Ancestors().OfType<BaseTypeDeclarationSyntax>().Reverse().Select(TypeName).Append(TypeName(type)));
        var members = new List<object>();
        void Add(SyntaxNode node, string id, string memberType, string kind, SyntaxTokenList modifiers, string? expression = null)
        {
            fields++;
            members.Add(new { name = id, type = memberType, kind, modifiers = modifiers.ToString(), line = tree.GetLineSpan(node.Span).StartLinePosition.Line + 1,
                documentation = node.GetLeadingTrivia().ToFullString().Trim(), expression,
                destination = "Consultare F2-CONTRATTI.md e f2-destinazioni.json; mapping di dettaglio da completare in F5", units = "Da riscontrare nell'adattatore; non inferite dal solo nome", status = "inventariato" });
        }
        foreach (var member in type.ChildNodes())
        {
            if (member is FieldDeclarationSyntax field)
                foreach (var variable in field.Declaration.Variables) Add(field, variable.Identifier.Text, field.Declaration.Type.ToString(), "field", field.Modifiers);
            else if (member is PropertyDeclarationSyntax property)
                Add(property, property.Identifier.Text, property.Type.ToString(), "property", property.Modifiers, property.ExpressionBody?.Expression.ToString());
            else if (member is EventFieldDeclarationSyntax ev)
                foreach (var variable in ev.Declaration.Variables) Add(ev, variable.Identifier.Text, ev.Declaration.Type.ToString(), "event", ev.Modifiers);
            else if (member is EnumMemberDeclarationSyntax enumeration)
                Add(enumeration, enumeration.Identifier.Text, name, "enumValue", default, enumeration.EqualsValue?.Value.ToString());
        }
        if (type is RecordDeclarationSyntax record && record.ParameterList != null)
            foreach (var parameter in record.ParameterList.Parameters)
                Add(parameter, parameter.Identifier.Text, parameter.Type?.ToString() ?? "", "positionalProperty", parameter.Modifiers);
        declarations.Add(new { source = Path.GetRelativePath(workspace, file).Replace('\\', '/'), sha256 = Convert.ToHexString(SHA256.HashData(bytes)),
            line = tree.GetLineSpan(type.Span).StartLinePosition.Line + 1, type = ns + "." + name, kind = type.Kind().ToString(),
            bases = type.BaseList?.Types.Select(t => t.Type.ToString()).ToArray() ?? Array.Empty<string>(), members });
    }
}
var report = new { schema = "GPC-migration-field-inventory-v1", scope = roots, typeCount = declarations.Count, memberCount = fields,
    notes = "Roslyn syntax inventory: declared fields/properties/events/enum values, including static UI state and computed properties. Inherited members are listed once on their declaring type. No claim of completed adapters or dimensional validation.", types = declarations };
File.WriteAllText(args[1], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }).Replace("\r\n", "\n").Replace("\n", "\r\n") + "\r\n", new UTF8Encoding(false));
Console.WriteLine($"{declarations.Count} types; {fields} declared members; {args[1]}");
