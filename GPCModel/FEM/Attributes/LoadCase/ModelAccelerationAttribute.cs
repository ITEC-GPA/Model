using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.FEM.Attributes
{
    /// <summary>
    /// Represent an acceleration attribute of the <see cref="FemModel"/>
    /// </summary>
    [Serializable]
    public class ModelAccelerationAttribute : LoadCaseAttribute, IModelAttribute, ISerializable
    {

        /// <summary>
        /// Acceleration along the axis: <see cref="CoordinateSystem.V1"/> [L/T^2]
        /// </summary>
        public double A1 { get; set; }
        /// <summary>
        /// Acceleration along the axis: <see cref="CoordinateSystem.V2"/> [L/T^2]
        /// </summary>
        public double A2 { get; set; }
        /// <summary>
        /// Acceleration along the axis: <see cref="CoordinateSystem.V3"/> [L/T^2]
        /// </summary>
        public double A3 { get; set; }
        public CoordinateSystem CoordinateSystem { get; set;  }


        /// <remarks>This constructor set the <see cref="ModelAccelerationAttribute.CoordinateSystem"/> to <see cref="CoordinateSystem.Global"/> </remarks>
        internal ModelAccelerationAttribute(string loadCaseName)
            : this(loadCaseName, CoordinateSystem.Global, 0, 0, 0)
        {

        }


        internal ModelAccelerationAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            A1 = (double)info.GetValue("A1", typeof(double));
            A2 = (double)info.GetValue("A2", typeof(double));
            A3 = (double)info.GetValue("A3", typeof(double));
            CoordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("A1", A1);
            info.AddValue("A2", A2);
            info.AddValue("A3", A3);
            info.AddValue("CoordinateSystem", CoordinateSystem);
        }


        /// <param name="loadCaseName"></param>
        /// <param name="coordinateSystem"></param>
        /// <param name="a1">Acceleration along the axis: <see cref="CoordinateSystem.V1"/> [L/T^2]</param>
        /// <param name="a2">Acceleration along the axis: <see cref="CoordinateSystem.V2"/> [L/T^2]</param>
        /// <param name="a3">Acceleration along the axis: <see cref="CoordinateSystem.V3"/> [L/T^2]</param>
        public ModelAccelerationAttribute(string loadCaseName, CoordinateSystem coordinateSystem, double a1, double a2, double a3) 
            : base(loadCaseName)
        {
            CoordinateSystem = coordinateSystem ?? throw new ArgumentNullException();

            A3 = a1;
            A1 = a2;
            A2 = a3;
        }


        /// <param name="loadCaseName"></param>
        /// <param name="a1">Acceleration along the axis: <see cref="CoordinateSystem.V1"/> [L/T^2]</param>
        /// <param name="a2">Acceleration along the axis: <see cref="CoordinateSystem.V2"/> [L/T^2]</param>
        /// <param name="a3">Acceleration along the axis: <see cref="CoordinateSystem.V3"/> [L/T^2]</param>
        /// <remarks>This constructor set the <see cref="ModelAccelerationAttribute.CoordinateSystem"/> to <see cref="CoordinateSystem.Global"/> </remarks>
        public ModelAccelerationAttribute(string loadCaseName, double a1, double a2, double a3) : this(loadCaseName, CoordinateSystem.Global, a1, a2, a3) { }


        public ModelAccelerationAttribute(ModelAccelerationAttribute loadCaseAttribute) 
            : this(loadCaseAttribute.LoadCaseName, loadCaseAttribute.CoordinateSystem, loadCaseAttribute.A3, loadCaseAttribute.A1, loadCaseAttribute.A2)
        {
            
        }

        public override object Clone()
        {
            return new ModelAccelerationAttribute(this);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ModelAccelerationAttribute objCasted = obj as ModelAccelerationAttribute;
            return objCasted != null &&
                   base.Equals(objCasted) &&
                   A3 == objCasted.A3 &&
                   A1 == objCasted.A1 &&
                   A2 == objCasted.A2 &&
                   EqualityComparer<CoordinateSystem>.Default.Equals(CoordinateSystem, objCasted.CoordinateSystem);
        }


        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + A3.GetHashCode();
            hashCode = hashCode * -17 + A1.GetHashCode();
            hashCode = hashCode * -17 + A2.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(CoordinateSystem);
            return hashCode;
        }

        public static bool operator ==(ModelAccelerationAttribute obj1, ModelAccelerationAttribute obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ModelAccelerationAttribute obj1, ModelAccelerationAttribute obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
