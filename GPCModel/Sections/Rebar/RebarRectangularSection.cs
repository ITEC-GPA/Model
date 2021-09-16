using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Rebar
{
	public class RebarRectangularSection : SectionRectangular, IRebarSection
	{
        #region Properties

		public RebarMaterial RebarMaterial => (RebarMaterial)_material;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="name">The name of section</param>
		/// <param name="height"></param>
		/// <param name="width"></param>
		/// <param name="rebarMaterial">The material</param>
		/// <param name="id">The unique id</param>
		public RebarRectangularSection(string name, double height, double width, RebarMaterial rebarMaterial, int id = IDUNASSIGNED)
            : base(height, width, rebarMaterial, name)
        {
            _id = id;
        }

        public RebarRectangularSection(SectionRectangular section, int id = IDUNASSIGNED)
            : base(section)
        {
            _id = id;
        }

        public RebarRectangularSection(SerializationInfo info, StreamingContext context) 
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
