using System;
using System.Linq;
using GPC.Model.Checking.Reports;

namespace GPC.Model.Structure.Members
{
    /// <summary>Supports of the design span of a deflection check: both span ends (chord through the displaced supports), or a
    /// cantilever supported at the span start or at the span end (deflection relative to the translation of the supported end).</summary>
    public enum MemberDeflectionScheme
    {
        SupportedEnds,
        CantileverAtStart,
        CantileverAtEnd
    }

    /// <summary>A declared value with the source that qualifies it (standard clause or table row, national annex, project agreement,
    /// fabrication drawing). Never a default.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class MemberDeclaredValue
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Value>k__BackingField", IsRequired = true)]
        public double Value { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Source>k__BackingField", IsRequired = true)]
        public string Source { get; private set; }

        public MemberDeclaredValue(double value, string source)
        {
            CheckValue.Finite(value, nameof(value));
            CheckValue.Text(source, nameof(source));
            Value = value;
            Source = source;
        }
    }

    /// <summary>
    /// Serviceability declarations that a physical member carries for its deformation tasks (MemberCheckSpecification.Deformation):
    /// support scheme and cantilever reference length of the deflection limits, n of the limits L/n of delta_max (EN 1990 w_max) and of
    /// the variable actions (NTC delta_2, EN 1990 w_3), precamber, n of the drift limit h/n and minimum natural frequency, each with its
    /// source. A value left null is taken from the configuration of the engine; the engine reports the origin of every value it uses.
    /// The values are declarations: their engineering truth stays with the designer, no value of a standard is implied.
    /// </summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class MemberServiceabilityLimits
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<DeflectionScheme>k__BackingField", IsRequired = true)]
        public MemberDeflectionScheme? DeflectionScheme { get; private set; }

        /// <summary>Evidence that the member is supported as declared at the ends of its design span; required with the scheme.</summary>
        [field: System.Runtime.Serialization.DataMember(Name = "<DeflectionSchemeSource>k__BackingField", IsRequired = true)]
        public string DeflectionSchemeSource { get; private set; }

        /// <summary>Reference length of the deflection limits of a cantilever as a multiple of its length (for example 2 where the
        /// applied rule refers the limit of a cantilever to twice its length). Used only when the scheme in force is a cantilever.</summary>
        [field: System.Runtime.Serialization.DataMember(Name = "<CantileverLengthFactor>k__BackingField", IsRequired = true)]
        public MemberDeclaredValue CantileverLengthFactor { get; private set; }

        /// <summary>n of the limit L/n on delta_max = |delta_tot + precamber|.</summary>
        [field: System.Runtime.Serialization.DataMember(Name = "<MaxDeflectionRatio>k__BackingField", IsRequired = true)]
        public MemberDeclaredValue MaxDeflectionRatio { get; private set; }

        /// <summary>Signed precamber (mm) along the checked member axis at the node of the largest total deflection; 0 for none.</summary>
        [field: System.Runtime.Serialization.DataMember(Name = "<Precamber>k__BackingField", IsRequired = true)]
        public MemberDeclaredValue Precamber { get; private set; }

        /// <summary>n of the limit L/n on the deflection of the variable actions.</summary>
        [field: System.Runtime.Serialization.DataMember(Name = "<VariableDeflectionRatio>k__BackingField", IsRequired = true)]
        public MemberDeclaredValue VariableDeflectionRatio { get; private set; }

        /// <summary>n of the limit h/n on the relative transverse displacement of the span ends.</summary>
        [field: System.Runtime.Serialization.DataMember(Name = "<DriftRatio>k__BackingField", IsRequired = true)]
        public MemberDeclaredValue DriftRatio { get; private set; }

        /// <summary>Minimum natural frequency (Hz).</summary>
        [field: System.Runtime.Serialization.DataMember(Name = "<MinimumFrequency>k__BackingField", IsRequired = true)]
        public MemberDeclaredValue MinimumFrequency { get; private set; }

        public MemberServiceabilityLimits(MemberDeflectionScheme? deflectionScheme = null, string deflectionSchemeSource = null,
            MemberDeclaredValue cantileverLengthFactor = null, MemberDeclaredValue maxDeflectionRatio = null, MemberDeclaredValue precamber = null,
            MemberDeclaredValue variableDeflectionRatio = null, MemberDeclaredValue driftRatio = null, MemberDeclaredValue minimumFrequency = null)
        {
            if (deflectionScheme.HasValue && !Enum.IsDefined(typeof(MemberDeflectionScheme), deflectionScheme.Value))
                throw new ArgumentException("InvalidMemberDeflectionScheme");
            if (deflectionScheme.HasValue != (deflectionSchemeSource != null)) throw new ArgumentException("DeflectionSchemeAndSourceTogether");
            if (deflectionSchemeSource != null) CheckValue.Text(deflectionSchemeSource, nameof(deflectionSchemeSource));
            foreach (var (value, name) in new[] { (cantileverLengthFactor, nameof(cantileverLengthFactor)), (maxDeflectionRatio, nameof(maxDeflectionRatio)),
                (variableDeflectionRatio, nameof(variableDeflectionRatio)), (driftRatio, nameof(driftRatio)), (minimumFrequency, nameof(minimumFrequency)) })
                if (value != null && !(value.Value > 0)) throw new ArgumentOutOfRangeException(name, "A positive value is required.");
            // A span supported at both ends uses L = span: a cantilever reference there would be a dead declaration.
            if (deflectionScheme == MemberDeflectionScheme.SupportedEnds && cantileverLengthFactor != null)
                throw new ArgumentException("CantileverReferenceOnSupportedSpan");
            if (!deflectionScheme.HasValue && new[] { cantileverLengthFactor, maxDeflectionRatio, precamber, variableDeflectionRatio, driftRatio, minimumFrequency }.All(v => v == null))
                throw new ArgumentException("EmptyMemberServiceabilityLimits");
            DeflectionScheme = deflectionScheme;
            DeflectionSchemeSource = deflectionSchemeSource;
            CantileverLengthFactor = cantileverLengthFactor;
            MaxDeflectionRatio = maxDeflectionRatio;
            Precamber = precamber;
            VariableDeflectionRatio = variableDeflectionRatio;
            DriftRatio = driftRatio;
            MinimumFrequency = minimumFrequency;
        }
    }
}
