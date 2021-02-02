using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.IN.PhysicalInventory;

namespace PX.Objects.IN
{
	public class INPIController : PXGraph<INPIController>
	{
		public PXSave<INPIHeader> Save;
		public PXCancel<INPIHeader> Cancel;

		public PXSelectJoin<INPIHeader, 
			InnerJoin<INSite, 
				On<INPIHeader.FK.Site>>, 
			Where<Match<INSite, Current<AccessInfo.userName>>>> PIHeader;
		public PXSelect<INPIDetailUpdate> PIDetailUpdate;

		public PXSelect<INPIStatusItem> PIStatusItem;
		public PXSelect<INPIStatusLoc> PIStatusLoc;

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		protected virtual void INPIStatusItem_PIID_CacheAttached(PXCache sender)
		{ 
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		protected virtual void INPIStatusLoc_PIID_CacheAttached(PXCache sender)
		{ 
		}

		public virtual void ReopenPI(string piId)
		{
			INPIHeader header = PIHeader.Current = PIHeader.Search<INPIHeader.pIID>(piId);
			if (header?.Status != INPIHdrStatus.InReview) 
				return;

			header.Status = INPIHdrStatus.Entering;
			header.PIAdjRefNbr = null;
			PIHeader.Update(header);

			Save.Press();
		}

		public virtual INPIDetailUpdate AccumulateFinalCost(string piId, int piLineNbr, decimal costAmt)
		{
			var row = new INPIDetailUpdate
			{
				PIID = piId,
				LineNbr = piLineNbr,
			};
			row = this.PIDetailUpdate.Insert(row);

			row.FinalExtVarCost += costAmt;

			return this.PIDetailUpdate.Update(row);
		}

		public virtual void ReleasePI(string piId)
		{
			var header = PIHeader.Current = PIHeader.Search<INPIHeader.pIID>(piId);
			CreatePILocksManager().UnlockInventory();
			header.TotalVarCost = this.PIDetailUpdate.Cache.Inserted.RowCast<INPIDetailUpdate>().Sum(d => d.FinalExtVarCost) ?? 0m;
			header.Status = INPIHdrStatus.Completed;
			PIHeader.Update(header);

			Save.Press();
		}

		protected virtual PILocksManager CreatePILocksManager()
		{
			INPIHeader header = PIHeader.Current;
			return new PILocksManager(this, PIStatusItem, PIStatusLoc, (int)header.SiteID, header.PIID);
		}
	}

    [Serializable]
	public class INPIEntry : INPIController, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
	{
		#region Menu
		public PXAction<INPIHeader> Insert;
		public PXCopyPasteAction<INPIHeader> CopyPaste;
		public PXDelete<INPIHeader> Delete;
		public PXFirst<INPIHeader> First;
		public PXPrevious<INPIHeader> Previous;
		public PXNext<INPIHeader> Next;
		public PXLast<INPIHeader> Last;
		#endregion

		[PXFilterable]
		[PXImport(typeof(INPIHeader))]
		public PXSelectJoin<INPIDetail,
			LeftJoin<InventoryItem, On<INPIDetail.FK.InventoryItem>,
			LeftJoin<INSubItem, On<INPIDetail.FK.SubItem>>>,
			Where<INPIDetail.pIID, Equal<Optional<INPIHeader.pIID>>,
				And<Where<INPIDetail.inventoryID, IsNull, Or<InventoryItem.inventoryID, IsNotNull>>>>,
			OrderBy<Asc<INPIDetail.lineNbr>>> PIDetail;
		public PXSelect<INPIDetail, Where<INPIDetail.pIID, Equal<Current<INPIHeader.pIID>>>> PIDetailPure;
		public PXSelect<INPIHeader, Where<INPIHeader.pIID, Equal<Current<INPIHeader.pIID>>>> PIHeaderInfo;
		public PXSelect<INSetup> INSetup;
		public PXSetup<INSite, Where<INSite.siteID, Equal<Current<INPIHeader.siteID>>>> insite;
		public PXFilter<PIGeneratorSettings> GeneratorSettings;
		public INBarCodeItemLookup<INBarCodeItem> AddByBarCode;
		public PXSetup<INSetup> Setup;

		/// <summary><see cref="INPIHeader.PIID"/> CacheAttached</summary>
		[PXMergeAttributes]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXSelector(typeof(Search2<INPIHeader.pIID, 
			InnerJoin<INSite, 
				On<INPIHeader.FK.Site>>, 
			Where<Match<INSite, Current<AccessInfo.userName>>>, OrderBy<Desc<INPIHeader.pIID>>>), Filterable = true)]
		protected void INPIHeader_PIID_CacheAttached(PXCache sender) { }

		public bool DisableCostCalculation = false;

		public virtual bool IsCostCalculationEnabled()
		{
			INPIHeader header = PIHeader.Current;
			return !DisableCostCalculation && header?.Status == INPIHdrStatus.Entering;
		}

		public INPIEntry()
		{
			PXDefaultAttribute.SetPersistingCheck<PIGeneratorSettings.randomItemsLimit>(GeneratorSettings.Cache, null, PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<PIGeneratorSettings.lastCountPeriod>(GeneratorSettings.Cache, null, PXPersistingCheck.Nothing);
		}

		[PXUIField(DisplayName = ActionsMessages.Insert, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		[PXInsertButton]
		protected virtual IEnumerable insert(PXAdapter adapter)
		{
			if(GeneratorSettings.AskExt((graph, name) => ResetGenerateSettings(graph)) == WebDialogResult.OK && GeneratorSettings.VerifyRequired())
			{
                PIGenerator generator = PXGraph.CreateInstance<PIGenerator>();
				generator.GeneratorSettings.Current = GeneratorSettings.Current;
				generator.CalcPIRows(true);
				if (generator.piheader.Current != null)
				{
					this.Clear();
					return PIHeader.Search<INPIHeader.pIID>(generator.piheader.Current.PIID);
				}
			}
			return adapter.Get();
		}

		private static void ResetGenerateSettings(PXGraph graph)
		{
			INPIEntry entry = graph as INPIEntry;
			if (entry == null) return;
			entry.GeneratorSettings.Cache.Clear();
			entry.GeneratorSettings.Insert(new PIGeneratorSettings());
		}

		#region AddLineByBarCode

		public PXAction<INPIHeader> addLine;
		[PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXLookupButton(Tooltip = Messages.AddNewLine)]
		public virtual IEnumerable AddLine(PXAdapter adapter)
		{
			if (AddByBarCode.AskExt(
				(graph, view) => ((INPIEntry)graph).AddByBarCode.Reset(false)) == WebDialogResult.OK &&
				this.AddByBarCode.VerifyRequired())
				UpdatePhysicalQty();
			return adapter.Get();
		}

		public PXAction<INPIHeader> addLine2;
		[PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXLookupButton(Tooltip = Messages.AddNewLine)]
		public virtual IEnumerable AddLine2(PXAdapter adapter)
		{
			if (this.AddByBarCode.VerifyRequired())
			{
				UpdatePhysicalQty();
			}
			return adapter.Get();
		}

		protected virtual void INBarCodeItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<INBarCodeItem.uOM>(this.AddByBarCode.Cache, null, false);
		}

		protected virtual void INBarCodeItem_ExpireDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INPIDetail exists =
			PXSelectReadonly<INPIDetail,
				Where<INPIDetail.pIID, Equal<Current<INPIHeader.pIID>>,
					And<INPIDetail.inventoryID, Equal<Current<INBarCodeItem.inventoryID>>,
						And<INPIDetail.lotSerialNbr, Equal<Current<INBarCodeItem.lotSerialNbr>>>>>>.SelectWindowed(this, 0, 1);
			if (exists != null)
			{
				e.NewValue = exists.ExpireDate;
				e.Cancel = true;
			}
		}

        protected virtual void INBarCodeItem_SiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (this.PIHeader.Current != null)
                e.NewValue = this.PIHeader.Current.SiteID;
            e.Cancel = true;
        }

		protected virtual void INBarCodeItem_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			INBarCodeItem row = (INBarCodeItem)e.Row;
			if (row != null && row.AutoAddLine == true && this.AddByBarCode.VerifyRequired(true) && row.Qty > 0)
				UpdatePhysicalQty();
		}

