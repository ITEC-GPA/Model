using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;


namespace GPC.Model.Fem.Attributes
{
    [Obsolete("This class needs to be revised and in case modified")]
    public class ModelGravityAttribute : LoadCaseAttribute, IModelAttribute, ISerializable
    {

        /// <summary>
        /// Value of the gravity acceleration [mm/s^2]
        /// </summary>
        public const double GRAVITYACCELERATION = 9806.65;


        public Vector3d Vector { get; private set; }

        public double Acceleration { get; private set; }


        public ModelGravityAttribute(string loadCaseName) : base(loadCaseName)
        {

        }

        public ModelGravityAttribute(ModelGravityAttribute modelGravityAttribute)
            : base(modelGravityAttribute.LoadCaseName)
        {
            Vector = modelGravityAttribute.Vector;
            Acceleration = modelGravityAttribute.Acceleration;
        }

        protected ModelGravityAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Vector = (Vector3d)info.GetValue("Vector", typeof(Vector3d));
            Acceleration = (double)info.GetValue("Acceleration", typeof(double));
        }


        public void SetGravityX(double value)
        {
            Vector = CoordinateSystem.Global.V1;
            Acceleration = value;
        }


        public void SetGravityY(double value)
        {
            Vector = CoordinateSystem.Global.V2;
            Acceleration = value;
        }


        public void SetGravityZ(double value)
        {
            Vector = CoordinateSystem.Global.V3;
            Acceleration = value;
        }


        public override object Clone()
        {
            return new ModelGravityAttribute(this);
        }


        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ModelGravityAttribute objCasted = obj as ModelGravityAttribute;

            return objCasted != null &&
                   base.Equals(objCasted) &&
                   Vector == objCasted.Vector &&
                   Acceleration == objCasted.Acceleration;
        }


        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + Vector.GetHashCode();
            hashCode = hashCode * -17 + Acceleration.GetHashCode();
            return hashCode;
        }


        public static bool operator ==(ModelGravityAttribute obj1, ModelGravityAttribute obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }


        public static bool operator !=(ModelGravityAttribute obj1, ModelGravityAttribute obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
