using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;
using GPC.Model.Elements;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.Straus7
{
    public interface IStraus7SurfacePressureWriteApi
    {
        void SetGlobalSurfacePressure(int plate,int loadCase,int face,int projection,double[] pressure);
        void SetNormalSurfacePressure(int plate,int loadCase,double minusZ,double plusZ);
    }
    public sealed partial class Straus7NativeWriter : IStraus7SurfacePressureWriteApi
    {
        public void SetGlobalSurfacePressure(int plate,int loadCase,int face,int projection,double[] pressure) =>
            api.CheckWrite(api.WriteFunction<FiveDoubles>("St7SetPlateGlobalPressure3S")(Id,plate,face,projection,loadCase,pressure),"St7SetPlateGlobalPressure3S");
        public void SetNormalSurfacePressure(int plate,int loadCase,double minusZ,double plusZ) =>
            Three("St7SetPlateNormalPressure2",plate,loadCase,new[]{minusZ,plusZ});
    }
    public sealed partial class Straus7ModelWritePlan
    {
        private sealed partial class Builder
        {
            internal bool RequiresSurfacePressureApi;
            private void SurfacePressures(AreaElement plate,int id,Dictionary<int,Vector3d> existing)
            {
                var global=new Dictionary<(int Case,int Face),(SurfacePressureProjection Projection,Vector3d Vector)>();
                foreach(var p in existing)global.Add((p.Key,2),(SurfacePressureProjection.None,p.Value));
                var normal=new Dictionary<int,double[]>();
                foreach(var p in plate.Loads.Values.OfType<GPC.Model.Loads.ShellFacePressure>())
                {
                    p.Validate();int lc=Load(p.LoadCase),face=p.Face==ShellFace.PositiveNormal?2:1;
                    if(p.NormalPressure.HasValue)
                    {
                        if(!normal.TryGetValue(lc,out var v))normal.Add(lc,v=new double[2]);v[face-1]+=p.NormalPressure.Value;
                    }
                    else if(global.TryGetValue((lc,face),out var previous))
                    {
                        if(previous.Projection!=p.Projection)throw new NotSupportedException("ConflictingSurfacePressureProjection");
                        if(p.Projection==SurfacePressureProjection.Resultant&&Axes.Length(previous.Vector.CrossProduct(p.GlobalPressure))>1e-10*Axes.Length(previous.Vector)*Axes.Length(p.GlobalPressure))
                            throw new NotSupportedException("NonCollinearProjectedResultantsRequireSeparateCases");
                        global[(lc,face)]=(p.Projection,previous.Vector+p.GlobalPressure);
                    }
                    else global.Add((lc,face),(p.Projection,new Vector3d(p.GlobalPressure.X,p.GlobalPressure.Y,p.GlobalPressure.Z)));
                }
                foreach(var p in global)
                {
                    int lc=p.Key.Case,face=p.Key.Face,projection=(int)p.Value.Projection;var v=Values(p.Value.Vector);
                    if(projection==0&&face==2)Operations.Add(a=>a.SetPlatePressure(id,lc,face,(double[])v.Clone()));
                    else {RequiresSurfacePressureApi=true;Operations.Add(a=>((IStraus7SurfacePressureWriteApi)a).SetGlobalSurfacePressure(id,lc,face,projection,(double[])v.Clone()));}
                }
                foreach(var p in normal)
                {
                    RequiresSurfacePressureApi=true;int lc=p.Key;double minus=p.Value[0],plus=p.Value[1];Finite(p.Value);
                    Operations.Add(a=>((IStraus7SurfacePressureWriteApi)a).SetNormalSurfacePressure(id,lc,minus,plus));
                }
            }
        }
    }
}
