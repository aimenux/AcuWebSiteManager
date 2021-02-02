using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PX.Common;
using PX.Data;
using System.Collections;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CT;
using PX.Objects.CR.Workflows;
using PX.Objects.GL;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR
{
	public class CRCaseMaint : PXGraph<CRCaseMaint, CRCase>
	{
		#region Selects

		//TODO: need review
		[PXHidden]
		public PXSelect<BAccount>
			bAccountBasic;
        [PXHidden]
        public PXSelect<BAccountR>
            bAccountRBasic;

        [PXHidden]
        public PXSelect<Contact>
            Contacts;

        [PXHidden]
        public PXSelect<Contract>
            Contracts;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<Company>
			company;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<CRSetup>
			Setup;
		

		[PXViewName(Messages.Case)]
		public PXSelectJoin<CRCase,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRCase.customerID>>>,
				Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>
			Case;

		[PXHidden]
		[PXCopyPasteHiddenFields(typeof(CRCase.description))]
		public PXSelect<CRCase,
			Where<CRCase.caseCD, Equal<Current<CRCase.caseCD>>>>
			CaseCurrent;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<CRCase.noteID>>>>
			CaseActivityStatistics;

		[PXHidden]
		public PXSetup<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Optional<CRCase.caseClassID>>>> Class;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<CRCase>
			Answers;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
		[CRReference(typeof(CRCase.customerID), typeof(CRCase.contactID))]
		public CRActivityList<CRCase>
			Activities;
		
		[PXHidden]
		public PXSelect<CRActivity>
			ActivitiesSelect;

		[PXViewName(Messages.CaseReferences)]
		[PXViewDetailsButton(typeof(CRCase), 
			typeof(Select<CRCase, Where<CRCase.caseCD, Equal<Current<CRCaseReference.childCaseCD>>>>))]
		public PXSelectJoin<CRCaseReference,
            LeftJoin<CRCaseRelated, On<CRCaseRelated.caseCD, Equal<CRCaseReference.childCaseCD>>>>
			CaseRefs;

	    [PXHidden] 
        public PXSelect<CRCaseRelated, Where<CRCaseRelated.caseCD, Equal<Current<CRCaseReference.childCaseCD>>>>
	        CaseRelated;
        
		[PXCopyPasteHiddenView]
        [PXViewName(Messages.Relations)]
		[PXFilterable]
		public CRRelationsList<CRCase.noteID>
			Relations;

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.OwnerUser)]		
		public PXSelectReadonly<Users, Where<Users.pKID, Equal<Current<CRCase.ownerID>>>> OwnerUser;
		#endregion

		#region Ctors

		public CRCaseMaint()
		{
			if (string.IsNullOrEmpty(Setup.Current.CaseNumberingID))
			{
				throw new PXSetPropertyException(Messages.NumberingIDIsNull, Messages.CRSetup);
			}

			PXUIFieldAttribute.SetRequired<CRCase.caseClassID>(Case.Cache, true);

			Activities.GetNewEmailAddress =
				() =>
				{
					var current = Case.Current;
					if (current != null)
					{
						var contact = current.ContactID.
							With(_ => (Contact)PXSelect<Contact, 
								Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
							Select(this, _.Value));
						if (contact != null && !string.IsNullOrWhiteSpace(contact.EMail))
							return PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName);

						var customerContact = current.CustomerID.
							With(_ => (PXResult<Contact, BAccount>)PXSelectJoin<Contact,
								InnerJoin<BAccount, On<BAccount.defContactID, Equal<Contact.contactID>>>,
								Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
							Select(this, _.Value)).
							With(_ => (Contact)_);
						if (customerContact != null && !string.IsNullOrWhiteSpace(customerContact.EMail))
							return PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(customerContact.EMail, customerContact.DisplayName);
					}
					return String.Empty;
				};

			this.EnsureCachePersistence(typeof(CRPMTimeActivity));
			var bAccountCache = Caches[typeof(BAccount)];
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);
		}

		#endregion

		#region Data Handlers

		protected virtual IEnumerable caseRefs()
		{
			var currentCaseCd = Case.Current.With(_ => _.CaseCD);
			if (currentCaseCd == null) yield break;

			var ht = new HybridDictionary();
			foreach (CRCaseReference item in
				PXSelect<CRCaseReference,
					Where<CRCaseReference.parentCaseCD, Equal<Required<CRCaseReference.parentCaseCD>>>>.
				Select(this, currentCaseCd))
			{
				var childCaseCd = item.ChildCaseCD ?? string.Empty;
				if (ht.Contains(childCaseCd)) continue;

				ht.Add(childCaseCd, item);
                var relCase = SelectCase(childCaseCd);
				if (relCase == null)
					continue;

              /*  PXUIFieldAttribute.SetEnabled<CRCaseRelated.status>(CaseRelated.Cache, relCase, false);
                PXUIFieldAttribute.SetEnabled<CRCaseRelated.ownerID>(CaseRelated.Cache, relCase, false);
                PXUIFieldAttribute.SetEnabled<CRCaseRelated.workgroupID>(CaseRelated.Cache, relCase, false);*/

                yield return new PXResult<CRCaseReference, CRCaseRelated>(item, relCase);
			}

			var cache = CaseRefs.Cache;
			var oldIsDirty = cache.IsDirty;

			foreach (CRCaseReference item in 
				PXSelect<CRCaseReference,
					Where<CRCaseReference.childCaseCD, Equal<Required<CRCaseReference.childCaseCD>>>>.
				Select(this, currentCaseCd))
			{
				var parentCaseCd = item.ParentCaseCD ?? string.Empty;
				if (ht.Contains(parentCaseCd)) continue;
				var relCase = SelectCase(parentCaseCd);
				if(relCase == null)
					continue;

				ht.Add(parentCaseCd, item);
				cache.Delete(item);
				var newItem = (CRCaseReference)cache.CreateInstance();
				newItem.ParentCaseCD = currentCaseCd;
				newItem.ChildCaseCD = parentCaseCd;

			    switch (item.RelationType)
			    {
                    case CaseRelationTypeAttribute._DEPENDS_ON_VALUE:
			            newItem.RelationType = CaseRelationTypeAttribute._BLOCKS_VALUE;
                        break;
                    case CaseRelationTypeAttribute._DUBLICATE_OF_VALUE:
			            newItem.RelationType = CaseRelationTypeAttribute._DUBLICATE_OF_VALUE;
                        break;
                    case CaseRelationTypeAttribute._RELATED_VALUE:
			            newItem.RelationType = CaseRelationTypeAttribute._RELATED_VALUE;
                        break;
                    default:
			            newItem.RelationType = CaseRelationTypeAttribute._DEPENDS_ON_VALUE;
			            break;
			    }
				
				newItem = (CRCaseReference)cache.Insert(newItem);
				cache.IsDirty = oldIsDirty;
                yield return new PXResult<CRCaseReference, CRCaseRelated>(newItem, relCase);
			}
		}

		/*public virtual IEnumerable casereferencesdependson()
		{
			var idsHashtable = new Hashtable();
			foreach (RelatedCase item in CaseReferencesDependsOn.Cache.Cached)
			{
				idsHashtable.Add(item.CaseID, item);
				var status = CaseReferencesDependsOn.Cache.GetStatus(item);
				if (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated || status == PXEntryStatus.Notchanged)
				{
					var @case = (CRCase)PXSelect<CRCase, Where<CRCase.caseID, Equal<Required<CRCase.caseID>>>>.Select(this, item.CaseID);
					yield return new PXResult<RelatedCase, CRCase>(item, @case);
				}
			}
			foreach (PXResult<CRCaseReference, CRCase> item in
				PXSelectJoin<CRCaseReference,
				InnerJoin<CRCase, On<CRCaseReference.childCaseID, Equal<CRCase.caseID>>>,
				Where<CRCaseReference.parentCaseID, Equal<Current<CRCase.caseID>>>>.
				Select(this))
			{
				var @case = (CRCase)item;
				if (idsHashtable.ContainsKey(@case.CaseID)) continue;

				yield return new PXResult<RelatedCase, CRCase>(
					new RelatedCase
						{
							CaseID = @case.CaseID,
							RelationType = CaseRelationTypeAttribute._DEPENDS_ON_VALUE
						},
					@case);
			}

			foreach (PXResult<CRCaseReference, CRCase> item in
				PXSelectJoin<CRCaseReference,
				InnerJoin<CRCase, On<CRCaseReference.parentCaseID, Equal<CRCase.caseID>>>,
				Where<CRCaseReference.childCaseID, Equal<Current<CRCase.caseID>>>>.
				Select(this))
			{
				var @case = (CRCase)item;
				if (idsHashtable.ContainsKey(@case.CaseID)) continue;

				yield return new PXResult<RelatedCase, CRCase>(
					new RelatedCase
						{
							CaseID = @case.CaseID,
							RelationType = CaseRelationTypeAttribute._BLOCKS_VALUE
						},
					@case);
			}
		}*/

		/*public override void Persist()
		{
			CorrectRelatedCaseRecords();

			base.Persist();
		}*/

		#endregion

		#region Actions

		public new PXSave<CRCase> Save;
		public new PXCancel<CRCase> Cancel;
		public new PXInsert<CRCase> Insert;
		public new PXCopyPasteAction<CRCase> CopyPaste;
		public new PXDelete<CRCase> Delete;
		public new PXFirst<CRCase> First;
		public new PXPrevious<CRCase> Previous;
		public new PXNext<CRCase> Next;
		public new PXLast<CRCase> Last;

		public PXAction<CRCase> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			List<CRCase> list = new List<CRCase>(adapter.Get<CRCase>());
			Save.Press();

			PXLongOperation.StartOperation(this, delegate
				{
					CRCaseMaint graph = PXGraph.CreateInstance<CRCaseMaint>();

					foreach (CRCase @case in list)
					{
						if (@case == null || @case.Released == true) continue;
						graph.CheckBillingSettings(@case);
						graph.ReleaseCase(@case);
					}
				});

			return adapter.Get();
		}

		public PXMenuAction<CRCase> Action;
		public PXMenuInquiry<CRCase> Inquiry;

		public PXAction<CRCase> takeCase;
		[PXUIField(DisplayName = "Take Case", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable TakeCase(PXAdapter adapter)
		{
		    foreach (CRCase curCase in adapter.Get<CRCase>())
		    {
		        var caseCur = (CRCase)Case.Cache.CreateCopy(curCase);	        
		        caseCur.OwnerID = EmployeeMaint.GetCurrentEmployeeID(this);
		        if (caseCur.WorkgroupID != null)
		        {
		            EPCompanyTreeMember member = PXSelect<EPCompanyTreeMember,
		                Where<EPCompanyTreeMember.userID, Equal<Current<AccessInfo.userID>>,
		                    And<EPCompanyTreeMember.workGroupID, Equal<Required<CRCase.workgroupID>>>>>.
		                Select(this, caseCur.WorkgroupID);

		            if (member == null)
		            {
		                caseCur.WorkgroupID = null;
		            }
		        }
		        if (caseCur.OwnerID != Case.Current.OwnerID || caseCur.WorkgroupID != Case.Current.WorkgroupID)
		        {
		            caseCur = Case.Update(caseCur);		           
		        }

			    if (this.IsContractBasedAPI)
				    this.Save.Press();

		        yield return caseCur;
		    }
		}

		public PXAction<CRCase> assign;
		[PXUIField(DisplayName = Messages.Assign, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable Assign(PXAdapter adapter)
		{
			if (!Setup.Current.DefaultCaseAssignmentMapID.HasValue)
			{
				throw new PXSetPropertyException(Messages.AssignNotSetupCase, Messages.CRSetup);
			}

			var processor = new EPAssignmentProcessor<CRCase>(this);
			processor.Assign(CaseCurrent.Current, Setup.Current.DefaultCaseAssignmentMapID);

			CaseCurrent.Update(CaseCurrent.Current);
			
			if (this.IsContractBasedAPI)
				this.Save.Press();

			return adapter.Get();
		}

		public PXAction<CRCase> viewInvoice;
		[PXUIField(DisplayName = Messages.ViewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			//Direct Billing:
			if (CaseCurrent.Current != null && !string.IsNullOrEmpty(CaseCurrent.Current.ARRefNbr))
			{
				ARInvoiceEntry target = PXGraph.CreateInstance<ARInvoiceEntry>();
				target.Clear();
				target.Document.Current = target.Document.Search<ARInvoice.refNbr>(CaseCurrent.Current.ARRefNbr);
				throw new PXRedirectRequiredException(target, "ViewInvoice");
			}
			
			//Contract Billing:
			if (CaseCurrent.Current != null && CaseCurrent.Current.ContractID != null)
			{
				PMTran usageTran = PXSelect<PMTran, Where<PMTran.origRefID, Equal<Current<CRCase.noteID>>>>.Select(this);
				if (usageTran != null && !string.IsNullOrEmpty(usageTran.ARRefNbr))
				{
					ARInvoiceEntry target = PXGraph.CreateInstance<ARInvoiceEntry>();
					target.Clear();
					target.Document.Current = target.Document.Search<ARInvoice.refNbr>(usageTran.ARRefNbr);
					throw new PXRedirectRequiredException(target, "ViewInvoice");
				}
			}

			

			return adapter.Get();
		}

        public PXAction<CRCase> addNewContact;
        [PXUIField(DisplayName = Messages.AddNewContact, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable AddNewContact(PXAdapter adapter)
        {
            if (Case.Current != null && Case.Current.CustomerID != null)
            {
                ContactMaint target = PXGraph.CreateInstance<ContactMaint>();
                target.Clear();
                Contact maincontact = target.Contact.Insert();
                maincontact.BAccountID = Case.Current.CustomerID;

				CRContactClass ocls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>
					.SelectSingleBound(this, new object[] { maincontact });
				if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
				{
					maincontact.WorkgroupID = Case.Current.WorkgroupID;
					maincontact.OwnerID = Case.Current.OwnerID;
				}

                maincontact = target.Contact.Update(maincontact);
                throw new PXRedirectRequiredException(target, true, "Contact") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            return adapter.Get();
        }
        #endregion

        #region Event Handlers

        #region Contacts

        [CustomerProspectVendor(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual void Contact_BAccountID_CacheAttached(PXCache sender)
		{

		}

		#endregion

		#region CRCase

		[PopupMessage]
		[CRMBAccount]
		[CustomerAndProspectRestrictor]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRCase.customerID> e) { }

		[CRCaseBillableTime]
		[PXDBInt]
		[PXUIField(DisplayName = "Billable Time", Enabled = false)]
		public virtual void CRCase_TimeBillable_CacheAttached(PXCache sender) { }

        [PXDBInt]
        [PXUIField(DisplayName = "Contract")]
        [PXSelector(typeof(Search2<Contract.contractID,
                LeftJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>>,
            Where<Contract.baseType, Equal<CTPRType.contract>,
                And<Where<Current<CRCase.customerID>, IsNull,
                        Or2<Where<Contract.customerID, Equal<Current<CRCase.customerID>>,
                            And<Current<CRCase.locationID>, IsNull>>,
                        Or2<Where<ContractBillingSchedule.accountID, Equal<Current<CRCase.customerID>>,
                            And<Current<CRCase.locationID>, IsNull>>,
                        Or2<Where<Contract.customerID, Equal<Current<CRCase.customerID>>,
                            And<Contract.locationID, Equal<Current<CRCase.locationID>>>>,
                        Or<Where<ContractBillingSchedule.accountID, Equal<Current<CRCase.customerID>>,
                            And<ContractBillingSchedule.locationID, Equal<Current<CRCase.locationID>>>>>>>>>>>,
            OrderBy<Desc<Contract.contractCD>>>),
            DescriptionField = typeof(Contract.description),
            SubstituteKey = typeof(Contract.contractCD), Filterable = true)]
        [PXRestrictor(typeof(Where<Contract.status, Equal<Contract.status.active>, Or<Contract.status, Equal<Contract.status.inUpgrade>>>), Messages.ContractIsNotActive)]
        [PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, Or<Contract.expireDate, IsNull>>), Messages.ContractExpired)]
        [PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), Messages.ContractActivationDateInFuture, typeof(Contract.startDate))]
        [PXFormula(typeof(Default<CRCase.customerID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual void CRCase_ContractID_CacheAttached(PXCache sender) { }

		private void RecalcDetails(PXCache sender, CRCase row)
		{
			using (new PXConnectionScope())
			{
				bool IsReadOnly = sender.GetStatus(row) == PXEntryStatus.Notchanged;

				CRPMTimeActivity firstResponseActivity = null;

				CRCase cached = row;
				if (row.InitResponse == null)
				{
					cached = (CRCase)sender.Locate(row);
				}

				PXView view = new PXView(this,  true, Activities.View.BqlSelect);
				view.WhereNew<Where<CRActivity.refNoteID, Equal<Current<CRCase.noteID>>,
					And2<Where<CRActivity.isPrivate, IsNull, Or<CRActivity.isPrivate, Equal<False>>>,
					And<CRActivity.ownerID, IsNotNull,
					And2<Where<CRActivity.incoming, IsNull, Or<CRActivity.incoming, Equal<False>>>,
					And<Where<CRActivity.isExternal, IsNull, Or<CRActivity.isExternal, Equal<False>>>>>>>>>();
				view.OrderByNew<OrderBy<Asc<CRPMTimeActivity.startDate>>>();
				PXResult<CRPMTimeActivity, CRReminder> res = (PXResult<CRPMTimeActivity, CRReminder>)view.SelectSingleBound(new object[] { cached });

				firstResponseActivity = res;

				if (firstResponseActivity != null && firstResponseActivity.StartDate != null)
				{
					TimeSpan createDate = new TimeSpan(row.CreatedDateTime.Value.Ticks);
					TimeSpan action = new TimeSpan(firstResponseActivity.StartDate.Value.Ticks);
					sender.SetValue<CRCase.initResponse>(row, ((int)action.TotalMinutes - (int)createDate.TotalMinutes));
					sender.RaiseFieldUpdated<CRCase.initResponse>(row, null);
				}
			}
		}

		protected virtual void CRCase_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var caseRow = e.Row as CRCase;
			if (caseRow == null) return;

			Activities.DefaultSubject = PXMessages.LocalizeFormatNoPrefixNLA(Messages.CaseEmailDefaultSubject, caseRow.CaseCD, caseRow.Subject);

			var caseClass = PXSelectorAttribute.Select<CRCase.caseClassID>(cache, e.Row) as CRCaseClass;
			if (caseClass != null)
			{
				Activities.DefaultEMailAccountId = caseClass.DefaultEMailAccountID;
			}

			var perItemBilling = false;
			var denyOverrideBillable = false;
			if (caseClass != null)
			{
				denyOverrideBillable = caseClass.AllowOverrideBillable != true;
				perItemBilling = caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity;
			}

			if (caseRow.IsBillable != true)
				caseRow.ManualBillableTimes = false;

			var isNotReleased = caseRow.Released != true;
			if (isNotReleased)
			{
				PXUIFieldAttribute.SetEnabled<CRCase.manualBillableTimes>(cache, caseRow, caseRow.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<CRCase.isBillable>(cache, caseRow, !perItemBilling && !denyOverrideBillable);
				var canModifyBillableTimes = caseRow.IsBillable == true && caseRow.ManualBillableTimes == true;
				PXUIFieldAttribute.SetEnabled<CRCase.timeBillable>(cache, caseRow, canModifyBillableTimes);
				PXUIFieldAttribute.SetEnabled<CRCase.overtimeBillable>(cache, caseRow, canModifyBillableTimes);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, caseRow, false);
			}
			PXUIFieldAttribute.SetEnabled<CRCase.caseCD>(cache, caseRow, true);

			RecalcDetails(cache, caseRow);
            
			release.SetEnabled(isNotReleased && caseRow.IsBillable == true && !perItemBilling);
			Activities.Cache.AllowInsert = isNotReleased;
		    Guid? userID = EmployeeMaint.GetCurrentEmployeeID(this);            
            takeCase.SetEnabled(userID != null && caseRow.OwnerID != EmployeeMaint.GetCurrentEmployeeID(this) && isNotReleased);

			PXUIFieldAttribute.SetRequired<CRCase.customerID>(cache, (caseRow.IsBillable == true || (caseClass != null && caseClass.RequireCustomer == true)));
			PXUIFieldAttribute.SetRequired<CRCase.contractID>(cache, (caseClass != null && PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>() && caseClass.RequireContract == true));
			PXUIFieldAttribute.SetRequired<CRCase.contactID>(cache, (caseClass != null && caseClass.RequireContact == true));

			if (caseRow.ContactID != null && caseRow.CustomerID != null)
		    {
                Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.SelectSingleBound(this, null, caseRow.ContactID);
		        if (contact != null && contact.BAccountID != caseRow.CustomerID)
		            Case.Cache.RaiseExceptionHandling<CRCase.contactID>(caseRow, caseRow.ContactID,
		                new PXSetPropertyException(Messages.ContractBAccountDiffer, PXErrorLevel.Warning));
		    }            
		}

		protected virtual void CRCase_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var caseRow = (CRCase) e.Row;
			var caseClass = PXSelectorAttribute.Select<CRCase.caseClassID>(cache, e.Row) as CRCaseClass;

			PXDefaultAttribute.SetPersistingCheck<CRCase.customerID>(cache, caseRow, (caseRow.IsBillable == true || (caseClass != null && caseClass.RequireCustomer == true)) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CRCase.contractID>(cache, caseRow, (caseClass != null && PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>() && caseClass.RequireContract == true) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CRCase.contactID>(cache, caseRow, (caseClass != null && caseClass.RequireContact == true) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
		}

		protected virtual void CRCase_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			var caseRow = (CRCase)e.Row;
			var caseCd = caseRow?.CaseCD;
			if (caseRow == null || String.IsNullOrEmpty(caseCd))
				return;

			var referenceCache = CaseRefs.Cache;
			var ht = new HybridDictionary();
			foreach (CRCaseReference item in
				PXSelect<CRCaseReference,
					Where<CRCaseReference.parentCaseCD, Equal<Required<CRCaseReference.parentCaseCD>>>>.
				Select(this, caseCd))
			{
				var childCaseCd = item.ChildCaseCD ?? string.Empty;
				if (ht.Contains(childCaseCd)) continue;

				ht.Add(childCaseCd, item);
				referenceCache.Delete(item);
			}

			foreach (CRCaseReference item in
				PXSelect<CRCaseReference,
					Where<CRCaseReference.childCaseCD, Equal<Required<CRCaseReference.childCaseCD>>>>.
				Select(this, caseCd))
			{
				var parentCaseCd = item.ParentCaseCD ?? string.Empty;
				if (ht.Contains(parentCaseCd)) continue;

				ht.Add(parentCaseCd, item);
				referenceCache.Delete(item);
			}
		}

		protected virtual void CRCase_SLAETA_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CRCase row = e.Row as CRCase;
			if (row == null || row.CreatedDateTime == null) return;

			if (row.ClassID != null && row.Severity != null)
			{
				var severity = (CRClassSeverityTime)PXSelect<CRClassSeverityTime,
														Where<CRClassSeverityTime.caseClassID, Equal<Required<CRClassSeverityTime.caseClassID>>,
														And<CRClassSeverityTime.severity, Equal<Required<CRClassSeverityTime.severity>>>>>.
														Select(this, row.ClassID, row.Severity);
				if (severity != null && severity.TimeReaction != null)
				{
					e.NewValue = ((DateTime)row.CreatedDateTime).AddMinutes((int)severity.TimeReaction);
					e.Cancel = true;
				}
			}

			if (row.Severity != null && row.ContractID != null)
			{
				var template = (Contract)PXSelect<Contract, Where<Contract.contractID, Equal<Required<CRCase.contractID>>>>.Select(this, row.ContractID);
				if (template == null) return;
				
				var sla = (ContractSLAMapping)PXSelect<ContractSLAMapping,
												  Where<ContractSLAMapping.severity, Equal<Required<CRCase.severity>>,
												  And<ContractSLAMapping.contractID, Equal<Required<CRCase.contractID>>>>>.
												  Select(this, row.Severity, template.TemplateID);
				if (sla != null && sla.Period != null)
				{
					e.NewValue = ((DateTime)row.CreatedDateTime).AddMinutes((int)sla.Period);
					e.Cancel = true;
				}
			}
		}

		protected virtual void CRCase_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRCase;
			var oldRow = e.OldRow as CRCase;
			if (row == null || oldRow == null) return;

			if (row.OwnerID == null)
			{
				row.AssignDate = null;
			}
			else if (oldRow.OwnerID == null)
			{
				row.AssignDate = PXTimeZoneInfo.Now;
			}
		}

		protected virtual void CRCase_ContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CRCase;
			if (row == null || row.CustomerID == null) return;

			var contactsSet = PXSelectJoin<Contact,
				InnerJoin<BAccount,
					On<BAccount.bAccountID, Equal<Contact.bAccountID>,
						And<BAccount.defContactID, NotEqual<Contact.contactID>>>>,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>,
				And<Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>>>>.
				Select(this, row.CustomerID);

			if (contactsSet != null && contactsSet.Count == 1)
			{
				e.NewValue = ((Contact)contactsSet[0]).ContactID;
				e.Cancel = true;
			}
			else if (contactsSet != null && row.ContactID != null && contactsSet.Any(contact => PXResult.Unwrap<Contact>(contact).ContactID == row.ContactID))
			{
				//Keep previous contact.
				e.NewValue = row.ContactID;
				e.Cancel = true;
			}
		}

		protected virtual void CRCase_ContractID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CRCase row = e.Row as CRCase;
			if (row == null || row.CustomerID == null) return;

			List<object> contracts = PXSelectorAttribute.SelectAll<CRCase.contractID>(sender, e.Row);
			if (contracts.Exists(contract => PXResult.Unwrap<Contract>(contract).ContractID == row.ContractID))
			{
				e.NewValue = row.ContractID;
			}
			else if (contracts.Count == 1)
			{
				e.NewValue = PXResult.Unwrap<Contract>(contracts[0]).ContractID;
			}
			e.Cancel = true;
		}

		protected virtual void CRCase_ContractID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CRCase crcase = (CRCase)e.Row;
			Contract contract = PXResult.Unwrap<Contract>(PXSelectorAttribute.Select<CRCase.contractID>(sender, e.Row, e.NewValue));
			if (crcase == null || contract == null) return;

			int daysLeft;
			if (Accessinfo.BusinessDate != null
				&& ContractMaint.IsInGracePeriod(contract, (DateTime)Accessinfo.BusinessDate, out daysLeft))
			{
				sender.RaiseExceptionHandling<CRCase.contractID>(crcase, e.NewValue, new PXSetPropertyException(Messages.ContractInGracePeriod, PXErrorLevel.Warning, daysLeft));
			}
		}
		#endregion

		#region CRCaseReference

		[PXDBString(IsKey = true)]
		[PXDBLiteDefault(typeof(CRCase.caseCD))]
		[PXUIField(Visible = false)]
		public virtual void CRCaseReference_ParentCaseCD_CacheAttached(PXCache sender)
		{
			
		}

		protected virtual void CRCaseReference_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CRCaseReference;
			if (row == null || row.ChildCaseCD == null || row.ParentCaseCD == null) return;

			var alternativeRecord = (CRCaseReference)PXSelect<CRCaseReference,
				Where<CRCaseReference.parentCaseCD, Equal<Required<CRCaseReference.parentCaseCD>>,
					And<CRCaseReference.childCaseCD, Equal<Required<CRCaseReference.childCaseCD>>>>>.
				Select(this, row.ChildCaseCD, row.ParentCaseCD);
			if (alternativeRecord != null)
				sender.Delete(alternativeRecord);
		}

        protected virtual void CRCaseReference_RelationType_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            
        }

		protected virtual void CRCaseReference_ChildCaseCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as CRCaseReference;
			if (row == null || e.NewValue == null) return;

			if (object.Equals(row.ParentCaseCD, e.NewValue))
			{
				e.Cancel = true;
				throw new PXSetPropertyException(Messages.CaseCannotDependUponItself);
			}
		}

		#endregion

		//#region EPActivity		

		//[PXDefault(typeof(Search<BAccount.noteID, 
		//	Where<BAccount.bAccountID, Equal<Current<CRCase.customerID>>>>),
		//	PersistingCheck = PXPersistingCheck.Nothing)] 
		//[PXMergeAttributes(Method = MergeMethod.Merge)]
		//public virtual void EPActivity_ParentRefNoteID_CacheAttached(PXCache sender) { }

		//#endregion

		#endregion

		#region CacheAttached

		[PXBool]
		[PXDefault(false)]
		[PXDBCalced(typeof(True), typeof(Boolean))]
		protected virtual void BAccountR_ViewInCrm_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDatetimeScalar(typeof(Search<CRActivityStatistics.lastActivityDate, Where<CRActivityStatistics.noteID, Equal<CRCase.noteID>>>), PreserveTime = true, UseTimeZone = true)]
		protected virtual void CRCase_LastActivity_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region Private Methods

		private CRCaseRelated SelectCase(object caseCd)
		{
			if (caseCd == null) return null;

            return (CRCaseRelated)PXSelect<CRCaseRelated,
                Where<CRCaseRelated.caseCD, Equal<Required<CRCase.caseCD>>>>.
				Select(this, caseCd);
		}
        
		protected virtual void ReleaseCase(CRCase item)
		{
            RegisterEntry registerEntry = (RegisterEntry)PXGraph.CreateInstance(typeof(RegisterEntry));

            PXSelectBase<EPActivityApprove> select = new PXSelect<EPActivityApprove,
                Where<EPActivityApprove.refNoteID, Equal<Required<EPActivityApprove.refNoteID>>>>(this);

            List<EPActivityApprove> list = new List<EPActivityApprove>();
            foreach (EPActivityApprove activity in select.Select(item.NoteID))
            {
                list.Add(activity);

				if ((activity.TimeSpent.GetValueOrDefault() != 0 || activity.TimeBillable.GetValueOrDefault() != 0) && activity.ApproverID != null && activity.ApprovalStatus != ActivityStatusListAttribute.Completed && activity.ApprovalStatus != ActivityStatusListAttribute.Canceled)
                {
                    throw new PXException(Messages.OneOrMoreActivitiesAreNotApproved);
                }
            }

			bool tranAdded = false;

            if (item.ContractID != null)
            {
                //Contract Billing:

	            using (PXTransactionScope ts = new PXTransactionScope())
	            {
		            RecordContractUsage(item);

                    if (!EmployeeActivitiesRelease.RecordCostTrans(registerEntry, list, out tranAdded))
                    {
                        throw new PXException(Messages.FailedRecordCost);
                    }
	                this.TimeStamp = registerEntry.TimeStamp;
                    //foreach (EPActivity activity in PXSelect<EPActivity, Where<EPActivity.refNoteID, Equal<Required<CRCase.noteID>>>>.Select(this, item.NoteID))
                    //{
                    //    activity.tstamp = registerEntry.TimeStamp;
                    //}


                    item.Released = true;
                                        string saveResolution = item.Resolution;
										Case.Update(item);
                                        item.Resolution = saveResolution;
										this.Save.Press();
                    ts.Complete();
                }
            }
            else
            {
                //Direct Billing:
                Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, item.CustomerID);
                if (customer == null)
                {
                    throw new PXException(Messages.CustomerNotFound);
                }
			
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                    invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
                    invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

                    ARInvoice invoice = (ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].CreateInstance();
                    invoice.DocType = ARDocType.Invoice;
                    invoice = (ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].Insert(invoice);
                    invoice.CustomerID = item.CustomerID;
                    invoice.CustomerLocationID = item.LocationID;
                    invoice.DocDate = Accessinfo.BusinessDate;
					invoice.DocDesc = item.Subject;
					invoice = invoiceEntry.Document.Update(invoice);

					invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.hold>(invoice, false);                    
					invoiceEntry.customer.Current.CreditRule = customer.CreditRule;
					invoice = invoiceEntry.Document.Update(invoice);
                    foreach (ARTran tran in GenerateARTrans(item))
                    {
                        invoiceEntry.Transactions.Insert(tran);
                    }
					ARInvoice oldInvoice = (ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].CreateCopy(invoice);
					invoice.CuryOrigDocAmt = invoice.CuryDocBal;
                    invoice.OrigDocAmt = invoice.DocBal;
					invoiceEntry.Caches[typeof(ARInvoice)].RaiseRowUpdated(invoice, oldInvoice);
					invoiceEntry.Caches[typeof(ARInvoice)].SetValue<ARInvoice.curyOrigDocAmt>(invoice, invoice.CuryDocBal);

                    invoiceEntry.Actions.PressSave();

                    item.Released = true;                    
                    item.ARRefNbr = invoiceEntry.Document.Current.RefNbr;
                    string saveResolution = item.Resolution;
                    Case.Update(item);
                    item.Resolution = saveResolution;
                    this.Save.Press();
					
                    if (!EmployeeActivitiesRelease.RecordCostTrans(registerEntry, list, out tranAdded))
                    {
                        throw new PXException(Messages.FailedRecordCost);
                    }


                    ts.Complete();
                }
            }

			if (tranAdded)//there can be no cost transactions at all - they were created when a timecard was released.
            {
                EPSetup setup = PXSelect<EPSetup>.Select(registerEntry);

                if (setup != null && setup.AutomaticReleasePM == true)
                {
                    PX.Objects.PM.RegisterRelease.Release(registerEntry.Document.Current);
                }
            }
		}
        
        protected virtual void RecordContractUsage(CRCase item)
        {
            RegisterEntry registerEntry = CreateInstance<RegisterEntry>();
            registerEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            registerEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.
            registerEntry.Document.Cache.Insert();
            registerEntry.Document.Current.Description = item.Subject;
            registerEntry.Document.Current.Released = true;
			registerEntry.Document.Current.Status = PMRegister.status.Released;
			registerEntry.EnsureCachePersistence(typeof(CRPMTimeActivity));

	        foreach (PMTran tran in GeneratePMTrans(item))
            {
				registerEntry.Transactions.Insert(tran);
				UsageMaint.AddUsage(registerEntry.ContractDetails.Cache, tran.ProjectID, tran.InventoryID, tran.BillableQty ?? 0m, tran.UOM);
			}

            item.Released = true;
            string saveResolution = item.Resolution;
            Case.Update(item);
            item.Resolution = saveResolution;

            registerEntry.Save.Press();
        }
        
		public override object GetValueExt(string viewName, object data, string fieldName)
		{
			object ret = base.GetValueExt(viewName, data, fieldName);
			if (String.Equals(viewName, "CaseCurrent", StringComparison.OrdinalIgnoreCase) && String.Equals(fieldName, "CustomerID", StringComparison.OrdinalIgnoreCase) && ret is PXFieldState && !String.IsNullOrEmpty(((PXFieldState)ret).Error))
			{
				((PXFieldState)ret).Error = null;
			}
			return ret;
		}
        
        protected virtual List<PMTran> GeneratePMTrans(CRCase @case)
		{
            Contract contract = PXSelect<Contract,
				Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.
				Select(this, @case.ContractID);

            CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, @case.CaseClassID);

			List<PMTran> result = new List<PMTran>();

            DateTime startDate = Accessinfo.BusinessDate ?? (DateTime)@case.CreatedDateTime;
			DateTime endDate = startDate.Add(new TimeSpan(0, (@case.TimeBillable ?? 0), 0));

			PXResultset<CRPMTimeActivity> list = PXSelect<CRPMTimeActivity,
				Where<CRPMTimeActivity.refNoteID, Equal<Required<CRPMTimeActivity.refNoteID>>>,
				OrderBy<Desc<CRPMTimeActivity.createdDateTime>>>.
				Select(this, @case.NoteID);

			#region For Case without activities
			if (list.Count > 0)
			{
				startDate = (DateTime)((CRPMTimeActivity)list[0]).StartDate;
				endDate = startDate;
			}
			#endregion

			PXCache cache = null;
			foreach (CRPMTimeActivity activity in list)
			{
				if (cache == null) cache = Caches[activity.GetType()];
				if (activity.ClassID == CRActivityClass.Activity && activity.IsBillable == true)
				{
					if (activity.StartDate != null && (DateTime)activity.StartDate < startDate)
					{
						startDate = (DateTime)activity.StartDate;
					}

					if (activity.EndDate != null && (DateTime)activity.EndDate > endDate)
					{
						endDate = (DateTime)activity.EndDate;
					}
					activity.Billed = true;
				}
				activity.Released = true;				
				cache.Update(activity);
			}

            if (contract.CaseItemID != null)
			{
				InventoryItem item = PXSelect<InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                    Select(this, contract.CaseItemID);

				PMTran newTran = new PMTran();
				newTran.ProjectID = contract.ContractID;
                newTran.InventoryID = contract.CaseItemID;
				newTran.AccountGroupID = contract.ContractAccountGroup;
				newTran.OrigRefID = @case.NoteID;
				newTran.BAccountID = @case.CustomerID;
				newTran.LocationID = @case.LocationID;
				newTran.Description = @case.Subject;
				newTran.StartDate = startDate;
				newTran.EndDate = endDate;
			    newTran.Date = endDate;
				newTran.Qty = 1;
				newTran.BillableQty = 1;
				newTran.UOM = item.SalesUnit;
				newTran.Released = true;
				newTran.Allocated = true;
				newTran.BillingID = contract.BillingID;
				newTran.IsQtyOnly = true;
				newTran.CaseCD = @case.CaseCD;
				result.Add(newTran);
			}

			#region Record Labor Usage
            if (caseClass.LabourItemID != null)
			{
				int totalBillableMinutes = (@case.TimeBillable ?? 0);

                if (caseClass.OvertimeItemID != null)
				{
					totalBillableMinutes -= (@case.OvertimeBillable ?? 0);
				}

				if (totalBillableMinutes > 0)
				{
					if (caseClass.PerItemBilling == BillingTypeListAttribute.PerCase && caseClass.RoundingInMinutes > 1)
					{
						decimal fraction = Convert.ToDecimal(totalBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
						int points = Convert.ToInt32(Math.Ceiling(fraction));
						totalBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
					}

					if (caseClass.PerItemBilling == BillingTypeListAttribute.PerCase && caseClass.MinBillTimeInMinutes > 0)
					{
						totalBillableMinutes = Math.Max(totalBillableMinutes, (int)caseClass.MinBillTimeInMinutes);
					}

					InventoryItem item = PXSelect<InventoryItem,
						Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                        Select(this, caseClass.LabourItemID);
					PMTran newLabourTran = new PMTran();
					newLabourTran.ProjectID = contract.ContractID;
                    newLabourTran.InventoryID = caseClass.LabourItemID;
					newLabourTran.AccountGroupID = contract.ContractAccountGroup;
					newLabourTran.OrigRefID = @case.NoteID;
					newLabourTran.BAccountID = @case.CustomerID;
					newLabourTran.LocationID = @case.LocationID;
					newLabourTran.Description = @case.Subject;
					newLabourTran.StartDate = startDate;
					newLabourTran.EndDate = endDate;
                    newLabourTran.Date = endDate;
					newLabourTran.UOM = item.SalesUnit;
					newLabourTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalBillableMinutes).TotalHours);
					newLabourTran.BillableQty = newLabourTran.Qty;
					newLabourTran.Released = true;
					newLabourTran.Allocated = true;
					newLabourTran.BillingID = contract.BillingID;
					newLabourTran.IsQtyOnly = true;
					newLabourTran.CaseCD = @case.CaseCD;
					result.Add(newLabourTran);
				}
			}
			#endregion

			#region Record Overtime Usage

            if (caseClass.OvertimeItemID.HasValue)
			{
				InventoryItem item = PXSelect<InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                    Select(this, caseClass.OvertimeItemID);
				int totalOvertimeBillableMinutes = (@case.OvertimeBillable ?? 0);

				if (totalOvertimeBillableMinutes > 0)
				{
					if (caseClass.PerItemBilling == BillingTypeListAttribute.PerCase && caseClass.RoundingInMinutes > 1)
					{
						decimal fraction = Convert.ToDecimal(totalOvertimeBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
						int points = Convert.ToInt32(Math.Ceiling(fraction));
						totalOvertimeBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
					}

					PMTran newOvertimeTran = new PMTran();
					newOvertimeTran.ProjectID = contract.ContractID;
                    newOvertimeTran.InventoryID = caseClass.OvertimeItemID;
					newOvertimeTran.AccountGroupID = contract.ContractAccountGroup;
					newOvertimeTran.OrigRefID = @case.NoteID;
					newOvertimeTran.BAccountID = @case.CustomerID;
					newOvertimeTran.LocationID = @case.LocationID;
					newOvertimeTran.Description = @case.Subject;
					newOvertimeTran.StartDate = startDate;
					newOvertimeTran.EndDate = endDate;
                    newOvertimeTran.Date = endDate;
					newOvertimeTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalOvertimeBillableMinutes).TotalHours);
					newOvertimeTran.BillableQty = newOvertimeTran.Qty;
					newOvertimeTran.UOM = item.SalesUnit;
					newOvertimeTran.Released = true;
					newOvertimeTran.Allocated = true;
					newOvertimeTran.BillingID = contract.BillingID;
					newOvertimeTran.IsQtyOnly = true;
					newOvertimeTran.CaseCD = @case.CaseCD;
					result.Add(newOvertimeTran);
				}
			}

			#endregion


			return result;
		}

        protected virtual List<ARTran> GenerateARTrans(CRCase c)
        {
            CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, c.CaseClassID);

            List<ARTran> result = new List<ARTran>();

            DateTime startDate = (DateTime)c.CreatedDateTime;
            DateTime endDate = startDate.Add(new TimeSpan(0, (c.TimeBillable ?? 0), 0));

            PXResultset<CRPMTimeActivity> list = PXSelect<CRPMTimeActivity,
                Where<CRPMTimeActivity.refNoteID, Equal<Required<CRPMTimeActivity.refNoteID>>>,
                OrderBy<Desc<CRPMTimeActivity.createdDateTime>>>.
                Select(this, c.NoteID);

            #region For Case without activities
            if (list.Count > 0)
            {
                startDate = (DateTime)((CRPMTimeActivity)list[0]).StartDate;
                endDate = startDate;
            }
            #endregion

            PXCache cache = null;
            foreach (CRPMTimeActivity activity in list)
            {
                if (cache == null) cache = Caches[activity.GetType()];
                cache.Current = activity;
                if (activity.ClassID == CRActivityClass.Activity && activity.IsBillable == true)
                {
                    if (activity.StartDate != null && (DateTime)activity.StartDate < startDate)
                    {
                        startDate = (DateTime)activity.StartDate;
                    }

                    if (activity.EndDate != null && (DateTime)activity.EndDate > endDate)
                    {
                        endDate = (DateTime)activity.EndDate;
                    }
                    activity.Billed = true;
					cache.Update(activity);
				}
            }
            
            #region Record Labor Usage
            if (caseClass.LabourItemID != null)
            {
                int totalBillableMinutes = (c.TimeBillable ?? 0);

                if (caseClass.OvertimeItemID != null)
                {
                    totalBillableMinutes -= (c.OvertimeBillable ?? 0);
                }

                if (totalBillableMinutes > 0)
                {
                    if (caseClass.RoundingInMinutes > 1)
                    {
                        decimal fraction = Convert.ToDecimal(totalBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
                        int points = Convert.ToInt32(Math.Ceiling(fraction));
                        totalBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
                    }

                    if (caseClass.MinBillTimeInMinutes > 0)
                    {
                        totalBillableMinutes = Math.Max(totalBillableMinutes, (int)caseClass.MinBillTimeInMinutes);
                    }

                    InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, caseClass.LabourItemID);
                    ARTran newLabourTran = new ARTran();
                    newLabourTran.InventoryID = caseClass.LabourItemID;
                    newLabourTran.TranDesc = c.Subject;
                    newLabourTran.UOM = item.SalesUnit;
                    newLabourTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalBillableMinutes).TotalHours);
					newLabourTran.CaseCD = c.CaseCD;
                    newLabourTran.ManualPrice = false;
                    
                    result.Add(newLabourTran);
                }
            }
            #endregion

            #region Record Overtime Usage

            if (caseClass.OvertimeItemID.HasValue)
            {
                InventoryItem item = PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                    Select(this, caseClass.OvertimeItemID);
                int totalOvertimeBillableMinutes = (c.OvertimeBillable ?? 0);

                if (totalOvertimeBillableMinutes > 0)
                {
                    if (caseClass.RoundingInMinutes > 1)
                    {
                        decimal fraction = Convert.ToDecimal(totalOvertimeBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
                        int points = Convert.ToInt32(Math.Ceiling(fraction));
                        totalOvertimeBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
                    }

                    ARTran newOvertimeTran = new ARTran();
                    newOvertimeTran.InventoryID = caseClass.OvertimeItemID;
                    newOvertimeTran.TranDesc = c.Subject;
                    newOvertimeTran.UOM = item.SalesUnit;
                    newOvertimeTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalOvertimeBillableMinutes).TotalHours);
					newOvertimeTran.CaseCD = c.CaseCD;
                    newOvertimeTran.ManualPrice = true;
                    
                    result.Add(newOvertimeTran);
                }
            }

            #endregion


            return result;
        }

		private bool VerifyField<TField>(object row, object newValue)
			where TField : IBqlField
		{
			if (row == null) return true;

			var result = false;
			var cache = Caches[row.GetType()];
			try
			{
				result = cache.RaiseFieldVerifying<TField>(row, ref newValue);
			}
			catch (StackOverflowException) { throw; }
			catch (OutOfMemoryException) { throw; }
			catch (Exception) { }

			return result;
		}

		private void CheckBillingSettings(CRCase @case)
		{

			CRPMTimeActivity activity = PXSelectReadonly<CRPMTimeActivity,
				Where<CRPMTimeActivity.isBillable, Equal<True>,
					And2<Where<CRPMTimeActivity.uistatus, IsNull,
						Or<CRPMTimeActivity.uistatus, Equal<ActivityStatusAttribute.open>>>,
					And<Where<CRPMTimeActivity.refNoteID, Equal<Current<CRCase.noteID>>>>>>>.SelectSingleBound(this, new object[] { @case });
			if (activity != null)
			{
				throw new PXException(Messages.CloseCaseWithHoldActivities);
			}

			CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, @case.CaseClassID);


			if (caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity)
			{
				throw new PXException(Messages.OnlyBillByActivity);
			}

		    if (@case.IsBillable == true)
		    {
		        if (@case.ContractID == null)
		        {
		            if (caseClass.LabourItemID == null)
		                throw new PXException(Messages.CaseClassDetailsIsNotSet);
					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, caseClass.LabourItemID);
			        if (item != null && item.SalesAcctID == null)
			        {
						throw new PXException(Messages.SalesAccountIsNotSetForLaborItem);
			        }
		        }
		        else
		        {
		            Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Current<CRCase.contractID>>>>.SelectSingleBound(this, new object[] {@case});

		            if (caseClass.LabourItemID == null && contract.CaseItemID == null)
		            {
		                throw new PXException(Messages.CaseClassDetailsIsNotSet);
		            }
		        }
		    }

		}
		#endregion
	}

    [Serializable]
    public partial class CRCaseRelated : CRCase
    {

        public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }

        [PXDBString(1, IsFixed = true)]
		[CaseWorkflow.States.List(BqlField = typeof(CRCase.status))]
		[PXUIField(DisplayName = "Status", Enabled = false)]
        public override String Status { get; set; }

        public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

        [PXDBGuid()]
        [PXOwnerSelector(typeof(CRCase.workgroupID))]
        [PXUIField(DisplayName = "Owner", Enabled = false)]
        public override Guid? OwnerID { get; set; }

        public new abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Workgroup", Enabled = false)]
        [PXCompanyTreeSelector]
        public override Int32? WorkgroupID { get; set; }
    }
}
