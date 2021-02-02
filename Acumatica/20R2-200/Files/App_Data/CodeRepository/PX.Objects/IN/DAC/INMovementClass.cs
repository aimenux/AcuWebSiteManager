using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.TX;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INMovementClass, PXDacType.Catalogue)]
	[PXPrimaryGraph(typeof(INMovementClassMaint))]
	public partial class INMovementClass : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INMovementClass>.By<movementClassID>
		{
			public static INMovementClass Find(PXGraph graph, string movementClassID) => FindBy(graph, movementClassID);
		}
		#endregion
		#region MovementClassID
		public abstract class movementClassID : PX.Data.BQL.BqlString.Field<movementClassID> { }
		protected String _MovementClassID;
		[PXDefault()]
        [PXDBString(1, IsKey = true)]
		[PXUIField(DisplayName = "Movement Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String MovementClassID
		{
			get
			{
				return this._MovementClassID;
			}
			set
			{
				this._MovementClassID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
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
        #region MaxTurnoverPct
		public abstract class maxTurnoverPct : PX.Data.BQL.BqlDecimal.Field<maxTurnoverPct> { }
		protected Decimal? _MaxTurnoverPct;
        [PXDBDecimal(2, MinValue = 0, MaxValue = 1000000000)]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Max. Turnover %")]

		public virtual Decimal? MaxTurnoverPct
        {
            get
            {
				return this._MaxTurnoverPct;
            }
            set
            {
				this._MaxTurnoverPct = value;
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
