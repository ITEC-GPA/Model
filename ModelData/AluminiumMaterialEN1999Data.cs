using GPC.Model.Materials;

namespace GPC.Model.Data.Aluminium
{
    /// <summary>
    /// Predefined aluminium alloys of EN 1999-1-1: each property returns a new instance
    /// </summary>
    public class AluminiumMaterialEN1999Data
    {
        // Set strainU as 0.5*ε_u=0.5*A_50

        #region Structural 

        /// <summary>
        /// Default Aluminium "5005 - O" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum5005_O => new AluminiumMaterialEN1999("5005 - O", 70000, 35, 100, 0.12, AluminiumMaterial.AluminiumTypes.Structural, 50);

        /// <summary>
        /// Default Aluminium "5005 - H111" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum5005_H111 => new AluminiumMaterialEN1999("5005 - H111", 70000, 35, 100, 0.12, AluminiumMaterial.AluminiumTypes.Structural, 50);

        /// <summary>
        /// Default Aluminium "5005 - H12" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum5005_H12 => new AluminiumMaterialEN1999("5005 - H12", 70000, 95, 125, 0.035, AluminiumMaterial.AluminiumTypes.Structural, 12.5);

        /// <summary>
        /// Default Aluminium "5005 - H22/H32" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum5005_H22_H32 => new AluminiumMaterialEN1999("5005 - H22/H32", 70000, 80, 125, 0.05, AluminiumMaterial.AluminiumTypes.Structural, 12.5);

        /// <summary>
        /// Default Aluminium "5005 - H14" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum5005_H14 => new AluminiumMaterialEN1999("5005 - H14", 70000, 120, 145, 0.025, AluminiumMaterial.AluminiumTypes.Structural, 12.5);

        /// <summary>
        /// Default Aluminium "5005 - H24/H34" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum5005_H24_H34 => new AluminiumMaterialEN1999("5005 - H24/H34", 70000, 110, 145, 0.04, AluminiumMaterial.AluminiumTypes.Structural, 12.5);

        /// <summary>
        /// Default Aluminium "6005A - T6" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum6005A_T6 => new AluminiumMaterialEN1999("6005A - T6", 70000, 200, 250, 0.04, AluminiumMaterial.AluminiumTypes.Structural, 25);

        /// <summary>
        /// Default Aluminium "6060 - T5" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum6060_T5 => new AluminiumMaterialEN1999("6060 - T5", 70000, 100, 140, 0.04, AluminiumMaterial.AluminiumTypes.Structural, 25);

        /// <summary>
        /// Default Aluminium "6060 - T6" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum6060_T6 => new AluminiumMaterialEN1999("6060 - T6", 70000, 140, 170, 0.04, AluminiumMaterial.AluminiumTypes.Structural, 20);

        /// <summary>
        /// Default Aluminium "6061 - T6" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum6061_T6 => new AluminiumMaterialEN1999("6061 - T6", 70000, 240, 260, 0.04, AluminiumMaterial.AluminiumTypes.Structural, 20);

        /// <summary>
        /// Default Aluminium "6063 - T5" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum6063_T5 => new AluminiumMaterialEN1999("6063 - T5", 70000, 110, 160, 0.035, AluminiumMaterial.AluminiumTypes.Structural, 25);

        /// <summary>
        /// Default Aluminium "6063 - T6" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum6063_T6 => new AluminiumMaterialEN1999("6063 - T6", 70000, 160, 195, 0.04, AluminiumMaterial.AluminiumTypes.Structural, 25);

        /// <summary>
        /// Default Aluminium "6082 - T5" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum6082_T5 => new AluminiumMaterialEN1999("6082 - T5", 70000, 230, 270, 0.04, AluminiumMaterial.AluminiumTypes.Structural, 5);

        /// <summary>
        /// Default Aluminium "6082 - T6" according to EN1999-1-1:2023.
        /// </summary>
        public static AluminiumMaterialEN1999 Aluminum6082_T6 => new AluminiumMaterialEN1999("6082 - T6", 70000, 240, 295, 0.04, AluminiumMaterial.AluminiumTypes.Structural, 20);

        #endregion
    }
}
