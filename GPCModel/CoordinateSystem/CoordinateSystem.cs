using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using MathNet.Numerics.LinearAlgebra;


namespace GPC.Model.CoordinateSystems
{
    public class CoordinateSystem : ModelObject
    {
        #region Variables
        protected Point3d _origin;
        protected Vector3d _v11;
        protected Vector3d _v22;
        protected Vector3d _v33;
        protected Vector3d _InvOrigin;
        protected double _rotAngle;
        protected Matrix<double> _trfMatrix;
        #endregion


        #region Properties
        public Point3d Origin => _origin;
        public Vector3d V11 => _v11;
        public Vector3d V22 => _v22;
        public Vector3d V33 => _v33;
        public double RotAngle => _rotAngle;
        public Matrix<double> TrfMatrix => _trfMatrix;
        #endregion

        #region Public Constructors
        public CoordinateSystem(Guid guid, Point3d p1, Point3d p2, Point3d p3, double rotAngle = 0) 
            : base(guid)
        {
            _rotAngle = rotAngle;
            _trfMatrix = Matrix<double>.Build.Dense(3, 4, 0.0);
            SetTransformationMatrix(p1, p2, p3);
        }
        public CoordinateSystem(SerializationInfo info, StreamingContext context)
            : base (info, context)
        {
            _v11 = (Vector3d)info.GetValue("V11Direction", typeof(Vector3d));
            _v22 = (Vector3d)info.GetValue("V22Direction", typeof(Vector3d));
            _v33 = (Vector3d)info.GetValue("V33Direction", typeof(Vector3d));
            _rotAngle = (double)info.GetValue("RotationAngle", typeof(double));
            _InvOrigin = (Vector3d)info.GetValue("InvariantOrigin", typeof(Vector3d));
            _trfMatrix = (Matrix<double>)info.GetValue("TransformationMatrix", typeof(Matrix<double>));
        }
        #endregion

        #region Public Methods Specific
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("V11Direction", _v11);
            info.AddValue("V22Direction", _v22);
            info.AddValue("V33Direction", _v33);
            info.AddValue("RotationAngle", _rotAngle);
            info.AddValue("InvariantOrigin", _InvOrigin);
            info.AddValue("TransformationMatrix", _trfMatrix);
        }
        public virtual void SetOrigin(Point3d origin)
        {
            _trfMatrix[0, 3] = origin.X;
            _trfMatrix[1, 3] = origin.Y;
            _trfMatrix[2, 3] = origin.Z;
            _origin = new Point3d(origin.X, origin.Y, origin.Z);
        }

        public virtual void SetTransformationMatrix(Point3d p1, Point3d p2, Point3d p3)
        {
            SetOrigin(p1);

            Vector3d v1 = new Vector3d((p2.X - p1.X), (p2.Y - p1.Y), (p2.Z - p1.Z));
            v1.Unitize();
            Vector3d v2 = new Vector3d((p3.X - p1.X), (p3.Y - p1.Y), (p3.Z - p1.Z));
            v2.Unitize();
            v2 = v2 - (v1 * v2) * v1;
            v2.Unitize();
            Vector3d v3 = v1 ^ v2;

            _trfMatrix[0, 0] = v1.X;
            _trfMatrix[1, 0] = v1.Y;
            _trfMatrix[2, 0] = v1.Z;

            _trfMatrix[0, 1] = v2.X;
            _trfMatrix[1, 1] = v2.Y;
            _trfMatrix[2, 1] = v2.Z;

            _trfMatrix[0, 2] = v3.X;
            _trfMatrix[1, 2] = v3.Y;
            _trfMatrix[2, 2] = v3.Z;

            _v11 = v1;
            _v22 = v2;
            _v33 = v3;

            _InvOrigin = new Vector3d(-(_trfMatrix[0, 0] * Origin.X + _trfMatrix[1, 0] * Origin.Y + _trfMatrix[2, 0] * Origin.Z),
                                      -(_trfMatrix[0, 1] * Origin.X + _trfMatrix[1, 1] * Origin.Y + _trfMatrix[2, 1] * Origin.Z),
                                      -(_trfMatrix[0, 2] * Origin.X + _trfMatrix[1, 2] * Origin.Y + _trfMatrix[2, 2] * Origin.Z));
        }

        public virtual Point3d PointToLocal(Point3d pGlob)
        {
            Point3d pLocal = new Point3d(_trfMatrix[0,0] * pGlob.X + _trfMatrix[1,0] * pGlob.Y + _trfMatrix[2,0] * pGlob.Z + _InvOrigin.X,
                                         _trfMatrix[0,1] * pGlob.X + _trfMatrix[1,1] * pGlob.Y + _trfMatrix[2,1] * pGlob.Z + _InvOrigin.Y,
                                         _trfMatrix[0,2] * pGlob.X + _trfMatrix[1,2] * pGlob.Y + _trfMatrix[2,2] * pGlob.Z + _InvOrigin.Z);
            return pLocal;
        }
        #endregion
    }
}
