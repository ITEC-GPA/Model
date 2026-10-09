using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;
using GPC.Model.ElementProperties;
using GPC.Model.Core;
using GPC.Model.Sections.Concrete;

using GPC.Model.PostProcessing;

namespace GPC.Model.PostProcessing
{
    /// <summary>Design alternatives keyed by persistent element GUID. Setters capture values; later caller edits do not change the scenario.</summary>
    [Serializable]
    public sealed class VerificationScenario
    {
        private readonly Dictionary<Guid, byte[]> _beams = new Dictionary<Guid, byte[]>();
        private readonly Dictionary<Guid, byte[]> _sections = new Dictionary<Guid, byte[]>();
        [OptionalField, FingerprintWhenSet] private Dictionary<Guid, byte[]> _plateSections;
        private readonly Dictionary<Guid, byte[]> _shells = new Dictionary<Guid, byte[]>();
        public string Id { get; private set; }
        public string AnalysisId { get; private set; }
        public string Name { get; private set; }
        private VerificationScenario() { }
        public static VerificationScenario Create(string analysisId, string name)
        {
            if (string.IsNullOrWhiteSpace(analysisId) || string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Analysis identity and scenario name required.");
            return new VerificationScenario { Id = Guid.NewGuid().ToString("D"), AnalysisId = analysisId, Name = name };
        }
        public string Fingerprint => ModelValues.Fingerprint(new object[] { Id, AnalysisId, Name,
            _beams.OrderBy(p => p.Key).ToArray(), _sections.OrderBy(p => p.Key).ToArray(), _shells.OrderBy(p => p.Key).ToArray() }.Concat(_plateSections == null ? Array.Empty<object>() : new object[] { _plateSections.OrderBy(p=>p.Key).ToArray() }));
        public void SetBeamDesign(Guid element, BeamProperty property)
        {
            if (element == Guid.Empty || property is null) throw new ArgumentException("Element GUID and beam property required.");
            _beams[element] = AnalysisStorage.Write(property);
        }
        public void SetBeamSections(Guid element, IEnumerable<BeamSectionAssignment> sections)
        {
            if (element == Guid.Empty || sections == null) throw new ArgumentException("Element GUID and section assignments required.");
            var items = sections.ToArray();
            if (items.Any(s => s == null)) throw new ArgumentException("Null section assignment.");
            _sections[element] = AnalysisStorage.Write(items);
        }
        public void SetShellDesign(Guid element, ShellAssignments assignments)
        {
            if (element == Guid.Empty || assignments == null) throw new ArgumentException("Element GUID and shell assignments required.");
            _shells[element] = AnalysisStorage.Write(assignments);
        }
        /// <summary>Captures a complete plate section, including reinforcement. Replaces legacy element-owned resistant data.</summary>
        public void SetShellSection(Guid element, PlateProperty property)
        {
            if(element==Guid.Empty || property is null) throw new ArgumentException("Element GUID and plate property required.");
            PlateSectionValidation.ValidateProperty(property);
            var bytes=AnalysisStorage.Write(property);
            if(_plateSections==null) _plateSections=new Dictionary<Guid,byte[]>();
            _plateSections[element]=bytes;
        }
        internal void Apply(Models.Model model)
        {
            foreach (var item in _beams)
            {
                var beam = model.BeamElements.Values.SingleOrDefault(b => b.Guid == item.Key)
                    ?? throw new ArgumentException("ScenarioBeamNotFound: " + item.Key);
                if (!_sections.ContainsKey(item.Key) && beam.Assignments.Sections.Count != 0
                    && (beam.Assignments.Sections.Count != 1 || beam.Assignments.Sections[0].Law != "Constant"
                        || beam.Assignments.Sections[0].Start != 0 || beam.Assignments.Sections[0].End != 1))
                    throw new ArgumentException("Variable sections require explicit SetBeamSections.");
                beam.BeamProperty = AnalysisStorage.Read<BeamProperty>(item.Value);
                if (!_sections.ContainsKey(item.Key) && beam.Assignments.Sections.Count == 1)
                {
                    // Replace both legacy and general slots without leaving conflicting definitions.
                    beam.Assignments.Sections[0].Section = null;
                    beam.Assignments.Sections[0].Property = beam.BeamProperty;
                }
            }
            foreach (var item in _sections)
            {
                var beam = model.BeamElements.Values.SingleOrDefault(b => b.Guid == item.Key)
                    ?? throw new ArgumentException("ScenarioBeamNotFound: " + item.Key);
                beam.Assignments.Sections.Clear(); beam.Assignments.Sections.AddRange(AnalysisStorage.Read<BeamSectionAssignment[]>(item.Value));
            }
            foreach (var item in _shells)
            {
                var shell = model.AreaElements.Values.SingleOrDefault(a => a.Guid == item.Key)
                    ?? throw new ArgumentException("ScenarioShellNotFound: " + item.Key);
                var value = AnalysisStorage.Read<ShellAssignments>(item.Value);
                shell.Assignments.PhysicalThickness = value.PhysicalThickness; shell.Assignments.Offset = value.Offset;
                shell.Assignments.LayerAxes = value.LayerAxes; shell.Assignments.ReinforcementZone = value.ReinforcementZone;
                shell.Assignments.Layers.Clear(); shell.Assignments.Layers.AddRange(value.Layers);
            }
            if(_plateSections!=null) foreach(var item in _plateSections)
            {
                var shell=model.AreaElements.Values.SingleOrDefault(a=>a.Guid==item.Key) ?? throw new ArgumentException("ScenarioShellNotFound: "+item.Key);
                if(_shells.ContainsKey(item.Key) && (shell.Assignments.Layers.Count!=0 || shell.Assignments.PhysicalThickness.HasValue))
                    throw new ArgumentException("ConflictingScenarioPlateStorage");
                shell.PlateProperty=AnalysisStorage.Read<PlateProperty>(item.Value);
                shell.Assignments.Layers.Clear(); shell.Assignments.PhysicalThickness=null;
            }
        }
    }
}
