using System;
using GPC.Model.Structure;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results.Processing;

namespace GPC.Model.Models
{
    public partial class Model
    {
        public Dictionary<string,PhysicalSurfaceDefinition> PhysicalSurfaces { get; private set; } = new Dictionary<string,PhysicalSurfaceDefinition>(StringComparer.Ordinal);
        public IReadOnlyList<ModelDiagnostic> ValidatePhysicalSurfaces()
        {
            var issues=new List<ModelDiagnostic>(); var owners=new HashSet<int>();
            foreach(var pair in PhysicalSurfaces)
            {
                if(pair.Value==null || pair.Key!=pair.Value.Id) { issues.Add(ModelDiagnostic.Error("InvalidPhysicalSurfaceIdentity",message:pair.Key)); continue; }
                try
                {
                    foreach(var element in pair.Value.ResolveElements(this))
                        if(!owners.Add(element.Id)) issues.Add(ModelDiagnostic.Error("PlateInMultiplePhysicalSurfaces",element));
                }
                catch(ArgumentException ex) { issues.Add(ModelDiagnostic.Error(ex.Message,message:pair.Key)); }
            }
            return issues.AsReadOnly();
        }
        public PhysicalSurfaceDefinition SurfaceForElement(int elementId)
        {
            var matches=PhysicalSurfaces.Values.Where(s=>s!=null && s.ElementIds.Contains(elementId)).ToArray();
            if(matches.Length>1) throw new InvalidOperationException("PlateInMultiplePhysicalSurfaces");
            return matches.SingleOrDefault();
        }
        /// <summary>Assigns an owned section to every complete FEM plate in a surface/zone. Call on an edit-session draft.
        /// The plates are the sole owners of the assignment: zones only select IDs, so no section can diverge from a zone copy.
        /// Validation and cloning precede all mutations. Analysis provenance is never rewritten.</summary>
        public IReadOnlyList<int> AssignSurfaceSection(string surfaceId, PlateProperty property, string zoneId=null, CoordinateSystem sectionAxes=null)
        {
            if(surfaceId==null || !PhysicalSurfaces.TryGetValue(surfaceId,out var surface)) throw new ArgumentException("UnknownPhysicalSurface");
            var issues=ValidatePhysicalSurfaces(); if(issues.Count!=0) throw new ArgumentException(issues[0].Code);
            PlateSectionValidation.ValidateProperty(property);
            if(!property.PhysicalThickness.HasValue) throw new ArgumentException("MissingPhysicalThickness");
            var elements=surface.ResolveElements(this,zoneId);
            var axes=new List<CoordinateSystem>();
            foreach(var element in elements)
            {
                var frame=sectionAxes ?? element.Assignments.LayerAxes ?? element.CoordinateSystem;
                PostProcessing.Axes.Validate(frame); PostProcessing.Axes.Validate(element.CoordinateSystem);
                if(Math.Abs(PostProcessing.Axes.Dot(frame.V3,element.CoordinateSystem.V3))<1-1e-8)
                    throw new ArgumentException("SurfaceSectionAxesNotInPlatePlane");
                // Origins locate the same reference plane; physical offset remains an explicit element assignment.
                if(Math.Abs(PostProcessing.Axes.Dot(frame.Origin-element.CoordinateSystem.Origin,element.CoordinateSystem.V3))>1e-6)
                    throw new ArgumentException("SurfaceSectionReferencePlaneMismatch");
                axes.Add(ActionTransformations.AtPoint(frame,frame.Origin));
            }
            var owned=ModelArchive.CopyValue(property);
            for(int i=0;i<elements.Count;i++)
            {
                elements[i].PlateProperty=owned; elements[i].Assignments.LayerAxes=axes[i];
                elements[i].Assignments.Layers.Clear(); elements[i].Assignments.PhysicalThickness=null;
            }
            return Array.AsReadOnly(elements.Select(e=>e.Id).ToArray());
        }
    }
}
