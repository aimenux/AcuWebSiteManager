using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(INUnitMaint))]
    [PXCacheName(Messages.InventoryUnitConversions, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class INUnit : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INUnit>.By<recordID>
		{
			public static INUnit Find(PXGraph graph, long? recordID) => FindBy(graph, recordID);
		}
		public abstract class UK
		{
			public class ByGlobal : PrimaryKeyOf<INUnit>.By<unitType, fromUnit, toUnit>
			{
				public static INUnit Find(PXGraph graph, string fromUnit, string toUnit) => FindBy(graph, INUnitType.Global, fromUnit, toUnit);

				internal static INUnit FindDirty(PXGraph graph, string fromUnit, string toUnit)
				{
					return PXSelect<INUnit,
						Where<INUnit.unitType, Equal<INUnitType.global>,
							And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>,
							And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>>>.SelectWindowed(graph, 0, 1, fromUnit, toUnit);
				}
			}
			public class ByInventory : PrimaryKeyOf<INUnit>.By<unitType, inventoryID, fromUnit>
			{
				public static INUnit Find(PXGraph graph, int? inventoryID, string fromUnit) => FindBy(graph, INUnitType.InventoryItem, inventoryID, fromUnit);

				public static INUnit FindDirty(PXGraph graph, int? inventoryID, string fromUnit)
				{
					return PXSelect<INUnit,
						Where<INUnit.unitType, Equal<INUnitType.inventoryItem>,
							And<INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>,
							And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>>>.Select(graph, inventoryID, fromUnit);
				}
			}
			public class ByItemClass : PrimaryKeyOf<INUnit>.By<unitType, itemClassID, fromUnit>
			{
				public static INUnit Find(PXGraph graph, int? itemClassID, string fromUnit) => FindBy(graph, INUnitType.ItemClass, itemClassID, fromUnit);

				internal static INUnit FindDirty(PXGraph graph, int? itemClassID, string fromUnit)
				{
					return PXSelect<INUnit,
						Where<INUnit.unitType, Equal<INUnitType.itemClass>,
							And<INUnit.itemClassID, Equal<Required<INUnit.itemClassID>>,
							And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>>>.SelectWindowed(graph, 0, 1, itemClassID, fromUnit);
				}
			}
		}

		public static class FK
		{
			public class ItemClass : INItemClass.PK.ForeignKeyOf<INUnit>.By<itemClassID> { }
			public class Inventory : InventoryItem.PK.ForeignKeyOf<INUnit>.By<inventoryID> { }
		}
        #endregion
        #region UnitType
		public abstract class unitType : PX.Data.BQL.BqlShort.Field<unitType> { }
		protected Int16? _UnitType;
		[PXDBShort(IsKey = true)]
		[PXDefault((short)3)]
		[PXUIField(DisplayName="Unit Type", Visibility=PXUIVisibility.Invisible, Visible=false)]
		[PXIntList(new int[] {1,2,3}, new string[] {"Inventory Item","Item Class","Global"})]
		public virtual Int16? UnitType
		{
			get
			{
				return this._UnitType;
			}
			set
			{
				this._UnitType = value;
			}
		}
		#endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		protected int? _ItemClassID;
		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXParent(typeof(FK.ItemClass))]
		public virtual int? ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
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
		#region ToUnit
		public abstract class toUnit : PX.Data.BQL.BqlString.Field<toUnit> { }
		protected String _ToUnit;
		[PXDefault()]
		[INUnit(IsKey=true, DisplayName = "To Unit", Visibility = PXUIVisibility.Visible)]
		public virtual String ToUnit
		{
			get
			{
				return this._ToUnit;
			}
			set
			{
				this._ToUnit = value;
			}
		}
		#endregion
		#region SampleToUnit
		public abstract class sampleToUnit : PX.Data.BQL.BqlString.Field<sampleToUnit> { }
		protected String _SampleToUnit;
		[PXString(6, IsUnicode = true, InputMask=">aaaaaa")]
		[PXUIField(DisplayName="To Unit", Visible=false)]
		public virtual String SampleToUnit
		{
			[PXDependsOnFields(typeof(toUnit))]
			get
			{
				return this._SampleToUnit ?? this._ToUnit;
			}
			set
			{
				this._SampleToUnit = value;
			}
		}
		#endregion
		#region FromUnit
		public abstract class fromUnit : PX.Data.BQL.BqlString.Field<fromUnit> { }
		protected String _FromUnit;
		[PXDefault()]
		[INUnit(IsKey = true, DisplayName = "From Unit", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FromUnit
		{
			get
			{
				return this._FromUnit;
			}
			set
			{
				this._FromUnit = value;
			}
		}
		#endregion
		#region UnitMultDiv
		public abstract class unitMultDiv : PX.Data.BQL.BqlString.Field<unitMultDiv> { }
		protected String _UnitMultDiv;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(MultDiv.Multiply)]
		[PXUIField(DisplayName="Multiply/Divide", Visibility=PXUIVisibility.Visible)]
		[MultDiv.List()]
		public virtual String UnitMultDiv
		{
			get
			{
				return this._UnitMultDiv;
			}
			set
			{
				this._UnitMultDiv = value;
			}
		}
		#endregion
		#region UnitRate
		public abstract class unitRate : PX.Data.BQL.BqlDecimal.Field<unitRate> { }
		protected Decimal? _UnitRate;
		[PXDBDecimal(6, MinValue = 0d)]
		[PXDefault(TypeCode.Decimal,"1.0")]
		[PXUIField(DisplayName="Conversion Factor", Visibility=PXUIVisibility.Visible)]
        [PXFormula(typeof(Validate<INUnit.unitMultDiv>))]
		public virtual Decimal? UnitRate
		{
			get
			{
				return this._UnitRate;
			}
			set
			{
				this._UnitRate = value;
			}
		}
		#endregion
		#region PriceAdjustmentMultiplier
		public abstract class priceAdjustmentMultiplier : PX.Data.BQL.BqlDecimal.Field<priceAdjustmentMultiplier> { }
		protected Decimal? _PriceAdjustmentMultiplier;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "1.0")]
		[PXUIField(DisplayName = "Price Adjustment Multiplier", Visibility = PXUIVisibility.Visible)]
		[PXUIVerify(typeof(Where<priceAdjustmentMultiplier, Greater<decimal0>>), PXErrorLevel.Error, AP.Messages.ValueMustBeGreaterThanZero)]
		public virtual Decimal? PriceAdjustmentMultiplier
		{
			get
			{
				return this._PriceAdjustmentMultiplier;
			}
			set
			{
				this._PriceAdjustmentMultiplier = value;
			}
		}
		#endregion
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }
		protected long? _RecordID;
		[PXDBLongIdentity()]
		public virtual long? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}
