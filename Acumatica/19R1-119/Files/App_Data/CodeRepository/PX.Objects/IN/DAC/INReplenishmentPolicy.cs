using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[Serializable]
	[PXPrimaryGraph(typeof(INReplenishmentPolicyMaint))]
	[PXCacheName(Messages.ReplenishmentPolicy, PXDacType.Catalogue)]
	public partial class INReplenishmentPolicy : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INReplenishmentPolicy>.By<replenishmentPolicyID>
		{
			public static INReplenishmentPolicy Find(PXGraph graph, string replenishmentPolicyID) => FindBy(graph, replenishmentPolicyID);
		}
		#endregion
		#region ReplenishmentPolicyID
		public abstract class replenishmentPolicyID : PX.Data.BQL.BqlString.Field<replenishmentPolicyID> { }
		protected String _ReplenishmentPolicyID;
		[PXDefault()]
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Seasonality ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INReplenishmentPolicy.replenishmentPolicyID>))]
		[PX.Data.EP.PXFieldDescription]
		public virtual String ReplenishmentPolicyID
		{
			get
			{
				return this._ReplenishmentPolicyID;
			}
			set
			{
				this._ReplenishmentPolicyID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Required=true)]
		[PX.Data.EP.PXFieldDescription]
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
		#region CalendarID
		public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }
		protected String _CalendarID;
		[PXDBString(10, IsUnicode = true)]		
		[PXUIField(DisplayName = "Calendar", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual String CalendarID
		{
			get
			{
				return this._CalendarID;
			}
			set
			{
				this._CalendarID = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PX.Data.PXNote(DescriptionField = typeof(INReplenishmentPolicy.replenishmentPolicyID),
			Selector = typeof(Search<INReplenishmentPolicy.replenishmentPolicyID>), 
			FieldList = new [] { typeof(INReplenishmentPolicy.replenishmentPolicyID), typeof(INReplenishmentPolicy.descr) })]
		public virtual Guid? NoteID { get; set; }
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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

	public class INReplenishmentMethod
	{
		public const string None = "N";
		public const string MinMax = "M";
		public const string FixedReorder = "F";

		public class List : PXStringListAttribute
		{
			public List() : base(
				new[]
				{
					Pair(None, Messages.None),
					Pair(MinMax, Messages.MinMax),
					Pair(FixedReorder, Messages.FixedReorder),
				}) {}
		}

		public class none : PX.Data.BQL.BqlString.Constant<none>
		{
			public none() : base(None) { }
		}

		public class minMax : PX.Data.BQL.BqlString.Constant<minMax>
		{
			public minMax() : base(MinMax) { }
		}

		public class fixedReorder : PX.Data.BQL.BqlString.Constant<fixedReorder>
		{
			public fixedReorder() : base(FixedReorder) { }
		}
	}
	public class INReplenishmentSource
	{
		public const string None = "N";
		public const string Purchased = "P";
		public const string Manufactured = "M";
		public const string Transfer = "T";
		public const string DropShipToOrder = "D";
		public const string PurchaseToOrder = "O";
        public const string TransferToPurchase = "X";
		//public const string TransferToOrder = "R";
		public class List : PXStringListAttribute
		{
			public List() : base(
				new[]
				{
					Pair(None, Messages.None),
					Pair(Purchased, Messages.Purchased),
					Pair(Manufactured, Messages.Manufactured),
					Pair(Transfer, Messages.Transfer),
					Pair(DropShipToOrder, Messages.DropShip),
					Pair(PurchaseToOrder, Messages.PurchaseToOrder),
				}) {}
		}

		public class INPlanList : PXStringListAttribute
		{
			public INPlanList() : base(
				new[]
				{
                    Pair(None, Messages.None),
                    Pair(Purchased, Messages.Purchased),
					Pair(Transfer, Messages.Transfer),
				}) {}
		}

		public class SOList : PXStringListAttribute
		{
			public SOList() : base(
				new[]
				{
					Pair(DropShipToOrder, Messages.DropShipToOrder),
					Pair(PurchaseToOrder, Messages.PurchaseToOrder),
				}) {}
		}

		public class none : PX.Data.BQL.BqlString.Constant<none>
		{
			public none() : base(None) { }
		}
		public class purchased : PX.Data.BQL.BqlString.Constant<purchased>
		{
			public purchased() : base(Purchased){}
		}
		public class transfer : PX.Data.BQL.BqlString.Constant<transfer>
		{
			public transfer() : base(Transfer) { }
		}
		public class manufactured : PX.Data.BQL.BqlString.Constant<manufactured>
		{
			public manufactured() : base(Manufactured) { }
		}
		public class dropShipToOrder : PX.Data.BQL.BqlString.Constant<dropShipToOrder>
		{
			public dropShipToOrder() : base(DropShipToOrder) { }
		}
		public class purchaseToOrder : PX.Data.BQL.BqlString.Constant<purchaseToOrder>
		{
			public purchaseToOrder() : base(PurchaseToOrder) { }
		}
        public class transferToPurchase : PX.Data.BQL.BqlString.Constant<transferToPurchase>
		{
            public transferToPurchase() : base(TransferToPurchase) { }
        }
		public static bool IsTransfer(string value)
		{
            return value == Transfer || value == PurchaseToOrder || value == DropShipToOrder || value == Purchased;
		}
	}
}