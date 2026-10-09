using System;
using System.Collections.Generic;

namespace GPC.Model.Persistence
{
    /// <summary>Archive-facing view of the closed domain vocabulary.</summary>
    public static class ArchiveContractRegistry
    {
        public static IReadOnlyCollection<Type> SupportedTypes => Core.ModelDataContracts.SupportedTypes;
    }
}
