using System;
using PX.Data;
using System.Collections.Generic;

namespace PX.Objects.CR
{
	#region CRContactBatch
	[Serializable]
	public class CRContactBatch : Contact, IPXSelectable
	{
		#region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected", Filterable = false)]
		[CRMergeableAttribute(false)]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region DefAddressID
		public new abstract class defAddressID : PX.Data.IBqlField
		{
		}
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.IBqlField
		{
		}
		#endregion
		#region ContactType
		public new abstract class contactType : PX.Data.IBqlField
		{
		}
		#endregion
		#region ContactID
		public new abstract class contactID : PX.Data.IBqlField
		{
		}
		#endregion
	}
	#endregion

	[PXGraphName(Messages.CRMergeContactsMaint, typeof(Contact))]
	public class CRMergeContactsMaint :
		CRMergeMaint<CRMergeContactsMaint, CRContactBatch>
	{
		[PXFilterable(typeof(SelectedContact), AdditionalFilters = new Type[] { typeof(CRAttributeFilter) })]
		[CRMerge]
	    public PXSelectJoin<CRContactBatch,
            InnerJoin<BAccount, On<CRContactBatch.bAccountID, Equal<BAccount.bAccountID>>,
            LeftJoin<Address, On<Address.addressID, Equal<CRContactBatch.defAddressID>>>>,
            Where<CRContactBatch.contactType, NotEqual<ContactTypes.bAccountProperty>>> Items;

		public PXSelect<CROpportunityContacts> opprotunityContacts;
	    public PXSelect<SelectedContact> Contact;

	    #region Constructors
		public CRMergeContactsMaint()
			: base()
		{
		}
		public CRMergeContactsMaint(PXGraph graph)
			: base(graph)
		{
		}
		protected override void Initialize()
		{
			base.Initialize();

			PXUIFieldAttribute.SetEnabled<CRContactBatch.salutation>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRContactBatch.displayName>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRContactBatch.eMail>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRContactBatch.phone1>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRContactBatch.isActive>(Items.Cache, null, false);

			PXUIFieldAttribute.SetEnabled<BAccount.acctCD>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<BAccount.acctName>(Items.Cache, null, false);
		}
		#endregion

		protected override void PrePersistHandler(CRContactBatch newInstance, ICollection<CRContactBatch> deletingInstances)
		{
			if (deletingInstances.Count > 0)
			{
				// CROpportunityContacts
				RefItemsCorrector<CRContactBatch, CROpportunityContacts, CROpportunityContacts.contactID> opportunityContactsHelper =
					new RefItemsCorrector<CRContactBatch, CROpportunityContacts, CROpportunityContacts.contactID>(this, GetContactID);
				opportunityContactsHelper.Correct(newInstance, deletingInstances);

				// Commit changes
				CRHelper.SafetyPersist(opprotunityContacts.Cache, opportunityContactsHelper.SaveOperations);
			}
			base.PrePersistHandler(newInstance, deletingInstances);
		}

		private static object GetContactID(CRContactBatch instance)
		{
			return instance.ContactID;
		}

		protected override PXGraph GetGraphForDetails(CRContactBatch selectedItem)
		{
			ContactMaint target = CreateGraph<ContactMaint>();
			target.Contact.Current = target.Contact.Search<Contact.contactID>(selectedItem.ContactID);
			return target;
        }

        protected override PXSelectBase<CRContactBatch> MargeItems
        {
            get { return Items; }
        }

		protected override bool IsBatchsEqual(CRContactBatch x, CRContactBatch y)
		{
			return x.ContactID == y.ContactID;
		}
	}
}
