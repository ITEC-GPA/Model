using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GPC.Converter.Straus7
{
    /// <summary>A freedom case of a Straus7 model: number and name (names may repeat: the number tells them apart).</summary>
    public sealed class Straus7FreedomCaseInfo
    {
        public int Number { get; set; }
        public string Name { get; set; }
    }

    /// <summary>A load case that the linear static solver solves under a freedom case (St7GetLSALoadCaseState), with the name of its Model load case.</summary>
    public sealed class Straus7LinearCasePair
    {
        public int LoadCase { get; set; }
        public string Case { get; set; }
        public int FreedomCase { get; set; }
    }

    /// <summary>The freedom cases of a Straus7 model with several of them and the load case / freedom case pairs of its linear static solver
    /// settings, read by <see cref="Straus7ApiConverter"/> and kept as one preserved record of the Model (<see cref="Kind"/>); a model with
    /// one freedom case has none. GPC.Model keeps the results of a load case, not of a pair: a result case maps to its load case when the
    /// settings solve that load case under one freedom case (<see cref="Straus7LinearStaticResults.Cases"/>), and an LSA combination is a
    /// Model combination when each of its terms is such a pair. Restraints and node springs of the freedom cases are not mapped.</summary>
    public sealed class Straus7FreedomCaseData
    {
        public const string Kind = "Straus7 freedom cases", Record = "Straus7FreedomCases";
        public List<Straus7FreedomCaseInfo> FreedomCases { get; set; } = new List<Straus7FreedomCaseInfo>();
        public List<Straus7LinearCasePair> LinearCases { get; set; } = new List<Straus7LinearCasePair>();

        /// <summary>The freedom cases under which the settings solve <paramref name="loadCase"/>, ascending.</summary>
        public IReadOnlyList<int> FreedomCasesOf(int loadCase) => LinearCases.Where(p => p.LoadCase == loadCase).Select(p => p.FreedomCase).Distinct().OrderBy(f => f).ToArray();

        /// <summary>True when the settings solve <paramref name="loadCase"/> under <paramref name="freedomCase"/>.</summary>
        public bool Solves(int loadCase, int freedomCase) => LinearCases.Any(p => p.LoadCase == loadCase && p.FreedomCase == freedomCase);

        /// <summary>The short label of a freedom case, unique when names repeat: "FC 2".</summary>
        public static string Label(int freedomCase) => "FC " + freedomCase.ToString(CultureInfo.InvariantCulture);

        /// <summary>The data of a Model imported from a Straus7 model with several freedom cases; null otherwise (one freedom case, or an older converter).</summary>
        public static Straus7FreedomCaseData Read(GPC.Model.Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var record = model.PreservedSourceData.LastOrDefault(p => p.Kind == Kind && p.SourceRecord == Record && p.RawData != null);
            if (record == null) return null;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(record.RawData)))
            {
                var data = (Straus7FreedomCaseData)new DataContractJsonSerializer(typeof(Straus7FreedomCaseData)).ReadObject(stream);
                data.FreedomCases = data.FreedomCases ?? new List<Straus7FreedomCaseInfo>(); data.LinearCases = data.LinearCases ?? new List<Straus7LinearCasePair>();
                return data;
            }
        }

        internal GPC.Model.Structure.Assignments.PreservedAssignment Preserve()
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(Straus7FreedomCaseData)).WriteObject(stream, this);
                return new GPC.Model.Structure.Assignments.PreservedAssignment { Kind = Kind, SourceRecord = Record, RawData = Encoding.UTF8.GetString(stream.ToArray()),
                    UnitsAndAxes = "Straus7 numbers of load cases and freedom cases; Case is the name of the Model load case.",
                    UnsupportedReason = "GPC.Model has no load case / freedom case pairing and no restraints of several freedom cases: the pairs are data." };
            }
        }

        internal string Summary()
        {
            string Numbers(IEnumerable<int> values) => string.Join(", ", values.Select(v => v.ToString(CultureInfo.InvariantCulture)));
            var parts = FreedomCases.Select(f =>
            {
                var cases = LinearCases.Where(p => p.FreedomCase == f.Number).Select(p => p.LoadCase).OrderBy(l => l).ToArray();
                return Label(f.Number) + " '" + f.Name + "' (" + (cases.Length == 0 ? "no load case" : "load cases " + Numbers(cases)) + ")";
            });
            var shared = LinearCases.GroupBy(p => p.LoadCase).Where(g => g.Select(p => p.FreedomCase).Distinct().Count() > 1).Select(g => g.Key).OrderBy(l => l).ToArray();
            return FreedomCases.Count.ToString(CultureInfo.InvariantCulture) + " freedom cases in the linear static settings: " + string.Join("; ", parts) + "."
                + (shared.Length == 0 ? "" : " Load cases " + Numbers(shared) + " are solved under several freedom cases: their result cases and the LSA combinations that use them are not mapped.")
                + " A result case maps to its load case when the load case is solved under one freedom case; restraints and node springs of the freedom cases are not mapped.";
        }
    }

    /// <summary>A linear static result case with its load case and freedom case (<see cref="Straus7LinearStaticResults.Cases"/>). LoadCase and
    /// FreedomCase come from the native name; FreedomCase is null when the name gives none (a solution of one freedom case). ModelCase is the
    /// Model load case of the results, null when the case cannot be mapped (Reason).</summary>
    public sealed class Straus7LinearStaticCase
    {
        /// <summary>The result case number.</summary>
        public int Number { get; internal set; }
        /// <summary>The native name (St7GetResultCaseName).</summary>
        public string Name { get; internal set; }
        public int? LoadCase { get; internal set; }
        public int? FreedomCase { get; internal set; }
        public string FreedomCaseName { get; internal set; }
        public string ModelCase { get; internal set; }
        /// <summary>"&lt;load case&gt;: &lt;name&gt;", with " · FC &lt;freedom case&gt;" for a result case of several freedom cases: unique when names repeat.</summary>
        public string Label { get; internal set; }
        public string Reason { get; internal set; }
    }
}
