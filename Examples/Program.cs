using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            double E = 210000;
            double ni = 0.3;
            double density = 7850;
            double fy = 355;
            double fu = 510;

            IFormatter formatter = new BinaryFormatter();
            byte[] memory = new byte[1000];
            Stream stream = new MemoryStream(memory);

            SteelMaterial steel = new SteelMaterial("nome", E, ni, fy, fu, density);
            formatter.Serialize(stream, steel);
            stream.Close();

            Stream stream2 = new MemoryStream(memory);
            SteelMaterial steel2 = (SteelMaterial)formatter.Deserialize(stream2);
            stream2.Close();

            Console.WriteLine(steel2.Guid + " " + steel.Guid);
            Console.WriteLine(steel2.E + " " + steel.E);
        }
    }
}
