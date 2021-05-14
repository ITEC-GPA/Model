using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using GPC.Model.Materials;
using GPC.Model.FEM.Materials;

namespace GPC.Model.FEM.Properties
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class PlateProperty : ElementProperty, IPlateProperty
    {
        #region Variables
        protected FemMaterial _material;

        protected double _bendingThickness;

        protected double _membraneThickness;

        #endregion

        #region Properties

        public double BendingThickness => _bendingThickness;

        public double MembraneThickness => _membraneThickness;

        public FemMaterial Material => _material;

        #endregion

        #region Public Constructors

        /// <summary>
        /// <param name="material"></param>
        /// <param name="bendingThickness"> Bending thickness</param>
        /// <param name="membraneThickness"> Membranal thickness</param>
        /// <param name="name"></param>
        /// </summary>
        public PlateProperty(FemMaterial material, double bendingThickness, double membraneThickness, string name)
            : base(name)
        {
            _bendingThickness = bendingThickness;
            _membraneThickness = membraneThickness;
            _material = material ?? throw new ArgumentNullException("Material cannot be null");
        }


        public PlateProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bendingThickness = info.GetDouble("BendingThickness");
            _membraneThickness = info.GetDouble("MembranalThickness");
            throw new NotImplementedException();
        }


        #endregion


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BendingThickness", _bendingThickness);
            info.AddValue("MembranalThickness", _membraneThickness);
            info.AddValue("Material", _material);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            PlateProperty objCasted = obj as PlateProperty;
            return !(objCasted is null) && _bendingThickness == objCasted._bendingThickness &&
                                           _membraneThickness == objCasted._membraneThickness &&
                                           _material == objCasted._material &&
                                           base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _bendingThickness.GetHashCode();
            hashCode = hashCode * -17 + _membraneThickness.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<FemMaterial>.Default.GetHashCode(_material);
            return hashCode;
        }

        private string GetDebuggerDisplay()
        {
            return $"PlateProperty: {_name}";
        }
    }
}
