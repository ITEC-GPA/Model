using System.Runtime.InteropServices;
namespace GPC.Converter.Straus7
{
    public interface IStraus7InertiaWriteApi
    {
        void SetInertiaMassSelection(int loadCase,bool structural,bool nonStructural);
    }
    public sealed partial class Straus7NativeWriter : IStraus7InertiaWriteApi
    {
        public void SetInertiaMassSelection(int loadCase,bool structural,bool nonStructural)
            =>api.CheckWrite(api.WriteFunction<InertiaFlags>("St7SetLoadCaseMassOption")(Id,loadCase,(byte)(structural?1:0),(byte)(nonStructural?1:0)),"St7SetLoadCaseMassOption");
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]private delegate int InertiaFlags(int file,int loadCase,byte structural,byte nonStructural);
    }
}
