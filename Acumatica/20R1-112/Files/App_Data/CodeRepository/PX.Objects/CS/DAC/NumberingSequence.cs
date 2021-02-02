namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.NumberingSequence)]
	public partial class NumberingSequence : PX.Data.IBqlTable
	{
		#region NumberingID
		public abstract class numberingID : PX.Data.BQL.BqlString.Field<numberingID> { }
		protected String _NumberingID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Numbering ID", Visibility = PXUIVisibility.SelectorVisible, Visible=false, Enabled=false)]
		[PXDefault(typeof(Numbering.numberingID))]
		[PXParent(typeof(Select<Numbering, Where<Numbering.numberingID, Equal<Current<NumberingSequence.numberingID>>>>))]
		public virtual String NumberingID
		{
			get
			{
				return this._NumberingID;
			}
			set
			{
				this._NumberingID = value;
			}
		}
		#endregion
		#region NumberingSEQ
		public abstract class numberingSEQ : PX.Data.BQL.BqlInt.Field<numberingSEQ> { }
		protected Int32? _NumberingSEQ;
		[PXDBIdentity(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Numbering Seq", Visible = false, Enabled = false)]
		public virtual Int32? NumberingSEQ
		{
			get
			{
				return this._NumberingSEQ;
			}
			set
			{
				this._NumberingSEQ = value;
			}
		}
		#endregion
		#region NBranchID
		public abstract class nBranchID : PX.Data.BQL.BqlInt.Field<nBranchID> { }
		protected Int32? _NBranchID;
		[GL.Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? NBranchID
		{
			get
			{
				return this._NBranchID;
			}
			set
			{
				this._NBranchID = value;
			}
		}
		#endregion
		#region StartNbr
		public abstract class startNbr : PX.Data.BQL.BqlString.Field<startNbr> { }
		protected String _StartNbr;
		[PXDefault()]
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Start Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String StartNbr
		{
			get
			{
				return this._StartNbr;
			}
			set
			{
				this._StartNbr = value;
			}
		}
		#endregion
		#region EndNbr
		public abstract class endNbr : PX.Data.BQL.BqlString.Field<endNbr> { }
		protected String _EndNbr;
		[PXDefault()]
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "End Number", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual String EndNbr
		{
			get
			{
				return this._EndNbr;
			}
			set
			{
				this._EndNbr = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDefault()]
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion 
		#region LastNbr
		public abstract class lastNbr : PX.Data.BQL.BqlString.Field<lastNbr> { }
		protected String _LastNbr;
		[PXDefault()]
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Last Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String LastNbr
		{
			get
			{
				return this._LastNbr;
			}
			set
			{
				this._LastNbr = value;
			}
		}
		#endregion
		#region WarnNbr
		public abstract class warnNbr : PX.Data.BQL.BqlString.Field<warnNbr> { }
		protected String _WarnNbr;
		[PXDefault()]
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Warning Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String WarnNbr
		{
			get
			{
				return this._WarnNbr;
			}
			set
			{
				this._WarnNbr = value;
			}
		}
		#endregion
		#region NbrStep
		public abstract class nbrStep : PX.Data.BQL.BqlInt.Field<nbrStep> { }
		protected Int32? _NbrStep;
		[PXDBInt()]
		[PXUIField(DisplayName = "Numbering Step", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(1)]
		public virtual Int32? NbrStep
		{
			get
			{
				return this._NbrStep;
			}
			set
			{
				this._NbrStep = value;
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
	}
}
