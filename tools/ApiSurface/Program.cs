using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;

if (args.Length < 4 || args[0] is not ("capture" or "check"))
    throw new ArgumentException("capture|check <assembly-directory> <baseline-directory> <assembly-name> ...");
var directory = Path.GetFullPath(args[1]); var baseline = Path.GetFullPath(args[2]);
Directory.CreateDirectory(baseline);
var context = new AssemblyLoadContext("api-inspection", true);
context.Resolving += (_, name) => File.Exists(Path.Combine(directory, name.Name + ".dll"))
    ? context.LoadFromAssemblyPath(Path.Combine(directory, name.Name + ".dll")) : null;
var renameFile = Path.Combine(baseline, "type-renames.tsv");
var renames = File.Exists(renameFile) ? File.ReadAllLines(renameFile).Where(l => l.Length > 0 && !l.StartsWith('#'))
    .Select(l => l.Split('\t')).ToDictionary(p => p[0], p => p.Length == 2 && p[0] != p[1] ? p[1] : throw new InvalidOperationException("Invalid type rename."), StringComparer.Ordinal)
    : new Dictionary<string,string>(StringComparer.Ordinal);
var renamePattern = renames.Count == 0 ? null : new Regex("(?:" + string.Join("|", renames.Keys.OrderByDescending(n => n.Length).Select(Regex.Escape)) + @")(?=[+\[<>,| )&*]|$)");
string ApplyRenames(string line) => renamePattern == null ? line : renamePattern.Replace(line, m => renames[m.Value]);
int failures = 0;
foreach (var name in args.Skip(3))
{
    var assembly = context.LoadFromAssemblyPath(Path.Combine(directory, name + ".dll"));
    var lines = Surface(assembly).OrderBy(x => x, StringComparer.Ordinal).ToArray();
    var file = Path.Combine(baseline, name + ".txt");
    if (args[0] == "capture") { File.WriteAllLines(file, lines); Console.WriteLine($"{name}: captured {lines.Length} signatures"); continue; }
    var allowedFile = Path.Combine(baseline, name + ".removed-types.txt");
    var allowed = File.Exists(allowedFile) ? File.ReadAllLines(allowedFile).Where(l => l.Length > 0 && !l.StartsWith('#')).ToHashSet(StringComparer.Ordinal) : new();
    var removed = File.ReadAllLines(file).Select(ApplyRenames).Except(lines, StringComparer.Ordinal).ToArray();
    var unexpected = removed.Where(l => !allowed.Contains(l.Split('|')[1])).ToArray();
    foreach (var line in unexpected) Console.Error.WriteLine("Unexpected removal: " + line);
    foreach (var type in allowed.Where(t => !removed.Any(l => l.Split('|')[1] == t))) { Console.Error.WriteLine("Unused removal allowance: " + type); failures++; }
    failures += unexpected.Length;
    Console.WriteLine($"{name}: {lines.Length} signatures, {removed.Length - unexpected.Length} approved removals, {unexpected.Length} unexpected removals");
}
return failures == 0 ? 0 : 1;

static string TypeName(Type? t)
{
    if (t == null) return "-";
    if (t.IsGenericParameter) return (t.DeclaringMethod == null ? "!" : "!!") + t.GenericParameterPosition;
    if (t.IsArray) return TypeName(t.GetElementType()) + "[" + new string(',', t.GetArrayRank() - 1) + "]";
    if (t.IsByRef) return TypeName(t.GetElementType()) + "&";
    if (t.IsPointer) return TypeName(t.GetElementType()) + "*";
    return t.IsGenericType ? t.GetGenericTypeDefinition().FullName + "<" + string.Join(",", t.GetGenericArguments().Select(TypeName)) + ">" : t.FullName ?? t.Name;
}
static string Constraints(Type t) => string.Join(";", t.GetGenericArguments().Where(a => a.IsGenericParameter)
    .Select(a => a.GenericParameterPosition + ":" + a.GenericParameterAttributes + ":" + string.Join(",", a.GetGenericParameterConstraints().Select(TypeName).OrderBy(x => x, StringComparer.Ordinal))));
static bool Visible(MethodBase m) => m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly;
static string Value(object? value) => value == null ? "null" : value is string s ? System.Text.Json.JsonSerializer.Serialize(s) : Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null";
static IEnumerable<string> Surface(Assembly assembly)
{
    const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    foreach (var t in assembly.GetExportedTypes())
    {
        var id = t.FullName!;
        yield return $"T|{id}|{t.Attributes}|base={TypeName(t.BaseType)}|interfaces={string.Join(",", t.GetInterfaces().Select(TypeName).OrderBy(x => x, StringComparer.Ordinal))}|{Constraints(t)}";
        foreach (var f in t.GetFields(flags).Where(f => f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly))
            yield return $"F|{id}|{f.Name}|{TypeName(f.FieldType)}|{f.Attributes}|{(f.IsLiteral ? Value(f.GetRawConstantValue()) : "")}";
        foreach (var m in t.GetMethods(flags).Cast<MethodBase>().Concat(t.GetConstructors(flags)).Where(Visible))
        {
            var method = m as MethodInfo;
            var parameters = string.Join(",", m.GetParameters().Select(p => TypeName(p.ParameterType) + " " + p.Name + ":" + p.Attributes + (p.HasDefaultValue ? "=" + Value(p.DefaultValue) : "")));
            var constraints = method?.IsGenericMethod == true ? string.Join(";", method.GetGenericArguments().Select(a => a.GenericParameterPosition + ":" + a.GenericParameterAttributes + ":" + string.Join(",", a.GetGenericParameterConstraints().Select(TypeName)))) : "";
            yield return $"M|{id}|{m.Name}({parameters})|{TypeName(method?.ReturnType)}|{m.Attributes}|{constraints}";
        }
    }
}
