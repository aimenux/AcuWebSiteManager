using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Linq;
using PX.Data;

namespace PX.Objects.IN
{
	#region DACs
	[PXCacheName(Messages.INStoragePlace, PXDacType.Catalogue)]
	[PXProjection(typeof(
		SelectFrom<INCostSite>
		.LeftJoin<INLocation>.On<INLocation.locationID.IsEqual<INCostSite.costSiteID>>
		.LeftJoin<INCart>.On<INCart.cartID.IsEqual<INCostSite.costSiteID>>
		.LeftJoin<INSite>.On<INLocation.siteID.IfNullThen<INCart.siteID>.IsEqual<INSite.siteID>>
		.Where<INLocation.locationID.IsNotNull.Or<INCart.cartID.IsNotNull>>
		), Persistent = false)]
	[PXPrimaryGraph(
		new[] { typeof(INSiteMaint) },
		new[] { typeof(SelectFrom<INSite>.Where<INSite.siteID.IsEqual<siteID.FromCurrent>>) })]
	public class StoragePlace : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<StoragePlace>.By<siteID, storageID>
		{
			public static StoragePlace Find(PXGraph graph, int? siteID, int? storageID) => FindBy(graph, siteID, storageID);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<StoragePlace>.By<siteID> { }
			public class Cart : INCart.PK.ForeignKeyOf<StoragePlace>.By<siteID, cartID> { }
			public class Location : INLocation.PK.ForeignKeyOf<StoragePlace>.By<locationID> { }
		}
		#endregion
		#region SiteID
		[PXDBInt(IsKey = true, BqlTable = typeof(INSite))]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region SiteCD
		[PXDBString(BqlTable = typeof(INSite))]
		[PXUIField(DisplayName = "Warehouse ID", Visibility = PXUIVisibility.SelectorVisible)]
		public string SiteCD { get; set; }
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		#endregion
		#region LocationID
		[Location(typeof(siteID), BqlField = typeof(INLocation.locationID), Enabled = false)]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region CartID
		[PXDBInt(BqlField = typeof(INCart.cartID))]
		[PXUIField(DisplayName = "Cart ID", IsReadOnly = true)]
		[PXSelector(typeof(SearchFor<INCart.cartID>.In<SelectFrom<INCart>.Where<INCart.active.IsEqual<True>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		public int? CartID { get; set; }
		public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
		#endregion
		#region StorageID
		[PXInt(IsKey = true)]
		[PXDBCalced(typeof(INLocation.locationID.IfNullThen<INCart.cartID>), typeof(int))]
		public virtual Int32? StorageID { get; set; }
		public abstract class storageID : PX.Data.BQL.BqlInt.Field<storageID> { }
		#endregion
		#region StorageCD
		[PXString]
		[PXUIField(DisplayName = "Storage ID", IsReadOnly = true)]
		[PXDBCalced(typeof(INLocation.locationCD.IfNullThen<INCart.cartCD>), typeof(string))]
		public virtual string StorageCD { get; set; }
		public abstract class storageCD : PX.Data.BQL.BqlString.Field<storageCD> { }
		#endregion
		#region Descr
		[PXString]
		[PXUIField(DisplayName = "Description", IsReadOnly = true)]
		[PXDBCalced(typeof(INLocation.descr.IfNullThen<INCart.descr>), typeof(string))]
		public virtual String Descr { get; set; }
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion
		#region IsCart
		[PXBool]
		[PXUIField(DisplayName = "Cart", IsReadOnly = true, FieldClass = "Carts")]
		[PXDBCalced(typeof(True.When<INCart.cartID.IsNotNull>.NoDefault.Else<False>), typeof(bool))]
		public virtual Boolean? IsCart { get; set; }
		public abstract class isCart : PX.Data.BQL.BqlBool.Field<isCart> { }
		#endregion
		#region Active
		[PXBool]
		[PXUIField(DisplayName = "Active", IsReadOnly = true)]
		[PXDBCalced(typeof(INLocation.active.IfNullThen<INCart.active>), typeof(bool))]
		public virtual Boolean? Active { get; set; }
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion
	}

	[PXCacheName(Messages.INStoragePlaceSplit, PXDacType.Balance)]
	[PXProjection(typeof(
		SelectFrom<INCostSite>
		.LeftJoin<INLocationStatus>.On<INLocationStatus.locationID.IsEqual<INCostSite.costSiteID>>
		.LeftJoin<INCartSplit>.On<INCartSplit.cartID.IsEqual<INCostSite.costSiteID>>
		.Where<INLocationStatus.locationID.IsNotNull.Or<INCartSplit.cartID.IsNotNull>>
		), Persistent = false)]
	public class StorageSplit : IBqlTable
	{
		#region SiteID
		[PXInt(IsKey = true)]
		[PXDBCalced(typeof(/*INLotSerialStatus.siteID.IfNullThen<*/INLocationStatus.siteID/*>*/.IfNullThen<INCartSplit.siteID>), typeof(int))]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region StorageID
		[PXInt(IsKey = true)]
		[PXDBCalced(typeof(/*INLotSerialStatus.locationID.IfNullThen<*/INLocationStatus.locationID/*>*/.IfNullThen<INCartSplit.cartID>), typeof(int))]
		public virtual Int32? StorageID { get; set; }
		public abstract class storageID : PX.Data.BQL.BqlInt.Field<storageID> { }
		#endregion
		#region IsCart
		[PXBool(IsKey = true)]
		[PXUIField(DisplayName = "Cart", IsReadOnly = true)]
		[PXDBCalced(typeof(True.When<INCartSplit.cartID.IsNotNull>.NoDefault.Else<False>), typeof(bool))]
		public virtual Boolean? IsCart { get; set; }
		public abstract class isCart : PX.Data.BQL.BqlBool.Field<isCart> { }
		#endregion

		#region InventoryID
		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Inventory ID", IsReadOnly = true)]
		[PXSelector(typeof(InventoryItem.inventoryID), SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
		[PXDBCalced(typeof(/*INLotSerialStatus.inventoryID.IfNullThen<*/INLocationStatus.inventoryID/*>*/.IfNullThen<INCartSplit.inventoryID>), typeof(int))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Subitem", FieldClass = SubItemAttribute.DimensionName, IsReadOnly = true)]
		[PXDBCalced(typeof(/*INLotSerialStatus.subItemID.IfNullThen<*/INLocationStatus.subItemID/*>*/.IfNullThen<INCartSplit.subItemID>), typeof(int))]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region LotSerialNbr
		//[PXString]
		//[PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial", IsReadOnly = true)]
		//[PXDBCalced(typeof(INLotSerialStatus.lotSerialNbr.IfNullThen<INCartSplit.lotSerialNbr>), typeof(string))]
		//public virtual String LotSerialNbr { get; set; }
		//public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region ExpireDate
		//[PXDate]
		//[PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial", IsReadOnly = true)]
		//[PXDBCalced(typeof(INLotSerialStatus.expireDate.IfNullThen<INCartSplit.expireDate>), typeof(DateTime))]
		//public virtual DateTime? ExpireDate { get; set; }
		//public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		#endregion
		#region Qty
		[PXDecimal]
		[PXUIField(DisplayName = "Qty. On Hand", IsReadOnly = true)]
		[PXDBCalced(typeof(/*INLotSerialStatus.qtyOnHand.IfNullThen<*/INLocationStatus.qtyOnHand/*>*/.IfNullThen<INCartSplit.baseQty>), typeof(decimal))]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
		#region IsOverall
		//[PXBool]
		//[PXDBCalced(typeof(True.When<INLocationStatus.siteID.IsNotNull>.NoDefault.Else<False>), typeof(bool))]
		//public virtual Boolean? IsOverall { get; set; }
		//public abstract class isOverall : PX.Data.BQL.BqlBool.Field<isOverall> { }
		#endregion
	}

	[PXCacheName(Messages.INStoragePlaceStatus, PXDacType.Balance)]
	[PXProjection(typeof(
		SelectFrom<StoragePlace>
		.InnerJoin<StorageSplit>.On<
			StorageSplit.siteID.IsEqual<StoragePlace.siteID>
			.And<StorageSplit.storageID.IsEqual<StoragePlace.storageID>>
			.And<StorageSplit.isCart.IsEqual<StoragePlace.isCart>>>
		.InnerJoin<InventoryItem>.On<InventoryItem.inventoryID.IsEqual<StorageSplit.inventoryID>>
		.InnerJoin<INLotSerClass>.On<InventoryItem.FK.LotSerClass>
		), Persistent = false)]
	public class StoragePlaceStatus : IBqlTable
	{
		#region SplittedIcon
		[PXImage]
		[PXUIField(DisplayName = "", IsReadOnly = true)]
		[PXFormula(typeof(Empty.When<isCart.IsEqual<True>>.Else<GL.SplitIcon.parent>))]
		public virtual string SplittedIcon { get; set; }
		public abstract class splittedIcon : PX.Data.BQL.BqlString.Field<splittedIcon> { }
		#endregion
		#region SiteID
		[PXDBInt(IsKey = true, BqlTable = typeof(StoragePlace))]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region SiteCD
		[PXDBString(BqlTable = typeof(StoragePlace))]
		[PXUIField(DisplayName = "Warehouse", FieldClass = SiteAttribute.DimensionName, IsReadOnly = false)]
		public string SiteCD { get; set; }
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		#endregion
		#region LocationID
		[Location(typeof(siteID), BqlTable = typeof(StoragePlace), Enabled = false)]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region CartID
		[PXDBInt(BqlTable = typeof(StoragePlace))]
		[PXUIField(DisplayName = "Cart ID", IsReadOnly = true)]
		[PXSelector(typeof(SearchFor<INCart.cartID>.In<SelectFrom<INCart>.Where<INCart.active.IsEqual<True>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		public int? CartID { get; set; }
		public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
		#endregion
		#region StorageID
		[PXDBInt(IsKey = true, BqlTable = typeof(StoragePlace))]
		public virtual Int32? StorageID { get; set; }
		public abstract class storageID : PX.Data.BQL.BqlInt.Field<storageID> { }
		#endregion
		#region StorageCD
		[PXDBString(BqlTable = typeof(StoragePlace))]
		[PXUIField(DisplayName = "Storage ID", IsReadOnly = true)]
		public string StorageCD { get; set; }
		public abstract class storageCD : PX.Data.BQL.BqlString.Field<storageCD> { }
		#endregion
		#region Descr
		[PXDBString(BqlTable = typeof(StoragePlace))]
		[PXUIField(DisplayName = "Storage Description", IsReadOnly = true)]
		public virtual String Descr { get; set; }
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion
		#region IsCart
		[PXDBBool(IsKey = true, BqlTable = typeof(StoragePlace))]
		[PXUIField(DisplayName = "Cart", IsReadOnly = true, FieldClass = "Carts")]
		public virtual Boolean? IsCart { get; set; }
		public abstract class isCart : PX.Data.BQL.BqlBool.Field<isCart> { }
		#endregion
		#region Active
		[PXDBBool(BqlTable = typeof(StoragePlace))]
		[PXUIField(DisplayName = "Active", IsReadOnly = true)]
		public virtual Boolean? Active { get; set; }
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion

		#region InventoryID
		[PXDBInt(IsKey = true, BqlTable = typeof(InventoryItem))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region InventoryCD
		[PXDBString(BqlTable = typeof(InventoryItem))]
		[PXUIField(DisplayName = "Inventory ID", IsReadOnly = true)]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		#endregion
		#region InventoryDescr
		[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Description", IsReadOnly = true)]
		public virtual String InventoryDescr { get; set; }
		public abstract class inventoryDescr : PX.Data.BQL.BqlString.Field<inventoryDescr> { }
		#endregion
		#region SubItemID
		[PXDBInt(IsKey = true, BqlTable = typeof(StorageSplit))]
		[PXUIField(DisplayName = "Subitem", FieldClass = SubItemAttribute.DimensionName, IsReadOnly = true)]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region LotSerialNbr
		[PXString]
		[PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial", IsReadOnly = true)]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region ExpireDate
		[PXDBDate]
		[PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial", IsReadOnly = true)]
		public virtual DateTime? ExpireDate { get; set; }
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		#endregion
		#region Qty
		[PXDBDecimal(BqlTable = typeof(StorageSplit))]
		[PXUIField(DisplayName = "Qty. On Hand", IsReadOnly = true)]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
		#region IsOverall
		//[PXDBBool(BqlTable = typeof(StorageSplit))]
		//[PXUIField(DisplayName = "Is Location Total", IsReadOnly = true)]
		//public virtual Boolean? IsOverall { get; set; }
		//public abstract class isOverall : PX.Data.BQL.BqlBool.Field<isOverall> { }
		#endregion
		#region BaseUnit
		[INUnit(DisplayName = "Base Unit", BqlTable = typeof(InventoryItem), Enabled = false)]
		public virtual String BaseUnit { get; set; }
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		#endregion
	}

	[PXCacheName(Messages.INStoragePlaceStatusExpanded, PXDacType.Balance)]
	[PXProjection(typeof(
		SelectFrom<INLocation>
		.InnerJoin<INSite>.On<INLocation.FK.Site>
		.InnerJoin<INLotSerialStatus>.On<INLotSerialStatus.FK.Location>
		.InnerJoin<InventoryItem>.On<INLotSerialStatus.FK.InventoryItem>
		.InnerJoin<INLotSerClass>.On<InventoryItem.FK.LotSerClass>
		), Persistent = false)]
	public class StoragePlaceStatusExpanded : IBqlTable
	{
		#region SplittedIcon
		[PXImage]
		[PXFormula(typeof(GL.SplitIcon.split))]
		public virtual string SplittedIcon { get; set; }
		public abstract class splittedIcon : PX.Data.BQL.BqlString.Field<splittedIcon> { }
		#endregion
		#region SiteID
		[PXDBInt(IsKey = true, BqlTable = typeof(INLocation))]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region SiteCD
		[PXDBString(BqlTable = typeof(INSite))]
		public string SiteCD { get; set; }
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		#endregion
		#region LocationID
		[Location(typeof(siteID), BqlTable = typeof(INLocation), Enabled = false)]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region StorageID
		[PXDBInt(IsKey = true, BqlField = typeof(INLocation.locationID))]
		public virtual Int32? StorageID { get; set; }
		public abstract class storageID : PX.Data.BQL.BqlInt.Field<storageID> { }
		#endregion
		#region StorageCD
		[PXDBString(BqlField = typeof(INLocation.locationCD))]
		public string StorageCD { get; set; }
		public abstract class storageCD : PX.Data.BQL.BqlString.Field<storageCD> { }
		#endregion
		#region Descr
		[PXDBString(BqlTable = typeof(INLocation))]
		public virtual String Descr { get; set; }
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion
		#region Active
		[PXDBBool(BqlTable = typeof(INLocation))]
		public virtual Boolean? Active { get; set; }
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion

		#region InventoryID
		[PXDBInt(BqlTable = typeof(InventoryItem))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region InventoryCD
		[PXDBString(BqlTable = typeof(InventoryItem))]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		#endregion
		#region SubItemID
		[PXDBInt(BqlTable = typeof(INLotSerialStatus))]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region LotSerialNbr
		[PXDBString(BqlTable = typeof(INLotSerialStatus))]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region ExpireDate
		[PXDBDate(BqlTable = typeof(INLotSerialStatus))]
		public virtual DateTime? ExpireDate { get; set; }
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		#endregion
		#region Qty
		[PXDBDecimal(BqlField = typeof(INLotSerialStatus.qtyOnHand))]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
		#region BaseUnit
		[INUnit(DisplayName = "Base Unit", BqlTable = typeof(InventoryItem), Enabled = false)]
		public virtual String BaseUnit { get; set; }
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		#endregion
	}
	#endregion

	public class StoragePlaceEnq : PXGraph<StoragePlaceEnq>
	{
		public PXFilter<StoragePlaceFilter> Filter;

		[PXFilterable]
		public
			SelectFrom<StoragePlaceStatus>
			.View storages;
		public virtual IEnumerable Storages()
		{
			var result = new PXDelegateResult();
			result.IsResultSorted = true;
			result.IsResultTruncated = Filter.Current.ExpandByLotSerialNbr == false;

			PXSelectBase<StoragePlaceStatus> byLocationSelect =
				new SelectFrom<StoragePlaceStatus>
				.Where<
					StoragePlaceFilter.siteID.FromCurrent.NoDefault.IsEqual<StoragePlaceStatus.siteID>
					.And<StoragePlaceStatus.qty.IsGreater<Zero>>
					.And<
						Not<FeatureInstalled<FeaturesSet.wMSCartTracking>>
						.Or<StoragePlaceFilter.showLocations.FromCurrent.NoDefault.IsEqual<True>>.And<StoragePlaceStatus.isCart.IsEqual<False>>
						.Or<StoragePlaceFilter.showCarts.FromCurrent.NoDefault.IsEqual<True>>.And<StoragePlaceStatus.isCart.IsEqual<True>>>
				>
				.AggregateTo<
					GroupBy<StoragePlaceStatus.siteCD>,
					GroupBy<StoragePlaceStatus.storageCD>,
					GroupBy<StoragePlaceStatus.isCart>,
					GroupBy<StoragePlaceStatus.active>,
					GroupBy<StoragePlaceStatus.inventoryCD>,
					GroupBy<StoragePlaceStatus.subItemID>,
					Sum<StoragePlaceStatus.qty>>
				.OrderBy<
					StoragePlaceStatus.isCart.Asc,
					StoragePlaceStatus.siteCD.Asc,
					StoragePlaceStatus.storageCD.Desc,
					StoragePlaceStatus.active.Desc,
					StoragePlaceStatus.inventoryCD.Asc,
					StoragePlaceStatus.subItemID.Asc,
					StoragePlaceStatus.qty.Desc>
				.View(this);
			if (Filter.Current.StorageID != null)
				byLocationSelect.WhereAnd<Where<StoragePlaceFilter.storageID.FromCurrent.IsEqual<StoragePlaceStatus.storageID>>>();
			if (Filter.Current.InventoryID != null)
				byLocationSelect.WhereAnd<Where<StoragePlaceFilter.inventoryID.FromCurrent.IsEqual<StoragePlaceStatus.inventoryID>>>();
			if (Filter.Current.SubItemID != null)
				byLocationSelect.WhereAnd<Where<StoragePlaceFilter.subItemID.FromCurrent.IsEqual<StoragePlaceStatus.subItemID>>>();

			int startRow = PXView.StartRow;
			int totalRows = 0;
			var byLocation = Filter.Current.ExpandByLotSerialNbr == true
				? byLocationSelect.SelectMain()
				: byLocationSelect.View.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows).RowCast<StoragePlaceStatus>().ToArray();

			if (byLocation.Length > 0 && Filter.Current.ExpandByLotSerialNbr == true)
			{
				PXSelectBase<StoragePlaceStatusExpanded> byLotSerialSelect =
					new SelectFrom<StoragePlaceStatusExpanded>
					.Where<
						StoragePlaceFilter.siteID.FromCurrent.NoDefault.IsEqual<StoragePlaceStatusExpanded.siteID>
						.And<StoragePlaceStatusExpanded.qty.IsGreater<Zero>>
					>
					.AggregateTo<
						GroupBy<StoragePlaceStatusExpanded.siteCD>,
						GroupBy<StoragePlaceStatusExpanded.storageCD>,
						GroupBy<StoragePlaceStatusExpanded.active>,
						GroupBy<StoragePlaceStatusExpanded.inventoryCD>,
						GroupBy<StoragePlaceStatusExpanded.subItemID>,
						GroupBy<StoragePlaceStatusExpanded.lotSerialNbr>,
						Sum<StoragePlaceStatusExpanded.qty>>
					.OrderBy<
						StoragePlaceStatusExpanded.siteCD.Asc,
						StoragePlaceStatusExpanded.storageCD.Desc,
						StoragePlaceStatusExpanded.active.Desc,
						StoragePlaceStatusExpanded.inventoryCD.Asc,
						StoragePlaceStatusExpanded.subItemID.Asc,
						StoragePlaceStatusExpanded.lotSerialNbr.Asc,
						StoragePlaceStatusExpanded.qty.Desc>
					.View(this);
				if (Filter.Current.StorageID != null)
					byLotSerialSelect.WhereAnd<Where<StoragePlaceFilter.storageID.FromCurrent.IsEqual<StoragePlaceStatusExpanded.storageID>>>();
				if (Filter.Current.InventoryID != null)
					byLotSerialSelect.WhereAnd<Where<StoragePlaceFilter.inventoryID.FromCurrent.IsEqual<StoragePlaceStatusExpanded.inventoryID>>>();
				if (Filter.Current.SubItemID != null)
					byLotSerialSelect.WhereAnd<Where<StoragePlaceFilter.subItemID.FromCurrent.IsEqual<StoragePlaceStatusExpanded.subItemID>>>();
				if (Filter.Current.LotSerialNbr != null)
					byLotSerialSelect.WhereAnd<Where<StoragePlaceFilter.lotSerialNbr.FromCurrent.IsEqual<StoragePlaceStatusExpanded.lotSerialNbr>>>();

				var byLotSerial =
					byLotSerialSelect
					.SelectMain()
					.Select(
						r => new StoragePlaceStatus
						{
							SplittedIcon = r.SplittedIcon,
							SiteID = r.SiteID,
							SiteCD = r.SiteCD,
							LocationID = r.LocationID,
							CartID = null,
							StorageID = r.StorageID,
							StorageCD = r.StorageCD,
							Descr = r.Descr,
							IsCart = false,
							Active = r.Active,
							InventoryID = r.InventoryID,
							InventoryCD = r.InventoryCD,
							SubItemID = r.SubItemID,
							LotSerialNbr = r.LotSerialNbr,
							ExpireDate = r.ExpireDate,
							Qty = r.Qty,
							BaseUnit = r.BaseUnit
						})
					.ToArray();

				if (byLotSerial.Length > 0)
				{
					int locationIdx = 1;
					int lotSerIdx = 0;
					StoragePlaceStatus current = byLocation[0];
					result.Add(current);
					while (locationIdx < byLocation.Length || lotSerIdx < byLotSerial.Length)
					{
						if (locationIdx >= byLocation.Length
							|| lotSerIdx < byLotSerial.Length
							&& current.SiteID == byLotSerial[lotSerIdx].SiteID
							&& current.StorageID == byLotSerial[lotSerIdx].StorageID
							&& current.InventoryID == byLotSerial[lotSerIdx].InventoryID
							&& current.SubItemID == byLotSerial[lotSerIdx].SubItemID)
						{
							result.Add(byLotSerial[lotSerIdx]);
							lotSerIdx++;
						}
						else
						{
							current = byLocation[locationIdx];
							result.Add(current);
							locationIdx++;
						}
					}
				}
				else
				{
					result.AddRange(byLocation);
				}
			}
			else
			{
				result.AddRange(byLocation);
			}

			PXView.StartRow = 0;
			return result;
		}

		protected virtual void _(Events.RowSelected<StoragePlaceFilter> e)
			=> storages.Cache.Adjust<PXUIFieldAttribute>()
				.For<StoragePlaceStatus.splittedIcon>(a => a.Visible = e.Row?.ExpandByLotSerialNbr ?? false)
				.SameFor<StoragePlaceStatus.lotSerialNbr>()
				.SameFor<StoragePlaceStatus.expireDate>();

		public override bool IsDirty => false;

		[PXHidden]
		public class StoragePlaceFilter : IBqlTable
		{
			#region SiteID
			[Site(Required = true)]
			public int? SiteID { get; set; }
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
			#region StorageID
			[PXInt]
			[PXUIField(DisplayName = "Storage ID")]
			[PXSelector(
				typeof(SearchFor<StoragePlace.storageID>.In<SelectFrom<StoragePlace>.Where<StoragePlace.active.IsEqual<True>.And<StoragePlace.siteID.IsEqual<siteID.FromCurrent>>>>),
				typeof(StoragePlace.siteCD), typeof(StoragePlace.storageCD), typeof(StoragePlace.isCart), typeof(StoragePlace.active),
				SubstituteKey = typeof(StoragePlace.storageCD),
				DescriptionField = typeof(StoragePlace.descr),
				ValidateValue = false)]
			[PXFormula(typeof(Default<siteID>))]
			public int? StorageID { get; set; }
			public abstract class storageID : PX.Data.BQL.BqlInt.Field<storageID> { }
			#endregion
			#region InventoryID
			[StockItem]
			public virtual Int32? InventoryID { get; set; }
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#endregion
			#region SubItemID
			[SubItem(typeof(inventoryID))]
			[PXFormula(typeof(Default<inventoryID>))]
			public virtual Int32? SubItemID { get; set; }
			public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			#endregion
			#region LotSerialNbr
			[LotSerialNbr]
			[PXFormula(typeof(Default<inventoryID>))]
			public virtual String LotSerialNbr { get; set; }
			public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
			#endregion
			#region ShowLocations
			[PXBool]
			[PXUnboundDefault(true)]
			[PXUIField(DisplayName = "Show Locations", FieldClass = "Carts")]
			public virtual Boolean? ShowLocations { get; set; }
			public abstract class showLocations : PX.Data.BQL.BqlBool.Field<showLocations> { }
			#endregion
			#region ShowCarts
			[PXBool]
			[PXUnboundDefault(typeof(FeatureInstalled<FeaturesSet.wMSCartTracking>))]
			[PXUIField(DisplayName = "Show Carts", FieldClass = "Carts")]
			public virtual Boolean? ShowCarts { get; set; }
			public abstract class showCarts : PX.Data.BQL.BqlBool.Field<showCarts> { }
			#endregion
			#region ExpandByLotSerialNbr
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Expand By Lot/Serial Number", Visibility = PXUIVisibility.Visible)]
			public virtual bool? ExpandByLotSerialNbr { get; set; }
			public abstract class expandByLotSerialNbr : PX.Data.BQL.BqlBool.Field<expandByLotSerialNbr> { }
			#endregion
		}
	}
}