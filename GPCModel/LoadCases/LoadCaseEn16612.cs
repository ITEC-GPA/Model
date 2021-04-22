using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    /// <summary>
    /// This class rapresent a <see cref="LoadCase"/> with the additional information required by the Standard EN 16612:2020
    /// </summary>
    public class LoadCaseEn16612 : LoadCase, ISerializable
    {
        #region PUBLIC ENUMS
        public enum LoadCaseEn16612Types
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

        private readonly LoadCaseEn16612Types _loadCaseEn16612Type;

        #region PUBLIC CONSTRUCTOR

        public LoadCaseEn16612(string name, LoadCaseTypes loadCaseType, LoadCaseEn16612Types loadCaseEn16612Type, Guid guid)
            : base(name, loadCaseType, guid)
        {
            this._loadCaseEn16612Type = loadCaseEn16612Type;
        }

        public LoadCaseEn16612(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCaseEn16612Type = (LoadCaseEn16612Types)info.GetValue("LoadCaseEn16612Type", typeof(LoadCaseEn16612Types));
        }

        #endregion

        public LoadCaseEn16612Types GetLoadCasePrEnType() => _loadCaseEn16612Type;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCaseEn16612Type", _loadCaseEn16612Type);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            LoadCaseEn16612 objCasted = obj as LoadCaseEn16612;
            return !(objCasted is null) && _loadCaseEn16612Type.Equals(objCasted._loadCaseEn16612Type) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _loadCaseEn16612Type.GetHashCode();
            return hashCode;
        }


        public static bool operator ==(LoadCaseEn16612 obj1, LoadCaseEn16612 obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(LoadCaseEn16612 obj1, LoadCaseEn16612 obj2)
        {
            return !(obj1 == obj2);
        }
    }
}