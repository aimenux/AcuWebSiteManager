using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.Objects.AP;
using PX.SM;
using PX.TM;
using System;
using PX.Data;
using PX.EP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CA;
using PX.Objects.PM;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.Common;
using PX.Objects.EP.DAC;

namespace PX.Objects.EP
{
	[Serializable]
	public partial class EPEmployeeCompanyTreeMember : EPCompanyTreeMember
	{
		#region WorkGroupID
		public new abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
		[PXDBInt(IsKey = true)]
		[PXParent(typeof(Select<EPEmployee,
		 Where<EPEmployee.userID, Equal<Current<EPEmployeeCompanyTreeMember.userID>>>>))]
		[PXSelector(typeof(EPCompanyTree.workGroupID), SubstituteKey = typeof(EPCompanyTree.description))]
		[PXUIField(DisplayName = "Workgroup ID")]
		public override int? WorkGroupID
		{
			get
			{
				return this._WorkGroupID;
			}
			set
			{
				this._WorkGroupID = value;
			}
		}
		#endregion

		#region UserID
		public new abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		[PXDBGuid(IsKey = true)]
		[PXDefault(typeof(EPEmployee.userID))]
		[PXUIField(DisplayName = "User", Visible = false)]
		public override Guid? UserID
		{
			get
			{
				return this._UserID;
			}
			set
			{
				this._UserID = value;
			}
		}
		#endregion
	}

	public class EmployeeMaint : PXGraph<EmployeeMaint>
	{
        #region Selects Declartion

        [PXViewName(Messages.EPEmployee)]
        public PXSelectJoin<EPEmployee, LeftJoin<GL.Branch, On<GL.Branch.bAccountID, Equal<EPEmployee.parentBAccountID>>>, Where<EPEmployee.parentBAccountID, IsNull, Or<MatchWithBranch<GL.Branch.branchID>>>> Employee;
        public PXSelect<BAccount2> BAccountParent;
		public PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPEmployee.bAccountID>>>> CurrentEmployee;
		public PXSetup<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Optional<EPEmployee.vendorClassID>>>> EmployeeClass;

        [PXViewName(Messages.FinancialSettings)]        
        public PXSelect<Location, Where<Location.bAccountID, Equal<Current<EPEmployee.bAccountID>>,
			And<Location.locationID, Equal<Current<EPEmployee.defLocationID>>>>> DefLocation;

        [PXViewName(Messages.Address)]
        public PXSelect<Address, Where<Address.bAccountID, Equal<Optional<EPEmployee.parentBAccountID>>,
			And<Address.addressID, Equal<Optional<EPEmployee.defAddressID>>>>> Address;

        [PXViewName(Messages.Contact)]
        public SelectContactEmailSync<Where<Contact.bAccountID, Equal<Optional<EPEmployee.parentBAccountID>>,
			And<Contact.contactID, Equal<Optional<EPEmployee.defContactID>>>>> Contact;

		public PXSelectJoin<EMailSyncAccount,
			InnerJoin<BAccount,
				On<BAccount.bAccountID, Equal<EMailSyncAccount.employeeID>>>,
			Where<BAccount.defContactID, Equal<Optional<Contact.contactID>>>> SyncAccount;

		public PXSelect<EMailAccount,
			Where<EMailAccount.emailAccountID, Equal<Optional<EMailSyncAccount.emailAccountID>>>> EMailAccounts;

		public PXSelectJoin<VendorPaymentMethodDetail, 
                    InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<VendorPaymentMethodDetail.paymentMethodID>,
							    And<PaymentMethodDetail.detailID, Equal<VendorPaymentMethodDetail.detailID>,
                                    And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                                Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>,
                    Where<VendorPaymentMethodDetail.bAccountID, Equal<Current<Location.bAccountID>>,
			            And<VendorPaymentMethodDetail.locationID, Equal<Current<Location.locationID>>,
    			            And<VendorPaymentMethodDetail.paymentMethodID, Equal<Current<Location.vPaymentMethodID>>>>>> PaymentDetails;

		public PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<Location.vPaymentMethodID>>,
                                And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                                Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>> PaymentTypeDetails;
		public PXSelect<BAccount2, Where<BAccount2.acctCD, Equal<Required<BAccount2.acctCD>>>> BAccount;
		public PXSelectJoin<ContactNotification,
			InnerJoin<NotificationSetup, On<NotificationSetup.setupID, Equal<ContactNotification.setupID>>>,
			Where<ContactNotification.contactID, Equal<Optional<EPEmployee.defContactID>>>> NWatchers;

		public PXSelect<EPEmployeeCompanyTreeMember,
			Where<EPEmployeeCompanyTreeMember.userID, Equal<Current<EPEmployee.userID>>>> CompanyTree;

		public PXSelect<PMLaborCostRate,
			Where<PMLaborCostRate.employeeID, Equal<Current<EPEmployee.bAccountID>>,
			And<PMLaborCostRate.type, Equal<PMLaborCostRateType.employee>>>,
			OrderBy<Asc<PMLaborCostRate.effectiveDate>>> EmployeeRates;
		public PXSelect<EPEmployeePosition,
			Where<EPEmployeePosition.employeeID, Equal<Current<EPEmployee.bAccountID>>>,
			OrderBy<Desc<EPEmployeePosition.startDate>>> EmployeePositions;

		public PXSelect<EPEmployeePosition,
			Where<EPEmployeePosition.employeeID, Equal<Current<EPEmployee.bAccountID>>, And<EPEmployeePosition.isActive, Equal<True>>>> ActivePosition;

		public PXSelectJoin<EPEmployeeClassLaborMatrix
				, LeftJoin<IN.InventoryItem, On<IN.InventoryItem.inventoryID, Equal<EPEmployeeClassLaborMatrix.labourItemID>>
					, LeftJoin<EPEarningType, On<EPEarningType.typeCD, Equal<EPEmployeeClassLaborMatrix.earningType>>>
					>
					, Where<EPEmployeeClassLaborMatrix.employeeID, Equal<Current<EPEmployee.bAccountID>>>>
	
			LaborMatrix;
        [PXCopyPasteHiddenView]
		public PXSelectUsers<EPEmployee, Where<Users.pKID, Equal<Current<EPEmployee.userID>>>> User;
        [PXCopyPasteHiddenView]
		public PXSelectUsersInRoles UserRoles;
        [PXCopyPasteHiddenView]
		public PXSelectAllowedRoles Roles;

		[PXViewName(CR.Messages.Answers)]
		public CRAttributeList<EPEmployee>
			Answers;

		public PXSelect<EPWingman, Where<EPWingman.employeeID, Equal<Current<EPEmployee.bAccountID>>>> Wingman;

		public PXSetup<GL.Branch, Where<GL.Branch.bAccountID, Equal<Optional<EPEmployee.parentBAccountID>>>> company;
		public PXSetup<GL.Company> companySetup;

        [PXHidden]
        public PXFilter<GenTimeCardFilter> GenTimeCardFilter;

        [PXViewName(Messages.Activities)]
		[PXFilterable]
		[CRReference(typeof(EPEmployee.bAccountID), Persistent = true)]
		public CRActivityList<EPEmployee>
			Activities;

        public PXSelectJoin<EPRule,
            InnerJoin<EPAssignmentMap, On<EPAssignmentMap.assignmentMapID, Equal<EPRule.assignmentMapID>>>,
             Where<EPRule.ownerID, Equal<Current<EPEmployee.userID>>>> AssigmentAndApprovalMaps;

		public PXSelect<GL.Branch> BranchView;

		public PXSelectJoin<EPEmployeeCorpCardLink,
			LeftJoin<CACorpCard,
				On<CACorpCard.corpCardID, Equal<EPEmployeeCorpCardLink.corpCardID>>>,
			Where<EPEmployeeCorpCardLink.employeeID, Equal<Current<EPEmployee.bAccountID>>>> EmployeeCorpCards;


