using System;
using System.Runtime.Serialization;

namespace GPC.Model.Attributes
{
    /// <summary>
    /// An identifier given by the user to an element, to select it (e.g. the Element Id of Arachne). Distinct from the element
    /// <see cref="ModelObjectId.Name"/>, which an application may assign itself (a numbering): only labelled elements answer to a
    /// search by label. The label is not empty.
    /// </summary>
    [Serializable]
    public sealed class ElementLabelAttribute : Attribute
    {
        public string Label { get; private set; }

        public ElementLabelAttribute(string label)
        {
            if (string.IsNullOrEmpty(label)) throw new ArgumentException("EmptyElementLabel", nameof(label));
            Label = label;
        }

        private ElementLabelAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SerializationVersions.Read(info, "ElementLabelAttributeVersion", 1);
            Label = info.GetString("Label");
            if (string.IsNullOrEmpty(Label)) throw new SerializationException("EmptyElementLabel");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ElementLabelAttributeVersion", 1);
            info.AddValue("Label", Label);
        }
    }
}
