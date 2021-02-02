using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.CS
{
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(ReasonCodeMaint))]
	[PXCacheName(Messages.ReasonCode, PXDacType.Config, CacheGlobal = true)]
	public partial class ReasonCode : PX.Data.IBqlTable
	{
        #region Keys
        public class PK : PrimaryKeyOf<ReasonCode>.By<reasonCodeID>
        {
            public static ReasonCode Find(PXGraph graph, string reasonCodeID) => FindBy(graph, reasonCodeID);
        }
		public static class FK
		{
			public class Sub : GL.Sub.PK.ForeignKeyOf<ReasonCode>.By<subID> { }
			public class SalesSub : GL.Sub.PK.ForeignKeyOf<ReasonCode>.By<salesSubID> { }
		}
        #endregion
        #region ReasonCodeID
		public abstract class reasonCodeID : PX.Data.BQL.BqlString.Field<reasonCodeID>
		{
			public const int Length = 20;
		}
		protected String _ReasonCodeID;
		[PXDefault()]
		[PXDBString(reasonCodeID.Length, IsKey = true, InputMask = ">aaaaaaaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Reason Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID>))]
		[PXReferentialIntegrityCheck]
		public virtual String ReasonCodeID
		{
			get
			{
				return this._ReasonCodeID;
			}
			set
			{
				this._ReasonCodeID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBLocalizableString(60, IsUnicode = true)]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(DescriptionField = typeof(Account.description), Visibility = PXUIVisibility.SelectorVisible, AvoidControlAccounts = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<ReasonCode.accountID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(typeof(ReasonCode.accountID), DescriptionField = typeof(Sub.description), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<ReasonCode.subID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region Usage
		public abstract class usage : PX.Data.BQL.BqlString.Field<usage> { }
		protected String _Usage;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Usage", Visibility=PXUIVisibility.SelectorVisible)]
		[ReasonCodeUsages.List()]
		[PXDefault]
		public virtual String Usage
		{
			get
			{
				return this._Usage;
			}
			set
			{
				this._Usage = value;
			}
		}
		#endregion

		#region SubMask
		public abstract class subMask : PX.Data.BQL.BqlString.Field<subMask> { }
		protected String _SubMask;
		[PXDefault()]
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Combine Sub From Combined", Visible=false)]//required for Copy-Paste
		public virtual String SubMask
		{
			get
			{
				return this._SubMask;
			}
			set
			{
				this._SubMask = value;
			}
		}
		#endregion
		#region SubMaskInventory
		public abstract class subMaskInventory : PX.Data.BQL.BqlString.Field<subMaskInventory> { }
		[PXDefault()]
		[PXString(30, IsUnicode = true, InputMask = "")]
		[IN.ReasonCodeSubAccountMask(DisplayName = "Combine Sub from")]
		public virtual String SubMaskInventory
		{
			get
			{
				return this._SubMask;
			}
			set
			{
				this._SubMask = value;
			}
		}
		#endregion
		#region SubMaskFinance
		public abstract class subMaskFinance : PX.Data.BQL.BqlString.Field<subMaskFinance> { }
		[PXDefault()]
		[PXString(30, IsUnicode = true, InputMask = "")]
		[CS.ReasonCodeSubAccountMask(DisplayName = "Combine Sub from")]
		public virtual String SubMaskFinance
		{
			get
			{
				return this._SubMask;
			}
			set
			{
				this._SubMask = value;
			}
		}
		#endregion

		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;
		[Account(DisplayName = "Sales Account", DescriptionField = typeof(Account.description), Visibility = PXUIVisibility.SelectorVisible, AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<ReasonCode.salesAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;
		[SubAccount(typeof(ReasonCode.salesAcctID), DisplayName = "Sales Sub.", DescriptionField = typeof(Sub.description), Visibility = PXUIVisibility.SelectorVisible)]
		[PXForeignReference(typeof(Field<ReasonCode.salesSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
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

	public class ReasonCodeUsages
	{
		public const string Sales = "S";
		public const string CreditWriteOff = "C";
		public const string BalanceWriteOff = "B";
		public const string Issue = INDocType.Issue;
		public const string Receipt = INDocType.Receipt;
		public const string Transfer = INDocType.Transfer;
		public const string Adjustment = INDocType.Adjustment;
		public const string Disassembly = INDocType.Disassembly;
		public const string VendorReturn = "N";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Sales, Messages.Sales),
					Pair(CreditWriteOff, Messages.CreditWriteOff),
					Pair(BalanceWriteOff, Messages.BalanceWriteOff),
					Pair(Issue, IN.Messages.Issue),
					Pair(Receipt, IN.Messages.Receipt),
					Pair(Adjustment, IN.Messages.Adjustment),
					Pair(Transfer, IN.Messages.Transfer),
					Pair(Disassembly, IN.Messages.Disassembly),
					Pair(VendorReturn, Messages.VendorReturn)
				})
			{ }
		}

		public class sales : PX.Data.BQL.BqlString.Constant<sales>
		{
			public sales() : base(Sales) {}
		}

		public class creditWriteOff : PX.Data.BQL.BqlString.Constant<creditWriteOff>
		{
			public creditWriteOff() : base(CreditWriteOff) {}
		}

		public class balanceWriteOff : PX.Data.BQL.BqlString.Constant<balanceWriteOff>
		{
			public balanceWriteOff() : base(BalanceWriteOff) {}
		}

		public class issue : PX.Data.BQL.BqlString.Constant<issue>
		{
			public issue() : base(Issue) {}
		}

		public class receipt : PX.Data.BQL.BqlString.Constant<receipt>
		{
			public receipt() : base(Receipt) {}
		}

		public class transfer : PX.Data.BQL.BqlString.Constant<transfer>
		{
			public transfer() : base(Transfer) {}
		}

		public class adjustment : PX.Data.BQL.BqlString.Constant<adjustment>
		{
			public adjustment() : base(Adjustment) {}
		}

		public class disassembly : PX.Data.BQL.BqlString.Constant<disassembly>
		{
			public disassembly() : base(Disassembly) {}
		}

		public class vendorReturn : PX.Data.BQL.BqlString.Constant<vendorReturn>
		{
			public vendorReturn() : base(VendorReturn) { }
		}
	}
}
