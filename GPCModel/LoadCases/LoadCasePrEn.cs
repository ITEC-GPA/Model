using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    public class LoadCasePrEn : LoadCase, ISerializable
    {
        #region PUBLIC ENUMS
        public enum LoadCasePrEnType
        {
            [Description("Wind gust load mediterranean")] WindGustLoadMediterranean,
            [Description("Wind gust load other")] WindGustLoadOther,
            [Description("Wind storm load mediterranean")] WindStormLoadMediterranean,
            [Description("Wind storm load other")] WindStormLoadOther,
            [Description("Balaustrade duty")] BalustradeDuty,
            [Description("Balaustrade crowds")] BalustradeCrowds,
            [Description("Maintenance")] Maintenance,
            [Description("Snow canopies")] SnowCanopies,
            [Description("Snow roofs")] SnowRoofs,
            [Description("Permanent")] Permanent
            //[Description("Climatic summer")] ClimaticSummer,
            //[Description("Climatic winter")] ClimaticWinter,
        }
        #endregion

        private LoadCasePrEnType _loadCasePrEnType;

        #region PUBLIC CONSTRUCTOR

        public LoadCasePrEn(string name, LoadCaseTypes loadCaseType, LoadCasePrEnType loadCasePrEnType, Guid guid)
            : base(name, loadCaseType, guid)
        {
            this._loadCasePrEnType = loadCasePrEnType;
        }

        public LoadCasePrEn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCasePrEnType = (LoadCasePrEnType)info.GetValue("LoadCasePrEnType", typeof(LoadCasePrEnType));
        }

        #endregion

        public LoadCasePrEnType GetLoadCasePrEnType() => _loadCasePrEnType;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCasePrEnType", _loadCasePrEnType);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            LoadCasePrEn objCasted = obj as LoadCasePrEn;
            return !(objCasted is null) && _loadCasePrEnType.Equals(objCasted._loadCasePrEnType) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _loadCasePrEnType.GetHashCode();
            return hashCode;
        }


        public static bool operator ==(LoadCasePrEn obj1, LoadCasePrEn obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(LoadCasePrEn obj1, LoadCasePrEn obj2)
        {
            return !(obj1 == obj2);
        }
    }
}