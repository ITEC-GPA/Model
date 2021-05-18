using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.FEM;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class ResultNodeDisplacement : Result, ISerializable, IEquatable<ResultNodeDisplacement>
    {
        #region Variables

        private readonly double _d1;                   
        private readonly double _d2;                   
        private readonly double _d3;                   
        private readonly double _r1;                  
        private readonly double _r2;                  
        private readonly double _r3;                  

        #endregion

        #region Properties

        public double D1 => _d1;
        public double D2 => _d2;
        public double D3 => _d3;
        public double R1 => _r1;
        public double R2 => _r2;
        public double R3 => _r3;

        #endregion


        #region Public Constructors

        /// <param name="node">Node where these result are referred </param>
        /// <param name="Case">The case where these results are reffered </param>
        /// <param name="coordinateSystem">Coordinate system where these result are provided </param>
        /// <param name="d1">Displacement along <see cref="CoordinateSystem.V1"/> direction </param>
        /// <param name="d2">Displacement along <see cref="CoordinateSystem.V2"/> direction </param>
        /// <param name="d3">Displacement along <see cref="CoordinateSystem.V3"/> direction </param>
        /// <param name="r1">Rotation around <see cref="CoordinateSystem.V1"/> direction </param>
        /// <param name="r2">Rotation around <see cref="CoordinateSystem.V2"/> direction </param>
        /// <param name="r3">Rotation around <see cref="CoordinateSystem.V3"/> direction </param>
        public ResultNodeDisplacement(Node node, ILoadCase Case, CoordinateSystem coordinateSystem, double d1, double d2, double d3, double r1, double r2, double r3) 
            : base(node, Case, null, coordinateSystem)
        {
            _d1 = d1;
            _d2 = d2;
            _d3 = d3;
            _r1 = r1;
            _r2 = r2;
            _r3 = r3;
        }
        

        public ResultNodeDisplacement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }


        #endregion


        #region Public Methods - Get displacement


        /// <summary>
        /// Return the resulting displacement 
        /// </summary>
        public double GetResultingDisplacement()
        {
            return Math.Sqrt(Math.Pow(_d1, 2) + Math.Pow(_d2, 2) + Math.Pow(_d3, 2));
        }

        /// <summary>
        /// Return the resulting vector displacement 
        /// </summary>
        public Vector3d GetResultingVectorDisplacement()
        {
            return new Vector3d(_d1, _d2, _d3);
        }

        /// <summary>
        /// Return the resulting Rotation 
        /// </summary>
        public double GetResultingRotation()
        {
            return Math.Sqrt(Math.Pow(_r1, 2) + Math.Pow(_r2, 2) + Math.Pow(_r3, 2));
        }

        /// <summary>
        /// Return the resulting vector rotation 
        /// </summary>
        public Vector3d GetResultingVectorRotation()
        {
            return new Vector3d(_r1, _r2, _r3);
        }

        /// <summary>
        /// Return the global displacements of the point
        /// </summary>
        /// <returns>Array of displacements in global coordinate</returns>
        public double[] GetGlobalDisplacements()
        {
            Vector3d DisplResult = new Vector3d(_d1, _d2, _d3);
            Vector3d GlobalDisplResult = _coordinateSystem.ToGlobal(DisplResult);

            Vector3d RotResult = new Vector3d(_r1, _r2, _r3);
            Vector3d GlobalRotResult = _coordinateSystem.ToGlobal(RotResult);

            double[] globalDispRot = new double[6];

            globalDispRot[0] = GlobalDisplResult.X;
            globalDispRot[1] = GlobalDisplResult.Y;
            globalDispRot[2] = GlobalDisplResult.Z;
            globalDispRot[3] = GlobalRotResult.X;
            globalDispRot[4] = GlobalRotResult.Y;
            globalDispRot[5] = GlobalRotResult.Z;

            return globalDispRot;
        }

        #endregion



        public Node GetNode()
        {
            return (Node)Element;
        }


        public override int GetElementId()
        {
            return Element.Id;
        }


        public override int GetResultPointId()
        {
            return ResultPoint.Id;
        }




        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as ResultNodeDisplacement);
        }

        public bool Equals(ResultNodeDisplacement other)
        {
            return !(other is null) &&
                    _d1 == other._d1 &&
                    _d2 == other._d2 &&
                    _d3 == other._d3 &&
                    _r1 == other._r1 &&
                    _r2 == other._r2 &&
                    _r3 == other._r3 &&
                    base.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _d1.GetHashCode();
                hashCode = hashCode * -17 + _d2.GetHashCode();
                hashCode = hashCode * -17 + _d3.GetHashCode();
                hashCode = hashCode * -17 + _r1.GetHashCode();
                hashCode = hashCode * -17 + _r2.GetHashCode();
                hashCode = hashCode * -17 + _r3.GetHashCode();
                return hashCode; 
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(ResultNodeDisplacement obj1, ResultNodeDisplacement obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultNodeDisplacement obj1, ResultNodeDisplacement obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
