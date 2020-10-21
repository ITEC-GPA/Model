using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    public class LoadCasePrEn : LoadCase
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
        public LoadCasePrEnType GetLoadCasePrEnType => _loadCasePrEnType;

        #region PUBLIC CONSTRUCTOR

        public LoadCasePrEn(string name, LoadCaseType loadCaseType, LoadCasePrEnType loadCasePrEnType, Guid guid)
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCasePrEnType", _loadCasePrEnType);
        }

    }
}