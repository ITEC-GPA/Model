using GPC.Model.Materials;

namespace GPC.Model.Data.Steel
{
	/// <summary>
	/// Predefined structural steels of AISC 360: each property returns a new instance
	/// </summary>
	public class SteelMaterialAISC360Data
	{
        // <summary>
        // ASTM Designation, values from ksi to MPa from Table 2-3 of AISC manual 2005.
        // </summary>
        #region AISC manual 2005

        /// <summary>
        /// A36
        /// F_y = 36 ksi
        /// F_u = 58 ksi
        /// </summary>
        public static SteelMaterialAISC360 A36 => new SteelMaterialAISC360(name: "A36", elasticModulus: 199947.9615, fyk: 248.2112, fu: 399.8959, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A53 Gr. B
        /// F_y = 35 ksi
        /// F_u = 60 ksi
        /// </summary>
        public static SteelMaterialAISC360 A53GrB => new SteelMaterialAISC360(name: "A53 Gr. B", elasticModulus: 199947.9615, fyk: 241.3165, fu: 413.6854, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A500 Gr. B, 42 ksi
        /// F_y = 42 ksi
        /// F_u = 58 ksi
        /// </summary>
        public static SteelMaterialAISC360 A500GrB42 => new SteelMaterialAISC360(name: "A500 Gr. B (42 ksi)", elasticModulus: 199947.9615, fyk: 289.5798, fu: 399.8959, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A500 Gr. B, 46 ksi
        /// F_y = 46 ksi
        /// F_u = 58 ksi
        /// </summary>
        public static SteelMaterialAISC360 A500GrB46 => new SteelMaterialAISC360(name: "A500 Gr. B (46 ksi)", elasticModulus: 199947.9615, fyk: 317.1588, fu: 399.8959, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A500 Gr. C, 46 ksi
        /// F_y = 46 ksi
        /// F_u = 62 ksi
        /// </summary>
        public static SteelMaterialAISC360 A500GrC46 => new SteelMaterialAISC360(name: "A500 Gr. C (42 ksi)", elasticModulus: 199947.9615, fyk: 317.1588, fu: 427.4750, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A500 Gr. C, 50 ksi
        /// F_y = 50 ksi
        /// F_u = 62 ksi
        /// </summary>
        public static SteelMaterialAISC360 A500GrC50 => new SteelMaterialAISC360(name: "A500 Gr. C (50 ksi)", elasticModulus: 199947.9615, fyk: 344.7379, fu: 427.4750, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A501
        /// F_y = 36 ksi
        /// F_u = 58 ksi
        /// </summary>
        public static SteelMaterialAISC360 A501 => new SteelMaterialAISC360(name: "A501", elasticModulus: 199947.9615, fyk: 248.2113, fu: 399.8959, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A529 Gr. 50
        /// F_y = 50 ksi
        /// F_u = 65 ksi
        /// </summary>
        public static SteelMaterialAISC360 A529Gr50 => new SteelMaterialAISC360(name: "A529 Gr. 50", elasticModulus: 199947.9615, fyk: 344.7379, fu: 448.1592, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A529 Gr. 55
        /// F_y = 55 ksi
        /// F_u = 70 ksi
        /// </summary>
        public static SteelMaterialAISC360 A529Gr55 => new SteelMaterialAISC360(name: "A529 Gr. 55", elasticModulus: 199947.9615, fyk: 379.2117, fu: 482.6330, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A572 Gr. 42
        /// F_y = 42 ksi
        /// F_u = 60 ksi
        /// </summary>
        public static SteelMaterialAISC360 A572Gr42 => new SteelMaterialAISC360(name: "A572 Gr. 42", elasticModulus: 199947.9615, fyk: 289.5798, fu: 413.6854, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A572 Gr. 50
        /// F_y = 50 ksi
        /// F_u = 65 ksi
        /// </summary>
        public static SteelMaterialAISC360 A572Gr50 => new SteelMaterialAISC360(name: "A572 Gr. 50", elasticModulus: 199947.9615, fyk: 344.7379, fu: 448.1592, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A572 Gr. 55
        /// F_y = 55 ksi
        /// F_u = 70 ksi
        /// </summary>
        public static SteelMaterialAISC360 A572Gr55 => new SteelMaterialAISC360(name: "A572 Gr. 55", elasticModulus: 199947.9615, fyk: 379.2117, fu: 482.6330, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A572 Gr. 60
        /// F_y = 60 ksi
        /// F_u = 75 ksi
        /// </summary>
        public static SteelMaterialAISC360 A572Gr60 => new SteelMaterialAISC360(name: "A572 Gr. 60", elasticModulus: 199947.9615, fyk: 413.6854, fu: 517.1068, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A572 Gr. 65
        /// F_y = 65 ksi
        /// F_u = 80 ksi
        /// </summary>
        public static SteelMaterialAISC360 A572Gr65 => new SteelMaterialAISC360(name: "A572 Gr. 65", elasticModulus: 199947.9615, fyk: 448.1592, fu: 551.5806, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A618 Gr. I &amp; II
        /// F_y = 50 ksi
        /// F_u = 70 ksi
        /// </summary>
        public static SteelMaterialAISC360 A618GrI => new SteelMaterialAISC360(name: "A618 Gr. I & II", elasticModulus: 199947.9615, fyk: 344.7379, fu: 482.6330, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A618 Gr. III
        /// F_y = 50 ksi
        /// F_u = 65 ksi
        /// </summary>
        public static SteelMaterialAISC360 A618GrIII => new SteelMaterialAISC360(name: "A618 Gr. III", elasticModulus: 199947.9615, fyk: 344.7379, fu: 448.1592, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A913 Gr. 50
        /// F_y = 50 ksi
        /// F_u = 60 ksi
        /// </summary>
        public static SteelMaterialAISC360 A913Gr50 => new SteelMaterialAISC360(name: "A913 Gr. 50", elasticModulus: 199947.9615, fyk: 344.7379, fu: 413.6854, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A913 Gr. 60
        /// F_y = 60 ksi
        /// F_u = 75 ksi
        /// </summary>
        public static SteelMaterialAISC360 A913Gr60 => new SteelMaterialAISC360(name: "A913 Gr. 60", elasticModulus: 199947.9615, fyk: 413.6854, fu: 517.1068, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A913 Gr. 65
        /// F_y = 65 ksi
        /// F_u = 80 ksi
        /// </summary>
        public static SteelMaterialAISC360 A913Gr65 => new SteelMaterialAISC360(name: "A913 Gr. 65", elasticModulus: 199947.9615, fyk: 448.1592, fu: 551.5806, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A913 Gr. 70
        /// F_y = 70 ksi
        /// F_u = 90 ksi
        /// </summary>
        public static SteelMaterialAISC360 A913Gr70 => new SteelMaterialAISC360(name: "A913 Gr. 70", elasticModulus: 199947.9615, fyk: 482.6330, fu: 620.5282, steelType: SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// A992
        /// F_y = 50 ksi
        /// F_u = 65 ksi
        /// </summary>
        public static SteelMaterialAISC360 A992 => new SteelMaterialAISC360(name: "A992", elasticModulus: 199947.9615, fyk: 344.7379, fu: 448.1592, steelType: SteelMaterial.SteelTypes.Structural);

        #endregion
    }
}
