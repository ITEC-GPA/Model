using GPC.Model.Materials;

namespace GPC.Model.Data.Steel
{
	public class BoltMaterialEN1993Data
	{
        #region Structural 

        /// <summary>
        /// Default steel according to EN1993 for bolt class 4.6.
        /// </summary>
        public static BoltMaterialEN1993 Class4_6 => new BoltMaterialEN1993("4.6", 210000, 240, 400);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 4.8.
        /// </summary>
        public static BoltMaterialEN1993 Class4_8 => new BoltMaterialEN1993("4.8", 210000, 320, 400);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 5.6.
        /// </summary>
        public static BoltMaterialEN1993 Class5_6 => new BoltMaterialEN1993("5.6", 210000, 300, 500);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 5.8.
        /// </summary>
        public static BoltMaterialEN1993 Class5_8 => new BoltMaterialEN1993("5.8", 210000, 400, 500);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 6.8.
        /// </summary>
        public static BoltMaterialEN1993 Class6_8 => new BoltMaterialEN1993("6.8", 210000, 480, 600);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 8.8.
        /// </summary>
        public static BoltMaterialEN1993 Class8_8 => new BoltMaterialEN1993("8.8", 210000, 640, 800);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 10.9.
        /// </summary>
        public static BoltMaterialEN1993 Class10_9 => new BoltMaterialEN1993("10.9", 210000, 900, 1000);

        #endregion
    }
}
