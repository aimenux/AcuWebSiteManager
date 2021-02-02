namespace PX.Objects.CM
{
	using System;
	using PX.Data;
    using PX.Objects.GL;
	
	/// <summary>
	/// Represents details of the currency translation definitions.
	/// Each record of this type specifies the way translation is performed for a particular range of accounts and subaccounts
	/// (in the scope of the corresponding master record of the <see cref="TranslDef"/> type).
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TranslDefDet)]
	public partial class TranslDefDet : PX.Data.IBqlTable
	{
		#region TranslDefId
		public abstract class translDefId : PX.Data.BQL.BqlString.Field<translDefId> { }
		protected String _TranslDefId;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(TranslDef))]
		[PXParent(typeof(Select<TranslDef, Where<TranslDef.translDefId, Equal<Current<TranslDefDet.translDefId>>>>))]
		public virtual String TranslDefId
		{
			get
			{
				return this._TranslDefId;
			}
			set
			{
				this._TranslDefId = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(TranslDef.lineCntr))]
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
		#region AccountIdFrom
		public abstract class accountIdFrom : PX.Data.BQL.BqlInt.Field<accountIdFrom> { }
		protected Int32? _AccountIdFrom;
		[Account(DisplayName = "Account From", DescriptionField = typeof(Account.description), Required = true)]
		public virtual Int32? AccountIdFrom
		{
			get
			{
				return this._AccountIdFrom;
			}
			set
			{
				this._AccountIdFrom = value;
			}
		}
		#endregion
		#region SubIdFrom
		public abstract class subIdFrom : PX.Data.BQL.BqlInt.Field<subIdFrom> { }
		protected Int32? _SubIdFrom;
		[SubAccount(typeof(TranslDefDet.accountIdFrom), DisplayName = "Subaccount From")]
		public virtual Int32? SubIdFrom
		{
			get
			{
				return this._SubIdFrom;
			}
			set
			{
				this._SubIdFrom = value;
			}
		}
		#endregion
		#region AccountIdTo
		public abstract class accountIdTo : PX.Data.BQL.BqlInt.Field<accountIdTo> { }
		protected Int32? _AccountIdTo;
		[Account(DisplayName = "Account To", DescriptionField = typeof(Account.description), Required = true)]
		public virtual Int32? AccountIdTo
		{
			get
			{
				return this._AccountIdTo;
			}
			set
			{
				this._AccountIdTo = value;
			}
		}
		#endregion
		#region SubIdTo
		public abstract class subIdTo : PX.Data.BQL.BqlInt.Field<subIdTo> { }
		protected Int32? _SubIdTo;
		[SubAccount(typeof(TranslDefDet.accountIdTo), DisplayName = "Subaccount To")]
		public virtual Int32? SubIdTo
		{
			get
			{
				return this._SubIdTo;
			}
			set
			{
				this._SubIdTo = value;
			}
		}
		#endregion
		#region CalcMode
		public abstract class calcMode : PX.Data.BQL.BqlShort.Field<calcMode> { }
		protected Int16? _CalcMode;
		[PXDBShort()]
		[PXUIField(DisplayName = "Translation Method", Required = true)]
		[PXDefault((short)1)]
		[PXIntList("1;YTD Balance,2;PTD Balance")]
		public virtual Int16? CalcMode
		{
			get
			{
				return this._CalcMode;
			}
			set
			{
				this._CalcMode = value;
			}
		}
		#endregion
		#region RateTypeId
		public abstract class rateTypeId : PX.Data.BQL.BqlString.Field<rateTypeId> { }
		protected String _RateTypeId;
		[PXDBString(6, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Rate Type", Required = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		public virtual String RateTypeId
		{
			get
			{
				return this._RateTypeId;
			}
			set
			{
				this._RateTypeId = value;
			}
		}
		#endregion
		#region NoteID
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			protected Guid? _NoteID;
			[PXNote()]
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
		
		public static string GetAcct(PXGraph graph, Int32 accId)
		{
			Account acct = (Account) PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(graph, accId);
			return acct.AccountCD;
		}
		public static string GetSub(PXGraph graph, Int32 subId)
		{
			Sub sub = (Sub) PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(graph, subId);
			return sub.SubCD;
		}
	}
}
