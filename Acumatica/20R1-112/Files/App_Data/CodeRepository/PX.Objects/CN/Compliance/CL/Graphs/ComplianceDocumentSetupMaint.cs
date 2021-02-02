using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Helpers;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using Constants = PX.Objects.CN.Common.Descriptor.Constants;
using PoMessages = PX.Objects.PO.Messages;

namespace PX.Objects.CN.Compliance.CL.Graphs
{
    public class ComplianceDocumentSetupMaint : PXGraph<ComplianceDocumentSetupMaint>
    {
        public PXFilter<ComplianceAttributeFilter> Filter;

        public PXSelect<ComplianceAttribute,
            Where<ComplianceAttribute.type, Equal<Current<ComplianceAttributeFilter.type>>>> Mapping;

        public PXSelect<LienWaiverSetup> LienWaiverSetup;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> MappingCommon;

        public CRNotificationSetupList<ComplianceNotification> ComplianceNotifications;

        public PXSelect<NotificationSetupRecipient,
            Where<NotificationSetupRecipient.setupID, Equal<Current<NotificationSetup.setupID>>>> Recipients;

        public PXSelect<ComplianceAttributeType> AttributeType;
        public PXSave<LienWaiverSetup> Save;
        public PXCancel<LienWaiverSetup> Cancel;

        public ComplianceDocumentSetupMaint()
        {
            FeaturesSetHelper.CheckConstructionFeature();
        }

        [PXDBString(10)]
        [PXDefault]
        [VendorContactType.ClassList]
        [PXUIField(DisplayName = "Contact Type")]
        [PXCheckUnique(typeof(NotificationSetupRecipient.contactID), Where =
            typeof(Where<NotificationSetupRecipient.setupID.IsEqual<NotificationSetupRecipient.setupID.FromCurrent>>))]
        protected virtual void NotificationSetupRecipient_ContactType_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Contact ID")]
        [PXNotificationContactSelector(typeof(NotificationSetupRecipient.contactType),
            typeof(SearchFor<Contact.contactID>
                .In<SelectFrom<Contact>
                    .LeftJoin<EPEmployee>.On<EPEmployee.parentBAccountID.IsEqual<Contact.bAccountID>
                        .And<EPEmployee.defContactID.IsEqual<Contact.contactID>>>
                    .Where<NotificationSetupRecipient.contactType.FromCurrent.IsEqual<NotificationContactType.employee>
                        .And<EPEmployee.acctCD.IsNotNull>>>))]
        protected virtual void NotificationSetupRecipient_ContactID_CacheAttached(PXCache sender)
        {
        }

        protected virtual void _(Events.RowSelected<ComplianceAttributeFilter> args)
        {
            var filter = args.Row;
            if (filter != null)
            {
                var isLienWaiverAttribute = filter.Type == GetLienWaiverAttributeTypeId();
                Mapping.Enable(!isLienWaiverAttribute);
            }
        }

        protected virtual void _(Events.RowPersisting<CSAttributeGroup> args)
        {
            if (args.Operation == PXDBOperation.Insert && args.Row != null && !DoesAttributeExist(args.Row.AttributeID))
            {
                args.Cancel = true;
                MappingCommon.Cache.Clear();
            }
        }

        protected virtual void _(Events.RowInserting<ComplianceAttribute> args)
        {
            var attribute = args.Row;
            if (attribute != null && Filter.Current != null)
            {
                attribute.Type = Filter.Current.Type;
            }
        }

        protected virtual void _(Events.RowInserting<CSAttributeGroup> args)
        {
            args.Row.EntityClassID = Constants.ComplianceAttributeClassId;
            args.Row.EntityType = typeof(ComplianceDocument).FullName;
        }

        protected virtual void _(Events.RowDeleting<ComplianceAttribute> args)
        {
            var attributeId = args.Row.AttributeId;
            var compliance = new PXSelect<ComplianceDocument>(this);
            var attributeType = new PXSelect<ComplianceAttributeType,
                    Where<ComplianceAttributeType.complianceAttributeTypeID,
                        Equal<Required<ComplianceAttributeType.complianceAttributeTypeID>>>>(this)
                .SelectSingle(args.Row.Type);
            switch (attributeType.Type.Trim())
            {
                case ComplianceDocumentType.Status:
                    compliance
                        .WhereAnd<Where<ComplianceDocument.status, Equal<Required<ComplianceAttribute.attributeId>>>>();
                    break;
                default:
                    compliance
                        .WhereAnd<Where<ComplianceDocument.documentTypeValue,
                            Equal<Required<ComplianceAttribute.attributeId>>>>();
                    break;
            }
            if (compliance.SelectSingle(attributeId) != null)
            {
                throw new PXException(ComplianceMessages.CannotDeleteAttributeMessage);
            }
        }

        protected virtual void _(Events.RowDeleting<CSAttributeGroup> args)
        {
            var confirmationResult = ShowConfirmationDialog();
            if (confirmationResult == WebDialogResult.Cancel)
            {
                args.Cancel = true;
            }
            else
            {
                DeleteAttributeFromCompliance(args.Row.AttributeID);
            }
        }

        private WebDialogResult ShowConfirmationDialog()
        {
            return MappingCommon.Ask(PoMessages.Warning,
                ComplianceMessages.DeleteComplianceAttributeConfirmationDialogBody, MessageButtons.OKCancel,
                MessageIcon.Question);
        }

        private void DeleteAttributeFromCompliance(string attributeId)
        {
            var attributes = GetAttributeGroups(attributeId);
            foreach (var attribute in attributes)
            {
                this.Caches<CSAttributeGroup>().Delete(attribute);
            }
        }

        private IEnumerable<CSAttributeGroup> GetAttributeGroups(string attributeId)
        {
            return new PXSelect<CSAttributeGroup,
                    Where<CSAttributeGroup.attributeID, Equal<Required<CSAttributeGroup.attributeID>>,
                        And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>>(this)
                .Select(attributeId).FirstTableItems.ToList();
        }

        private bool DoesAttributeExist(string attributeId)
        {
            return new PXSelect<CSAttribute,
                    Where<CSAttribute.attributeID, Equal<Required<CSAttribute.attributeID>>>>(this)
                .SelectSingle(attributeId) != null;
        }

        private int? GetLienWaiverAttributeTypeId()
        {
            return Select<ComplianceAttributeType>()
                .Single(type => type.Type == ComplianceDocumentType.LienWaiver)
                .ComplianceAttributeTypeID;
        }
    }
}