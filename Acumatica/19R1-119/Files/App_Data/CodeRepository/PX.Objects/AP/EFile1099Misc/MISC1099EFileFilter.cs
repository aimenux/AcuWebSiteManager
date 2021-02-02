using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;

namespace PX.Objects.AP
{
	[Serializable]
	public class MISC1099EFileFilter : IBqlTable
	{
		#region OrganizationID

		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(DisplayName = "Transmitter")]
		[PXRestrictor(typeof(Where<Organization.reporting1099, Equal<True>>), Messages.EFilingIsAvailableOnlyCompaniesWithEnabled1099)]
		public virtual int? OrganizationID { get; set; }

		#endregion

		#region FinYear

		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		protected String _FinYear;
		[PXDBString(4, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "1099 Year", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<AP1099Year.finYear,
			Where<AP1099Year.organizationID, Equal<Optional<MISC1099EFileFilter.organizationID>>>>))]
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
		#region Include
		public abstract class include : PX.Data.BQL.BqlString.Field<include>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { TransmitterOnly, AllMarkedOrganizations },
					new string[] { Messages.TransmitterOnly, Messages.AllMarkedOrganizations })
				{ }
			}
			public const string TransmitterOnly = "T";
			public const string AllMarkedOrganizations = "A";

			public class transmitterOnly : PX.Data.BQL.BqlString.Constant<transmitterOnly>
			{
				public transmitterOnly() : base(TransmitterOnly) { }
			}

			public class allMarkedOrganizations : PX.Data.BQL.BqlString.Constant<allMarkedOrganizations>
			{
				public allMarkedOrganizations() : base(AllMarkedOrganizations) { }
			}
		}
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Prepare for")]
		[include.List]
		[PXDefault(include.TransmitterOnly)]
		public virtual string Include { get; set; }
		#endregion
		#region Box7
		public abstract class box7 : PX.Data.BQL.BqlString.Field<box7>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Box7All, Box7Equal, Box7NotEqual },
					new string[] { Messages.Box7All, Messages.Box7Equal, Messages.Box7NotEqual }) { }
			}

			public const string Box7All = "AL";
			public const string Box7Equal = "EQ";
			public const string Box7NotEqual = "NE";

			public class box7All : PX.Data.BQL.BqlString.Constant<box7All>
			{
				public box7All() : base(Box7All) { }
			}

			public class box7Equal : PX.Data.BQL.BqlString.Constant<box7Equal>
			{
				public box7Equal() : base(Box7Equal) { }
			}

			public class box7NotEqual : PX.Data.BQL.BqlString.Constant<box7NotEqual>
			{
				public box7NotEqual() : base(Box7NotEqual) { }
			}

			public class box7Nbr : PX.Data.BQL.BqlShort.Constant<box7Nbr>
			{
				public box7Nbr() : base(7) { }
			}
		}

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "NEC (Box 7)")]
		[box7.List]
		[PXDefault(box7.Box7All)]
		public virtual string Box7 { get; set; }
		#endregion

		#region IsPriorYear

		public abstract class isPriorYear : PX.Data.BQL.BqlBool.Field<isPriorYear> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Prior Year")]
		public virtual bool? IsPriorYear { get; set; }

		#endregion

		#region IsCorrectionReturn

		public abstract class isCorrectionReturn : PX.Data.BQL.BqlBool.Field<isCorrectionReturn> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Correction File")]
		public virtual bool? IsCorrectionReturn { get; set; }

		#endregion

		#region IsLastFiling

		public abstract class isLastFiling : PX.Data.BQL.BqlBool.Field<isLastFiling> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Last Filing")]
		public virtual bool? IsLastFiling { get; set; }

		#endregion

		#region ReportingDirectSalesOnly

		public abstract class reportingDirectSalesOnly : PX.Data.BQL.BqlBool.Field<reportingDirectSalesOnly> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Direct Sales only")]
		public virtual bool? ReportingDirectSalesOnly { get; set; }

		#endregion

		#region IsTestMode

		public abstract class isTestMode : PX.Data.BQL.BqlBool.Field<isTestMode> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Test File")]
		public virtual bool? IsTestMode { get; set; }

		#endregion
	}
}