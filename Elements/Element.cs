using System;


namespace GPC.Model.Elements
{
    /// <summary>
    /// Element base abstract class that is the base for all the objects inside GPC environment.
    /// </summary>

    [Serializable]
    public abstract class Element : Object
    {
        private Guid _guid;

        /// <summary>
        /// </summary>
        /// <param name="guid">The guid id of the object</param>
        protected Element(Guid guid)
        {
            Guid = guid;
        }

        public Guid Guid { get => _guid; private set => _guid = value; }
    }
}
