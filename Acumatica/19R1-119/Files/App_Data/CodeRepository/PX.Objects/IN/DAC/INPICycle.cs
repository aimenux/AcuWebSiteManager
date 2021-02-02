using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.TX;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INPICycle, PXDacType.Catalogue)]
	[PXPrimaryGraph(typeof(INPICycleMaint))]
	public partial class INPICycle : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INPICycle>.By<cycleID>
		{
			public static INPICycle Find(PXGraph graph, string cycleID) => FindBy(graph, cycleID);
		}
		#endregion
		#region CycleID
		public abstract class cycleID : PX.Data.BQL.BqlString.Field<cycleID> { }
		protected String _CycleID;
		[PXDefault()]
		[PXDBString(10, IsUnicode = true, IsKey = true /*, InputMask = ">LLLLLLLLLL" */ )]
		[PXUIField(DisplayName="Cycle ID", Visibility=PXUIVisibility.SelectorVisible )]
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

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDefault]
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
    #region CountsPerYear
    public abstract class countsPerYear : PX.Data.BQL.BqlShort.Field<countsPerYear> { }
    protected Int16? _CountsPerYear;
		[PXDefault((short)0)]
    [PXDBShort(MinValue = 0)]
    [PXUIField(DisplayName = "Counts Per Year", Visibility = PXUIVisibility.SelectorVisible)]
    public virtual Int16? CountsPerYear
    {
        get
        {
            return this._CountsPerYear;
        }
        set
        {
            this._CountsPerYear = value;
        }
    }
    #endregion
    #region MaxCountInaccuracyPct
        public abstract class maxCountInaccuracyPct : PX.Data.BQL.BqlDecimal.Field<maxCountInaccuracyPct> { }
        protected Decimal? _MaxCountInaccuracyPct;
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]  
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Max. Count Inaccuracy %")]
        public virtual Decimal? MaxCountInaccuracyPct
        {
            get
            {
                return this._MaxCountInaccuracyPct;
            }
            set
            {
                this._MaxCountInaccuracyPct = value;
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
