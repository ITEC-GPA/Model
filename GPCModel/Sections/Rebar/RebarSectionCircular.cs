using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Rebar
{
    [Serializable]
    public class RebarSectionCircular : SectionCircular, IRebarSection
    {
        #region Properties

        public SteelMaterial RebarMaterial => (SteelMaterial)_material;

        #endregion

        #region Public Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of section</param>
        /// <param name="diameter">Th diameter</param>
        /// <param name="rebarMaterial">The material</param>
        /// <param name="id">The unique id</param>
        public RebarSectionCircular(string name, double diameter, SteelMaterial rebarMaterial, int id = IDUNASSIGNED)
            : base(diameter, rebarMaterial, name)
        {
            _id = id;
        }

        public RebarSectionCircular(SectionCircular sectionCircular)
            : base(sectionCircular)
        {
        }

        public RebarSectionCircular(double diameter, SteelMaterial material)
            : this("", diameter, material)
        {
        }

        protected RebarSectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Equals - hashcode - Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}
