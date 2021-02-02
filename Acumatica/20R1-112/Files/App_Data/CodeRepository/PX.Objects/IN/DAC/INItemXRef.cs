using System.Linq;
using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.IN
{
	[Serializable]
    [PXCacheName(Messages.XReferences)]
	public partial class INItemXRef : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemXRef>.By<inventoryID, alternateType, bAccountID, alternateID, subItemID>
		{
			public static INItemXRef Find(PXGraph graph, int? inventoryID, string alternateType, int? bAccountID, string alternateID, int? subItemID)
				=> FindBy(graph, inventoryID, alternateType, bAccountID, alternateID, subItemID);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemXRef>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INItemXRef>.By<subItemID> { }
			public class BAccount : CR.BAccount.PK.ForeignKeyOf<INItemXRef>.By<bAccountID> { }
		}
        #endregion
        #region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(IsKey = true, DirtyRead = true)]
		[PXDBDefault(typeof(InventoryItem.inventoryID), DefaultForInsert=false, DefaultForUpdate = false)]
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
		[SubItem(typeof(INItemXRef.inventoryID), IsKey = true)]
		[PXDefault()]
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
		#region AlternateType
		public abstract class alternateType : PX.Data.BQL.BqlString.Field<alternateType> { }
		protected String _AlternateType;
		[PXDBString(4, IsKey = true)]
		[INAlternateType.List()]
		[PXDefault(INAlternateType.Global)]
		[PXUIField(DisplayName="Alternate Type")]
		public virtual String AlternateType
		{
			get
			{
				return this._AlternateType;
			}
			set
			{
				this._AlternateType = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDefault()]
		[PXRestrictor(typeof(Where<BAccountR.type, Equal<BAccountType.vendorType>, And<Optional<INItemXRef.alternateType>, Equal<INAlternateType.vPN>,
			Or<BAccountR.type, Equal<BAccountType.customerType>, And<Optional<INItemXRef.alternateType>, Equal<INAlternateType.cPN>,
			Or<BAccountR.type, Equal<BAccountType.combinedType>, And<Where<Optional<INItemXRef.alternateType>, Equal<INAlternateType.vPN>, Or<Optional<INItemXRef.alternateType>, Equal<INAlternateType.cPN>>>>>>>>>),
			Messages.DoesNotMatchWithAlternateType, typeof(BAccountR.acctCD), typeof(BAccountR.type))]
		[INItemXRefBAccount(DisplayName = "Vendor/Customer", IsKey = true)]
		[PXParent(typeof(FK.BAccount))]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PXDBString(50, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Alternate ID")]
		public virtual String AlternateID
		{
			get
			{
				return this._AlternateID;
			}
			set
			{
				this._AlternateID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;

		[INUnitXRef(typeof(inventoryID), DirtyRead = true)]
		public virtual String UOM
		{
			get { return this._UOM; }
			set { this._UOM = value; }
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

    [PXHidden]
	[PXProjection(typeof(Select<INItemXRef, Where<INItemXRef.alternateType, In3<INAlternateType.global, INAlternateType.vPN, INAlternateType.cPN>>>))]
	public class INItemPartNumber : INItemXRef
	{		
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region AlternateType
		public new abstract class alternateType : PX.Data.BQL.BqlString.Field<alternateType> { }
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion
		#region AlternateID
		public new abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		#endregion
		#region UOM
		public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		#endregion
		#region Descr
		public new abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion
	}

	[PXHidden]
	[PXBreakInheritance]
	public class INStockItemXRef : INItemXRef
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region AlternateType
		public new abstract class alternateType : PX.Data.BQL.BqlString.Field<alternateType> { }
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion
		#region AlternateID
		public new abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		#endregion
	}

	[PXHidden]
	[PXBreakInheritance]
	public class INNonStockItemXRef : INItemXRef
	{
		[PXDefault, SubItem(typeof(inventoryID), IsKey = true, Disabled = true)]
		public override Int32? SubItemID { get; set; }
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region AlternateType
		public new abstract class alternateType : PX.Data.BQL.BqlString.Field<alternateType> { }
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion
		#region AlternateID
		public new abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		#endregion
	}

	public class INUnitXRefAttribute : INUnitAttribute
	{
		public INUnitXRefAttribute(Type inventoryIDField) : base(inventoryIDField) {}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			string uom = (string)e.NewValue;
			if (string.IsNullOrEmpty(uom)) return;
			if (sender.Graph.Caches[typeof(INUnit)].Cached.Cast<INUnit>().Any(u => Equals(u.FromUnit, uom))) return;
			if (PXSelectReadonly<INUnit, Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>.SelectWindowed(sender.Graph, 0, 1, uom).Count > 0) return;

			base.FieldVerifying(sender, e);
		}
	}

	public class INAlternateType
	{
		public class List : PXStringListAttribute
		{
			public List() : base(
				new[]
			{ 
					Pair(CPN, Messages.CPN),
					Pair(VPN, Messages.VPN),
					Pair(Global, Messages.Global),
					Pair(Barcode, Messages.Barcode),
				}) {}
		}

		public const string CPN = "0CPN";
		public const string VPN = "0VPN";
		public const string MFPN = "MFPN";
		public const string Global = "GLBL";
		public const string Substitute = "SBST";
		public const string Obsolete = "OBSL";
		public const string SKU = "SKU";
		public const string UPC = "UPC";
		public const string EAN = "EAN";
		public const string ISBN = "ISBN";
		public const string GTIN = "GTIN";
		public const string Barcode = "BAR";

		public class cPN : PX.Data.BQL.BqlString.Constant<cPN>
		{
			public cPN() : base(CPN) { ;}
		}

		public class vPN : PX.Data.BQL.BqlString.Constant<vPN>
		{
			public vPN() : base(VPN) { ;}
		}

		public class global : PX.Data.BQL.BqlString.Constant<global>
		{
			public global() : base(Global) { ; }
		}

		public class obsolete : PX.Data.BQL.BqlString.Constant<obsolete>
		{
			public obsolete() : base(Obsolete) { ; }
		}
		public class barcode : PX.Data.BQL.BqlString.Constant<barcode>
		{
			public barcode() : base(Barcode) { ; }
		}
		public static string ConvertFromPrimary(INPrimaryAlternateType aPrimaryType)
		{
			return aPrimaryType == INPrimaryAlternateType.VPN ? INAlternateType.VPN : INAlternateType.CPN; 
		}

		public static INPrimaryAlternateType? ConvertToPrimary( string aAlternateType)
		{
			INPrimaryAlternateType? result = null;
			switch (aAlternateType)
			{
				case INAlternateType.VPN: 
					result =  INPrimaryAlternateType.VPN; break;
				case INAlternateType.CPN:
					result =  INPrimaryAlternateType.CPN; break;				
			}
			return result;
		}

	}
}
