using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Standards
{
    public enum ConcreteStandardKind { Ntc2018, ModelCode2010, En1992p11, UniEn1992p11, DinEn1992p11, DsEn1992p11, NsEn1992p11, Cnr204, CsTr34 }

    /// <summary>Explicit first-generation concrete profiles used by the supported application adapters.
    /// Availability of a profile does not imply that every verification in that standard is implemented.</summary>
    public static class ConcreteStandardCatalog
    {
        private static readonly IReadOnlyDictionary<ConcreteStandardKind, string> Names = new Dictionary<ConcreteStandardKind, string>
        {
            [ConcreteStandardKind.Ntc2018] = "NTC 2018", [ConcreteStandardKind.ModelCode2010] = "Model Code 2010",
            [ConcreteStandardKind.En1992p11] = "EN 1992-1-1", [ConcreteStandardKind.UniEn1992p11] = "UNI EN 1992-1-1",
            [ConcreteStandardKind.DinEn1992p11] = "DIN EN 1992-1-1", [ConcreteStandardKind.DsEn1992p11] = "DS EN 1992-1-1",
            [ConcreteStandardKind.NsEn1992p11] = "NS EN 1992-1-1", [ConcreteStandardKind.Cnr204] = "CNR-DT 204/2006", [ConcreteStandardKind.CsTr34] = "CS-TR34"
        };
        public static IReadOnlyList<ConcreteStandardKind> Supported { get; } = Array.AsReadOnly(Enum.GetValues(typeof(ConcreteStandardKind)).Cast<ConcreteStandardKind>().ToArray());
        public static string Name(ConcreteStandardKind kind) => Names.TryGetValue(kind, out string name) ? name : throw new ArgumentException("Unknown concrete standard.");
        public static ConcreteStandardKind Parse(string name)
        {
            foreach (var item in Names) if (item.Value == name) return item.Key;
            throw new ArgumentException("Normativa non disponibile nelle DLL: " + name);
        }
        public static StandardModelCode2010 Create(ConcreteStandardKind kind)
        {
            switch (kind)
            {
                case ConcreteStandardKind.Ntc2018: return new StandardNTC2018Concrete();
                case ConcreteStandardKind.ModelCode2010: return new StandardModelCode2010();
                case ConcreteStandardKind.En1992p11: return new StandardEN1992p11();
                case ConcreteStandardKind.UniEn1992p11: return new StandardUNIEN1992p11();
                // Preserve the reviewed adapter profiles without changing historical constructors or serialized standards.
                case ConcreteStandardKind.DinEn1992p11: return new StandardDINEN1992p11 { AlphaCT = .85 };
                case ConcreteStandardKind.DsEn1992p11: return new StandardDSEN1992p11();
                case ConcreteStandardKind.NsEn1992p11: return new StandardNSEN1992p11 { AlphaCT = .85, SteelCoefficientStrainTension = .4 };
                case ConcreteStandardKind.Cnr204: return new StandardCNR204();
                case ConcreteStandardKind.CsTr34: return new StandardCSTR34();
                default: throw new ArgumentException("Unknown concrete standard.");
            }
        }
    }
}
