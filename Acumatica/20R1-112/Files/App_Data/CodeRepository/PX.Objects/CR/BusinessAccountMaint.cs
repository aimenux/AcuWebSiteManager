using System;
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.SO;
using PX.SM;
using PX.Objects.CR.DAC;
using PX.Objects.CA;
using PX.Objects.CR.MassProcess;
using PX.Data.MassProcess;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CR.Extensions.CRCreateActions;
using System.Linq;
using System.Collections.Generic;

namespace PX.Objects.CR
{
	public class BusinessAccountMaint : PXGraph<BusinessAccountMaint, BAccount>
	{
		#region Selects

		[PXHidden]
		public PXSelect<BAccount>
			BaseBAccounts;

        [PXHidden]
        public PXSelect<BAccountCRM>
            BaseBAccountsCRM;

		[PXHidden]
		public PXSetup<GL.Branch>
			Branches;

		[PXHidden]
		[PXCheckCurrent]
		public CM.CMSetupSelect 
			CMSetup;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<GL.Company>
			cmpany;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<CRSetup> 
			Setup;

		[PXHidden]
		public PXSelect<Location>
			BaseLocations;

		[PXViewName(Messages.BAccount)]
		public PXSelectJoin<BAccount, 
			LeftJoin<Contact, 
				On<Contact.contactID, Equal<BAccount.defContactID>>>,
			Where2<Match<Current<AccessInfo.userName>>, 
			And<Where<BAccount.type, Equal<BAccountType.customerType>,
				Or<BAccount.type, Equal<BAccountType.prospectType>,
				Or<BAccount.type, Equal<BAccountType.combinedType>,
				Or<BAccount.type, Equal<BAccountType.vendorType>>>>>>>>
			BAccount;

		[PXHidden]
		public PXSelectJoin<BAccount, 
			LeftJoin<Contact, 
				On<Contact.contactID, Equal<BAccount.defContactID>>>,
			Where<BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>>>
			CurrentBAccount;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<BAccount.noteID>>>>
			BAccountActivityStatistics;

		[PXViewName(Messages.Address)]
		public PXSelect<Address, 
			Where<Address.bAccountID, Equal<Current<BAccount.bAccountID>>, And<Address.addressID, Equal<Current<BAccount.defAddressID>>>>>
			AddressCurrent;

        [PXViewName(Messages.MainContact)]
		[PXCopyPasteHiddenFields(typeof(Contact.duplicateStatus), typeof(Contact.duplicateFound))]
		public PXSelect<Contact,
			Where<Contact.contactID, Equal<Current<BAccount.defContactID>>>>
			DefContact;

		[PXViewName(Messages.Contacts)]
		[PXFilterable]
		[PXViewSavedDetailsButton(typeof(BAccount))]
		public PXSelectJoin<Contact, 
			LeftJoin<Address,
				On<Address.addressID, Equal<Contact.defAddressID>>>,
			Where<
				Contact.bAccountID, Equal<Current<BAccount.bAccountID>>,
				And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>
			Contacts;


