using System;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.PJ.RequestsForInformation.PJ.DAC
{
    [Serializable]
    [PXCacheName(CacheNames.RequestForInformationRelation)]
    public class RequestForInformationRelation : IBqlTable
    {
        [PXDBIdentity(IsKey = true, DatabaseFieldName = "RelationId")]
        [PXUIField(Visible = false)]
        public virtual int? RequestForInformationRelationId
        {
            get;
            set;
        }

        [PXParent(typeof(Select<Contact, Where<Contact.noteID, Equal<Current<requestForInformationNoteId>>>>))]
        [PXParent(typeof(Select<BAccount, Where<BAccount.noteID, Equal<Current<requestForInformationNoteId>>>>))]
        [PXDBGuid(DatabaseFieldName = "RefNoteId")]
        [PXDBDefault(null)]
        public virtual Guid? RequestForInformationNoteId
        {
            get;
            set;
        }

        [PXDBString(2)]
        [PXUIField(DisplayName = "Role")]
        [PXDefault]
        [RequestForInformationRoleList]
        public virtual string Role
        {
            get;
            set;
        }

        [PXDBBool]
        [PXUIField(DisplayName = "Primary")]
        [PXDefault(false)]
        public virtual bool? IsPrimary
        {
            get;
            set;
        }

        [PXDBString(40, DatabaseFieldName = "TargetType")]
        [PXUIField(DisplayName = "Type")]
        [RequestForInformationRelationType]
        [PXFormula(typeof(Default<role>))]
        public virtual string Type
        {
            get;
            set;
        }

        [PXDBGuid(DatabaseFieldName = "TargetNoteId")]
        [PXUIField(DisplayName = "Document")]
        [RequestForInformationRelationDocumentSelector]
        [PXFormula(typeof(Default<type>))]
        [PXUIEnabled(typeof(Where<role, Equal<RequestForInformationRoleListAttribute.relatedEntity>>))]
        public virtual Guid? DocumentNoteId
        {
            get;
            set;
        }

        [PXDBInt(DatabaseFieldName = "EntityId")]
        [PXUIField(DisplayName = "Account")]
        [PXFormula(typeof(Default<documentNoteId>))]
        [RequestForInformationRelationAccountSelector]
        public virtual int? BusinessAccountId
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Account/Employee", Enabled = false)]
        public virtual string BusinessAccountCd
        {
            get;
            set;
        }

        [PXDBInt(DatabaseFieldName = "ContactId")]
        [PXUIField(DisplayName = "Contact")]
        [PXFormula(typeof(Default<documentNoteId>))]
        [PXFormula(typeof(Default<businessAccountId>))]
        [RequestForInformationRelationContactSelector]
        [PXUIEnabled(typeof(Where<type, NotEqual<RequestForInformationRelationTypeAttribute.apInvoice>,
            Or<type, NotEqual<RequestForInformationRelationTypeAttribute.arInvoice>,
            Or<type, NotEqual<RequestForInformationRelationTypeAttribute.requestForInformation>>>>))]
        public virtual int? ContactId
        {
            get;
            set;
        }

        [PXDBBool]
        [PXUIField(DisplayName = "Add to CC")]
        public virtual bool? AddToCc
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Name", Enabled = false)]
        public virtual string BusinessAccountName
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Contact", Enabled = false)]
        public virtual string ContactName
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Email", Enabled = false)]
        public virtual string ContactEmail
        {
            get;
            set;
        }

        public abstract class contactEmail : IBqlField
        {
        }

        public abstract class contactName : IBqlField
        {
        }

        public abstract class businessAccountName : IBqlField
        {
        }

        public abstract class businessAccountId : IBqlField
        {
        }

        public abstract class addToCc : IBqlField
        {
        }

        public abstract class businessAccountCd : IBqlField
        {
        }

        public abstract class contactId : IBqlField
        {
        }

        public abstract class type : IBqlField
        {
        }

        public abstract class documentNoteId : IBqlField
        {
        }

        public abstract class isPrimary : IBqlField
        {
        }

        public abstract class requestForInformationNoteId : IBqlField
        {
        }

        public abstract class role : IBqlField
        {
        }

        public abstract class requestForInformationRelationId : IBqlField
        {
        }
    }
}