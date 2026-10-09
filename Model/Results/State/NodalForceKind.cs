using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restraints;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;

namespace GPC.Model.Results.State
{
    public enum NodalForceKind { Unknown, SupportReaction, SpringForce, LinkForce, ElementEndForce, ElementNodeForce }
}
