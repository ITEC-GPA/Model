using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Laminated glass. This represent a multilayer glass panel. Between each layer there is an interlayer
    /// </summary>
    [Serializable]
    public class LaminatedGlass : GlassPanel
    {
        #region Variables
        private readonly MonolithicGlass[] _monolithicGlasses;
        private readonly Interlayer[] _interlayers;
        #endregion

        #region Properties
        public MonolithicGlass[] MonolithicGlasses => _monolithicGlasses;
        public Interlayer[] Interlayers => _interlayers;
        #endregion

        #region Public Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="monolithicGlasses">Monolithic glasses composing the laminated panel</param>
        /// <param name="interlayers">Interlayers between monolithic glasses</param>
        public LaminatedGlass(MonolithicGlass[] monolithicGlasses, Interlayer[] interlayers) : this(monolithicGlasses, interlayers, Guid.Empty)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="monolithicGlasses">Monolithic glasses composing the laminated panel</param>
        /// <param name="interlayers">Interlayers between monolithic glasses</param>
        /// <param name="guid">The guid of of the glass</param>
        public LaminatedGlass(MonolithicGlass[] monolithicGlasses, Interlayer[] interlayers, Guid guid) : base(guid)
        {
            if (monolithicGlasses == null)
                throw new ArgumentException("Monolithic glasses cannot be null");
        
            if (monolithicGlasses.Length < 2)            
                throw new ArgumentException("Number of monolithic glasses should be greater than one");
            
            if (interlayers == null || interlayers.Length == 0)            
                throw new ArgumentException("No interlayer provided");
            

            // Validazione dati di input
            if (monolithicGlasses.Length - 1 != interlayers.Length)
            {
                throw new ArgumentException("MonolithicGlasses.Length - 1 != interlayers.Length");
            }

            _monolithicGlasses = monolithicGlasses;
            _interlayers = interlayers;
        }

        public LaminatedGlass(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _monolithicGlasses = (MonolithicGlass[])info.GetValue("MonolithicGlasses", typeof(MonolithicGlass[]));
            _interlayers = (Interlayer[])info.GetValue("Interlayers", typeof(Interlayer[]));
        }

        #endregion 

        #region PUBLIC METHODS
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("MonolithicGlasses", _monolithicGlasses);
            info.AddValue("Interlayers", _interlayers);
        }

        #endregion PUBLIC METHODS

    }
}
