using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.UI;
using PX.Common.Extensions;
using PX.Common;

namespace PX.Objects.CR.WebUI
{
	public class OUSearchMaintWebUI : PXUIExtension<OUSearchMaint>
	{
		[PXUIField(DisplayName = "Back Button")]
		[PXUIActionContainer]
		public class HeaderActions : PXExposedView<Filter>
		{
			[PXUIControlButton]
			public abstract class back : PXExposedAction { }
		}
		[PXUIField(DisplayName = "Message")]
		[PXUIContainer(Id = "Form")]
		public class Filter : PXExposedView<OUSearchEntity>
		{
			[PXUIDropDownField]
			public abstract class outgoingEmail : OUSearchEntity.outgoingEmail { }
			[PXUIField(DisplayName = "Contact ID")]
			[PXUIControlField(SuppressLabel = true)]
			public abstract class contactID : OUSearchEntity.contactID { }
			[PXUIControlField(Readonly = true)]
			public abstract class email : OUSearchEntity.eMail { }
			[PXUIControlField]
			public abstract class ErrorMessage : OUSearchEntity.errorMessage { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class displayName : OUSearchEntity.displayName { }
			[PXUIControlField]
			public abstract class newContactFirstName : OUSearchEntity.newContactFirstName { }
			[PXUIControlField]
			public abstract class newContactLastName : OUSearchEntity.newContactLastName { }
			[PXUIControlField]
			public abstract class newContactEmail : OUSearchEntity.newContactEmail { }
			[PXUIControlField]
			public abstract class salutation : OUSearchEntity.salutation { }
			[PXUIControlField]
			public abstract class bAccountID : OUSearchEntity.bAccountID { }
			[PXUIControlField]
			public abstract class fullName : OUSearchEntity.fullName { }
			[PXUIDropDownField]
			public abstract class leadSource : OUSearchEntity.leadSource { }
			[PXUIDropDownField]
			public abstract class contactSource : OUSearchEntity.contactSource { }
			[PXUIControlField]
			public abstract class countryID : OUSearchEntity.countryID { }
			[PXUIControlFieldWithLabel(Label = typeof(entityName))]
			public abstract class entityID : OUSearchEntity.entityID { }
			[PXUIControlHiddenField]
			public abstract class entityName : OUSearchEntity.entityName { }
		}
		[PXUIField(DisplayName = "New Case")]
		[PXUIContainer(Id = "Case")]
		public class NewCase : PXExposedView<OUCase>
		{
			[PXUIControlField]
			public abstract class caseClassID : OUCase.caseClassID { }
			[PXUIControlField]
			public abstract class contractID : OUCase.contractID { }
			[PXUIDropDownField]
			public abstract class severity : OUCase.severity { }
			[PXUIControlField]
			public abstract class subject : OUCase.subject { }
		}
		[PXUIField(DisplayName = "New Opportunity")]
		[PXUIContainer(Id = "Opportunity")]
		public class NewOpportunity : PXExposedView<OUOpportunity>
		{
			[PXUIControlField]
			public abstract class classID : OUOpportunity.classID { }
			[PXUIControlField]
			public abstract class subject : OUOpportunity.subject { }
			[PXUIDropDownField]
			public abstract class stageID : OUOpportunity.stageID { }
			[PXUIDateField]
			public abstract class closeDate : OUOpportunity.closeDate { }
			[PXUIControlField]
			public abstract class manualAmount : OUOpportunity.manualAmount { }
			[PXUIControlField]
			public abstract class currencyID : OUOpportunity.currencyID { }
			[PXUIControlField]
			public abstract class branchID : OUOpportunity.branchID { }
		}
		[PXUIField(DisplayName = "Activity")]
		[PXUIContainer(Id = "Activity")]
		public class NewActivity : PXExposedView<OUActivity>
		{
			[PXUIControlField]
			public abstract class subject : OUActivity.subject { }
			[PXUIControlField]
			public abstract class caseCD : OUActivity.caseCD { }
			[PXUIControlField]
			public abstract class opportunityID : OUActivity.opportunityID { }
			[PXUIControlField]
			public abstract class isLinkContact : OUActivity.isLinkContact { }
			[PXUIControlField]
			public abstract class isLinkCase : OUActivity.isLinkCase { }
			[PXUIControlField]
			public abstract class isLinkOpportunity : OUActivity.isLinkOpportunity { }
		}
		[PXUIField(DisplayName = "Actions")]
		[PXUIActionContainer]
        public class Actions : PXExposedView<Filter>
        {
            [PXUIControlButton]
            public abstract class createAPDoc : PXExposedAction { }
            [PXUIControlButton]
            public abstract class createActivity : PXExposedAction { }
            [PXUIControlButton]
            public abstract class createOpportunity : PXExposedAction { }
            [PXUIControlButton]
            public abstract class createCase : PXExposedAction { }
            [PXUIControlButton]
            public abstract class createLead : PXExposedAction { }
            [PXUIControlButton]
            public abstract class createContact : PXExposedAction { }
            [PXUIControlButton]
            public abstract class goCreateLead : PXExposedAction { }
            [PXUIControlButton]
            public abstract class goCreateContact : PXExposedAction { }
            [PXUIControlButton]
            public abstract class viewContact : PXExposedAction { }
            [PXUIControlButton]
            public abstract class viewBAccount : PXExposedAction { }
            [PXUIControlButton]
            public abstract class viewEntity : PXExposedAction { }
            [PXUIControlButton]
            public abstract class goCreateActivity : PXExposedAction { }
            [PXUIControlButton]
            public abstract class goCreateCase : PXExposedAction { }
            [PXUIControlButton]
            public abstract class goCreateOpportunity : PXExposedAction { }
            [PXUIControlButton]
            public abstract class reply : PXExposedAction { }
            [PXUIControlButton]
            public abstract class logOut : PXExposedAction { }
        }
		[PXUIField(DisplayName = "Attachments")]
		[PXUIAttachmentsContainer(AttachmentsView = "APBillAttachments")]
		public class Attachments : PXExposedView<Filter>
		{
		}
		[PXUIField(DisplayName = "Outlook Data")]
		[PXUIOutlookData(Id = "Outlook-Data")]
		public class SourceMessage : PXExposedView<OUMessage, Filter>
		{
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class emailAddress : Filter.outgoingEmail { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class displayName : Filter.displayName { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class newFirstName : Filter.newContactFirstName { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class newLastName : Filter.newContactLastName { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class messageId : OUMessage.messageId { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class to : OUMessage.to { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class cC : OUMessage.cC { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class subject : OUMessage.subject { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class itemId : OUMessage.itemId { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class isIncome : OUMessage.isIncome { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class ewsUrl : OUMessage.ewsUrl { }
			[PXUIControlHiddenField(StoreValue = true)]
			public abstract class apiToken : OUMessage.token { }
		}
		public override void Initialize()
		{
			base.Initialize();
			AddFileFields();
		}
		private void AddFileFields()
		{
			var attachments = Base.APBillAttachments.Select()
				.AsEnumerable()
				.Select(a => (OUAPBillAttachment)a)
				.ToArray();

			for (var i = 0; i < attachments.Length; i++)
			{
				var itemIdCaptured = attachments[i].ItemId;
				var idCaptured = attachments[i].Id;
				var fieldName = string.Format("File{0}", i);
				if (!Base.Filter.Cache.Fields.Contains(fieldName))
				{
					Base.Filter.Cache.Fields.Add(fieldName);
					AddField<Attachments>(fieldName, nameof(Filter) + "." + fieldName);
					Base.FieldSelecting.AddHandler(Base.PrimaryView, fieldName, (s, e) =>
					{
						Base.OUAPBillAttachmentSelectFileFieldSelecting(s, e, itemIdCaptured, idCaptured);
					});
					Base.FieldUpdating.AddHandler(Base.PrimaryView, fieldName, (s, e) =>
					{
						Base.OUAPBillAttachmentSelectFileFieldUpdating(s, e, itemIdCaptured, idCaptured);
					});
				}
			}
		}
	}
}
