using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Common;

namespace PX.Objects.PM
{
	public class SetupMaint : PXGraph<SetupMaint>
	{
		#region DAC Overrides

		#region PMProject
		[PXDBIdentity(IsKey = true)]
        protected virtual void PMProject_ContractID_CacheAttached(PXCache sender)
        { }

        [PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Project Template ID")]
        protected virtual void PMProject_ContractCD_CacheAttached(PXCache sender)
        { }

        [PXDBInt]
        protected virtual void PMProject_CustomerID_CacheAttached(PXCache sender)
        { }

        [PXDBBool]
        [PXDefault(true)]
        protected virtual void PMProject_NonProject_CacheAttached(PXCache sender)
        { }

        [PXDBInt]
        protected virtual void PMProject_TemplateID_CacheAttached(PXCache sender)
        { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void PMProject_CuryID_CacheAttached(PXCache sender)
		{ }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void PMProject_BillingCuryID_CacheAttached(PXCache sender)
		{ }
		#endregion

		#region PMCostCode
		[PXDBString(30, IsUnicode = true)]
		[PXDefault()]
		protected virtual void PMCostCode_CostCodeCD_CacheAttached(PXCache sender)
		{ }

		[PXDBIdentity(IsKey = true)]
		protected virtual void PMCostCode_CostCodeID_CacheAttached(PXCache sender)
		{ }
		#endregion

		#region Inventory Item

		[PXDefault()]
		[PXDBString(InputMask = "", IsUnicode = true)]//IsKey = false in order to update the <N/A>
		protected virtual void InventoryItem_InventoryCD_CacheAttached(PXCache sender)
		{ }

		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		protected virtual void InventoryItem_BaseUnit_CacheAttached(PXCache sender)
		{ }

		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		protected virtual void InventoryItem_SalesUnit_CacheAttached(PXCache sender)
		{ }

		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		protected virtual void InventoryItem_PurchaseUnit_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_ReasonCodeSubID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_SalesAcctID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_SalesSubID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_InvtAcctID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_InvtSubID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_COGSAcctID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_COGSSubID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_StdCstRevAcctID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_StdCstRevSubID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_StdCstVarAcctID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_StdCstVarSubID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_PPVAcctID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_PPVSubID_CacheAttached(PXCache sender)
		{ }
		[PXDBInt]
		protected virtual void InventoryItem_POAccrualAcctID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_POAccrualSubID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_LCVarianceAcctID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_LCVarianceSubID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_DeferralAcctID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_DeferralSubID_CacheAttached(PXCache sender)
		{ } 

		[PXDBInt]
		protected virtual void InventoryItem_DefaultSubItemID_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		protected virtual void InventoryItem_ItemClassID_CacheAttached(PXCache sender)
		{ }

		[PXDBString(10, IsUnicode = true)]
		protected virtual void InventoryItem_TaxCategoryID_CacheAttached(PXCache sender)
		{ }
		#endregion

		#region NotificationSetupRecipient
		[PXDBString(10)]
		[PXDefault]
		[NotificationContactType.ProjectTemplateList]
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

		#endregion

		public PXSelect<PMSetup> Setup;
		public PXSave<PMSetup> Save;
		public PXCancel<PMSetup> Cancel;
		public PXSetup<Company> Company;
		public PXSelect<PMProject,
			Where<PMProject.nonProject, Equal<True>>> DefaultProject;
		public PXSelect<PMCostCode,
			Where<PMCostCode.isDefault, Equal<True>>> DefaultCostCode;
		public PXSelect<InventoryItem,
		Where<InventoryItem.itemStatus, Equal<InventoryItemStatus.unknown>>> EmptyItem;

		public CRNotificationSetupList<PMNotification> Notifications;
		public PXSelect<NotificationSetupRecipient,
			Where<NotificationSetupRecipient.setupID, Equal<Current<PMNotification.setupID>>>> Recipients;
		
		public SetupMaint()
		{
			if (string.IsNullOrEmpty(Company.Current.BaseCuryID))
			{
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), PXMessages.LocalizeNoPrefix(CS.Messages.OrganizationMaint));
			}

			PXDefaultAttribute.SetPersistingCheck<PM.PMProject.defaultSubID>(DefaultProject.Cache, null, PXPersistingCheck.Nothing);
		}


		//public PXAction<PMSetup> debug;
		//[PXUIField(DisplayName = "Debug")]
		//[PXProcessButton]
		//public void Debug()
		//{
			
