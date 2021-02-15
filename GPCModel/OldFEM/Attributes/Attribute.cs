using GPC.Model.LoadCases;
using System;

namespace GPC.Model.FEM.Attributes
{
    public abstract class Attribute
    {
        private Guid _guid;
        private LoadCase _loadCase;

        public LoadCase LoadCase => _loadCase;


        public Attribute(LoadCase loadcase, Guid guid)
        {
            this._loadCase = loadcase ?? throw new ArgumentNullException("Loadcase cannot be null");
            _guid = guid;
        }


        public Attribute(LoadCase loadcase) : this(loadcase, Guid.NewGuid())
        {

        }
    }
}
