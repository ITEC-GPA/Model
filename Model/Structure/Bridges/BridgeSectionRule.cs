using System;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Structure.Members;

namespace GPC.Model.Structure.Bridges
{
    /// <summary>Section assignment before FEM subdivision. Stations are alignment arc lengths in mm;
    /// scale factors act on section dimensions, not on area or inertia. Group/Segment zero selects all.</summary>
    [Serializable,DataContract]
    public sealed class BridgeSectionRule
    {
        [field:DataMember(Order=0)] public BridgeMemberFamily Family {get;set;}
        [field:DataMember(Order=1)] public int Group {get;set;}
        [field:DataMember(Order=2)] public int Segment {get;set;}
        [field:DataMember(Order=3)] public double Start {get;set;}
        [field:DataMember(Order=4)] public double End {get;set;}
        [field:DataMember(Order=5)] public BeamProperty Property {get;set;}
        [field:DataMember(Order=6)] public Vector2d ScaleFrom {get;set;}=new Vector2d(1,1);
        [field:DataMember(Order=7)] public Vector2d ScaleTo {get;set;}=new Vector2d(1,1);
        public void Validate()
        {
            BridgeDimensions.Finite(Start);BridgeDimensions.Finite(End);
            if(!Enum.IsDefined(typeof(BridgeMemberFamily),Family)||Group<0||Segment<0||Start<0||End<=Start||Property==null||ScaleFrom==null||ScaleTo==null)
                throw new ArgumentException("InvalidBridgeSectionRule");
            foreach(double v in new[]{ScaleFrom.X,ScaleFrom.Y,ScaleTo.X,ScaleTo.Y})BridgeDimensions.Positive(v);
            if(Family!=BridgeMemberFamily.Girder&&(ScaleFrom.X!=ScaleTo.X||ScaleFrom.Y!=ScaleTo.Y))throw new ArgumentException("VariableTaperRequiresLongitudinalGirder");
            (Property as ElasticBeamProperty)?.Validate();
        }
        public bool Selects(BridgeMemberMetadata member)
        {
            Validate();if(member==null)throw new ArgumentNullException(nameof(member));
            return member.Family==Family&&(Group==0||member.Group==Group)&&(Segment==0||member.Segment==Segment);
        }
        public Vector2d Scale(double station)
        {
            Validate();BridgeDimensions.Finite(station);double t=Math.Max(0,Math.Min(1,(station-Start)/(End-Start)));
            return new Vector2d(ScaleFrom.X+t*(ScaleTo.X-ScaleFrom.X),ScaleFrom.Y+t*(ScaleTo.Y-ScaleFrom.Y));
        }
        [OnDeserialized] private void OnRead(StreamingContext context)=>Validate();
    }

    /// <summary>Default property for a bridge member family; null Family is the explicit all-family fallback.</summary>
    [Serializable,DataContract]
    public sealed class BridgeFamilyProperty
    {
        [field:DataMember(Order=0)] public BridgeMemberFamily? Family {get;set;}
        [field:DataMember(Order=1)] public BeamProperty Property {get;set;}
        public void Validate()
        {
            if(Family.HasValue&&!Enum.IsDefined(typeof(BridgeMemberFamily),Family.Value)||Property==null)throw new ArgumentException("InvalidBridgeFamilyProperty");
            (Property as ElasticBeamProperty)?.Validate();
        }
        [OnDeserialized] private void OnRead(StreamingContext context)=>Validate();
    }
}
