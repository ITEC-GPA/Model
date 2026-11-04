namespace GPC.Model.Core
{
    /// <summary>What a copy of <see cref="ModelValues"/> may share with its source instead of copying it. The default
    /// (<see cref="None"/>) is a complete, independent copy. Options of a call, not data: never archived (a class, not an enum, so that
    /// it stays out of the archive vocabulary). Options combine with <c>|</c>, e.g.
    /// <c>ModelCopyOptions.ShareProperties | ModelCopyOptions.ShareCoordinateSystems</c>.</summary>
    public sealed class ModelCopyOptions
    {
        private ModelCopyOptions(bool shareProperties, bool shareCoordinateSystems)
        { SharesProperties = shareProperties; SharesCoordinateSystems = shareCoordinateSystems; }

        /// <summary>Everything is copied: the copy and its source have no object in common but the immutable values (strings,
        /// numbers, compact results).</summary>
        public static ModelCopyOptions None { get; } = new ModelCopyOptions(false, false);

        /// <summary>
        /// The element properties (<see cref="ElementProperties.ElementProperty"/>: beam, plate and brick properties), the materials
        /// (<see cref="Materials.Material"/>) and the sections (<see cref="Sections.ISectionShape"/>) that the copied value cites are
        /// the instances of the source, as the elements of FeMM shared their property; everything else is copied as usual (elements,
        /// nodes, coordinate systems, assignments, loads, cases, groups, attributes). The value itself is always copied, also when it
        /// is a property: then its own material and section are shared.
        /// <para>The graph is the one a complete copy would have, with the same aliases: a property cited by the registry of a model
        /// and by its elements is one instance in the copy too, so the elements of a copied model cite the properties registered in
        /// the copied model. Fingerprints and archives are those of a complete copy.</para>
        /// <para>Sharing is safe only while nobody changes a shared instance: whoever wants a different property, material or
        /// section creates a new one (copy on modify, e.g. <see cref="ModelValues.CopyValue{T}(T)"/> of the property, with a new
        /// identity when the new content does not replace the old one everywhere) and assigns it. A renamed shared property would be
        /// renamed in every model that registers it.</para>
        /// <para>A graph that the copy in memory does not reproduce is copied through its XML, completely: sharing is a permission,
        /// never a guarantee.</para>
        /// </summary>
        public static ModelCopyOptions ShareProperties { get; } = new ModelCopyOptions(true, false);

        /// <summary>
        /// The coordinate systems (<see cref="GPC.Geometry.CoordinateSystem"/>) that the copied value cites are the instances of the
        /// source: those of the elements, of the restraints, of the loads and of the result values (user decision of 4/11/2026, lane L9).
        /// The value itself is always copied, also when it is a coordinate system. A coordinate system owns its origin and axes (its
        /// constructors and <see cref="GPC.Geometry.CoordinateSystem.SetOrigin"/> copy the points), so sharing it leaves the aliases of the
        /// graph as a complete copy has them; fingerprints and archives are those of a complete copy.
        /// <para>A separate option from <see cref="ShareProperties"/>: a property is a definition that many elements cite, a coordinate
        /// system is state of its element with mutators in its API (<c>Rotate*</c>, <c>Move</c>, <c>SetOrigin</c>, and its origin
        /// and axes, mutable points and vectors). It is safe only while nobody changes a shared coordinate system in place: a caller
        /// opts in after checking that its code assigns new coordinate systems (copy on modify) instead of changing them.</para>
        /// </summary>
        public static ModelCopyOptions ShareCoordinateSystems { get; } = new ModelCopyOptions(false, true);

        /// <summary>True when the properties, materials and sections are shared (<see cref="ShareProperties"/>).</summary>
        public bool SharesProperties { get; }

        /// <summary>True when the coordinate systems are shared (<see cref="ShareCoordinateSystems"/>).</summary>
        public bool SharesCoordinateSystems { get; }

        /// <summary>What either option shares (null: nothing).</summary>
        public static ModelCopyOptions operator |(ModelCopyOptions left, ModelCopyOptions right)
            => new ModelCopyOptions(left?.SharesProperties == true || right?.SharesProperties == true,
                left?.SharesCoordinateSystems == true || right?.SharesCoordinateSystems == true);

        public override string ToString()
            => SharesProperties && SharesCoordinateSystems ? nameof(ShareProperties) + " | " + nameof(ShareCoordinateSystems)
                : SharesProperties ? nameof(ShareProperties) : SharesCoordinateSystems ? nameof(ShareCoordinateSystems) : nameof(None);
    }
}
