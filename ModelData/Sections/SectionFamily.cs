namespace GPC.Model.Data.Sections
{
    /// <summary>
    /// The geometric family of a commercial section: it decides the section of Model that represents it (see <see cref="SectionMappings"/>).
    /// The commercial series (IPE, HE A, UPN, W...) are data of the catalogs, not families
    /// </summary>
    public enum SectionFamily
    {
        /// <summary>
        /// I and H sections with parallel flanges and root fillets: IPE, HE, HL, HD, HP, UB, UC (EN 10365); W, M, HP (ASTM A6)
        /// </summary>
        ParallelFlangeIH,
        /// <summary>
        /// I sections with taper flanges, root and toe radii: IPN, J (EN 10365); S (ASTM A6)
        /// </summary>
        TaperFlangeI,
        /// <summary>
        /// Channels with parallel flanges, root and toe radii: UPE, PFC (EN 10365)
        /// </summary>
        ParallelFlangeChannel,
        /// <summary>
        /// Channels with taper flanges, root and toe radii: UPN (EN 10365); C, MC (ASTM A6)
        /// </summary>
        TaperFlangeChannel,
        /// <summary>
        /// Equal and unequal leg angles with root and toe radii: L (EN 10056-1, ASTM A6)
        /// </summary>
        Angle,
        /// <summary>
        /// Tees: T (EN 10055), WT, MT, ST (ASTM A6)
        /// </summary>
        Tee,
        /// <summary>
        /// Circular hollow sections: CHS (EN 10210-2, EN 10219-2), Pipe and round HSS (AISC)
        /// </summary>
        CircularHollow,
        /// <summary>
        /// Square and rectangular hollow sections with rounded corners: SHS, RHS (EN 10210-2, EN 10219-2), HSS (AISC)
        /// </summary>
        RectangularHollow,
        /// <summary>
        /// Two angles back to back: 2L (AISC)
        /// </summary>
        DoubleAngle,
    }

    /// <summary>
    /// How faithfully the section of Model represents the commercial section (the measured deviations are documented by the tests of the
    /// catalogs)
    /// </summary>
    public enum MappingFidelity
    {
        /// <summary>
        /// The geometry of the standard is represented (radii included): area, moments of inertia and moduli within the rounding of the
        /// published values
        /// </summary>
        Exact,
        /// <summary>
        /// A detail of the geometry is simplified (e.g. the slope of the flanges or the radii): the deviations are documented in
        /// <see cref="SectionMapping.Notes"/>
        /// </summary>
        Approximated,
        /// <summary>
        /// Model has no section for the family yet: <see cref="SectionMapping.Create"/> throws
        /// </summary>
        NotSupported,
    }
}
