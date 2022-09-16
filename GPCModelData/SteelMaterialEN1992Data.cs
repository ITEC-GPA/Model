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

        public static SteelMaterial Y1030C => new SteelMaterial("Y1030", 205000, 1030, 1030, 0.04, SteelMaterial.SteelTypes.Bars);
        public static SteelMaterial Y1030CHardening => new SteelMaterial("Y1030 Hardening", 205000, 1030, 1180, 0.04, SteelMaterial.SteelTypes.Bars);

        public static SteelMaterial Y1050C => new SteelMaterial("Y1050", 205000, 1050, 1050, 0.04, SteelMaterial.SteelTypes.Bars);
        public static SteelMaterial Y1050CHardening => new SteelMaterial("Y1050 Hardening", 205000, 1050, 1240, 0.04, SteelMaterial.SteelTypes.Bars);

        public static SteelMaterial Y1100C => new SteelMaterial("Y1100", 205000, 1100, 1100, 0.04, SteelMaterial.SteelTypes.Bars);
        public static SteelMaterial Y1100CHardening => new SteelMaterial("Y1100 Hardening", 205000, 1100, 1280, 0.04, SteelMaterial.SteelTypes.Bars);

        public static SteelMaterial Y1230C => new SteelMaterial("Y1230", 205000, 1230, 1230, 0.04, SteelMaterial.SteelTypes.Bars);
        public static SteelMaterial Y1230CHardening => new SteelMaterial("Y1230 Hardening", 205000, 1230, 1420, 0.04, SteelMaterial.SteelTypes.Bars);

        #endregion

        #region Tendon

        public static SteelMaterial Y1570C => new SteelMaterial("Y1570", 195000, 1420, 1420, 0.035, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterial Y1570CHardening => new SteelMaterial("Y1570 Hardening", 195000, 1420, 1570, 0.035, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterial Y1620C => new SteelMaterial("Y1620", 195000, 1420, 1420, 0.035, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterial Y1620CHardening => new SteelMaterial("Y1620 Hardening", 195000, 1420, 1620, 0.035, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterial Y1670C => new SteelMaterial("Y1670", 195000, 1480, 1480, 0.035, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterial Y1670CHardening => new SteelMaterial("Y1670 Hardening", 195000, 1480, 1670, 0.035, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterial Y1770C => new SteelMaterial("Y1770", 195000, 1560, 1560, 0.035, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterial Y1770CHardening => new SteelMaterial("Y1770 Hardening", 195000, 1560, 1770, 0.035, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterial Y1860C => new SteelMaterial("Y1860", 195000, 1640, 1640, 0.035, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterial Y1860CHardening => new SteelMaterial("Y1860 Hardening", 195000, 1640, 1860, 0.035, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterial Y1960C => new SteelMaterial("Y1960", 195000, 1740, 1740, 0.035, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterial Y1960CHardening => new SteelMaterial("Y1960 Hardening", 195000, 1740, 1960, 0.035, SteelMaterial.SteelTypes.Tendon);

        public static SteelMaterial Y2060C => new SteelMaterial("Y2060C", 195000, 1820, 1820, 0.035, SteelMaterial.SteelTypes.Tendon);
        public static SteelMaterial Y2060CHardening => new SteelMaterial("Y2060C Hardening", 195000, 1820, 2060, 0.035, SteelMaterial.SteelTypes.Tendon);

        #endregion
    }
}
