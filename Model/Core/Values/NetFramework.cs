namespace GPC.Model.Core
{
    /// <summary>True on .NET Framework, whose core library is mscorlib. There the persisted names of the framework types
    /// (HistoricalTypeNames) and the text of doubles (DoubleText) follow .NET explicitly, so that archives and fingerprints are
    /// the same on both runtimes.</summary>
    internal static class NetFramework
    {
        internal static readonly bool Running = typeof(object).Assembly.GetName().Name == "mscorlib";
    }
}
