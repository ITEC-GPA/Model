using System;

namespace GPC.Model.FEM 
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
        }

        public static int MAXDOFPERNODE = Enum.GetNames(typeof(DOF)).Length;

        public Solver() { }

        public void Call() { }
    }
}
