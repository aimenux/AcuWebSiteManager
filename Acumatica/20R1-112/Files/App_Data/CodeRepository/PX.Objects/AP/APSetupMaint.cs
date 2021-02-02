using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;

namespace PX.Objects.AP
{
	public class APSetupMaint : PXGraph<APSetupMaint>
	{
		public PXSave<APSetup> Save;
		public PXCancel<APSetup> Cancel;
		public PXSelect<APSetup> Setup;
		public PXSelect<APSetupApproval> SetupApproval;
		//public PXSelect<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<APSetup.dfltVendorClassID>>>> DefaultVendorClass;
		public PXSelect<AP1099Box> Boxes1099;
		public PXSelect<Account, Where<Account.accountID,Equal<Required<Account.accountID>>>> Account_AccountID;
		public PXSelect<Account, Where<Account.box1099, Equal<Required<Account.box1099>>>> Account_Box1099;

		public CRNotificationSetupList<APNotification> Notifications;
		public PXSelect<NotificationSetupRecipient,
			Where<NotificationSetupRecipient.setupID, Equal<Current<APNotification.setupID>>>> Recipients;

		#region CacheAttached
		[PXDBString(10)]
		[PXDefault]
		[VendorContactType.ClassList]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckUnique(typeof(NotificationSetupRecipient.contactID),
			Where = typeof(Where<NotificationSetupRecipient.setupID, Equal<Current<NotificationSetupRecipient.setupID>>>))]
		public virtual void NotificationSetupRecipient_ContactType_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationSetupRecipient.contactType),
			typeof(Search2<Contact.contactID,
				LeftJoin<EPEmployee,
							On<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>,
							And<EPEmployee.defContactID, Equal<Contact.contactID>>>>,
				Where<Current<NotificationSetupRecipient.contactType>, Equal<NotificationContactType.employee>,
							And<EPEmployee.acctCD, IsNotNull>>>))]
		public virtual void NotificationSetupRecipient_ContactID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region Actions
		public PXAction<APSetup> viewAssignmentMap;
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ViewAssignmentMap(PXAdapter adapter)
		{
			if (SetupApproval.Current != null)
			{
				PXGraph graph = null;
				APSetupApproval setupApproval = SetupApproval.Current;
				EPAssignmentMap map = (EPAssignmentMap)PXSelect<EPAssignmentMap,
					Where<EPAssignmentMap.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>>>.Select(this, setupApproval.AssignmentMapID).First();
				if (map.MapType == EPMapType.Approval)
				{
					graph = PXGraph.CreateInstance<EPApprovalMapMaint>();
				}
				else if (map.MapType == EPMapType.Assignment)
				{
					graph = PXGraph.CreateInstance<EPAssignmentMapMaint>();
				}
				else if (map.MapType == EPMapType.Legacy && map.AssignmentMapID > 0)
				{
					graph = PXGraph.CreateInstance<EPAssignmentMaint>();
				}
				else
				{
					graph = PXGraph.CreateInstance<EPAssignmentAndApprovalMapEnq>();
				}

				PXRedirectHelper.TryRedirect(graph, map, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}
		#endregion
		#region Setups
		public CM.CMSetupSelect CMSetup;
		public PXSetup<GL.Company> Company;
		public PXSetup<GLSetup> GLSetup;
		#endregion

		public APSetupMaint()
		{
			GLSetup setup = GLSetup.Current;

            Boxes1099.Cache.AllowDelete = false;
            Boxes1099.Cache.AllowInsert = false;
            Boxes1099.Cache.AllowUpdate = true;

			PXUIFieldAttribute.SetVisible<APSetup.applyQuantityDiscountBy>(Setup.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.vendorDiscounts>() && PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>());
		}

		protected virtual void AP1099Box_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			AP1099Box row = (AP1099Box)e.Row;
            if (row == null || row.OldAccountID != null) return;
            
            Account acct = (Account)Account_Box1099.Select(row.BoxNbr);

			if (acct != null)
			{
                row.AccountID    = acct.AccountID;
                row.OldAccountID = acct.AccountID;
			}
		}

		protected virtual void AP1099Box_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Update)
			{
				e.Cancel = true;
				return;
			}

			foreach (AP1099Box box in Boxes1099.Cache.Updated)
			{
				if (box.OldAccountID != null && (box.AccountID == null || box.OldAccountID != box.AccountID))
				{
					Account acct = (Account)Account_AccountID.Select(box.OldAccountID);
					if (acct != null)
					{
						acct.Box1099 = null;
						Account_AccountID.Cache.Update(acct);
					}
				}

				if (box.AccountID != null && (box.OldAccountID == null || box.OldAccountID != box.AccountID))
				{
					Account acct = (Account)Account_AccountID.Select(box.AccountID);
					if (acct != null)
					{
						acct.Box1099 = box.BoxNbr;
						Account_AccountID.Cache.Update(acct);
					}
				}
			}
		}
        protected virtual void APSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
	        var row = (APSetup) e.Row;
            
			if (e.Row == null) 
				return;

            PXUIFieldAttribute.SetEnabled<APSetup.invoicePrecision>(sender, row, (row.InvoiceRounding != RoundingType.Currency));
            PXUIFieldAttribute.SetEnabled<APSetup.numberOfMonths>(sender, row, row.RetentionType == AR.RetentionTypeList.FixedNumOfMonths);

			VerifyInvoiceRounding(sender, row);

			if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() && !PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				PXDefaultAttribute.SetDefault<APSetup.vendorPriceUpdate>(sender, APVendorPriceUpdateType.Purchase);
				PXStringListAttribute.SetList<APSetup.vendorPriceUpdate>(sender, null, new string[] { APVendorPriceUpdateType.None, APVendorPriceUpdateType.Purchase, APVendorPriceUpdateType.ReleaseAPBill },
				new string[] { Messages.VendorUpdateNone, Messages.VendorUpdatePurchase, Messages.VendorUpdateAPBillRelease });
			}
		}

        protected virtual void APSetup_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            APSetup row = e.Row as APSetup;
            if (row == null) return;

            if (row != null && (!sender.ObjectsEqual<APSetup.retentionType>(e.Row, e.OldRow) || !sender.ObjectsEqual<APSetup.numberOfMonths>(e.Row, e.OldRow)))
            {
                if (row.RetentionType == AR.RetentionTypeList.LastPrice)
                    sender.RaiseExceptionHandling<APSetup.retentionType>(e.Row, ((APSetup)e.Row).RetentionType, new PXSetPropertyException(Messages.LastPriceWarning, PXErrorLevel.Warning));
                if (row.RetentionType == AR.RetentionTypeList.FixedNumOfMonths)
                {
					if (row.NumberOfMonths != 0) sender.RaiseExceptionHandling<APSetup.retentionType>(e.Row, ((APSetup)e.Row).RetentionType, new PXSetPropertyException(Messages.HistoricalPricesWarning, PXErrorLevel.Warning, row.NumberOfMonths));
					if (row.NumberOfMonths == 0) sender.RaiseExceptionHandling<APSetup.retentionType>(e.Row, ((APSetup)e.Row).RetentionType, new PXSetPropertyException(Messages.HistoricalPricesUnlimitedWarning, PXErrorLevel.Warning, row.NumberOfMonths));
                }
            }
        }

		private void VerifyInvoiceRounding(PXCache sender, APSetup row)
		{
			var hasError = false;
			if (row.InvoiceRounding != RoundingType.Currency)
			{
				var glSetup = GLSetup.Current;

				if (glSetup.RoundingLimit == 0m)
				{
					hasError = true;
					sender.RaiseExceptionHandling<APSetup.invoiceRounding>(row, null, new PXSetPropertyException(AR.Messages.ShouldSpecifyRoundingLimit, PXErrorLevel.Warning));
				}
			}

			if (!hasError)
			{
				sender.RaiseExceptionHandling<APSetup.invoiceRounding>(row, null, null);
			}
		}

		protected virtual void APSetup_MigrationMode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSetup row = e.Row as APSetup;
			if (row == null) return;

			bool? oldvalue = (bool?)e.OldValue;

			if (row.MigrationMode == true && oldvalue != true)
			{
				GLTran glTransactionFromModule = PXSelect<GLTran,
					Where<GLTran.module, Equal<BatchModule.moduleAP>>>.SelectSingleBound(this, null);
				if (glTransactionFromModule != null)
				{
					sender.RaiseExceptionHandling<APSetup.migrationMode>(row, row.MigrationMode,
						new PXSetPropertyException(Common.Messages.MigrationModeActivateGLTransactionFromModuleExist, PXErrorLevel.Warning));
				}
			}
			else if (row.MigrationMode != true && oldvalue == true)
			{
				APRegister unreleasedMigratedDocument = PXSelect<APRegister,
					Where<APRegister.released, NotEqual<True>,
						And<APRegister.isMigratedRecord, Equal<True>>>>.SelectSingleBound(this, null);
				if (unreleasedMigratedDocument != null)
				{
					sender.RaiseExceptionHandling<APSetup.migrationMode>(row, row.MigrationMode,
						new PXSetPropertyException(Common.Messages.MigrationModeDeactivateUnreleasedMigratedDocumentExist, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void APSetupApproval_DocType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSetupApproval row = e.Row as APSetupApproval;
			if (row == null)
			{
				return;
			}
			sender.SetDefaultExt<APSetupApproval.assignmentMapID>(row);
			sender.SetDefaultExt<APSetupApproval.assignmentNotificationID>(row);
		}
	}
}
