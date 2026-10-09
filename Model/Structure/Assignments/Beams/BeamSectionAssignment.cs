using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restrains;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;

namespace GPC.Model.PostProcessing
{

    [Serializable]
    public sealed class BeamSectionAssignment
    {
        [OptionalField, GPC.Model.Core.FingerprintWhenSet]
        private GPC.Model.ElementProperties.BeamProperty _property;
        [OptionalField, GPC.Model.Core.FingerprintWhenSet]
        private GPC.Model.ElementProperties.BeamProperty _endProperty;

        public double Start { get; set; }
        public double End { get; set; }
        /// <summary>Legacy concrete API. New material-neutral callers use <see cref="Property"/>.</summary>
        public ReinforcedConcreteSection Section { get; set; }
        [field: OptionalField] public ReinforcedConcreteSection EndSection { get; set; }
        /// <summary>The assigned property, including steel, concrete and composite beam properties.
        /// Concrete values retain their historical serialized representation.</summary>
        public GPC.Model.ElementProperties.BeamProperty Property
        {
            get => SectionLaws.ResolveProperty(_property, Section);
            set { Section = value as ReinforcedConcreteSection; _property = Section is null ? value : null; }
        }
        public GPC.Model.ElementProperties.BeamProperty EndProperty
        {
            get => SectionLaws.ResolveProperty(_endProperty, EndSection);
            set { EndSection = value as ReinforcedConcreteSection; _endProperty = EndSection is null ? value : null; }
        }
        /// <summary>For Tabulated: exact, absolute stations in the assignment domain. No implicit interpolation.</summary>
        [field: OptionalField] public List<BeamSectionStation> Stations { get; private set; } = new List<BeamSectionStation>();
        public string Law { get; set; } = "Constant";
        [OnDeserialized] private void OnDeserialized(StreamingContext context) { if (Stations == null) Stations = new List<BeamSectionStation>(); }
    }
}
