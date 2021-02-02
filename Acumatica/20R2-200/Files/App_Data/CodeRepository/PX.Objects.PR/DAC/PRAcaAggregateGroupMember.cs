using PX.Data;
using PX.Data.BQL.Fluent;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRAcaAggregateGroupMember)]
	public class PRAcaAggregateGroupMember : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Obsolete]
		[PXDBInt]
		public virtual int? BranchID { get; set; }
		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false)]
		[PXDBDefault(typeof(PRAcaCompanyYearlyInformation.orgBAccountID))]
		public virtual int? OrgBAccountID { get; set; }
		#endregion
		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		[PXDBString(4, IsKey = true)]
		[PXUIField(DisplayName = Messages.Year)]
		[PXParent(typeof(
			Select<PRAcaCompanyYearlyInformation,
				Where<PRAcaCompanyYearlyInformation.year, Equal<Current<PRAcaAggregateGroupMember.year>>,
				And<PRAcaCompanyYearlyInformation.orgBAccountID, Equal<Current<PRAcaAggregateGroupMember.orgBAccountID>>,
				And<PRAcaCompanyYearlyInformation.isPartOfAggregateGroup, Equal<True>>>>>))]
		[PXDBDefault(typeof(PRAcaCompanyYearlyInformation.year))]
		public virtual string Year { get; set; }
		#endregion
		#region MemberCompanyName
		public abstract class memberCompanyName : PX.Data.BQL.BqlString.Field<memberCompanyName> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Company Name", Required = true)]
		public virtual string MemberCompanyName { get; set; }
		#endregion
		#region MemberEin
		public abstract class memberEin : PX.Data.BQL.BqlString.Field<memberEin> { }
		[PXDBString(9, InputMask = "##-#######", IsKey = true)]
		[PXUIField(DisplayName = "Member EIN", Required = true)]
		public virtual string MemberEin { get; set; }
		#endregion
		#region HighestMonthlyFteNumber
		public abstract class highestMonthlyFteNumber : PX.Data.BQL.BqlInt.Field<highestMonthlyFteNumber> { }
		[PXDBInt(MinValue = 0)]
		[PXUIField(DisplayName = "Highest Monthly FTE Number")]
		public virtual int? HighestMonthlyFteNumber { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
