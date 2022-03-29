using System;

namespace GPC.Model.Fem
{
    public class Solver : ModelObject
    {
        public enum DOF
        {
            DX,   //0
            DY,   //1
            DZ,   //2
            RX,   //3
            RY,   //4
            RZ,   //5
            DDX,   //0
            DDY,   //1
            DDZ,   //2
        }

        public static int MAXDOFPERNODE = Enum.GetNames(typeof(DOF)).Length;

        public Solver() { }

        public void Call() { }
    }
}
