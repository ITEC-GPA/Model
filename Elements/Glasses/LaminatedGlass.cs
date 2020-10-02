using System;

namespace GPC.Model.Elements
{
    /// <summary>
    /// Laminated glass. This represent a multilayer glass panel. Between each layer there is an interlayer
    /// </summary>
    public class LaminatedGlass : GlassPanel
    {
        private readonly MonolithicGlass[] _monolithicGlasses;
        private readonly Interlayer[] _interlayers;

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
            // TODO: validare array
        }
    }
}
