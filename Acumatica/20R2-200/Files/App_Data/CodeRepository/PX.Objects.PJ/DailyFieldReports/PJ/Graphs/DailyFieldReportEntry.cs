using System.Collections.Generic;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Services;
using PX.Objects.PJ.DailyFieldReports.PM.CacheExtensions;
using PX.Objects.PJ.DailyFieldReports.PM.Services;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Api.Models;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Graphs
{
    public class DailyFieldReportEntry : PXGraph<DailyFieldReportEntry, DailyFieldReport>
    {
        public PXSetup<ProjectManagementSetup> ProjectManagementSetup;

        [PXViewName("Daily Field Report")]
        [PXCopyPasteHiddenFields(typeof(DailyFieldReport.icon))]
        public SelectFrom<DailyFieldReport>.View DailyFieldReport;

        public PXAction<DailyFieldReport> Print;
        public PXAction<DailyFieldReport> ViewAddressOnMap;

        public PXSetup<ProjectManagementSetup> PJSetup;

		[InjectDependency]
        public IProjectDataProvider ProjectDataProvider
        {
            get;
            set;
        }

        public DailyFieldReportEntry()
        {
	        var pjsetup = PJSetup.Current;
        }

		[PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print/Email", MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        public virtual void print()
        {
            Persist();
            var parameters = new Dictionary<string, string>
            {
                [DailyFieldReportConstants.Print.DailyFieldReportId] = DailyFieldReport.Current.DailyFieldReportCd
            };
            throw new PXReportRequiredException(parameters, ScreenIds.DailyFieldReportForm, null);
        }

        public override void CopyPasteGetScript(bool isImportSimple, List<Command> script, List<Container> containers)
        {
            var dailyFieldReportCopyConfigurationService = new DailyFieldReportCopyConfigurationService(this);
            dailyFieldReportCopyConfigurationService.ConfigureCopyPasteFields(script, containers);
        }

        public virtual void _(Events.RowPersisting<DailyFieldReport> args)
        {
            var dailyFieldReport = args.Row;
            var project = ProjectDataProvider.GetProject(this, dailyFieldReport.ProjectId);
            if (dailyFieldReport.Date < project?.StartDate)
            {
                args.Cache.RaiseException<DailyFieldReport.date>(dailyFieldReport,
                    DailyFieldReportMessages.DfrDateMustNotBeEarlierThenProjectStartDate);
            }
        }

        public virtual void _(Events.RowInserting<DailyFieldReport> args)
        {
            PXContext.SetScreenID(ScreenIds.DailyFieldReport);
        }

        public virtual void _(Events.FieldUpdated<DailyFieldReport, DailyFieldReport.projectId> args)
        {
            var dailyFieldReport = args.Row;
            if (dailyFieldReport.ProjectId != null)
            {
                var project = ProjectDataProvider.GetProject(this, dailyFieldReport.ProjectId);
                var projectExt = PXCache<PMProject>.GetExtension<PMProjectExt>(project);
                PMSiteAddress address = PXSelect<PMSiteAddress, Where<PMSiteAddress.addressID, Equal<Required<PMProjectExt.siteAddressID>>>>.Select(this, projectExt.SiteAddressID);
                dailyFieldReport.SiteAddress = address.AddressLine1;
                dailyFieldReport.City = address.City;
                dailyFieldReport.CountryID = address.CountryID;
                dailyFieldReport.State = address.State;
                dailyFieldReport.PostalCode = address.PostalCode;
                dailyFieldReport.Latitude = address.Latitude;
                dailyFieldReport.Longitude = address.Longitude;
            }
        }

        public virtual void _(Events.FieldUpdated<DailyFieldReport.countryId> args)
        {
            DailyFieldReport.Current.State = null;
        }

        [PXUIField(DisplayName = PX.Objects.CR.Messages.ViewOnMap)]
        [PXButton]
        public virtual void viewAddressOnMap()
        {
            var dailyFieldReport = DailyFieldReport.Current;
            new MapService(this).viewAddressOnMap(dailyFieldReport, dailyFieldReport.SiteAddress);
        }
    }
}