		public EmployeeMaint()
		{
			PXUIFieldAttribute.SetDisplayName<ContactNotification.classID>(this.NWatchers.Cache, Messages.CustomerVendorClass);
            PXUIFieldAttribute.SetDisplayName<EPRule.name>(this.AssigmentAndApprovalMaps.Cache, Messages.Rule);
            PXUIFieldAttribute.SetDisplayName<EPRule.stepID>(this.AssigmentAndApprovalMaps.Cache, Messages.Step);
            PXUIFieldAttribute.SetDisplayName<EPAssignmentMap.name>(this.Caches[typeof(EPAssignmentMap)], Messages.MapID);

            PXUIFieldAttribute.SetEnabled<EPLoginTypeAllowsRole.rolename>(Roles.Cache, null, false);
			ApplyUserAccessRights(User, Roles);
			Roles.Cache.AllowInsert = false;
			Roles.Cache.AllowDelete = false;
            Action.AddMenuAction(detachUser);
            Action.AddMenuAction(ChangeID);
			inquiry.AddMenuAction(laborCostRates);

			// Ensure the cache exists
			this.Caches<RedirectEmployeeParameters>();

			EmployeeCorpCards.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.expenseManagement>();

		}

		public override void Clear()
		{
			RedirectEmployeeParameters redirectParameters = this.Caches<RedirectEmployeeParameters>().Current as RedirectEmployeeParameters;
			base.Clear();
			if(redirectParameters != null)
			{
				this.Caches<RedirectEmployeeParameters>().Insert(redirectParameters);
			}
			isPaymentMergedFlag = false;
		}

		private bool isPaymentMergedFlag = false;

        #endregion

        #region Buttons Definition

        public PXAction<EPEmployee> generateTimeCards;
        [PXUIField(DisplayName = Messages.GenerateTimeCards)]
        [PXButton]
        public virtual IEnumerable GenerateTimeCards(PXAdapter adapter)
        {
            if (Employee.Current != null && Employee.Current.BAccountID != null)
            {
                if (GenTimeCardFilter.AskExt(
                    (gr, view) =>
                    {
                        EPTimeCard r = PXSelect<EPTimeCard, Where<EPTimeCard.employeeID, Equal<Required<EPTimeCard.employeeID>>>, OrderBy<Desc<EPTimeCard.weekId>>>.SelectWindowed(this,0,1,Employee.Current.BAccountID);
                        if (r == null)
                            throw new PXException(Messages.ThereAreNoTimeCardsToGenerate);
                        GenTimeCardFilter.Cache.SetValueExt<GenTimeCardFilter.lastDateGenerated>(GenTimeCardFilter.Current, (((EPTimeCard)r).WeekStartDate ?? Accessinfo.BusinessDate).Value.AddDays(7));
                        foreach (EPEmployeePosition p in
                                PXSelectGroupBy<EPEmployeePosition, Where<EPEmployeePosition.employeeID, Equal<Required<EPEmployee.bAccountID>>>,
                                Aggregate<Max<EPEmployeePosition.startDate>>>.SelectWindowed(this, 0, 1, Employee.Current.BAccountID))
                        {
                            if(p.StartDate < GenTimeCardFilter.Current.LastDateGenerated)
                                GenTimeCardFilter.Cache.SetValueExt<GenTimeCardFilter.generateUntil>(GenTimeCardFilter.Current, GenTimeCardFilter.Current.LastDateGenerated);
                            else
                                GenTimeCardFilter.Cache.SetValueExt<GenTimeCardFilter.generateUntil>(GenTimeCardFilter.Current, p.StartDate);
                            break;
                        }
                    }) == WebDialogResult.OK)
                {
                    PXLongOperation.StartOperation(this, () =>
                    {
                        if (GenTimeCardFilter.Current.LastDateGenerated == null || GenTimeCardFilter.Current.GenerateUntil == null || GenTimeCardFilter.Current.GenerateUntil <= GenTimeCardFilter.Current.LastDateGenerated)
                        {
                            throw new PXOperationCompletedWithErrorException(Messages.WrongDates);
                        }
                        TimeCardMaint tcgraph = PXGraph.CreateInstance<TimeCardMaint>();
                        EPTimeCard newCard = null;
                        do
                        {
                            if (newCard != null)
                            {
                                tcgraph.Document.Cache.SetValueExt<EPTimeCard.employeeID>(newCard, Employee.Current.BAccountID);
                                tcgraph.Document.Cache.SetValueExt<EPTimeCard.isReleased>(newCard, true);
                                tcgraph.Document.Cache.SetValueExt<EPTimeCard.isHold>(newCard, false);
                                tcgraph.Document.Cache.SetValueExt<EPTimeCard.isApproved>(newCard, true);
                                tcgraph.Document.Insert(newCard);
                                tcgraph.Save.Press();
                            }
                            newCard = (EPTimeCard)tcgraph.Document.Cache.CreateInstance();
                            tcgraph.Document.Cache.SetValueExt<EPTimeCard.weekId>(newCard, tcgraph.GetNextWeekID(Employee.Current.BAccountID));
                        }
                        while (newCard.WeekStartDate < GenTimeCardFilter.Current.GenerateUntil);
                    });
                }
            }
            return adapter.Get();
        }

