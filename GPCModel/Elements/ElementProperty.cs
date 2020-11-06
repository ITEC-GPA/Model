using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Elements
{
    /// <summary>
    /// Element base abstract class that is the base for all the objects inside GPC environment.
    /// </summary>

    [Serializable]
    public abstract class ElementProperty : ModelObject
    {
        #region Variables
        /// <summary>
        /// <param name="_material"> Material of the element</param>
        /// <param name="_name"> Material of the element</param>
        /// </summary>
        protected Material _material;
        #endregion

        #region Properties
        public Material Material => _material;
        #endregion

        #region Public Constructors
        protected ElementProperty(Material material)
            : base(new Guid())
        {
        }

        protected ElementProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion Public Constructors

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Material", _material);
        }
    }
}

