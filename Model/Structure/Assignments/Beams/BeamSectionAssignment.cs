using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restraints;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;

namespace GPC.Model.Structure.Assignments
{
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamSectionAssignment
    {
        [OptionalField, GPC.Model.Core.FingerprintWhenSet]
        [System.Runtime.Serialization.DataMember(IsRequired = false)]
        private GPC.Model.ElementProperties.BeamProperty _property;
        [OptionalField, GPC.Model.Core.FingerprintWhenSet]
        [System.Runtime.Serialization.DataMember(IsRequired = false)]
        private GPC.Model.ElementProperties.BeamProperty _endProperty;
        [field: System.Runtime.Serialization.DataMember(Name = "<Start>k__BackingField", IsRequired = true)]
        public double Start { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<End>k__BackingField", IsRequired = true)]
        public double End { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Section>k__BackingField", IsRequired = true)]
        /// <summary>Legacy concrete API. New material-neutral callers use <see cref = "Property"/>.</summary>
        public ReinforcedConcreteSection Section { get; set; }

        [field: OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<EndSection>k__BackingField", IsRequired = false)]
        public ReinforcedConcreteSection EndSection { get; set; }

        /// <summary>The assigned property, including steel, concrete and composite beam properties.
        /// Concrete values retain their historical serialized representation.</summary>
        public GPC.Model.ElementProperties.BeamProperty Property
        {
            get => SectionLaws.ResolveProperty(_property, Section);
            set
            {
                Section = value as ReinforcedConcreteSection;
                _property = Section is null ? value : null;
            }
        }

        public GPC.Model.ElementProperties.BeamProperty EndProperty
        {
            get => SectionLaws.ResolveProperty(_endProperty, EndSection);
            set
            {
                EndSection = value as ReinforcedConcreteSection;
                _endProperty = EndSection is null ? value : null;
            }
        }

        /// <summary>For Tabulated: exact, absolute stations in the assignment domain. No implicit interpolation.</summary>
        [field: OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Stations>k__BackingField", IsRequired = false)]
        public List<BeamSectionStation> Stations { get; private set; } = new List<BeamSectionStation>();

        [field: System.Runtime.Serialization.DataMember(Name = "<Law>k__BackingField", IsRequired = true)]
        public string Law { get; set; } = "Constant";

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Stations == null)
                Stations = new List<BeamSectionStation>();
        }
    }
}
