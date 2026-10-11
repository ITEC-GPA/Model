using System.Runtime.Serialization;
using System.Text;
using GPC.Model.Core;

/// <summary>
/// Round trip of the tests through the data contracts of the archives (DataContractSerializer with the closed vocabulary of
/// <see cref="ModelValues.DataContracts"/>), in place of BinaryFormatter: removed from .NET 9 and unsafe with data from others
/// (architecture review R10). Same shape as the formatter it replaces; each value is written with its type and length, so several
/// values can share a stream. In the global namespace so that every test namespace sees it.
/// </summary>
internal sealed class ContractFormatter
{
    public void Serialize(Stream stream, object value)
    {
        using var buffer = new MemoryStream();
        Serializer(value.GetType()).WriteObject(buffer, value);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(value.GetType().AssemblyQualifiedName!); writer.Write((int)buffer.Length); writer.Write(buffer.ToArray());
    }

    public object Deserialize(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var type = Type.GetType(reader.ReadString(), true)!;
        using var buffer = new MemoryStream(reader.ReadBytes(reader.ReadInt32()));
        return Serializer(type).ReadObject(buffer)!;
    }

    /// <summary>A copy of the value through its serialization data.</summary>
    public static T Copy<T>(T value) where T : class
    {
        var formatter = new ContractFormatter(); using var stream = new MemoryStream();
        formatter.Serialize(stream, value); stream.Position = 0; return (T)formatter.Deserialize(stream);
    }

    private static DataContractSerializer Serializer(Type type) => new(type, new DataContractSerializerSettings
    {
        KnownTypes = ModelValues.DataContracts.Append(type).Distinct(), PreserveObjectReferences = true, MaxItemsInObjectGraph = int.MaxValue
    });
}
