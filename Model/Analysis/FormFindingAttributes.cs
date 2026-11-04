using System;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Analysis
{
    /// <summary>
    /// A form-finding objective carried by a beam before the analysis definition exists (application input, e.g. a Grasshopper
    /// beam that receives a force density): the objective and the load case of the pre-load it produces. The application moves
    /// the attributes into <see cref="FormFindingDefinition.Goals"/> of the analysis graph, binding <see cref="FormFindingGoal.BeamGuid"/>
    /// to the beam; the <see cref="FormFindingGoal.BeamGuid"/> stored here is not used. Several attributes on one beam keep their order
    /// (collection id), so the application can apply its own precedence. Units as <see cref="FormFindingGoal"/>.
    /// </summary>
    [Serializable]
    public sealed class FormFindingGoalAttribute : Attributes.Attribute
    {
        /// <summary>The objective (not null).</summary>
        public FormFindingGoal Goal { get; private set; }
        /// <summary>The load case of the pre-load found for the beam; null when the objective has none.</summary>
        public LoadCaseBase LoadCase { get; set; }

        public FormFindingGoalAttribute(FormFindingGoal goal, LoadCaseBase loadCase)
        {
            Goal = goal ?? throw new ArgumentNullException(nameof(goal));
            LoadCase = loadCase;
        }

        private FormFindingGoalAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SerializationVersions.Read(info, "FormFindingGoalAttributeVersion", 1);
            Goal = (FormFindingGoal)info.GetValue("Goal", typeof(FormFindingGoal)) ?? throw new SerializationException("MissingFormFindingGoal");
            LoadCase = (LoadCaseBase)info.GetValue("LoadCase", typeof(LoadCaseBase));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("FormFindingGoalAttributeVersion", 1);
            info.AddValue("Goal", Goal);
            info.AddValue("LoadCase", LoadCase);
        }
    }

    /// <summary>
    /// Penalties on the initial global coordinates of a node before the analysis definition exists (application input): the
    /// application moves them into <see cref="FormFindingDefinition.CoordinateGoals"/>. Zero disables a coordinate objective.
    /// </summary>
    [Serializable]
    public sealed class FormFindingCoordinateAttribute : Attributes.Attribute
    {
        /// <summary>The weights along global X, Y and Z (dimensionless, not null).</summary>
        public Vector3d Weights { get; private set; }

        public FormFindingCoordinateAttribute(Vector3d weights)
        {
            Weights = weights ?? throw new ArgumentNullException(nameof(weights));
        }

        private FormFindingCoordinateAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SerializationVersions.Read(info, "FormFindingCoordinateAttributeVersion", 1);
            Weights = (Vector3d)info.GetValue("Weights", typeof(Vector3d)) ?? throw new SerializationException("MissingCoordinateWeights");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("FormFindingCoordinateAttributeVersion", 1);
            info.AddValue("Weights", Weights);
        }
    }
}
