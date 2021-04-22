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
    public class ModelAccelerationAttribute : LoadCaseAttribute, IModelAttribute
    {
        /// <summary>
        /// Value of the gravity accelaration [mm/s^2]
        /// </summary>
        public const double GRAVITYACCELERATION = 9806.65;

        private double _a1;
        private double _a2;
        private double _a3;

        private CoordinateSystem _coordinateSystem;



        public double A1 => _a2;
        public double A2 => _a3;
        public double A3 => _a1;
        public CoordinateSystem CoordinateSystem => _coordinateSystem;


        internal ModelAccelerationAttribute(string loadCaseName)
            : this(loadCaseName, null, 0, 0, 0)
        {

        }

        /// <param name="loadCaseName"></param>
        /// <param name="coordinateSystem"></param>
        /// <param name="a1">Acceleration along the axis: <see cref="CoordinateSystem.V1"/> [L/T^2]</param>
        /// <param name="a2">Acceleration along the axis: <see cref="CoordinateSystem.V2"/> [L/T^2]</param>
        /// <param name="a3">Acceleration along the axis: <see cref="CoordinateSystem.V3"/> [L/T^2]</param>
        public ModelAccelerationAttribute(string loadCaseName, CoordinateSystem coordinateSystem, double a1, double a2, double a3) 
            : base(loadCaseName)
        {
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException();

            _a1 = a1;
            _a2 = a2;
            _a3 = a3;
        }


        /// <param name="loadCaseName"></param>
        /// <param name="a1">Acceleration along the axis: <see cref="CoordinateSystem.V1"/> [L/T^2]</param>
        /// <param name="a2">Acceleration along the axis: <see cref="CoordinateSystem.V2"/> [L/T^2]</param>
        /// <param name="a3">Acceleration along the axis: <see cref="CoordinateSystem.V3"/> [L/T^2]</param>
        /// <remarks>This constructor set the <see cref="ModelAccelerationAttribute.CoordinateSystem"/> to <see cref="CoordinateSystem.Global"/> </remarks>
        public ModelAccelerationAttribute(string loadCaseName, double a1, double a2, double a3) : this(loadCaseName, CoordinateSystem.Global, a1, a2, a3) { }


        public ModelAccelerationAttribute(ModelAccelerationAttribute loadCaseAttribute) 
            : this(loadCaseAttribute.LoadCaseName, loadCaseAttribute._coordinateSystem, loadCaseAttribute._a1, loadCaseAttribute._a2, loadCaseAttribute._a3)
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
                   _a1 == objCasted._a1 &&
                   _a2 == objCasted._a2 &&
                   _a3 == objCasted._a3 &&
                   EqualityComparer<CoordinateSystem>.Default.Equals(_coordinateSystem, objCasted._coordinateSystem);
        }


        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _a1.GetHashCode();
            hashCode = hashCode * -17 + _a2.GetHashCode();
            hashCode = hashCode * -17 + _a3.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
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
