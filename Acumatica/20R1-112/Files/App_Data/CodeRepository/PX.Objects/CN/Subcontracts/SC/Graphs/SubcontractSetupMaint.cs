using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Common.Helpers;
using PX.Objects.CN.Subcontracts.PO.CacheExtensions;
using PX.Objects.CN.Subcontracts.SC.DAC;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PO;
using CRMessages = PX.Objects.CR.Messages;
using Messages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.SC.Graphs
{
    public class SubcontractSetupMaint : POSetupMaint
    {
        public PXSelectJoin<CSAnswers,
            RightJoin<POOrder, On<CSAnswers.refNoteID, Equal<POOrder.noteID>>>> Answers;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<PoOrderExt.pOOrderExtTypeName>,
                And<CSAttributeGroup.entityClassID, Equal<PoOrderExt.subcontractClass>>>> Attributes;

        public new CRNotificationSetupList<SubcontractNotification> Notifications;

        public new PXSelect<NotificationSetupRecipient,
            Where<NotificationSetupRecipient.setupID, Equal<Current<SubcontractNotification.setupID>>>> Recipients;

        public PXSetup<POSetup> PurchaseOrderSetup;

        public SubcontractSetupMaint()
        {
            FeaturesSetHelper.CheckConstructionFeature();
            var dummy = PurchaseOrderSetup.Current;
        }

        public IEnumerable setup()
        {
            return PXSelect<POSetup>.Select(this).FirstTableItems.Select(UpdateSubcontractSetupStatusIfRequired);
        }

        public override void Persist()
        {
            if (SetupApproval.Current != null
                && SetupApproval.Current.AssignmentMapID == null
                && SetupApproval.Current.AssignmentNotificationID == null)
            {
                SetupApproval.Delete(SetupApproval.Current);
            }
            base.Persist();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(typeof(Search<PoSetupExt.subcontractRequestApproval>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void _(Events.CacheAttached<POSetupApproval.isActive> e) { }

        public virtual void _(Events.RowInserting<POSetupApproval> args)
        {
            args.Row.OrderType = POOrderType.RegularSubcontract;
        }

        public virtual void _(Events.RowInserting<CSAttributeGroup> args)
        {
            args.Row.EntityClassID = Messages.SubcontractClassId;
            args.Row.EntityType = typeof(PoOrderExt).FullName;
        }

        public virtual void _(Events.RowDeleting<CSAttributeGroup> args)
        {
            var attributeGroup = args.Row;
            if (attributeGroup == null)
            {
                return;
            }
            if (attributeGroup.IsActive == true)
            {
                throw new PXSetPropertyException(CRMessages.AttributeCannotDeleteActive);
            }
            if (IsDeleteConfirmed())
            {
                DeleteAnswers(attributeGroup.AttributeID);
            }
            else
            {
                args.Cancel = true;
            }
        }

        private void DeleteAnswers(string attributeId)
        {
            foreach (var answer in Answers.SelectMain().Where(a => a.AttributeID == attributeId))
            {
                Answers.Cache.Delete(answer);
            }
        }

        private bool IsDeleteConfirmed()
        {
            return Attributes.Ask(Messages.AttributeDeleteWarningHeader,
                CRMessages.AttributeDeleteWarning, MessageButtons.OKCancel) == WebDialogResult.OK;
        }

        private POSetup UpdateSubcontractSetupStatusIfRequired(POSetup setup)
        {
	        PoSetupExt poSetupExt = Setup.Cache.GetExtension<PoSetupExt>(setup);
            if (!poSetupExt.IsSubcontractSetupSaved.GetValueOrDefault())
            {
                poSetupExt.IsSubcontractSetupSaved = true;

                Setup.Cache.SetDefaultExt<PoSetupExt.requireSubcontractControlTotal>(setup);
                Setup.Cache.SetDefaultExt<PoSetupExt.subcontractNumberingID>(setup);
                Setup.Cache.SetDefaultExt<PoSetupExt.subcontractRequestApproval>(setup);

                Setup.Cache.Update(setup);
            }
            return setup;
        }
    }
}