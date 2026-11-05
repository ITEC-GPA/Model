using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Attributes;
using GPC.Model.Core;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
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
            /// <summary>The attributes that enter the native model: an element label (<see cref="ElementLabelAttribute"/>, an
            /// identifier given by the user to select the element, e.g. the Element Id of BridGHe and Arachne) is descriptive and is not
            /// written (the native numbering is the GPC one), as the former BridGHe exporter. The bridge attributes (member, super element,
            /// section state, rigid connection: lane L11) are descriptive too: the application lifts them into the physical members, the
            /// verification recipe and the rigid links of the model, which are written. The domain preflight refuses them on the wrong
            /// kind of element.</summary>
            private static int Exported(Element element)=>element.Attributes.Values.Count(a=>!Descriptive(a));
            private static bool Descriptive(GPC.Model.Attributes.Attribute a)=>a is ElementLabelAttribute
                || a is GPC.Model.Structure.Bridges.BridgeMemberAttribute || a is GPC.Model.Structure.Bridges.SuperElementAttribute
                || a is GPC.Model.Structure.Bridges.BridgeSectionStateAttribute || a is GPC.Model.Structure.Bridges.RigidConnectionAttribute;

            private void Node(NodeElement node)
            {
                int id=Entities[node.Guid];var p=node.Assignments;
                if(p.Active==false || p.Dofs!=null || Exported(node)!=0 || p.Links.Count!=0 || p.Mass!=null)
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
                // 5/11/2026: the work-equivalent nodal forces of the varying plate pressures (PlatePressureNodeForces), global as the point loads.
                if(pressureNodeForces.TryGetValue(node.Guid,out var pressures))
                    foreach(var pair in pressures)
                    {
                        if(!totals.TryGetValue(pair.Key,out var total))totals.Add(pair.Key,total=new double[6]);
                        total[0]+=pair.Value.X;total[1]+=pair.Value.Y;total[2]+=pair.Value.Z;
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
                var axes=input.SectionAxes;var tangent=beam.NodeJ.Position-beam.NodeI.Position;double length=Axes.Length(tangent);
                if(!(length>1e-9)||Math.Abs(Axes.Dot(tangent/length,axes.V3)-1)>1e-8)throw new ArgumentException("InconsistentStrausBeamAxes");
                var nodes=new[]{Entities[beam.NodeI.Guid],Entities[beam.NodeJ.Guid]};
                var property=BeamPropertyData(beam);
                // Native beam axes are principal axes: the section frame rotated by the principal angle about the beam axis (C5).
                var principal=PrincipalAxes(axes,property.Section[10]);var axesData=AxesData(principal);bool rotated=!ReferenceEquals(principal,axes);
                var taper=input.Physical?.Taper;bool uniformTaper=taper==null||taper.ScaleI1==taper.ScaleI2&&taper.ScaleJ1==taper.ScaleJ2&&taper.Alignment1==TaperAlignment.Symmetric&&taper.Alignment2==TaperAlignment.Symmetric;
                // Uniform symmetric tapers do not depend on the transverse axes; other axis-wise data would be applied in the wrong frame.
                if(rotated&&input.Physical!=null&&(!uniformTaper||input.Physical.TemperatureGradients.Count!=0||input.Physical.Supports.Count!=0))
                    throw new NotSupportedException("PrincipalAngleWithTaperGradientOrSupportRequiresExplicitAxisConvention");
                var cutoff=(beam.BeamProperty as ElasticBeamProperty)?.CutoffLaw;
                if((cutoff!=null)!=(formulation==4)||cutoff!=null&&input.AnalysisProfile!=null)throw new ArgumentException("CutoffLawRequiresCutoffBarFormulationAndExplicitSection");
                cutoff?.Validate();
                if(NativeSections.TryGetValue(beam.Guid,out var nativeSection))
                {
                    if(input.AnalysisProfile!=null)throw new NotSupportedException("NativeSectionRequiresPhysicalBeamProperty");
                    var reference=nativeSection.ReferenceProperties;
                    for(int k=0;k<11;k++)if(Math.Abs(reference[k]-property.Section[k])>1e-8*Math.Max(1,Math.Abs(reference[k])))
                        throw new ArgumentException("NativeSectionPhysicalPropertiesMismatch: slot "+k);
                }
                else if(input.AnalysisProfile!=null&&input.Physical?.Taper!=null)throw new NotSupportedException("TaperRequiresDimensionalSectionGeometry");
                bool sliced=beamIntegrationSlices.TryGetValue(beam.Guid,out int slices);
                var beamMaterial=(beam.BeamProperty as ElasticBeamProperty)?.Material??(beam.BeamProperty as SteelSection)?.SteelMaterial;
                if(cutoff!=null&&beamMaterial?.AnalysisData?.StressStrainPairs!=null)throw new NotSupportedException("CutoffLawCannotBeCombinedWithMaterialStressStrainLaw");
                // Equivalent beams share one native property, BXS and material law, named after the GPC property and material (C5).
                string key=PropertyKey(beam,formulation,property,nativeSection,sliced?slices:0);
                if(!beamProperties.TryGetValue(key,out int pid))
                {
                    pid=beamProperties.Count+1;beamProperties.Add(key,pid);
                    string name=string.IsNullOrWhiteSpace(beam.BeamProperty?.Name)?"Beam property "+pid:beam.BeamProperty.Name;
                    Operations.Add(a=>a.CreateBeamProperty(pid,name,formulation,(double[])property.Section.Clone(),(double[])property.Material.Clone()));
                    string materialName=input.AnalysisProfile==null?beamMaterial?.Name:null;
                    if(!string.IsNullOrWhiteSpace(materialName))Operations.Add(a=>a.SetBeamMaterialName(pid,materialName));
                    if(cutoff!=null)
                    {
                        RequiresMaterialApi=true;int failure=(int)cutoff.Failure;bool keep=cutoff.KeepMass;double tension=cutoff.TensionLimit,compression=cutoff.CompressionLimit;
                        Operations.Add(a=>((IStraus7MaterialWriteApi)a).SetCutoffLaw(pid,failure,keep,tension,compression));
                    }
                    if(nativeSection!=null)Operations.Add(a=>((IStraus7NativeSectionWriteApi)a).SetNativeSection(pid,nativeSection,(double[])property.Section.Clone()));
                    else if(input.AnalysisProfile==null)BeamSectionGeometry(beam,pid,property.Section);
                    if(sliced)
                    {
                        RequiresSectionGeometryApi=true;
                        Operations.Add(a=>((IStraus7SectionGeometryWriteApi)a).SetBeamIntegrationSlices(pid,slices,(double[])property.Section.Clone()));
                    }
                    if(input.AnalysisProfile==null)MaterialLaw(pid,1,beamMaterial);
                }
                Operations.Add(a=>a.SetElement(1,id,pid,(int[])nodes.Clone()));Group(beam,1);Operations.Add(a=>a.SetBeamAxes(id,(double[])axesData.Clone()));
                var offsetI=Global(input.OffsetI,input.OffsetAxes??axes);var offsetJ=Global(input.OffsetJ,input.OffsetAxes??axes);
                if(Axes.Length(offsetI-offsetJ)>1e-8)throw new NotSupportedException("UnequalBeamOffsetsRequireRigidEndLinks");
                double axial=Axes.Dot(offsetI,axes.V3);if(Math.Abs(axial)>1e-8)throw new NotSupportedException("AxialBeamOffsetRequiresRigidEndLinks");
                var offsets=new[]{Axes.Dot(offsetI,principal.V1),Axes.Dot(offsetI,principal.V2)};
                if(input.SectionCentroidOffset!=null && Math.Abs(input.SectionCentroidOffset.X)+Math.Abs(input.SectionCentroidOffset.Y)>1e-8)
                    throw new NotSupportedException("SectionCentroidOffsetRequiresExplicitReferenceConvention");
                if(offsets.Any(v=>v!=0))Operations.Add(a=>a.SetBeamOffset(id,(double[])offsets.Clone()));
                if(beam.Attributes.Values.Count(a=>a is BeamReleasesAttribute)>1)throw new ArgumentException("DuplicateBeamReleases");
                foreach(var attribute in beam.Attributes.Values.Where(a=>!Descriptive(a)))
                {
                    if(!(attribute is BeamReleasesAttribute release)||release.Phase!=null||release.CoordinateSystem!=null&&!Same(release.CoordinateSystem,axes)&&!Same(release.CoordinateSystem,principal))
                        throw new NotSupportedException("UnsupportedStrausBeamAttributeOrReleaseAxes");
                    for(int end=1;end<=2;end++)for(int r=0;r<2;r++)
                    {
                        int beamEnd=end;bool rotation=r==1;var connections=(end==1?release.I:release.J).Skip(r*3).Take(3).ToArray();
                        if(connections.Any(c=>c==null||c.Kind==BeamConnectionKind.RelativeFixity))throw new NotSupportedException("RelativeBeamFixityRequiresStiffnessConversion");
                        // Native releases act in the principal axes: a section-frame release must be the same about both transverse axes.
                        if(rotated&&!(release.CoordinateSystem!=null&&Same(release.CoordinateSystem,principal))&&(connections[0].Kind!=connections[1].Kind||connections[0].Value!=connections[1].Value))
                            throw new NotSupportedException("SectionFrameBeamReleaseDiffersInPrincipalAxes");
                        // R3 constants: brReleased=0, brFixed=1, brPartial=2.
                        var status=connections.Select(c=>c.Kind==BeamConnectionKind.Continuous?1:c.Kind==BeamConnectionKind.Released?0:2).ToArray();
                        var values=connections.Select(c=>c.Value).ToArray();Operations.Add(a=>a.SetBeamRelease(id,beamEnd,rotation,(int[])status.Clone(),(double[])values.Clone()));
                    }
                }
                BeamLoads(beam,principal,offsetI);
            }

            /// <summary>Restraint and spring components of one node in one native freedom case, in one frame.</summary>
            private sealed class NodeFreedomAttributes
            {
                internal readonly int FreedomCase;internal readonly CoordinateSystem Frame;internal readonly bool RestraintCall;
                internal readonly int[] Status=new int[6];internal readonly double[] Prescribed=new double[6],Stiffness=new double[6];
                internal NodeFreedomAttributes(int freedomCase,CoordinateSystem frame,bool restraintCall=true){FreedomCase=freedomCase;Frame=frame;RestraintCall=restraintCall;}
            }

            /// <summary>Principal axes: the section frame rotated by the principal angle (rad) about V3; the same instance when the angle is zero.</summary>
            private static CoordinateSystem PrincipalAxes(CoordinateSystem axes,double angle)
            {
                if(Math.Abs(angle)<=1e-12)return axes;double c=Math.Cos(angle),s=Math.Sin(angle);
                return new CoordinateSystem(axes.Origin,axes.V1*c+axes.V2*s,axes.V2*c-axes.V1*s);
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
                // bsISection: B1/T1 bottom flange, B2/T2 top flange, T3 web (native readback in C3 tests; same order as the importer).
                {shape=7;dimensions=new[]{h.LenghtBottomFlange,h.LenghtTopFlange,h.Height,h.ThicknessBottomFlange,h.ThicknessTopFlange,h.ThicknessWeb};}
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
                    // The parts of a welded section are one body: one conforming mesh, so that R31 computes its BXS properties and recovers the
                    // stresses on the welded whole (review L6; before, parts on separate nodes: SA1 of a cruciform 13 % low). The parts of another
                    // built-up section, connected at discrete points, stay on their own nodes.
                    if(section is SectionWelded welded&&outlines.Count>1)
                    {
                        var conforming=welded.GetConformingMesh();
                        foreach(var vertex in conforming.Vertices){var xyz=(double[])vertex.Clone();Finite(xyz);vertices.Add(xyz);}
                        faces.AddRange(conforming.Triangles);outlines=Array.Empty<Shape2d>();
                    }
                    var partMeshes=new List<(List<double[]> Vertices,List<int[]> Faces)>();
                    foreach(var outline in outlines)
                    {
                        var mesh=new Section(outline).GetMesh(initialMeshOnly:true);
                        if(mesh==null||mesh.Faces.Count==0)throw new ArgumentException("EmptyStrausSectionMesh");
                        var nodeIds=new Dictionary<int,int>();var partVertices=new List<double[]>();var partFaces=new List<int[]>();
                        foreach(var vertex in mesh.Vertices)
                        {
                            nodeIds.Add(vertex.Id,partVertices.Count);
                            var xyz=new[]{vertex.Point.X,vertex.Point.Y,vertex.Point.Z};Finite(xyz);partVertices.Add(xyz);
                        }
                        foreach(var face in mesh.Faces)partFaces.Add(mesh.GetFaceVertices(face).Select(v=>nodeIds[v.Id]).ToArray());
                        partMeshes.Add((partVertices,partFaces));
                    }
                    // The parts of a built-up section that is not welded stay separate bodies for R31, as for GPC (user decision 5 of 4/11,
                    // «separa i nodi»): R31 merges the nodes it finds closer than its tolerance, so touching vertices of different parts move into
                    // their own part by a negligible distance (Straus7SectionPartSeparation). Parts that do not touch keep their mesh value for value.
                    if(section is SectionBuiltUp&&partMeshes.Count>1)Straus7SectionPartSeparation.Separate(partMeshes,section.Name);
                    foreach(var part in partMeshes)
                    {
                        int offset=vertices.Count;vertices.AddRange(part.Vertices);
                        foreach(var face in part.Faces)faces.Add(face.Select(i=>i+offset).ToArray());
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

            // Caches of this export only (no static cache): fingerprints per object instance, shear areas per section value (ν = 0 for every
            // material: decisione utente 4/11, De Saint Venant a ν = 0), native beam property per equivalent property data.
            private readonly Dictionary<object,string> fingerprints=new Dictionary<object,string>(ReferenceComparer.Instance);
            private readonly Dictionary<string,ShearAreaResult> shearAreas=new Dictionary<string,ShearAreaResult>(StringComparer.Ordinal);
            private readonly Dictionary<string,int> beamProperties=new Dictionary<string,int>(StringComparer.Ordinal);
            /// <summary>Warnings of the preparation (the plan's <see cref="Straus7ModelWritePlan.Diagnostics"/>).</summary>
            internal readonly List<ModelDiagnostic> Diagnostics=new List<ModelDiagnostic>();

            /// <summary>The FE shear areas of one section value, or the failure the serial computation would have thrown at that beam.</summary>
            private sealed class ShearAreaResult
            {
                internal double Area1,Area2;internal int Bodies;internal System.Runtime.ExceptionServices.ExceptionDispatchInfo Failure;internal bool Reported;
            }
            private string ShearKey(ISectionShape section)=>FingerprintOf(section);

            /// <summary>The shear areas of the distinct steel sections of this export, computed in parallel before the beams are written (0.05–1.5 s
            /// each; BridGHe girders have many segment sections). Same values as one by one: each section is solved on its own. A failure is kept and
            /// thrown at its beam, in beam order, as the serial preparation did.</summary>
            private void PrepareShearAreas()
            {
                var pending=new List<KeyValuePair<string,ISectionShape>>();
                foreach(var beam in Model.BeamElements.Values.OrderBy(b=>Entities[b.Guid]))
                {
                    if(beam.Assignments.AnalysisProfile!=null||!(beam.BeamProperty is SteelSection steel)||steel.SteelMaterial==null||steel.SectionShape==null)continue;
                    string key=ShearKey(steel.SectionShape);
                    if(shearAreas.ContainsKey(key))continue;
                    shearAreas.Add(key,new ShearAreaResult());pending.Add(new KeyValuePair<string,ISectionShape>(key,steel.SectionShape));
                }
                System.Threading.Tasks.Parallel.ForEach(pending,item=>
                {
                    var result=shearAreas[item.Key];
                    try{var areas=global::GPC.Model.Sections.SectionShearAreas.Compute(item.Value);result.Area1=areas.Area1;result.Area2=areas.Area2;result.Bodies=areas.Bodies;}
                    catch(Exception error){result.Failure=System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error);}
                });
            }
            private sealed class ReferenceComparer : IEqualityComparer<object>
            {
                internal static readonly ReferenceComparer Instance=new ReferenceComparer();
                public new bool Equals(object x,object y)=>ReferenceEquals(x,y);
                public int GetHashCode(object value)=>System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
            }
            private string FingerprintOf(object value)
            {
                if(value==null)return "";
                if(!fingerprints.TryGetValue(value,out var fingerprint))fingerprints.Add(value,fingerprint=ModelValues.Fingerprint(new object[]{value}));
                return fingerprint;
            }
            private static string Doubles(double[] values)=>string.Join(",",values.Select(v=>v.ToString("R",System.Globalization.CultureInfo.InvariantCulture)));
            /// <summary>Beams with equal native property data share one native property and its BXS (legacy: one property per BridGHe
            /// property). The key holds every input of the property-level operations. Tapered and untapered beams of one property get two
            /// native properties, so that the dimensional-geometry check of the property (TaperRequiresDimensionalSectionGeometry) runs for
            /// the tapered ones; the taper itself is written per element (St7SetBeamTaper2, Straus7PhysicalWritePlan), so tapered beams with
            /// different tapers share their property.</summary>
            private string PropertyKey(BeamElement beam,int formulation,(double[] Section,double[] Material) property,Straus7SectionDefinition native,int slices)
            {
                var input=beam.Assignments;
                return string.Join("|",new[]{formulation.ToString(System.Globalization.CultureInfo.InvariantCulture),Doubles(property.Section),Doubles(property.Material),
                    beam.BeamProperty?.Name??"",FingerprintOf(beam.BeamProperty),FingerprintOf(input.AnalysisProfile),native?.Fingerprint()??"",
                    slices.ToString(System.Globalization.CultureInfo.InvariantCulture),input.Physical?.Taper!=null?"taper":""});
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
                    // De Saint Venant at ν = 0 (decisione utente 4/11): the areas depend on the section geometry only, not on the material.
                    string key=ShearKey(section);
                    if(!shearAreas.TryGetValue(key,out var areas))
                    {
                        areas=new ShearAreaResult();var computed=global::GPC.Model.Sections.SectionShearAreas.Compute(section);
                        areas.Area1=computed.Area1;areas.Area2=computed.Area2;areas.Bodies=computed.Bodies;shearAreas.Add(key,areas);
                    }
                    areas.Failure?.Throw();
                    shear1=areas.Area1;shear2=areas.Area2;
                    if(areas.Bodies>1&&!areas.Reported)
                    {
                        areas.Reported=true;
                        Diagnostics.Add(new ModelDiagnostic{Code="SeparateSectionPartsShearAreas",Severity=GPC.Model.Core.Diagnostics.DiagnosticSeverity.Warning,ElementId=beam.Id,
                            Family=GPC.Model.Core.Identity.EntityFamily.Beam,Record=steel.Name,
                            Message=string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                "{0}: {1} separate parts (connected at discrete points or not touching); SA1 = {2:G6}, SA2 = {3:G6} mm² with the parts bending together "+
                                "about their own centroids and no shear flow between them.",section.Name,areas.Bodies,areas.Area1,areas.Area2),
                            SuggestedAction="Check that the parts act together in shear (battens, gusset plates); otherwise model them as separate members."});
                    }
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
