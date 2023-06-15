using GPC.Model.Materials;

namespace GPC.Model.Data.Steel
{
	public class SteelMaterialEN1992Data
	{
        #region Rebar

        public static SteelMaterialEN1992 B450A => new SteelMaterialEN1992("B450A", 200000, 450, 450, 0.03, SteelMaterial.SteelTypes.Rebar);
        public static SteelMaterialEN1992 B450AHardening => new SteelMaterialEN1992("B450A Hardening", 200000, 450, 540, 0.03, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialEN1992 B450C => new SteelMaterialEN1992("B450C", 200000, 450, 450, 0.075, SteelMaterial.SteelTypes.Rebar);
        public static SteelMaterialEN1992 B450CHardening => new SteelMaterialEN1992("B450C Hardening", 200000, 450, 540, 0.075, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialEN1992 B500A => new SteelMaterialEN1992("B500A", 200000, 500, 500, 0.03, SteelMaterial.SteelTypes.Rebar);
        public static SteelMaterialEN1992 B500AHardening => new SteelMaterialEN1992("B500A Hardening", 200000, 500, 525, 0.03, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialEN1992 B500B => new SteelMaterialEN1992("B500B", 200000, 500, 500, 0.05, SteelMaterial.SteelTypes.Rebar);
        public static SteelMaterialEN1992 B500BHardening => new SteelMaterialEN1992("B500B Hardening", 200000, 500, 550, 0.05, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialEN1992 B500C => new SteelMaterialEN1992("B500C", 200000, 500, 500, 0.075, SteelMaterial.SteelTypes.Rebar);
        public static SteelMaterialEN1992 B500CHardening => new SteelMaterialEN1992("B500C Hardening", 200000, 500, 575, 0.075, SteelMaterial.SteelTypes.Rebar);

        #endregion

        #region Bars

        public static SteelMaterialEN1992 Y1180C => new SteelMaterialEN1992("Y1180", 205000, 1030, 1030, 0.075, SteelMaterial.SteelTypes.Bars);
        public static SteelMaterialEN1992 Y1180CHardening => new SteelMaterialEN1992("Y1180 Hardening", 205000, 1030, 1180, 0.075, SteelMaterial.SteelTypes.Bars);

        public static SteelMaterialEN1992 Y1240C => new SteelMaterialEN1992("Y1240", 205000, 1050, 1050, 0.075, SteelMaterial.SteelTypes.Bars);
        public static SteelMaterialEN1992 Y1240CHardening => new SteelMaterialEN1992("Y1240 Hardening", 205000, 1050, 1240, 0.075, SteelMaterial.SteelTypes.Bars);

        public static SteelMaterialEN1992 Y1280C => new SteelMaterialEN1992("Y1280", 205000, 1100, 1100, 0.075, SteelMaterial.SteelTypes.Bars);
        public static SteelMaterialEN1992 Y1280CHardening => new SteelMaterialEN1992("Y1280 Hardening", 205000, 1100, 1280, 0.075, SteelMaterial.SteelTypes.Bars);

        public static SteelMaterialEN1992 Y1370C => new SteelMaterialEN1992("Y1370", 205000, 1200, 1200, 0.075, SteelMaterial.SteelTypes.Bars);
        public static SteelMaterialEN1992 Y1370CHardening => new SteelMaterialEN1992("Y1370 Hardening", 205000, 1200, 1370, 0.075, SteelMaterial.SteelTypes.Bars);

        public static SteelMaterialEN1992 Y1420C => new SteelMaterialEN1992("Y1420", 205000, 1230, 1230, 0.075, SteelMaterial.SteelTypes.Bars);
        public static SteelMaterialEN1992 Y1420CHardening => new SteelMaterialEN1992("Y1420 Hardening", 205000, 1230, 1420, 0.075, SteelMaterial.SteelTypes.Bars);

        public static SteelMaterialEN1992 Y1470C => new SteelMaterialEN1992("Y1470", 205000, 1290, 1290, 0.075, SteelMaterial.SteelTypes.Bars);
        public static SteelMaterialEN1992 Y1470CHardening => new SteelMaterialEN1992("Y1470 Hardening", 205000, 1290, 1470, 0.075, SteelMaterial.SteelTypes.Bars);

        #endregion

        #region Tendon

        public static SteelMaterialEN1992 Y1570C => new SteelMaterialEN1992("Y1570", 195000, 1360, 1360, 0.075, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterialEN1992 Y1570CHardening => new SteelMaterialEN1992("Y1570 Hardening", 195000, 1360, 1570, 0.075, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterialEN1992 Y1620C => new SteelMaterialEN1992("Y1620", 195000, 1420, 1420, 0.075, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterialEN1992 Y1620CHardening => new SteelMaterialEN1992("Y1620 Hardening", 195000, 1420, 1620, 0.075, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterialEN1992 Y1670C => new SteelMaterialEN1992("Y1670", 195000, 1480, 1480, 0.075, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterialEN1992 Y1670CHardening => new SteelMaterialEN1992("Y1670 Hardening", 195000, 1480, 1670, 0.075, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterialEN1992 Y1770C => new SteelMaterialEN1992("Y1770", 195000, 1560, 1560, 0.075, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterialEN1992 Y1770CHardening => new SteelMaterialEN1992("Y1770 Hardening", 195000, 1560, 1770, 0.075, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterialEN1992 Y1860C => new SteelMaterialEN1992("Y1860", 195000, 1640, 1640, 0.075, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterialEN1992 Y1860CHardening => new SteelMaterialEN1992("Y1860 Hardening", 195000, 1640, 1860, 0.075, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterialEN1992 Y1960C => new SteelMaterialEN1992("Y1960", 195000, 1740, 1740, 0.075, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterialEN1992 Y1960CHardening => new SteelMaterialEN1992("Y1960 Hardening", 195000, 1740, 1960, 0.075, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterialEN1992 Y2060C => new SteelMaterialEN1992("Y2060", 195000, 1820, 1820, 0.075, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterialEN1992 Y2060CHardening => new SteelMaterialEN1992("Y2060 Hardening", 195000, 1820, 2060, 0.075, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterialEN1992 Y2160C => new SteelMaterialEN1992("Y2160", 195000, 1900, 1900, 0.075, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterialEN1992 Y2160CHardening => new SteelMaterialEN1992("Y2160 Hardening", 195000, 1900, 2160, 0.075, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterialEN1992 Y2260C => new SteelMaterialEN1992("Y2260", 195000, 1980, 1980, 0.075, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterialEN1992 Y2260CHardening => new SteelMaterialEN1992("Y2260 Hardening", 195000, 1980, 2260, 0.075, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterialEN1992 Y2360C => new SteelMaterialEN1992("Y2360", 195000, 2070, 2070, 0.075, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterialEN1992 Y2360CHardening => new SteelMaterialEN1992("Y2360 Hardening", 195000, 2070, 2360, 0.075, SteelMaterial.SteelTypes.Tendon);

        #endregion
    }
}
