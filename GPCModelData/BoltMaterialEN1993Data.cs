using GPC.Model.Materials;

namespace GPC.Model.Data.Steel
{
	public class BoltMaterialEN1993Data
	{
		#region Structural 

		/// <summary>
		/// Default steel according to EN1993 for bolt class 4.6.
		/// </summary>
		public static BoltMaterialEN1993 Class3_6 => new BoltMaterialEN1993("3.6", 210000, 180, 300, 0.25);

		/// <summary>
		/// Default steel according to EN1993 for bolt class 4.6.
		/// </summary>
		public static BoltMaterialEN1993 Class4_6 => new BoltMaterialEN1993("4.6", 210000, 240, 400, 0.22);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 4.8.
        /// </summary>
        public static BoltMaterialEN1993 Class4_8 => new BoltMaterialEN1993("4.8", 210000, 320, 400, 0.16);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 5.6.
        /// </summary>
        public static BoltMaterialEN1993 Class5_6 => new BoltMaterialEN1993("5.6", 210000, 300, 500, 0.20);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 5.8.
        /// </summary>
        public static BoltMaterialEN1993 Class5_8 => new BoltMaterialEN1993("5.8", 210000, 400, 500, 0.10);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 6.8.
        /// </summary>
        public static BoltMaterialEN1993 Class6_8 => new BoltMaterialEN1993("6.8", 210000, 480, 600, 0.08);

        /// <summary>
        /// Default steel according to EN1993 for bolt class 8.8.
        /// </summary>
        public static BoltMaterialEN1993 Class8_8 => new BoltMaterialEN1993("8.8", 210000, 640, 800, 0.12);

		/// <summary>
		/// Default steel according to EN1993 for bolt class 8.8.
		/// </summary>
		public static BoltMaterialEN1993 Class9_8 => new BoltMaterialEN1993("9.8", 210000, 720, 900, 0.10);

		/// <summary>
		/// Default steel according to EN1993 for bolt class 10.9.
		/// </summary>
		public static BoltMaterialEN1993 Class10_9 => new BoltMaterialEN1993("10.9", 210000, 900, 1000, 0.09);

		/// <summary>
		/// Default steel according to EN1993 for bolt class 12.9.
		/// </summary>
		public static BoltMaterialEN1993 Class12_9 => new BoltMaterialEN1993("12.9", 210000, 1080, 1200, 0.08);

		/// <summary>
		/// Default steel according to EN1993 for bolt class 12.9.
		/// </summary>
		public static BoltMaterialEN1993 Class14_9 => new BoltMaterialEN1993("14.9", 210000, 1260, 1400, 0.07);

		#endregion
	}
}
