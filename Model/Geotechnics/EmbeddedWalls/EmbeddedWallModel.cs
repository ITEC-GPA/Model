using System;
using System.Collections.Generic;

namespace GPC.Model.Geotechnics.EmbeddedWalls
{
    /// <summary>All lengths/elevations in mm, forces N, moments Nmm, stresses MPa; z upwards, horizontal displacement to the right.
    /// The model represents the declared tributary width, not automatically one metre. Inputs are copied by the analysis.</summary>
    public sealed class EmbeddedWallModel
    {
        public string Name { get; set; } = "Paratia";
        public string CaseId { get; set; } = "Caratteristico";
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double TributaryWidth { get; set; } = 1000;
        public double MaximumElementLength { get; set; } = 250;
        public bool FixTopDisplacement { get; set; }
        public bool FixTopRotation { get; set; }
        public bool FixBottomDisplacement { get; set; }
        public bool FixBottomRotation { get; set; }
        public IReadOnlyList<EmbeddedWallSegment> Segments { get; set; } = Array.Empty<EmbeddedWallSegment>();
        public IReadOnlyList<EmbeddedWallSoil> LeftSoils { get; set; } = Array.Empty<EmbeddedWallSoil>();
        public IReadOnlyList<EmbeddedWallSoil> RightSoils { get; set; } = Array.Empty<EmbeddedWallSoil>();
        public IReadOnlyList<EmbeddedWallSupport> Supports { get; set; } = Array.Empty<EmbeddedWallSupport>();
        public IReadOnlyList<EmbeddedWallStage> Stages { get; set; } = Array.Empty<EmbeddedWallStage>();
    }

    public sealed class EmbeddedWallSegment
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        /// <summary>EI of the represented strip (N mm²); no hidden cracking reduction.</summary>
        public double FlexuralRigidity { get; set; }
        public double WeightPerLength { get; set; }
        public string SectionId { get; set; } = "";
        public string Source { get; set; } = "";
    }

    public enum EmbeddedWallDrainage { Drained, UndrainedTotalStress }

    public sealed class EmbeddedWallSoil
    {
        public Soil Soil { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        /// <summary>Pressure/displacement stiffness, N/mm³. Must be supplied with a source; never inferred from a soil name.</summary>
        public double HorizontalModulus { get; set; }
        public double AtRestCoefficient { get; set; }
        public EmbeddedWallDrainage Drainage { get; set; }
        public string StiffnessSource { get; set; } = "";
    }

    public sealed class EmbeddedWallGround
    {
        public double Elevation { get; set; }
        public double? WaterElevation { get; set; }
        /// <summary>Uniform surface surcharge (MPa), resolved for this analysis case.</summary>
        public double Surcharge { get; set; }
    }

    public enum EmbeddedWallSupportKind { Anchor, Strut, Bilateral }

    public sealed class EmbeddedWallSupport
    {
        public string Id { get; set; } = "";
        public double Elevation { get; set; }
        public EmbeddedWallSupportKind Kind { get; set; }
        /// <summary>Axial stiffness of one physical support EA/L, N/mm.</summary>
        public double AxialStiffness { get; set; }
        /// <summary>Inclination from horizontal, radians. Positive downwards.</summary>
        public double Inclination { get; set; }
        /// <summary>Positive movement to the right increases axial force when +1, to the left when -1.</summary>
        public int ResistingDirection { get; set; } = 1;
        public double Spacing { get; set; } = 1000;
        /// <summary>Axial force cap per physical support, N; null = elastic without a cap.</summary>
        public double? Capacity { get; set; }
    }

    public sealed class EmbeddedWallSupportActivation
    {
        public string SupportId { get; set; } = "";
        /// <summary>Axial lock-off force per physical support, N. Installation uses the previous committed wall displacement.</summary>
        public double Prestress { get; set; }
    }

    public sealed class EmbeddedWallPointLoad
    {
        public double Elevation { get; set; }
        public double HorizontalForce { get; set; }
        /// <summary>Generalized moment conjugate to du/d(depth), Nmm.</summary>
        public double Moment { get; set; }
        public double DownwardForce { get; set; }
    }

    /// <summary>Complete stage snapshot. Omission of a formerly active support removes it; reinstalling creates a new reference.
    /// Stages are an ordered history. Independent combinations must run independent histories.</summary>
    public sealed class EmbeddedWallStage
    {
        public string Id { get; set; } = "";
        public EmbeddedWallGround Left { get; set; } = new EmbeddedWallGround();
        public EmbeddedWallGround Right { get; set; } = new EmbeddedWallGround();
        public IReadOnlyList<EmbeddedWallSupportActivation> ActiveSupports { get; set; } = Array.Empty<EmbeddedWallSupportActivation>();
        public IReadOnlyList<EmbeddedWallPointLoad> PointLoads { get; set; } = Array.Empty<EmbeddedWallPointLoad>();
        /// <summary>Uniform horizontal distributed action on the represented strip, N/mm.</summary>
        public double DistributedLoad { get; set; }
        public double StiffnessFactor { get; set; } = 1;
    }
}
