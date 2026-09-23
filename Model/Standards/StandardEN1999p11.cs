using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// Eurocode 9 - Design of aluminium structures.
    /// </summary>
    [Serializable]
    public class StandardEN1999p11 : Standard, ISerializable
    {
        #region Properties

        /// <summary>
        /// EN 1999-1-1:2023 definition. --> Resistance of cross section and resistance of members to instability, based on f_o.
        /// </summary>
        public double GammaM1 { get; set; }

        /// <summary>
        /// EN 1999-1-1:2023 definition. --> Resistance of cross sections to fracture and resistance of joints in tension, shear and bearing, based on f_u.
        /// </summary>
        public double GammaM2 { get; set; }

        /// <summary>
        /// EN 1999-1-1:2023 definition. --> Resistance of cross sections to fracture and resistance of joints in tension, shear and bearing, based on f_w.
        /// </summary>
        public double GammaMw { get; set; }

        /// <summary>
        /// EN 1999-1-1:2023 definition. --> Resistance of pin connections for ultimate limit state.
        /// </summary>
        public double GammaMp { get; set; }

        /// <summary>
        /// EN 1999-1-1:2023 definition. --> Resistance of pin connections for serviceability limit state.
        /// </summary>
        public double GammaMpSer { get; set; }

        /// <summary>
        /// EN 1999-1-1:2023 definition. --> Slip resistance of connections at ultimate limit state.
        /// </summary>
        public double GammaMs { get; set; }

        /// <summary>
        /// EN 1999-1-1:2023 definition. --> Slip resistance of connections at serviceability limit state.
        /// </summary>
        public double GammaMsSer { get; set; }

        public override StandardGroupType StandardGroup => StandardGroupType.European;

        #endregion

        #region Constructor

        public StandardEN1999p11(string name = "EN 1999-1-1:2023", string remarks = "Eurocode 9")
            : base(name, remarks)
        {
            GammaM1 = 1.1; // EN 1999-1-1:2023, UNI EN 1999-1-1:2023 (E)
            GammaM2 = 1.25; // EN 1999-1-1:2023, UNI EN 1999-1-1:2023 (E)
            GammaMw = 1.25; // EN 1999-1-1:2023, UNI EN 1999-1-1:2023 (E)
            GammaMp = 1.25; // EN 1999-1-1:2023, UNI EN 1999-1-1:2023 (E)
            GammaMpSer = 1.0; // EN 1999-1-1:2023, UNI EN 1999-1-1:2023 (E)
            GammaMs = 1.4; // EN 1999-1-1:2023, UNI EN 1999-1-1:2023 (E)
            GammaMsSer = 1.0; // EN 1999-1-1:2023, UNI EN 1999-1-1:2023 (E)
        }

        protected StandardEN1999p11(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            GammaM1 = info.GetDouble("GammaM1");
            GammaM2 = info.GetDouble("GammaM2");
            GammaMw = info.GetDouble("GammaMw");
            GammaMp = info.GetDouble("GammaMp");
            GammaMpSer = info.GetDouble("GammaMpSer");
            GammaMs = info.GetDouble("GammaMs");
            GammaMsSer = info.GetDouble("GammaMsSer");
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GammaM1", GammaM1);
            info.AddValue("GammaM2", GammaM2);
            info.AddValue("GammaMw", GammaMw);
            info.AddValue("GammaMp", GammaMp);
            info.AddValue("GammaMpSer", GammaMpSer);
            info.AddValue("GammaMs", GammaMs);
            info.AddValue("GammaMsSer", GammaMsSer);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is StandardEN1999p11 p &&
                   GammaM1 == p.GammaM1 &&
                   GammaM2 == p.GammaM2 &&
                   GammaMw == p.GammaMw &&
                   GammaMp == p.GammaMp &&
                   GammaMpSer == p.GammaMpSer &&
                   GammaMs == p.GammaMs &&
                   GammaMsSer == p.GammaMsSer;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -59 + base.GetHashCode();
                hashCode = hashCode * -59 + GammaM1.GetHashCode();
                hashCode = hashCode * -59 + GammaM2.GetHashCode();
                hashCode = hashCode * -59 + GammaMw.GetHashCode();
                hashCode = hashCode * -59 + GammaMp.GetHashCode();
                hashCode = hashCode * -59 + GammaMpSer.GetHashCode();
                hashCode = hashCode * -59 + GammaMs.GetHashCode();
                hashCode = hashCode * -59 + GammaMsSer.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}
