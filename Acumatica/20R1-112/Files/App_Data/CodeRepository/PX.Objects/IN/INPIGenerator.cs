using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Data.BQL;
using PX.Objects.IN.PhysicalInventory;

namespace PX.Objects.IN
{
	#region PI Generator Settings

	[Serializable]
	public partial class PIGeneratorSettings : PX.Data.IBqlTable
	{
		#region PIClassID
		public abstract class pIClassID : PX.Data.BQL.BqlString.Field<pIClassID> { }
		protected String _PIClassID;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Type ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INPIClass.pIClassID>))]
		[PXDefault()]
		public virtual String PIClassID
		{
			get
			{
				return this._PIClassID;
			}
			set
			{
				this._PIClassID = value;
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDefault()]
		[Site]
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

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDefault()]
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion

		#region Method
		public abstract class method : PX.Data.BQL.BqlString.Field<method> { }
		protected String _Method;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Generation Method", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PIMethod.List]
		public virtual String Method
		{
			get
			{
				return this._Method;
			}
			set
			{
				this._Method = value;
			}
		}
		#endregion

		#region SelectedMethod
		public abstract class selectedMethod : PX.Data.BQL.BqlString.Field<selectedMethod> { }
		protected String _SelectedMethod;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Selection Method", Enabled = false)]
		[PIInventoryMethod.List]
		public virtual String SelectedMethod
		{
			get
			{
				return this._SelectedMethod;
			}
			set
			{
				this._SelectedMethod = value;
			}
		}
		#endregion

		#region ByFrequency
		public abstract class byFrequency : PX.Data.BQL.BqlBool.Field<byFrequency> { }
		protected Boolean? _ByFrequency;
		[PXDBBool()]
		[PXUIField(DisplayName = "By Frequency")]
		public virtual Boolean? ByFrequency
		{
			get
			{
				return this._ByFrequency;
			}
			set
			{
				this._ByFrequency = value;
			}
		}
		#endregion

		#region CycleID
		public abstract class cycleID : PX.Data.BQL.BqlString.Field<cycleID> { }
		protected String _CycleID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(INPICycle.cycleID), DescriptionField = typeof(INPICycle.descr))]
		[PXUIField(DisplayName = "Cycle ID")]
		public virtual String CycleID
		{
			get
			{
				return this._CycleID;
			}
			set
			{
				this._CycleID = value;
			}
		}
		#endregion

		#region ABCCodeID
		public abstract class aBCCodeID : PX.Data.BQL.BqlString.Field<aBCCodeID> { }
		protected String _ABCCodeID;
		[PXDBString(1, IsFixed = true)]
		[PXSelector(typeof(INABCCode.aBCCodeID), DescriptionField = typeof(INABCCode.descr))]
		[PXUIField(DisplayName = "ABC Code")]
		public virtual String ABCCodeID
		{
			get
			{
				return this._ABCCodeID;
			}
			set
			{
				this._ABCCodeID = value;
			}
		}
		#endregion

		#region MovementClassID
		public abstract class movementClassID : PX.Data.BQL.BqlString.Field<movementClassID> { }
		protected String _MovementClassID;
		[PXDBString(1, IsFixed = true)]
		[PXSelector(typeof(INMovementClass.movementClassID), DescriptionField = typeof(INMovementClass.descr))]
		[PXUIField(DisplayName = "Movement Class ID")]
		public virtual String MovementClassID
		{
			get
			{
				return this._MovementClassID;
			}
			set
			{
				this._MovementClassID = value;
			}
		}
		#endregion

		#region BlankLines
		public abstract class blankLines : PX.Data.BQL.BqlShort.Field<blankLines> { }
		protected Int16? _BlankLines;
		[PXDefault((Int16)0)]
		[PXDBShort(MinValue = 0, MaxValue = 10000)]
		[PXUIField(DisplayName = "Blank Lines To Append")]
		public virtual Int16? BlankLines
		{
			get
			{
				return this._BlankLines;
			}
			set
			{
				this._BlankLines = value;
			}
		}
		#endregion

		#region RandomItemsLimit
		public abstract class randomItemsLimit : PX.Data.BQL.BqlShort.Field<randomItemsLimit> { }
		protected Int16? _RandomItemsLimit;
		[PXDefault((Int16)0)]
		[PXShort(MinValue = 0, MaxValue = 10000)]
		[PXUIField(DisplayName = "Number of Items (up to)")]
		public virtual Int16? RandomItemsLimit
		{
			get
			{
				return this._RandomItemsLimit;
			}
			set
			{
				this._RandomItemsLimit = value;
			}
		}
		#endregion

		#region LastCountPeriod
		public abstract class lastCountPeriod : PX.Data.BQL.BqlShort.Field<lastCountPeriod> { }
		protected Int16? _LastCountPeriod;
		[PXDefault((Int16)0)]
		[PXDBShort(MinValue = 0, MaxValue = 10000)]
		[PXUIField(DisplayName = "Last Count Before (days)", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int16? LastCountPeriod
		{
			get
			{
				return this._LastCountPeriod;
			}
			set
			{
				this._LastCountPeriod = value;
			}
		}
		#endregion

		#region RandomSeed
		// to save set of randomly-selected items between roundtrips 
		public int RandomSeed;
		#endregion

		#region MaxLastCountDate
		public abstract class maxLastCountDate : PX.Data.BQL.BqlDateTime.Field<maxLastCountDate> { }
		protected DateTime? _MaxLastCountDate;
		[PXDate]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Last Count Not Later")]
		public virtual DateTime? MaxLastCountDate
		{
			get
			{
				return this._MaxLastCountDate;
			}
			set
			{
				this._MaxLastCountDate = value;
			}
		}
		#endregion
	}

	#endregion
	
	#region PIPreliminaryResult
    [Serializable]
	public partial class PIPreliminaryResult : PX.Data.IBqlTable
	{

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region TagNumber
		public abstract class tagNumber : PX.Data.BQL.BqlInt.Field<tagNumber> { }
		protected Int32? _TagNumber;
		[PXInt(MinValue = 0)]
		[PXUIField(DisplayName = "Tag Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? TagNumber
		{
			get
			{
				return this._TagNumber;
			}
			set
			{
				this._TagNumber = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Inventory ID")]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Subitem")]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
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
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Lot/Serial Number", Visibility = PXUIVisibility.SelectorVisible)]
		//[INLotSerialNbr(typeof(PIPreliminaryResult.inventoryID), typeof(PIPreliminaryResult.subItemID), typeof(PIPreliminaryResult.locationID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Expiration Date")]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region BookQty
		public abstract class bookQty : PX.Data.BQL.BqlDecimal.Field<bookQty> { }
		protected Decimal? _BookQty;
		[PXDBQuantity]
		//[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Book Qty.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? BookQty
		{
			get
			{
				return this._BookQty;
			}
			set
			{
				this._BookQty = value;
			}
		}
		#endregion
		#region BaseUnit
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		protected String _BaseUnit;
		[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible)]
		public virtual String BaseUnit
		{
			get
			{
				return this._BaseUnit;
			}
			set
			{
				this._BaseUnit = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
		public virtual int? ItemClassID { get; set; }
		#endregion
	}
	#endregion

	#region ExcludedLocation
	[Serializable]
	public partial class ExcludedLocation : PX.Data.IBqlTable
	{
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Location ID")]
		[PXDimensionSelector(
			LocationAttribute.DimensionName,
			typeof(Search<INLocation.locationID,
				Where<INLocation.siteID, Equal<Current<PIGeneratorSettings.siteID>>,
				And<INLocation.active, Equal<boolTrue>>>>),
			typeof(INLocation.locationCD),
			DescriptionField = typeof(INLocation.descr))]
		public virtual Int32? LocationID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Enabled = false)]
		public virtual String Descr
		{
			get;
			set;
		}
		#endregion
	}
	#endregion

	#region ExcludedInventoryItem
	[Serializable]
	public partial class ExcludedInventoryItem : PX.Data.IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		[StockItem(IsKey = true, DescriptionField = typeof(InventoryItem.descr))]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Enabled = false)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String Descr
		{
			get;
			set;
		}
		#endregion
	}
	#endregion

	public class PIGenerator : PXGraph<PIGenerator>
	{
		public PXCancel<PIGeneratorSettings> Cancel;

		public PXFilter<PIGeneratorSettings> GeneratorSettings;

		public PXFilter<PIGeneratorSettings> CurrentGeneratorSettings;

		#region Cache Attached

		[PXDBInt(IsKey = true)]
		protected virtual void INLocationStatus_InventoryID_CacheAttached(PXCache sender)
		{ 
		}

		[PXDBInt(IsKey = true)]
		protected virtual void INLocationStatus_SubItemID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt(IsKey = true)]
		protected virtual void INLocationStatus_SiteID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt(IsKey = true)]
		protected virtual void INLocationStatus_LocationID_CacheAttached(PXCache sender)
		{
		}

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

		#endregion

		public PXSelectOrderBy<PIPreliminaryResult, OrderBy<Asc<PIPreliminaryResult.lineNbr>>> PreliminaryResultRecs;

		public PXFilter<INLocation> LocationsToLock;
		[PXVirtualDAC] // We should disable requests to DB table from cache as the table does not exists.
		[PXNotCleanable]
		public PXFilter<ExcludedLocation> ExcludedLocations;

		public PXFilter<InventoryItem> InventoryItemsToLock;
		[PXVirtualDAC] // We should disable requests to DB table from cache as the table does not exists.
		[PXNotCleanable]
		public PXFilter<ExcludedInventoryItem> ExcludedInventoryItems;

		public PXSelect<INPIHeader> piheader;
		public PXSelect<INPIDetail, Where<INPIDetail.pIID, Equal<Current<INPIHeader.pIID>>>> pidetail;

        public PXSelect<INLocationStatus> inlocationstatus;

		public PXSelect<INPIStatusItem> inpistatusitem;
		public PXSelect<INPIStatusLoc> inpistatusloc;
		// (PICountStatus flag now in INPIStatusXXX, not INItemSite)

		public PXSetup<INSetup> insetup;

		public PXAction<PIGeneratorSettings> GeneratePI;

		[PXUIField(DisplayName = Messages.GeneratePI, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable generatePI(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this.UID, (argument =>
				{
					PIGenerator graph = PXGraph.CreateInstance<PIGenerator>();
					graph.GeneratorSettings.Current = (PIGeneratorSettings) argument[0];
					foreach (ExcludedLocation location in (List<ExcludedLocation>) argument[1])
					{
						graph.ExcludedLocations.Cache.SetStatus(location, PXEntryStatus.Updated);
					}
					foreach (ExcludedInventoryItem inventory in (List<ExcludedInventoryItem>) argument[2])
					{
						graph.ExcludedInventoryItems.Cache.SetStatus(inventory, PXEntryStatus.Updated);
					}

					// based on generation settings, recreate list (as in PreliminaryResultRecs) and save it to INPIHeader / INPIDetail tables, updating statuses in INItemSite and LastTagNumber in INSetup					
                    var list = graph.CalcPIRows(true, false);
                    if (list.Count == 0)
					{
						throw new PXException(Messages.PIEmpty);
					}

					// !!!  upon PI generation - redirect to the PI Review screen here 
					INPIReview target = PXGraph.CreateInstance<INPIReview>();
					target.Clear();
					target.PIHeader.Current = PXSelect<INPIHeader, Where<INPIHeader.pIID, Equal<Current<INPIHeader.pIID>>>>.Select(this);

					throw new PXRedirectRequiredException(target, "View INPIReview");
				}
				),
				new object[]
				{
					this.GeneratorSettings.Current,
					this.ExcludedLocations.Select().RowCast<ExcludedLocation>().ToList(),
					this.ExcludedInventoryItems.Select().RowCast<ExcludedInventoryItem>().ToList(),
				});

			return adapter.Get();
		}
	
		public PIGenerator()
		{
			PreliminaryResultRecs.Cache.AllowInsert = false;
			PreliminaryResultRecs.Cache.AllowDelete = false;
			PreliminaryResultRecs.Cache.AllowUpdate = false;

			LocationsToLock.AllowInsert = false;
			LocationsToLock.AllowUpdate = false;
			LocationsToLock.AllowDelete = false;

			InventoryItemsToLock.AllowInsert = false;
			InventoryItemsToLock.AllowUpdate = false;
			InventoryItemsToLock.AllowDelete = false;
		}

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		public virtual List<PIPreliminaryResult> CalcPIRows(bool updateDB)
		{
			return CalcPIRows(updateDB, true);
		}

		public virtual List<PIPreliminaryResult> CalcPIRows(bool updateDB, bool saveEmpty)
		{
			INSetup insetuprec = insetup.Current;
			PIGeneratorSettings gs = GeneratorSettings.Current;

			if (gs?.PIClassID == null || gs?.SiteID == null)
			{
				return new List<PIPreliminaryResult>();
			}

			if (updateDB)
			{
				piheader.Cache.Clear();
				pidetail.Cache.Clear();
				inpistatusitem.Cache.Clear();
				inpistatusloc.Cache.Clear();
			}

			var piTypeLocations = GetPiTypeLocations();
			var excludedInventoryIds = ExcludedInventoryItems.Select().RowCast<ExcludedInventoryItem>()
				.Where(i => i.InventoryID != null).Select(i => (int)i.InventoryID).ToHashSet();
			var excludedLocationIds = ExcludedLocations.Select().RowCast<ExcludedLocation>()
				.Where(i => i.LocationID != null).Select(i => (int)i.LocationID).ToHashSet();

			var intermediateResult = PrepareIntermediateResult(piTypeLocations, excludedInventoryIds, excludedLocationIds);

			int NextLineNbr = 1;

			if (intermediateResult.Count == 0 && (gs.Method != PIMethod.FullPhysicalInventory || gs.BlankLines == 0
				|| piTypeLocations.Count > 0 && piTypeLocations.All(l => excludedLocationIds.Contains((int)((INLocation)l).LocationID))))
				return new List<PIPreliminaryResult>();

			if (updateDB)
			{
				INPIHeader ph_rec = new INPIHeader();
				ReasonCode rc_rec = ReasonCode.PK.Find(this, insetuprec.PIReasonCode);
				ph_rec.PIClassID = gs.PIClassID;
				ph_rec.Descr = gs.Descr;
				ph_rec.SiteID = gs.SiteID;
				ph_rec.Status = INPIHdrStatus.Counting;
				if (rc_rec != null)
				{
					ph_rec.PIAdjAcctID = rc_rec.AccountID;
					ph_rec.PIAdjSubID = rc_rec.SubID;
				}
				ph_rec.TagNumbered = insetuprec.PIUseTags;
				ph_rec.TotalNbrOfTags = 0;

				ph_rec.LineCntr = 0;

				// zeroes - for the init
				ph_rec.TotalVarQty = 0m;
				ph_rec.TotalVarCost = 0m;

				// pih_rec.FinPeriodID pih_rec.TranPeriodID - ��� ������ PI

				piheader.Cache.Insert(ph_rec);
			}

			var resultList = new List<PIPreliminaryResult>();
			foreach (var il in intermediateResult)
			{
				InventoryItem ii_rec = il.QueryResult.GetItem<InventoryItem>();

				PIPreliminaryResult item = new PIPreliminaryResult();

				item.InventoryID = il.InventoryID;
				item.SubItemID = il.SubItemID;
				item.LocationID = il.LocationID;
				item.Descr = ii_rec.Descr;
				item.BaseUnit = ii_rec.BaseUnit;
				item.ItemClassID = ii_rec.ItemClassID;

				INLotSerialStatus lss_rec = il.QueryResult.GetItem<INLotSerialStatus>();
				if (lss_rec?.LotSerialNbr != null) // nulls left-joined - non-lot-serial
				{
					item.LotSerialNbr = lss_rec.LotSerialNbr;
					item.ExpireDate = lss_rec.ExpireDate;
					//If client has not completed return with created shipment, then QtyActual may exeed QtyOnHand
					// and the cliend will face with PI Adjustment release problems
					item.BookQty = Math.Min((decimal)lss_rec.QtyActual, (decimal)lss_rec.QtyOnHand);
				}
				else
				{
					item.LotSerialNbr = null;
					item.ExpireDate = null;
					item.BookQty = 0;

					INLocationStatus ls_rec = il.QueryResult.GetItem<INLocationStatus>();
					if (ls_rec != null)
					{
						//If client has not completed return with created shipment, then QtyActual may exeed QtyOnHand
						// and the cliend will face with PI Adjustment release problems
						item.BookQty = Math.Min((decimal)ls_rec.QtyActual, (decimal)ls_rec.QtyOnHand);
					}
				}

				item.LineNbr = NextLineNbr++;
				if (insetuprec.PIUseTags ?? false)
				{
					item.TagNumber = (insetuprec.PILastTagNumber ?? 0) + item.LineNbr;
				}

				if (gs.ByFrequency == true)
				{
					if (gs.Method == PIMethod.ByCycle && !IsReadyToBeCounted<INPICycle>(il.QueryResult))
						continue;

					if (gs.Method == PIMethod.ByABCClass && !IsReadyToBeCounted<INABCCode>(il.QueryResult))
						continue;

					if (gs.Method == PIMethod.ByMovementClass && !IsReadyToBeCounted<INMovementClass>(il.QueryResult))
						continue;
				}

				resultList.Add(item);

				if (!updateDB)
					continue;

				INPIHeader pih_rec = piheader.Current;

				INPIDetail pid_rec = new INPIDetail();

				pid_rec.BookQty = item.BookQty;

				//pih_rec.LineCntr++;
				if (insetuprec.PIUseTags ?? false)
				{
					pih_rec.TotalNbrOfTags++;
				}

				pid_rec.LineNbr = item.LineNbr;
				pid_rec.TagNumber = item.TagNumber;

				pid_rec.InventoryID = item.InventoryID;
				pid_rec.LocationID = item.LocationID;
				pid_rec.LotSerialNbr = item.LotSerialNbr;
				pid_rec.ExpireDate = item.ExpireDate;
				pid_rec.Status = INPIDetStatus.NotEntered;
				pid_rec.SubItemID = item.SubItemID;
				pid_rec.LineType = INPIDetLineType.Normal;

				piheader.Update(pih_rec);
				pid_rec = pidetail.Insert(pid_rec);
			}

			// blank lines generation
			for (int i = 0; i < gs.BlankLines; i++)
			{
				PIPreliminaryResult item = new PIPreliminaryResult();

				item.LineNbr = NextLineNbr++;
				item.BookQty = 0m;
				if (insetuprec.PIUseTags ?? false)
				{
					item.TagNumber = (insetuprec.PILastTagNumber ?? 0) + item.LineNbr;
				}
				resultList.Add(item);

				if (!updateDB) continue;

				INPIHeader pih_rec = piheader.Current;

				INPIDetail pid_rec = new INPIDetail();

				pid_rec.BookQty = item.BookQty;

				pih_rec.LineCntr++;
				if (insetuprec.PIUseTags ?? false)
					pih_rec.TotalNbrOfTags++;

				pid_rec.LineNbr = item.LineNbr;
				pid_rec.TagNumber = item.TagNumber;

				pid_rec.InventoryID = item.InventoryID;
				pid_rec.LocationID = item.LocationID;
				pid_rec.LotSerialNbr = item.LotSerialNbr;
				pid_rec.ExpireDate = item.ExpireDate;
				pid_rec.Status = INPIDetStatus.NotEntered;
				pid_rec.SubItemID = item.SubItemID;

				pid_rec.LineType = INPIDetLineType.Blank;

				piheader.Cache.Update(pih_rec);
				pid_rec = pidetail.Insert(pid_rec);
			}

			if (!updateDB)
				return resultList;

			if (insetuprec.PIUseTags ?? false)
			{
				insetuprec.PILastTagNumber = (insetuprec.PILastTagNumber ?? 0) + NextLineNbr - 1;
				insetup.Cache.Update(insetuprec);
			}

			// TODO: can we avoid double call 'PrepareIntermediateResult' in order to use 'InventoryItems.Select().RowCast<InventoryItem>().ToList()'?
			bool fullItemsLock = (gs.Method == PIMethod.FullPhysicalInventory);
			HashSet<int> inventoryItemIds = fullItemsLock ? excludedInventoryIds
				: GetInventoryItemsToLock(intermediateResult, excludedInventoryIds).Select(i => (int)i.InventoryID).ToHashSet();
			bool fullLocationsLock = (piTypeLocations.Count == 0);
			HashSet<int> locationIds = fullLocationsLock ? excludedLocationIds
				: LocationsToLock.Select().RowCast<INLocation>().Select(l => (int)l.LocationID).ToHashSet();

			var site = INSite.PK.Find(this, GeneratorSettings.Current?.SiteID);
			CreatePILocksManager(piheader.Current.PIID).Lock(
				fullItemsLock, inventoryItemIds,
				fullLocationsLock, locationIds,
				site.SiteCD.Trim());

			if (saveEmpty || resultList.Count > 0)
			{
				this.Actions.PressSave();
			}

			return resultList;
		}

		protected virtual List<PIItemLocationInfo> PrepareIntermediateResult(
			PXResultset<INLocation> piTypeLocations,
			HashSet<int> excludedInventoryIds,
			HashSet<int> excludedLocationIds)
		{
			PIGeneratorSettings gs = GeneratorSettings.Current;

			if (gs?.PIClassID == null || gs?.SiteID == null)
			{
				return new List<PIItemLocationInfo>();
			}

			BqlCommand initialCmd = BqlCommand.CreateInstance(
				typeof(
				Select2<INLocationStatus,
						InnerJoin<InventoryItem,
							On2<INLocationStatus.FK.InventoryItem,
								And<InventoryItem.stkItem, Equal<True>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
								And<Where<Match<InventoryItem, Current<AccessInfo.userName>>>>>>>>,
						LeftJoin<INLotSerClass,
							On<InventoryItem.FK.LotSerClass>,
						LeftJoin<INLotSerialStatus,
							On<INLotSerialStatus.inventoryID, Equal<INLocationStatus.inventoryID>,
								And<INLotSerClass.lotSerAssign, Equal<INLotSerAssign.whenReceived>,
								And<INLotSerialStatus.subItemID, Equal<INLocationStatus.subItemID>,
								And<INLotSerialStatus.siteID, Equal<INLocationStatus.siteID>,
								And<INLotSerialStatus.locationID, Equal<INLocationStatus.locationID>,
								And<INLotSerClass.lotSerTrack, NotEqual<INLotSerTrack.notNumbered>>>>>>>,
						InnerJoin<INSubItem,
							On<INLocationStatus.FK.SubItem>,
						InnerJoin<INLocation,
							On<INLocationStatus.FK.Location>,
						InnerJoin<INItemSiteSettings,
							On<INItemSiteSettings.inventoryID, Equal<INLocationStatus.inventoryID>,
								And<INItemSiteSettings.siteID, Equal<INLocationStatus.siteID>>>>>>>>>,
						Where<InventoryItem.stkItem, Equal<boolTrue>,
							And<INLocationStatus.siteID, Equal<Current<PIGeneratorSettings.siteID>>,
							And<Where<INLotSerialStatus.inventoryID, IsNotNull,
								And<INLotSerialStatus.qtyActual, NotEqual<decimal0>,
								Or<INLotSerialStatus.inventoryID, IsNull,
								And<INLocationStatus.qtyActual, NotEqual<decimal0>>>>>>>>>));

			if (piTypeLocations.Count > 0)
			{
				initialCmd = UpdateCommandByLocationList<INLocationStatus.locationID>(initialCmd);
			}

			BqlCommandWithParameters cmd = UpdateCommandByGenerationMethod<INLocationStatus.siteID, INLocationStatus.inventoryID,
				INLocationStatus.subItemID, INLocationStatus.locationID>(new BqlCommandWithParameters(initialCmd));

			var intermediateResult = new List<PIItemLocationInfo>();

			INPIClass piClass = (INPIClass)PXSelectorAttribute.Select<PIGeneratorSettings.pIClassID>(GeneratorSettings.Cache, gs);
			var comparer = CreateItemLocationComparer(piClass);

			var view = new PXView(this, true, cmd.Command);
			intermediateResult.AddRange(
				SelectWithinFieldScope(view, cmd.GetParameters(), comparer.GetSortColumns())
					.Where(il => !excludedInventoryIds.Contains(il.InventoryID) && !excludedLocationIds.Contains(il.LocationID)));

			// Adding Items with 0 book qty:
			if (piClass.IncludeZeroItems == true && gs.Method != PIMethod.FullPhysicalInventory
				&& (gs.Method != PIMethod.ByInventoryItemSelected || gs.SelectedMethod != PIInventoryMethod.ItemsHavingNegativeBookQty))
			{
				var zeroItemsInitialCmd = BqlCommand.CreateInstance(typeof(
					Select5<INItemSiteHistDay,
						InnerJoin<InventoryItem,
							On<InventoryItem.inventoryID, Equal<INItemSiteHistDay.inventoryID>>,
						InnerJoin<INSubItem,
										On<INSubItem.subItemID, Equal<INItemSiteHistDay.subItemID>>,
						InnerJoin<INLocation,
										On<INLocation.locationID, Equal<INItemSiteHistDay.locationID>>,
						InnerJoin<INLotSerClass, 
							On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>,
								And<INLotSerClass.lotSerTrack, Equal<INLotSerTrack.notNumbered>>>>>>>,
						Where<INItemSiteHistDay.sDate, GreaterEqual<Required<INItemSiteHistDay.sDate>>,
							And<INItemSiteHistDay.siteID, Equal<Current<PIGeneratorSettings.siteID>>,
							And<Where<INItemSiteHistDay.qtyDebit, NotEqual<Zero>, Or<INItemSiteHistDay.qtyCredit, NotEqual<Zero>>>>>>,
						Aggregate<GroupBy<INItemSiteHistDay.inventoryID,
							GroupBy<INItemSiteHistDay.subItemID,
							GroupBy<INItemSiteHistDay.locationID>>>>>));

				bool useLastCountDate = gs.Method == PIMethod.ByInventoryItemSelected
					&& gs.SelectedMethod == PIInventoryMethod.LastCountDate && gs.MaxLastCountDate != null
					&& gs.MaxLastCountDate.Value < ((DateTime)Accessinfo.BusinessDate).AddYears(-1);

				BqlCommandWithParameters zeroItemsCmd = new BqlCommandWithParameters(zeroItemsInitialCmd);
				zeroItemsCmd.WhereParameters.Add(useLastCountDate ? gs.MaxLastCountDate : ((DateTime)Accessinfo.BusinessDate).AddYears(-1));

				if (piTypeLocations.Count > 0)
				{
					zeroItemsCmd.Command = UpdateCommandByLocationList<INItemSiteHistDay.locationID>(zeroItemsCmd.Command);
				}

				zeroItemsCmd = UpdateCommandByGenerationMethod<INItemSiteHistDay.siteID, INItemSiteHistDay.inventoryID,
					INItemSiteHistDay.subItemID, INItemSiteHistDay.locationID>(zeroItemsCmd);

				var existingItems = intermediateResult.ToHashSet();
				var zeroItemsView = new PXView(this, true, zeroItemsCmd.Command);
				intermediateResult.AddRange(SelectWithinFieldScope(zeroItemsView, zeroItemsCmd.GetParameters(), null)
					.Where(il => !existingItems.Contains(il) && !excludedInventoryIds.Contains(il.InventoryID) && !excludedLocationIds.Contains(il.LocationID)));
			}

			// Post-processing for Randomly-Selected Items and Items Having Negative Book Qty:
			if (gs.Method == PIMethod.ByInventoryItemSelected)
			{
				switch (gs.SelectedMethod)
				{
					case PIInventoryMethod.RandomlySelectedItems:
						short randomItemsLimit = gs.RandomItemsLimit ?? 0;
						if (randomItemsLimit == 0)
							break;

						var allInventoryIDs = intermediateResult.Select(il => il.InventoryID).Distinct().ToList();
						if (allInventoryIDs.Count > randomItemsLimit)
						{
							allInventoryIDs.Sort(); // to get the same list between roundtrips
							SetRandomSeedIfZero();
							Random randObj = new Random(gs.RandomSeed); // to get the same list between roundtrips

							// get random inventory ids
							var randomInventoryIDs = new HashSet<int>();
							for (int i = 0; i < randomItemsLimit; i++)
							{
								int nextRandomElementToSelect = randObj.Next(0, (allInventoryIDs.Count));
								//2nd parm in Next is exclusive, so Count instead of Count-1
								randomInventoryIDs.Add(allInventoryIDs[nextRandomElementToSelect]);
								allInventoryIDs.RemoveAt(nextRandomElementToSelect);
							}

							intermediateResult = intermediateResult.Where(it => randomInventoryIDs.Contains(it.InventoryID)).ToList();
						}
						break;

					case PIInventoryMethod.ItemsHavingNegativeBookQty:
						var newIntermediateResult = new List<PIItemLocationInfo>();

						// If Item has negative BookQty on one location we should add this item with all other locations too.
						foreach (var iRes in intermediateResult)
						{
							newIntermediateResult.Add(iRes);

							cmd.Command = cmd.Command.WhereNew<Where<InventoryItem.stkItem, Equal<boolTrue>,
								And<INLocationStatus.siteID, Equal<Current<PIGeneratorSettings.siteID>>,
									And<INLocationStatus.inventoryID, Equal<Required<INLocationStatus.inventoryID>>,
										And<INLocationStatus.subItemID, Equal<Required<INLocationStatus.subItemID>>,
											And<INLocationStatus.locationID, NotEqual<Required<INLocationStatus.locationID>>>>>>>>();

							cmd.WhereParameters = new List<object>
							{ 
								iRes.InventoryID,
								iRes.SubItemID,
								iRes.LocationID
							};

							var negativeBookQtyView = new PXView(this, true, cmd.Command);
							newIntermediateResult.AddRange(SelectWithinFieldScope(negativeBookQtyView, cmd.GetParameters(), comparer.GetSortColumns())
								.Where(il => !excludedInventoryIds.Contains(il.InventoryID) && !excludedLocationIds.Contains(il.LocationID)));
						}

						intermediateResult = newIntermediateResult;
						break;
				}
			}

			// We need to restore the sort order.
			intermediateResult.Sort(comparer);
			return intermediateResult;
		}

		protected virtual IItemLocationComparer CreateItemLocationComparer(INPIClass piClass)
		{
			return new PIItemLocationComparer(piClass);
		}

		protected virtual PILocksManager CreatePILocksManager(string piId)
		{
			PIGeneratorSettings gs = GeneratorSettings.Current;
			return new PILocksManager(this, inpistatusitem, inpistatusloc, (int)gs.SiteID, piId);
		}

		protected virtual IEnumerable<PIItemLocationInfo> SelectWithinFieldScope(PXView view, object[] parameters, string[] sortColumns)
		{
			int startRow = 0;
			int totalRows = 0;

			using (new PXFieldScope(view,
				typeof(InventoryItem.inventoryID),
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.descr),
				typeof(InventoryItem.stkItem),
				typeof(InventoryItem.baseUnit),
				typeof(InventoryItem.itemClassID),
				typeof(INSubItem.subItemID),
				typeof(INSubItem.subItemCD),
				typeof(INLocation.locationID),
				typeof(INLocation.locationCD),
				typeof(INLocationStatus.qtyOnHand),
				typeof(INLocationStatus.qtyActual),
				typeof(INLotSerialStatus.lotSerialNbr),
				typeof(INLotSerialStatus.expireDate),
				typeof(INLotSerialStatus.qtyOnHand),
				typeof(INLotSerialStatus.qtyActual),
				typeof(INPICycle.countsPerYear),
				typeof(INABCCode.countsPerYear),
				typeof(INMovementClass.countsPerYear),
				typeof(LastPICountDate.lastCountDate)))
			{
				return view.Select(null, parameters, null, sortColumns, null, null, ref startRow, -1, ref totalRows)
						.Select(result => PIItemLocationInfo.Create((PXResult)result));
			}
		}

		protected virtual ICollection<InventoryItem> GetInventoryItemsToLock(List<PIItemLocationInfo> intermediateResult, HashSet<int> excludedInventoryIds)
		{
			PIGeneratorSettings gs = GeneratorSettings.Current;
			return GetDistinctItems(
				intermediateResult.Select(il => il.QueryResult.GetItem<InventoryItem>())
					.Union(GetAdditionalInventoryItemsToLock(gs)),
				excludedInventoryIds);
		}

		protected ICollection<InventoryItem> GetDistinctItems(IEnumerable<InventoryItem> inventoryItems, HashSet<int> excludedInventoryIds)
		{
			var distinctItemsById = new Dictionary<int, InventoryItem>();
			foreach (var item in inventoryItems)
			{
				if (excludedInventoryIds.Contains((int)item.InventoryID) || distinctItemsById.ContainsKey((int)item.InventoryID))
					continue;

				distinctItemsById.Add((int)item.InventoryID, item);
			}
			return distinctItemsById.Values;
		}

		protected virtual List<InventoryItem> GetAdditionalInventoryItemsToLock(PIGeneratorSettings gs)
		{
			if (gs.Method != PIMethod.FullPhysicalInventory &&
				(gs.Method != PIMethod.ByInventoryItemSelected || //ByInventoryItemSelected->ListOfItems || AnyClass\Cycle method
				gs.SelectedMethod != PIInventoryMethod.ItemsHavingNegativeBookQty && 
				gs.SelectedMethod != PIInventoryMethod.RandomlySelectedItems &&
				gs.SelectedMethod != PIInventoryMethod.LastCountDate) &&
								(gs.ByFrequency == false || // AND NOT ( (ByABCClass||ByMovementClass||ByCycle) && ByFrequency == true )
									gs.Method != PIMethod.ByABCClass && 
									gs.Method != PIMethod.ByMovementClass &&
									gs.Method != PIMethod.ByCycle))
				// Prepare additional locks only for:
				// * ByInventoryItemSelected -> ListOfItems
				// * ByItemClass
				// * (ByABCClass || ByMovementClass || ByCycle) && ByFrequency == false
			{
				BqlCommand initialCmd = BqlCommand.CreateInstance(typeof(
					Select4<InventoryItem,
					Where<InventoryItem.stkItem, Equal<True>,
						And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
						And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
						And<Where<Match<InventoryItem, Current<AccessInfo.userName>>>>>>>,
					Aggregate<GroupBy<InventoryItem.inventoryID, Count>>>));

				// According to the above 'if' part of generic params are not used
				BqlCommandWithParameters cmd = UpdateCommandByGenerationMethod<Current<PIGeneratorSettings.siteID>, InventoryItem.inventoryID,
					BqlPlaceholder.A, BqlPlaceholder.B>(new BqlCommandWithParameters(initialCmd));

				int startRow = 0;
				int totalRows = 0;
				PXView view = new PXView(this, true, cmd.Command);
				using(new PXFieldScope(view, typeof(InventoryItem.inventoryID), typeof(InventoryItem.stkItem), typeof(InventoryItem.inventoryCD)))
				{
					return view.Select(null, cmd.GetParameters(), null, null, null, null, ref startRow, -1, ref totalRows)
							.RowCast<InventoryItem>()
							.ToList();
				}
			}

			return new List<InventoryItem>();
		}

		private BqlCommand UpdateCommandByLocationList<FieldLocationID>(BqlCommand cmd)
			where FieldLocationID : IBqlField
		{
			return BqlCommand.AppendJoin<InnerJoin<INPIClassLocation,
								On<INPIClassLocation.pIClassID, Equal<Current<PIGeneratorSettings.pIClassID>>,
								And<INPIClassLocation.locationID, Equal<FieldLocationID>>>>>(cmd);
		}

		protected virtual BqlCommandWithParameters UpdateCommandByGenerationMethod<TSiteIdField, TInventoryIdField, TSubItemIdField, TLocationIdField>(
			BqlCommandWithParameters cmd)
			where TSiteIdField : IBqlOperand
			where TInventoryIdField : IBqlField
			where TSubItemIdField : IBqlField
			where TLocationIdField : IBqlField
		{
			PIGeneratorSettings gs = GeneratorSettings.Current;
			switch (gs.Method)
			{
				case PIMethod.FullPhysicalInventory: // full physical inventory : no additional WHERE
					break;
				case PIMethod.ByInventoryItemSelected:

					if (gs.SelectedMethod == PIInventoryMethod.LastCountDate)
					{
						cmd.Command = BqlCommand.AppendJoin<LeftJoin<LastPICountDate, // Last Count Date is needed for several methods
											On<LastPICountDate.siteID, Equal<TSiteIdField>,
												And<LastPICountDate.inventoryID, Equal<TInventoryIdField>,
												And<LastPICountDate.subItemID, Equal<TSubItemIdField>,
												And<LastPICountDate.locationID, Equal<TLocationIdField>>>>>>>(cmd.Command);

						cmd.Command  = cmd.Command.WhereAnd<Where<LastPICountDate.lastCountDate, IsNull,
							Or<LastPICountDate.lastCountDate, LessEqual<Current<PIGeneratorSettings.maxLastCountDate>>>>>();
					}

					if (gs.SelectedMethod == PIInventoryMethod.ItemsHavingNegativeBookQty)
					{// Here we should have Join to INLocationStatus, so generic params are redundant here.
						cmd.Command = JoinLotSerial(cmd.Command);
						cmd.Command = cmd.Command.WhereNew<
							Where<InventoryItem.stkItem, Equal<boolTrue>,
								And<INLocationStatus.siteID, Equal<Current<PIGeneratorSettings.siteID>>,
								And<Where<INLotSerialStatus.inventoryID, IsNotNull,
									And<INLotSerialStatus.qtyActual, Less<decimal0>,
									Or<INLotSerialStatus.inventoryID, IsNull,
									And<INLocationStatus.qtyActual, Less<decimal0>>>>>>>>>();
					}

					if (gs.SelectedMethod == PIInventoryMethod.ListOfItems)
						cmd.Command = BqlCommand.AppendJoin<InnerJoin<INPIClassItem,
										On<INPIClassItem.pIClassID, Equal<Current<PIGeneratorSettings.pIClassID>>,
									And<INPIClassItem.inventoryID, Equal<InventoryItem.inventoryID>>>>>(cmd.Command);
					break;
				case PIMethod.ByItemClassID:
					string[] wildcards = 
						PXSelectReadonly2<INItemClass,
						InnerJoin<INPIClassItemClass, On<INPIClassItemClass.FK.ItemClass>>, 
						Where<INPIClassItemClass.pIClassID, Equal<Current<PIGeneratorSettings.pIClassID>>>>
						.Select(this)
						.RowCast<INItemClass>()
						.Select(ic => ItemClassTree.MakeWildcard(ic.ItemClassCD.TrimEnd()))
						.ToArray();
					if (wildcards.Any())
					{
						Type[] itemClassJoin = BqlCommand.Decompose(typeof(
							InnerJoin<INItemClass, On2<InventoryItem.FK.ItemClass,
							And<INItemClass.itemClassID, In2<Search<INItemClass.itemClassID, BqlNone>>>>>));
						itemClassJoin[itemClassJoin.LastIndex()] = WhereFieldIsLikeOneOfParameters<INItemClass.itemClassCD>(wildcards.Length);
						cmd.Command = BqlCommand.AppendJoin(cmd.Command, BqlCommand.Compose(itemClassJoin)); 
						cmd.JoinParameters.AddRange(wildcards);
					}
					break;
				case PIMethod.ByABCClass:
					cmd.Command = JoinItemSiteSettings<TSiteIdField, TInventoryIdField>(cmd.Command);
					if (gs.ByFrequency == true)
						cmd.Command = BqlCommand.AppendJoin<
							InnerJoin<INABCCode,
									On<INABCCode.aBCCodeID, Equal<INItemSiteSettings.aBCCodeID>,
										And<INABCCode.countsPerYear, IsNotNull,
										And<INABCCode.countsPerYear, NotEqual<short0>>>>,
							LeftJoin<LastPICountDate, // Last Count Date is needed for several methods
									On<LastPICountDate.siteID, Equal<TSiteIdField>,
										And<LastPICountDate.inventoryID, Equal<TInventoryIdField>,
										And<LastPICountDate.locationID, Equal<TLocationIdField>>>>>>>(cmd.Command);
					else
						cmd.Command = cmd.Command.WhereAnd<Where<INItemSiteSettings.aBCCodeID, Equal<Current<PIGeneratorSettings.aBCCodeID>>>>();
					break;
				case PIMethod.ByMovementClass:
					cmd.Command = JoinItemSiteSettings<TSiteIdField, TInventoryIdField>(cmd.Command);
					if (gs.ByFrequency == true)
						cmd.Command = BqlCommand.AppendJoin<
							InnerJoin<INMovementClass,
									On<INMovementClass.movementClassID, Equal<INItemSiteSettings.movementClassID>,
										And<INMovementClass.countsPerYear, IsNotNull,
										And<INMovementClass.countsPerYear, NotEqual<short0>>>>,
						 LeftJoin<LastPICountDate, // Last Count Date is needed for several methods
								On<LastPICountDate.siteID, Equal<TSiteIdField>,
									And<LastPICountDate.inventoryID, Equal<TInventoryIdField>,
									And<LastPICountDate.locationID, Equal<TLocationIdField>>>>>>>(cmd.Command);
					else
						cmd.Command = cmd.Command.WhereAnd<Where<INItemSiteSettings.movementClassID, Equal<Current<PIGeneratorSettings.movementClassID>>>>();
					break;


				case PIMethod.ByCycle:
					if (gs.ByFrequency == true)
						cmd.Command = BqlCommand.AppendJoin<
							InnerJoin<INPICycle,
									On<INPICycle.cycleID, Equal<InventoryItem.cycleID>,
										And<INPICycle.countsPerYear, IsNotNull,
										And<INPICycle.countsPerYear, NotEqual<short0>>>>,
							 LeftJoin<LastPICountDate, // Last Count Date is needed for several methods
									On<LastPICountDate.siteID, Equal<TSiteIdField>,
										And<LastPICountDate.inventoryID, Equal<TInventoryIdField>,
										And<LastPICountDate.locationID, Equal<TLocationIdField>>>>>>>(cmd.Command);
					else
						cmd.Command = cmd.Command.WhereAnd<Where<InventoryItem.cycleID, Equal<Current<PIGeneratorSettings.cycleID>>>>();
					break;
			}
			return cmd;
		}

		private Type WhereFieldIsLikeOneOfParameters<TField>(int parametersCount) where TField : IBqlField
		{
			if (parametersCount < 1) throw new IndexOutOfRangeException();

			Type likeParameter = typeof(Like<Required<TField>>);
			if (parametersCount == 1)
				return typeof(Where<,>).MakeGenericType(typeof(TField), likeParameter);
			
			Type binaryChain = typeof(Or<,>).MakeGenericType(typeof(TField), likeParameter);
			for (int i = 0; i < parametersCount - 2; i++)
				binaryChain = typeof(Or<,,>).MakeGenericType(typeof(TField), likeParameter, binaryChain);

			return typeof(Where<,,>).MakeGenericType(typeof(TField), likeParameter, binaryChain);
		}

		private BqlCommand JoinItemSiteSettings<TSiteIdField, TInventoryIdField>(BqlCommand cmd)
			where TSiteIdField : IBqlOperand
			where TInventoryIdField : IBqlField
		{
			if (cmd.GetTables().Contains(typeof(INItemSiteSettings)))
				return cmd;

			return BqlCommand.AppendJoin<
				InnerJoin<INItemSiteSettings,
					On<INItemSiteSettings.inventoryID, Equal<TInventoryIdField>,
						And<INItemSiteSettings.siteID, Equal<TSiteIdField>>>>>(cmd);
		}

		private BqlCommand JoinLotSerial(BqlCommand cmd)
		{
			if (!cmd.GetTables().Contains(typeof(INLotSerClass)) && cmd.GetTables().Contains(typeof(InventoryItem)))
				cmd = BqlCommand.AppendJoin<LeftJoin<INLotSerClass,
											On<InventoryItem.FK.LotSerClass>>>(cmd);

			if (!cmd.GetTables().Contains(typeof(INLotSerialStatus)))
				cmd = BqlCommand.AppendJoin<LeftJoin<INLotSerialStatus,
								On<INLotSerialStatus.inventoryID, Equal<INLocationStatus.inventoryID>,
									And<INLotSerClass.lotSerAssign, Equal<INLotSerAssign.whenReceived>,
									And<INLotSerialStatus.subItemID, Equal<INLocationStatus.subItemID>,
									And<INLotSerialStatus.siteID, Equal<INLocationStatus.siteID>,
									And<INLotSerialStatus.locationID, Equal<INLocationStatus.locationID>,
									And<INLotSerClass.lotSerTrack, NotEqual<INLotSerTrack.notNumbered>>>>>>>>>(cmd);
			return cmd;
		}

		private bool IsReadyToBeCounted<Cycle>(PXResult it)
			where Cycle : class, IBqlTable, new()
		{
			Cycle cycle = PXResult.Unwrap<Cycle>(it);
			LastPICountDate last = PXResult.Unwrap<LastPICountDate>(it);
			if (cycle == null || last == null)
				return false;

			DateTime lastCountDate = last.LastCountDate ?? DateTime.MinValue;
			DateTime countDate = this.Accessinfo.BusinessDate.GetValueOrDefault();
			short? CountsPerYear = (short?)this.Caches[typeof(Cycle)].GetValue(cycle, typeof(INMovementClass.countsPerYear).Name);

			if (CountsPerYear == null || CountsPerYear == 0)
				return false; //Skip Inventory Items with classes where CountsPerYear is not set.

			if (countDate.Year > lastCountDate.Year)
				return true;

			int countEachDays = (DateTime.IsLeapYear(countDate.Year) ? 366 : 365) / (int)CountsPerYear;
			return (countDate - lastCountDate).TotalDays > countEachDays;
		}

		#region Delegates
		protected virtual IEnumerable preliminaryResultRecs()
		{
			return CalcPIRows(false);
		}

		protected virtual IEnumerable inventoryItemsToLock()
		{
			PIGeneratorSettings gs = GeneratorSettings.Current;
			if (gs?.PIClassID == null)
				return new PXResultset<InventoryItem>();
			
			if (gs.Method == PIMethod.FullPhysicalInventory)
				return new PXResultset<InventoryItem>();

			var piTypeLocations = GetPiTypeLocations();
			var excludedInventoryIds = ExcludedInventoryItems.Select().RowCast<ExcludedInventoryItem>()
				.Where(i => i.InventoryID != null).Select(i => (int)i.InventoryID).ToHashSet();
			var excludedLocationIds = ExcludedLocations.Select().RowCast<ExcludedLocation>()
				.Where(i => i.LocationID != null).Select(i => (int)i.LocationID).ToHashSet();

			var intermediateResult = PrepareIntermediateResult(piTypeLocations, excludedInventoryIds, excludedLocationIds);
			return GetInventoryItemsToLock(intermediateResult, excludedInventoryIds);
		}

		protected virtual IEnumerable excludedInventoryItems()
		{
			return ExcludedInventoryItems.Cache.Updated;
		}

		protected virtual IEnumerable locationsToLock()
		{
			if (GeneratorSettings.Current?.PIClassID == null)
				return new PXResultset<INLocation>();

			var piTypeLocations = GetPiTypeLocations();

			if (piTypeLocations.Count == 0)
				return new PXResultset<INLocation>();

			var excludedLocationIds = ExcludedLocations.Select()
				.RowCast<ExcludedLocation>()
				.Select(l => l.LocationID)
				.ToHashSet();

			var result = new PXResultset<INLocation>();
			foreach (var resLocation in piTypeLocations)
			{
				INLocation loc = resLocation;
				if (excludedLocationIds.Contains(loc.LocationID))
					continue;

				result.Add(resLocation);
			}
			return result;
		}

		protected virtual IEnumerable excludedLocations()
		{
			return ExcludedLocations.Cache.Updated;
		}
		#endregion

		private PXResultset<INLocation> GetPiTypeLocations()
		{
			var query = new PXSelectReadonly2<INLocation,
				InnerJoin<INPIClassLocation, On<INLocation.locationID, Equal<IN.INPIClassLocation.locationID>>>,
				Where<INPIClassLocation.pIClassID, Equal<Current<PIGeneratorSettings.pIClassID>>>>(this);

			PXResultset<INLocation> result;
			using (new PXFieldScope(query.View, typeof(INLocation.locationID), typeof(INLocation.locationCD), typeof(INLocation.descr)))
			{
				result = query.Select();
			}

			return result;
		}

		private void SetRandomSeedIfZero()
		{
			PIGeneratorSettings gs = GeneratorSettings.Current;
			if ((gs != null) && (gs.RandomSeed == 0))
			{
				Random randObj = new Random(); // the same as Random(Environment.TickCount)
				gs.RandomSeed = randObj.Next();
			}
		}

		#region Events
		protected virtual void ExcludedInventoryItem_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			// We can't use PXFormula to get description. PXVirtualDAC view attribute blocks FieldUpdated events.
			var excludedItem = (ExcludedInventoryItem)e.NewRow;
			var filedValue = PXSelectorAttribute.GetField(
				sender,
				excludedItem,
				nameof(ExcludedInventoryItem.InventoryID),
				excludedItem.InventoryID,
				nameof(InventoryItem.descr));
			excludedItem.Descr = filedValue as string;// if field is empty it can be byte[].
		}

		protected virtual void ExcludedLocation_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			// We can't use PXFormula to get description. PXVirtualDAC view attribute blocks FieldUpdated events.
			var excludedLoc = (ExcludedLocation)e.NewRow;
			var filedValue = PXSelectorAttribute.GetField(
				sender,
				excludedLoc,
				nameof(ExcludedLocation.LocationID),
				excludedLoc.LocationID,
				nameof(INLocation.descr));
			excludedLoc.Descr = filedValue as string;// if field is empty it can be byte[].
			
		}

		protected virtual void PIGeneratorSettings_ByFrequency_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PIGeneratorSettings row = (PIGeneratorSettings)e.Row;
			if (row == null) { return; }
			if (row.ByFrequency == true)
			{
				row.ABCCodeID = null;
				row.CycleID = null;
				row.MovementClassID = null;
			}
		}

		protected virtual void PIGeneratorSettings_PIClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var settingsRow = (PIGeneratorSettings)e.Row;
			if (settingsRow == null)
				return;

			var piClass = (INPIClass)PXSelectorAttribute.Select<PIGeneratorSettings.pIClassID>(sender, settingsRow);
			if (piClass == null)
				return;

			PXCache source = this.Caches[typeof(INPIClass)];
			foreach (string fieldName in sender.Fields)
			{
				bool isClassIdField = fieldName.Equals(typeof(PIGeneratorSettings.pIClassID).Name, StringComparison.InvariantCultureIgnoreCase);
				if (!source.Fields.Contains(fieldName) || isClassIdField)
					continue;

				object value = source.GetValueExt(piClass, fieldName);
				value = (value is PXFieldState) ? ((PXFieldState)value).Value : value;

				if (value == null)
					continue;

				sender.SetValuePending(settingsRow, fieldName, value);
			}
		}

		protected virtual void PIGeneratorSettings_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var settingsRow = (PIGeneratorSettings)e.Row;
			if (settingsRow.PIClassID == null) // Cancel button was pressed
			{
				ClearExcludesCache<ExcludedInventoryItem>(ExcludedInventoryItems.Cache);
				ClearExcludesCache<ExcludedLocation>(ExcludedLocations.Cache);
			}
		}

		protected virtual void PIGeneratorSettings_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var settingsRow = (PIGeneratorSettings)e.Row;
			var oldSettingsRow = (PIGeneratorSettings)e.OldRow;
			if (settingsRow.PIClassID != oldSettingsRow.PIClassID)
			{
				ClearExcludesCache<ExcludedInventoryItem>(ExcludedInventoryItems.Cache);
				ClearExcludesCache<ExcludedLocation>(ExcludedLocations.Cache);
			}
			else if (settingsRow.SiteID != oldSettingsRow.SiteID)
			{
				ClearExcludesCache<ExcludedLocation>(ExcludedLocations.Cache);
			}
		}

		protected virtual void ClearExcludesCache<T>(PXCache excludesCache)
			where T : class, IBqlTable, new()
		{
			// We can't use Cache.Clear() method as the cache is marked with [PXNotCleanable] attribute.
			var itemsToRemove = excludesCache.Cached.Cast<T>().ToList();
			foreach (var item in itemsToRemove)
			{
				excludesCache.Remove(item);
			}
		}

		protected virtual void PIGeneratorSettings_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            PIGeneratorSettings row = (PIGeneratorSettings)e.Row;
			this.GeneratePI.SetEnabled(row.Method != null && row.SiteID != null);

			INPIClass piClass = (INPIClass)PXSelectorAttribute.Select<PIGeneratorSettings.pIClassID>(sender, row);
			if (piClass != null)
			{
				PXCache source = this.Caches[typeof(INPIClass)];
				foreach (string field in sender.Fields)
				{
					if (string.Compare(field, typeof(PIGeneratorSettings.pIClassID).Name, true) == 0 ||
							string.Compare(field, typeof(PIGeneratorSettings.descr).Name, true) == 0 ||
							!source.Fields.Contains(field)) continue;

					object value = source.GetValue(piClass, field);
					INPIClass def = new INPIClass();
					object defValue;
					source.RaiseFieldDefaulting(field, def, out defValue);
					PXUIFieldAttribute.SetEnabled(sender, field, value == null || object.Equals(value, defValue));
				}

				PXUIFieldAttribute.SetWarning<PIGeneratorSettings.pIClassID>(sender, row, piClass.UnlockSiteOnCountingFinish == true
					? Messages.PIGenerationEarlyInventoryUnfreezeWarning
					: null);
			}
			PXUIFieldAttribute.SetVisible<PIGeneratorSettings.selectedMethod>(sender, row, row.Method == PIMethod.ByInventoryItemSelected);
			PXUIFieldAttribute.SetVisible<PIGeneratorSettings.randomItemsLimit>(sender, row,
																				row.Method == PIMethod.ByInventoryItemSelected &&
																				row.SelectedMethod == PIInventoryMethod.RandomlySelectedItems);
			PXUIFieldAttribute.SetVisible<PIGeneratorSettings.maxLastCountDate>(sender, row,
																	row.Method == PIMethod.ByInventoryItemSelected &&
																	row.SelectedMethod == PIInventoryMethod.LastCountDate);

			PXUIFieldAttribute.SetVisible<PIGeneratorSettings.aBCCodeID>(sender, row, row.Method == PIMethod.ByABCClass);
			PXUIFieldAttribute.SetVisible<PIGeneratorSettings.movementClassID>(sender, row, row.Method == PIMethod.ByMovementClass);
			PXUIFieldAttribute.SetVisible<PIGeneratorSettings.cycleID>(sender, row, row.Method == PIMethod.ByCycle);
			PXUIFieldAttribute.SetVisible<PIGeneratorSettings.byFrequency>(sender, row,
						row.Method == PIMethod.ByABCClass ||
						row.Method == PIMethod.ByMovementClass ||
						row.Method == PIMethod.ByCycle);

			PXUIFieldAttribute.SetEnabled<PIGeneratorSettings.byFrequency>(sender, row,
					piClass != null && piClass.CycleID == null && piClass.MovementClassID == null && piClass.ABCCodeID == null);

			if (piClass != null && piClass.CycleID == null && piClass.MovementClassID == null && piClass.ABCCodeID == null)
			{
				PXUIFieldAttribute.SetEnabled<PIGeneratorSettings.aBCCodeID>(sender, row, row.ByFrequency == false);
				PXUIFieldAttribute.SetEnabled<PIGeneratorSettings.movementClassID>(sender, row, row.ByFrequency == false);
				PXUIFieldAttribute.SetEnabled<PIGeneratorSettings.cycleID>(sender, row, row.ByFrequency == false);
			}
			PXUIFieldAttribute.SetEnabled<PIGeneratorSettings.method>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<PIGeneratorSettings.selectedMethod>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<PIGeneratorSettings.maxLastCountDate>(sender, row,
					row.LastCountPeriod == null || row.LastCountPeriod == 0);

			if (row.LastCountPeriod != null && row.LastCountPeriod != 0)
			{
				row.MaxLastCountDate = this.Accessinfo.BusinessDate.Value.AddDays(-row.LastCountPeriod.GetValueOrDefault());
			}

			ExcludedLocations.AllowInsert = piClass != null && row.SiteID != null;
			ExcludedLocations.AllowDelete = piClass != null && row.SiteID != null;
			ExcludedLocations.AllowUpdate = piClass != null && row.SiteID != null;
			ExcludedInventoryItems.AllowInsert = piClass != null;
			ExcludedInventoryItems.AllowDelete = piClass != null;
			ExcludedInventoryItems.AllowUpdate = piClass != null;

			PXUIFieldAttribute.SetVisible<PIPreliminaryResult.tagNumber>(PreliminaryResultRecs.Cache, null, insetup.Current.PIUseTags == true);
		}

		protected virtual void INSetup_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			INSetup row = (INSetup)e.Row;
			if (row == null) return;
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

			PXDefaultAttribute.SetPersistingCheck<INSetup.iNTransitAcctID>(cache, e.Row, PXAccess.FeatureInstalled<FeaturesSet.warehouse>() ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<INSetup.iNTransitSubID>(cache, e.Row, PXAccess.FeatureInstalled<FeaturesSet.warehouse>() ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
		}
		#endregion
	}


	#region # Assignment Order

	public class PINumberAssignmentOrder
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(EmptySort, "-"),
					Pair(ByLocationID, Messages.ByLocationID),
					Pair(ByInventoryID, Messages.ByInventoryID),
					Pair(BySubItem, Messages.BySubItem),
					Pair(ByLotSerial, Messages.ByLotSerial),
					Pair(ByInventoryDescription, Messages.ByInventoryDescription),
				}) {}
		}

		public const string EmptySort = "ES";
		public const string ByLocationID = "LI";
		public const string ByInventoryID = "II";
		public const string BySubItem = "SI";
		public const string ByLotSerial = "LS";
		public const string ByInventoryDescription = "ID";

		public class emptySort : PX.Data.BQL.BqlString.Constant<emptySort>
		{
			public emptySort() : base(EmptySort) { }
		}

		public class byLocationID : PX.Data.BQL.BqlString.Constant<byLocationID>
		{
			public byLocationID() : base(ByLocationID) { }
		}

		public class byInventoryID : PX.Data.BQL.BqlString.Constant<byInventoryID>
		{
			public byInventoryID() : base(ByInventoryID) { }
		}

		public class bySubItem : PX.Data.BQL.BqlString.Constant<bySubItem>
		{
			public bySubItem() : base(BySubItem) { }
		}
		public class byLotSerial : PX.Data.BQL.BqlString.Constant<byLotSerial>
		{
			public byLotSerial() : base(ByLotSerial) { }
		}

		public class byInventoryDescription : PX.Data.BQL.BqlString.Constant<byInventoryDescription>
		{
			public byInventoryDescription() : base(ByInventoryDescription) { }
		}
	}

	#endregion # Assignment Order

	public static class PIGenerationMethod
	{
		public const string FullPhysicalInventory = "FPI";
		public const string ByMovementClassCountFrequency = "MCF";
		public const string ByABCClassCountFrequency = "ACF";
		public const string ByCycleID = "BCI";
		public const string ByCycleCountFrequency = "CCF";
		public const string LastCountDate = "LCD";
		public const string ByPreviousPIID = "PPI";
		public const string ByItemClassID = "BIC";
		public const string ListOfItems = "LOI";
		public const string RandomlySelectedItems = "RSI";
		public const string ItemsHavingNegativeBookQty = "HNQ";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(FullPhysicalInventory, Messages.FullPhysicalInventory),
					Pair(ByCycleCountFrequency, Messages.ByCycleCountFrequency),
					Pair(ByMovementClassCountFrequency, Messages.ByMovementClassCountFrequency),
					Pair(ByABCClassCountFrequency, Messages.ByABCClassCountFrequency),
					Pair(ByCycleID, Messages.ByCycleID),
					Pair(LastCountDate, Messages.LastCountDate),
					Pair(ByPreviousPIID, Messages.ByPreviousPIID),
					Pair(ByItemClassID, Messages.ByItemClassID),
					Pair(ListOfItems, Messages.ListOfItems),
					Pair(RandomlySelectedItems, Messages.RandomlySelectedItems),
					Pair(ItemsHavingNegativeBookQty, Messages.ItemsHavingNegativeBookQty),
				}) {}
		}
	}

	#region Projections

	#region LastPICountDate projection
	// projection to calc last max. CountDate for site-inventory combination
	/*
		SELECT 
	 		h.SiteID,
	 		d.InventoryID, 
	 		MAX (h.PICountDate)
		FROM
					INPIHeader h
	 		JOIN	INPIDetail d ON d.PIID = h.PIID
		WHERE
					h.Status = 'C' -- Completed
			AND		d.Status = 'E' -- Entered	
		GROUP BY 
	 		d.InventoryID, 
	 		h.SiteID
	  
	*/

	[PXProjection(typeof(Select5<INPIHeader,
		InnerJoin<INPIDetail,
			On<INPIDetail.FK.PIHeader>>,
		Where<INPIHeader.status, Equal<INPIHdrStatus.completed>,
			And<INPIDetail.status, Equal<INPIDetStatus.entered>>>,
		Aggregate<GroupBy<INPIHeader.siteID,
			GroupBy<INPIDetail.inventoryID,
			GroupBy<INPIDetail.subItemID,
			GroupBy<INPIDetail.locationID,
			Max<INPIHeader.countDate>>>>>>>))]
    [Serializable]
    [PXHidden]
	public partial class LastPICountDate : PX.Data.IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(BqlField = typeof(INPIHeader.siteID))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(BqlField = typeof(INPIDetail.inventoryID))]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<INPIDetail.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>))]
		[SubItem(typeof(INPIDetail.inventoryID), BqlField = typeof(INPIDetail.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(INPIHeader.siteID), Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(INPIDetail.locationID))]
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
		#region CountDate
		public abstract class lastCountDate : PX.Data.BQL.BqlDateTime.Field<lastCountDate> { }
		protected DateTime? _LastCountDate;
		[PXDBDate(BqlField = typeof(INPIHeader.countDate))]
		public virtual DateTime? LastCountDate
		{
			get
			{
				return this._LastCountDate;
			}
			set
			{
				this._LastCountDate = value;
			}
		}
		#endregion
	}

	#endregion

	#endregion Projections
}
