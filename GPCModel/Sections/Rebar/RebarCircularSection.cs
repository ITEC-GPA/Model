using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Rebar
{
	public class RebarCircularSection : SectionCircular, IRebarSection
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
		public RebarCircularSection(string name, double diameter, RebarMaterial rebarMaterial, int id = IDUNASSIGNED)
            : base(diameter, rebarMaterial, name)
        {
            _id = id;
        }

        public RebarCircularSection(SectionCircular sectionCircular)
            : base(sectionCircular)
        {         
        }

        public RebarCircularSection(double diameter, RebarMaterial material) 
            : this("", diameter, material)
        {
        }

        public RebarCircularSection(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        #endregion


        #region Public Methods Specific

        public void ChangeDiameter(double newDiamter)
        {
            _diameter = newDiamter;
        }

        public void ChangeMaterial(RebarMaterial newMaterial)
        {
            _material = newMaterial;
        }

        #endregion

        #region Private Methods Specific


        #endregion

        #region Field Serialization

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}
