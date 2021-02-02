using System;
using PX.Data;
using System.Collections.Generic;

namespace PX.Objects.CR
{
	#region CRBAccountBatch
	[Serializable]
	public class CRBAccountBatch : BAccount, IPXSelectable
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
		#region BAccountID
		public new abstract class bAccountID : PX.Data.IBqlField
		{
		}
		[PXDBIdentity()]
		[CRMergeableAttribute(false)]
		public override Int32? BAccountID
		{
			get
			{
				return base.BAccountID;
			}
			set
			{
				base.BAccountID = value;
			}
		}
		#endregion
		#region AcctCD
		public new abstract class acctCD : PX.Data.IBqlField
		{
		}
		//[PXDBString(10, IsKey = true)]
		//public override String AcctCD
		//{
		//    get
		//    {
		//        return base.AcctCD;
		//    }
		//    set
		//    {
		//        base.AcctCD = value;
		//    }
		//}
		#endregion

		#region BaseContactID
		public new abstract class baseContactID : PX.Data.IBqlField { }
		[BaseContactID]
		[PXSelector(typeof(Search<Contact.contactID, Where<Contact.contactType, Equal<ContactTypes.person>>>),
				DescriptionField = typeof(Contact.displayName), DirtyRead = true)]
		public override Int32? BaseContactID
		{
			get
			{
				return base.BaseContactID;
			}
			set
			{
				base.BaseContactID = value;
			}
		}
		#endregion
		#region DefContactID
		public new abstract class defContactID : PX.Data.IBqlField
		{
		}
		#endregion
		#region DefAddressID
		public new abstract class defAddressID : PX.Data.IBqlField
		{
		}
		[DefAddressID]
		[PXSelector(typeof(Address.addressID), DescriptionField = typeof(Address.displayName), DirtyRead = true)]
		public override Int32? DefAddressID
		{
			get
			{
				return base.DefAddressID;
			}
			set
			{
				base.DefAddressID = value;
			}
		}
		#endregion
		#region DefLocationID
		public new abstract class defLocationID : PX.Data.IBqlField { }
		[DefLocationID]
		[PXSelector(typeof(Location.locationID), DescriptionField = typeof(Location.locationCD), DirtyRead = true)]
		public override Int32? DefLocationID
		{
			get
			{
				return base.DefLocationID;
			}
			set
			{
				base.DefLocationID = value;
			}
		}
		#endregion
		#region Type
		public new abstract class type : PX.Data.IBqlField
		{
		}
		#endregion
		#region CreatedByID
		public new abstract class createdByID : PX.Data.IBqlField
		{
		}
		[PXDBCreatedByID()]
		public override Guid? CreatedByID
		{
			get
			{
				return base.CreatedByID;
			}
			set
			{
				base.CreatedByID = value;
			}
		}
		#endregion
	}
	#endregion

	#region SelectedBAccountR
    [PXSubstitute(GraphType = typeof(CRMergeBAccountsMaint))]
    [PXSubstitute(GraphType = typeof(BusinessAccountMaint))]
	public class SelectedBAccountR : BAccountR
	{
        public static SelectedBAccountR FromBAccountR(BAccountR bAccountR)
        {
            SelectedBAccountR result = new SelectedBAccountR();
            result.BAccountID = bAccountR.BAccountID;
            result.AcctCD = bAccountR.AcctCD;
            result.AcctName = bAccountR.AcctName;
            result.Type = bAccountR.Type;
            result.AcctReferenceNbr = bAccountR.AcctReferenceNbr;
            result.ParentBAccountID = bAccountR.ParentBAccountID;
            result.Status = bAccountR.Status;
            result.DefAddressID = bAccountR.DefAddressID;
            result.DefContactID = bAccountR.DefContactID;
            result.BaseContactID = bAccountR.BaseContactID;
            result.DefLocationID = bAccountR.DefLocationID;
            result.TaxZoneID = bAccountR.TaxZoneID;
            result.TaxRegistrationID = bAccountR.TaxRegistrationID;
            result.WorkgroupID = bAccountR.WorkgroupID;
            result.OwnerID = bAccountR.OwnerID;
            return result;
        }
	}
	#endregion

	[PXGraphName(Messages.CRMergeBAccountsMaint, typeof(BAccount))]
    public class CRMergeBAccountsMaint :
		CRMergeMaint<CRMergeBAccountsMaint, CRBAccountBatch>
	{
		[PXFilterable(typeof(SelectedBAccountR), typeof(SelectedContact), typeof(SelectedAddress), AdditionalFilters = new Type[] { typeof(CRAttributeFilter) })]
		[CRMerge]
        public PXSelectJoin<CRBAccountBatch,
            LeftJoin<Contact, On<Contact.bAccountID, Equal<CRBAccountBatch.bAccountID>,
                And<Contact.contactID, Equal<CRBAccountBatch.defContactID>>>,
            LeftJoin<Address, On<Address.bAccountID, Equal<CRBAccountBatch.bAccountID>,
                And<Address.addressID, Equal<CRBAccountBatch.defAddressID>>>>>,
            Where<CRBAccountBatch.type, Equal<BAccountType.prospectType>>> Items;

		public PXSelect<Location> locations;
		public PXSelect<Address> addresses;
		public PXSelect<Contact> contact;
		public PXSelect<CROpportunity> opportunity;
		public PXSelect<SelectedAddress> DefAddress;
		public PXSelect<SelectedContact> DefContact;
		public PXSelect<SelectedBAccountR> BAccount;

	    #region Constructors
		public CRMergeBAccountsMaint()
			: base()
		{
		}
		public CRMergeBAccountsMaint(PXGraph graph)
			: base(graph)
		{
		}
		protected override void Initialize()
		{
			base.Initialize();

			PXUIFieldAttribute.SetEnabled<CRBAccountBatch.acctName>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRBAccountBatch.acctReferenceNbr>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRBAccountBatch.status>(Items.Cache, null, false);

			PXUIFieldAttribute.SetEnabled<Address.city>(Items.Cache, null, false);

			PXUIFieldAttribute.SetEnabled<Contact.salutation>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Contact.displayName>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Contact.eMail>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Contact.phone1>(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Contact.isActive>(Items.Cache, null, false);
		}
		#endregion

		protected override PXGraph GetGraphForDetails(CRBAccountBatch selectedItem)
		{
			BusinessAccountMaint target = CreateGraph<BusinessAccountMaint>();
			target.BAccount.Current = target.BAccount.Search<BAccount.acctCD>(selectedItem.AcctCD);
			return target;
		}

		protected override void PrePersistHandler(CRBAccountBatch newInstance, ICollection<CRBAccountBatch> deletingInstances)
		{
			if (deletingInstances.Count > 0)
			{
				// Location
				RefItemsCorrectorBase<CRBAccountBatch, Location> locationHelper =
					new RefItemsCorrector<CRBAccountBatch, Location, Location.bAccountID>(this, GetBAccountID);
				locationHelper.Correct(newInstance, deletingInstances);
				// Address
				RefItemsCorrectorBase<CRBAccountBatch, Address> addressHelper =
					new RefItemsCorrector<CRBAccountBatch, Address, Address.bAccountID>(this, GetBAccountID);
				addressHelper.Correct(newInstance, deletingInstances);
				// Contact
				RefItemsCorrectorBase<CRBAccountBatch, Contact> contactHelper =
					new RefItemsCorrector<CRBAccountBatch, Contact, Contact.bAccountID>(this, GetBAccountID);
				contactHelper.Correct(newInstance, deletingInstances);
				// Opportunity
				RefItemsCorrectorBase<CRBAccountBatch, CROpportunity> opportunityHelper =
					new RefItemsCorrector<CRBAccountBatch, CROpportunity, CROpportunity.bAccountID>(this, GetBAccountID);
				opportunityHelper.Correct(newInstance, deletingInstances);

				// Commit changes
				CRHelper.SafetyPersist(locations.Cache, locationHelper.SaveOperations);
				locations.Cache.IsDirty = false;
				CRHelper.SafetyPersist(addresses.Cache, addressHelper.SaveOperations);
				addresses.Cache.IsDirty = false;
				CRHelper.SafetyPersist(contact.Cache, contactHelper.SaveOperations);
				addresses.Cache.IsDirty = false;
				CRHelper.SafetyPersist(opportunity.Cache, opportunityHelper.SaveOperations);
				addresses.Cache.IsDirty = false;
			}
			base.PrePersistHandler(newInstance, deletingInstances);
		}

		private static object GetBAccountID(CRBAccountBatch instance)
		{
			return instance.BAccountID;
        }

        protected override PXSelectBase<CRBAccountBatch> MargeItems
        {
            get { return Items; }
        }

		protected override bool IsBatchsEqual(CRBAccountBatch x, CRBAccountBatch y)
		{
			return x.AcctCD == y.AcctCD;
		}
	}
}