        public PXAction<EPEmployee> viewContact;
        [PXUIField(DisplayName = "View Contact", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
        public virtual IEnumerable ViewContact(PXAdapter adapter)
        {
            Contact contact = Contact.Current;
            if (contact != null)
            {
                Save.Press();
                ContactMaint graph = CreateInstance<ContactMaint>();
                graph.Contact.Current = graph.Contact.Search<Contact.contactID>(contact.ContactID);
                throw new PXRedirectRequiredException(graph, "View Contact");
            }
            return adapter.Get();
        }

        public PXSave<EPEmployee> Save;
		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual System.Collections.IEnumerable Cancel(PXAdapter a)
		{
			foreach (PXResult<EPEmployee> res in (new PXCancel<EPEmployee>(this, "Cancel")).Press(a))
			{
				EPEmployee e = res;
				if (Employee.Cache.GetStatus(e) == PXEntryStatus.Inserted)
				{
					BAccount e1 = BAccount.Select(e.AcctCD);
					if (e1 != null)
					{
						Employee.Cache.RaiseExceptionHandling<EPEmployee.acctCD>(e, null, new PXSetPropertyException(Messages.BAccountExists));
					}
				}
				yield return e;
			}
		}
		public PXAction<EPEmployee> cancel;
		public PXInsert<EPEmployee> Insert;
		public PXCopyPasteAction<EPEmployee> Edit; 
		public PXDelete<EPEmployee> Delete;
		public PXFirst<EPEmployee> First;
		public PXPrevious<EPEmployee> Prev;
		public PXNext<EPEmployee> Next;
		public PXLast<EPEmployee> Last;
		public PXMenuAction<EPEmployee> Action;		

		public PXAction<EPEmployee> detachUser;
		[PXButton]
		[PXUIField(DisplayName = "Detach Employee Login")]
		protected virtual IEnumerable DetachUser(PXAdapter a)
		{
			EPEmployee employee = Employee.Current;
			Contact contact = Contact.Select();
			if (employee != null && contact != null)
			{ 
                if (AssigmentAndApprovalMaps.Select() != null)
                {
                    if (this.AssigmentAndApprovalMaps.Ask(Messages.EmployeeInAssigmentAndApprovalMap, MessageButtons.OKCancel)
                    == WebDialogResult.OK)
                    {
                        contact = PXCache<Contact>.CreateCopy(contact);
                        contact.UserID = null;
                        Contact.Update(contact);

                        employee = PXCache<EPEmployee>.CreateCopy(employee);
                        employee.UserID = null;
                        Employee.Update(employee);
                        return a.Get();
                    }
                    return a.Get();
                }

                if (CompanyTree.Select() != null)
                {
                    if (this.AssigmentAndApprovalMaps.Ask(Messages.EmployeeInCompanyTree, MessageButtons.OKCancel)
                    == WebDialogResult.OK)
                    {
                        contact = PXCache<Contact>.CreateCopy(contact);
                        contact.UserID = null;
                        Contact.Update(contact);

                        employee = PXCache<EPEmployee>.CreateCopy(employee);
                        employee.UserID = null;
                        Employee.Update(employee);
                        return a.Get();
                    }
                    return a.Get();
                }

                contact = PXCache<Contact>.CreateCopy(contact);
                contact.UserID = null;
                Contact.Update(contact);

                employee = PXCache<EPEmployee>.CreateCopy(employee);
                employee.UserID = null;
                Employee.Update(employee);                
            }
            return a.Get();
        }

        public PXAction<EPEmployee> viewMap;
        [PXUIField(DisplayName = "View Assignment Map", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable ViewMap(PXAdapter adapter)
        {
            EPRule ePRule = AssigmentAndApprovalMaps.Current;
            if (ePRule != null)
            {
                EPAssignmentMapMaint graph1 = PXGraph.CreateInstance<EPAssignmentMapMaint>();
                EPApprovalMapMaint graph2 = PXGraph.CreateInstance<EPApprovalMapMaint>();

                EPAssignmentMap current1 = graph1.AssigmentMap.Search<EPAssignmentMap.assignmentMapID>(ePRule.AssignmentMapID);                
                if (current1 != null)
                {
                    graph1.AssigmentMap.Current = current1;
                    throw new PXRedirectRequiredException(graph1, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }

                EPAssignmentMap current2 = graph2.AssigmentMap.Search<EPAssignmentMap.assignmentMapID>(ePRule.AssignmentMapID);
                if (current2 != null)
                {
                    graph2.AssigmentMap.Current = current2;
                    throw new PXRedirectRequiredException(graph2, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }  
            }            
            return adapter.Get();
        }

        public PXChangeID<EPEmployee, EPEmployee.acctCD> ChangeID;

		public PXMenuAction<EPEmployee> inquiry;
		[PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.InquiriesFolder)]
		[PXUIField(DisplayName = "Inquiries")]
		public virtual IEnumerable Inquiry(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<EPEmployee> laborCostRates;
		[PXUIField(DisplayName = "Labor Cost Rates", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable LaborCostRates(PXAdapter adapter)
		{
			if (Employee.Current != null)
			{
				LaborCostRateMaint graph = PXGraph.CreateInstance<LaborCostRateMaint>();
				graph.Filter.Current.EmployeeID = Employee.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Labor Cost Rates");
			}
			return adapter.Get();
		}

		#endregion

		#region EPEmployee Events

		[PXDBGuid()]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.Invisible)]
		protected virtual void BAccount2_OwnerID_CacheAttached(PXCache sender)
		{
			
		}

		[PXSelector(typeof(Users.pKID), SubstituteKey = typeof(Users.username), DescriptionField = typeof(Users.fullName), CacheGlobal = true, DirtyRead = true)]
		[PXUIField(DisplayName = "Employee Login", Visibility = PXUIVisibility.Visible)]
		[PXDBGuid]
		protected virtual void EPEmployee_UserID_CacheAttached(PXCache cache)
		{
			
		}        

        protected void ApplyUserAccessRights(params PXSelectBase[] datamembers)
		{
			AccessUsers graph;
			string targetScreenID = "SM201010"/*PXSiteMap.Provider.FindSiteMapNode(typeof (AccessUsers)).ScreenID*/;
			string screenID = PXContext.GetScreenID();
			using (new PXPreserveScope())
			{
				try
				{
					PXContext.SetScreenID(targetScreenID);
					graph = CreateInstance<AccessUsers>();
				}
				finally
				{
					PXContext.SetScreenID(screenID);
				}
			}
			graph.UserList.Current = PXSelect<Users, Where<Users.pKID, Equal<Required<EPEmployee.userID>>>>.Select(graph, Accessinfo.UserID);
			foreach (PXView view in datamembers.Select(sb => sb.View))
			{
				PXCache src = graph.Caches[view.CacheGetItemType()];

				view.AllowSelect = src.AllowSelect;
				view.AllowInsert = src.AllowInsert;
				view.AllowUpdate = src.AllowUpdate;
				view.AllowDelete = src.AllowUpdate;
			}
		}

		protected virtual void EPEmployee_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPEmployee row = (EPEmployee) e.Row;

			var mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();

			PXUIFieldAttribute.SetVisible<EPEmployee.curyID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<EPEmployee.allowOverrideCury>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<EPEmployee.allowOverrideRate>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetRequired<EPEmployeeClass.termsID>(cache, false);
			PXUIFieldAttribute.SetDisplayName<Contact.displayName>(Contact.Cache, Messages.EmployeeContact);

			bool isEmployeeExists = e.Row != null && cache.GetStatus(e.Row) != PXEntryStatus.Inserted;
			if (isEmployeeExists)
			{
				this.FillPaymentDetails();
			}

			this.CompanyTree.Cache.AllowInsert =
				this.CompanyTree.Cache.AllowUpdate =
				this.CompanyTree.Cache.AllowDelete = row != null && row.UserID != null;

            generateTimeCards.SetEnabled(row != null && row.UserID != null);
			laborCostRates.SetEnabled(isEmployeeExists);

            if (row == null) return;
			
			detachUser.SetEnabled(row.UserID != null);

			bool isUserInserted = row.UserID == null || User.Cache.GetStatus(User.Current) == PXEntryStatus.Inserted;
			bool hasLoginType = isUserInserted && User.Current != null && User.Current.LoginTypeID != null;
			PXUIFieldAttribute.SetEnabled<Users.loginTypeID>(User.Cache, User.Current, isUserInserted);
			PXUIFieldAttribute.SetEnabled<Users.username>(User.Cache, User.Current, hasLoginType);
			PXUIFieldAttribute.SetEnabled<Users.password>(User.Cache, User.Current, hasLoginType);

			PXDefaultAttribute.SetPersistingCheck<Users.username>(User.Cache, User.Current, hasLoginType ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<Users.username>(User.Cache, hasLoginType);

			PXUIFieldAttribute.SetRequired<Users.password>(User.Cache, hasLoginType);

			PXDefaultAttribute.SetPersistingCheck<Contact.eMail>(Contact.Cache, Contact.Current, hasLoginType ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<Contact.eMail>(Contact.Cache, hasLoginType);

			User.Current = (Users)User.View.SelectSingleBound(new[] { e.Row });
			User.ToggleActions(User.Cache, User.Current);
		}

	    protected virtual void Users_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
	    {
            Users row = (Users)e.Row;
	        if (row == null) return;

            bool isUserInserted = User.Cache.GetStatus(User.Current) == PXEntryStatus.Inserted;
            bool hasLoginType = isUserInserted && User.Current != null && User.Current.LoginTypeID != null;
            PXUIFieldAttribute.SetEnabled<Users.loginTypeID>(User.Cache, User.Current, isUserInserted);
            PXUIFieldAttribute.SetEnabled<Users.username>(User.Cache, User.Current, hasLoginType);
            PXUIFieldAttribute.SetEnabled<Users.password>(User.Cache, User.Current, hasLoginType);

            PXDefaultAttribute.SetPersistingCheck<Users.username>(User.Cache, User.Current, hasLoginType ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            PXUIFieldAttribute.SetRequired<Users.username>(User.Cache, hasLoginType);
	    }

	    protected virtual void EPEmployee_DefLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{

		}
		protected virtual void EPEmployee_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
			{
				if (companySetup.Current == null || string.IsNullOrEmpty(companySetup.Current.BaseCuryID))
				{
					throw new PXException();
				}
				e.NewValue = companySetup.Current.BaseCuryID;
				e.Cancel = true;
			}
		}
		protected virtual void EPEmployee_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			EPEmployee row = (EPEmployee)e.Row;

			using (new ReadOnlyScope(Address.Cache, Contact.Cache, DefLocation.Cache))
			{
				Address addr = new Address {BAccountID = Employee.Current.ParentBAccountID};
				addr = (Address)Address.Cache.Insert(addr);

				Contact cont = new Contact {BAccountID = Employee.Current.ParentBAccountID};
				cont = (Contact)Contact.Cache.Insert(cont);

				cont.Phone1Type = "H1";
				cont.Phone2Type = "C";
				cont.Phone3Type = "B1";
				cont.FaxType = "HF";
				cont.DefAddressID = addr.AddressID;

				Employee.Current.DefContactID = cont.ContactID;
				Employee.Current.DefAddressID = addr.AddressID;
				Employee.Current.AcctName = cont.DisplayName;

                foreach (PXResult<CRLocation, GL.Branch, BAccount2> parent in PXSelectJoin<CRLocation,
                    InnerJoin<GL.Branch, On<GL.Branch.bAccountID, Equal<CRLocation.bAccountID>>,
                    InnerJoin<BAccount2, On<BAccount2.bAccountID, Equal<CRLocation.bAccountID>, And<BAccount2.defLocationID, Equal<CRLocation.locationID>>>>>,
                    Where<BAccount2.bAccountID, Equal<Current<EPEmployee.parentBAccountID>>>>.Select(this))
                {
                    Contact.Cache.SetValueExt<Contact.fullName>(cont, ((BAccount2)parent).AcctName);
                    Location defLoc = new Location
                    {
	                    BAccountID = ((EPEmployee) e.Row).BAccountID,
	                    LocType = LocTypeList.EmployeeLoc,
	                    Descr = PXMessages.LocalizeNoPrefix(CR.Messages.DefaultLocationDescription),
	                    VTaxZoneID = ((CRLocation) parent).VTaxZoneID,
	                    VBranchID = ((GL.Branch) parent).BranchID
                    };

                    object newValue = PXMessages.LocalizeNoPrefix(CR.Messages.DefaultLocationCD);
                    DefLocation.Cache.RaiseFieldUpdating<Location.locationCD>(defLoc, ref newValue);
                    defLoc.LocationCD = (string)newValue;


                    defLoc = DefLocation.Insert(defLoc);

                    if (defLoc != null)
                    {
                        defLoc.VAPAccountLocationID = defLoc.LocationID;
                        defLoc.VPaymentInfoLocationID = defLoc.LocationID;
                        defLoc.CARAccountLocationID = defLoc.LocationID;

                        defLoc.VDefAddressID = addr.AddressID;
                        defLoc.VDefContactID = cont.ContactID;
                        defLoc.DefAddressID = addr.AddressID;
                        defLoc.DefContactID = cont.ContactID;
                        defLoc.VRemitAddressID = addr.AddressID;
                        defLoc.VRemitContactID = cont.ContactID;

                        cache.SetValue<EPEmployee.defLocationID>(e.Row, defLoc.LocationID);
                    }
                }

				FillPaymentDetails();
			}
		}

		protected virtual void EPEmployee_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPEmployee employee = e.Row as EPEmployee;

			if (employee == null)
				return;

			if (!sender.ObjectsEqual<EPEmployee.parentBAccountID>(e.Row, e.OldRow))
			{
				var parent = (PX.Objects.GL.Branch)PXSelectorAttribute.Select<EPEmployee.parentBAccountID>(sender, e.Row);

				if (employee.ParentBAccountID != null)
				{
					BranchView.Cache.SetStatus(parent, PXEntryStatus.Updated);
				}
				
				bool found = false;
				foreach (Address addr in Address.Cache.Inserted)
				{
					addr.BAccountID = employee.ParentBAccountID;
					addr.CountryID = company.Current.CountryID;
					found = true;
				}

				if (!found)
				{
					Address addr = (Address)Address.View.SelectSingleBound(new object[] { e.OldRow }) ?? new Address();

					addr.BAccountID = employee.ParentBAccountID ?? addr.BAccountID;
					addr.CountryID = company.Current.CountryID;
					Address.Cache.Update(addr);

				}

				found = false;
				foreach (Contact cont in Contact.Cache.Inserted)
				{
					cont.FullName = parent?.AcctName;
					cont.BAccountID = employee.ParentBAccountID;
					cont.DefAddressID = null;
					foreach (Address record in Address.Cache.Inserted)
					{
						cont.DefAddressID = record.AddressID;
					} 
					found = true;
				}

				if (!found)
				{
					Contact cont = (Contact)Contact.View.SelectSingleBound(new object[] { e.OldRow }) ?? new Contact();
					cont.FullName = parent?.AcctName;
					cont.BAccountID = employee.ParentBAccountID ?? cont.BAccountID;
					cont.DefAddressID = null;
					foreach (Address record in Address.Cache.Inserted)
					{
						cont.DefAddressID = record.AddressID;
					}
					Contact.Cache.Update(cont);
				}

				found = false;
				foreach (Location loc in DefLocation.Cache.Inserted)
				{
					loc.VBranchID = company.Current.BranchID;
					foreach (Address record in Address.Cache.Inserted)
					{
						loc.DefAddressID = record.AddressID;
					}

					foreach (Contact record in Contact.Cache.Inserted)
					{
						loc.DefContactID = record.ContactID;
					}
					found = true;
				}

				if (!found)
				{
					Location loc = (Location)DefLocation.View.SelectSingleBound(new object[] { e.Row });

					loc.VBranchID = company.Current.BranchID;
					foreach (Address record in Address.Cache.Inserted)
					{
						loc.DefAddressID = record.AddressID;
						loc.VRemitAddressID = record.AddressID;
					}

					foreach (Contact record in Contact.Cache.Inserted)
					{
						loc.DefContactID = record.ContactID;
						loc.VRemitContactID = record.ContactID;
					}
					DefLocation.Cache.Update(loc);
				}
			}
		}

        protected virtual void EPEmployee_VendorClassID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var row = (EPEmployee)e.Row;
            var vc = (VendorClass)PXSelectorAttribute.Select<VendorClass.vendorClassID>(cache, row, e.NewValue);
            this.doCopyClassSettings = false;
            if (vc != null)
            {
                this.doCopyClassSettings = true;
                if (cache.GetStatus(row) != PXEntryStatus.Inserted)
                {
                    if (Employee.Ask(Messages.Warning, Messages.EmployeeClassChangeWarning, MessageButtons.YesNo) == WebDialogResult.No)
                    {
                        this.doCopyClassSettings = false;
                    }
                }
            }
        }

		protected virtual void EPEmployee_VendorClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EmployeeClass.RaiseFieldUpdated(sender, e.Row);

			EPEmployee row = (EPEmployee)e.Row;
			if (row.VendorClassID != null && this.doCopyClassSettings)
			{
				Location defLoc = DefLocation.Current ?? DefLocation.Select();

				sender.SetDefaultExt<EPEmployee.curyID>(e.Row);
				sender.SetDefaultExt<EPEmployee.curyRateTypeID>(e.Row);
				sender.SetDefaultExt<EPEmployee.allowOverrideCury>(e.Row);
				sender.SetDefaultExt<EPEmployee.allowOverrideRate>(e.Row);
				sender.SetDefaultExt<EPEmployee.calendarID>(e.Row);
				sender.SetDefaultExt<EPEmployee.termsID>(e.Row);

				sender.SetDefaultExt<EPEmployee.salesAcctID>(e.Row);
				sender.SetDefaultExt<EPEmployee.salesSubID>(e.Row);
				sender.SetDefaultExt<EPEmployee.expenseAcctID>(e.Row);
				sender.SetDefaultExt<EPEmployee.expenseSubID>(e.Row);
				sender.SetDefaultExt<EPEmployee.prepaymentAcctID>(e.Row);
				sender.SetDefaultExt<EPEmployee.prepaymentSubID>(e.Row);
				sender.SetDefaultExt<EPEmployee.discTakenAcctID>(e.Row);
				sender.SetDefaultExt<EPEmployee.discTakenSubID>(e.Row);
				sender.SetDefaultExt<EPEmployee.hoursValidation>(e.Row);

				DefLocation.Cache.SetDefaultExt<Location.vAPAccountID>(defLoc);
				DefLocation.Cache.SetDefaultExt<Location.vAPSubID>(defLoc);
				DefLocation.Cache.SetDefaultExt<Location.vTaxZoneID>(defLoc);
				DefLocation.Cache.SetDefaultExt<Location.vCashAccountID>(defLoc);
				DefLocation.Cache.SetDefaultExt<Location.vPaymentMethodID>(defLoc);
			}
		}

		protected virtual void EPEmployee_UserID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null)
			{
				EPEmployee row = (EPEmployee)e.Row;
				EPEmployee ep = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(this, e.NewValue);
				if (ep != null && ep.AcctCD != row.AcctCD)
				{
					Users us = PXSelect<Users, Where<Users.pKID, Equal<Required<Users.pKID>>>>.Select(this, e.NewValue);

					Employee.Cache.RaiseExceptionHandling<EPEmployee.userID>(e.Row, us.Username, new PXSetPropertyException(Messages.EmployeeLoginExists, ep.AcctCD, ep.AcctName));

					e.NewValue = null;
				}
				else
				{
					this.CompanyTree.Cache.Clear();
				}
			}
		}

		protected virtual void EPEmployee_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EPEmployee doc = (EPEmployee)e.Row;

			if (string.IsNullOrEmpty(doc.TermsID) == false && e.Operation != PXDBOperation.Delete)
			{
				Terms terms = (Terms)PXSelectorAttribute.Select<EPEmployee.termsID>(Employee.Cache, doc);

				if (terms != null && terms.DiscPercent.HasValue && terms.DiscPercent.Value != decimal.Zero)
				{
					if (sender.RaiseExceptionHandling<EPEmployee.termsID>(e.Row, doc.TermsID, new PXSetPropertyException(Messages.EmployeeTermsCannotHaveCashDiscounts, typeof(EPEmployee.termsID).Name)))
					{
						throw new PXRowPersistingException(typeof(EPEmployee.termsID).Name, doc.TermsID, Messages.EmployeeTermsCannotHaveCashDiscounts, typeof(EPEmployee.termsID).Name);
					}
				}

				if (terms != null && terms.InstallmentType == CS.TermsInstallmentType.Multiple)
				{
					if (sender.RaiseExceptionHandling<EPEmployee.termsID>(e.Row, doc.TermsID, new PXSetPropertyException(Messages.EmployeeTermsCannotHaveMultiplyInstallments, typeof(EPEmployee.termsID).Name)))
					{
						throw new PXRowPersistingException(typeof(EPEmployee.termsID).Name, doc.TermsID, Messages.EmployeeTermsCannotHaveMultiplyInstallments, typeof(EPEmployee.termsID).Name);
					}
				}
			}
		}

