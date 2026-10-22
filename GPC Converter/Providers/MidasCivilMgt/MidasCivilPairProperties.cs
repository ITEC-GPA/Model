using System;
using System.Linq;

namespace GPC.Converter.MidasCivil
{
    public sealed partial class MidasCivilTextReader
    {
        // Layout verified on native 9.5.5 EXPORTMXT. Other versions/layouts remain preserved.
        private static void ReadPairs(CivilTextBlock block, ImportBatch batch, double length)
        {
            foreach (var line in block.Lines)
            {
                var f = MidasCivilTextSyntax.Fields(line.Text, line.Record(block.Command));
                if (f.Length < 14 || f[1] != "DBUSER" || (f[12] != "2L" && f[12] != "2C")) continue;
                var id = Id(f[0], line, block.Command);
                if (batch.Sections.Any(s => s.Id == id)) throw line.Error(block.Command, "Duplicate section ID " + id);
                if (f[13] != "2" || f.Length < 19 || f.Length > 24 || (f[10] != "YES" && f[10] != "NO") || (f[11] != "YES" && f[11] != "NO"))
                {
                    Warn(batch, "CivilPairUnresolved", line.Record(block.Command), "Only the native-tested explicit User dimension layout is mapped.");
                    continue;
                }
                var dimensions = f.Skip(14).Select(v => Number(v, line, block.Command)).ToArray();
                IdenticalSectionPair definition;
                try { definition = MidasIdenticalPairs.Definition(f[12], dimensions, length); }
                catch (ArgumentException ex) { throw line.Error(block.Command, ex.Message); }
                bool centroid = f[3] == "CC" && f.Skip(4).Take(6).All(v => Number(v, line, block.Command) == 0);
                var section = new SectionRecord { Id = id, Name = Required(f[2], line, block.Command), IdenticalPair = definition,
                    Reference = new SectionReference { IsUnresolved = !centroid }, Record = line.Record(block.Command) };
                batch.Sections.Add(section);
                if (!centroid) Warn(batch, "CivilPairOffsetUnresolved", section.Record, "Pair outline imported; this source section offset is not qualified.");
                if (f[11] == "YES") Warn(batch, "CivilPairWarpingPreserved", section.Record, "Source warping option is retained only; no warping stiffness is inferred.");
            }
        }

        private static void ReadSteelMaterials(CivilTextBlock block, ImportBatch batch, double force, double length)
        {
            foreach (var line in block.Lines)
            {
                var f = MidasCivilTextSyntax.Fields(line.Text, line.Record(block.Command));
                if (f.Length != 15 || f[1] != "STEEL" || f[6] != "C" || f[7] != "NO" || (f[9] != "1" && f[9] != "2")) continue;
                var id = Id(f[0], line, block.Command);
                if (batch.Materials.Any(m => m.Id == id)) throw line.Error(block.Command, "Duplicate material ID " + id);
                var material = new MaterialRecord { Id = id, Name = Required(f[2], line, block.Command), Kind = MaterialKind.Steel, Record = line.Record(block.Command) };
                if (f[9] == "1")
                {
                    material.Standard = f[10]; material.Grade = f[12];
                    material.ElasticModulus = Scaled(f[14], force / (length * length), line, block.Command);
                }
                else
                {
                    material.ElasticModulus = Scaled(f[10], force / (length * length), line, block.Command);
                    material.Poisson = Number(f[11], line, block.Command);
                    material.ThermalExpansion = Number(f[12], line, block.Command);
                    material.Density = Scaled(f[13], force / (length * length * length) / 9806.65, line, block.Command);
                }
                batch.Materials.Add(material);
            }
        }
    }
}
