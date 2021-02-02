using System.Collections;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CN.Compliance.PM.DAC;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.AP.GraphExtensions
{
    public class VendorClassMaintLienWaiverExtension : PXGraphExtension<VendorClassMaint>
    {
        public PXSetup<LienWaiverSetup> LienWaiverSetup;

        public SelectFrom<LienWaiverRecipient>
            .RightJoin<PMProject>
            .On<LienWaiverRecipient.projectId.IsEqual<PMProject.contractID>>.View LienWaiverRecipientProjects;

        public PXAction<VendorClass> AddToProjects;

        [InjectDependency]
        public IProjectDataProvider ProjectDataProvider
        {
            get;
            set;
        }

        private string VendorClassId => Base.CurVendorClassRecord.Current.VendorClassID;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public IEnumerable lienWaiverRecipientProjects()
        {
            var projects = ProjectDataProvider.GetProjects(Base);
            return projects.Select(MaintainLienWaiverRecipientsLinks);
        }

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Add to Project")]
        public virtual void addToProjects()
        {
            if (LienWaiverRecipientProjects.AskExt((graph, viewName) => PrepareDialogWindow(), true).IsPositive())
            {
                LienWaiverRecipientProjects.SelectMain().ForEach(MaintainLienWaiverRecipientsLinks);
                Base.Actions.PressSave();
            }
        }

        public virtual void _(Events.RowSelected<VendorClass> args)
        {
            if (args.Row != null)
            {
                AddToProjects.SetVisible(LienWaiverSetup.Current.ShouldGenerateConditional.GetValueOrDefault() ||
                    LienWaiverSetup.Current.ShouldGenerateUnconditional.GetValueOrDefault());
            }
        }

        public virtual void VendorClass_RowPersisted(PXCache cache, PXRowPersistedEventArgs args)
        {
            var status = cache.GetStatus(args.Row);
            if (status == PXEntryStatus.Inserted && IsAddVendorClassConfirmed()
                && LienWaiverRecipientProjects.AskExt(true).IsPositive())
            {
                LienWaiverRecipientProjects.SelectMain().ForEach(MaintainLienWaiverRecipientsLinks);
            }
        }

        private void PrepareDialogWindow()
        {
            Base.RowPersisted.RemoveHandler<VendorClass>(VendorClass_RowPersisted);
            LienWaiverRecipientProjects.Cache.Clear();
            Base.Actions.PressSave();
        }

        private void MaintainLienWaiverRecipientsLinks(LienWaiverRecipient lienWaiverRecipient)
        {
            if (lienWaiverRecipient.Selected != true)
            {
                LienWaiverRecipientProjects.Delete(lienWaiverRecipient);
            }
        }

        private PXResult<LienWaiverRecipient, PMProject> MaintainLienWaiverRecipientsLinks(PMProject project)
        {
            var linkedRecipient = GetLinkedLienWaiverRecipient(project);
            if (linkedRecipient == null)
            {
                var lienWaiverRecipient = CreateLienWaiverRecipient(project);
                return new PXResult<LienWaiverRecipient, PMProject>(lienWaiverRecipient, project);
            }
            if (linkedRecipient.Selected == null)
            {
                linkedRecipient.Selected = true;
            }
            return new PXResult<LienWaiverRecipient, PMProject>(linkedRecipient, project);
        }

        private LienWaiverRecipient CreateLienWaiverRecipient(Contract project)
        {
            return new LienWaiverRecipient
            {
                ProjectId = project.ContractID,
                VendorClassId = VendorClassId
            };
        }

        private LienWaiverRecipient GetLinkedLienWaiverRecipient(Contract project)
        {
            return Base.Select<LienWaiverRecipient>().SingleOrDefault(
                recipient => recipient.ProjectId == project.ContractID && recipient.VendorClassId == VendorClassId);
        }

        private bool IsAddVendorClassConfirmed()
        {
            return Base.VendorClassRecord.Ask(ComplianceMessages.WouldYouLikeToAddVendorClassToExistingProjects,
                MessageButtons.YesNo).IsPositive();
        }
    }
}