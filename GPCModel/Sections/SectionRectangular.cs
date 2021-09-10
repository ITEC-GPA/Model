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
		public SectionRectangular(double height, double width, Material material, string name = "")
			:base(material, name)
		{
			_height = height;
			_width = width;
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
