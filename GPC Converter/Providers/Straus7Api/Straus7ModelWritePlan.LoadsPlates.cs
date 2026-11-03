using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Loads.Assignments;

namespace GPC.Converter.Straus7
{
    public sealed partial class Straus7ModelWritePlan
    {
        private sealed partial class Builder
        {
            private void BeamLoads(BeamElement beam, CoordinateSystem axes, Vector3d beamOffset)
            {
                int id=Entities[beam.Guid], assignmentId=0;
                foreach(var load in beam.Assignments.Loads)
                {
                    if(load==null || load.IsEquivalentNodalRepresentation || (load.StationDomain??"NodeToNode")!="NodeToNode")
                        throw new NotSupportedException("UnsupportedBeamLoadStationDomainOrNodalAlias");
                    Finite(new[]{load.Start,load.End});
                    if(load.Start<0||load.Start>1||load.End<load.Start||load.End>1)throw new ArgumentException("InvalidBeamLoadInterval");
                    int nativeId=++assignmentId;
                    // GPC NodeToNode actions act on the node line; native beam actions act on its offset reference line.
                    var eccentricity=Global(load.Eccentricity,load.EccentricityCoordinateSystem??CoordinateSystem.Global)-beamOffset;
                    if(load.Concentrated!=null)
                    {
                        if(load.StartIntensity!=null||load.EndIntensity!=null||load.Start!=load.End)throw new ArgumentException("AmbiguousConcentratedBeamLoad");
                        Vector3d span=beam.NodeJ.Position-beam.NodeI.Position;
                        if(Axes.Length(load.Concentrated.Point-(beam.NodeI.Position+span*load.Start))>1e-7)throw new ArgumentException("InconsistentConcentratedBeamLoadPosition");
                        int lc=Load(load.Concentrated.LoadCase);var v=load.Concentrated.GetGlobalForces();Finite(v);
                        AddMoment(v,eccentricity);
                        var force=v.Take(3).Concat(new[]{load.Start}).ToArray();var moment=v.Skip(3).Concat(new[]{load.Start}).ToArray();
                        if(force.Take(3).Any(x=>x!=0))Operations.Add(a=>a.SetBeamPointLoad(id,lc,nativeId,false,(double[])force.Clone()));
                        if(moment.Take(3).Any(x=>x!=0))Operations.Add(a=>a.SetBeamPointLoad(id,lc,nativeId,true,(double[])moment.Clone()));
                    }
                    else
                    {
                        if(load.StartIntensity==null||load.EndIntensity==null||load.Start==load.End)throw new ArgumentException("MissingDistributedBeamLoad");
                        int lc=Load(load.StartIntensity.LoadCase);
                        if(Load(load.EndIntensity.LoadCase)!=lc)throw new ArgumentException("InconsistentDistributedBeamLoadCases");
                        var start=load.StartIntensity.GetGlobalForces();var end=load.EndIntensity.GetGlobalForces();Finite(start);Finite(end);
                        double factor=1;
                        if(load.LengthConvention==BeamLoadLengthConvention.ProjectedLength)
                        {
                            var normal=load.ProjectionPlaneNormal;
                            if(normal==null||!Axes.IsFinite(normal)||Math.Abs(Axes.Length(normal)-1)>1e-9)throw new ArgumentException("ProjectionPlaneRequiresUnitNormal");
                            double cos=Axes.Dot(normal,axes.V3);factor=Math.Sqrt(Math.Max(0,1-cos*cos));
                        }
                        else if(load.LengthConvention!=BeamLoadLengthConvention.ActualLength)throw new ArgumentException("UnknownBeamLoadLengthConvention");
                        AddMoment(start,eccentricity);AddMoment(end,eccentricity);
                        var ms=new Vector3d(start[3],start[4],start[5]);var me=new Vector3d(end[3],end[4],end[5]);
                        var principal=new[]{axes.V1,axes.V2,axes.V3};
                        for(int j=0;j<6;j++)
                        {
                            int axis=j%3+1;bool moment=j>=3;
                            double v0=(moment?Axes.Dot(ms,principal[j-3]):start[j])*factor;
                            double v1=(moment?Axes.Dot(me,principal[j-3]):end[j])*factor;
                            if(v0==0&&v1==0)continue;
                            var values=new[]{v0,v1,0,0,load.Start,1-load.End};
                            Operations.Add(a=>a.SetBeamDistributedLoad(id,lc,nativeId,axis,moment,(double[])values.Clone()));
                        }
                    }
                }
            }
            private static void AddMoment(double[] forceMoment,Vector3d eccentricity)
            {
                var m=eccentricity.CrossProduct(new Vector3d(forceMoment[0],forceMoment[1],forceMoment[2]));
                forceMoment[3]+=m.X;forceMoment[4]+=m.Y;forceMoment[5]+=m.Z;Finite(forceMoment);
            }
            private void Plate(AreaElement plate)
            {
                int id=Entities[plate.Guid];
                if(plate.Nodes.Count!=3&&plate.Nodes.Count!=4)throw new NotSupportedException("StrausPlateRequiresThreeOrFourConnectedNodes");
                if(plate.Nodes.Any(n=>!Entities.ContainsKey(n.Guid)))throw new ArgumentException("UnregisteredStrausPlateNode");
                if(plate.Attributes.Count!=0)throw new NotSupportedException("UnsupportedStrausPlateAttributes");
                PlateSectionValidation.ValidateProperty(plate.PlateProperty);
                if(!(plate.PlateProperty is IFemPlateProperty p))throw new NotSupportedException("StrausPlateRequiresExplicitFemProperty");
                var material=p.Material;int formulation=4;
                if(plate.PlateProperty is ElasticPlateProperty elastic)
                    switch(elastic.Formulation)
                    {
                        case ElasticPlateFormulation.Shell: formulation=4;break;
                        case ElasticPlateFormulation.Membrane: formulation=6;break;
                        case ElasticPlateFormulation.PlaneStress: formulation=1;break;
                        case ElasticPlateFormulation.PlaneStrain: formulation=2;break;
                        case ElasticPlateFormulation.Axisymmetric: formulation=3;break;
                        case ElasticPlateFormulation.LoadPatch: formulation=7;break;
                    }
                if(formulation==7&&plate.Assignments.Physical?.LoadPatch==null)throw new ArgumentException("MissingLoadPatchDistribution");
                var o=material?.Orthotropy;double rho=material?.Density??0,alpha=material?.AlfaThermalExpansion??0;
                var values=formulation==7?new double[8]:o==null?new[]{material.E,material.Ni,rho,alpha,0,0,0,0}:
                    new[]{o.E1,o.E2,o.E3,o.G12,o.G23,o.G31,o.Nu12,o.Nu23,o.Nu31,rho,alpha,alpha,alpha,0,0,0,0,0};
                Finite(values);var nodes=plate.Nodes.Select(n=>Entities[n.Guid]).ToArray();string name=string.IsNullOrWhiteSpace(plate.Name)?"Plate "+id:plate.Name;
                double bending=p.BendingThickness,membrane=p.MembraneThickness;bool orthotropic=o!=null;
                if(orthotropic&&plate.Assignments.LayerAxes==null)throw new ArgumentException("OrthotropicPlateRequiresMaterialAxes");
                Operations.Add(a=>a.CreatePlateProperty(id,name,formulation,bending,membrane,(double[])values.Clone(),orthotropic));
                Operations.Add(a=>a.SetElement(2,id,id,(int[])nodes.Clone()));Group(plate,2);
                if(orthotropic){var axes=AxesData(plate.Assignments.LayerAxes);Operations.Add(a=>a.SetPlateAxes(id,(double[])axes.Clone()));}
                if(plate.Assignments.Offset.HasValue)
                {double offset=plate.Assignments.Offset.Value;Finite(new[]{offset});Operations.Add(a=>a.SetPlateOffset(id,offset));}
                var totals=new Dictionary<int,Vector3d>();
                foreach(var load in plate.Loads.Values)
                {
                    Vector3d pressure;
                    if(load is AreaLoad area){FullPlateCoverage(area.Shape,plate);pressure=Global(new Vector3d(area.P1,area.P2,area.P3),area.CoordinateSystem);}
                    else if(load is NormalAreaLoad normal){FullPlateCoverage(normal.Shape,plate);pressure=normal.Shape.GetCoordinateSystem().V3*normal.Pressure;}
                    else throw new NotSupportedException("UnsupportedStrausPlateLoad: "+load.GetType().Name);
                    int lc=Load(load.LoadCase);Values(pressure);
                    totals[lc]=totals.TryGetValue(lc,out var previous)?previous+pressure:pressure;
                }
                foreach(var pair in totals)
                {int lc=pair.Key;var v=Values(pair.Value);Operations.Add(a=>a.SetPlatePressure(id,lc,2,(double[])v.Clone()));}
            }
            private static void FullPlateCoverage(Shape shape,AreaElement plate)
            {
                if(shape?.Fill==null||(shape.Holes?.Length??0)!=0||(shape.Childs?.Length??0)!=0)
                    throw new NotSupportedException("PlatePressureRequiresFullElementCoverage");
                var boundary=shape.Fill.ToArray();var nodes=plate.Nodes.Select(n=>n.Position).ToArray();int count=nodes.Length;
                if(boundary.Length==count+1&&Axes.Length(boundary[0]-boundary[count])<1e-7)boundary=boundary.Take(count).ToArray();
                bool matches=boundary.Length==count&&Enumerable.Range(0,count).Any(start=>new[]{1,-1}.Any(direction=>
                    Enumerable.Range(0,count).All(i=>Axes.Length(boundary[(start+direction*i+count)%count]-nodes[i])<1e-7)));
                if(!matches)throw new NotSupportedException("PlatePressureRequiresFullElementCoverage");
            }
            private void ModelActions()
            {
                var gravity=new Dictionary<int,Vector3d>();
                foreach(var load in Model.ModelLoads.Values)
                {
                    if(!(load is ModelGravityLoad g))throw new NotSupportedException("UnsupportedStrausModelLoad: "+load.GetType().Name);
                    int lc=Load(g.LoadCase);var v=Global(g.GravityVector,g.CoordinateSystem);Values(v);
                    gravity[lc]=gravity.TryGetValue(lc,out var previous)?previous+v:v;
                }
                foreach(var pair in gravity)
                {
                    int lc=pair.Key;var v=Values(pair.Value);if(v.Count(x=>x!=0)>1)throw new NotSupportedException("ObliqueGravityRequiresExplicitMassAccelerationConversion");
                    if(v.Any(x=>x!=0))Operations.Add(a=>a.SetGravity(lc,(double[])v.Clone()));
                }
                if(Model.Combinations.Count!=0&&Freedoms.Count>1)throw new NotSupportedException("CombinationRequiresExplicitFreedomCasePairing");
                int i=0;
                foreach(var combination in Model.Combinations.Values.OrderBy(c=>c.Name,StringComparer.Ordinal))
                {
                    int id=++i;string name=combination.Name;var pairs=combination.GetLoadCaseCoefficientsPair();
                    var cases=pairs.Select(c=>Load(c.Key)).ToArray();var factors=pairs.Select(c=>c.Value).ToArray();Finite(factors);
                    Operations.Add(a=>a.AddCombination(id,name,(int[])cases.Clone(),(double[])factors.Clone(),1));
                }
            }
        }
    }
}