		//}

		protected virtual void _(Events.RowSelected<PMSetup> e)
		{
			EnsureDefaultCostCode(e.Row);

			PMProject rec = DefaultProject.SelectWindowed(0, 1);
			if (rec != null && IsInvalid(rec))
			{
				rec.IsActive = true;
				rec.Status = ProjectStatus.Active;
				rec.RestrictToEmployeeList = false;
				rec.RestrictToResourceList = false;
				rec.VisibleInAP = true;
				rec.VisibleInAR = true;
				rec.VisibleInCA = true;
				rec.VisibleInCR = true;
				rec.VisibleInEA = true;
				rec.VisibleInGL = true;
				rec.VisibleInIN = true;
				rec.VisibleInPO = true;
				rec.VisibleInSO = true;
				rec.VisibleInTA = true;
				rec.CustomerID = null;

				if (DefaultProject.Cache.GetStatus(rec) == PXEntryStatus.Notchanged)
					DefaultProject.Cache.SetStatus(rec, PXEntryStatus.Updated);

				DefaultProject.Cache.IsDirty = true;
			}
		}

		protected virtual void _(Events.RowSelected<NotificationSetup> e)
		{
			if (e.Row != null)
			{
				if (e.Row.NotificationCD == ProformaEntry.ProformaNotificationCD || e.Row.NotificationCD == ChangeOrderEntry.ChangeOrderNotificationCD)
				{
					PXUIFieldAttribute.SetEnabled<NotificationSetup.active>(e.Cache, e.Row, false);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<NotificationSetup.active>(e.Cache, e.Row, true);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMSetup, PMSetup.costCommitmentTracking> e)
		{
			PXPageCacheUtils.InvalidateCachedPages();
		}

		public virtual void PMSetup_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PMSetup row = (PMSetup)e.Row;
			if (row == null) return;

            PMProject rec = DefaultProject.SelectWindowed(0, 1);
            if (rec == null)
            {
                InsertDefaultProject(row);
            }
            else
            {
                rec.ContractCD = row.NonProjectCode;
				rec.IsActive = true;
				rec.Status = ProjectStatus.Active;
				rec.VisibleInAP = true;
				rec.VisibleInAR = true;
				rec.VisibleInCA = true;
				rec.VisibleInCR = true;
				rec.VisibleInEA = true;
				rec.VisibleInGL = true;
				rec.VisibleInIN = true;
				rec.VisibleInPO = true;
				rec.VisibleInSO = true;
				rec.VisibleInTA = true;
				rec.RestrictToEmployeeList = false;
				rec.RestrictToResourceList = false;

	            DefaultProject.Cache.MarkUpdated(rec);
            }

			EnsureDefaultCostCode(row);
			EnsureEmptyItem(row);
		}
		public virtual void PMSetup_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PMProject rec = DefaultProject.SelectWindowed(0, 1);
			PMSetup row = (PMSetup)e.Row;
			if (row == null) return;

			if(rec == null)
			{
				InsertDefaultProject(row);
			}
			else if(!sender.ObjectsEqual<PMSetup.nonProjectCode>(e.Row, e.OldRow))
			{
				rec.ContractCD = row.NonProjectCode;
				DefaultProject.Cache.MarkUpdated(rec);
			}

			InventoryItem item = EmptyItem.SelectWindowed(0, 1);
			
			if (item == null)
			{
				InsertEmptyItem(row);
			}
			else if (!sender.ObjectsEqual<PMSetup.emptyItemCode>(e.Row, e.OldRow))
			{
				item.InventoryCD = row.EmptyItemCode;
				
				if (EmptyItem.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
					EmptyItem.Cache.SetStatus(item, PXEntryStatus.Updated);
			}

			EnsureEmptyItem(row);
		}

		public virtual bool IsInvalid(PMProject nonProject)
		{
			if (nonProject.IsActive == false) return true;
			if (nonProject.Status != ProjectStatus.Active) return true;
			if (nonProject.RestrictToEmployeeList == true) return true;
			if (nonProject.RestrictToResourceList == true) return true;
			if (nonProject.VisibleInAP == false) return true;
			if (nonProject.VisibleInAR == false) return true;
			if (nonProject.VisibleInCA == false) return true;
			if (nonProject.VisibleInCR == false) return true;
			if (nonProject.VisibleInEA == false) return true;
			if (nonProject.VisibleInGL == false) return true;
			if (nonProject.VisibleInIN == false) return true;
			if (nonProject.VisibleInPO == false) return true;
			if (nonProject.VisibleInSO == false) return true;
			if (nonProject.VisibleInTA == false) return true;
			if (nonProject.CustomerID != null) return true;

			return false;
		}
		
		public virtual void InsertDefaultProject(PMSetup row)
		{
			PMProject rec = new PMProject();
			rec.CustomerID = null;
			rec.ContractCD = row.NonProjectCode;
			rec.Description = PXLocalizer.Localize(Messages.NonProjectDescription);
			PXDBLocalizableStringAttribute.SetTranslationsFromMessage<PMProject.description>
				(Caches[typeof(PMProject)], rec, Messages.NonProjectDescription);
			rec.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
			rec.IsActive = true;
			rec.Status = ProjectStatus.Active;
			rec.ServiceActivate = false;
			rec.VisibleInAP = true;
			rec.VisibleInAR = true;
			rec.VisibleInCA = true;
			rec.VisibleInCR = true;
			rec.VisibleInEA = true;
			rec.VisibleInGL = true;
			rec.VisibleInIN = true;
			rec.VisibleInPO = true;
			rec.VisibleInSO = true;
			rec.VisibleInTA = true;
			rec = DefaultProject.Insert(rec);
		}

		public virtual void EnsureDefaultCostCode(PMSetup row)
		{
			PMCostCode costcode = DefaultCostCode.SelectWindowed(0, 1);
			if (costcode == null)
			{
				InsertDefaultCostCode(row);
			}
			else 
			{
				if (costcode.CostCodeCD.Length != GetCostCodeLength() )
				{
					costcode.CostCodeCD = new string('0', GetCostCodeLength());
					if (DefaultCostCode.Cache.GetStatus(costcode) == PXEntryStatus.Notchanged)
						DefaultCostCode.Cache.SetStatus(costcode, PXEntryStatus.Updated);

					DefaultCostCode.Cache.IsDirty = true;
				}
				if (costcode.NoteID == null)
				{
					costcode.NoteID = Guid.NewGuid();
					if (DefaultCostCode.Cache.GetStatus(costcode) == PXEntryStatus.Notchanged)
						DefaultCostCode.Cache.SetStatus(costcode, PXEntryStatus.Updated);

					DefaultCostCode.Cache.IsDirty = true;
				}

			}
		}
		
		public virtual void InsertDefaultCostCode(PMSetup row)
		{
			PMCostCode rec = new PMCostCode();
			rec.CostCodeCD = new string('0', GetCostCodeLength());
			rec.Description = "DEFAULT";
			rec.IsDefault = true;
			rec = DefaultCostCode.Insert(rec);
		}

		public virtual short GetCostCodeLength()
		{
			Dimension dm = PXSelect<Dimension, Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>.Select(this, CostCodeAttribute.COSTCODE);

			if (dm != null && dm.Length != null)
			{
				return dm.Length.Value;
			}
			else
			{
				return 4;
			}
		}

		public virtual void EnsureEmptyItem(PMSetup row)
		{
			InventoryItem item = EmptyItem.SelectWindowed(0, 1);
			if (item == null)
			{
				InsertEmptyItem(row);
			}
			else
			{
				UpdateEmptyItem(item, row);
			}
		}

		public virtual void InsertEmptyItem(PMSetup row)
		{
			InventoryItem rec = new InventoryItem();
			rec.InventoryCD = row.EmptyItemCode;
			rec.ItemStatus = InventoryItemStatus.Unknown;
			rec.ItemType = INItemTypes.NonStockItem;
			rec.BaseUnit = row.EmptyItemUOM;
			rec.SalesUnit = row.EmptyItemUOM;
			rec.PurchaseUnit = row.EmptyItemUOM;
			rec.StkItem = false;
			rec.TaxCalcMode = TX.TaxCalculationMode.TaxSetting;
			rec = EmptyItem.Insert(rec);
		}

		protected virtual void UpdateEmptyItem(InventoryItem rec, PMSetup row)
		{
			if (rec.BaseUnit != row.EmptyItemUOM || rec.SalesUnit != row.EmptyItemUOM || rec.PurchaseUnit != row.EmptyItemUOM)
			{
				rec.BaseUnit = row.EmptyItemUOM;
				rec.SalesUnit = row.EmptyItemUOM;
				rec.PurchaseUnit = row.EmptyItemUOM;

				EmptyItem.Cache.MarkUpdated(rec);
			}
		}
	}
}
