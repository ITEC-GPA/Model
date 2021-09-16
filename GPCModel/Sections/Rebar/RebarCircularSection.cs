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
        // public const double PRESTRESSED_LIMIT = 0.0000001;

        #region Variables

        protected double _epsilonP;
        protected double _tensionP;

        #endregion

        #region Properties

        public double EpsilonP => _epsilonP;

        public double TensionP => _tensionP;

        public bool IsPrestressed => _tensionP > 0.0;

		public RebarMaterial RebarMaterial => (RebarMaterial)_material;

		#endregion

		#region Public Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">The name of section</param>
		/// <param name="diameter">Th diameter</param>
		/// <param name="rebarMaterial">The material</param>
		/// <param name="epsilonP"></param>
		/// <param name="tensionP"></param>
		/// <param name="id">The unique id</param>
		public RebarCircularSection(string name, double diameter, RebarMaterial rebarMaterial, 
            double epsilonP = 0.0, double tensionP = 0.0, int id = IDUNASSIGNED)
            : base(diameter, rebarMaterial, name)
        {
            _epsilonP = epsilonP;
            _tensionP = tensionP;
            _id = id;
        }

        public RebarCircularSection(SectionCircular sectionCircular, double epsilonP = 0.0, double tensionP = 0.0)
            : base(sectionCircular)
        {
            _epsilonP = epsilonP;
            _tensionP = tensionP;            
        }

        public RebarCircularSection(string name, double diameter, RebarMaterial rebarMaterial, 
            int id = IDUNASSIGNED, double epsilonP = 0.0, double tensionP = 0.0)
            : this(name, diameter, rebarMaterial, epsilonP, tensionP, id)
        {
        }

        public RebarCircularSection(double diameter, RebarMaterial material) 
            : this("", diameter, material, 0.0, 0.0)
        {
        }

        public RebarCircularSection(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _epsilonP = info.GetDouble("EpsilonP");
            _tensionP = info.GetDouble("TensionP");
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

        public void AddPrestress(double tensionP)
        {
            _tensionP = tensionP;
        }

        #endregion

        #region Private Methods Specific


        #endregion

        #region Field Serialization

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("EpsilonP", _epsilonP);
            info.AddValue("TensionP", _tensionP);
        }

        #endregion
    }
}