		protected virtual void EPEmployee_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			EPEmployee employee = e.Row as EPEmployee;
			if (!string.IsNullOrEmpty(employee.AcctCD)
				&& employee.Status != CR.BAccount.status.Inactive
				&& sender.GetStatus(e.Row) != PXEntryStatus.InsertedDeleted)
			{
				e.Cancel = true;
				throw new PXException(EP.Messages.DisableEmployeeBeforeDeleting);
			}
		}

		protected virtual void EPEmployee_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			EPEmployee employee = e.Row as EPEmployee;

			Users user = PXSelect<Users, Where<Users.pKID, Equal<Required<EPEmployee.userID>>>>.Select(this, employee.UserID);
			if (user != null)
			{
				user.LoginTypeID = null;
				user.IsApproved = false;
				User.Update(user);
			}
		}

		#endregion

		#region Location Events

		protected virtual void Location_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Employee.Cache.GetValue<EPEmployee.bAccountID>(Employee.Current);
			e.Cancel = true;
		}

		protected virtual void Location_LocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Location parent = PXSelect<Location, Where<Location.bAccountID, Equal<Current<EPEmployee.parentBAccountID>>, And<Location.locationCD, Equal<Current<Location.locationCD>>>>>.SelectMultiBound(this, new object[] { e.Row });

			if (parent != null)
			{
				e.NewValue = LocTypeList.EmployeeLoc;
				e.Cancel = true;
			}
		}

		protected virtual void Location_CBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void Location_CARAccountLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = sender.GetValue<Location.locationID>(e.Row);
		}

		protected virtual void Location_VAPAccountLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = sender.GetValue<Location.locationID>(e.Row);
		}

		protected virtual void Location_VPaymentInfoLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = sender.GetValue<Location.locationID>(e.Row);
		}

		protected virtual void Location_VDefAddressID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Employee.Cache.GetValue<EPEmployee.defAddressID>(Employee.Current);
			e.Cancel = true;
		}

		protected virtual void Location_VDefContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Employee.Cache.GetValue<EPEmployee.defContactID>(Employee.Current);
			e.Cancel = true;
		}

		protected virtual void Location_VRemitAddressID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Employee.Cache.GetValue<EPEmployee.defAddressID>(Employee.Current);
			e.Cancel = true;
		}

		protected virtual void Location_VRemitContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Employee.Cache.GetValue<EPEmployee.defContactID>(Employee.Current);
			e.Cancel = true;
		}

		protected virtual void Location_VAPAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = EmployeeClass.Cache.GetValue<EPEmployeeClass.aPAcctID>(EmployeeClass.Current);
			e.Cancel = (EmployeeClass.Current != null);
		}

		protected virtual void Location_VAPSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = EmployeeClass.Cache.GetValue<EPEmployeeClass.aPSubID>(EmployeeClass.Current);
			e.Cancel = (EmployeeClass.Current != null);
		}

		protected virtual void Location_VTaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = EmployeeClass.Cache.GetValue<EPEmployeeClass.taxZoneID>(EmployeeClass.Current);
			e.Cancel = (EmployeeClass.Current != null);
		}

		protected virtual void Location_VCashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Location row = (Location)e.Row;
			if (row != null )
			{
				EPEmployeeClass epClass = this.EmployeeClass.Current;
				if (epClass!= null && epClass.CashAcctID.HasValue 
					&& row.VPaymentMethodID == epClass.PaymentMethodID)
				{
					e.NewValue = epClass.CashAcctID;
					e.Cancel = true;
				}
				else
				{
					e.NewValue = null;
					e.Cancel = true;
				}
			}			
		}

		protected virtual void Location_VPaymentMethodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = EmployeeClass.Cache.GetValue<EPEmployeeClass.paymentMethodID>(EmployeeClass.Current);
			e.Cancel = (EmployeeClass.Current != null);
		}

		protected virtual void Location_VPaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Location row = (Location)e.Row;
			this.isPaymentMergedFlag = false;
			this.FillPaymentDetails(row);
			cache.SetDefaultExt<Location.vCashAccountID>(e.Row);
			this.PaymentDetails.View.RequestRefresh();
		}

		protected virtual void Location_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Location row = (Location)e.Row;
			if (row != null) 
			{
				FillPaymentDetails(row);				
				PXUIFieldAttribute.SetEnabled<Location.vCashAccountID>(sender, e.Row, (String.IsNullOrEmpty(row.VPaymentMethodID) == false));

				PXUIFieldAttribute.SetRequired<Location.vAPAccountID>(sender, false);
				PXUIFieldAttribute.SetRequired<Location.vAPSubID>(sender, false);
			}

		}

		object _KeyToAbort = null;
		protected virtual void Location_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					if ((int?)sender.GetValue<Location.vAPAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VAPAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<Location.vAPAccountLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<Location.vPaymentInfoLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VPaymentInfoLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<Location.vPaymentInfoLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<Location.cARAccountLocationID>(e.Row) < 0)
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
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<Location.vAPAccountLocationID>(e.Row, KeyAborted);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<Location.vPaymentInfoLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<Location.vPaymentInfoLocationID>(e.Row, KeyAborted);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<Location.cARAccountLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<Location.cARAccountLocationID>(e.Row, KeyAborted);
						}
					}
					_KeyToAbort = null;
				}
			}
		}

		#endregion

		#region Other Events
		
		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(Location.bAccountID))]
		[PXUIField(DisplayName = "BAccountID", Visible = false, Enabled = false)]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<VendorPaymentMethodDetail.bAccountID>>>>))]
		protected virtual void VendorPaymentMethodDetail_BAccountID_CacheAttached(PXCache sender)
		{

		}

		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(Location.locationID))]
		[PXUIField(Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<VendorPaymentMethodDetail.bAccountID>>, And<Location.locationID, Equal<Current<VendorPaymentMethodDetail.locationID>>>>>))]
		protected virtual void VendorPaymentMethodDetail_LocationID_CacheAttached(PXCache sender)
		{

		}

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault(typeof(Search<Location.vPaymentMethodID, Where<Location.bAccountID, Equal<Current<VendorPaymentMethodDetail.bAccountID>>, And<Location.locationID, Equal<Current<VendorPaymentMethodDetail.locationID>>>>>))]
		[PXUIField(DisplayName = "Payment Method", Visible = false)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID>))]
		protected virtual void VendorPaymentMethodDetail_PaymentMethodID_CacheAttached(PXCache sender)
		{

		}
		
        [PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "ID", Visible = false, Enabled = false)]
		[PXSelector(typeof(Search<PaymentMethodDetail.detailID, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<VendorPaymentMethodDetail.paymentMethodID>>,
                        And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                        Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>), DescriptionField = typeof(PaymentMethodDetail.descr))]
		protected virtual void VendorPaymentMethodDetail_DetailID_CacheAttached(PXCache sender)
		{

		}

		protected virtual void EPEmployeeCompanyTreeMember_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPEmployeeCompanyTreeMember row = e.Row as EPEmployeeCompanyTreeMember;
			EPEmployeeCompanyTreeMember oldRow = e.OldRow as EPEmployeeCompanyTreeMember;
			if (row != null && oldRow != null && row.IsOwner == true && oldRow.IsOwner != true)
			{
				foreach (EPEmployeeCompanyTreeMember member in
				PXSelect<EPEmployeeCompanyTreeMember,
					Where<EPEmployeeCompanyTreeMember.workGroupID, Equal<Required<EPEmployeeCompanyTreeMember.workGroupID>>,
						And<EPEmployeeCompanyTreeMember.isOwner, Equal<boolTrue>>>>.Select(this, row.WorkGroupID))
				{
					if (member.UserID != row.UserID)
					{
						EPEmployeeCompanyTreeMember upd = PXCache<EPEmployeeCompanyTreeMember>.CreateCopy(member);
						upd.IsOwner = false;
						this.CompanyTree.Update(upd);
					}
				}
			}
		}

		#endregion

		#region Contact Events

        [PXDBIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
        [PXParent(typeof(Select<BAccount, Where<BAccount.defContactID, Equal<Current<Contact.contactID>>>>))]
		protected virtual void Contact_ContactID_CacheAttached(PXCache sender)
		{

		}

		[PXDBGuid]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.Invisible)]
		protected virtual void Contact_OwnerID_CacheAttached(PXCache sender)
		{

		}

		[PXDBString(2, IsFixed = true)]
		[PXDefault(ContactTypesAttribute.Employee)]
		protected virtual void Contact_ContactType_CacheAttached(PXCache sender)
		{

		}
        
        [PXDBString(100, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Last Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		protected virtual void Contact_LastName_CacheAttached(PXCache sender)
        {

        }

		protected virtual void Contact_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void Contact_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Employee.Current != null)
			{
				e.NewValue = this.Employee.Current.ParentBAccountID;
				e.Cancel = true;
			}
		}

		protected virtual void Contact_Salutation_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPEmployeePosition position = PXSelect<EPEmployeePosition,Where<EPEmployeePosition.employeeID, Equal<Current<EPEmployee.bAccountID>>,
								And<EPEmployeePosition.isActive, Equal<True>>>>.Select(this);

			if (position != null)
				e.NewValue = ((EPPosition)PXSelectorAttribute.Select<EPEmployeePosition.positionID>(Employee.Cache, position)).With(_ => _.Description);
		}

		protected virtual void EPEmployee_PositionID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contact.Cache.SetDefaultExt<Contact.salutation>(Contact.Current);
		}

		protected virtual void Contact_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Contact cont = e.Row as Contact;
			if (cont == null) return;

			Employee.Current.DefContactID = cont.ContactID;
		}

		protected virtual void Contact_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Contact cont = (Contact)e.Row;
			if (cont == null) return;

			bool isUserInserted = cont.UserID == null || User.Cache.GetStatus(User.Current) == PXEntryStatus.Inserted;
			bool hasLoginType = isUserInserted && User.Current != null && User.Current.LoginTypeID != null;

			PXDefaultAttribute.SetPersistingCheck<Contact.eMail>(cache, cont, hasLoginType && User.Current.Username != null ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<Contact.eMail>(cache, hasLoginType && User.Current.Username != null);
		}

		protected virtual void Contact_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Contact cont = (Contact)e.Row;
			//cont.FullName = cont.DisplayName;
			Employee.SetValueExt<EPEmployee.acctName>(Employee.Current, cont.DisplayName);
		}

		protected virtual void Contact_Email_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contact contact = (Contact)e.Row;

			foreach (EMailSyncAccount syncAccount in SyncAccount.Select(contact.ContactID)
					   .RowCast<EMailSyncAccount>()
					   .Select(account => (EMailSyncAccount)SyncAccount.Cache.CreateCopy(account)))
			{
				syncAccount.Address = contact.EMail;

				syncAccount.ContactsExportDate = null;
				syncAccount.ContactsImportDate = null;
				syncAccount.EmailsExportDate = null;
				syncAccount.EmailsImportDate = null;
				syncAccount.TasksExportDate = null;
				syncAccount.TasksImportDate = null;
				syncAccount.EventsExportDate = null;
				syncAccount.EventsImportDate = null;

				EMailAccount mailAccount = EMailAccounts.Select(syncAccount.EmailAccountID);
                if(mailAccount != null)
			    {
				mailAccount.Address = syncAccount.Address;
				EMailAccounts.Update(mailAccount);
			    }
				SyncAccount.Update(syncAccount);

			}
		}

		protected virtual void Contact_DefAddressID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion

		#region Address Envents

		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.bAccountID, Equal<Current<EPEmployee.parentBAccountID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Address_CountryID_CacheAttached(PXCache sender)
		{
		}


		[PXDefault(typeof(IsNull<Current<RedirectEmployeeParameters.parentBAccountID>, CurrentBranchBAccountID>))]
		[PXDBInt]
		[PXUIField(DisplayName = "Branch")]
		[PXDimensionSelector("BIZACCT", typeof(Search<GL.Branch.bAccountID, Where<GL.Branch.active, Equal<True>, And<MatchWithBranch<GL.Branch.branchID>>>>), typeof(Branch.branchCD), DescriptionField = typeof(GL.Branch.acctName))]
		protected virtual void EPEmployee_ParentBAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(true, typeof(RedirectEmployeeParameters.routeEmails))]
		[PXDBBool]
		[PXUIField(DisplayName = "Route Emails")]
		protected virtual void EPEmployee_RouteEmails_CacheAttached(PXCache sender)
		{
		}

		protected virtual void Address_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void Address_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Employee.Current != null)
			{
				e.NewValue = this.Employee.Current.ParentBAccountID;
				e.Cancel = true;
			}
		}

		protected virtual void Address_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Address addr = e.Row as Address;
			if (addr != null)
			{
				Employee.Current.DefAddressID = addr.AddressID;
			}
		}

        protected virtual void Address_CountryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            Address addr = (Address)e.Row;
            if ((string)e.OldValue != addr.CountryID)
            {
                addr.State = null;
            }
        }


		#endregion

		#region Location Events
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		protected virtual void Location_DefAddressID_CacheAttached(PXCache sender)
		{ 
		}

		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		protected virtual void Location_DefContactID_CacheAttached(PXCache sender)
		{ 
		}
		#endregion

		// for correct redirect to UserMaint (#34819).
		[PXInt]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(Search2<Contact.contactID, LeftJoin<Users, On<Contact.userID, Equal<Users.pKID>>>,
			Where<Current<Users.guest>, Equal<True>, And<Contact.contactType, Equal<ContactTypesAttribute.person>,
			Or<Current<Users.guest>, NotEqual<True>, And<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>>>),
			typeof(Contact.displayName),
			typeof(Contact.salutation),
			typeof(Contact.fullName),
			typeof(Contact.eMail),
			typeof(Users.username),
			SubstituteKey = typeof(Contact.displayName))]
		[PXRestrictor(typeof(Where<Contact.eMail, IsNotNull, Or<Current<Users.source>, Equal<PXUsersSourceListAttribute.activeDirectory>>>), PX.Objects.CR.Messages.ContactWithoutEmail, typeof(Contact.displayName))]
		[PXDBScalar(typeof(Search<Contact.contactID, Where<Contact.userID, Equal<Users.pKID>>>))]
		protected virtual void Users_ContactID_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid(IsKey = true)]
		[PXDefault]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		public virtual void Users_PKID_CacheAttached(PXCache sender) { }

		//The Following code exists only for ContractBasedAPI backward compatibility.
		[PXDBString(1)]
		[PXDefault(PMLaborCostRateType.Employee)]
		[PXUIField(DisplayName = "Labor Rate Type")]
		public virtual void PMLaborCostRate_Type_CacheAttached(PXCache sender) { }

		//The Following code exists only for ContractBasedAPI backward compatibility.
		[PXDBDefault(typeof(EPEmployee.bAccountID))]
		[PXDBInt()]
		[PXUIField(DisplayName = "Employee")]
		public virtual void PMLaborCostRate_EmployeeID_CacheAttached(PXCache sender) { }

		[PXDBString(64, IsUnicode = true, InputMask = ""/*"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA||.'@-_"*/)]
		[PXDefault]
		[PXUIField(DisplayName = "Login")]
		[PXUIRequired(typeof(Where<Users.loginTypeID, IsNotNull, And<EntryStatus, Equal<EntryStatus.inserted>>>))]
		public virtual void Users_Username_CacheAttached(PXCache sender) { }

		[PXDBInt]
		[PXUIField(DisplayName = "User Type")]
		[PXRestrictor(typeof(Where<EPLoginType.entity, Equal<EPLoginType.entity.employee>>), CR.Messages.NonEmployeeLoginType, typeof(EPLoginType.loginTypeName))]
		[PXSelector(typeof(Search5<EPLoginType.loginTypeID, LeftJoin<EPManagedLoginType, On<EPLoginType.loginTypeID, Equal<EPManagedLoginType.loginTypeID>>,
								LeftJoin<Users, On<EPManagedLoginType.parentLoginTypeID, Equal<Users.loginTypeID>>,
								LeftJoin<ContactMaint.CurrentUser, On<ContactMaint.CurrentUser.pKID, Equal<Current<AccessInfo.userID>>>>>>,
								Where<Users.pKID, Equal<ContactMaint.CurrentUser.pKID>, And<ContactMaint.CurrentUser.guest, Equal<True>,
									Or<ContactMaint.CurrentUser.guest, NotEqual<True>>>>,
								Aggregate<GroupBy<EPLoginType.loginTypeID, GroupBy<EPLoginType.loginTypeName, GroupBy<EPLoginType.requireLoginActivation, GroupBy<EPLoginType.resetPasswordOnLogin>>>>>>),
			SubstituteKey = typeof(EPLoginType.loginTypeName))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void Users_LoginTypeID_CacheAttached(PXCache sender) { }

        [PXDBBool]
		[PXUIField(DisplayName = "Guest Account")]
		[PXFormula(typeof(Switch<Case<Where<Selector<Users.loginTypeID, EPLoginType.entity>, Equal<EPLoginType.entity.contact>>, True>, False>))]
		protected virtual void Users_Guest_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXFormula(typeof(Selector<Users.loginTypeID, EPLoginType.requireLoginActivation>))]
		protected virtual void Users_IsPendingActivation_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXUIField(DisplayName = "Force User to Change Password on Next Login")]
		[PXFormula(typeof(Switch<Case<Where<Selector<Users.loginTypeID, EPLoginType.resetPasswordOnLogin>, Equal<True>>, True>, False>))]
		protected virtual void Users_PasswordChangeOnNextLogin_CacheAttached(PXCache sender) { }

		[PXBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Generate Password")]
		protected virtual void Users_GeneratePassword_CacheAttached(PXCache sender) { }

		[PXDBString(64, IsKey = true, IsUnicode = true, InputMask = "")]
		[PXDefault(typeof(Users.username))]
		[PXParent(typeof(Select<Users, Where<Users.username, Equal<Current<UsersInRoles.username>>>>))]
		protected virtual void UsersInRoles_Username_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(EPEmployee.bAccountID))]
		protected virtual void _(Events.CacheAttached<EPEmployeeCorpCardLink.employeeID> e)
		{
		}

		protected virtual void EPEmployeePosition_IsTerminated_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
			EPEmployeePosition row = e.Row as EPEmployeePosition;
			if (row != null)
			{
				if (row.IsTerminated == true)
				{
					row.IsActive = false;
				}
				else
				{
					row.IsRehirable = false;
					row.TermReason = null;
				}
			}
		}

        protected void UncheckEmployeePositionsIsActive()
			{
					bool requestRefresh = false;

					PXSelectBase<EPEmployeePosition> selectSiblings = new PXSelect<EPEmployeePosition, 
						Where<EPEmployeePosition.employeeID, Equal<Current<EPEmployeePosition.employeeID>>,
						And<EPEmployeePosition.lineNbr, NotEqual<Current<EPEmployeePosition.lineNbr>>,
                        And<EPEmployeePosition.isActive, Equal<True>>>>>(this);

                    foreach (EPEmployeePosition ps in selectSiblings.Select())
					{
						EPEmployeePosition item = PXCache<EPEmployeePosition>.CreateCopy(ps);
						item.IsActive = false;
						EmployeePositions.Update(item);
						requestRefresh = true;
					}

					if (requestRefresh)
					{
						EmployeePositions.View.RequestRefresh();
					}
				}

        protected virtual void EPEmployeePosition_IsActive_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPEmployeePosition row = e.Row as EPEmployeePosition;
			if (row != null)
			{
				if (row.IsActive == true)
				{
					row.IsTerminated = false;
					row.TermReason = null;
					row.IsRehirable = false;

                    UncheckEmployeePositionsIsActive();
				}
			}
		}
		
		protected virtual void EPEmployeePosition_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			EPEmployeePosition row = e.Row as EPEmployeePosition;
			if (row != null)
			{
				PXSelectBase<EPEmployeePosition> selectActiveSiblings = new PXSelect<EPEmployeePosition,
						Where<EPEmployeePosition.employeeID, Equal<Current<EPEmployeePosition.employeeID>>,
						And<EPEmployeePosition.lineNbr, NotEqual<Current<EPEmployeePosition.lineNbr>>,
						And<EPEmployeePosition.isActive, Equal<True>>>>>(this);

                if (this.IsExport || this.IsImport)
                {
                    if (row.IsActive ?? false)
                    {
                        UncheckEmployeePositionsIsActive();
                    }
                }
                else
                {
				if (selectActiveSiblings.View.SelectMultiBound(new object[] { row }).Count == 0)
				{
					row.IsActive = true;
				}
				else
				{
					row.IsActive = false;
				}
			}
		}
		}

        protected virtual void GenTimeCardFilter_GenerateUntil_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            GenTimeCardFilter row = e.Row as GenTimeCardFilter;
            if (row == null) return;

            if (row.GenerateUntil < row.LastDateGenerated)
            {
                GenTimeCardFilter.Cache.RaiseExceptionHandling<GenTimeCardFilter.generateUntil>(GenTimeCardFilter.Current, GenTimeCardFilter.Current.GenerateUntil, new PXSetPropertyException(Messages.UntilGreaterFrom, PXErrorLevel.Error));
            }
            if (row.GenerateUntil > Accessinfo.BusinessDate)
            {
                GenTimeCardFilter.Cache.RaiseExceptionHandling<GenTimeCardFilter.generateUntil>(GenTimeCardFilter.Current, GenTimeCardFilter.Current.GenerateUntil, new PXSetPropertyException(Messages.UntilGreaterThanNow, PXErrorLevel.Warning));
            }
        }

        protected virtual void EPEmployeePosition_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPEmployeePosition row = (EPEmployeePosition) e.Row;
			if (row == null) return;

			if (row.EndDate < row.StartDate)
			{
				sender.RaiseExceptionHandling<EPEmployeePosition.endDate>(e.Row, ((EPEmployeePosition)e.Row).EndDate, new PXSetPropertyException(Messages.EndDateMustBeGreaterOrEqualToTheStartDate, PXErrorLevel.Error));
			}
		}

		#region Utility Functions

		
		public static Guid? GetCurrentEmployeeID(PXGraph graph)
		{
            Simple.EPEmployee emp = PXSelect<Simple.EPEmployee, Where<Simple.EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.SelectSingleBound(graph, null);
		    return (emp != null)? emp.UserID : null;
		}

		public static EPEmployee GetCurrentEmployee(PXGraph graph)
		{
			PXSelectReadonly<EPEmployee,
				Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.
				Clear(graph);
			var set = PXSelectReadonly<EPEmployee,
				Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.
				Select(graph);
			return set == null || set.Count == 0 ? null : (EPEmployee)set[0][typeof(EPEmployee)];
		}

		protected virtual void FillPaymentDetails()
		{
			Location account = this.DefLocation.Current ?? DefLocation.Select();
		}

		protected virtual void FillPaymentDetails(Location account)
		{
			if (account != null)
			{
				if (!isPaymentMergedFlag)
				{
					if (!string.IsNullOrEmpty(account.VPaymentMethodID))
					{
						List<PaymentMethodDetail> toAdd = new List<PaymentMethodDetail>();
						foreach (PaymentMethodDetail it in this.PaymentTypeDetails.Select())
						{
							VendorPaymentMethodDetail detail = null;
							foreach (VendorPaymentMethodDetail iPDet in this.PaymentDetails.Select())
							{
								if (iPDet.DetailID == it.DetailID)
								{
									detail = iPDet;
									break;
								}
							}
							if (detail == null)
							{
								toAdd.Add(it);
							}
						}
						using (ReadOnlyScope rs = new ReadOnlyScope(this.PaymentDetails.Cache))
						{
							foreach (PaymentMethodDetail it in toAdd)
							{
								VendorPaymentMethodDetail detail = new VendorPaymentMethodDetail();
								detail.BAccountID = account.BAccountID;
								detail.LocationID = account.LocationID;
								detail.DetailID = it.DetailID;
								this.PaymentDetails.Insert(detail);
							}
							if (toAdd.Count > 0)
							{
								this.PaymentDetails.View.RequestRefresh();
							}
						}
					}
					this.isPaymentMergedFlag = true;
				}
			}
		}

		#endregion

        public override void Persist()
		{
			if (User.Current != null && Contact.Current != null && User.Cache.GetStatus(User.Current) == PXEntryStatus.Inserted)
			{
				Users copy = PXCache<Users>.CreateCopy(User.Current);

				copy.OldPassword = User.Current.Password;
				copy.NewPassword = User.Current.Password;
				copy.ConfirmPassword = User.Current.Password;

				copy.FirstName = Contact.Current.FirstName;
				copy.LastName = Contact.Current.LastName;
				copy.Email = Contact.Current.EMail;

				copy.IsAssigned = true;
				User.Update(copy);
			}

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                EPEmployee _employee = (EPEmployee)Employee.Cache.Current;
                if (_employee != null && _employee.BAccountID > 0) // skip insert
                {
                    ContactMaint graph = CreateInstance<ContactMaint>();
                    Contact contact = Contact.Current;
                    bool shouldBeActive = (_employee.Status != CR.BAccount.status.Hold && _employee.Status != CR.BAccount.status.Inactive);
                    if (contact != null && contact.IsActive.GetValueOrDefault(false) != shouldBeActive)
                    {
                        contact.IsActive = shouldBeActive;
                        graph.ContactCurrent.Cache.Update(contact);
                        graph.Save.Press();
                        this.TimeStamp = graph.TimeStamp;
                        this.Caches[typeof(SyncTimeTag)].Clear();
                    }                    
                }
                base.Persist();
                ts.Complete();
            }

            if (User.Current != null && User.Current.ContactID == null && Contact.Current != null) // for correct redirection to user after inserting
			{
				User.Current.ContactID = Contact.Current.ContactID;
			}
		}

		protected virtual void Users_State_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnValue = e.ReturnValue ?? Users.state.NotCreated;
		}

		protected virtual void Users_LoginTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UserRoles.Cache.Clear();
			if (((Users)e.Row).LoginTypeID == null)
			{
				User.Cache.Clear();
				Employee.Current.UserID = null;
			}
		}

        protected virtual void Users_Username_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            Guid? restoredGuid = Access.GetGuidFromDeletedUser((string)e.NewValue);
            if (restoredGuid != null)
            {
                ((Users)e.Row).PKID = restoredGuid;
            }
        }
        protected virtual void Users_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            Users user = (Users)e.Row;
            if (user == null || Contact.Current == null || ((Users)e.Row).LoginTypeID == null) return;
            Employee.Current.UserID = user.PKID;
            if (Contact.Current == null)
                Contact.Current = Contact.Select();
            Contact.Current.UserID = user.PKID;
            Contact.Cache.MarkUpdated(Contact.Current);
        }

        protected virtual void Users_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Users user = (Users)e.Row;
			if (Employee.Current.UserID == null)
			{
				Employee.Current.UserID = user.PKID;
				if (Contact.Current == null)
					Contact.Current = Contact.Select();
				Contact.Current.UserID = user.PKID;
				Contact.Cache.MarkUpdated(Contact.Current);
			}
			else
			{
				User.Cache.Clear();
				UserRoles.Cache.Clear();
			}

			EPLoginType ltype = PXSelect<EPLoginType, Where<EPLoginType.loginTypeID, Equal<Current<Users.loginTypeID>>>>.SelectSingleBound(this, new object[] { user });
			user.Username = Contact.Current != null && ltype != null && ltype.EmailAsLogin == true ? Contact.Current.EMail : null;
		}

        protected virtual void EPRule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            EPRule row = (EPRule)e.Row;
            if (row != null)
            {
                EPRule steprow = PXSelect<EPRule, Where<EPRule.ruleID, Equal<Required<EPRule.stepID>>,
                        And<EPRule.stepID, IsNull>>>.Select(this, row.StepID);
                if (steprow != null)
                    row.StepName = steprow.Name;
            }
        }

        #region Private members
        private bool doCopyClassSettings;
        #endregion
    }

    [Serializable]
	[PXHidden]
	public class GenTimeCardFilter : IBqlTable
    {

        public abstract class lastDateGenerated : PX.Data.BQL.BqlDateTime.Field<lastDateGenerated> { }
        private DateTime? _LastDateGenerated;
        [PXDate]
        [PXUIField(DisplayName = "From", Enabled = false)]
        public virtual DateTime? LastDateGenerated
        {
            get
            {
                return _LastDateGenerated;
            }
            set
            {
                _LastDateGenerated = value;
            }
        }

        public abstract class generateUntil : PX.Data.BQL.BqlDateTime.Field<generateUntil> { }
        private DateTime? _GenerateUntil;
        [PXDate]
        [PXUIField(DisplayName = "Until")]
        public virtual DateTime? GenerateUntil
        {
            get
            {
                return _GenerateUntil;
            }
            set
            {
                _GenerateUntil = value;
            }
        }
	}

	[Serializable]
	[PXHidden]
	public class RedirectEmployeeParameters : IBqlTable
	{
		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		[PXInt]
		public virtual int? ParentBAccountID
		{
			get;
			set;
		}
		#endregion

		#region RouteEmails
		public abstract class routeEmails : PX.Data.BQL.BqlBool.Field<routeEmails> { }

		[PXBool]
		public virtual bool? RouteEmails { get; set; }
		#endregion
	}
}
