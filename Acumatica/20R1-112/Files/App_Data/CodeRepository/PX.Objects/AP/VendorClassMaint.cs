using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.GL;
using PX.Objects.EP.Standalone;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;


namespace PX.Objects.AP
{
	public class VendorClassMaint : PXGraph<VendorClassMaint>
	{

		#region Buttons Declaration
		public PXSave<VendorClass> Save;
		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual System.Collections.IEnumerable Cancel(PXAdapter a)
		{
			foreach (PXResult<VendorClass, EPEmployeeClass> e in (new PXCancel<VendorClass>(this, "Cancel")).Press(a))
			{
				if (VendorClassRecord.Cache.GetStatus((VendorClass)e) == PXEntryStatus.Inserted)
				{
					EPEmployeeClass e1 = PXSelect<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Required<EPEmployeeClass.vendorClassID>>>>.Select(this, ((VendorClass)e).VendorClassID);
					if (e1 != null)
					{
						VendorClassRecord.Cache.RaiseExceptionHandling<VendorClass.vendorClassID>((VendorClass)e, ((VendorClass)e).VendorClassID, new PXSetPropertyException(Messages.EmployeeClassExists));
					}
				}
				yield return e;
			}
		}

		public PXAction<VendorClass> cancel;
		public PXInsert<VendorClass> Insert;
		public PXCopyPasteAction<VendorClass> Edit; 
		public PXDelete<VendorClass> Delete;
		public PXFirst<VendorClass> First;
		public PXPrevious<VendorClass> Prev;
		public PXNext<VendorClass> Next;
		public PXLast<VendorClass> Last;
		#endregion


		public PXSelectJoin<VendorClass, LeftJoin<EPEmployeeClass, On<EPEmployeeClass.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<EPEmployeeClass.vendorClassID, IsNull>> VendorClassRecord;
		public PXSelect<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<VendorClass.vendorClassID>>>> CurVendorClassRecord;
		[PXViewName(CR.Messages.Attributes)]
        public CSAttributeGroupList<VendorClass, Vendor> Mapping;

		public CRClassNotificationSourceList<NotificationSource,
			VendorClass.vendorClassID, APNotificationSource.vendor> NotificationSources;

		public PXSelect<NotificationRecipient,
			Where<NotificationRecipient.refNoteID, IsNull,
				And<NotificationRecipient.sourceID, Equal<Optional<NotificationSource.sourceID>>>>> NotificationRecipients;

		public PXSelect<Vendor,
			Where<Vendor.vendorClassID, Equal<Current<VendorClass.vendorClassID>>>> Vendors;

		public PXAction<VendorClass> resetGroup;

