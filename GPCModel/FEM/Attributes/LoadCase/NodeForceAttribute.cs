using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.FEM.Attributes
{
    [Serializable]
    public sealed class NodeForceAttribute : LoadCaseAttribute, INodeLoadCaseAttribute, IEquatable<NodeForceAttribute>, ISerializable
    {
        #region variables
        private double _f1;
        private double _f2;
        private double _f3;
        private double _m1;
        private double _m2;
        private double _m3;
        private CoordinateSystem _coordinateSystem;
        #endregion

        #region Properties
        public double F1 => _f1;
        public double F2 => _f2;
        public double F3 => _f3;
        public double M1 => _m1;
        public double M2 => _m2;
        public double M3 => _m3;
        public CoordinateSystem CoordinateSystem => _coordinateSystem;
        #endregion

        public NodeForceAttribute(string loadCaseName, CoordinateSystem cSys, double f1, double f2, double f3, double m1, double m2, double m3)
            : this (loadCaseName, cSys, f1, f2, f3, m1, m2, m3, string.Empty)
        {

        }

        public NodeForceAttribute(string loadCaseName, CoordinateSystem cSys, double f1, double f2, double f3, double m1, double m2, double m3, string name) 
            : base(loadCaseName, name)
        {
            _f1 = f1;
            _f2 = f2;
            _f3 = f3;
            _m1 = m1;
            _m2 = m2;
            _m3 = m3;
            _coordinateSystem = cSys;
        }

        public NodeForceAttribute(NodeForceAttribute nodeForceAttribute)
            : base(nodeForceAttribute)
        {
            _f1 = nodeForceAttribute._f1;
            _f2 = nodeForceAttribute._f2;
            _f3 = nodeForceAttribute._f3;
            _m1 = nodeForceAttribute._m1;
            _m2 = nodeForceAttribute._m2;
            _m3 = nodeForceAttribute._m3;
            _coordinateSystem = nodeForceAttribute._coordinateSystem;
        }


        public NodeForceAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _f1 = info.GetDouble("F1");
            _f2 = info.GetDouble("F2");
            _f3 = info.GetDouble("F3");
            _m1 = info.GetDouble("M1");
            _m2 = info.GetDouble("M2");
            _m3 = info.GetDouble("M3");
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("F1", _f1);
            info.AddValue("F2", _f2);
            info.AddValue("F3", _f3);
            info.AddValue("M1", _m1);
            info.AddValue("M2", _m2);
            info.AddValue("M3", _m3);
            info.AddValue("CoordinateSystem", _coordinateSystem);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as NodeForceAttribute);
        }


        public bool Equals(NodeForceAttribute other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other.Equals(other._coordinateSystem)
                                    && other.Equals(other._f1)
                                    && other.Equals(other._f2)
                                    && other.Equals(other._f3)
                                    && other.Equals(other._m1)
                                    && other.Equals(other._m2)
                                    && other.Equals(other._m3)
                                    && base.Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _f1.GetHashCode();
            hashCode = hashCode * -17 + _f2.GetHashCode();
            hashCode = hashCode * -17 + _f3.GetHashCode();
            hashCode = hashCode * -17 + _m1.GetHashCode();
            hashCode = hashCode * -17 + _m2.GetHashCode();
            hashCode = hashCode * -17 + _m3.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
            return hashCode;
        }

        public override object Clone()
        {
            return new NodeForceAttribute(this);
        }

        #region Override Operator
        public static bool operator ==(NodeForceAttribute obj1, NodeForceAttribute obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(NodeForceAttribute obj1, NodeForceAttribute obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion

    }
}
