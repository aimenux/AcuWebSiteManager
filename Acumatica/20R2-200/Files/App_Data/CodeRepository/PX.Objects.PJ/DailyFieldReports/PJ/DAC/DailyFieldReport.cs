using System;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.TM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [Serializable]
    [PXEMailSource]
    [PXPrimaryGraph(typeof(DailyFieldReportEntry))]
    [PXCacheName(CacheNames.DailyFieldReport)]
    public class DailyFieldReport : BaseCache, IBqlTable, IAssign, PX.CS.Contracts.Interfaces.IAddressLocation
    {
        [PXBool]
        public bool? Selected
        {
            get;
            set;
        }

        [PXDBIdentity]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDefault]
        [PXFieldDescription]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = "DFR ID", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [AutoNumber(typeof(ProjectManagementSetup.dailyFieldReportNumberingId), typeof(AccessInfo.businessDate))]
        [PXSelector(typeof(dailyFieldReportCd))]
        public virtual string DailyFieldReportCd
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField]
        public virtual bool? Hold
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? Approved
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? Rejected
        {
            get;
            set;
        }

        [PXDefault]
        [Project(typeof(Where<PMProject.nonProject.IsEqual<False>
                .And<PMProject.baseType.IsEqual<CTPRType.project>>
                .And<PMProject.status.IsEqual<ProjectStatus.active>>>),
            DisplayName = "Project", Visibility = PXUIVisibility.SelectorVisible)]
        [PXUIEnabled(typeof(hold.IsEqual<True>))]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "DFR Date", Visibility = PXUIVisibility.SelectorVisible)]
        [PXUIEnabled(typeof(hold.IsEqual<True>))]
        public virtual DateTime? Date
        {
            get;
            set;
        }

        [PXDBString(50, IsUnicode = true)]
        [DailyFieldReportStatus.List]
        [PXDefault(DailyFieldReportStatus.Hold)]
        [PXUIField(DisplayName = "Status", Required = true,
            Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string Status
        {
            get;
            set;
        }

       
        [PXDefault(typeof(SearchFor<PMProject.ownerID>
            .Where<PMProject.contractID.IsEqual<projectId.FromCurrent>>))]
        [PXFormula(typeof(Default<projectId>))]
        [Owner(DisplayName = "Project Manager")]
        [PXUIEnabled(typeof(Where<projectId.IsNotNull.And<hold.IsEqual<True>>>))]
        [PXForeignReference(typeof(Field<projectManagerId>.IsRelatedTo<PMProject.ownerID>))]
        public virtual int? ProjectManagerId
        {
            get;
            set;
        }

        /// <summary>
        /// i
        /// </summary>
        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By", Visibility = PXUIVisibility.SelectorVisible)]
        public override Guid? CreatedById
        {
            get;
            set;
        }

        [PXNote]
        [DailyFieldReportSearchable]
        public override Guid? NoteID
        {
            get;
            set;
        }

        [PXInt]
        [PXSelector(
            typeof(Search<EPCompanyTree.workGroupID>),
            SubstituteKey = typeof(EPCompanyTree.description))]
        public virtual int? WorkgroupID
        {
            get;
            set;
        }

        [Owner(IsDBField = false)]
        public virtual int? OwnerID
        {
            get;
            set;
        }

        [PXDBLocalizableString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Site Address")]
        public string SiteAddress
        {
            get;
            set;
        }

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "City")]
        public string City
        {
            get;
            set;
        }

        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "Country")]
        [Country]
        public string CountryID
        {
            get;
            set;
        }

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "State")]
        [State(typeof(countryId))]
        public string State
        {
            get;
            set;
        }

        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Postal Code")]
        [PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), typeof(countryId))]
        public string PostalCode
        {
            get;
            set;
        }

        [PXDBDecimal(6, MaxValue = 90f, MinValue = -90f)]
        [PXUIField(DisplayName = "Latitude")]
        public decimal? Latitude
        {
            get;
            set;
        }

        [PXDBDecimal(6, MaxValue = 180f, MinValue = -180f)]
        [PXUIField(DisplayName = "Longitude")]
        public decimal? Longitude
        {
            get;
            set;
        }

        [PXDecimal]
        [PXUIField(DisplayName = "Temperature", Enabled = false)]
        [TemperatureLevelConversion]
        [PXUIVisible(typeof(WeatherIntegrationSetup.isConfigurationEnabled.FromCurrent.IsEqual<True>))]
        public decimal? TemperatureLevel
        {
            get;
            set;
        }

        [PXDecimal(MinValue = 0)]
        [PXUIField(DisplayName = "Humidity(%)", Enabled = false)]
        [PXUIVisible(typeof(WeatherIntegrationSetup.isConfigurationEnabled.FromCurrent.IsEqual<True>))]
        public decimal? Humidity
        {
            get;
            set;
        }

        [PXString]
        public string Icon
        {
            get;
            set;
        }

        [PXDateAndTime(UseTimeZone = false)]
        [PXUIField(DisplayName = "Time Observed", Enabled = false)]
        [PXUIVisible(typeof(WeatherIntegrationSetup.isConfigurationEnabled.FromCurrent.IsEqual<True>))]
        public DateTime? TimeObserved
        {
            get;
            set;
        }
        public string AddressLine1 
        {
            get
            {
                return SiteAddress;
            }
            set
            {
                SiteAddress = value;
            }
        }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }

        public abstract class selected : BqlBool.Field<selected>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class dailyFieldReportCd : BqlString.Field<dailyFieldReportCd>
        {
        }

        public abstract class status : BqlString.Field<status>
        {
        }

        public abstract class hold : BqlBool.Field<hold>
        {
        }

        public abstract class approved : BqlBool.Field<approved>
        {
        }

        public abstract class rejected : BqlBool.Field<rejected>
        {
        }

        public abstract class date : BqlDateTime.Field<date>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class projectManagerId : BqlInt.Field<projectManagerId>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

        public abstract class workgroupID : BqlInt.Field<workgroupID>
        {
        }

        public abstract class ownerID : BqlInt.Field<ownerID>
        {
        }

        public abstract class siteAddress : BqlString.Field<siteAddress>
        {
        }

        public abstract class city : BqlString.Field<city>
        {
        }

        public abstract class countryId : BqlString.Field<countryId>
        {
        }

        public abstract class state : BqlString.Field<state>
        {
        }

        public abstract class postalCode : BqlString.Field<postalCode>
        {
        }

        public abstract class latitude : BqlDecimal.Field<latitude>
        {
        }

        public abstract class longitude : BqlDecimal.Field<longitude>
        {
        }

        public abstract class temperatureLevel : BqlDecimal.Field<temperatureLevel>
        {
        }

        public abstract class humidity : BqlDecimal.Field<humidity>
        {
        }

        public abstract class icon : BqlString.Field<icon>
        {
        }

        public abstract class timeObserved : BqlDateTime.Field<timeObserved>
        {
        }
    }
}