namespace GPC.Model.Core
{
    /// <summary>What a copy of <see cref="ModelValues"/> may share with its source instead of copying it. The default
    /// (<see cref="None"/>) is a complete, independent copy. Options of a call, not data: never archived (a class, not an enum, so that
    /// it stays out of the archive vocabulary).</summary>
    public sealed class ModelCopyOptions
    {
        private ModelCopyOptions(bool shareProperties) { SharesProperties = shareProperties; }

        /// <summary>Everything is copied: the copy and its source have no object in common but the immutable values (strings,
        /// numbers, compact results).</summary>
        public static ModelCopyOptions None { get; } = new ModelCopyOptions(false);

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
        public static ModelCopyOptions ShareProperties { get; } = new ModelCopyOptions(true);

        /// <summary>True for <see cref="ShareProperties"/>.</summary>
        public bool SharesProperties { get; }

        public override string ToString() => SharesProperties ? nameof(ShareProperties) : nameof(None);
    }
}