		#region Cache Attached
		#region NotificationSource
		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search<NotificationSetup.setupID,
			Where<NotificationSetup.sourceCD, Equal<APNotificationSource.vendor>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
        [PXUIField(DisplayName = "Mailing ID")]
		protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(VendorClass.vendorClassID))]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select2<VendorClass,
			InnerJoin<NotificationSetup, On<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>,
			Where<VendorClass.vendorClassID, Equal<Current<NotificationSource.classID>>>>))]
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
				And<Where<SiteMap.screenID, Like<PXModule.ap_>,
							 Or<SiteMap.screenID, Like<PXModule.po_>,
							 Or<SiteMap.screenID, Like<PXModule.sc_>, 
							 Or<SiteMap.screenID, Like<PXModule.cl_>,
							 Or<SiteMap.screenID, Like<PXModule.rq_>>>>>>>>,
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
		[VendorContactType.ClassList]
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

		[PXProcessButton]
        [PXUIField(DisplayName = "Apply Restriction Settings to All Vendors")]
		protected virtual IEnumerable ResetGroup(PXAdapter adapter)
		{
			if (VendorClassRecord.Ask(Messages.Warning, Messages.GroupUpdateConfirm, MessageButtons.OKCancel) == WebDialogResult.OK)
			{
				Save.Press();
				string classID = VendorClassRecord.Current.VendorClassID;
				PXLongOperation.StartOperation(this, delegate()
				{
					Reset(classID);
				});
			}
			return adapter.Get();
		}
		protected static void Reset(string classID)
		{
			VendorClassMaint graph = PXGraph.CreateInstance<VendorClassMaint>();
			graph.VendorClassRecord.Current = graph.VendorClassRecord.Search<VendorClass.vendorClassID>(classID);
			if (graph.VendorClassRecord.Current != null)
			{
				foreach (Vendor vend in graph.Vendors.Select())
				{
					vend.GroupMask = graph.VendorClassRecord.Current.GroupMask;
					graph.Vendors.Cache.SetStatus(vend, PXEntryStatus.Updated);
				}
				graph.Save.Press();
			}
		}
		public PXSelect<PX.SM.Neighbour> Neighbours;
		public override void Persist()
		{
			if (VendorClassRecord.Current != null)
			{
				CS.SingleGroupAttribute.PopulateNeighbours<VendorClass.groupMask>(VendorClassRecord, Neighbours, typeof(PX.SM.Users), typeof(PX.SM.Users));
				CS.SingleGroupAttribute.PopulateNeighbours<VendorClass.groupMask>(VendorClassRecord, Neighbours, typeof(Vendor), typeof(Vendor));
				CS.SingleGroupAttribute.PopulateNeighbours<VendorClass.groupMask>(VendorClassRecord, Neighbours, typeof(VendorClass), typeof(VendorClass));
				CS.SingleGroupAttribute.PopulateNeighbours<VendorClass.groupMask>(VendorClassRecord, Neighbours, typeof(PX.SM.Users), typeof(Vendor));
				CS.SingleGroupAttribute.PopulateNeighbours<VendorClass.groupMask>(VendorClassRecord, Neighbours, typeof(Vendor), typeof(PX.SM.Users));
				CS.SingleGroupAttribute.PopulateNeighbours<VendorClass.groupMask>(VendorClassRecord, Neighbours, typeof(PX.SM.Users), typeof(VendorClass));
				CS.SingleGroupAttribute.PopulateNeighbours<VendorClass.groupMask>(VendorClassRecord, Neighbours, typeof(VendorClass), typeof(PX.SM.Users));
				CS.SingleGroupAttribute.PopulateNeighbours<VendorClass.groupMask>(VendorClassRecord, Neighbours, typeof(VendorClass), typeof(Vendor));
				CS.SingleGroupAttribute.PopulateNeighbours<VendorClass.groupMask>(VendorClassRecord, Neighbours, typeof(Vendor), typeof(VendorClass));
			}
			base.Persist();
			GroupHelper.Clear();
		}
		
		#region Setups
		public PXSetup<Company> Company;
		public PXSetup<GLSetup> GLSetup;

        #endregion

		public VendorClassMaint()
		{
			GLSetup setup = GLSetup.Current;

			PXUIFieldAttribute.SetVisible<VendorClass.localeName>(VendorClassRecord.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);
		}

		public virtual void VendorClass_CashAcctID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			//cache.SetDefaultExt<VendorClass.paymentMethodID>(e.Row);
		}

		public virtual void VendorClass_CashAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			VendorClass row = (VendorClass)e.Row;
			if (row != null)
			{

				VendorClass defaultVClass = PXSelectJoin<VendorClass,
						InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>.Select(this);

				if (defaultVClass != null && row.VendorClassID!= defaultVClass.VendorClassID 
						&& row.PaymentMethodID == defaultVClass.PaymentMethodID)
				{
					e.NewValue = defaultVClass.CashAcctID;
					e.Cancel = true;
				}
				else
				{
					e.NewValue = null;
					e.Cancel = true;
				}
			}
		}

		public virtual void VendorClass_PaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e) 
		{
			cache.SetDefaultExt<VendorClass.cashAcctID>(e.Row);
		}

        protected virtual void VendorClass_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
	        var mcFeatureActivated = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<VendorClass.curyID>(cache, null, mcFeatureActivated);
			PXUIFieldAttribute.SetVisible<VendorClass.curyRateTypeID>(cache, null, mcFeatureActivated);
			PXUIFieldAttribute.SetVisible<VendorClass.allowOverrideCury>(cache, null, mcFeatureActivated);
			PXUIFieldAttribute.SetVisible<VendorClass.allowOverrideRate>(cache, null, mcFeatureActivated);
			PXUIFieldAttribute.SetVisible<VendorClass.unrealizedGainAcctID>(cache, null, mcFeatureActivated);
			PXUIFieldAttribute.SetVisible<VendorClass.unrealizedGainSubID>(cache, null, mcFeatureActivated);
			PXUIFieldAttribute.SetVisible<VendorClass.unrealizedLossAcctID>(cache, null, mcFeatureActivated);
			PXUIFieldAttribute.SetVisible<VendorClass.unrealizedLossSubID>(cache, null, mcFeatureActivated);
			if (e.Row!=null)
			{
				VendorClass row = (VendorClass)e.Row; 
				PXUIFieldAttribute.SetEnabled<VendorClass.cashAcctID>(cache, e.Row,String.IsNullOrEmpty(row.PaymentMethodID) == false);
			}
		}

        public virtual void VendorClass_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            VendorClass vclass = (VendorClass)e.Row;
            if (vclass == null) return;

            APSetup setup = PXSelect<APSetup>.Select(this);
            if (setup != null && vclass.VendorClassID == setup.DfltVendorClassID)
            {
                throw new PXException(Messages.VendorClassCanNotBeDeletedBecauseItIsUsed);
            }
        }
        
        protected virtual void VendorClass_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void VendorClass_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() == false)
			{
				e.Cancel = true;
			}
		}

		protected virtual void VendorClass_CuryRateTypeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() == false)
			{
				e.Cancel = true;
			}
		}

		public virtual void VendorClass_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			VendorClass row = (VendorClass)e.Row;
			PXSelectBase<CashAccount> select = new PXSelect<CashAccount, Where<CashAccount.cashAccountID,
								Equal<Required<VendorClass.cashAcctID>>>>(this);
			if (!String.IsNullOrEmpty(row.CuryID) && (row.AllowOverrideCury??false) != true)
			{
				CashAccount acct = select.Select(row.CashAcctID);
				if (acct != null)
				{
					if (row.CuryID != acct.CuryID)
					{
                        if (cache.RaiseExceptionHandling<VendorClass.cashAcctID>(e.Row, acct.CashAccountCD, new PXSetPropertyException(Messages.VendorCuryDifferentDefPayCury, typeof(VendorClass.cashAcctID).Name)))
						{
							throw new PXRowPersistingException(typeof(VendorClass.cashAcctID).Name, null, Messages.VendorCuryDifferentDefPayCury, typeof(VendorClass.cashAcctID).Name);
						}
					}
				}
			}
			if (VendorClassRecord.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted)
			{
				EPEmployeeClass e1 = PXSelect<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<VendorClass.vendorClassID>>>>.SelectSingleBound(this, new object[]{e.Row});
				if (e1 != null)
				{
					cache.IsDirty = false;
					e.Cancel = true;
					throw new PXRowPersistingException(typeof(VendorClass.vendorClassID).Name, null, Messages.EmployeeClassExists);
				}
			}

		}

	}
}
