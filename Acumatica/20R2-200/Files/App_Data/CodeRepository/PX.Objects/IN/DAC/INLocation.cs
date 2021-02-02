using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INLocation, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class INLocation : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INLocation>.By<locationID>.Dirty
		{
			public static INLocation Find(PXGraph graph, int? locationID) => FindBy(graph, locationID, (locationID ?? 0) <= 0);
		}
		public static class FK
		{
			public class CostSite : INCostSite.PK.ForeignKeyOf<INLocation>.By<costSiteID> { }
			public class Site : INSite.PK.ForeignKeyOf<INLocation>.By<siteID> { }
			public class PrimaryInventoryItem : InventoryItem.PK.ForeignKeyOf<INLocation>.By<primaryItemID> { }
			public class PrimaryItemClass : INItemClass.PK.ForeignKeyOf<INLocation>.By<primaryItemClassID> { }
		}
        #endregion
        #region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[PXDBForeignIdentity(typeof(INCostSite))]
		[PXReferentialIntegrityCheck]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(IsKey = true, Visible = false)]
		[PXParent(typeof(FK.Site))]
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
		#region LocationCD
		public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
		protected String _LocationCD;
		[IN.LocationRaw(IsKey=true)]
		[PXDefault()]
		public virtual String LocationCD
		{
			get
			{
				return this._LocationCD;
			}
			set
			{
				this._LocationCD = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
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
		#region IsCosted
		public abstract class isCosted : PX.Data.BQL.BqlBool.Field<isCosted> { }
		protected Boolean? _IsCosted;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cost Separately")]
		public virtual Boolean? IsCosted
		{
			get
			{
				return this._IsCosted;
			}
			set
			{
				this._IsCosted = value;
			}
		}
		#endregion
		#region IsSorting
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Sort Location")]
		public virtual Boolean? IsSorting { get; set; }
		public abstract class isSorting : PX.Data.BQL.BqlBool.Field<isSorting> { }
		#endregion
		#region CostSiteID
		public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
		protected int? _CostSiteID;
		[PXDBCalced(typeof(Switch<Case<Where<INLocation.isCosted, Equal<boolTrue>>, INLocation.locationID>, INLocation.siteID>), typeof(int))]
		public virtual int? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region InclQtyAvail
		public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
		protected Boolean? _InclQtyAvail;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName="Include in Qty. Available")]
		[PXFormula(typeof(False.When<isSorting.IsEqual<True>>.Else<inclQtyAvail>))]
		[PXUIEnabled(typeof(isSorting.IsEqual<False>))]
		public virtual Boolean? InclQtyAvail
		{
			get
			{
				return this._InclQtyAvail;
			}
			set
			{
				this._InclQtyAvail = value;
			}
		}
		#endregion
		#region AssemblyValid
		public abstract class assemblyValid : PX.Data.BQL.BqlBool.Field<assemblyValid> { }
		protected Boolean? _AssemblyValid;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Assembly Allowed")]
		public virtual Boolean? AssemblyValid
		{
			get
			{
				return this._AssemblyValid;
			}
			set
			{
				this._AssemblyValid = value;
			}
		}
		#endregion
		#region PickPriority
		public abstract class pickPriority : PX.Data.BQL.BqlShort.Field<pickPriority> { }
		protected Int16? _PickPriority;
		[PXDBShort(MinValue = 0, MaxValue = 999)]
		[PXDefault((short)1)]
		[PXUIField(DisplayName = "Pick Priority")]
		public virtual Int16? PickPriority
		{
			get
			{
				return this._PickPriority;
			}
			set
			{
				this._PickPriority = value;
			}
		}
		#endregion
		#region PathPriority
		[PXDBInt(MinValue = 0)]
		[PXDefault(1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Path")]
		public virtual Int32? PathPriority { get; set; }
		public abstract class pathPriority : PX.Data.BQL.BqlInt.Field<pathPriority> { }
		#endregion
		#region SalesValid
		public abstract class salesValid : PX.Data.BQL.BqlBool.Field<salesValid> { }
		protected Boolean? _SalesValid;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Sales Allowed")]
		public virtual Boolean? SalesValid
		{
			get
			{
				return this._SalesValid;
			}
			set
			{
				this._SalesValid = value;
			}
		}
		#endregion
		#region ReceiptsValid
		public abstract class receiptsValid : PX.Data.BQL.BqlBool.Field<receiptsValid> { }
		protected Boolean? _ReceiptsValid;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Receipts Allowed")]
		public virtual Boolean? ReceiptsValid
		{
			get
			{
				return this._ReceiptsValid;
			}
			set
			{
				this._ReceiptsValid = value;
			}
		}
		#endregion
		#region TransfersValid
		public abstract class transfersValid : PX.Data.BQL.BqlBool.Field<transfersValid> { }
		protected Boolean? _TransfersValid;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Transfers Allowed")]
		public virtual Boolean? TransfersValid
		{
			get
			{
				return this._TransfersValid;
			}
			set
			{
				this._TransfersValid = value;
			}
		}
		#endregion
		#region PrimaryItemValid
		public abstract class primaryItemValid : PX.Data.BQL.BqlString.Field<primaryItemValid> { }
		protected String _PrimaryItemValid;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INPrimaryItemValid.PrimaryNothing)]
		[INPrimaryItemValid.List()]
		[PXUIField(DisplayName="Primary Item Validation")]
		public virtual String PrimaryItemValid
		{
			get
			{
				return this._PrimaryItemValid;
			}
			set
			{
				this._PrimaryItemValid = value;
			}
		}
		#endregion
        #region PrimaryItemID
		public abstract class primaryItemID : PX.Data.BQL.BqlInt.Field<primaryItemID> { }
		protected Int32? _PrimaryItemID;
		[StockItem(DisplayName="Primary Item")]
		[PXForeignReference(typeof(FK.PrimaryInventoryItem))]
		public virtual Int32? PrimaryItemID
		{
			get
			{
				return this._PrimaryItemID;
			}
			set
			{
				this._PrimaryItemID = value;
			}
		}
		#endregion
		#region PrimaryItemClassID
		public abstract class primaryItemClassID : PX.Data.BQL.BqlInt.Field<primaryItemClassID> { }
		protected int? _PrimaryItemClassID;
		[PXDBInt]
		[PXUIField(DisplayName = "Primary Item Class")]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
		public virtual int? PrimaryItemClassID
		{
			get
			{
				return this._PrimaryItemClassID;
			}
			set
			{
				this._PrimaryItemClassID = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PM.Project]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PM.ProjectTask(typeof(INLocation.projectID), AllowNull = true)]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected bool? _Active;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active
		{
			get
			{
				return _Active;
			}
			set
			{
				_Active = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
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
		public const string Main = "MAIN";
		public class main : PX.Data.BQL.BqlString.Constant<main>
		{
			public main()
				: base(Main)
			{
			}
		}
	}

	public class INPrimaryItemValid
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(PrimaryNothing, Messages.PrimaryNothing),
					Pair(PrimaryItemWarning, Messages.PrimaryItemWarning),
					Pair(PrimaryItemError, Messages.PrimaryItemError),
					Pair(PrimaryItemClassWarning, Messages.PrimaryItemClassWarning),
					Pair(PrimaryItemClassError, Messages.PrimaryItemClassError),
				}) {}
		}

		public const string PrimaryNothing = "N";
		public const string PrimaryItemError = "I";
		public const string PrimaryItemClassError = "C";
        public const string PrimaryItemWarning = "X";
        public const string PrimaryItemClassWarning = "Y";


		public class primaryNothing : PX.Data.BQL.BqlString.Constant<primaryNothing>
		{
			public primaryNothing() : base(PrimaryNothing) { ;}
		}

		public class primaryItem : PX.Data.BQL.BqlString.Constant<primaryItem>
		{
			public primaryItem() : base(PrimaryItemError) { ;}
		}

		public class primaryItemClass : PX.Data.BQL.BqlString.Constant<primaryItemClass>
		{
			public primaryItemClass() : base(PrimaryItemClassError) { ;}
		}

        public class primaryItemWarn : PX.Data.BQL.BqlString.Constant<primaryItemWarn>
		{
            public primaryItemWarn() : base(PrimaryItemWarning) { ;}
        }

        public class primaryItemClassWarn : PX.Data.BQL.BqlString.Constant<primaryItemClassWarn>
		{
            public primaryItemClassWarn() : base(PrimaryItemClassWarning) { ;}
        }
	}

    
}