		private void UpdatePhysicalQty()
		{
			INBarCodeItem item = AddByBarCode.Current;

			this.SelectTimeStamp();

			INPIDetail detail =
			PXSelect<INPIDetail,
				Where<INPIDetail.pIID, Equal<Current<INPIHeader.pIID>>,
					And<INPIDetail.inventoryID, Equal<Current<INBarCodeItem.inventoryID>>,
						And<INPIDetail.subItemID, Equal<Current<INBarCodeItem.subItemID>>,
							And<INPIDetail.locationID, Equal<Current<INBarCodeItem.locationID>>,
								And<Where<INPIDetail.lotSerialNbr, IsNull,
									Or<INPIDetail.lotSerialNbr, Equal<Current<INBarCodeItem.lotSerialNbr>>>>>>>>>>.SelectWindowed(this, 0, 1);
			if (detail == null)
			{
				detail = new INPIDetail { InventoryID = item.InventoryID };
				detail = PXCache<INPIDetail>.CreateCopy(this.PIDetail.Insert(detail));
				detail.SubItemID = item.SubItemID;
				detail.LocationID = item.LocationID;
				detail.LotSerialNbr = item.LotSerialNbr;
				detail = PXCache<INPIDetail>.CreateCopy(this.PIDetail.Update(detail));
				detail.PhysicalQty = item.Qty;
				detail.ExpireDate = item.ExpireDate;
			}
			else
			{
				detail = PXCache<INPIDetail>.CreateCopy(detail);
				detail.PhysicalQty = detail.PhysicalQty.GetValueOrDefault() + item.Qty.GetValueOrDefault();
			}
			if (!string.IsNullOrEmpty(item.ReasonCode))
				detail.ReasonCode = item.ReasonCode;
			this.PIDetail.Update(detail);

			item.Description = PXMessages.LocalizeFormatNoPrefixNLA(Messages.PILineUpdated,
																			 AddByBarCode.GetValueExt<INBarCodeItem.inventoryID>(item).ToString().Trim(),
																			 Setup.Current.UseInventorySubItem == true ? ":" + AddByBarCode.GetValueExt<INBarCodeItem.subItemID>(item) : string.Empty,
																			 AddByBarCode.GetValueExt<INBarCodeItem.qty>(item),
																			 item.UOM,
																			 detail.LineNbr);
			AddByBarCode.Reset(true);
			this.AddByBarCode.View.RequestRefresh();
		}

		#endregion

		#region Events

		protected virtual void INPIHeader_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (this.IsContractBasedAPI)
            {
                return;
            }

			if (e.Row == null) { return; }
			INPIHeader h = (INPIHeader)e.Row;

			PIHeader.Cache.AllowDelete = h.Status != INPIHdrStatus.InReview && h.Status != INPIHdrStatus.Completed;

			PIHeader.Cache.AllowUpdate =
			PIDetail.Cache.AllowInsert =
			PIDetail.Cache.AllowDelete =
			PIDetail.Cache.AllowUpdate = (h.Status == INPIHdrStatus.Counting) || (h.Status == INPIHdrStatus.Entering);

