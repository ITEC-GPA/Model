using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Rebar
{
    [Serializable]
    public class RebarSectionCircular : SectionCircular, IRebarSection, ISerializable
    {
        protected SteelMaterial _steelMaterial;

        public SteelMaterial RebarMaterial
        {
            get => _steelMaterial;
            set => _steelMaterial = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of section</param>
        /// <param name="diameter">Th diameter</param>
        /// <param name="rebarMaterial">The material</param>
        /// <param name="id">The unique id</param>
        public RebarSectionCircular(string name, double diameter, SteelMaterial rebarMaterial, int id = IDUNASSIGNED)
            : base(diameter, name)
        {
            _steelMaterial = rebarMaterial;
            _id = id;
        }

        public RebarSectionCircular(double diameter, SteelMaterial material)
            : this("", diameter, material)
        {
        }

        protected RebarSectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("RebarSectionCircularVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            if (version > 1)
                _steelMaterial = (SteelMaterial)info.GetValue("SteelMaterial", typeof(SteelMaterial));
            else
                // Before version 2 this was a GPCCheckers.Core.Mvvm.Models.SteelMaterialModel class.
                // Change of name in TypenameConverterBinder.
                _steelMaterial = (SteelMaterial)info.GetValue("Material", typeof(SteelMaterial));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("RebarSectionCircularVersion", version);

            info.AddValue("SteelMaterial", _steelMaterial);
        }
    }
}
