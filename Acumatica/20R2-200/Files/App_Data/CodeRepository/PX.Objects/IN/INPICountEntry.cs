using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN.PhysicalInventory;
using PX.Objects.RQ;

namespace PX.Objects.IN
{

	public class INPICountEntry : PXGraph<INPICountEntry>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
	{
		public PXAction<INPIHeader> save;
		public PXCancel<INPIHeader> Cancel;
		public PXFirst<INPIHeader> First;
		public PXPrevious<INPIHeader> Previous;
		public PXNext<INPIHeader> Next;
		public PXLast<INPIHeader> Last;


		public PXSelect<INPIHeader, 
			Where<INPIHeader.status, Equal<INPIHdrStatus.counting>>> PIHeader;
		public PXFilter<PICountFilter> Filter;
		public INBarCodeItemLookup<INBarCodeItem> AddByBarCode;

		[PXImport(typeof(INPIHeader))]
		public PXSelectJoin<INPIDetail,
			InnerJoin<INPIHeader, 
				On<INPIDetail.FK.PIHeader>,
			InnerJoin<InventoryItem, 
				On<INPIDetail.FK.InventoryItem>, 
			LeftJoin<INSubItem, 
				On<INPIDetail.FK.SubItem>>>>,
			Where<INPIDetail.pIID, Equal<Current<INPIHeader.pIID>>,
			And<INPIDetail.inventoryID, IsNotNull,
			And<
			Where2<Where<Current<PICountFilter.startLineNbr>, IsNull, 
									      Or<INPIDetail.lineNbr, GreaterEqual<Current<PICountFilter.startLineNbr>>>>,
				And<Where<Current<PICountFilter.endLineNbr>, IsNull, 
									      Or<INPIDetail.lineNbr, LessEqual<Current<PICountFilter.endLineNbr>>>>>>>>>> PIDetail;
		#region Cache Attached
		#region INPIHeader
		[PXDefault()]
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(
			typeof(Search<INPIHeader.pIID,
				Where<INPIHeader.status, Equal<INPIHdrStatus.counting>>, 
				OrderBy<Desc<INPIHeader.pIID>>>),
			Filterable = true)]
		protected virtual void INPIHeader_PIID_CacheAttached(PXCache sender)
		{
		}	
		#endregion
		#endregion

