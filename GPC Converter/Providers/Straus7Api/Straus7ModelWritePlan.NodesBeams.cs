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
                if(p.Restrains.Any(r=>r==null||r.Restrain==null||r.Case!=null))throw new ArgumentException("RestraintRequiresExplicitFreedomCaseMigration");
                if(p.Restrains.GroupBy(r=>r.FreedomCase?.Guid).Any(g=>g.Select(r=>ModelValues.Fingerprint(new object[]{r.Restrain.CoordinateSystem??CoordinateSystem.Global})).Distinct().Count()>1))
                    throw new NotSupportedException("StrausNodeRequiresOneRestraintFramePerFreedomCase");
                foreach(var group in p.Restrains.GroupBy(r=>new {Case=r.FreedomCase?.Guid,Axes=ModelValues.Fingerprint(new object[]{r.Restrain.CoordinateSystem??CoordinateSystem.Global})}))
                {
                    var first=group.First();
                    int fc=Freedom(first.FreedomCase),ucs=Ucs(first.Restrain.CoordinateSystem);
                    var status=new int[6];var prescribed=new double[6];var stiffness=new double[6];
                    foreach(var restraint in group)
                    {
                        if(restraint.Phase!=null)throw new NotSupportedException("PhaseDependentRestraintRequiresStageBoundaryConversion");
                        foreach(var dof in restraint.Restrain.Restrains)
                        {
                            int k=(int)dof.Dof;if(k<0||k>5||!scopes.Add(fc+":"+k))throw new ArgumentException("OverlappingOrInvalidStrausRestraint");
                            status[k]=dof.IsRestrained||dof.HasImposedDisplacement?1:0;prescribed[k]=dof.ImposedDisplacement;stiffness[k]=dof.Stiffness;
                            if(dof.HasStiffness&&dof.HasImposedDisplacement)throw new NotSupportedException("CoupledPrescribedSpringRequiresExplicitLink");
                        }
                    }
                    Finite(prescribed);Finite(stiffness);Operations.Add(a=>a.SetNodeRestraint(id,fc,ucs,(int[])status.Clone(),(double[])prescribed.Clone()));
                    for(int r=0;r<2;r++)
                    {bool rotation=r==1;var values=stiffness.Skip(r*3).Take(3).ToArray();if(values.Any(v=>v!=0))Operations.Add(a=>a.SetNodeSpring(id,fc,ucs,rotation,(double[])values.Clone()));}
                }
                if(p.GroundSprings.Count>1)throw new NotSupportedException("MultipleGroundSpringsRequireExplicitAssembly");
                foreach(var spring in p.GroundSprings)
                {
                    if(Freedoms.Count>1)throw new NotSupportedException("UnscopedSpringWithMultipleFreedomCases");
                    int ucs=Ucs(spring.CoordinateSystem);
                    for(int r=0;r<6;r++)for(int c=0;c<6;c++)if(r!=c&&spring[r,c]!=0)throw new NotSupportedException("CoupledGroundSpringRequiresMatrixLinkWriter");
                    for(int r=0;r<2;r++)
                    {
                        bool rotation=r==1;var values=Enumerable.Range(r*3,3).Select(i=>spring[i,i]).ToArray();Finite(values);
                        if(Enumerable.Range(r*3,3).Any(i=>scopes.Contains("1:"+i)))throw new ArgumentException("OverlappingStrausSpringAndRestraint");
                        if(values.Any(v=>v!=0))Operations.Add(a=>a.SetNodeSpring(id,1,ucs,rotation,(double[])values.Clone()));
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
                if(input.Physical?.Taper!=null) BeamSectionGeometry(beam,id,property.Section);
                if(beamIntegrationSlices.TryGetValue(beam.Guid,out int slices))
                {
                    RequiresSectionGeometryApi=true;
                    Operations.Add(a=>((IStraus7SectionGeometryWriteApi)a).SetBeamIntegrationSlices(id,slices,(double[])property.Section.Clone()));
                }
                if(input.AnalysisProfile==null)MaterialLaw(id,1,(beam.BeamProperty as ElasticBeamProperty)?.Material??(beam.BeamProperty as SteelSection)?.SteelMaterial);
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
                        var status=connections.Select(c=>c.Kind==BeamConnectionKind.Continuous?0:c.Kind==BeamConnectionKind.Released?1:2).ToArray();
                        var values=connections.Select(c=>c.Value).ToArray();Operations.Add(a=>a.SetBeamRelease(id,beamEnd,rotation,(int[])status.Clone(),(double[])values.Clone()));
                    }
                }
                BeamLoads(beam,axes,offsetI);
            }

            private static Vector3d Global(Vector3d v,CoordinateSystem axes)
            {if(v==null)return new Vector3d(0,0,0);Axes.Validate(axes);return axes.V1*v.X+axes.V2*v.Y+axes.V3*v.Z;}

            internal bool RequiresSectionGeometryApi;
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
                else throw new NotSupportedException("TaperSectionGeometryNotRepresentable: "+section?.GetType().Name);
                RequiresSectionGeometryApi=true;
                Operations.Add(a=>((IStraus7SectionGeometryWriteApi)a).SetBeamSectionGeometry(id,shape,(double[])dimensions.Clone(),(double[])properties.Clone()));
            }

            private static (double[] Section,double[] Material) BeamPropertyData(BeamElement beam)
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
                else if(beam.BeamProperty is SteelSection steel) {material=steel.SteelMaterial;section=steel.SectionShape;}
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
