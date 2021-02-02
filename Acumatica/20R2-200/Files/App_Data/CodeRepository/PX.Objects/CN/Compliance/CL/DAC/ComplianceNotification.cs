using PX.Data;
using PX.Objects.CA;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CS;
using PX.SM;
using Constants = PX.Objects.CN.Common.Descriptor.Constants;
using Messages = PX.Objects.CA.Messages;
using urlReports = PX.Objects.Common.urlReports;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [PXProjection(typeof(Select<NotificationSetup,
        Where<module, Equal<PXModule.cl>>>), Persistent = true)]
    [PXCacheName("Compliance Notification")]
    public class ComplianceNotification : NotificationSetup
    {
        [PXDBString(2, IsFixed = true, IsKey = true)]
        [PXDefault(PXModule.Cl)]
        public override string Module
        {
            get;
            set;
        }

        [PXDefault(Descriptor.Constants.ComplianceNotification.LienWaiverNotificationSourceCd)]
        [PXDBString(10, IsKey = true, InputMask = "")]
        public override string SourceCD
        {
            get;
            set;
        }

        [PXDBString(8, InputMask = "CC.CC.CC.CC")]
        [PXUIField(DisplayName = Constants.Report)]
        [PXSelector(typeof(Search<SiteMap.screenID,
                Where<SiteMap.screenID, Like<PXModule.cl_>,
                    And<SiteMap.url, Like<urlReports>>>,
                OrderBy<Asc<SiteMap.screenID>>>),
            typeof(SiteMap.screenID),
            typeof(SiteMap.title),
            Headers = new[]
            {
                Messages.ReportID,
                Messages.ReportName
            },
            DescriptionField = typeof(SiteMap.title))]
        public override string ReportID
        {
            get;
            set;
        }
    }
}