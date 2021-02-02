namespace PX.Objects.CT
{
	using System;
	using PX.Data;
	using PX.Objects.CR;

	[PXPrimaryGraph(typeof(ContractMaint))]
	[System.SerializableAttribute()]
	[PXCacheName(Messages.ContractSLAMapping)]
	public partial class ContractSLAMapping : PX.Data.IBqlTable
	{
		#region ContractSLAMappingID
		public abstract class contractSLAMappingID : PX.Data.BQL.BqlInt.Field<contractSLAMappingID> { }
		protected Int32? _ContractSLAMappingID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual Int32? ContractSLAMappingID
		{
			get
			{
				return this._ContractSLAMappingID;
			}
			set
			{
				this._ContractSLAMappingID = value;
			}
		}
		#endregion
		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		protected Int32? _ContractID;
		[PXDBInt()]
		[PXDBDefault(typeof(Contract.contractID))]
		[PXParent(typeof(Select<Contract, Where<Contract.contractID, Equal<Current<ContractSLAMapping.contractID>>>>))]
		public virtual Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion
		#region Severity
		public abstract class severity : PX.Data.BQL.BqlString.Field<severity> { }
		protected String _Severity;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("M")]
		[PXUIField(DisplayName = "Severity")]
		[PXStringList(new string[] { "H", "L", "M" },
			new string[] { "High", "Low", "Medium" },
			BqlField = typeof(CRCase.severity))]
		public virtual String Severity
		{
			get
			{
				return this._Severity;
			}
			set
			{
				this._Severity = value;
			}
		}
		#endregion
		#region Period
		public abstract class period : PX.Data.BQL.BqlInt.Field<period> { }
		protected Int32? _Period;
		[PXDBTimeSpanLong()]
		[PXUIField(DisplayName = "Terms")]
		public virtual Int32? Period
		{
			get
			{
				return this._Period;
			}
			set
			{
				this._Period = value;
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
