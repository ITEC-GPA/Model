using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
	public class SectionRectangular : Section
	{
        #region Variables

        protected readonly double _height;
        protected readonly double _width;

		#endregion


		#region Properties

		/// <summary>
		/// The height of the section
		/// </summary>
		public double Height => _height;

        /// <summary>
        /// The width of the section
        /// </summary>
        public double Width => _width;

		#endregion


		#region Public Constructors

		/// <summary>
		/// Default rectangular section constructor
		/// </summary>
		/// <param name="height">The height of the section</param>
		/// <param name="width">The width of the section</param>
		/// <param name="material">The material of the section</param>
		/// <param name="name">The name of the section</param>
		/// <param name="id">The unique id</param>
		public SectionRectangular(double height, double width, Material material, string name = "", int id = IDUNASSIGNED)
			: base(material, name)
		{
			_height = height;
			_width = width;
			_id = id;
		}

		public SectionRectangular(SectionRectangular section, int id = IDUNASSIGNED)
			: this(section.Height, section.Width, section.Material, section.Name, id)
		{
		}

		public SectionRectangular(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
			_height = info.GetDouble("Height");
			_width = info.GetDouble("Width");
		}

		#endregion

		public double CalculateArea()
		{
			return Width * Height;
		}


	}
}
