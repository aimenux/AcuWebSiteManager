using System;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.WeatherLists;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CS;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Weather")]
    public class DailyFieldReportWeather : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public int? DailyFieldReportWeatherId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId>>))]
        public int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDBString(IsUnicode = true)]
        public string Icon
        {
            get;
            set;
        }

        [PXDBDateAndTime(DisplayNameTime = "Time Observed", UseTimeZone = false)]
        public DateTime? TimeObserved
        {
            get;
            set;
        }

        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Cloudiness(%)")]
        public int? Cloudiness
        {
            get;
            set;
        }

        [PXDBString(50, IsUnicode = true)]
        [SkyState.List]
        [PXUIField(DisplayName = "Sky")]
        public string SkyState
        {
            get;
            set;
        }

        [PXDBDecimal]
        [PXUIField]
        [TemperatureLevelConversion]
        public decimal? TemperatureLevel
        {
            get;
            set;
        }

        [PXDecimal]
        [PXUIField(DisplayName = "Temperature", Visible = false, Visibility = PXUIVisibility.Invisible)]
        [TemperatureLevelConversion]
        public decimal? TemperatureLevelMobile
        {
            get => TemperatureLevel;
            set => TemperatureLevel = value;
        }

        [PXDBString(50, IsUnicode = true)]
        [Temperature.List]
        [PXUIField(DisplayName = "Temperature Perceived")]
        public string Temperature
        {
            get;
            set;
        }

        [PXDBDecimal(MinValue = 0)]
        [PXUIField(DisplayName = "Humidity(%)")]
        public decimal? Humidity
        {
            get;
            set;
        }

        [PXDBDecimal(MinValue = 0)]
        [PXDefault(typeof(decimal0))]
        [PXUIField]
        [PrecipitationAmountConversion]
        public decimal? PrecipitationAmount
        {
            get;
            set;
        }

        [PXDecimal]
        [PXUIField(DisplayName = "Rain/Snow", Visible = false, Visibility = PXUIVisibility.Invisible)]
        [PrecipitationAmountConversion]
        public decimal? PrecipitationAmountMobile
        {
            get => PrecipitationAmount;
            set => PrecipitationAmount = value;
        }

        [PXDBString(50, IsUnicode = true)]
        [Precipitation.List]
        [PXUIField(DisplayName = "Precipitation Description")]
        public string Precipitation
        {
            get;
            set;
        }

        [PXDBDecimal(MinValue = 0)]
        [PXUIField]
        [WindSpeedConversion]
        public decimal? WindSpeed
        {
            get;
            set;
        }

        [PXDecimal]
        [PXUIField(DisplayName = "Wind", Visible = false, Visibility = PXUIVisibility.Invisible)]
        [WindSpeedConversion]
        public decimal? WindSpeedMobile
        {
            get => WindSpeed;
            set => WindSpeed = value;
        }

        [PXDBString(50, IsUnicode = true)]
        [WindPower.List]
        [PXUIField(DisplayName = "Wind Description")]
        public string WindPower
        {
            get;
            set;
        }

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Site Conditions")]
        public string LocationCondition
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Delay")]
        public bool? IsObservationDelayed
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public string Description
        {
            get;
            set;
        }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "Last Modified By", Enabled = false)]
        public override Guid? LastModifiedById
        {
            get;
            set;
        }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modification Date", Enabled = false)]
        public override DateTime? LastModifiedDateTime
        {
            get;
            set;
        }

        [PXNote]
        public override Guid? NoteID
        {
            get;
            set;
        }

        public abstract class dailyFieldReportWeatherId : BqlInt.Field<dailyFieldReportWeatherId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class icon : BqlString.Field<icon>
        {
        }

        public abstract class timeObserved : BqlDateTime.Field<timeObserved>
        {
        }

        public abstract class cloudiness : BqlInt.Field<cloudiness>
        {
        }

        public abstract class skyState : BqlString.Field<skyState>
        {
        }

        public abstract class temperatureLevel : BqlDecimal.Field<temperatureLevel>
        {
        }

        public abstract class temperatureLevelMobile : BqlDecimal.Field<temperatureLevelMobile>
        {
        }

        public abstract class temperature : BqlString.Field<temperature>
        {
        }

        public abstract class humidity : BqlDecimal.Field<humidity>
        {
        }

        public abstract class precipitationAmount : BqlDecimal.Field<precipitationAmount>
        {
        }

        public abstract class precipitationAmountMobile : BqlDecimal.Field<precipitationAmountMobile>
        {
        }

        public abstract class precipitation : BqlString.Field<precipitation>
        {
        }

        public abstract class windSpeed : BqlDecimal.Field<windSpeed>
        {
        }

        public abstract class windSpeedMobile : BqlDecimal.Field<windSpeedMobile>
        {
        }

        public abstract class windPower : BqlString.Field<windPower>
        {
        }

        public abstract class locationCondition : BqlString.Field<locationCondition>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }

        public abstract class isObservationDelayed : BqlBool.Field<isObservationDelayed>
        {
        }
    }
}
