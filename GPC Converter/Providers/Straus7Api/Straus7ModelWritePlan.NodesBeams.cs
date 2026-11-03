using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Attributes;
using GPC.Model.Core;
using GPC.Model.Core.Coordinates;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.Straus7
{
    public sealed partial class Straus7ModelWritePlan
    {
        private sealed partial class Builder
        {
            private void Node(NodeElement node)
            {
                int id=Entities[node.Guid];var p=node.Assignments;
                if(p.Active==false || p.Dofs!=null || node.Attributes.Count!=0 || p.Links.Count!=0 || p.Mass!=null)
                    throw new NotSupportedException("StrausNodeDofsLinksMassOrAttributesNeedExplicitMapping");
                if(!Axes.IsFinite(node.Position))throw new ArgumentException("InvalidStrausNodePosition");
                // Straus7 groups contain elements, not nodes. Nodal groups remain organizational GPC metadata.
                var xyz=new[]{node.Position.X,node.Position.Y,node.Position.Z};Operations.Add(a=>a.SetNode(id,(double[])xyz.Clone()));
                var scopes=new HashSet<string>(StringComparer.Ordinal);
                if(p.Restrains.Any(r=>r==null||r.Restrain==null))throw new ArgumentException("MissingStrausNodeRestraint");
                foreach(var restraint in p.Restrains)
                {
                    bool prescribed=restraint.Restrain.Restrains.Any(d=>d.HasImposedDisplacement);
                    if(!prescribed)
                    {if(restraint.Case!=null)throw new ArgumentException("CaseScopedRestraintRequiresExplicitFreedomCaseMigration");continue;}
                    var loadCase=restraint.Case as GPC.Model.LoadCases.LoadCaseBase;
                    int load=Load(loadCase),freedom=Freedom(restraint.FreedomCase);
                    if(prescribedCases.TryGetValue(freedom,out int previous)&&previous!=load)throw new NotSupportedException("IndependentPrescribedCasesRequireDistinctFreedomCases");
                    prescribedCases[freedom]=load;
                }
                // St7SetNodeRestraint6 and St7SetNodeK*3F replace all components of one node/native freedom case, so every
                // assignment that resolves to the same native case (an unscoped one included) is merged into one call per attribute.
                var attributes=new List<NodeFreedomAttributes>();
                foreach(var restraint in p.Restrains)
                {
                    if(restraint.Phase!=null)throw new NotSupportedException("PhaseDependentRestraintRequiresStageBoundaryConversion");
                    int fc=Freedom(restraint.FreedomCase);var frame=restraint.Restrain.CoordinateSystem??CoordinateSystem.Global;Axes.Validate(frame);
                    var target=attributes.SingleOrDefault(x=>x.FreedomCase==fc);
                    if(target==null)attributes.Add(target=new NodeFreedomAttributes(fc,frame));
                    else if(!Same(target.Frame,frame))throw new NotSupportedException("StrausNodeRequiresOneRestraintFramePerFreedomCase");
                    foreach(var dof in restraint.Restrain.Restrains)
                    {
                        int k=(int)dof.Dof;if(k<0||k>5||!scopes.Add(fc+":"+k))throw new ArgumentException("OverlappingOrInvalidStrausRestraint");
                        target.Status[k]=dof.IsRestrained||dof.HasImposedDisplacement?1:0;target.Prescribed[k]=dof.ImposedDisplacement;target.Stiffness[k]=dof.Stiffness;
                        if(dof.HasStiffness&&dof.HasImposedDisplacement)throw new NotSupportedException("CoupledPrescribedSpringRequiresExplicitLink");
                    }
                }
                if(p.GroundSprings.Count>1)throw new NotSupportedException("MultipleGroundSpringsRequireExplicitAssembly");
                var groundSprings=new List<NodeFreedomAttributes>();
                foreach(var spring in p.GroundSprings)
                {
                    if(Freedoms.Count>1)throw new NotSupportedException("UnscopedSpringWithMultipleFreedomCases");
                    var frame=spring.CoordinateSystem??CoordinateSystem.Global;Axes.Validate(frame);
                    for(int r=0;r<6;r++)for(int c=0;c<6;c++)if(r!=c&&spring[r,c]!=0)throw new NotSupportedException("CoupledGroundSpringRequiresMatrixLinkWriter");
                    var values=Enumerable.Range(0,6).Select(i=>spring[i,i]).ToArray();Finite(values);
                    for(int i=0;i<6;i++)if(values[i]!=0&&scopes.Contains("1:"+i))throw new ArgumentException("OverlappingStrausSpringAndRestraint");
                    var target=attributes.SingleOrDefault(x=>x.FreedomCase==1);
                    for(int r=0;r<2;r++)
                    {
                        var family=Enumerable.Range(r*3,3).ToArray();if(family.All(i=>values[i]==0))continue;
                        // One native spring call per family and case: it merges with restraint springs only in the same frame.
                        var owner=target!=null&&Same(target.Frame,frame)?target:null;
                        if(owner==null&&target!=null&&family.Any(i=>target.Stiffness[i]!=0))throw new NotSupportedException("StrausNodeSpringFramesConflictInFreedomCase");
                        if(owner==null){owner=groundSprings.SingleOrDefault(x=>Same(x.Frame,frame));if(owner==null)groundSprings.Add(owner=new NodeFreedomAttributes(1,frame,restraintCall:false));}
                        foreach(int i in family)if(values[i]!=0)owner.Stiffness[i]=values[i];
                    }
                }
                var frames=new List<KeyValuePair<CoordinateSystem,int>>();
                foreach(var attribute in attributes.Concat(groundSprings))
                {
                    int fc=attribute.FreedomCase;
                    int ucs=frames.Where(f=>Same(f.Key,attribute.Frame)).Select(f=>f.Value).DefaultIfEmpty(0).First();
                    if(ucs==0){ucs=Ucs(attribute.Frame);frames.Add(new KeyValuePair<CoordinateSystem,int>(attribute.Frame,ucs));}
                    var status=attribute.Status;var prescribed=attribute.Prescribed;var stiffness=attribute.Stiffness;Finite(prescribed);Finite(stiffness);
                    if(attribute.RestraintCall)Operations.Add(a=>a.SetNodeRestraint(id,fc,ucs,(int[])status.Clone(),(double[])prescribed.Clone()));
                    for(int r=0;r<2;r++)
                    {
                        bool rotation=r==1;var values=stiffness.Skip(r*3).Take(3).ToArray();if(values.Any(v=>v!=0))Operations.Add(a=>a.SetNodeSpring(id,fc,ucs,rotation,(double[])values.Clone()));
                    }
                }
                var totals=new Dictionary<int,double[]>();
                foreach(var load in node.Loads.Values)
                {
                    if(!(load is PointLoad point))throw new NotSupportedException("UnsupportedStrausNodalLoad: "+load.GetType().Name);
                    int lc=Load(point.LoadCase);if(!totals.TryGetValue(lc,out var total))totals.Add(lc,total=new double[6]);
                    var values=point.GetGlobalForces();Finite(values);
                    Vector3d offset=point.Point-node.Position;var moment=offset.CrossProduct(new Vector3d(values[0],values[1],values[2]));
                    values[3]+=moment.X;values[4]+=moment.Y;values[5]+=moment.Z;
                    for(int k=0;k<6;k++)total[k]+=values[k];
                }
                foreach(var pair in totals)
                {int lc=pair.Key;Finite(pair.Value);var force=pair.Value.Take(3).ToArray();var moment=pair.Value.Skip(3).ToArray();Operations.Add(a=>a.SetNodeLoad(id,lc,false,(double[])force.Clone()));Operations.Add(a=>a.SetNodeLoad(id,lc,true,(double[])moment.Clone()));}
            }

            private void Beam(BeamElement beam)
            {
                var input=beam.Assignments;int id=Entities[beam.Guid];
                if(beam.NodeI==null||beam.NodeJ==null||!Entities.ContainsKey(beam.NodeI.Guid)||!Entities.ContainsKey(beam.NodeJ.Guid))throw new ArgumentException("UnconnectedStrausBeam");
                if(input.RigidLengthI!=0||input.RigidLengthJ!=0||input.OrientationNodeId.HasValue||input.SectionGeometryAxes!=null||input.OtherAssignments.Count!=0)
                    throw new NotSupportedException("UnsupportedStrausBeamGeometryAssignment");
                if(input.Sections.Count!=0)throw new NotSupportedException("ExplicitBeamSectionScheduleRequiresSubdivision");
                if(beam.Loads.Count!=0)throw new NotSupportedException("BeamLoadsRequireExplicitStationAssignments");
                int formulation=input.Formulation==BeamFormulation.StraightTwoNode?6:input.Formulation==BeamFormulation.Truss?3:input.Formulation==BeamFormulation.Cable?2:input.Formulation==BeamFormulation.CutoffBar?4:
                    input.Formulation==BeamFormulation.PointContact?5:input.Formulation==BeamFormulation.Pipe?8:input.Formulation==BeamFormulation.Connection?9:
                    throw new NotSupportedException("UnsupportedStrausBeamFormulation: "+input.Formulation);
                if(input.SectionAxes==null)throw new ArgumentException("ExplicitBeamSectionAxesRequired");
                var axes=input.SectionAxes;var axesData=AxesData(axes);var tangent=beam.NodeJ.Position-beam.NodeI.Position;double length=Axes.Length(tangent);
                if(!(length>1e-9)||Math.Abs(Axes.Dot(tangent/length,axes.V3)-1)>1e-8)throw new ArgumentException("InconsistentStrausBeamAxes");
                var nodes=new[]{Entities[beam.NodeI.Guid],Entities[beam.NodeJ.Guid]};
                var property=BeamPropertyData(beam);string name=string.IsNullOrWhiteSpace(beam.Name)?"Beam "+id:beam.Name;
                Operations.Add(a=>a.CreateBeamProperty(id,name,formulation,(double[])property.Section.Clone(),(double[])property.Material.Clone()));
                var cutoff=(beam.BeamProperty as ElasticBeamProperty)?.CutoffLaw;
                if((cutoff!=null)!=(formulation==4)||cutoff!=null&&input.AnalysisProfile!=null)throw new ArgumentException("CutoffLawRequiresCutoffBarFormulationAndExplicitSection");
                if(cutoff!=null)
                {
                    cutoff.Validate();RequiresMaterialApi=true;int failure=(int)cutoff.Failure;bool keep=cutoff.KeepMass;double tension=cutoff.TensionLimit,compression=cutoff.CompressionLimit;
                    Operations.Add(a=>((IStraus7MaterialWriteApi)a).SetCutoffLaw(id,failure,keep,tension,compression));
                }
                if(NativeSections.TryGetValue(beam.Guid,out var nativeSection))
                {
                    if(input.AnalysisProfile!=null)throw new NotSupportedException("NativeSectionRequiresPhysicalBeamProperty");
                    var reference=nativeSection.ReferenceProperties;
                    for(int k=0;k<11;k++)if(Math.Abs(reference[k]-property.Section[k])>1e-8*Math.Max(1,Math.Abs(reference[k])))
                        throw new ArgumentException("NativeSectionPhysicalPropertiesMismatch: slot "+k);
                    Operations.Add(a=>((IStraus7NativeSectionWriteApi)a).SetNativeSection(id,nativeSection,(double[])property.Section.Clone()));
                }
                else if(input.AnalysisProfile==null) BeamSectionGeometry(beam,id,property.Section);
                else if(input.Physical?.Taper!=null)throw new NotSupportedException("TaperRequiresDimensionalSectionGeometry");
                if(beamIntegrationSlices.TryGetValue(beam.Guid,out int slices))
                {
                    RequiresSectionGeometryApi=true;
                    Operations.Add(a=>((IStraus7SectionGeometryWriteApi)a).SetBeamIntegrationSlices(id,slices,(double[])property.Section.Clone()));
                }
                var beamMaterial=(beam.BeamProperty as ElasticBeamProperty)?.Material??(beam.BeamProperty as SteelSection)?.SteelMaterial;
                if(cutoff!=null&&beamMaterial?.AnalysisData?.StressStrainPairs!=null)throw new NotSupportedException("CutoffLawCannotBeCombinedWithMaterialStressStrainLaw");
                if(input.AnalysisProfile==null)MaterialLaw(id,1,beamMaterial);
                Operations.Add(a=>a.SetElement(1,id,id,(int[])nodes.Clone()));Group(beam,1);Operations.Add(a=>a.SetBeamAxes(id,(double[])axesData.Clone()));
                var offsetI=Global(input.OffsetI,input.OffsetAxes??axes);var offsetJ=Global(input.OffsetJ,input.OffsetAxes??axes);
                if(Axes.Length(offsetI-offsetJ)>1e-8)throw new NotSupportedException("UnequalBeamOffsetsRequireRigidEndLinks");
                double axial=Axes.Dot(offsetI,axes.V3);if(Math.Abs(axial)>1e-8)throw new NotSupportedException("AxialBeamOffsetRequiresRigidEndLinks");
                var offsets=new[]{Axes.Dot(offsetI,axes.V1),Axes.Dot(offsetI,axes.V2)};
                if(input.SectionCentroidOffset!=null && Math.Abs(input.SectionCentroidOffset.X)+Math.Abs(input.SectionCentroidOffset.Y)>1e-8)
                    throw new NotSupportedException("SectionCentroidOffsetRequiresExplicitReferenceConvention");
                if(offsets.Any(v=>v!=0))Operations.Add(a=>a.SetBeamOffset(id,(double[])offsets.Clone()));
                if(beam.Attributes.Values.Count(a=>a is BeamReleasesAttribute)>1)throw new ArgumentException("DuplicateBeamReleases");
                foreach(var attribute in beam.Attributes.Values)
                {
                    if(!(attribute is BeamReleasesAttribute release)||release.Phase!=null||release.CoordinateSystem!=null&&!Same(release.CoordinateSystem,axes))
                        throw new NotSupportedException("UnsupportedStrausBeamAttributeOrReleaseAxes");
                    for(int end=1;end<=2;end++)for(int r=0;r<2;r++)
                    {
                        int beamEnd=end;bool rotation=r==1;var connections=(end==1?release.I:release.J).Skip(r*3).Take(3).ToArray();
                        if(connections.Any(c=>c==null||c.Kind==BeamConnectionKind.RelativeFixity))throw new NotSupportedException("RelativeBeamFixityRequiresStiffnessConversion");
                        // R3 constants: brReleased=0, brFixed=1, brPartial=2.
                        var status=connections.Select(c=>c.Kind==BeamConnectionKind.Continuous?1:c.Kind==BeamConnectionKind.Released?0:2).ToArray();
                        var values=connections.Select(c=>c.Value).ToArray();Operations.Add(a=>a.SetBeamRelease(id,beamEnd,rotation,(int[])status.Clone(),(double[])values.Clone()));
                    }
                }
                BeamLoads(beam,axes,offsetI);
            }

            /// <summary>Restraint and spring components of one node in one native freedom case, in one frame.</summary>
            private sealed class NodeFreedomAttributes
            {
                internal readonly int FreedomCase;internal readonly CoordinateSystem Frame;internal readonly bool RestraintCall;
                internal readonly int[] Status=new int[6];internal readonly double[] Prescribed=new double[6],Stiffness=new double[6];
                internal NodeFreedomAttributes(int freedomCase,CoordinateSystem frame,bool restraintCall=true){FreedomCase=freedomCase;Frame=frame;RestraintCall=restraintCall;}
            }

            private static Vector3d Global(Vector3d v,CoordinateSystem axes)
            {if(v==null)return new Vector3d(0,0,0);Axes.Validate(axes);return axes.V1*v.X+axes.V2*v.Y+axes.V3*v.Z;}

            internal bool RequiresSectionGeometryApi;
            internal bool RequiresSectionMeshApi;
            private readonly Dictionary<int,int> prescribedCases=new Dictionary<int,int>();

            private void ValidatePrescribedCases()
            {
                if(prescribedCases.Count==0)return;
                if(Model.Stages.Count==0)
                {
                    if(Loads.Count!=1||Freedoms.Count>1||Model.MovingLoads!=null)
                        throw new NotSupportedException("PrescribedDisplacementsWithMultipleCasesRequireExplicitStageFactors");
                    return;
                }
                foreach(var stage in Model.Stages.Values)
                {
                    var state=stage.AnalysisState??throw new ArgumentException("PrescribedDisplacementsRequireExplicitStageState");
                    state.Validate(Model);
                    foreach(var pair in prescribedCases)
                    {
                        Guid freedom=Freedoms.Single(p=>p.Value==pair.Key).Key,load=Loads.Single(p=>p.Value==pair.Value).Key;
                        if(!state.ActiveFreedomCaseGuids.Contains(freedom))continue;
                        foreach(var increment in state.Increments)
                        {
                            double loadFactor=increment.LoadFactors.SingleOrDefault(p=>p.CaseGuid==load)?.Factor??0;
                            double freedomFactor=increment.FreedomFactors.SingleOrDefault(p=>p.CaseGuid==freedom)?.Factor??0;
                            if(Math.Abs(loadFactor-freedomFactor)>1e-12*Math.Max(1,Math.Max(Math.Abs(loadFactor),Math.Abs(freedomFactor))))
                                throw new ArgumentException("PrescribedDisplacementLoadAndFreedomFactorsDiffer");
                        }
                    }
                }
            }
            private void BeamSectionGeometry(BeamElement beam,int id,double[] properties)
            {
                if(beam.Assignments.AnalysisProfile!=null)throw new NotSupportedException("TaperRequiresDimensionalSectionGeometry");
                var section=(beam.BeamProperty as ElasticBeamProperty)?.Section??(beam.BeamProperty as SteelSection)?.SectionShape;
                int shape;double[] dimensions;
                if(section is SectionRectangular rectangle && rectangle.Angle==0)
                {shape=3;dimensions=new[]{rectangle.Width,rectangle.Height,0,0,0,0};}
                else if(section is SectionCircular circle)
                {shape=1;dimensions=new[]{circle.Diameter,0,0,0,0,0};}
                else if(section is SectionCHS pipe)
                {shape=2;dimensions=new[]{pipe.Diameter,0,0,pipe.Thickness,0,0};}
                else if(section is SectionH h && h.R==0)
                {shape=7;dimensions=new[]{h.LenghtTopFlange,h.LenghtBottomFlange,h.Height,h.ThicknessTopFlange,h.ThicknessWeb,h.ThicknessBottomFlange};}
                else if(section is SectionRHS box && box.R==0 && box.ThicknessTop==box.ThicknessBottom && box.ThicknessWebLeft==box.ThicknessWebRight)
                {shape=4;dimensions=new[]{box.Width,box.Height,0,box.ThicknessTop,box.ThicknessWebLeft,0};}
                else
                {
                    var outlines=section is SectionBuiltUp built?built.GetOutlines():section?.Shape==null?Array.Empty<Shape2d>():new[]{section.Shape};
                    if(outlines.Count==0)
                    {
                        if(beam.Assignments.Physical?.Taper!=null)throw new NotSupportedException("TaperRequiresDimensionalSectionGeometry");
                        return; // Explicit generic section properties have no geometric outline.
                    }
                    var vertices=new List<double[]>();var faces=new List<int[]>();
                    foreach(var outline in outlines)
                    {
                        var mesh=new Section(outline).GetMesh(initialMeshOnly:true);
                        if(mesh==null||mesh.Faces.Count==0)throw new ArgumentException("EmptyStrausSectionMesh");
                        var nodeIds=new Dictionary<int,int>();
                        foreach(var vertex in mesh.Vertices)
                        {
                            nodeIds.Add(vertex.Id,vertices.Count);
                            var xyz=new[]{vertex.Point.X,vertex.Point.Y,vertex.Point.Z};Finite(xyz);vertices.Add(xyz);
                        }
                        foreach(var face in mesh.Faces)faces.Add(mesh.GetFaceVertices(face).Select(v=>nodeIds[v.Id]).ToArray());
                    }
                    var xyzData=vertices.ToArray();var faceData=faces.ToArray();
                    RequiresSectionMeshApi=true;
                    Operations.Add(a=>((IStraus7SectionMeshWriteApi)a).SetBeamSectionMesh(id,
                        xyzData.Select(v=>(double[])v.Clone()).ToArray(),faceData.Select(f=>(int[])f.Clone()).ToArray(),(double[])properties.Clone()));
                    return;
                }
                RequiresSectionGeometryApi=true;
                Operations.Add(a=>((IStraus7SectionGeometryWriteApi)a).SetBeamSectionGeometry(id,shape,(double[])dimensions.Clone(),(double[])properties.Clone()));
            }

            /// <summary>Shear areas per section instance of this export only (no static cache).</summary>
            private readonly Dictionary<ISectionShape,(double Area1,double Area2)> shearAreas=new Dictionary<ISectionShape,(double Area1,double Area2)>(ReferenceComparer.Instance);
            private sealed class ReferenceComparer : IEqualityComparer<ISectionShape>
            {
                internal static readonly ReferenceComparer Instance=new ReferenceComparer();
                public bool Equals(ISectionShape x,ISectionShape y)=>ReferenceEquals(x,y);
                public int GetHashCode(ISectionShape value)=>System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
            }
            private (double[] Section,double[] Material) BeamPropertyData(BeamElement beam)
            {
                var profile=beam.Assignments.AnalysisProfile;
                if(profile!=null)
                {
                    if((beam.Assignments.Physical?.TemperatureGradients.Count??0)!=0
                        ||(beam.NodeI.Assignments.Physical?.Temperatures.Count??0)!=0||(beam.NodeJ.Assignments.Physical?.Temperatures.Count??0)!=0)
                        throw new NotSupportedException("ThermalBeamProfileRequiresExplicitThermalRigidity");
                    profile.Validate();var p=profile.At(0);
                    if(profile.Interpolation!=ProfileInterpolation.Constant || profile.Stations.Count!=2)throw new NotSupportedException("VariableBeamRigidityRequiresSubdivision");
                    if(!(p.EA>0))throw new ArgumentException("NonPositiveBeamAxialRigidity");
                    // Explicit analysis surrogate E=1, G=0.5; preserves rigidities and line mass without assigning a design strength.
                    return (new[]{p.EA,p.EI1,p.EI2,2*p.GJ,0,0,2*p.GA1,2*p.GA2,0,0,0},new[]{1,.5,0,p.MassPerLength/p.EA,0,0,0,0,0});
                }
                Material material;ISectionShape section;double shear1=0,shear2=0;
                if(beam.BeamProperty is ElasticBeamProperty elastic)
                {elastic.Validate();material=elastic.Material;section=elastic.Section;shear1=elastic.ShearArea1;shear2=elastic.ShearArea2;}
                else if(beam.BeamProperty is SteelSection steel)
                {
                    // GPC shear areas from the section geometry (user decision 3/11): Straus7 no longer derives them.
                    material=steel.SteelMaterial;section=steel.SectionShape;
                    if(!shearAreas.TryGetValue(section,out var areas))shearAreas.Add(section,areas=SectionShearAreas.Compute(section));
                    shear1=areas.Area1;shear2=areas.Area2;
                }
                else throw new NotSupportedException("StrausBeamRequiresElasticPropertyOrAnalysisProfile");
                if(material.Orthotropy!=null)throw new NotSupportedException("OrthotropicBeamRequiresExplicitRigidityProfile");
                var centroid=section.Centroid;
                var shearCenter=section.ShearCenter;
                if(shearCenter==null)throw new NotSupportedException("SectionShearCenterRequired");
                double dx=shearCenter.X-centroid.X,dy=shearCenter.Y-centroid.Y,c=Math.Cos(section.AngleX1),s=Math.Sin(section.AngleX1);
                var shape=new[]{section.Area,section.J11,section.J22,section.Jt,c*dx+s*dy,-s*dx+c*dy,shear1,shear2,centroid.X,centroid.Y,section.AngleX1};
                material.AnalysisData?.Validate();var analysis=material.AnalysisData;
                var values=new[]{material.E,analysis?.BeamShearModulus??material.GetShearModule(),material.Ni,material.Density,material.AlfaThermalExpansion,
                    analysis?.ViscousDampingDensity??0,analysis?.ModalDampingRatio??0,(analysis?.ThermalConductivity??0)/1000,(analysis?.SpecificHeat??0)/1000};Finite(shape);Finite(values);
                return(shape,values);
            }
        }
    }
}
