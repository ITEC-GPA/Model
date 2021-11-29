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

        public RebarMaterial RebarMaterial => (RebarMaterial)_material;

        #endregion

        #region Public Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of section</param>
        /// <param name="diameter">Th diameter</param>
        /// <param name="rebarMaterial">The material</param>
        /// <param name="id">The unique id</param>
        public RebarSectionCircular(string name, double diameter, RebarMaterial rebarMaterial, int id = IDUNASSIGNED)
            : base(diameter, rebarMaterial, name)
        {
            _id = id;
        }

        public RebarSectionCircular(SectionCircular sectionCircular)
            : base(sectionCircular)
        {
        }

        public RebarSectionCircular(double diameter, RebarMaterial material)
            : this("", diameter, material)
        {
        }

        public RebarSectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Field Serialization

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}