            addLine.SetEnabled(PIDetail.Cache.AllowUpdate);
        }

		protected virtual void INPIHeader_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			INPIHeader h = (INPIHeader)e.Row;
			if (h == null)
				return;

			CreatePILocksManager().UnlockInventory();
		}

		protected virtual void INPIDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var detail = (INPIDetail)e.Row;
			if (detail == null)
				return;

			INLotSerClass lotSer = SelectLotSerClass(detail.InventoryID);
			bool notNormal = detail.LineType != INPIDetLineType.Normal;
			bool notNormalSerial = notNormal & LSRequired(lotSer);
			bool requestExpireDate = notNormalSerial && lotSer.LotSerTrackExpiration == true;
			bool isDebitLine = detail?.VarQty > 0;
			bool isEntering = PIHeader.Current.Status == INPIHdrStatus.Entering;

			PXUIFieldAttribute.SetEnabled<INPIDetail.inventoryID>(sender, detail, notNormal);
			PXUIFieldAttribute.SetEnabled<INPIDetail.subItemID>(sender, detail, notNormal);
			PXUIFieldAttribute.SetEnabled<INPIDetail.locationID>(sender, detail, notNormal);
			PXUIFieldAttribute.SetEnabled<INPIDetail.lotSerialNbr>(sender, detail, notNormalSerial);
			PXUIFieldAttribute.SetEnabled<INPIDetail.expireDate>(sender, detail, requestExpireDate);
			PXUIFieldAttribute.SetEnabled<INPIDetail.physicalQty>(sender, detail, AreKeysFieldsEntered(detail));
			PXUIFieldAttribute.SetEnabled<INPIDetail.unitCost>(sender, detail, isEntering && isDebitLine);
			PXUIFieldAttribute.SetEnabled<INPIDetail.manualCost>(sender, detail, isEntering && isDebitLine);
			PXUIFieldAttribute.SetVisible<INPIDetail.tagNumber>(sender, null, INSetup.Current.PIUseTags == true);
		}

		public virtual decimal GetBookQty(INPIDetail detail)
		{
			if (!LSRequired(detail.InventoryID))
			{
				ReadOnlyLocationStatus status = ReadOnlyLocationStatus.PK.Find(this, detail.InventoryID, detail.SubItemID, detail.SiteID, detail.LocationID);
				
				return status?.QtyActual ?? 0m;
			}
			else
			{
				ReadOnlyLotSerialStatus status = ReadOnlyLotSerialStatus.PK.Find(this, detail.InventoryID, detail.SubItemID, detail.SiteID, detail.LocationID, detail.LotSerialNbr);
				
				return status?.QtyActual ?? 0m;
			}
		}

		protected virtual void INPIDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var detail = (INPIDetail)e.Row;
			if (detail != null)
			{
				detail.BookQty = GetBookQty(detail);
			}
		}

		protected virtual void INPIDetail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var detail = (INPIDetail)e.NewRow;
			if (detail != null && !sender.ObjectsEqual<INPIDetail.inventoryID, INPIDetail.siteID, INPIDetail.subItemID, INPIDetail.locationID, INPIDetail.lotSerialNbr>(e.Row, e.NewRow))
			{
				detail.BookQty = GetBookQty(detail);
			}
		}

		protected virtual void INPIDetail_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			INPIDetail d = (INPIDetail)e.Row;
			if(d.LineType != INPIDetLineType.UserEntered && e.ExternalCall)
				throw new PXException(Messages.PILineDeleted);
		}

		#region _FieldVerifying events

		protected virtual void INPIDetail_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INPIDetail detail = (INPIDetail)e.Row;
			if (e.NewValue == null || detail == null)
				return;

			try
			{
				ValidateDuplicate(detail.Status, (int?)e.NewValue, detail.SubItemID, detail.LocationID, detail.LotSerialNbr, detail.LineNbr);
				ValidatePIInventoryLocation((int?)e.NewValue, detail.LocationID);
			}
			catch (PXSetPropertyException ex)
			{
				var invalidItem = InventoryItem.PK.Find(this, (int)e.NewValue);
				e.NewValue = invalidItem?.InventoryCD;
				throw ex;
			}
		}

		protected virtual void INPIDetail_SubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INPIDetail detail = (INPIDetail)e.Row;
			if (e.NewValue as int? == null || detail == null || !PXAccess.FeatureInstalled<FeaturesSet.subItem>())
				return;

			try
			{
				ValidateDuplicate(detail.Status, detail.InventoryID, (int?) e.NewValue, detail.LocationID, detail.LotSerialNbr, detail.LineNbr);
			}
			catch (PXSetPropertyException ex)
			{
				var subItem = INSubItem.PK.Find(this, (int)e.NewValue);
				e.NewValue = subItem?.SubItemCD;
				throw ex;
			}
		}
		
		protected virtual void INPIDetail_LocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INPIDetail detail = (INPIDetail)e.Row;
			if (e.NewValue as int? == null || detail == null)
				return;

			try
			{
				ValidateDuplicate(detail.Status, detail.InventoryID, detail.SubItemID, (int?)e.NewValue, detail.LotSerialNbr, detail.LineNbr);
				ValidatePIInventoryLocation(detail.InventoryID, (int?)e.NewValue);
			}
			catch (PXSetPropertyException ex)
			{
				var invalidLocation = INLocation.PK.Find(this, (int)e.NewValue);
				e.NewValue = invalidLocation?.LocationCD;
				throw ex;
			}
		}

		protected virtual void INPIDetail_LotSerialNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (((e.NewValue as string) == null) || (e.Row == null))
			{
				return;
			}
			INPIDetail d = (INPIDetail)e.Row;

			ValidateDuplicate(d.Status, d.InventoryID, d.SubItemID, d.LocationID, (string) e.NewValue, d.LineNbr);
		}

		protected virtual void INPIDetail_PhysicalQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if(e.NewValue == null) return;

			decimal value = (decimal) e.NewValue;
			INPIDetail d = (INPIDetail)e.Row;
			INLotSerClass inclass = SelectLotSerClass(d.InventoryID);
			if ((decimal?)e.NewValue < 0m)			
				throw new PXSetPropertyException(CS.Messages.Entry_GE, PXErrorLevel.Error, (int)0);
		
			if (inclass != null &&
				LSRequired(inclass) && 
				inclass.LotSerTrack == INLotSerTrack.SerialNumbered &&
				 value != 0m && value != 1m)
				throw new PXSetPropertyException(Messages.PIPhysicalQty);

		}

		#endregion _FieldVerifying events

		protected virtual void INPIDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INPIDetail detail = e.Row as INPIDetail;

			if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
				&& detail.LineType != INPIDetLineType.Normal && detail.Status == INPIDetStatus.Entered)
			{
				CheckDefault<INPIDetail.inventoryID>(sender, detail);
				CheckDefault<INPIDetail.subItemID>(sender, detail);
				CheckDefault<INPIDetail.locationID>(sender, detail);
				INLotSerClass lotSer = SelectLotSerClass(detail.InventoryID);
				if (LSRequired(lotSer))
				{
					CheckDefault<INPIDetail.lotSerialNbr>(sender, detail);
					if (lotSer.LotSerTrackExpiration == true)
						CheckDefault<INPIDetail.expireDate>(sender, detail);
				}
			}

			if (detail != null && sender.GetStatus(detail) == PXEntryStatus.Inserted)
			{
				INSetup setup = INSetup.Select();

				if (setup.PIUseTags == true)
				{
					setup.PILastTagNumber++;
					detail.TagNumber = setup.PILastTagNumber;
					INSetup.Update(setup);
				}
			}
		}

		protected virtual void INSetup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
			{
				PXDefaultAttribute.SetPersistingCheck<INSetup.iNTransitAcctID>(sender, e.Row, PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<INSetup.iNTransitSubID>(sender, e.Row, PXPersistingCheck.Nothing);
			}
		}

		private static void CheckDefault<Field>(PXCache sender, INPIDetail row)
			where Field : IBqlField
		{
			string fieldname = typeof (Field).Name;
			if(sender.GetValue<Field>(row) == null)
				if (sender.RaiseExceptionHandling(fieldname, row, null,
					new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, fieldname))))
					throw new PXRowPersistingException(fieldname, null, ErrorMessages.FieldIsEmpty, fieldname);
		}

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Type ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INPIClass.pIClassID, Where<INPIClass.method, Equal<PIMethod.fullPhysicalInventory>>>))]
		[PXDefault()]
		protected virtual void PIGeneratorSettings_PIClassID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void PIGeneratorSettings_PIClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PIGeneratorSettings row = (PIGeneratorSettings)e.Row;
			if (row == null) { return; }
			INPIClass pi = (INPIClass)PXSelectorAttribute.Select<PIGeneratorSettings.pIClassID>(sender, row);
			if (pi != null)
			{
				PXCache source = this.Caches[typeof(INPIClass)];
				foreach (string field in sender.Fields)
				{
					if (string.Compare(field, typeof(PIGeneratorSettings.pIClassID).Name, true) != 0 &&
						source.Fields.Contains(field))
						sender.SetValuePending(row, field, source.GetValueExt(pi, field));
				}
			}
		}

		protected virtual void INPIDetail_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INPIDetail detail = (INPIDetail)e.Row;
			if (detail == null || PIHeader.Cache.Current == null)
				return;

			if (detail.InventoryID != null)
				sender.SetDefaultExt<INPIDetail.subItemID>(detail);
			else
				sender.SetValue<INPIDetail.subItemID>(detail, null);

			if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
				sender.SetValue<INPIDetail.locationID>(detail, null);
			sender.SetValue<INPIDetail.lotSerialNbr>(detail, null);
			sender.SetValueExt<INPIDetail.physicalQty>(detail, null);
		}

		protected virtual void INPIDetail_SubItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INPIDetail detail = (INPIDetail)e.Row;
			if (detail == null || PIHeader.Cache.Current == null)
				return;

			sender.SetValueExt<INPIDetail.physicalQty>(detail, null);
		}

		protected virtual void INPIDetail_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INPIDetail detail = (INPIDetail)e.Row;
			if (detail == null || PIHeader.Cache.Current == null)
				return;

			if (detail.LocationID == null)
				sender.SetValueExt<INPIDetail.physicalQty>(detail, null);
		}

		protected virtual void INPIDetail_LotSerialNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INPIDetail detail = (INPIDetail)e.Row;
			if (detail == null || PIHeader.Cache.Current == null)
				return;

			if (string.IsNullOrWhiteSpace(detail.LotSerialNbr))
				sender.SetValueExt<INPIDetail.physicalQty>(detail, null);
		}

		protected virtual void INPIDetail_PhysicalQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INPIDetail detail = (INPIDetail)e.Row;
			if (detail == null)
				return;

			if (!AreKeysFieldsEntered(detail) || detail.PhysicalQty == null)
			{
				sender.SetValue<INPIDetail.manualCost>(detail, false);
				sender.SetValue<INPIDetail.varQty>(detail, null);
				sender.SetValueExt<INPIDetail.unitCost>(detail, null);
				sender.SetValueExt<INPIDetail.extVarCost>(detail, null);
				sender.SetValue<INPIDetail.status>(detail, INPIDetStatus.NotEntered);

				return;
			}

			decimal varQty = (decimal)(detail.PhysicalQty - detail.BookQty);
			sender.SetValue<INPIDetail.varQty>(detail, varQty);
			sender.SetValue<INPIDetail.status>(detail, INPIDetStatus.Entered);

			if (PIHeader.Current?.Status != INPIHdrStatus.Entering)
				return;

			if (varQty <= 0m && detail.ManualCost == true)
			{
				sender.SetValueExt<INPIDetail.manualCost>(detail, false);
			}

			decimal? oldVarQty = e.OldValue != null ? (decimal?)e.OldValue - detail.BookQty : null;
			if (varQty >= 0)
			{
				if (e.OldValue == null || oldVarQty < 0)
					DefaultDebitLineCost(sender, detail);

				sender.SetValueExt<INPIDetail.extVarCost>(detail, detail.UnitCost * varQty);
			}

			if (!IsCostCalculationEnabled())
				return;

			if (e.OldValue == null)
			{
				var relatedLines = GetRelatedLines(detail);
				UpdateRelatedLinesCost(sender, relatedLines);
			}
		}

		protected virtual void INPIDetail_ManualCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var detail = (INPIDetail)e.Row;
			if (detail?.ManualCost == false && (bool)e.OldValue == true)
			{
				DefaultDebitLineCost(sender, detail);
			}
		}

		protected virtual void INPIDetail_UnitCost_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			var detail = (INPIDetail)e.Row;
			if (detail == null)
				return;

			if (detail.VarQty > 0 && e.NewValue == null)
			{
				e.NewValue = 0m;
			}
		}

		protected virtual void INPIDetail_UnitCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var detail = (INPIDetail)e.Row;
			if (detail == null)
				return;

			if (detail.VarQty > 0 && e.ExternalCall && detail.IsCostDefaulting != true)
			{
				sender.SetValue<INPIDetail.manualCost>(e.Row, true);
				sender.SetValueExt<INPIDetail.extVarCost>(
					detail,
					detail.UnitCost != null && detail.VarQty != null
						? detail.UnitCost.Value * detail.VarQty.Value
						: (decimal?)null);
			}
		}

		protected virtual void INPIDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var detail = (INPIDetail)e.Row;
			var oldDetail = (INPIDetail)e.OldRow;

			if (IsCostCalculationEnabled())
			{
				if (oldDetail.PhysicalQty == null)
				{
					RecalcTotals();
					PIDetail.View.RequestRefresh();
					return;
				}

				if (detail.PhysicalQty == null && AreKeysFieldsEntered(oldDetail))
				{
					var prevRelatedLines = GetRelatedLines(oldDetail);
					UpdateRelatedLinesCost(sender, prevRelatedLines);

					RecalcTotals();
					PIDetail.View.RequestRefresh();
					return;
				}

				// Next, VarQty can't be null.
				if (!sender.ObjectsEqual<INPIDetail.inventoryID, INPIDetail.subItemID, INPIDetail.locationID, INPIDetail.lotSerialNbr>(detail, oldDetail)
					&& AreKeysFieldsEntered(oldDetail))
				{
					var prevRelatedLines = GetRelatedLines(oldDetail);
					UpdateRelatedLinesCost(sender, prevRelatedLines);
				}

				if (AreKeysFieldsEntered(detail))
				{
					var relatedLines = GetRelatedLines(detail);
					UpdateRelatedLinesCost(sender, relatedLines);
				}
			}

			RecalcTotals();
			PIDetail.View.RequestRefresh();
		}

		protected virtual void INPIDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			INPIHeader header = PIHeader.Current;
			if (header == null || PIHeader.Cache.GetStatus(header).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
				return;

			var deletedDetail = (INPIDetail)e.Row;
			if (!IsCostCalculationEnabled() || !AreKeysFieldsEntered(deletedDetail) || (deletedDetail.VarQty ?? 0m) == 0m)
				return;

			// Updating costs for items related by the cost layers.
			var relatedLines = GetRelatedLines(deletedDetail);
			UpdateRelatedLinesCost(sender, relatedLines);

			RecalcTotals();
			PIDetail.View.RequestRefresh();
		}
		#endregion Events

		#region misc.

		protected virtual void ValidatePIInventoryLocation(int? inventoryID, int? locationID)
		{
			if (inventoryID == null || locationID == null) return;

			var inspector = new PILocksInspector(PIHeader.Current.SiteID.Value);
			if (!inspector.IsInventoryLocationIncludedInPI(inventoryID, locationID, PIHeader.Current.PIID))
			{
				throw new PXSetPropertyException(Messages.InventoryShouldBeUsedInCurrentPI);
			}
		}

		private void ValidateDuplicate(string status, int? inventoryID, int? subItemID, int? locationID, string lotSerialNbr, int? lineNbr)
		{
			if (inventoryID == null || subItemID == null || locationID == null)
				return;

			foreach (INPIDetail it in 
					PXSelect<INPIDetail, 
					Where<INPIDetail.pIID, Equal<Current<INPIHeader.pIID>>,
						And<INPIDetail.inventoryID, Equal<Required<INPIDetail.inventoryID>>, 
						And<INPIDetail.subItemID, Equal<Required<INPIDetail.subItemID>>, 
						And<INPIDetail.locationID, Equal<Required<INPIDetail.locationID>>,
						And<INPIDetail.lineNbr, NotEqual<Required<INPIDetail.lineNbr>>>>>>>>
					.Select(this, inventoryID, subItemID, locationID, lineNbr))
			{
				if (string.Equals(
						(it.LotSerialNbr ?? "").Trim(), (lotSerialNbr ?? "").Trim(),
						StringComparison.OrdinalIgnoreCase))
				{
					throw new PXSetPropertyException(Messages.ThisCombinationIsUsedAlready, it.LineNbr);
				}
			}

			if (string.IsNullOrEmpty(lotSerialNbr))
				return;

			INLotSerClass lsc_rec = SelectLotSerClass(inventoryID);
			if(lsc_rec.LotSerTrack == INLotSerTrack.SerialNumbered &&
				 lsc_rec.LotSerAssign == INLotSerAssign.WhenReceived)
			{
				INPIDetail serialDuplicate =
					PXSelect<INPIDetail,
						Where<INPIDetail.pIID, Equal<Current<INPIHeader.pIID>>,
							And<INPIDetail.inventoryID, Equal<Required<INPIDetail.inventoryID>>,
							And<INPIDetail.lotSerialNbr, Equal<Required<INPIDetail.lotSerialNbr>>,
							And<INPIDetail.lineType, Equal<INPIDetLineType.userEntered>,
							And<INPIDetail.lineNbr, NotEqual<Required<INPIDetail.lineNbr>>>>>>>>
						.Select(this, inventoryID, lotSerialNbr, lineNbr);

				if (serialDuplicate != null)
					throw new PXSetPropertyException(Messages.ThisSerialNumberIsUsedAlready, serialDuplicate.LineNbr);

				if(status == INPIDetStatus.NotEntered)
				{
					INLotSerialStatus lotstatus =
						PXSelect<INLotSerialStatus,
							Where<INLotSerialStatus.siteID, Equal<Current<INPIHeader.siteID>>,
								And<INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>,
									And<INLotSerialStatus.lotSerialNbr, Equal<Required<INLotSerialStatus.lotSerialNbr>>,
									And<INLotSerialStatus.qtyOnHand, Greater<decimal0>>>>>>
							.SelectWindowed(this, 0, 1, inventoryID, lotSerialNbr);

					if (lotstatus != null)
					{
						INPIDetail locationPI =
						PXSelect<INPIDetail,
							Where<INPIDetail.pIID, Equal<Current<INPIHeader.pIID>>,
								And<INPIDetail.inventoryID, Equal<Required<INPIDetail.inventoryID>>,
									And<INPIDetail.locationID, Equal<Required<INPIDetail.locationID>>,
									And<INPIDetail.lotSerialNbr, Equal<Required<INPIDetail.lotSerialNbr>>,
									And<INPIDetail.lineNbr, NotEqual<Required<INPIDetail.lineNbr>>>>>>>>
							.Select(this, inventoryID, lotstatus.LocationID, lotSerialNbr, lineNbr);

						if(locationPI == null)
							throw new PXSetPropertyException(Messages.ThisSerialNumberIsUsedInItem);
					}
				}
			}
		}

		protected virtual bool LSRequired(int? p_InventoryID)
		{
			return LSRequired(SelectLotSerClass(p_InventoryID));
		}

		protected virtual bool LSRequired(INLotSerClass lsc_rec)
		{
			return lsc_rec == null ? false :
				 lsc_rec.LotSerTrack != INLotSerTrack.NotNumbered &&
				 lsc_rec.LotSerAssign == INLotSerAssign.WhenReceived;
		}

		protected virtual INLotSerClass SelectLotSerClass(int? inventoryID)
		{
			if (inventoryID == null)
				return null;
			
			InventoryItem ii_rec = InventoryItem.PK.Find(this, inventoryID);
			return ii_rec.LotSerClassID == null
			       	? null
			       	: INLotSerClass.PK.Find(this, ii_rec.LotSerClassID);
		}

		protected virtual bool AreKeysFieldsEntered(INPIDetail detail)
		{
			if (detail == null)
				return false;

			bool lsIsValid = !LSRequired(detail.InventoryID) || !string.IsNullOrWhiteSpace(detail.LotSerialNbr);
			return detail.InventoryID != null && detail.SubItemID != null && detail.LocationID != null && lsIsValid;
		}

		protected virtual string GetDetailsGroupingKey(INPIDetail detail)
		{
			if (!AreKeysFieldsEntered(detail))
				throw new InvalidOperationException();

			var keyParts = new List<string>(4)
			{
				detail.InventoryID.Value.ToString(),
				detail.SubItemID.Value.ToString()
			};

			var inventoryItem = INPIDetail.FK.InventoryItem.FindParent(this, detail);
			var location = INLocation.PK.Find(this, (int)detail.LocationID);

			// CostSeparately option works only with Average and FIFO valuated items.
			if (inventoryItem.ValMethod.IsIn(INValMethod.Average, INValMethod.FIFO) && location.IsCosted == true)
			{
				keyParts.Add(detail.LocationID.Value.ToString());
			}
			else if (inventoryItem.ValMethod == INValMethod.Specific)
			{
				keyParts.Add(detail.LotSerialNbr);
			}

			return string.Join("-", keyParts);
		}

		#endregion misc.

		#region Recalc Cost methods

		protected struct CostStatusSupplInfoRec
		{
			public decimal ProjectedQty; // OnHand + Var
			public decimal ProjectedCost; // OnHand + Var
		}


		protected class ProjectedTranRec
		{
			public bool AdjNotReceipt;
			public bool? ManualCost;
			public int LineNbr;
			public string UOM;
			public decimal? UnitCost;
			public decimal VarQtyPortion;
			public decimal VarCostPortion;

			// acct & sub from cost layer
			public int? AcctID;
			public int? SubID;

			public int? InventoryID;
			public int? SubItemID;
			public int? LocationID;
			public string LotSerialNbr;
			public DateTime? ExpireDate;

			public string OrigRefNbr;
            public string ReasonCode;
		}

		protected virtual ICollection<INPIDetail> GetRelatedLines(INPIDetail detail)
		{
			if (!AreKeysFieldsEntered(detail))
				throw new InvalidOperationException();

			if (detail.VarQty == null)
				return new List<INPIDetail> { detail };

			var arguments = new List<object>();

			var relatedLinesQuery = new PXSelect<INPIDetail,
				Where<INPIDetail.inventoryID, Equal<Required<INPIDetail.inventoryID>>,
					And<INPIDetail.subItemID, Equal<Required<INPIDetail.subItemID>>,
					And<INPIDetail.pIID, Equal<Required<INPIDetail.pIID>>>>>>(this);
			arguments.Add(detail.InventoryID);
			arguments.Add(detail.SubItemID);
			arguments.Add(detail.PIID);

			var inventoryItem = INPIDetail.FK.InventoryItem.FindParent(this, detail);
			var location = INLocation.PK.Find(this, (int)detail.LocationID);

			// CostSeparately option works only with Average and FIFO valuated items.
			if (inventoryItem.ValMethod.IsIn(INValMethod.Average, INValMethod.FIFO))
			{
				if (location?.IsCosted == true)
				{
					relatedLinesQuery.WhereAnd<Where<INPIDetail.locationID, Equal<Required<INPIDetail.locationID>>>>();
					arguments.Add(detail.LocationID);
				}
				else
				{
					relatedLinesQuery.Join<InnerJoin<INLocation,
						On<INLocation.locationID, Equal<INPIDetail.locationID>,
							And<INLocation.isCosted, Equal<boolFalse>>>>>();
				}
			}
			else if (inventoryItem.ValMethod == INValMethod.Specific)
			{
				relatedLinesQuery.WhereAnd<Where<INPIDetail.lotSerialNbr, Equal<Required<INPIDetail.lotSerialNbr>>>>();
				arguments.Add(detail.LotSerialNbr);
			}

			var relatedLines = relatedLinesQuery.Select(arguments.ToArray())
				.RowCast<INPIDetail>()
				.Where(line => AreKeysFieldsEntered(line) && line.VarQty != null)
				.ToList();

			// Adding item with location created on-the-fly as it can't satisfy InnerJoin<INLocation, > condition.
			if (location == null && relatedLines.FirstOrDefault(line => line.LineNbr == detail.LineNbr) == null)
			{
				relatedLines.Add(detail);
			}

			return relatedLines;
		}

		protected virtual IEnumerable<ProjectedTranRec> UpdateRelatedLinesCost(
			PXCache detailCache,
			IEnumerable<INPIDetail> relatedLines,
			bool adjustmentCreation = false,
			bool forseDebitLinesRecalculation = false)
		{
			var debitLines = new List<INPIDetail>();
			var creditLines = new List<INPIDetail>();
			foreach (var detail in relatedLines)
			{
				if (!AreKeysFieldsEntered(detail) || detail.VarQty == null)
					throw new InvalidOperationException();

				if (detail.VarQty >= 0m && (forseDebitLinesRecalculation
					|| detail.UnitCost == null || detail.ExtVarCost == null))
					DefaultDebitLineCost(detailCache, detail);

				if (detail.VarQty == 0m)
					continue;

				if (detail.VarQty > 0)
					debitLines.Add(detail);
				else
					creditLines.Add(detail);
			}

			decimal additionalDebitQty = 0m;
			decimal additionalDebitExtCost = 0m;
			var projectedTrans = new List<ProjectedTranRec>();
			foreach (var debitLine in debitLines)
			{
				additionalDebitQty += (decimal)debitLine.VarQty;
				// We can't use debitLine.ExtVarCost here as we will lose preceision. 
				additionalDebitExtCost += (decimal)debitLine.UnitCost * (decimal)debitLine.VarQty;

				projectedTrans.Add(CreateProjectedTran(debitLine, false));
			}

			if (creditLines.Count == 0)
				return projectedTrans;

			projectedTrans.AddRange(UpdateCreditLinesCost(detailCache, creditLines, additionalDebitQty, additionalDebitExtCost, adjustmentCreation));

			return projectedTrans;
		}

		protected virtual void DefaultDebitLineCost(PXCache detailCache, INPIDetail debitLine)
		{
			if (!AreKeysFieldsEntered(debitLine) || debitLine.VarQty == null)
				throw new InvalidOperationException();

			if (debitLine.ManualCost == true)
				return;

			debitLine.IsCostDefaulting = true;

			decimal unitCost = 0m;
			var item = InventoryItem.PK.Find(this, (int)debitLine.InventoryID);
			// INItemSite may not exist for just created items. But after the Adjustment release it will be created.
			var itemSite = INItemSite.PK.Find(this, debitLine.InventoryID, debitLine.SiteID);
			if (item.ValMethod == INValMethod.Standard)
			{
				unitCost = (itemSite?.StdCost ?? 0m) != 0m ? (decimal)itemSite?.StdCost
					: item.StdCost ?? 0m;
			}
			else
			{
				var itemCost = INItemCost.PK.Find(this, debitLine.InventoryID);
				if (item.ValMethod == INValMethod.Specific)
				{
					var costFromLayer = GetCostFromLastSpecificLayer(debitLine);

					unitCost = costFromLayer != 0m ? costFromLayer
						: (itemSite?.LastCost ?? 0m) != 0m ? (decimal)itemSite?.LastCost
						: itemCost.LastCost ?? 0m;
				}
				else // Average or FIFO
				{
					var site = INSite.PK.Find(this, debitLine.SiteID);

					const string averageCost = INSite.avgDefaultCost.AverageCost;
					string defaultCost = item.ValMethod == INValMethod.Average ? site.AvgDefaultCost : site.FIFODefaultCost;

					unitCost = defaultCost == averageCost && (itemSite?.AvgCost ?? 0m) > 0m ? (decimal)itemSite?.AvgCost
						: (itemSite?.LastCost ?? 0m) > 0m ? (decimal)itemSite?.LastCost
						: defaultCost == averageCost && (itemCost.AvgCost ?? 0m) > 0m ? (decimal)itemCost.AvgCost
						: itemCost.LastCost ?? 0m;
				}
			}

			detailCache.SetValueExt<INPIDetail.unitCost>(debitLine, unitCost);
			detailCache.SetValueExt<INPIDetail.extVarCost>(debitLine, PXDBPriceCostAttribute.Round(unitCost) * debitLine.VarQty.Value);
			
			debitLine.IsCostDefaulting = false;
			detailCache.MarkUpdated(debitLine);
		}

		protected decimal GetCostFromLastSpecificLayer(INPIDetail specificDetail)
		{
			INCostStatus lastLayer =
				PXSelectReadonly2<INCostStatus,
					InnerJoin<INCostSubItemXRef,
						On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>>,
				Where<INCostStatus.inventoryID, Equal<Required<INCostStatus.inventoryID>>,
					And<INCostStatus.qtyOnHand, GreaterEqual<decimal0>, // ignore OVERSOLD and zero layers.
					And<INCostSubItemXRef.subItemID, Equal<Required<INCostSubItemXRef.subItemID>>,
					And<INCostStatus.valMethod, Equal<INValMethod.specific>,
					And<INCostStatus.costSiteID, Equal<Required<INCostStatus.costSiteID>>,
					And<INCostStatus.lotSerialNbr, Equal<Required<INCostStatus.lotSerialNbr>>>>>>>>,
				OrderBy<Desc<INCostStatus.receiptDate, Desc<INCostStatus.receiptNbr>>>>
				.SelectWindowed(this, 0, 1, specificDetail.InventoryID, specificDetail.SubItemID, specificDetail.SiteID, specificDetail.LotSerialNbr);

			if (lastLayer == null || (lastLayer.QtyOnHand ?? 0m) == 0m)
				return 0m;

			return (lastLayer.TotalCost ?? 0m) / (decimal)lastLayer.QtyOnHand;
		}

		protected virtual IEnumerable<INCostStatus> ReadCostLayers(INPIDetail detail)
		{
			PXResultset<INCostStatus> costLayers =
				PXSelectJoin<INCostStatus,
					InnerJoin<INCostSubItemXRef,
						On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>,
					InnerJoin<INLocation,
						On<INLocation.locationID, Equal<Required<INPIDetail.locationID>>>>>,
				Where<INCostStatus.inventoryID, Equal<Required<INPIDetail.inventoryID>>,
					And<INCostStatus.qtyOnHand,Greater<decimal0>, // ignore OVERSOLD and zero layers
					And<INCostSubItemXRef.subItemID, Equal<Required<INPIDetail.subItemID>>,
					And2<
						Where2<
							Where2<
								Where<INCostStatus.valMethod, Equal<INValMethod.standard>, 
									Or<INCostStatus.valMethod, Equal<INValMethod.specific>,
									Or<INLocation.isCosted, Equal<boolFalse>>>>,
								And<INCostStatus.costSiteID, Equal<Required<INPIDetail.siteID>>>>,
							Or<
								Where2<
									Not<Where<INCostStatus.valMethod, Equal<INValMethod.standard>, 
										Or<INCostStatus.valMethod, Equal<INValMethod.specific>,
										Or<INLocation.isCosted, Equal<boolFalse>>>>>,
									And<INCostStatus.costSiteID, Equal<Required<INPIDetail.locationID>>>>>>,
					And<Where<INCostStatus.lotSerialNbr, Equal<Required<INPIDetail.lotSerialNbr>>,
						Or<INCostStatus.lotSerialNbr, IsNull,
						Or<INCostStatus.lotSerialNbr, Equal<BQLConstants.EmptyString>>>>>>>>>>
				.Select(
					this,
					detail.LocationID,
					detail.InventoryID,
					detail.SubItemID,
					detail.SiteID,
					detail.LocationID,
					detail.LotSerialNbr);

			return costLayers.RowCast<INCostStatus>();
		}

		protected virtual IComparer<INCostStatus> GetCostLayerComparer(INItemSiteSettings itemSite)
		{
			return new CostLayerComparer(itemSite);
		}

		protected virtual IEnumerable<ProjectedTranRec> UpdateCreditLinesCost(
			PXCache detailCache,
			IEnumerable<INPIDetail> creditLines,
			decimal additionalDebitQty,
			decimal additionalDebitExtCost,
			bool adjustmentCreation)
		{
			var sampleLine = creditLines.First();
			decimal debitLinesUnitCost = GetAdditionalUnitCost(sampleLine, additionalDebitQty, additionalDebitExtCost);

			var itemSiteSettings = INItemSiteSettings.PK.Find(this, sampleLine.InventoryID, sampleLine.SiteID);
			var costLayers = ReadCostLayers(sampleLine)
				.Select(layer => (INCostStatus)Caches[typeof(INCostStatus)].CreateCopy(layer))
				.ToList();
			costLayers.Sort(GetCostLayerComparer(itemSiteSettings));

			var projectedTrans = new List<ProjectedTranRec>();
			foreach (var creditLine in creditLines.OrderBy(l => (int)l.LineNbr))
			{
				// Let restQty, issuedQty and issuedExtCost be positive.
				decimal issuedQty = 0m;
				decimal issuedExtCost = 0m;
				decimal extrapolatedExtCost = 0m;
				decimal restQty = -(decimal)creditLine.VarQty;

				// Let tranQty and tranTotalCost be negative.
				decimal tranQty = 0;
				decimal tranTotalCost = 0;

				foreach (var costLayer in costLayers)
				{
					if (costLayer.QtyOnHand <= restQty)
					{
						tranQty = -costLayer.QtyOnHand.Value;
						tranTotalCost = -costLayer.TotalCost.Value;

						issuedQty += costLayer.QtyOnHand.Value;
						issuedExtCost += costLayer.TotalCost.Value;
						restQty -= costLayer.QtyOnHand.Value;

						costLayer.QtyOnHand = 0m;
						costLayer.TotalCost = 0m;
					}
					else
					{
						tranQty = -restQty;
						tranTotalCost = -PXDBCurrencyAttribute.BaseRound(this, (costLayer.TotalCost.Value / costLayer.QtyOnHand.Value) * restQty);

						costLayer.QtyOnHand -= restQty;
						costLayer.TotalCost -= -tranTotalCost;

						issuedQty += restQty;
						issuedExtCost += -tranTotalCost;
						restQty = 0m;
					}

					projectedTrans.Add(CreateProjectedTran(
						creditLine,
						true,
						PXDBPriceCostAttribute.Round(tranTotalCost / tranQty),
						tranQty,
						tranTotalCost,
						itemSiteSettings.ValMethod == INValMethod.FIFO ? costLayer.ReceiptNbr : null,
						costLayer.AccountID,
						costLayer.SubID));

					if (restQty == 0m)
						break;
				}

				while (costLayers.Count > 0 && costLayers[0].QtyOnHand == 0)
				{
					costLayers.RemoveAt(0);
				}

				if (restQty > 0)
				{
					if (additionalDebitQty >= restQty)
					{
						tranQty = -restQty;
						tranTotalCost = -PXDBCurrencyAttribute.BaseRound(this, debitLinesUnitCost * restQty);

						additionalDebitQty -= restQty;

						issuedQty += restQty;
						issuedExtCost += -tranTotalCost;

						projectedTrans.Add(CreateProjectedTran(
							creditLine,
							true,
							debitLinesUnitCost,
							tranQty,
							tranTotalCost));
					}
					else if (adjustmentCreation)
					{
						var location = INLocation.PK.Find(this, creditLine.LocationID);
						if (location.IsCosted == true)
						{
							throw new PXException(Messages.PINotEnoughQtyOnLocation,
								creditLine.LineNbr,
								detailCache.GetValueExt<INPIDetail.inventoryID>(creditLine),
								detailCache.GetValueExt<INPIDetail.subItemID>(creditLine),
								detailCache.GetValueExt<INPIDetail.siteID>(creditLine),
								detailCache.GetValueExt<INPIDetail.locationID>(creditLine));
						}
						else
						{
							throw new PXException(Messages.PINotEnoughQtyInWarehouse,
								creditLine.LineNbr,
								detailCache.GetValueExt<INPIDetail.inventoryID>(creditLine),
								detailCache.GetValueExt<INPIDetail.subItemID>(creditLine),
								detailCache.GetValueExt<INPIDetail.siteID>(creditLine));
						}
					}
					// If credit lines were entered first and there is not enough qty on cost layers.
					// We are going to extrapolate cost for the rest qty.
					else if (issuedQty > 0) 
					{
						var extrapolationUnitCost = PXDBPriceCostAttribute.Round(issuedExtCost / issuedQty);
						extrapolatedExtCost += extrapolationUnitCost * restQty;
					}
				}

				detailCache.SetValueExt<INPIDetail.unitCost>(creditLine, issuedQty != 0m ? issuedExtCost / issuedQty : 0m);
				detailCache.SetValueExt<INPIDetail.extVarCost>(creditLine, -(issuedExtCost + extrapolatedExtCost));
				detailCache.MarkUpdated(creditLine);
			}

			return projectedTrans;
		}

		protected virtual ProjectedTranRec CreateProjectedTran(
			INPIDetail detail,
			bool adjNotReceipt,
			decimal? tranUnitCost = null,
			decimal? tranQty = null,
			decimal? tranTotalCost = null,
			string receiptNbr = null,
			int? invAcctID = null,
			int? invSubID = null)
		{
			var projTran = new ProjectedTranRec();
			projTran.AdjNotReceipt = adjNotReceipt;
			projTran.OrigRefNbr = receiptNbr;
			projTran.LineNbr = detail.LineNbr.Value;

			projTran.ManualCost = detail.ManualCost;
			projTran.UnitCost = tranUnitCost ?? detail.UnitCost;
			projTran.VarQtyPortion = tranQty ?? detail.VarQty.Value;
			projTran.VarCostPortion = tranTotalCost ?? detail.ExtVarCost.Value;

			projTran.AcctID = invAcctID;
			projTran.SubID = invSubID;

			projTran.InventoryID = detail.InventoryID;
			projTran.SubItemID = detail.SubItemID;
			projTran.LocationID = detail.LocationID;
			projTran.LotSerialNbr = detail.LotSerialNbr;
			projTran.ReasonCode = detail.ReasonCode;
			projTran.ExpireDate = detail.ExpireDate;

			return projTran;
		}

		protected virtual decimal GetAdditionalUnitCost(INPIDetail detail, decimal additionalDebitQty, decimal additionalDebitExtCost)
		{
			decimal avgDebitLinesUnitCost = additionalDebitQty != 0m ? PXDBPriceCostAttribute.Round(additionalDebitExtCost / additionalDebitQty) : 0m;
			var item = InventoryItem.PK.Find(this, detail.InventoryID);
			if (item.ValMethod == INValMethod.Standard)
			{
				var itemSite = INItemSite.PK.Find(this, detail.InventoryID, detail.SiteID);
				return (itemSite?.StdCost ?? 0m) != 0m ? (decimal)itemSite.StdCost
					: item.StdCost ?? avgDebitLinesUnitCost;
			}

			return avgDebitLinesUnitCost;
		}

		protected virtual List<ProjectedTranRec> RecalcDemandCost(bool adjustmentCreation = false, bool forseDebitLinesRecalculation = false)
		{
			var header = (INPIHeader)PIHeader.Cache.Current;
			if (header == null || !IsCostCalculationEnabled())
				return new List<ProjectedTranRec>();

			var projectedTrans = new List<ProjectedTranRec>();
			var relatedDetailGroups = PIDetailPure.Select()
				.AsEnumerable()
				.RowCast<INPIDetail>()
				.Where(detail => AreKeysFieldsEntered(detail) && detail.Status == INPIDetStatus.Entered)
				.GroupBy(GetDetailsGroupingKey);

			foreach (var relatedDetails in relatedDetailGroups)
			{
				projectedTrans.AddRange(
					UpdateRelatedLinesCost(
						PIDetail.Cache,
						relatedDetails,
						adjustmentCreation,
						forseDebitLinesRecalculation));
			}

			return projectedTrans;
		}

	    private bool skipRecalcTotals = false;
		protected virtual void RecalcTotals()
		{
			if (skipRecalcTotals)
				return;

			decimal total_var_qty = 0m, total_var_cost = 0m, total_phys_qty = 0m;
			//  manually , not via PXFormula because of the problems in INPIReview with double-counting during cost recalculation called from FieldUpdated event

			foreach (INPIDetail detail in PIDetailPure.Select())
			{
				if (detail == null || detail.Status == INPIDetStatus.Skipped)
					continue;

				total_phys_qty += detail.PhysicalQty ?? 0m;
				total_var_qty += detail.VarQty ?? 0m;
				total_var_cost += detail.FinalExtVarCost ?? detail.ExtVarCost ?? 0m;
			}

			var header = (INPIHeader)PIHeader.Cache.Current;
			if (header != null)
			{
				header.TotalPhysicalQty = total_phys_qty;
				header.TotalVarQty = total_var_qty;
				header.TotalVarCost = total_var_cost;
				PIHeader.Update(header);
			}
		}

		#endregion Recalc Cost methods

		#region IPXPrepareItems

		public int excelRowNumber = 2;
		public bool importHasError = false;

		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			skipRecalcTotals = true;
			DisableCostCalculation = true;
			if (string.Compare(viewName, PIDetail.View.Name, true) == 0)
			{
				PXCache barCodeCache = AddByBarCode.Cache;
				INBarCodeItem item = (INBarCodeItem)(AddByBarCode.Current ?? barCodeCache.CreateInstance());
				try
				{
					barCodeCache.SetValueExt<INBarCodeItem.inventoryID>(item, GetImportedValue<INPIDetail.inventoryID>(values, true));
					if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
						barCodeCache.SetValueExt<INBarCodeItem.subItemID>(item, GetImportedValue<INPIDetail.subItemID>(values, true));
					if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
						barCodeCache.SetValueExt<INBarCodeItem.locationID>(item, GetImportedValue<INPIDetail.locationID>(values, true));
					if (PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>())
					{
						barCodeCache.SetValueExt<INBarCodeItem.lotSerialNbr>(item, GetImportedValue<INPIDetail.lotSerialNbr>(values, false));
						barCodeCache.SetValueExt<INBarCodeItem.expireDate>(item, GetImportedValue<INPIDetail.expireDate>(values, false));
					}
					barCodeCache.SetValueExt<INBarCodeItem.qty>(item, GetImportedValue<INPIDetail.physicalQty>(values, true));
					barCodeCache.SetValueExt<INBarCodeItem.autoAddLine>(item, false);
					barCodeCache.SetValueExt<INBarCodeItem.reasonCode>(item, GetImportedValue<INPIDetail.reasonCode>(values, false));

					barCodeCache.Update(item);
					UpdatePhysicalQty();
				}
				catch (Exception e)
				{
					PXTrace.WriteError(IN.Messages.RowError, excelRowNumber, e.Message);
					importHasError = true;
				}
				finally
				{
					excelRowNumber ++;
				}
			}
			return false;
		}

		public bool RowImporting(string viewName, object row)
		{
			return false;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return false;
		}

		public void PrepareItems(string viewName, IEnumerable items)
		{
		}

		private object GetImportedValue<Field>(IDictionary values, bool isRequired)
			where Field : IBqlField
		{
			INPIDetail item = (INPIDetail)PIDetail.Cache.CreateInstance();
			string displayName = PXUIFieldAttribute.GetDisplayName<Field>(PIDetail.Cache);
			if (!values.Contains(typeof(Field).Name) && isRequired)
				throw new PXException(Messages.CollumnIsMandatory, displayName);
			object value = values[typeof(Field).Name];
			PIDetail.Cache.RaiseFieldUpdating<Field>(item, ref value);
			if (isRequired && value == null)
				throw new PXException(ErrorMessages.FieldIsEmpty, displayName);
			return value;
		}

		#endregion

		#region IPXProcess
		public void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			skipRecalcTotals = false;
			DisableCostCalculation = false;

			RecalcDemandCost();
			RecalcTotals();

			if (importHasError)
				throw new Exception(IN.Messages.ImportHasError);
		}
		#endregion
	}


	public class INPIReview : INPIEntry  // (= PIEntry + some extended functionality)
	{
		public PXAction<INPIHeader> finishCounting;
		public PXAction<INPIHeader> completePI;

		public PXAction<INPIHeader> actionsFolder;

		public PXAction<INPIHeader> updateCost;
		public PXAction<INPIHeader> setNotEnteredToZero;
		public PXAction<INPIHeader> setNotEnteredToSkipped;
		public PXAction<INPIHeader> cancelPI;

		#region Adaptors

		[PXUIField(DisplayName = Common.Messages.Actions, MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ToolbarFolder)]
		protected virtual IEnumerable ActionsFolder(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.SetNotEnteredToZero)]
		[PXButton]
		protected virtual IEnumerable SetNotEnteredToZero(PXAdapter adapter)
		{
			INPIHeader header = PIHeader.Current;
			if (header == null || header.Status.IsNotIn(INPIHdrStatus.Counting, INPIHdrStatus.Entering)) 
				return adapter.Get();
			
			Save.Press();

			PXLongOperation.StartOperation(
				this,
				() =>
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						INPIReview docgraph = PXGraph.CreateInstance<INPIReview>();
						docgraph.PIHeader.Current = docgraph.PIHeader.Search<INPIHeader.pIID>(header.PIID);

						var details = PXSelectJoin<INPIDetail,
						LeftJoin<INItemSite, On<INItemSite.inventoryID, Equal<INPIDetail.inventoryID>,
							And<INItemSite.siteID, Equal<INPIDetail.siteID>>>,
						LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<INPIDetail.inventoryID>>>>,
						Where<INPIDetail.pIID, Equal<Required<INPIHeader.pIID>>>,
						OrderBy<Asc<INPIDetail.lineNbr>>>.Select(docgraph, header.PIID);

						docgraph.DisableCostCalculation = true;
						foreach (PXResult<INPIDetail, INItemSite, INItemCost> row in details)
			{
							INPIDetail detail = row;
							if (detail.Status != INPIDetStatus.NotEntered || !docgraph.AreKeysFieldsEntered(detail))
					continue;

							INItemSite.PK.StoreCached(docgraph, row);
							INItemCost.PK.StoreCached(docgraph, row);

							docgraph.PIDetail.Cache.SetValueExt<INPIDetail.physicalQty>(detail, 0m);
							docgraph.PIDetail.Cache.MarkUpdated(detail);
			}
						docgraph.DisableCostCalculation = false;

						docgraph.RecalcDemandCost();
						docgraph.RecalcTotals();
						docgraph.PIDetail.Cache.IsDirty = true;
						docgraph.Save.Press();

						ts.Complete();
					}
				});

			return adapter.Get();
		}
		

		[PXUIField(DisplayName = Messages.SetNotEnteredToSkipped)]
		[PXButton]
		protected virtual IEnumerable SetNotEnteredToSkipped(PXAdapter adapter)
		{
			INPIHeader header = PIHeader.Current;
			if (header == null || header.Status.IsNotIn(INPIHdrStatus.Counting, INPIHdrStatus.Entering)) 
				return adapter.Get();

			Save.Press();

			PXLongOperation.StartOperation(
				this,
				() =>
				{
					INPIReview docgraph = PXGraph.CreateInstance<INPIReview>();
					docgraph.PIHeader.Current = docgraph.PIHeader.Search<INPIHeader.pIID>(header.PIID);

					foreach (INPIDetail detail in docgraph.PIDetailPure.Select())
			{
				if (detail.Status != INPIDetStatus.NotEntered)
					continue;

						docgraph.PIDetail.Cache.SetValue<INPIDetail.status>(detail, INPIDetStatus.Skipped);
						docgraph.PIDetail.Cache.MarkUpdated(detail);
			}

					docgraph.PIDetail.Cache.IsDirty = true;
					docgraph.Save.Press();
				});

			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.UpdateCost)]
		[PXButton]
		protected virtual IEnumerable UpdateCost(PXAdapter adapter)
		{
			INPIHeader header = PIHeader.Current;
			if (header == null || header.Status.IsNotIn(INPIHdrStatus.Counting, INPIHdrStatus.Entering)) 
				return adapter.Get();

			Save.Press();

			PXLongOperation.StartOperation(
				this,
				() =>
				{
					INPIReview docgraph = PXGraph.CreateInstance<INPIReview>();
					docgraph.PIHeader.Current = docgraph.PIHeader.Search<INPIHeader.pIID>(header.PIID);

					docgraph.RecalcDemandCost(false, true);
					docgraph.RecalcTotals();

					docgraph.PIDetail.Cache.IsDirty = true;
					docgraph.Save.Press();
				});

			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.CompletePI, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable CompletePI(PXAdapter adapter)
		{
			foreach (INPIHeader ph in adapter.Get().RowCast<INPIHeader>())
			{
				bool checkOK = true;
				PIHeader.Current = ph;

				if (!((ph.Status == INPIHdrStatus.Counting) || (ph.Status == INPIHdrStatus.Entering)))
				{
					throw new PXException(Messages.Document_Status_Invalid);
				}
				
				// line entered status
				foreach (INPIDetail pd in this.PIDetail.Select())
				{
					if (pd.Status == INPIDetStatus.NotEntered)
					{
						if (pd.InventoryID != null)
						{
							PIDetail.Cache.RaiseExceptionHandling<INPIDetail.lineNbr>(pd, pd.LineNbr, new PXSetPropertyException(Messages.NotEnteredLineDataError, PXErrorLevel.RowError));
							checkOK = false;
						}
					}
				}

				if (checkOK)
				{
					Save.Press();
					INPIHeader ph1 = ph;
					PXLongOperation.StartOperation
					(
						this,
						() =>
							{
								INPIReview docgraph = PXGraph.CreateInstance<INPIReview>();
								docgraph.FinishEntering(ph1);
							}
						);
				}
				yield return ph;
			}
		}

		[PXUIField(DisplayName = Messages.FinishCounting, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable FinishCounting(PXAdapter adapter)
		{
			INPIHeader header = PIHeader.Current;

			if (header == null || header.Status != INPIHdrStatus.Counting) 
				return adapter.Get();

			header.Status = INPIHdrStatus.Entering;
			PIHeader.Update(header);

			RecalcDemandCost();
			RecalcTotals();

			var piClass = INPIClass.PK.Find(this, header.PIClassID);
			if (piClass != null && piClass.UnlockSiteOnCountingFinish == true)
				CreatePILocksManager().UnlockInventory(false);

			this.Save.Press();

			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.CancelPI, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable CancelPI(PXAdapter adapter)
		{
			INPIHeader header = PIHeader.Current;
			if (header == null || !((header.Status == INPIHdrStatus.Counting) || (header.Status == INPIHdrStatus.Entering))) 
				return adapter.Get(); 

			header.Status = INPIHdrStatus.Cancelled;
			PIHeader.Update(header);

			CreatePILocksManager().UnlockInventory();

			this.Save.Press();

			return adapter.Get();
		}

		#endregion Adaptors

		#region DAC overrides

		public PXFilter<PX.Data.PXImportAttribute.CSVSettings> cSVSettings;
		public PXFilter<PX.Data.PXImportAttribute.XLSXSettings> xLSXSettings;

		[PXString]
		[PXUIField(Visible = false)]
		public virtual void CSVSettings_Mode_CacheAttached(PXCache sender)
		{
		}

		[PXString]
		[PXUIField(Visible = false)]
		public virtual void XLSXSettings_Mode_CacheAttached(PXCache sender)
		{
		}
		#endregion

		public INPIReview()
		{
			this.actionsFolder.MenuAutoOpen = true;
			this.actionsFolder.AddMenuAction(this.updateCost);
			this.actionsFolder.AddMenuAction(this.setNotEnteredToZero);
			this.actionsFolder.AddMenuAction(this.setNotEnteredToSkipped);
			this.actionsFolder.AddMenuAction(this.cancelPI);
		}

		public virtual void FinishEntering(INPIHeader p_h)
		{
			INSetup insetup = INSetup.Select();

			INPIHeader header = PIHeader.Current = PIHeader.Search<INPIHeader.pIID>(p_h.PIID);
			if (header == null || insetup == null || insite.Current == null)
				return;

			List<ProjectedTranRec> projectedTrans = RecalcDemandCost(true);

			INRegister inAdjustment;
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				INAdjustmentEntry je = PXGraph.CreateInstance<INAdjustmentEntry>();

				je.insetup.Current.RequireControlTotal = false;
				je.insetup.Current.HoldEntry = false;

				INRegister newdoc = new INRegister();
				newdoc.BranchID = insite.Current.BranchID;
				newdoc.OrigModule = INRegister.origModule.PI;
				newdoc.PIID = header.PIID;
				je.adjustment.Cache.Insert(newdoc);

				foreach (ProjectedTranRec projectedTran in projectedTrans)
				{
					INLotSerClass lotSerClass =
						PXSelectReadonly2<INLotSerClass,
							InnerJoin<InventoryItem, On<InventoryItem.FK.LotSerClass>>,
						Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
						.SelectWindowed(this, 0, 1, projectedTran.InventoryID);

					if (lotSerClass != null &&
						lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered &&
						lotSerClass.LotSerAssign == INLotSerAssign.WhenReceived)
					{
						Sign sign = projectedTran.VarQtyPortion >= 0 ? Sign.Plus : Sign.Minus;
						int origVarQty = (int)Math.Abs(projectedTran.VarQtyPortion); // should be only integers
						projectedTran.VarCostPortion = projectedTran.VarCostPortion / origVarQty;
						projectedTran.VarQtyPortion = sign * 1;

						while (origVarQty > 0)
						{
							ProduceAdjustment(je, header, projectedTran);
							origVarQty -= 1;
						}
					}
					else
					{
						ProduceAdjustment(je, header, projectedTran);
					}
				}

				je.Save.Press();
				header.PIAdjRefNbr = je.adjustment.Current.RefNbr;
				inAdjustment = je.adjustment.Current;

				PIHeader.Current = header;
				header.Status = INPIHdrStatus.InReview;
				RecalcTotals();

				this.Save.Press();
				ts.Complete();
			}

			if (Setup.Current.AutoReleasePIAdjustment == true && inAdjustment != null)
			{
				INDocumentRelease.ReleaseDoc(new List<INRegister>{ inAdjustment }, false);
			}
		}

		private void ProduceAdjustment(INAdjustmentEntry adjustmentGraph, INPIHeader header, ProjectedTranRec projectedTran)
		{
			if (projectedTran.AdjNotReceipt)
			{
				INTran tran = new INTran();
				tran.BranchID = insite.Current.BranchID;
				tran.TranType = INTranType.Adjustment;
				tran.PIID = header.PIID;
				tran.PILineNbr = projectedTran.LineNbr;
				// INTranType.StandardCostAdjustment for standard-costed items ? ..not

				tran.InvtAcctID = projectedTran.AcctID;
				tran.InvtSubID = projectedTran.SubID;

				tran.AcctID = null; // left to be defaulted during release
				tran.SubID = null;

				tran.InventoryID = projectedTran.InventoryID;
				tran.SubItemID = projectedTran.SubItemID;
				tran.SiteID = header.SiteID;
				tran.LocationID = projectedTran.LocationID;
				
				tran.ManualCost = projectedTran.ManualCost;
				tran.UnitCost = projectedTran.UnitCost;
				
				tran.Qty = projectedTran.VarQtyPortion;
				tran.TranCost = projectedTran.VarCostPortion;
				tran.ReasonCode = projectedTran.ReasonCode;

				tran.OrigRefNbr = projectedTran.OrigRefNbr;
				tran = PXCache<INTran>.CreateCopy(adjustmentGraph.transactions.Insert(tran));
				tran.LotSerialNbr = projectedTran.LotSerialNbr;
				tran.ExpireDate = projectedTran.ExpireDate;
				tran = PXCache<INTran>.CreateCopy(adjustmentGraph.transactions.Update(tran));
			}
			else
			{
				INTran tran = new INTran();
				tran.BranchID = insite.Current.BranchID;
				tran.TranType = INTranType.Adjustment;
				tran.PIID = header.PIID;
				tran.PILineNbr = projectedTran.LineNbr;
				tran = PXCache<INTran>.CreateCopy(adjustmentGraph.transactions.Insert(tran));
				tran.InvtAcctID = projectedTran.AcctID;
				tran.InvtSubID = projectedTran.SubID;

				tran.AcctID = null; // left to be defaulted during release
				tran.SubID = null;

				tran.InventoryID = projectedTran.InventoryID;
				tran.SubItemID = projectedTran.SubItemID;
				tran.SiteID = header.SiteID;
				tran.LocationID = projectedTran.LocationID;
				tran.UOM = projectedTran.UOM;

				tran.ManualCost = projectedTran.ManualCost;
				tran.UnitCost = projectedTran.UnitCost;

				tran.Qty = projectedTran.VarQtyPortion;
				tran.TranCost = projectedTran.VarCostPortion;
				tran.ReasonCode = projectedTran.ReasonCode;
				tran = PXCache<INTran>.CreateCopy(adjustmentGraph.transactions.Update(tran));
				tran.LotSerialNbr = projectedTran.LotSerialNbr;
				tran.ExpireDate = projectedTran.ExpireDate;
				tran = PXCache<INTran>.CreateCopy(adjustmentGraph.transactions.Update(tran));
			}
		}

		protected override void INPIHeader_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.INPIHeader_RowSelected(sender, e);
			INPIHeader header = (INPIHeader)e.Row;
			if (header == null)
				return;

			cancelPI.SetEnabled(header.Status.IsIn(INPIHdrStatus.Counting, INPIHdrStatus.Entering));
			finishCounting.SetEnabled(header.Status == INPIHdrStatus.Counting);
			updateCost.SetEnabled(header.Status == INPIHdrStatus.Entering);
			setNotEnteredToZero.SetEnabled(header.Status == INPIHdrStatus.Entering);
			setNotEnteredToSkipped.SetEnabled(header.Status == INPIHdrStatus.Entering);
			completePI.SetEnabled(header.Status == INPIHdrStatus.Entering);
		}
	}

	#region PIInvtSiteLoc projection

	[System.SerializableAttribute]
	[PXProjection(typeof(Select5<INPIHeader,
		InnerJoin<INPIDetail,
			On<INPIDetail.FK.PIHeader>>,
		Aggregate<
			GroupBy<INPIHeader.pIID,
			GroupBy<INPIDetail.inventoryID,
			GroupBy<INPIHeader.siteID,
			GroupBy<INPIDetail.locationID>>>>>>))]

    [PXHidden]
	public partial class PIInvtSiteLoc : PX.Data.IBqlTable
	{
		#region PIID
		public abstract class pIID : PX.Data.BQL.BqlString.Field<pIID> { }
		protected String _PIID;
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(INPIHeader.pIID))]
		public virtual String PIID
		{
			get
			{
				return this._PIID;
			}
			set
			{
				this._PIID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(IsKey = true, BqlField = typeof(INPIDetail.inventoryID))]
		[PXDefault]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(IsKey = true, BqlField = typeof(INPIHeader.siteID))]
		[PXDefault]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[IN.Location(IsKey = true, BqlField = typeof(INPIDetail.locationID))]
		[PXDefault]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
	}

	#endregion PIInvtSiteLoc projection

}