		public PXSetup<INSetup> Setup; 
		public INPICountEntry()
		{
			PIHeader.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled<INPIHeader.descr>(PIHeader.Cache, null, false);

			PIDetail.WhereAndCurrent<PICountFilter>();
			PXUIFieldAttribute.SetEnabled(PIDetail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INPIDetail.physicalQty>(PIDetail.Cache, null, true);
			PIDetail.View.Clear();

			PXImportAttribute import = PIDetail.GetAttribute<PXImportAttribute>();
			import.MappingPropertiesInit += MappingPropertiesInit;
		}


		public void MappingPropertiesInit(object sender, PXImportAttribute.MappingPropertiesInitEventArgs e)
		{
			e.Names.Add(typeof(INPIDetail.inventoryID).Name);
			e.DisplayNames.Add(PXUIFieldAttribute.GetDisplayName<INPIDetail.inventoryID>(PIDetail.Cache));
			if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				e.Names.Add(typeof(INPIDetail.subItemID).Name);
				e.DisplayNames.Add(PXUIFieldAttribute.GetDisplayName<INPIDetail.subItemID>(PIDetail.Cache));
			}
			if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
			{
				e.Names.Add(typeof(INPIDetail.locationID).Name);
				e.DisplayNames.Add(PXUIFieldAttribute.GetDisplayName<INPIDetail.locationID>(PIDetail.Cache));
			}
			if (PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>())
			{
				e.Names.Add(typeof(INPIDetail.lotSerialNbr).Name);
				e.DisplayNames.Add(PXUIFieldAttribute.GetDisplayName<INPIDetail.lotSerialNbr>(PIDetail.Cache));
			}
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = false)]
        [PXSaveButton(CommitChanges = true, SpecialType = PXSpecialButtonType.Default)]
        protected virtual IEnumerable Save(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<INPIHeader> addLine;
		[PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXLookupButton(Tooltip = Messages.AddNewLine)]
		public virtual IEnumerable AddLine(PXAdapter adapter)
		{
			if (AddByBarCode.AskExt(
				(graph, view) => ((INPICountEntry) graph).AddByBarCode.Reset(false)) == WebDialogResult.OK && 
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
			if(exists != null)
			{
				e.NewValue = exists.ExpireDate;
				e.Cancel = true;
			}
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
			INPIHeader d = this.PIHeader.Current;

			this.SelectTimeStamp();

			using (PXTransactionScope sc = new PXTransactionScope())
			{
				INPIDetail detail = 
				PXSelectReadonly<INPIDetail,
					Where<INPIDetail.pIID, Equal<Current<INPIHeader.pIID>>,
						And<INPIDetail.inventoryID, Equal<Current<INBarCodeItem.inventoryID>>,
							And<INPIDetail.subItemID, Equal<Current<INBarCodeItem.subItemID>>,
								And<INPIDetail.locationID, Equal<Current<INBarCodeItem.locationID>>,
									And<Where<INPIDetail.lotSerialNbr, IsNull,
										Or<INPIDetail.lotSerialNbr, Equal<Current<INBarCodeItem.lotSerialNbr>>>>>>>>>>.SelectWindowed(this, 0, 1);
				
				
				if (detail == null)
				{
					INPIEntry entry = PXGraph.CreateInstance<INPIEntry>();
					entry.PIHeader.Current = entry.PIHeader.Search<INPIHeader.pIID>(d.PIID);

					detail = new INPIDetail { InventoryID = item.InventoryID };
					detail = PXCache<INPIDetail>.CreateCopy(entry.PIDetail.Insert(detail));
					detail.SubItemID = item.SubItemID;
					detail.LocationID = item.LocationID;
					detail.LotSerialNbr = item.LotSerialNbr;
					detail = PXCache<INPIDetail>.CreateCopy(entry.PIDetail.Update(detail));
					detail.PhysicalQty = item.Qty;
					detail.ExpireDate = item.ExpireDate;
					entry.PIDetail.Update(detail);

					entry.Save.Press();
                    var detailId = entry.PIDetail.Cache.GetValue(detail, nameof(INPIDetail.PIID));
                    var lineNbr = entry.PIDetail.Cache.GetValue(detail, nameof(INPIDetail.LineNbr));
					PIHeader.View.RequestRefresh();
                    PIDetail.Cache.Locate(new Dictionary<string, object>()
                        {{nameof(INPIDetail.PIID), detailId}, {nameof(INPIDetail.LineNbr), lineNbr}});
                }
				else
				{
					detail = PXCache<INPIDetail>.CreateCopy(detail);
					detail.PhysicalQty = detail.PhysicalQty.GetValueOrDefault() +  item.Qty.GetValueOrDefault();
					PIDetail.Update(detail);
				}

				sc.Complete();

				item.Description = PXMessages.LocalizeFormatNoPrefixNLA(Messages.PILineUpdated,
				                                 AddByBarCode.GetValueExt<INBarCodeItem.inventoryID>(item).ToString().Trim(),
				                                 Setup.Current.UseInventorySubItem == true ? ":" + AddByBarCode.GetValueExt<INBarCodeItem.subItemID>(item) : string.Empty,
				                                 AddByBarCode.GetValueExt<INBarCodeItem.qty>(item),
				                                 item.UOM,
				                                 detail.LineNbr);
			}
			AddByBarCode.Reset(true);
			this.AddByBarCode.View.RequestRefresh();
			this.SelectTimeStamp();
		}
		protected virtual void INPIHeader_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var header = (INPIHeader)e.Row;
			if (header == null)
				return;

			addLine.SetEnabled(header.PIID != null);
			addLine2.SetEnabled(header.PIID != null);

			var piClass = INPIClass.PK.Find(this, header.PIClassID);
			if (piClass == null)
				return;

			PXUIFieldAttribute.SetVisible<INPIDetail.bookQty>(PIDetail.Cache, null, piClass.HideBookQty != true);
			PXUIFieldAttribute.SetVisible<INPIDetail.varQty>(PIDetail.Cache, null, piClass.HideBookQty != true);
		}

		protected virtual void INPIHeader_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void INBarCodeItem_SiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.PIHeader.Current != null)
				e.NewValue = this.PIHeader.Current.SiteID;
			e.Cancel = true;
		}
		protected virtual void INBarCodeItem_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
			{
				if (this.Filter.Current != null)
					e.NewValue = this.Filter.Current.LocationID;
				e.Cancel = true;
			}
		}
		protected virtual void INBarCodeItem_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INBarCodeItem detail = (INBarCodeItem)e.Row;
			if (e.NewValue == null || detail == null)
				return;

			try
			{
				ValidatePIInventoryLocation((int?)e.NewValue, detail.LocationID);
			}
			catch (PXSetPropertyException ex)
			{
				var invalidItem = InventoryItem.PK.Find(this, (int)e.NewValue);
				e.NewValue = invalidItem?.InventoryCD;
				throw ex;
			}
		}
		protected virtual void INBarCodeItem_LocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INBarCodeItem detail = (INBarCodeItem)e.Row;
			if (e.NewValue as int? == null || detail == null)
				return;

			try
			{
				ValidatePIInventoryLocation(detail.InventoryID, (int?)e.NewValue);
			}
			catch (PXSetPropertyException ex)
			{
				var invalidLocation = INLocation.PK.Find(this, (int)e.NewValue);
				e.NewValue = invalidLocation?.LocationCD;
				throw ex;
			}
		}

		protected virtual void INPIDetail_PhysicalQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null) return;

			decimal value = (decimal)e.NewValue;
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

		protected virtual void INPIDetail_PhysicalQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var d = (INPIDetail)e.Row;
			if (d == null)
				return;

			sender.SetValue<INPIDetail.varQty>(d, d.PhysicalQty - d.BookQty);
			sender.SetValue<INPIDetail.status>(d, d.PhysicalQty != null
				? INPIDetStatus.Entered
				: INPIDetStatus.NotEntered);
		}

		protected virtual void INPIDetail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			INPIDetail d = (INPIDetail)e.NewRow;
			INPIDetail o = (INPIDetail)e.Row;
			if (d == null || o == null)
				return;

			using (PXTransactionScope sc = new PXTransactionScope())
			{
				if (!PXDatabase.Update<INPIDetail>(
					new PXDataFieldAssign(typeof(INPIDetail.status).Name, PXDbType.VarChar, d.Status),
					new PXDataFieldAssign(typeof(INPIDetail.physicalQty).Name, PXDbType.Decimal, d.PhysicalQty),
					new PXDataFieldAssign(typeof(INPIDetail.varQty).Name, PXDbType.Decimal, d.VarQty),
					new PXDataFieldRestrict(typeof(INPIDetail.pIID).Name, PXDbType.VarChar, d.PIID),
					new PXDataFieldRestrict(typeof(INPIDetail.lineNbr).Name, PXDbType.Int, d.LineNbr),
					new PXDataFieldRestrict(typeof(INPIDetail.Tstamp).Name, PXDbType.Timestamp, 8, this.TimeStamp, PXComp.LE)))
				{
					throw new PXException(ErrorMessages.RecordUpdatedByAnotherProcess, typeof (INPIDetail).Name);
				}

				PXDatabase.Update<INPIHeader>(
					new PXDataFieldAssign(typeof(INPIHeader.totalPhysicalQty).Name, PXDbType.Decimal, (d.PhysicalQty ?? 0) - (o.PhysicalQty ?? 0))
					{
						Behavior = PXDataFieldAssign.AssignBehavior.Summarize
					},
					new PXDataFieldAssign(typeof(INPIHeader.totalVarQty).Name, PXDbType.Decimal, (d.VarQty ?? 0) - (o.VarQty ?? 0))
					{
						Behavior = PXDataFieldAssign.AssignBehavior.Summarize
					},
					new PXDataFieldRestrict(typeof(INPIHeader.pIID).Name, PXDbType.VarChar, d.PIID));

				sc.Complete();
			}
		}

		protected virtual void INPIDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.SetStatus(e.Row, PXEntryStatus.Notchanged);
			sender.IsDirty = false;
			PIDetail.View.RequestRefresh();
			this.SelectTimeStamp();
		}	
		protected virtual void PICountFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PICountFilter filter = e.Row as PICountFilter;
			if(filter == null || filter.StartLineNbr == null || filter.EndLineNbr == null) return;
			if (filter.EndLineNbr < filter.StartLineNbr)
				filter.EndLineNbr = filter.StartLineNbr;
		}
		
		protected virtual INLotSerClass SelectLotSerClass(int? p_InventoryID)
		{
			if (p_InventoryID == null) return null;

			InventoryItem ii_rec = InventoryItem.PK.Find(this, p_InventoryID);
			return ii_rec.LotSerClassID == null
							? null
							: INLotSerClass.PK.Find(this, ii_rec.LotSerClassID);
		}
		protected virtual bool LSRequired(INLotSerClass lsc_rec)
		{
			return lsc_rec == null ? false :
				 lsc_rec.LotSerTrack != INLotSerTrack.NotNumbered &&
				 lsc_rec.LotSerAssign == INLotSerAssign.WhenReceived;
		}

		protected virtual void ValidatePIInventoryLocation(int? inventoryID, int? locationID)
		{
			if (inventoryID == null || locationID == null) return;

			var inspector = new PILocksInspector(PIHeader.Current.SiteID.Value);
			if (!inspector.IsInventoryLocationIncludedInPI(inventoryID, locationID, PIHeader.Current.PIID))
			{
				throw new PXSetPropertyException(Messages.InventoryShouldBeUsedInCurrentPI);
			}
		}

		#region import function
		public int excelRowNumber = 2;
		public bool importHasError = false;

		private object GetImportedValue<Field>(IDictionary values, bool isRequired)
			where Field : IBqlField
		{
			INPIDetail item = (INPIDetail)PIDetail.Cache.CreateInstance();
			string displayName = PXUIFieldAttribute.GetDisplayName<Field>(PIDetail.Cache);
			if (!values.Contains(typeof(Field).Name))
				throw new PXException(Messages.CollumnIsMandatory, displayName);
			object value = values[typeof(Field).Name];
			PIDetail.Cache.RaiseFieldUpdating<Field>(item, ref value);
			if (isRequired && value == null)
				throw new PXException(ErrorMessages.FieldIsEmpty, displayName);
			return value;
		}
		#endregion

		#region IPXPrepareItems
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
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
						barCodeCache.SetValueExt<INBarCodeItem.lotSerialNbr>(item, GetImportedValue<INPIDetail.lotSerialNbr>(values, false));
					barCodeCache.SetValueExt<INBarCodeItem.qty>(item, GetImportedValue<INPIDetail.physicalQty>(values, true));
					barCodeCache.SetValueExt<INBarCodeItem.autoAddLine>(item, false);
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
					excelRowNumber++;
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

		#endregion

		#region IPXProcess
		public void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			if (importHasError)
				throw new Exception(IN.Messages.ImportHasError);
		}
		#endregion

	}

    [Serializable]
	public partial class PICountFilter : IBqlTable
	{		
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(DisplayName="Inventory ID")]
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

		#region SubItem
		public abstract class subItem : PX.Data.BQL.BqlString.Field<subItem> { }
		protected String _SubItem;
		[SubItemRawExt(typeof(INSiteStatusFilter.inventoryID), DisplayName = "Subitem")]
		public virtual String SubItem
		{
			get
			{
				return this._SubItem;
			}
			set
			{
				this._SubItem = value;
			}
		}
		#endregion

		#region SubItemCD Wildcard
		public abstract class subItemCDWildcard : PX.Data.BQL.BqlString.Field<subItemCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubItemCDWildcard
		{
			get
			{
				//return SubItemCDUtils.CreateSubItemCDWildcard(this._SubItemCD);
				return this._SubItem == null ? null : SubCDUtils.CreateSubCDWildcard(this._SubItem, SubItemAttribute.DimensionName);
			}
		}
		#endregion				

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(INPIHeader.siteID), Visibility = PXUIVisibility.SelectorVisible)]
		//[Location(Visibility = PXUIVisibility.SelectorVisible)]
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

		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[LotSerialNbr]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion

		#region LotSerialNbrWildcard
		public abstract class lotSerialNbrWildcard : PX.Data.BQL.BqlString.Field<lotSerialNbrWildcard> { };
		[PXDBString(100, IsUnicode = true)]
		public virtual String LotSerialNbrWildcard
		{
			get
			{
				String sqlWildCard = PXDatabase.Provider.SqlDialect.WildcardAnything;
				return this._LotSerialNbr != null ? sqlWildCard + this._LotSerialNbr + sqlWildCard : null;
			}
		}
		#endregion

		#region StartLineNbr
		public abstract class startLineNbr : PX.Data.BQL.BqlInt.Field<startLineNbr> { }
		protected int? _StartLineNbr;
		[PXDBInt]
		[PXUIField(DisplayName = "Start Line Nbr.")]
		public virtual int? StartLineNbr
		{
			get
			{
				return this._StartLineNbr;
			}
			set
			{
				this._StartLineNbr = value;
			}
		}
		#endregion

		#region EndLineNbr
		public abstract class endLineNbr : PX.Data.BQL.BqlInt.Field<endLineNbr> { }
		protected int? _EndLineNbr;
		[PXDBInt]
		[PXUIField(DisplayName = "End Line Nbr.")]
		public virtual int? EndLineNbr
		{
			get
			{
				return this._EndLineNbr;
			}
			set
			{			
				this._EndLineNbr = value;
			}
		}
		#endregion
	}
}
