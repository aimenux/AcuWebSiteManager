using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;
using PX.Objects.TX;

namespace PX.Objects.AR
{
	public class CustomerClassMaint : PXGraph<CustomerClassMaint, CustomerClass>
	{
		public PXSelect<CustomerClass> CustomerClassRecord;
		public PXSelect<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<CustomerClass.customerClassID>>>> CurCustomerClassRecord;
        [PXViewName(CR.Messages.Attributes)]
        public CSAttributeGroupList<CustomerClass, Customer> Mapping;

        public CRClassNotificationSourceList<NotificationSource, 
			CustomerClass.customerClassID, ARNotificationSource.customer> NotificationSources;

		public PXSelect<NotificationRecipient,
			Where<NotificationRecipient.refNoteID, IsNull,
			  And<NotificationRecipient.sourceID, Equal<Optional<NotificationSource.sourceID>>>>> NotificationRecipients;

		public PXSelect<Customer,
			Where<Customer.customerClassID, Equal<Current<CustomerClass.customerClassID>>>> Customers;

		#region Cache Attached
		#region NotificationSource
		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search<NotificationSetup.setupID,
			Where<NotificationSetup.sourceCD, Equal<ARNotificationSource.customer>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
		[PXUIField(DisplayName = "Mailing ID")]
		protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(CustomerClass.customerClassID))]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select2<CustomerClass,
			InnerJoin<NotificationSetup, On<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>,
			Where<CustomerClass.customerClassID, Equal<Current<NotificationSource.classID>>>>))]
		protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report")]
		[PXDefault(typeof(Search<NotificationSetup.reportID,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SiteMap.screenID,
		Where<SiteMap.url, Like<Common.urlReports>,
			And<Where<SiteMap.screenID, Like<PXModule.ar_>,
						 Or<SiteMap.screenID, Like<PXModule.so_>,
						 Or<SiteMap.screenID, Like<PXModule.cr_>>>>>>,
		OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
		Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
		DescriptionField = typeof(SiteMap.title))]
		[PXFormula(typeof(Default<NotificationSource.setupID>))]
		protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region NotificationRecipient
		[PXDBInt]
		[PXDBLiteDefault(typeof(NotificationSource.sourceID))]
		protected virtual void NotificationRecipient_SourceID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10)]
		[PXDefault]
		[CustomerContactType.ClassList]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckUnique(typeof(NotificationRecipient.contactID),
			Where = typeof(Where<NotificationRecipient.refNoteID, IsNull, And<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>>>))]
		protected virtual void NotificationRecipient_ContactType_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationRecipient.contactType))]
		protected virtual void NotificationRecipient_ContactID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#endregion

		public PXAction<CustomerClass> resetGroup;
		[PXProcessButton]
        [PXUIField(DisplayName = "Apply Restriction Settings to All Customers")]
		protected virtual IEnumerable ResetGroup(PXAdapter adapter)
		{
			if (CustomerClassRecord.Ask(Messages.Warning, Messages.GroupUpdateConfirm, MessageButtons.OKCancel) == WebDialogResult.OK)
			{
				Save.Press();
				string classID = CustomerClassRecord.Current.CustomerClassID;
				PXLongOperation.StartOperation(this, delegate()
				{
					Reset(classID);
				});
			}
			return adapter.Get();
		}
		protected static void Reset(string classID)
		{
			CustomerClassMaint graph = PXGraph.CreateInstance<CustomerClassMaint>();
			graph.CustomerClassRecord.Current = graph.CustomerClassRecord.Search<CustomerClass.customerClassID>(classID);
			if (graph.CustomerClassRecord.Current != null)
			{
				foreach (Customer cust in graph.Customers.Select())
				{
					cust.GroupMask = graph.CustomerClassRecord.Current.GroupMask;
					graph.Customers.Cache.SetStatus(cust, PXEntryStatus.Updated);
				}
				graph.Save.Press();
			}
		}
		public PXSelect<PX.SM.Neighbour> Neighbours;
		public override void Persist()
		{
			if (CustomerClassRecord.Current != null)
			{
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(PX.SM.Users), typeof(PX.SM.Users));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(Customer), typeof(Customer));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(CustomerClass), typeof(CustomerClass));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(PX.SM.Users), typeof(Customer));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(Customer), typeof(PX.SM.Users));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(PX.SM.Users), typeof(CustomerClass));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(CustomerClass), typeof(PX.SM.Users));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(CustomerClass), typeof(Customer));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(Customer), typeof(CustomerClass));
			}
			base.Persist();
			GroupHelper.Clear();
		}

		#region Setups
		public PXSetup<GL.Company> Company;
        #endregion

		public virtual void CustomerClass_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{						
			CustomerClass row = (CustomerClass)e.Row;
            if (row == null) return;

			NotificationSource source = this.NotificationSources.Select(row.CustomerClassID);
            this.NotificationRecipients.Cache.AllowInsert = source != null;
            PXUIFieldAttribute.SetEnabled<CustomerClass.creditLimit>(cache, row, (row.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT
                        || row.CreditRule == CreditRuleTypes.CS_BOTH));
            PXUIFieldAttribute.SetEnabled<CustomerClass.overLimitAmount>(cache, row, (row.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT
                        || row.CreditRule == CreditRuleTypes.CS_BOTH));
			PXUIFieldAttribute.SetEnabled<CustomerClass.creditDaysPastDue>(cache, row, (row.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE
						|| row.CreditRule == CreditRuleTypes.CS_BOTH));

			PXUIFieldAttribute.SetEnabled<CustomerClass.smallBalanceLimit>(cache, row, (row.SmallBalanceAllow ?? false));

			PXUIFieldAttribute.SetEnabled<CustomerClass.finChargeID>(cache, row, (row.FinChargeApply ?? false));

			var mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<CustomerClass.curyID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.curyRateTypeID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.printCuryStatements>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.allowOverrideCury>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.allowOverrideRate>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.unrealizedGainAcctID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.unrealizedGainSubID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.unrealizedLossAcctID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.unrealizedLossSubID>(cache, null, mcFeatureInstalled);
		}

        public virtual void CustomerClass_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            CustomerClass cclass = (CustomerClass)e.Row;
            if (cclass == null) return;

            ARSetup setup = PXSelect<ARSetup>.Select(this);
            if (setup != null && cclass.CustomerClassID == setup.DfltCustomerClassID)
            {
                throw new PXException(Messages.CustomerClassCanNotBeDeletedBecauseItIsUsed);
            }
        }

        protected virtual void CustomerClass_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void CustomerClass_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() == false)
			{
				e.Cancel = true;
			}
		}

		protected virtual void CustomerClass_CuryRateTypeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() == false)
			{
				e.Cancel = true;
			}
		}

		public virtual void CustomerClass_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			CustomerClass row = (CustomerClass)e.Row;
			if (row.FinChargeApply ?? false)
			{
				if (row.FinChargeID== null)
				{
					if (cache.RaiseExceptionHandling<CustomerClass.finChargeID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CustomerClass.finChargeID).Name)))
					{
						throw new PXRowPersistingException(typeof(CustomerClass.finChargeID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CustomerClass.finChargeID).Name);
					}
				}
			}

			if (row?.RequireAvalaraCustomerUsageType == true && row.AvalaraCustomerUsageType == TXAvalaraCustomerUsageType.Default)
			{
				throw new PXRowPersistingException(typeof(CustomerClass.avalaraCustomerUsageType).Name,
					row.AvalaraCustomerUsageType, Common.Messages.NonDefaultAvalaraUsageType);
			}
		}
		
		public virtual void CustomerClass_StatementCycleId_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerClass row = (CustomerClass)e.Row;
			if (row == null) return;
			if ((row.StatementCycleId != null))
			{
				ARSetup setup = (ARSetup)PXSelect<ARSetup>.Select(this);
				if (setup != null && (setup.DefFinChargeFromCycle == true))
				{
					ARStatementCycle arSC = PXSelect<ARStatementCycle,
												Where<ARStatementCycle.statementCycleId, Equal<Required<ARStatementCycle.statementCycleId>>>>.
																		 Select(this, row.StatementCycleId);
					if ((arSC != null) && (arSC.FinChargeID != null))
					{
						row.FinChargeID = arSC.FinChargeID;
						this.CustomerClassRecord.Cache.RaiseFieldUpdated<CustomerClass.finChargeID>(row, null);
					}
				}
			}
		}	

		public virtual void CustomerClass_CreditRule_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerClass row = (CustomerClass)e.Row;
			if (row.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT
				|| row.CreditRule == CreditRuleTypes.CS_NO_CHECKING) 
			{
				row.CreditDaysPastDue = 0;
			}
			if (row.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE
				|| row.CreditRule == CreditRuleTypes.CS_NO_CHECKING)
			{
				row.CreditLimit = 0m;
			} 

		}

		public virtual void CustomerClass_SmallBalanceAllow_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerClass row = (CustomerClass)e.Row;
			row.SmallBalanceLimit = 0m;
		}

		public virtual void CustomerClass_FinChargeApply_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerClass row = (CustomerClass)e.Row;
			if (!(row.FinChargeApply ?? false))
			{
				row.FinChargeID= null;			
			}
		}

		public CustomerClassMaint() 
		{
			PXUIFieldAttribute.SetVisible<CustomerClass.cOGSAcctID>(CustomerClassRecord.Cache, null, false);
			PXUIFieldAttribute.SetVisible<CustomerClass.cOGSSubID>(CustomerClassRecord.Cache, null, false);
			PXUIFieldAttribute.SetVisible<CustomerClass.miscAcctID>(CustomerClassRecord.Cache, null, false);
			PXUIFieldAttribute.SetVisible<CustomerClass.miscSubID>(CustomerClassRecord.Cache, null, false);

			PXUIFieldAttribute.SetVisible<CustomerClass.localeName>(CustomerClassRecord.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);
		}
	}
}