		[PXViewName(Messages.Leads)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact))]
		public PXSelectJoin<
				CRLead,
			InnerJoin<Address,
				On<Address.addressID, Equal<Contact.defAddressID>>,
			LeftJoin<CRActivityStatistics,
				On<CRActivityStatistics.noteID, Equal<CRLead.noteID>>>>,
			Where<
				CRLead.bAccountID, Equal<Current<BAccount.bAccountID>>>,
			OrderBy<
				Desc<CRLead.createdDateTime>>>
			Leads;

		[PXViewName(Messages.Locations)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		public PXSelectJoin<Location, 
			LeftJoin<Address, On<Address.addressID, Equal<Location.defAddressID>>>, 
			Where<Location.bAccountID, Equal<Current<BAccount.bAccountID>>>> 
			Locations;

		[PXViewName(Messages.DeliverySettings)]
		public PXSelect<Location, 
			Where<Location.bAccountID, Equal<Current<BAccount.bAccountID>>, And<Location.locationID, Equal<Current<BAccount.defLocationID>>>>> 
			DefLocation;

		[PXHidden]
		public PXSelect<Location, 
			Where<Location.bAccountID, Equal<Current<BAccount.bAccountID>>, And<Location.locationID, Equal<Current<BAccount.defLocationID>>>>>
			DefLocationCurrent;

		[PXViewName(Messages.DeliveryContact)]
		public ContactSelect2<Search<Location.defContactID, Where<Location.bAccountID, Equal<Current<BAccount.bAccountID>>, And<Location.locationID, Equal<Current<BAccount.defLocationID>>>>>, 
			Location.isContactSameAsMain, Location.bAccountID> 
			DefLocationContact;

		[PXViewName(Messages.DeliveryAddress)]
		public BusinessAccountLocationAddressSelect
			DefLocationAddress;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<BAccount>
			Answers;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
        [CRReference(typeof(BAccount.bAccountID), Persistent = true)]
		public CRActivityList<BAccount> 
			Activities;
		
        [PXHidden]
		public PXFilter<ActivityContactFilter> 
			ActivityContacts;

		[PXHidden]
		public PXSelect<CROpportunityClass>
			CROpportunityClass;

		[PXViewName(Messages.Opportunities)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<BAccount,
				Where<BAccount.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CROpportunity.contactID>>>>))]
		public PXSelectJoin<CROpportunity,			
			LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>, 
			LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
			LeftJoin<CROpportunityClass, On<CROpportunityClass.cROpportunityClassID, Equal<CROpportunity.classID>>>>>>,
			Where<BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
					Or<BAccount.parentBAccountID, Equal<Current<BAccount.bAccountID>>>>> 
			Opportunities;

		[PXHidden]
		public PXSelect<CROpportunity> OpportunityLink;
		
		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Relations)]
		[PXFilterable]
		public CRRelationsList<BAccount.noteID>
			Relations;

		[PXViewName(Messages.Cases)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CRCase.contactID>>>>))]
		public PXSelectReadonly2<CRCase,
			LeftJoin<Contact, On<Contact.contactID, Equal<CRCase.contactID>>>, 
			Where<CRCase.customerID, Equal<Current<BAccount.bAccountID>>>> 
			Cases;

		[PXViewName(Messages.Contracts)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Location,
				Where<Location.bAccountID, Equal<Current<Contract.customerID>>, And<Location.locationID, Equal<Current<Contract.locationID>>>>>))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<BAccount,
				Where<BAccount.bAccountID, Equal<Current<Contract.customerID>>>>))]
		public PXSelectReadonly2<Contract,
			LeftJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>, 
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contract.customerID>>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>, 
			  And<Where<BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
							 Or<ContractBillingSchedule.accountID, Equal<Current<BAccount.bAccountID>>>>>>>
			Contracts;

		[PXViewName(Messages.Orders)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		public PXSelectReadonly<SOOrder,
			Where<SOOrder.customerID, Equal<Current<BAccount.bAccountID>>>>
			Orders;

        [PXCopyPasteHiddenView]
		[PXViewName(Messages.CampaignMember)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<CRCampaign,
				Where<CRCampaign.campaignID, Equal<Current<CRCampaignMembers.campaignID>>>>))]
		public PXSelectJoin<CRCampaignMembers,
			InnerJoin<CRCampaign, On<CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>>, 
			InnerJoin<Contact, On<Contact.contactID, Equal<CRCampaignMembers.contactID>>>>,
			Where<Contact.bAccountID, Equal<Current<BAccount.bAccountID>>>>
			Members;

		[PXHidden]
		public PXSelect<CRMarketingListMember>
			Subscriptions_stub;

		[PXViewName(Messages.Subscriptions)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<CRMarketingList,
				Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingListMember.marketingListID>>>>))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CRMarketingListMember.contactID>>>>), ActionName = "Subscriptions_Contact_ViewDetails")]
		public CRMMarketingBAccountSubscriptions
			Subscriptions;

        public CRNotificationSourceList<BAccount, BAccount.classID, CRNotificationSource.bAccount> NotificationSources;

        public CRNotificationRecipientList<BAccount, BAccount.classID> NotificationRecipients;

        #endregion

        #region Ctors
        public BusinessAccountMaint()
		{
			if (Branches.Current.BAccountID.HasValue == false) //TODO: need review
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(GL.Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));

			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(BAccount.Cache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(BAccount.Cache, Messages.BAccountName);

            PXUIFieldAttribute.SetDisplayName<Carrier.description>(Caches[typeof(Carrier)], Messages.CarrierDescription);

			Activities.GetNewEmailAddress =
				() =>
				{
					var contact = (Contact)PXSelect<Contact,
						Where<Contact.contactID, Equal<Current<BAccount.defContactID>>>>.
						Select(this);

					return contact != null && !string.IsNullOrWhiteSpace(contact.EMail)
						? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName)
						: String.Empty;
				};

			DefLocationContact.DoNotCorrectUI = true;
			DefLocationAddress.DoNotCorrectUI = true;

			PXUIFieldAttribute.SetRequired<BAccount.status>(BAccount.Cache, true);

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(Contacts.Cache, null);

			Action.AddMenuAction(ChangeID);

			Locations.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.accountLocations>();
            PXUIFieldAttribute.SetVisible<CRMarketingListMember.format>(Subscriptions.Cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.languageID>(DefContact.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);
		}

		#endregion

		#region Actions

		public PXAction<BAccount> viewCustomer;
		[PXUIField(DisplayName = Messages.ViewCustomer, Visible = false)]
		[PXButton]
		public virtual void ViewCustomer()
		{
			BAccount bacct = BAccount.Current;
			if (bacct == null || bacct.BAccountID == null) return;

			if (bacct.Type != BAccountType.CustomerType &&
				bacct.Type != BAccountType.CombinedType)
			{
				return;
			}

			if (this.IsDirty)
				Save.Press();

			var graph = PXGraph.CreateInstance<AR.CustomerMaint>();
			graph.BAccount.Current = graph.BAccount.Search<AR.Customer.acctCD>(bacct.AcctCD);
			throw new PXRedirectRequiredException(graph, "View Customer");
		}

		public PXAction<BAccount> viewVendor;
		[PXUIField(DisplayName = Messages.ViewVendor, Enabled = false, Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void ViewVendor()
		{
			BAccount bacct = BAccount.Current;
			if (bacct == null || bacct.BAccountID == null) return;

			if (bacct.Type != BAccountType.VendorType &&
				bacct.Type != BAccountType.CombinedType)
			{
				return;
			}

			if (this.IsDirty)
				Save.Press();

			var graph = PXGraph.CreateInstance<AP.VendorMaint>();
			graph.BAccount.Current = graph.BAccount.Search<AP.Vendor.acctCD>(bacct.AcctCD);
			throw new PXRedirectRequiredException(graph, "View Vendor");
		}

		public PXMenuAction<BAccount> Action;

		public PXAction<BAccount> converToCustomer;
		[PXUIField(DisplayName = Messages.ConvertToCustomer, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ConverToCustomer(PXAdapter adapter)
		{
			foreach (var pxresult in adapter.Get())
			{
			    BAccount bacct = PXResult.Unwrap<BAccount>(pxresult);

			    if (bacct != null && (bacct.Type == BAccountType.ProspectType || bacct.Type == BAccountType.VendorType))
				{
					if (this.IsDirty)
						Save.Press();

					AR.CustomerMaint editingBO = PXGraph.CreateInstance<PX.Objects.AR.CustomerMaint>();
					Contact defContact = this.DefContact.SelectSingle();
					AR.Customer customer = (AR.Customer)editingBO.BAccount.Cache.Extend<BAccount>(bacct);
					editingBO.BAccount.Current = customer;
					customer.NoteID = bacct.NoteID;
					customer.LocaleName = defContact?.LanguageID;
					customer.Type = (bacct.Type == BAccountType.ProspectType) ? BAccountType.CustomerType : BAccountType.CombinedType;
					LocationExtAddress defLocation = editingBO.DefLocation.Select();
					editingBO.DefLocation.Cache.RaiseRowSelected(defLocation);
					string locationType = (bacct.Type == BAccountType.ProspectType) ? LocTypeList.CustomerLoc : LocTypeList.CombinedLoc;
					editingBO.InitCustomerLocation(defLocation, locationType, false);
					defLocation = editingBO.DefLocation.Update(defLocation);
					foreach (LocationExtAddress iLoc in editingBO.Locations.Select())
					{
						if (iLoc.LocationID != defLocation.LocationID)
						{
							editingBO.InitCustomerLocation(iLoc, locationType, false);
							editingBO.Locations.Update(iLoc);
						}
					}
					editingBO.Caches[typeof(CSAnswers)].Clear();
					editingBO.Caches[typeof(CSAnswers)].ClearQueryCache();

					if (!this.IsContractBasedAPI)
						throw new PXRedirectRequiredException(editingBO, "Edit Customer");

					editingBO.Save.Press();
				}
			}
			return adapter.Get();
		}

		public PXAction<BAccount> converToVendor;
		[PXUIField(DisplayName = Messages.ConvertToVendor, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ConverToVendor(PXAdapter adapter)
		{
			foreach (PXResult<BAccount, Contact> pxresult in adapter.Get())
			{
				var bacct = (BAccount) pxresult;
				if (bacct != null && (bacct.Type == BAccountType.ProspectType || bacct.Type == BAccountType.CustomerType))
				{
					if (this.IsDirty)
						Save.Press();

					AP.VendorMaint editingBO = PXGraph.CreateInstance<AP.VendorMaint>();
					Contact defContact = this.DefContact.SelectSingle();
					AP.VendorR vendor = (AP.VendorR)editingBO.BAccount.Cache.Extend<BAccount>(bacct);
					editingBO.BAccount.Current = vendor;
					vendor.NoteID = bacct.NoteID;
					vendor.Type = (bacct.Type == BAccountType.ProspectType) ? BAccountType.VendorType : BAccountType.CombinedType;
					vendor.LocaleName = defContact?.LanguageID;
					LocationExtAddress defLocation = editingBO.DefLocation.Select();
					editingBO.DefLocation.Cache.RaiseRowSelected(defLocation);
					string locationType = (bacct.Type == BAccountType.ProspectType) ? LocTypeList.VendorLoc : LocTypeList.CombinedLoc;
					defLocation.VTaxZoneID = defLocation.CTaxZoneID;
					editingBO.InitVendorLocation(defLocation, locationType, false);
					defLocation = editingBO.DefLocation.Update(defLocation);
					foreach (LocationExtAddress iLoc in editingBO.Locations.Select())
					{
						if (iLoc.LocationID != defLocation.LocationID)
						{
							editingBO.InitVendorLocation(iLoc, locationType, false);
							editingBO.Locations.Update(iLoc);
						}
					}
					editingBO.Caches[typeof(CSAnswers)].Clear();
					editingBO.Caches[typeof(CSAnswers)].ClearQueryCache();

					if (!this.IsContractBasedAPI)
						throw new PXRedirectRequiredException(editingBO, Messages.EditVendor);

					editingBO.Save.Press();
				}
			}
			return adapter.Get();
		}

		public PXDBAction<BAccount> addOpportunity;
		[PXUIField(DisplayName = Messages.AddNewOpportunity)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		public virtual void AddOpportunity()
		{
			var row = CurrentBAccount.Current;
			if (row == null || row.BAccountID == null) return;

			var graph = PXGraph.CreateInstance<OpportunityMaint>();
            var newOpportunity = graph.Opportunity.Insert();
			newOpportunity.BAccountID = row.BAccountID;
			newOpportunity.LocationID = row.DefLocationID;

			CROpportunityClass ocls = PXSelect<CROpportunityClass, Where<CROpportunityClass.cROpportunityClassID, Equal<Current<CROpportunity.classID>>>>
				.SelectSingleBound(this, new object[] { newOpportunity });
			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				newOpportunity.WorkgroupID = row.WorkgroupID;
				newOpportunity.OwnerID = row.OwnerID;
			}

			//TODO: need calculate default contact
            newOpportunity = graph.Opportunity.Update(newOpportunity);
            graph.Opportunity.SetValueExt<CROpportunity.bAccountID>(newOpportunity, row.BAccountID);
            graph.Answers.CopyAllAttributes(newOpportunity, row);
			
			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXDBAction<BAccount> addCase;
		[PXUIField(DisplayName = Messages.AddNewCase)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		public virtual void AddCase()
		{
			var row = CurrentBAccount.Current;
			if (row == null || row.BAccountID == null) return;

            var graph = PXGraph.CreateInstance<CRCaseMaint>();

            var newCase = (CRCase)graph.Case.Cache.Insert();
            newCase.CustomerID = row.BAccountID;
            newCase.LocationID = row.DefLocationID;

            //TODO: need calculate default contact
            newCase = graph.Case.Update(newCase);
            graph.Answers.CopyAllAttributes(newCase, row);
			
			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXDBAction<BAccount> addContact;
		[PXUIField(DisplayName = Messages.AddContact)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual void AddContact()
		{
			var row = BAccount.Current;
			if (row == null || row.BAccountID == null) return;

			var graph = PXGraph.CreateInstance<ContactMaint>();
			var newContact = (Contact)graph.Contact.Cache.CreateInstance();
			newContact.BAccountID = row.BAccountID;
			newContact.ContactType = ContactTypesAttribute.Person;
			newContact.DefAddressID = row.DefAddressID;
            newContact = graph.Contact.Insert(newContact);

			CRContactClass ocls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>
				.SelectSingleBound(this, new object[] { newContact });
			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				newContact.WorkgroupID = row.WorkgroupID;
				newContact.OwnerID = row.OwnerID;
			}

			newContact = graph.Contact.Update(newContact);

            graph.Answers.CopyAllAttributes(newContact, row);
		    graph.Contact.Cache.IsDirty = false;
		    graph.Answers.Cache.IsDirty = false;
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXDBAction<BAccount> addLocation;
		[PXUIField(DisplayName = Messages.AddNewLocation)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual void AddLocation()
		{
			var row = BAccount.Current;
			if (row == null || row.BAccountID == null) return;

            LocationMaint graph = null;
		    switch (row.Type)
		    {
                case BAccountType.VendorType:
		            graph = PXGraph.CreateInstance<AP.VendorLocationMaint>();
		            break;
                case BAccountType.CustomerType:
                    graph = PXGraph.CreateInstance<AR.CustomerLocationMaint>();
                    break;
                default:
                    graph = PXGraph.CreateInstance<LocationMaint>();
		            break;
		    }

			Location baccLocation = DefLocation.SelectSingle();
			var newLocation = (Location)graph.Location.Cache.CreateInstance();
			newLocation.BAccountID = row.BAccountID;
			string locType;
			switch (row.Type)
			{
				case BAccountType.VendorType:
					locType = LocTypeList.VendorLoc;
					break;
				case BAccountType.CustomerType:
				case BAccountType.EmpCombinedType:
					locType = LocTypeList.CustomerLoc;
					break;
				case BAccountType.CombinedType:
					locType = LocTypeList.CombinedLoc;
					break;
				default:
					locType = baccLocation.LocType;
					break;
			}
			newLocation.LocType = locType;
			graph.Location.Insert(newLocation);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			
		}

		public PXAction<BAccount> setDefaultLocation;
		[PXUIField(DisplayName = Messages.SetDefault)]
		[PXButton]
		public virtual void SetDefaultLocation()
		{
			var account = BAccount.Current;
			if (account == null || account.BAccountID == null) return;

			var row = Locations.Current;
			if (row == null || row.LocationID == null) return;

			if (row.IsActive != true)
				throw new Exception(Messages.DefaultLocationCanNotBeNotActive);

			var currentDefault = 
				(Location)PXSelect<Location,
					Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.
					Select(this, account.BAccountID, account.DefLocationID);
			if (currentDefault != null && Locations.Cache.GetStatus(currentDefault) == PXEntryStatus.Inserted)
				Locations.Cache.Delete(currentDefault);

			account.DefLocationID = row.LocationID;
			BAccount.Cache.Update(account);
		}

		public PXAction<BAccount> viewMainOnMap;
		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void ViewMainOnMap()
		{
			var row = BAccount.Current;
			if (row == null || row.BAccountID == null || row.DefAddressID == null) return;

			var address = (Address)PXSelect<Address,
				Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(this, row.DefAddressID);
			if (address != null) BAccountUtility.ViewOnMap(address);
		}

		public PXAction<BAccount> viewDefLocationOnMap;
		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void ViewDefLocationOnMap()
		{
			var row = BAccount.Current;
			if (row == null || row.BAccountID == null || row.DefLocationID == null) return;

			var location = (Location)PXSelect<Location,
				Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.
				Select(this, row.BAccountID, row.DefLocationID);
			if (location == null || location.DefAddressID == null) return;

			var address = (Address)PXSelect<Address,
				Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(this, location.DefAddressID);
			if (address != null) BAccountUtility.ViewOnMap(address);
		}

		public PXAction<BAccount> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual void ValidateAddresses()
		{
			var row = BAccount.Current;
			if (row == null) return;

			var address = row.DefAddressID.With(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
					Select(this, _.Value));
			if (address != null && address.IsValidated != true)
				PXAddressValidator.Validate<Address>(this, address, true);

			var location = (Location)PXSelect<Location,
				Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.
				Select(this, row.BAccountID, row.DefLocationID);
			if (location == null || location.DefAddressID == null) return;

			var locationAddress = (Address)PXSelect<Address,
				Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(this, location.DefAddressID);
			if (locationAddress != null && locationAddress.IsValidated != true)
				PXAddressValidator.Validate<Address>(this, locationAddress, true);
		}
		
		public PXChangeID<BAccount, BAccount.acctCD> ChangeID;
        #endregion

        #region Cache Attached
        #region NotificationSource
        [PXSelector(typeof(Search<NotificationSetup.setupID,
            Where<NotificationSetup.sourceCD, Equal<CRNotificationSource.bAccount>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
        [PXUIField(DisplayName = "Mailing ID")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
        {
        }
        [PXDBString(10, IsUnicode = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
        {
        }
        [PXCheckUnique(typeof(NotificationSource.setupID), IgnoreNulls = false,
            Where = typeof(Where<NotificationSource.refNoteID, Equal<Current<NotificationSource.refNoteID>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationSource_NBranchID_CacheAttached(PXCache sender)
        {

        }
        [PXUIField(DisplayName = "Report")]
        [PXSelector(typeof(Search<SiteMap.screenID,
            Where<SiteMap.url, Like<urlReports>,
                And<SiteMap.screenID, Like<PXModule.cr_>>>,
            OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
            Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
            DescriptionField = typeof(SiteMap.title))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region NotificationRecipient
        [PXDBLiteDefault(typeof(NotificationSource.sourceID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_SourceID_CacheAttached(PXCache sender)
        {
        }
        [PXDefault]
        [CRMContactType.List]
        [PXCheckUnique(typeof(NotificationRecipient.contactID),
            Where = typeof(Where<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>,
            And<NotificationRecipient.refNoteID, Equal<Current<BAccount.noteID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_ContactType_CacheAttached(PXCache sender)
        {
        }
        [PXUIField(DisplayName = "Contact ID")]
        [PXNotificationContactSelector(typeof(NotificationRecipient.contactType), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_ContactID_CacheAttached(PXCache sender)
        {
        }
        [PXDBString(10, IsUnicode = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]	// remove PXDefault from DAC
        protected virtual void NotificationRecipient_ClassID_CacheAttached(PXCache sender)
        {
        }
        [PXUIField(DisplayName = "Email", Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_Email_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region CROpportunityClass
        [PXUIField(DisplayName = "Class Description")]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void CROpportunityClass_Description_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #endregion

        #region Event Handlers
        #region NotificationRecipient Events
        protected virtual void NotificationRecipient_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            NotificationRecipient row = (NotificationRecipient)e.Row;
            if (row == null) return;
            Contact contact = PXSelectorAttribute.Select<NotificationRecipient.contactID>(cache, row) as Contact;
            if (contact == null)
            {
                switch (row.ContactType)
                {
                    case CRMContactType.Primary:
                        contact = DefContact.SelectWindowed(0, 1);
                        break;
                    case CRMContactType.Shipping:
                        contact = DefLocationContact.View.SelectSingle(new object[] { BAccount.Cache.Current }) as Contact;
                        break;
                }
            }
            if (contact != null)
                row.Email = contact.EMail;
        }
        #endregion  

        #region SOOrder

		[SOOrderStatus.ListWithoutOrders()]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void SOOrder_Status_CacheAttached(PXCache sender)
		{

		}

		#endregion

		#region BAccount

		[PXDimensionSelector("BIZACCT",
			typeof(Search2<
				BAccountCRM.bAccountID,
			LeftJoin<Contact,
				On<Contact.bAccountID, Equal<BAccountCRM.bAccountID>,
				And<Contact.contactID, Equal<BAccountCRM.defContactID>>>,
			LeftJoin<Address,
				On<Address.bAccountID, Equal<BAccountCRM.bAccountID>,
				And<Address.addressID, Equal<BAccountCRM.defAddressID>>>>>,
			Where<
				Current<BAccount.bAccountID>, NotEqual<BAccountCRM.bAccountID>,
				And2<Where<
					BAccountCRM.type, Equal<BAccountType.customerType>,
					Or<BAccountCRM.type, Equal<BAccountType.prospectType>,
					Or<BAccountCRM.type, Equal<BAccountType.combinedType>,
					Or<BAccountCRM.type, Equal<BAccountType.vendorType>>>>>,
				And<Match<Current<AccessInfo.userName>>>>>>),
			substituteKey: typeof(BAccountCRM.acctCD),
			fieldList: new[]
			{
				typeof(BAccountCRM.acctCD),
				typeof(BAccountCRM.acctName),
				typeof(BAccountCRM.type),
				typeof(BAccountCRM.classID),
				typeof(BAccountCRM.status),
				typeof(Contact.phone1),
				typeof(Address.city),
				typeof(Address.countryID),
				typeof(Contact.eMail)
			},
			DescriptionField = typeof(BAccountCRM.acctName))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<BAccount.parentBAccountID> e) { }

		[PXDimensionSelector("BIZACCT", 
			typeof(Search2<BAccount.acctCD,
					LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccount.bAccountID>, And<Contact.contactID, Equal<BAccount.defContactID>>>,
					LeftJoin<Address, On<Address.bAccountID, Equal<BAccount.bAccountID>, And<Address.addressID, Equal<BAccount.defAddressID>>>>>,
				Where2<Where<BAccount.type, Equal<BAccountType.customerType>,
					Or<BAccount.type, Equal<BAccountType.prospectType>,
					Or<BAccount.type, Equal<BAccountType.combinedType>,
					Or<BAccount.type, Equal<BAccountType.vendorType>>>>>,
					And<Match<Current<AccessInfo.userName>>>>>),
			typeof(BAccount.acctCD),
			typeof(BAccount.acctCD), typeof(BAccount.acctName), typeof(BAccount.type), typeof(BAccount.classID), typeof(BAccount.status), typeof(Contact.phone1), 
			typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail))]
		[PXUIField(DisplayName = "Business Account ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void BAccount_AcctCD_CacheAttached(PXCache cache)
		{
			
		}
		
		[PXDBInt]
		[PXDBChildIdentity(typeof(Location.locationID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]	// remove PXSelector from DAC
		protected virtual void BAccount_DefLocationID_CacheAttached(PXCache sender)
		{

		}

		protected virtual void BAccount_ClassID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Setup.Current.DefaultCustomerClassID;
		}

		protected virtual void BAccount_Type_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccountType.ProspectType;
		}

        protected virtual void BAccount_AcctName_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            BAccount row = (BAccount)e.Row;
            CopyAcctNameInCompName(row);
        }

		protected virtual void BAccount_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (!cache.ObjectsEqual<BAccount.acctCD>(e.Row, e.OldRow) && ((BAccount)e.OldRow).AcctCD == null)
			{
				InitBAccount((BAccount)e.Row);
			}
		}
		protected virtual void BAccount_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = (BAccount)e.Row;
			if (row == null || string.IsNullOrEmpty(row.AcctCD)) return;
			InitBAccount(row);
		}
		private void InitBAccount(BAccount row)
		{
			//Inserting Address record
			if (row.DefAddressID == null)
			{
				var addressOldDirty = AddressCurrent.Cache.IsDirty;
				var addr = (Address)AddressCurrent.Cache.CreateInstance();
				addr.BAccountID = row.BAccountID;
				addr = AddressCurrent.Insert(addr);
				row.DefAddressID = addr.AddressID;
				AddressCurrent.Cache.IsDirty = addressOldDirty;
			}

			// Inserting Default Contact record
			if (row.DefContactID == null)
			{
				var contactsOldDirty = Contacts.Cache.IsDirty;
				var contact = (Contact)Contacts.Cache.CreateInstance();
				contact.ContactType = ContactTypesAttribute.BAccountProperty;
				contact.BAccountID = row.BAccountID;
				contact = Contacts.Insert(contact);
				row.DefContactID = contact.ContactID;
				Contacts.Cache.IsDirty = contactsOldDirty;
			}

			// Inserting delivery locaiton record
			if (row.DefLocationID == null)
			{
				var locationOldDirty = Locations.Cache.IsDirty;
				var location = (Location)Locations.Cache.CreateInstance();
				location.BAccountID = row.BAccountID;
				// Location CD need to be formatted accorfing to segmented key mask prior inserting
				object cd = PXMessages.LocalizeNoPrefix(Messages.DefaultLocationCD);
				Locations.Cache.RaiseFieldUpdating<Location.locationCD>(location, ref cd);
				location.LocationCD = (string)cd;

                location.LocType = LocTypeList.CompanyLoc;
				switch (row.Type)
				{
					case BAccountType.VendorType:
						location.LocType = LocTypeList.VendorLoc;
						break;
                    case BAccountType.CustomerType:
                        location.LocType = LocTypeList.CustomerLoc;
                        break;
					case BAccountType.CombinedType:
						location.LocType = LocTypeList.CombinedLoc;
						break;
				}
				location.Descr = PXMessages.LocalizeNoPrefix(Messages.DefaultLocationDescription);
				location.IsDefault = true;
				location.DefAddressID = row.DefAddressID;
				location.IsAddressSameAsMain = true;
				location.DefContactID = row.DefContactID;
				location.IsContactSameAsMain = true;
				location = (Location)Locations.Cache.Insert(location);
				row.DefLocationID = location.LocationID;
				Locations.Cache.IsDirty = locationOldDirty;
			}
		}

	    protected virtual void BAccountCRM_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
	    {
            //Used for corrrect navigation
	        this.BAccount.Current = (BAccount)e.Row;
	    }

	    protected virtual void BAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = (BAccount)e.Row;
			if (row == null) return;

			var isNotInserted = cache.GetStatus(row) != PXEntryStatus.Inserted;
			Relations.Cache.AllowInsert = isNotInserted;
			Opportunities.Cache.AllowInsert = isNotInserted;
			Cases.Cache.AllowInsert = isNotInserted;
			Members.Cache.AllowInsert = isNotInserted;
			Subscriptions.Cache.AllowInsert = isNotInserted;

			var isCustomerOrCombined = row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType;
			var isVendor = row.Type == BAccountType.VendorType;
			var isVendorOrCombined = row.Type == BAccountType.VendorType || row.Type == BAccountType.CombinedType;
			var isCustomerOrProspect = row.Type == BAccountType.CustomerType || row.Type == BAccountType.ProspectType;
			var isCustomerOrProspectOrCombined = isCustomerOrProspect || row.Type == BAccountType.CombinedType;
			var isProspect = row.Type == BAccountType.ProspectType;

			viewCustomer.SetVisible(isNotInserted && isCustomerOrCombined);
			viewVendor.SetVisible(isNotInserted && isVendorOrCombined);

			converToCustomer.SetEnabled(isNotInserted && (isVendor || isProspect));
			converToVendor.SetEnabled(isNotInserted && isCustomerOrProspect);
			addOpportunity.SetEnabled(isNotInserted && isCustomerOrProspectOrCombined);
			addCase.SetEnabled(isNotInserted && isCustomerOrProspectOrCombined);
			addLocation.SetEnabled(isNotInserted);
			addContact.SetEnabled(isNotInserted);
			validateAddresses.SetEnabled(isNotInserted);

			PXUIFieldAttribute.SetEnabled<BAccount.parentBAccountID>(cache, row, isCustomerOrCombined == false || PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>() == false);

		    CRCustomerClass customerClass = row.ClassID.
		        With(_ => (CRCustomerClass)PXSelectReadonly<CRCustomerClass,
		                Where<CRCustomerClass.cRCustomerClassID, Equal<Required<CRCustomerClass.cRCustomerClassID>>>>.
		            Select(this, _));
		    if (customerClass != null)
		    {
		        Activities.DefaultEMailAccountId = customerClass.DefaultEMailAccountID;
		    }

			PXUIFieldAttribute.SetVisible(Locations.Cache, null, typeof(Location.cPriceClassID).Name, isCustomerOrProspectOrCombined);
			PXUIFieldAttribute.SetVisible(Locations.Cache, null, typeof(Location.cSalesAcctID).Name, isCustomerOrCombined);
			PXUIFieldAttribute.SetVisible(Locations.Cache, null, typeof(Location.cSalesSubID).Name, isCustomerOrCombined);
		}

		protected virtual void BAccount_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			BAccount row = e.Row as BAccount;

			if (row != null && (row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType))
			{
				AR.Customer customer =
					SelectFrom<AR.Customer>
					.Where<AR.Customer.acctCD.IsEqual<@P.AsString>>
					.View
					.Select(this, row.AcctCD);
				AR.CustomerMaint.VerifyParentBAccountID<BAccount.parentBAccountID>(this, cache, customer, row);
			}
		}


		protected virtual void Contact_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = (Contact)e.Row;
			if (row == null || e.Operation != PXDBOperation.Update) return;

			var oldLang = (string)this.Contacts.Cache.GetValueOriginal<Contact.languageID>(row);
			if (oldLang == row.LanguageID) return;
			
			switch (BAccount.Current?.Type)
			{
				case BAccountType.CustomerType:
					
					PXDatabase.Update<AR.Customer>(
						new PXDataFieldAssign<AR.Customer.localeName>(row.LanguageID), 
						new PXDataFieldRestrict<AR.Customer.bAccountID>(BAccount.Current?.BAccountID));

					break;

				case BAccountType.VendorType:

					PXDatabase.Update<AP.Vendor>(
						new PXDataFieldAssign<AP.Vendor.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AP.Vendor.bAccountID>(BAccount.Current?.BAccountID));
					
					break;

				case BAccountType.CombinedType:

					PXDatabase.Update<AR.Customer>(
						new PXDataFieldAssign<AR.Customer.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AR.Customer.bAccountID>(BAccount.Current?.BAccountID));
					
					PXDatabase.Update<AP.Vendor>(
						new PXDataFieldAssign<AP.Vendor.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AP.Vendor.bAccountID>(BAccount.Current?.BAccountID));
					
					break;

			}
		}

		protected virtual void Address_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
		    Address row = e.Row as Address;
		    if (row != null)
		    {
                if (BAccount.Current.DefAddressID == null)
                {
                    BAccount.Current.DefAddressID = row.AddressID;
                    Caches[typeof(BAccount)].MarkUpdated(BAccount.Current);
                }
                else
                {
                    var ret = AddressCurrent.SelectSingle();
                    if (ret == null)
                    {
                        BAccount.Current.DefAddressID = row.AddressID;
                        Caches[typeof(BAccount)].MarkUpdated(BAccount.Current);
                    }
                }
            }
		}
		protected virtual void Contact_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.Row != null && this.BAccount.Current.DefContactID == null)
			{
				cache.SetValue<Contact.contactType>(e.Row, ContactTypesAttribute.BAccountProperty);				
				this.BAccount.Current.DefContactID = ((Contact)e.Row).ContactID;
			}
		}
		#endregion

		#region Contact

		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBLiteDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXParent(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Contact.bAccountID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void _(Events.CacheAttached<CRLead.bAccountID> e) { }

		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBLiteDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXParent(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Contact.bAccountID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Member Name", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false)]
		public virtual void Contact_MemberName_CacheAttached(PXCache sender)
		{

		}
		[PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.bAccountID))]
		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]	// replace single attribute from DAC
		public virtual void CROpportunity_BAccountID_CacheAttached(PXCache sender)
		{
			
		}

        [PXUIField(DisplayName = "Location")]                
        [PXDBLiteDefault(typeof(Location.locationID))]
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>>),
            DisplayName = "Location", 
            DescriptionField = typeof(Location.descr),
            BqlField = typeof(Standalone.CROpportunityRevision.locationID),
            DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void CROpportunity_LocationID_CacheAttached(PXCache sender)
        {

        }

		public override void Persist()
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
                ModifyContact();
				
				base.Persist();
				ts.Complete();
			}
		}

		#endregion

		#region Lead

		[PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRLead.memberName> e) { }

		#endregion

		#region Location

		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		[PXParent(typeof(Select<BAccount,Where<BAccount.bAccountID,Equal<Current<Location.bAccountID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void Location_BAccountID_CacheAttached(PXCache sender)
		{

		}

		[PXDBInt]
		[PXDBChildIdentity(typeof(Address.addressID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void Location_DefAddressID_CacheAttached(PXCache sender)
		{
			
		}

		[PXDBInt]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void Location_CARAccountLocationID_CacheAttached(PXCache sender)
		{
			
		}

		[PXDBInt]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void Location_VAPAccountLocationID_CacheAttached(PXCache sender)
		{
			
		}

		[PXDBInt]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void Location_VPaymentInfoLocationID_CacheAttached(PXCache sender)
		{
			
		}

		[PXShort]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void Location_VSiteIDIsNull_CacheAttached(PXCache sender)
		{
			
		}

		protected virtual void Location_VBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void Location_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = e.Row as Location;
			if (row == null) return;

			row.IsDefault = false;
			BAccount acct = BAccount.Current;
			if (acct == null) return;

			if (row.LocationID == acct.DefLocationID)
				row.IsDefault = true;

			bool CustomerDetailsVisible = (row.LocType == LocTypeList.CustomerLoc || row.LocType == LocTypeList.CombinedLoc);
			PXUIFieldAttribute.SetEnabled<Location.cTaxZoneID>(cache, null, CustomerDetailsVisible);
			PXUIFieldAttribute.SetEnabled<Location.cAvalaraCustomerUsageType>(cache, null, CustomerDetailsVisible);
			PXUIFieldAttribute.SetEnabled<Location.cBranchID>(cache, null, CustomerDetailsVisible);
		}

		object _KeyToAbort = null;

		protected virtual void Location_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					if ((int?)sender.GetValue<Location.vAPAccountLocationID>(e.Row) == null)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VAPAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<Location.vAPAccountLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<Location.vPaymentInfoLocationID>(e.Row) == null)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VPaymentInfoLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<Location.vPaymentInfoLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<Location.cARAccountLocationID>(e.Row) == null)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("CARAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<Location.cARAccountLocationID>(e.Row, _KeyToAbort);
					}
				}
				else
				{
					if (e.TranStatus == PXTranStatus.Aborted)
					{
						if (object.Equals(_KeyToAbort, sender.GetValue<Location.vAPAccountLocationID>(e.Row)))
						{
							sender.SetValue<Location.vAPAccountLocationID>(e.Row, null);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<Location.vPaymentInfoLocationID>(e.Row)))
						{
							sender.SetValue<Location.vPaymentInfoLocationID>(e.Row, null);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<Location.cARAccountLocationID>(e.Row)))
						{
							sender.SetValue<Location.cARAccountLocationID>(e.Row, null);
						}
					}
					_KeyToAbort = null;
				}
			}
		}

		#endregion

		#region Address

		[PXDBLiteDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Address.bAccountID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Address_BAccountID_CacheAttached(PXCache sender) { }

		#endregion

		#region CRCampaignMembers


		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(BAccount.defContactID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void CRCampaignMembers_ContactID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCheckUnique(typeof(CRCampaignMembers.contactID))]
		protected virtual void CRCampaignMembers_CampaignID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region CRMarketingListMember

		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(BAccount.defContactID), DefaultForUpdate = false)]
		[PXUIField(DisplayName = "Name")]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void CRMarketingListMember_ContactID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region CRPMTimeActivity

		[PXSelector(typeof(Contact.contactID), DescriptionField = typeof(Contact.memberName))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRPMTimeActivity_ContactID_CacheAttached(PXCache sender) { }

		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBChildIdentity(typeof(BAccount.bAccountID))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void CRPMTimeActivity_BAccountID_CacheAttached(PXCache sender) { }

        #endregion

        #region CRRelation

        [PXDBChildIdentity(typeof(BAccount.bAccountID))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<CRRelation.entityID> e) { }

        #endregion

        #endregion

        #region Private Methods
        private void CopyAcctNameInCompName(BAccount row)
        {
            if (this.DefContact.Current != null)
            {
                this.DefContact.Current.FullName = row.AcctName;
                this.DefContact.Cache.Update(this.DefContact.Current);
            }
        }

        public virtual void ModifyContact()
        {
            BAccount acct = (BAccount)BAccount.Cache.Current;
            if (acct != null && acct.Status == CR.BAccount.status.Inactive)
            {
                ContactMaint graph = CreateInstance<ContactMaint>();
                foreach (Contact c in Contacts.Select())
				{
					if (c.IsActive == false)
						continue;

                    c.IsActive = false;
                    graph.ContactCurrent.Cache.Update(c);
                    graph.Save.Press();
                }
            }
        }
		#endregion

		#region Extensions

		/// <exclude/>
		public class DefaultAccountOwner : CRDefaultDocumentOwner<
			BusinessAccountMaint, BAccount,
			BAccount.classID, BAccount.ownerID, BAccount.workgroupID>
		{ }

		/// <exclude/>
		public class CRDuplicateEntitiesForBAccountGraphExt : CRDuplicateEntities<BusinessAccountMaint, BAccount>
		{
			#region Initialization 

			protected override Type AdditionalConditions => typeof(
				
				DuplicateContact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>
				.And<BAccountR.status.IsNotEqual<BAccountR.status.inactive>>
			);

			protected override string WarningMessage => Messages.BAccountHavePossibleDuplicates;

			public static bool IsActive()
			{
				return IsExtensionActive();
			}

			public override void Initialize()
			{
				base.Initialize();

				DuplicateDocuments = new PXSelectExtension<DuplicateDocument>(Base.DefContact);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(BAccount)) { Key = typeof(BAccount.bAccountID) };
			}

			protected override DuplicateDocumentMapping GetDuplicateDocumentMapping()
			{
				return new DuplicateDocumentMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}

			#endregion

			#region Events

			[CRDuplicateBAccountSelector(typeof(MergeParams.sourceEntityID), SelectorMode = PXSelectorMode.DisplayModeHint)]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<MergeParams.targetEntityID> e) { }

			protected virtual void _(Events.FieldUpdated<BAccount, BAccount.status> e)
			{
				BAccount row = e.Row as BAccount;
				if (e.Row == null)
					return;

				if (row.Status != CR.BAccount.status.Inactive && row.Status != (string)e.OldValue)
				{
					var dupDoc = base.DuplicateDocuments.Current;

					base.DuplicateDocuments.SetValueExt<DuplicateDocument.duplicateStatus>(dupDoc, DuplicateStatusAttribute.NotValidated);
				}
			}

			public virtual void _(Events.FieldDefaulting<MergeParams, MergeParams.targetEntityID> e)
			{
				BAccount current = (BAccount)Base.Caches<BAccount>().Current;

				List<BAccount> duplicates = PXSelectorAttribute.SelectAll<MergeParams.targetEntityID>(e.Cache, e.Row)
					.RowCast<BAccount>()
					.Where(a => a.Type == BAccountType.CustomerType || a.Type == BAccountType.VendorType || a.Type == BAccountType.CombinedType)
					.ToList();

				e.NewValue = (current.Type == BAccountType.CustomerType || current.Type == BAccountType.VendorType || current.Type == BAccountType.CombinedType) 
							&& current.Status != CR.BAccount.status.Inactive
							|| duplicates.Count == 0
					? current.BAccountID
					: duplicates
						.OrderBy(duplicate => duplicate?.BAccountID)
						.FirstOrDefault()
						?.BAccountID;
			}

			public virtual void _(Events.FieldVerifying<MergeParams, MergeParams.targetEntityID> e)
			{
				int? targetID = e.NewValue as int?;

				if (PXSelectorAttribute.SelectAll<MergeParams.targetEntityID>(e.Cache, e.Row)
					.RowCast<BAccount>()
					.Any(account => account.BAccountID != targetID && account.Type != BAccountType.ProspectType))
				{
					BAccount acct = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<MergeParams.targetEntityID>>>>.Select(Base, e.Row.TargetEntityID);

					PXUIFieldAttribute.SetError<MergeParams.targetEntityID>(e.Cache, e.Row, Messages.OnlyBAccountMergeSources, acct.AcctCD);
				}
			}

			public virtual void _(Events.FieldSelecting<MergeParams, MergeParams.targetEntityID> e)
			{
				if (e.Row == null)
					return;

				int? targetID = e.Row.TargetEntityID;

				if (PXSelectorAttribute.SelectAll<MergeParams.targetEntityID>(e.Cache, e.Row)
					.RowCast<BAccount>()
					.Any(account => account.BAccountID != targetID && account.Type != BAccountType.ProspectType))
				{
					BAccount acct = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<MergeParams.targetEntityID>>>>.Select(Base, e.Row.TargetEntityID);

					PXUIFieldAttribute.SetError<MergeParams.targetEntityID>(e.Cache, e.Row, Messages.OnlyBAccountMergeSources, acct.AcctCD);

					return;
				}

				PXUIFieldAttribute.SetError<MergeParams.targetEntityID>(e.Cache, e.Row, null);
			}

			public override void _(Events.RowSelected<Extensions.CRDuplicateEntities.Document> e)
			{
				base._(e);

				if (e.Row == null) return;

				DuplicateAttach.SetVisible(false);
			}

			public virtual void _(Events.FieldSelecting<CRDuplicateRecord, CRDuplicateRecord.selected> e)
			{
				if (e.Row == null)
					return;

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, null, null, null, null, null, null, null, PXErrorLevel.Undefined,
					e.Row.DuplicateContactType == ContactTypesAttribute.BAccountProperty,
					null, null, PXUIVisibility.Undefined, null, null, null);
			}

			#endregion

			#region Actions

			[PXUIField(DisplayName = Messages.MarkAsValidated)]
			[PXButton]
			public override IEnumerable markAsValidated(PXAdapter adapter)
			{
				base.markAsValidated(adapter);

				foreach (PXResult<BAccount, Contact> pxresult in adapter.Get())
				{
					var account = (BAccount)pxresult;

					Contact defContact = Base.DefContact.View.SelectSingleBound(new object[] { account }) as Contact;

					if (defContact != null)
					{
						defContact = (Contact)Base.DefContact.Cache.CreateCopy(defContact);

						defContact.DuplicateStatus = DuplicateStatusAttribute.Validated;
						defContact.DuplicateFound = false;

						Base.DefContact.Update(defContact);

						if (Base.IsContractBasedAPI)
							Base.Save.Press();
					}
				}

				return adapter.Get();
			}

			[PXUIField(DisplayName = Messages.CloseAsDuplicate)]
			[PXButton]
			public override IEnumerable closeAsDuplicate(PXAdapter adapter)
			{
				base.closeAsDuplicate(adapter);

				foreach (PXResult<BAccount, Contact> pxresult in adapter.Get())
				{
					var account = (BAccount)pxresult;

					Contact defContact = Base.DefContact.View.SelectSingleBound(new object[] { account }) as Contact;

					if (defContact != null)
					{
						defContact = (Contact)Base.DefContact.Cache.CreateCopy(defContact);

						defContact.DuplicateStatus = DuplicateStatusAttribute.Duplicated;

						Base.DefContact.Update(defContact);

						if (Base.IsContractBasedAPI)
							Base.Save.Press();
					}
				}

				return adapter.Get();
			}

			[PXOverride]
			public virtual void Persist(Action del)
			{
				del();

				var doc = Documents.Current;
				if (doc == null)
					return;

				BAccount ba = Documents.Cache.GetMain(doc) as BAccount;

				if (Setup.Current?.ValidateAccountDuplicatesOnEntry == true && ba?.Status != CR.BAccount.status.Inactive)
				{
					CheckForDuplicates.Press();
				}
			}

			#endregion

			#region Overrides

			protected override BAccount GetTargetEntity(int targetID)
			{
				return PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(Base, targetID);
			}

			protected override Contact GetTargetContact(BAccount targetEntity)
			{
				return PXSelect<Contact, Where<Contact.contactID, Equal<Required<BAccount.defContactID>>>>.Select(Base, targetEntity.DefContactID);
			}

			protected override Address GetTargetAddress(BAccount targetEntity)
			{
				return PXSelect<Address, Where<Address.addressID, Equal<Required<BAccount.defAddressID>>>>.Select(Base, targetEntity.DefAddressID);
			}

			protected override void MergeEntities(PXGraph targetGraph, BAccount targetEntity, BAccount duplicateAccount)
			{
				int? defContactID = duplicateAccount.DefContactID;
				PXCache Contacts = targetGraph.Caches[typeof(Contact)];
				foreach (Contact contact in PXSelect<
							Contact, 
						Where<
							Contact.bAccountID, Equal<Required<BAccount.bAccountID>>>>
					.Select(targetGraph, duplicateAccount.BAccountID)
					.RowCast<Contact>()
					.Where(c => c.ContactID != defContactID)
					.Select(c => (Contact)Contacts.CreateCopy(c)))
				{
					contact.BAccountID = targetEntity.BAccountID;

					Contacts.Update(contact);
				}

				PXCache Activities = targetGraph.Caches[typeof(CRPMTimeActivity)];
				foreach (CRPMTimeActivity activity in PXSelect<
							CRPMTimeActivity, 
						Where<
							CRPMTimeActivity.bAccountID, Equal<Required<BAccount.bAccountID>>>>
					.Select(targetGraph, duplicateAccount.BAccountID)
					.RowCast<CRPMTimeActivity>()
					.Select(cas => (CRPMTimeActivity)Activities.CreateCopy(cas)))
				{
					if (activity.BAccountID == duplicateAccount.BAccountID)
					{
						activity.BAccountID = targetEntity.BAccountID;
					}
					activity.BAccountID = targetEntity.BAccountID;

					Activities.Update(activity);
				}

				PXCache Cases = targetGraph.Caches[typeof(CRCase)];
				foreach (CRCase cas in PXSelect<
							CRCase, 
						Where<
							CRCase.customerID, Equal<Required<BAccount.bAccountID>>>>
					.Select(targetGraph, duplicateAccount.BAccountID)
					.RowCast<CRCase>()
					.Select(cas => (CRCase)Cases.CreateCopy(cas)))
				{
					cas.CustomerID = targetEntity.BAccountID;

					Cases.Update(cas);
				}

				PXCache Opportunities = targetGraph.Caches[typeof(CROpportunity)];
				foreach (CROpportunity opp in PXSelect<
							CROpportunity, 
						Where<
							CROpportunity.bAccountID, Equal<Required<BAccount.bAccountID>>>>
					.Select(targetGraph, duplicateAccount.BAccountID)
					.RowCast<CROpportunity>()
					.Select(opp => (CROpportunity)Opportunities.CreateCopy(opp)))
				{
					opp.BAccountID = targetEntity.BAccountID;
					opp.LocationID = targetEntity.DefLocationID;

					Opportunities.Update(opp);
				}

				PXCache Relations = targetGraph.Caches[typeof(CRRelation)];
				foreach (CRRelation rel in PXSelectJoin<CRRelation,
					LeftJoin<CRRelation2,
						On<CRRelation.entityID, Equal<CRRelation2.entityID>,
						And<CRRelation.role, Equal<CRRelation2.role>,
						And<CRRelation2.refNoteID, Equal<Required<BAccount.noteID>>>>>>,
					Where<CRRelation2.entityID, IsNull,
						And<CRRelation.refNoteID, Equal<Required<BAccount.noteID>>>>>
					.Select(targetGraph, targetEntity.NoteID, duplicateAccount.NoteID)
					.RowCast<CRRelation>()
					.Select(rel => (CRRelation)Relations.CreateCopy(rel)))
				{
					rel.RelationID = null;
					rel.RefNoteID = targetEntity.NoteID;

					Relations.Insert(rel);
				}

				PXCache Leads = targetGraph.Caches[typeof(CRLead)];
				foreach (CRLead lead in PXSelect<
							CRLead,
						Where<
							CRLead.bAccountID, Equal<Required<BAccount.bAccountID>>>>
					.Select(targetGraph, duplicateAccount.BAccountID)
					.RowCast<CRLead>()
					.Select(lead => (CRLead)Leads.CreateCopy(lead)))
				{
					// do it silently to not trigger other events
					Leads.SetValue<CRLead.bAccountID>(lead, targetEntity.BAccountID);
				}
			}

			protected override bool CheckIsActive()
			{
				BAccount account = Base.BAccount.Current;

				if (account == null)
					return false;

				return account.Status != CR.BAccount.status.Inactive;
			}

			public override void GetAllProperties(List<FieldValue> values, HashSet<string> fieldNames)
			{
				int order = 0;

				values.AddRange(GetMarkedPropertiesOf<BAccount>(Base, ref order).Where(fld => fieldNames.Add(fld.Name)));
				values.AddRange(GetMarkedPropertiesOf<Contact>(Base, ref order).Where(fld => fieldNames.Add(fld.Name)));

				base.GetAllProperties(values, fieldNames);
			}

			protected override void DoDuplicateAttach(DuplicateDocument duplicateDocument)
			{
				return;
			}

			#endregion
		}

		/// <exclude/>
		public class CreateLeadFromAccountGraphExt : CRCreateLeadAction<BusinessAccountMaint, BAccount>
		{
			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<DocumentAddress>(Base.AddressCurrent);
			}

			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}
		}

		/// <exclude/>
		public class UpdateRelatedContactInfoFromAccountGraphExt : CRUpdateRelatedContactInfoGraphExt<BusinessAccountMaint>
		{
			protected virtual void _(Events.RowPersisted<Contact> e)
			{
				var row = e.Row;
				if (row == null || e.TranStatus != PXTranStatus.Open || e.Operation != PXDBOperation.Update || !ContactWasUpdated(e.Cache, row))
					return;

				BAccount account = Base.BAccount.Current ?? PXSelect<
						BAccount,
						Where<
							BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<BAccount.defContactID.IsEqual<@P.AsInt>>>>
					.Select(Base, row.BAccountID, row.ContactID);

				if (account == null)
					return;

				PXUpdateJoin<
					Set<Contact.firstName, Required<Contact.firstName>,
					Set<Contact.lastName, Required<Contact.lastName>,
					Set<Contact.fullName, Required<Contact.fullName>,
					Set<Contact.salutation, Required<Contact.salutation>,
					Set<Contact.eMail, Required<Contact.eMail>,
					Set<Contact.webSite, Required<Contact.webSite>,
					Set<Contact.phone1Type, Required<Contact.phone1Type>,
					Set<Contact.phone1, Required<Contact.phone1>,
					Set<Contact.phone2Type, Required<Contact.phone2Type>,
					Set<Contact.phone2, Required<Contact.phone2>,
					Set<Contact.phone3Type, Required<Contact.phone3Type>,
					Set<Contact.phone3, Required<Contact.phone3>,
					Set<Contact.faxType, Required<Contact.faxType>,
					Set<Contact.fax, Required<Contact.fax>,
					Set<Contact.grammValidationDateTime, Required<Contact.grammValidationDateTime>,
					Set<Contact.consentAgreement, Required<Contact.consentAgreement>,
					Set<Contact.consentDate, Required<Contact.consentDate>,
					Set<Contact.consentExpirationDate, Required<Contact.consentExpirationDate>
					>>>>>>>>>>>>>>>>>>,
				Contact,
				LeftJoin<Standalone.CRLead,
					On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>>,
				Where<
					// Leads that are linked to the same Account
					Contact.bAccountID.IsEqual<@P.AsInt>
					.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>>>
				.Update(Base,
					//Set
					row.FirstName,
					row.LastName,
					row.FullName,
					row.Salutation,
					row.EMail,
					row.WebSite,
					row.Phone1Type,
					row.Phone1,
					row.Phone2Type,
					row.Phone2,
					row.Phone3Type,
					row.Phone3,
					row.FaxType,
					row.Fax,
					new DateTime(1900, 1, 1),
					row.ConsentAgreement,
					row.ConsentDate,
					row.ConsentExpirationDate,

					// Where
					account.BAccountID);
			}

			protected virtual void _(Events.RowPersisted<Address> e)
			{
				var row = e.Row;
				if (row == null || e.TranStatus != PXTranStatus.Open || e.Operation != PXDBOperation.Update || !AddressWasUpdated(e.Cache, row))
					return;

				BAccount account = Base.BAccount.Current ?? PXSelect<
						BAccount,
						Where<
							BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<BAccount.defAddressID.IsEqual<@P.AsInt>>>>
					.Select(Base, row.BAccountID, row.AddressID);

				if (account == null || row.AddressID != account.DefAddressID)
					return;

				PXUpdateJoin<
					Set<Address.addressLine1, Required<Address.addressLine1>,
					Set<Address.addressLine2, Required<Address.addressLine2>,
					Set<Address.city, Required<Address.city>,
					Set<Address.state, Required<Address.state>,
					Set<Address.postalCode, Required<Address.postalCode>,
					Set<Address.countryID, Required<Address.countryID>>>>>>>,
				Address,
				InnerJoin<Contact,
					On<Contact.defAddressID.IsEqual<Address.addressID>>,
				LeftJoin<Standalone.CRLead,
					On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>>>,
				Where<
					// Leads that are linked to the same Account
					Contact.bAccountID.IsEqual<@P.AsInt>
					.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>>>
				.Update(Base,
					//Set
					row.AddressLine1,
					row.AddressLine2,
					row.City,
					row.State,
					row.PostalCode,
					row.CountryID,

					// Where
					account.BAccountID);
			}
		}

		/// <exclude/>
		public class LastNameOrCompanyNameRequiredGraphExt : PXGraphExtension<BusinessAccountMaint>
		{
			[PXRemoveBaseAttribute(typeof(PXUIRequiredAttribute))]
			protected virtual void _(Events.CacheAttached<CRLead.displayName> e) { }

			[PXRemoveBaseAttribute(typeof(CRLastNameDefaultAttribute))]
			protected virtual void _(Events.CacheAttached<CRLead.lastName> e) { }

			protected virtual void _(Events.RowPersisting<CRLead> e)
			{
				var row = e.Row;
				if (row == null) return;

				if (row.LastName == null && row.FullName == null)
					throw new PXSetPropertyException(Messages.LastNameOrFullNameReqired);
			}
		}

		#endregion
	}
}
