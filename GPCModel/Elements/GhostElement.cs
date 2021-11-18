using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// The purpose of this element is to give an instance to the abstract class Element.
    /// This can be usefull for example for debug purposes 
    /// </summary>
    internal class GhostElement : Element, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public GhostElement()
        {
        }

        public GhostElement(string name) 
            : base(name)
        {
        }

        public GhostElement(int id) 
            : base(id)
        {
        }

        public GhostElement(Guid guid) 
            : base(guid)
        {
        }

        public GhostElement(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        public GhostElement(int id, string name) 
            : base(id, name)
        {
        }

        public GhostElement(int id, string name, Guid guid) 
            : base(id, name, guid)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is GhostElement element && base.Equals(element);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
