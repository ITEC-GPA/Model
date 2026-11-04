namespace GPC.Model.Persistence
{
    /// <summary>The form an archive is written in. Every reader reads every form it knows; the writer chooses.</summary>
    public enum ModelArchiveFormat
    {
        /// <summary>XML of the data contracts (archive versions 1-4) or the package of version 5 (XML document and compact results):
        /// the form of the archives of ANTHEA, read by every GPC.Model.Persistence.</summary>
        Xml = 0,
        /// <summary>The same data contracts in a compact binary form, deflated (archive version 6, value archive version 2, sidecar
        /// manifest version 2): 10-20 times smaller and faster than the XML. Read from GPC.Model.Persistence 3.14 on; for plug-ins and
        /// sidecars that opt in. A graph the binary form does not reproduce is written as XML.</summary>
        Compact = 1
    }
}
