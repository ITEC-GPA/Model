using System;
using System.Linq;
using GPC.Model.Elements;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;

namespace GPC.Model.Results.Processing
{
    /// <summary>Physical identity of nodal actions, independent of their numerical representation.</summary>
    public static class NodalActions
    {
        public static void Validate(Models.Model model, NodeElement node, NodeResultForces sample)
        {
            if (model == null || node == null || sample == null) throw new ArgumentNullException();
            if (!model.NodesElements.Values.Any(n => ReferenceEquals(n, node))) throw new ArgumentException("ForeignResultNode");
            if (!Enum.IsDefined(typeof(NodalForceKind), sample.Kind) || sample.Kind == NodalForceKind.Unknown)
                throw new ArgumentException("UnresolvedNodalForceKind");
            if (sample.Body != ActionBody.OnNode && sample.Body != ActionBody.OnSupport && sample.Body != ActionBody.OnElement)
                throw new ArgumentException("UnresolvedNodalActionBody");
            if (sample.OwnerElementId.HasValue != sample.OwnerElementFamily.HasValue) throw new ArgumentException("IncompleteNodalForceOwner");
            bool elementForce = sample.Kind == NodalForceKind.ElementEndForce || sample.Kind == NodalForceKind.ElementNodeForce;
            if (elementForce)
            {
                if (!sample.OwnerElementId.HasValue || sample.Body == ActionBody.OnSupport) throw new ArgumentException("InvalidElementNodalForceOwnerOrBody");
                var owner = model.AllElements.SingleOrDefault(e => e.Id == sample.OwnerElementId && Models.Model.FamilyOf(e) == sample.OwnerElementFamily);
                if (owner is BeamElement beam)
                {
                    if (sample.Kind != NodalForceKind.ElementEndForce
                        || !(sample.ElementEnd == "I" && ReferenceEquals(beam.NodeI, node) || sample.ElementEnd == "J" && ReferenceEquals(beam.NodeJ, node)))
                        throw new ArgumentException("NodalForceBeamEndMismatch");
                }
                else if (owner is AreaElement plate)
                {
                    if (sample.Kind != NodalForceKind.ElementNodeForce || sample.ElementEnd != null || !plate.Nodes.Any(n => ReferenceEquals(n, node)))
                        throw new ArgumentException("NodalForcePlateNodeMismatch");
                }
                else throw new NotSupportedException("NodalForceOwnerMustBeBeamOrShell");
            }
            else
            {
                if (sample.OwnerElementId.HasValue || sample.ElementEnd != null) throw new ArgumentException("UnexpectedElementOwnerOnNodalReaction");
                if (sample.Kind == NodalForceKind.SupportReaction && sample.Body == ActionBody.OnElement) throw new ArgumentException("InvalidSupportReactionBody");
                if ((sample.Kind == NodalForceKind.SpringForce || sample.Kind == NodalForceKind.LinkForce) && string.IsNullOrWhiteSpace(sample.AggregationSet))
                    throw new ArgumentException("SpringOrLinkResultRequiresSourceIdentity");
            }
        }
    }
}
