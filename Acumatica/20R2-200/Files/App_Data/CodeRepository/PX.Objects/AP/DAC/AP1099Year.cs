using PX.Objects.GL;
using PX.Objects.GL.Attributes;

namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.AP1099Year)]
	public partial class AP1099Year : PX.Data.IBqlTable
	{
        private class string0101 : PX.Data.BQL.BqlString.Constant<string0101>
		{
            public string0101()
                : base("0101")
            {
            }
        }
        private class string1231 : PX.Data.BQL.BqlString.Constant<string1231>
		{
            public string1231()
                : base("1231")
            {
            }
        }

		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[PXDefault]
		[Organization(false, IsKey = true)]
		public virtual int? OrganizationID { get; set; }
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		protected String _FinYear;
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName="1099 Year", Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<AP1099Year.finYear,
									Where<AP1099Year.organizationID, Equal<Current2<AP1099Year.organizationID>>>>))]
		public virtual String FinYear
		{
			get
			{
				return this._FinYear;
			}
			set
			{
				this._FinYear = value;
			}
		}
		#endregion
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlString.Field<startDate> { }
        protected String _StartDate;
        [PXDBCalced(typeof(Add<AP1099Year.finYear, string0101>), typeof(string))]
        public virtual String StartDate
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
        #region EndDate
        public abstract class endDate : PX.Data.BQL.BqlString.Field<endDate> { }
        protected String _EndDate;
        [PXDBCalced(typeof(Add<AP1099Year.finYear, string1231>), typeof(string))]
        public virtual String EndDate
        {
            get
            {
                return this._EndDate;
            }
            set
            {
                this._EndDate = value;
            }
        }
        #endregion
        #region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new string[] { Open, Closed },
						new string[] { "Open", "Closed" }) { }
			}

			public const string Open = "N";
			public const string Closed = "C";

			public class open : PX.Data.BQL.BqlString.Constant<open>
			{
				public open() : base(Open) { ;}
			}

			public class closed : PX.Data.BQL.BqlString.Constant<closed>
			{
				public closed() : base(Closed) { ;}
			}
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(status.Open)]
		[status.List()]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
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
